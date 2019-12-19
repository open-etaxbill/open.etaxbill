namespace OpenETaxBill.Certifier
{
    partial class eXmlRequest
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelControl2 = new System.Windows.Forms.Panel();
            this.ceTestOk = new System.Windows.Forms.CheckBox();
            this.tbResultsReqSubmitUrl = new System.Windows.Forms.TextBox();
            this.uLabelControl3 = new System.Windows.Forms.Label();
            this.sbRequestSubmit = new System.Windows.Forms.Button();
            this.tbSubmitId = new System.Windows.Forms.TextBox();
            this.uLabelControl2 = new System.Windows.Forms.Label();
            this.btSave = new System.Windows.Forms.Button();
            this.btLoad = new System.Windows.Forms.Button();
            this.pnBackGround = new System.Windows.Forms.Panel();
            this.pnTop = new System.Windows.Forms.Panel();
            this.tbTargetXml = new System.Windows.Forms.RichTextBox();
            this.spTopLeft = new System.Windows.Forms.Splitter();
            this.tbSourceXml = new System.Windows.Forms.RichTextBox();
            this.uPanelControl1 = new System.Windows.Forms.Panel();
            this.uLabelControl1 = new System.Windows.Forms.Label();
            this.labelControl6 = new System.Windows.Forms.Label();
            this.xmlLoadDlg = new System.Windows.Forms.OpenFileDialog();
            this.panelControl2.SuspendLayout();
            this.pnBackGround.SuspendLayout();
            this.pnTop.SuspendLayout();
            this.uPanelControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelControl2
            // 
            this.panelControl2.Controls.Add(this.ceTestOk);
            this.panelControl2.Controls.Add(this.tbResultsReqSubmitUrl);
            this.panelControl2.Controls.Add(this.uLabelControl3);
            this.panelControl2.Controls.Add(this.sbRequestSubmit);
            this.panelControl2.Controls.Add(this.tbSubmitId);
            this.panelControl2.Controls.Add(this.uLabelControl2);
            this.panelControl2.Controls.Add(this.btSave);
            this.panelControl2.Controls.Add(this.btLoad);
            this.panelControl2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl2.Location = new System.Drawing.Point(0, 0);
            this.panelControl2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(844, 93);
            this.panelControl2.TabIndex = 5;
            // 
            // ceTestOk
            // 
            this.ceTestOk.Location = new System.Drawing.Point(53, 30);
            this.ceTestOk.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ceTestOk.Name = "ceTestOk";
            this.ceTestOk.Size = new System.Drawing.Size(99, 16);
            this.ceTestOk.TabIndex = 40;
            this.ceTestOk.Text = "인증 검사 중";
            this.ceTestOk.CheckedChanged += new System.EventHandler(this.ceTestOk_CheckedChanged);
            // 
            // tbResultsReqSubmitUrl
            // 
            this.tbResultsReqSubmitUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbResultsReqSubmitUrl.Location = new System.Drawing.Point(55, 4);
            this.tbResultsReqSubmitUrl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbResultsReqSubmitUrl.Name = "tbResultsReqSubmitUrl";
            this.tbResultsReqSubmitUrl.Size = new System.Drawing.Size(694, 21);
            this.tbResultsReqSubmitUrl.TabIndex = 39;
            this.tbResultsReqSubmitUrl.Text = "http://www.taxcerti.or.kr/etax/er/RequestResultsService/07855a68-d5a2-4e58-9833-e" +
    "a76b7703826";
            // 
            // uLabelControl3
            // 
            this.uLabelControl3.Location = new System.Drawing.Point(8, 6);
            this.uLabelControl3.Name = "uLabelControl3";
            this.uLabelControl3.Size = new System.Drawing.Size(41, 12);
            this.uLabelControl3.TabIndex = 38;
            this.uLabelControl3.Text = "요청URL";
            // 
            // sbRequestSubmit
            // 
            this.sbRequestSubmit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sbRequestSubmit.Location = new System.Drawing.Point(756, 4);
            this.sbRequestSubmit.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.sbRequestSubmit.Name = "sbRequestSubmit";
            this.sbRequestSubmit.Size = new System.Drawing.Size(79, 46);
            this.sbRequestSubmit.TabIndex = 33;
            this.sbRequestSubmit.Text = "REQUEST SUBMIT";
            this.sbRequestSubmit.Click += new System.EventHandler(this.sbRequestSubmit_Click);
            // 
            // tbSubmitId
            // 
            this.tbSubmitId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSubmitId.Location = new System.Drawing.Point(370, 30);
            this.tbSubmitId.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbSubmitId.Name = "tbSubmitId";
            this.tbSubmitId.Size = new System.Drawing.Size(379, 21);
            this.tbSubmitId.TabIndex = 31;
            this.tbSubmitId.Text = "40000000-20160708-451f22a828182f47921e93b1b747e5dc";
            // 
            // uLabelControl2
            // 
            this.uLabelControl2.Location = new System.Drawing.Point(293, 33);
            this.uLabelControl2.Name = "uLabelControl2";
            this.uLabelControl2.Size = new System.Drawing.Size(67, 14);
            this.uLabelControl2.TabIndex = 30;
            this.uLabelControl2.Text = "일련번호";
            // 
            // btSave
            // 
            this.btSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btSave.Location = new System.Drawing.Point(763, 58);
            this.btSave.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(70, 26);
            this.btSave.TabIndex = 23;
            this.btSave.Text = "SAVE";
            // 
            // btLoad
            // 
            this.btLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btLoad.Location = new System.Drawing.Point(687, 58);
            this.btLoad.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btLoad.Name = "btLoad";
            this.btLoad.Size = new System.Drawing.Size(70, 26);
            this.btLoad.TabIndex = 22;
            this.btLoad.Text = "LOAD";
            // 
            // pnBackGround
            // 
            this.pnBackGround.Controls.Add(this.pnTop);
            this.pnBackGround.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnBackGround.Location = new System.Drawing.Point(0, 93);
            this.pnBackGround.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnBackGround.Name = "pnBackGround";
            this.pnBackGround.Size = new System.Drawing.Size(844, 468);
            this.pnBackGround.TabIndex = 6;
            // 
            // pnTop
            // 
            this.pnTop.Controls.Add(this.tbTargetXml);
            this.pnTop.Controls.Add(this.spTopLeft);
            this.pnTop.Controls.Add(this.tbSourceXml);
            this.pnTop.Controls.Add(this.uPanelControl1);
            this.pnTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnTop.Location = new System.Drawing.Point(0, 0);
            this.pnTop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnTop.Name = "pnTop";
            this.pnTop.Size = new System.Drawing.Size(844, 468);
            this.pnTop.TabIndex = 12;
            // 
            // tbTargetXml
            // 
            this.tbTargetXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbTargetXml.Location = new System.Drawing.Point(401, 18);
            this.tbTargetXml.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbTargetXml.Name = "tbTargetXml";
            this.tbTargetXml.Size = new System.Drawing.Size(443, 450);
            this.tbTargetXml.TabIndex = 14;
            this.tbTargetXml.Text = "";
            // 
            // spTopLeft
            // 
            this.spTopLeft.Location = new System.Drawing.Point(396, 18);
            this.spTopLeft.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.spTopLeft.Name = "spTopLeft";
            this.spTopLeft.Size = new System.Drawing.Size(5, 450);
            this.spTopLeft.TabIndex = 13;
            this.spTopLeft.TabStop = false;
            // 
            // tbSourceXml
            // 
            this.tbSourceXml.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbSourceXml.Location = new System.Drawing.Point(0, 18);
            this.tbSourceXml.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbSourceXml.Name = "tbSourceXml";
            this.tbSourceXml.Size = new System.Drawing.Size(396, 450);
            this.tbSourceXml.TabIndex = 12;
            this.tbSourceXml.Text = "";
            // 
            // uPanelControl1
            // 
            this.uPanelControl1.Controls.Add(this.uLabelControl1);
            this.uPanelControl1.Controls.Add(this.labelControl6);
            this.uPanelControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.uPanelControl1.Location = new System.Drawing.Point(0, 0);
            this.uPanelControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.uPanelControl1.Name = "uPanelControl1";
            this.uPanelControl1.Size = new System.Drawing.Size(844, 18);
            this.uPanelControl1.TabIndex = 11;
            // 
            // uLabelControl1
            // 
            this.uLabelControl1.Dock = System.Windows.Forms.DockStyle.Right;
            this.uLabelControl1.Location = new System.Drawing.Point(779, 0);
            this.uLabelControl1.Name = "uLabelControl1";
            this.uLabelControl1.Size = new System.Drawing.Size(65, 18);
            this.uLabelControl1.TabIndex = 4;
            this.uLabelControl1.Text = "Sample XML";
            // 
            // labelControl6
            // 
            this.labelControl6.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelControl6.Location = new System.Drawing.Point(0, 0);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(64, 18);
            this.labelControl6.TabIndex = 3;
            this.labelControl6.Text = "Source XML";
            // 
            // xmlLoadDlg
            // 
            this.xmlLoadDlg.FileName = "*.*";
            this.xmlLoadDlg.Filter = "All Files (*.*)|*.*";
            // 
            // eXmlRequest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(844, 561);
            this.Controls.Add(this.pnBackGround);
            this.Controls.Add(this.panelControl2);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "eXmlRequest";
            this.Text = "상호운용성 검증 -> 처리결과 요청";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.eXmlInterop_FormClosing);
            this.Load += new System.EventHandler(this.eXmlInterop_Load);
            this.panelControl2.ResumeLayout(false);
            this.panelControl2.PerformLayout();
            this.pnBackGround.ResumeLayout(false);
            this.pnTop.ResumeLayout(false);
            this.uPanelControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelControl2;
        private System.Windows.Forms.Button btSave;
        private System.Windows.Forms.Button btLoad;
        private System.Windows.Forms.Panel pnBackGround;
        private System.Windows.Forms.Panel pnTop;
        private System.Windows.Forms.RichTextBox tbTargetXml;
        private System.Windows.Forms.Splitter spTopLeft;
        private System.Windows.Forms.RichTextBox tbSourceXml;
        private System.Windows.Forms.Panel uPanelControl1;
        private System.Windows.Forms.Label uLabelControl1;
        private System.Windows.Forms.Label labelControl6;
        private System.Windows.Forms.OpenFileDialog xmlLoadDlg;
        private System.Windows.Forms.TextBox tbSubmitId;
        private System.Windows.Forms.Label uLabelControl2;
        private System.Windows.Forms.Button sbRequestSubmit;
		private System.Windows.Forms.TextBox tbResultsReqSubmitUrl;
		private System.Windows.Forms.Label uLabelControl3;
        private System.Windows.Forms.CheckBox ceTestOk;
    }
}