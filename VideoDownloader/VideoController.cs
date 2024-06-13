using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace VideoDownloader
{
    class DownloadStreamInfo
    {
        public StreamManifest StreamManifest { get; private set; }
        public Video Video { get; private set; }

        public DownloadStreamInfo(StreamManifest streamManifest, Video video)
        {
            StreamManifest = streamManifest;
            Video = video;
        }
    }

    internal class VideoController
    {
        private YoutubeClient youtube;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VideoController()
        {
            // YoutubeClientのインスタンスを作成
            youtube = new YoutubeClient();
        }

        #region privateメソッド

        /// <summary>
        /// 動画または音声ファイルをダウンロードする
        /// </summary>
        /// <param name="streamInfo"></param>
        /// <param name="filePath"></param>
        /// <param name="progressBar"></param>
        /// <returns></returns>
        private async Task DownloadVideoOrAudioFileAsync(IStreamInfo streamInfo, string filePath, ProgressBar progressBar)
        {
            //var progress = new Progress<double>(p => progressBar.Value = (int)(p * 100));
            //await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath, progress);
            //progressBar.Value = progressBar.Maximum;

            //var task = youtube.Videos.Streams.DownloadAsync(streamInfo, filePath, new Progress<double>(p =>
            //{
            //    // 進捗をプログレスバーに反映
            //    progressBar.Value = (int)(p * progressBar.Maximum);
            //}));

            // ダウンロードが完了するまで待機
            //await task;
            //await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath, new Progress<double>(p =>
            //{
            //    // プログレスバーを更新
            //    progressBar.Value = (int)(p * 100);
            //}));
            var progressHandler = new Progress<double>(p =>
            {
                //// UIスレッドでプログレスバーを更新
                //if (progressBar.InvokeRequired)
                //{
                //    progressBar.Invoke(new Action(() => progressBar.Value = (int)(p * 100)));
                //}
                //else
                //{
                //    progressBar.Value = (int)(p * 100);
                //}
                progressBar.Invoke((MethodInvoker)(() => progressBar.Value = (int)(p * 100)));
            });

            await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath, progressHandler);

            // ダウンロードが完了したらプログレスバーを最大値に設定
            progressBar.Value = progressBar.Maximum;
        }

        /// <summary>
        /// ffmpegのプロセスを実行する
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="progressBar"></param>
        /// <returns></returns>
        private static async Task<bool> FfmpegProcessStartAsync(string arguments, ProgressBar progressBar)
        {
            string ffmpegPath = "ffmpeg"; // FFmpegの実行ファイルのパス

            var processStartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();

                var reader = process.StandardError;
                string? line;

                TimeSpan totalDuration = TimeSpan.Zero;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    // 総時間を取得
                    if (totalDuration == TimeSpan.Zero)
                    {
                        var match = Regex.Match(line, @"Duration: (\d{2}):(\d{2}):(\d{2})\.(\d{2})");
                        if (match.Success)
                        {
                            var hours = int.Parse(match.Groups[1].Value);
                            var minutes = int.Parse(match.Groups[2].Value);
                            var seconds = int.Parse(match.Groups[3].Value);
                            var milliseconds = int.Parse(match.Groups[4].Value) * 10;
                            totalDuration = new TimeSpan(0, hours, minutes, seconds, milliseconds);
                        }
                    }

                    // 進行状況を取得
                    var timeMatch = Regex.Match(line, @"time=(\d{2}):(\d{2}):(\d{2})\.(\d{2})");
                    if (timeMatch.Success)
                    {
                        var hours = int.Parse(timeMatch.Groups[1].Value);
                        var minutes = int.Parse(timeMatch.Groups[2].Value);
                        var seconds = int.Parse(timeMatch.Groups[3].Value);
                        var milliseconds = int.Parse(timeMatch.Groups[4].Value) * 10;

                        var currentTime = new TimeSpan(0, hours, minutes, seconds, milliseconds);

                        if (totalDuration != TimeSpan.Zero)
                        {
                            var progressPercentage = (double)currentTime.Ticks / totalDuration.Ticks * 100;
                            progressBar.Invoke((MethodInvoker)(() => progressBar.Value = (int)progressPercentage));
                        }
                    }
                }

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                await process.WaitForExitAsync();

                if (!string.IsNullOrEmpty(error))
                {
                    return false;
                }

                progressBar.Value = progressBar.Maximum;
                return true;
            }
        }

        /// <summary>
        /// 動画ファイルと音声ファイルの結合を行う
        /// </summary>
        /// <param name="videoFilePath"></param>
        /// <param name="audioFilePath"></param>
        /// <param name="outputFilePath"></param>
        /// <returns></returns>
        private static async Task<bool> JoinVideoAndAudioAsync(string videoFilePath, string audioFilePath, string outputFilePath, ProgressBar progressBar)
        {
            return await FfmpegProcessStartAsync($"-i \"{videoFilePath}\" -i \"{audioFilePath}\" -c:v copy -c:a aac -strict experimental \"{outputFilePath}\"", progressBar);
        }

        /// <summary>
        /// webmをwavに変換する
        /// </summary>
        /// <param name="webmFilePath"></param>
        /// <param name="outputFilePath"></param>
        /// <param name="progressBar"></param>
        /// <returns></returns>
        private async Task<bool> ConvertWebmToWavAsync(string webmFilePath, string outputFilePath, ProgressBar progressBar)
        {
            return await FfmpegProcessStartAsync($"-i \"{webmFilePath}\" -vn -acodec pcm_s16le -ar 44100 -ac 2 \"{outputFilePath}\"", progressBar);
        }

        /// <summary>
        /// webmをmp3に変換する
        /// </summary>
        /// <param name="webmFilePath"></param>
        /// <param name="outputFilePath"></param>
        /// <param name="progressBar"></param>
        /// <returns></returns>
        private async Task<bool> ConvertWebmToMp3Async(string webmFilePath, string outputFilePath, ProgressBar progressBar)
        {
            return await FfmpegProcessStartAsync($"-i \"{webmFilePath}\" -vn -acodec libmp3lame -q:a 2 \"{outputFilePath}\"", progressBar);
        }

        /// <summary>
        /// 無向なファイル名文字を取り除く
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string RemoveInvalidFileNameChars(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Concat(fileName.Split(invalidChars));
        }

        /// <summary>
        /// 無向なパス名文字を取り除く
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string RemoveInvalidPathChars(string path)
        {
            var invalidChars = Path.GetInvalidPathChars();
            return string.Concat(path.Split(invalidChars));
        }

        #endregion

        #region publicメソッド

        /// <summary>
        /// 動画情報を読み込む
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<DownloadStreamInfo?> LoadStreamInfoAsync(string url)
        {
            // 動画IDを解析
            var videoId = VideoId.Parse(url);

            // ストリーム情報を取得
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
            if (streamManifest == null)
            {
                return null;
            }

            // 動画情報を取得
            var video = await youtube.Videos.GetAsync(videoId);

            return new DownloadStreamInfo(streamManifest, video);
        }

        /// <summary>
        /// YouTubeの動画をダウンロードする
        /// </summary>
        /// <param name="downloadStreamInfo"></param>
        /// <param name="isVideo"></param>
        /// <param name="format"></param>
        /// <param name="quality"></param>
        /// <param name="outputDir"></param>
        /// <param name="progressBar"></param>
        /// <param name="lblProgressText"></param>
        /// <returns></returns>
        public async Task<bool> DownloadYoutubeAsync(DownloadStreamInfo downloadStreamInfo,
                                                     bool isVideo, string format, string quality, string outputDir,
                                                     ProgressBar progressBar, Label lblProgressText)
        {
            if (downloadStreamInfo == null || string.IsNullOrEmpty(format)) { return false; }

            IStreamInfo? videoStreamInfo = null;
            IStreamInfo? audioStreamInfo = null;

            if (isVideo)
            {
                // 指定された画質のビデオストリームを取得
                videoStreamInfo = downloadStreamInfo.StreamManifest.GetVideoOnlyStreams().FirstOrDefault(s => s.VideoQuality.Label == quality);
                if (videoStreamInfo == null)
                {
                    videoStreamInfo = downloadStreamInfo.StreamManifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();
                }

                // 最高品質のオーディオストリームを取得
                audioStreamInfo = downloadStreamInfo.StreamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                if (videoStreamInfo == null || audioStreamInfo == null)
                {
                    return false;
                }

                // 無効な文字を取り除く
                var videoTitle = RemoveInvalidFileNameChars(downloadStreamInfo.Video.Title);
                // 一時ファイルパス
                string videoFilePath = Path.Combine(outputDir, $"{videoTitle}_video.{videoStreamInfo.Container.Name}");
                string audioFilePath = Path.Combine(outputDir, $"{videoTitle}_audio.{audioStreamInfo.Container.Name}");
                string outputFilePath = Path.Combine(outputDir, $"{videoTitle}.mp4");
                // 無効な文字を取り除く
                videoFilePath = RemoveInvalidPathChars(videoFilePath);
                audioFilePath = RemoveInvalidPathChars(audioFilePath);
                outputFilePath = RemoveInvalidPathChars(outputFilePath);

                // ビデオストリームをダウンロード
                progressBar.Value = 0;
                lblProgressText.Text = "動画ダウンロード中...";
                //await youtube.Videos.Streams.DownloadAsync(videoStreamInfo, videoFilePath);
                await DownloadVideoOrAudioFileAsync(videoStreamInfo, videoFilePath, progressBar);
                lblProgressText.Text = "動画ダウンロード完了";

                // オーディオストリームをダウンロード
                progressBar.Value = 0;
                lblProgressText.Text = "音声ダウンロード中...";
                //await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, audioFilePath);
                await DownloadVideoOrAudioFileAsync(audioStreamInfo, audioFilePath, progressBar);
                lblProgressText.Text = "音声ダウンロード完了";

                // 結合後のファイルが存在する場合は削除しておく
                if (File.Exists(outputFilePath))
                {
                    File.Delete(outputFilePath);
                }

                // FFmpegを使用してビデオとオーディオを結合
                progressBar.Value = 0;
                lblProgressText.Text = "動画ファイルと音声ファイルの結合中...";
                var result = await JoinVideoAndAudioAsync(videoFilePath, audioFilePath, outputFilePath, progressBar);
                lblProgressText.Text = "動画ファイルと音声ファイルの結合完了";

                // 一時ファイルの削除
                File.Delete(videoFilePath);
                File.Delete(audioFilePath);

                if (!result)
                {
                    lblProgressText.Text = "動画ファイルと音声ファイルの結合失敗";
                    return false;
                }
            }
            else
            {
                // オーディオストリームを取得
                audioStreamInfo = downloadStreamInfo.StreamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                if (audioStreamInfo == null)
                {
                    return false;
                }

                // 無効な文字を取り除く
                var videoTitle = RemoveInvalidFileNameChars(downloadStreamInfo.Video.Title);
                // ダウンロード先のファイルパス
                string fileExtension = audioStreamInfo.Container.Name;
                string filePath = Path.Combine(outputDir, $"{videoTitle}.{fileExtension}");
                string outputFilePath = Path.Combine(outputDir, $"{videoTitle}.{format}");

                // オーディオストリームをダウンロード
                progressBar.Value = 0;
                lblProgressText.Text = "音声ダウンロード中...";
                //await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, filePath);
                await DownloadVideoOrAudioFileAsync(audioStreamInfo, filePath, progressBar);
                lblProgressText.Text = "音声ダウンロード完了";

                // 変換後のファイルが存在する場合は削除しておく
                if (File.Exists(outputFilePath))
                {
                    File.Delete(outputFilePath);
                }

                lblProgressText.Text = "音声ダウンロード変換中...";
                switch (format)
                {
                    case "wav":
                        await ConvertWebmToWavAsync(filePath, outputFilePath, progressBar);
                        break;
                    case "mp3":
                        await ConvertWebmToMp3Async(filePath, outputFilePath, progressBar);
                        break;
                }
                lblProgressText.Text = "音声ダウンロード変換完了";

                // ダウンロードしたファイルを削除する
                File.Delete(filePath);
            }

            return true;
        }

        #endregion
    }
}
