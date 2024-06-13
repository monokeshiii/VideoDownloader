namespace VideoDownloader
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tbDownloadUrl = new TextBox();
            btnExecute = new Button();
            tbOutputDir = new TextBox();
            cbVideoQualityFormat = new ComboBox();
            btnVideoInfoLoad = new Button();
            progressBar = new ProgressBar();
            lblProgressText = new Label();
            label1 = new Label();
            lblVideoTitle = new Label();
            SuspendLayout();
            // 
            // tbDownloadUrl
            // 
            tbDownloadUrl.Location = new Point(12, 12);
            tbDownloadUrl.Name = "tbDownloadUrl";
            tbDownloadUrl.PlaceholderText = "ダウンロード元URL";
            tbDownloadUrl.Size = new Size(360, 23);
            tbDownloadUrl.TabIndex = 0;
            // 
            // btnExecute
            // 
            btnExecute.Enabled = false;
            btnExecute.Location = new Point(297, 70);
            btnExecute.Name = "btnExecute";
            btnExecute.Size = new Size(75, 23);
            btnExecute.TabIndex = 4;
            btnExecute.Text = "実行";
            btnExecute.UseVisualStyleBackColor = true;
            btnExecute.Click += btnExecute_Click;
            // 
            // tbOutputDir
            // 
            tbOutputDir.Location = new Point(12, 41);
            tbOutputDir.Name = "tbOutputDir";
            tbOutputDir.PlaceholderText = "保存先（入力がない場合は実行ファイルと同じ場所）";
            tbOutputDir.Size = new Size(360, 23);
            tbOutputDir.TabIndex = 1;
            // 
            // cbVideoQualityFormat
            // 
            cbVideoQualityFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            cbVideoQualityFormat.FormattingEnabled = true;
            cbVideoQualityFormat.Location = new Point(12, 70);
            cbVideoQualityFormat.MaxDropDownItems = 12;
            cbVideoQualityFormat.Name = "cbVideoQualityFormat";
            cbVideoQualityFormat.Size = new Size(121, 23);
            cbVideoQualityFormat.TabIndex = 2;
            // 
            // btnVideoInfoLoad
            // 
            btnVideoInfoLoad.Location = new Point(216, 70);
            btnVideoInfoLoad.Name = "btnVideoInfoLoad";
            btnVideoInfoLoad.Size = new Size(75, 23);
            btnVideoInfoLoad.TabIndex = 3;
            btnVideoInfoLoad.Text = "読込";
            btnVideoInfoLoad.UseVisualStyleBackColor = true;
            btnVideoInfoLoad.Click += btnVideoInfoLoad_Click;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(12, 156);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(360, 23);
            progressBar.TabIndex = 3;
            // 
            // lblProgressText
            // 
            lblProgressText.AutoSize = true;
            lblProgressText.Location = new Point(12, 138);
            lblProgressText.Name = "lblProgressText";
            lblProgressText.Size = new Size(31, 15);
            lblProgressText.TabIndex = 4;
            lblProgressText.Text = "進捗";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 105);
            label1.Name = "label1";
            label1.Size = new Size(55, 15);
            label1.TabIndex = 4;
            label1.Text = "タイトル：";
            // 
            // lblVideoTitle
            // 
            lblVideoTitle.Location = new Point(63, 105);
            lblVideoTitle.Name = "lblVideoTitle";
            lblVideoTitle.Size = new Size(309, 35);
            lblVideoTitle.TabIndex = 4;
            lblVideoTitle.Text = "動画タイトル";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 191);
            Controls.Add(lblVideoTitle);
            Controls.Add(label1);
            Controls.Add(lblProgressText);
            Controls.Add(progressBar);
            Controls.Add(cbVideoQualityFormat);
            Controls.Add(btnVideoInfoLoad);
            Controls.Add(btnExecute);
            Controls.Add(tbOutputDir);
            Controls.Add(tbDownloadUrl);
            MaximizeBox = false;
            MaximumSize = new Size(400, 230);
            Name = "MainForm";
            ShowIcon = false;
            Text = "VideoDonwloader";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox tbDownloadUrl;
        private Button btnExecute;
        private TextBox tbOutputDir;
        private ComboBox cbVideoQualityFormat;
        private Button btnVideoInfoLoad;
        private ProgressBar progressBar;
        private Label lblProgressText;
        private Label label1;
        private Label lblVideoTitle;
    }
}
