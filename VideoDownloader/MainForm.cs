using AngleSharp.Media;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using static System.Windows.Forms.DataFormats;

namespace VideoDownloader
{
    public partial class MainForm : Form
    {
        private readonly string UserSettingsFileName = Path.Combine(Directory.GetCurrentDirectory(), "UserSettings.json");

        private VideoController videoController;
        private DownloadStreamInfo? downloadStreamInfo;

        public MainForm()
        {
            InitializeComponent();

            videoController = new VideoController();

            lblProgressText.Text = "";
            lblVideoTitle.Text = "";

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        #region �C�x���g

        /// <summary>
        /// �t�H�[���ǂݍ��ݎ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            // ���[�U�[�ݒ�t�@�C�������݂���ꍇ
            if (File.Exists(UserSettingsFileName))
            {
                // Json���f�V���A���C�Y
                using (var sr = new StreamReader(UserSettingsFileName, Encoding.UTF8))
                {
                    var data = sr.ReadToEnd();
                    var userSettingsJson = JsonConvert.DeserializeObject<UserSettingsJson>(data);
                    if (userSettingsJson == null) { return; }
                    tbDownloadUrl.Text = userSettingsJson.DownloadUrl;
                    tbOutputDir.Text = userSettingsJson.SaveDirectory;
                }
            }
        }

        /// <summary>
        /// �t�H�[���N���[�W���O
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Json�Ƀ��[�U�[�ݒ�l��ۑ�����
            UserSettingsJson userSettingsJson = new UserSettingsJson()
            {
                DownloadUrl = tbDownloadUrl.Text,
                SaveDirectory = tbOutputDir.Text,
            };
            // �V���A���C�Y���ď�������
            var data = JsonConvert.SerializeObject(userSettingsJson);
            using (var sw = new StreamWriter(UserSettingsFileName, false, Encoding.UTF8))
            {
                // JSON �f�[�^���t�@�C���ɏ�������
                sw.Write(data);
            }
        }

        /// <summary>
        /// �ǂݍ��݃{�^��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnVideoInfoLoad_Click(object sender, EventArgs e)
        {
            try
            {
                btnVideoInfoLoad.Enabled = false;
                btnExecute.Enabled = false;
                cbVideoQualityFormat.Enabled = false;

                downloadStreamInfo = await videoController.LoadStreamInfoAsync(tbDownloadUrl.Text);
                if (downloadStreamInfo == null)
                {
                    return;
                }

                List<ItemSet> src = new List<ItemSet>();
                foreach (var videoInfo in downloadStreamInfo.StreamManifest.GetVideoOnlyStreams().Where(s => s.Container.Name != "webm"))
                {
                    src.Add(new ItemSet($"{videoInfo.Container.Name}({videoInfo.VideoQuality.Label})",
                                        $"{videoInfo.Container.Name},{videoInfo.VideoQuality.Label}"));
                }
                src.Add(new ItemSet("wav", "wav"));
                src.Add(new ItemSet("mp3", "mp3"));
                // ComboBox�ɕ\���ƒl���Z�b�g
                cbVideoQualityFormat.DataSource = src.DistinctBy(s => s.ItemDisp).ToList();
                cbVideoQualityFormat.DisplayMember = "ItemDisp";
                cbVideoQualityFormat.ValueMember = "ItemValue";

                lblVideoTitle.Text = downloadStreamInfo.Video.Title;

                btnExecute.Enabled = true;
                cbVideoQualityFormat.Enabled = true;
            }
            catch (Exception ex)
            {
                btnExecute.Enabled = false;
                MessageBox.Show(ex.Message, "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnVideoInfoLoad.Enabled = true;
            }
        }

        /// <summary>
        /// ���s�{�^��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnExecute_Click(object sender, EventArgs e)
        {
            try
            {
                btnExecute.Enabled = false;
                btnVideoInfoLoad.Enabled = false;
                cbVideoQualityFormat.Enabled = false;

                var cbVal = cbVideoQualityFormat.SelectedValue?.ToString()?.Split(',');
                if (cbVal == null) { return; }
                bool isVideo = cbVal.Count() > 1;
                string format = cbVal != null && cbVal.Count() > 0 ? cbVal[0] : string.Empty;
                string quality = cbVal != null && cbVal.Count() > 1 ? cbVal[1] : string.Empty;
                string outputDir = !string.IsNullOrEmpty(tbOutputDir.Text) ? tbOutputDir.Text : Directory.GetCurrentDirectory();

                if (await videoController.DownloadYoutubeAsync(downloadStreamInfo, isVideo, format, quality, outputDir, progressBar, lblProgressText))
                {
                    lblProgressText.Text = "�_�E�����[�h����";
                    progressBar.Value = 100;
                }
                else
                {
                    lblProgressText.Text = "�_�E�����[�h���s";
                    progressBar.Value = 0;
                }
            }
            catch (Exception ex)
            {
                lblProgressText.Text = ex.Message;
                MessageBox.Show(ex.Message, "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                progressBar.Value = 0;
            }
            finally
            {
                btnExecute.Enabled = true;
                btnVideoInfoLoad.Enabled = true;
                cbVideoQualityFormat.Enabled = true;
            }
        }

        #endregion
    }

    public class ItemSet
    {
        // DisplayMember��ValueMember�ɂ̓v���p�e�B�Ŏw�肷��d�g��
        public string ItemDisp { get; set; }
        public string ItemValue { get; set; }

        // �v���p�e�B���R���X�g���N�^�ŃZ�b�g
        public ItemSet(string d, string v)
        {
            ItemDisp = d;
            ItemValue = v;
        }
    }
}
