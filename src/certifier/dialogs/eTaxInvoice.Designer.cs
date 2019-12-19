namespace OpenETaxBill.Certifier
{
    partial class eTaxInvoice
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
            this.cbKind2 = new System.Windows.Forms.ComboBox();
            this.cbKind1 = new System.Windows.Forms.ComboBox();
            this.lbKind2 = new System.Windows.Forms.Label();
            this.uLabelControl3 = new System.Windows.Forms.Label();
            this.tbTaxInvoiceSubmitUrl = new System.Windows.Forms.TextBox();
            this.uLabelControl2 = new System.Windows.Forms.Label();
            this.sbCheckSign = new System.Windows.Forms.Button();
            this.sbFirstRefTarget = new System.Windows.Forms.Button();
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
            this.panelControl2.Controls.Add(this.cbKind2);
            this.panelControl2.Controls.Add(this.cbKind1);
            this.panelControl2.Controls.Add(this.lbKind2);
            this.panelControl2.Controls.Add(this.uLabelControl3);
            this.panelControl2.Controls.Add(this.tbTaxInvoiceSubmitUrl);
            this.panelControl2.Controls.Add(this.uLabelControl2);
            this.panelControl2.Controls.Add(this.sbCheckSign);
            this.panelControl2.Controls.Add(this.sbFirstRefTarget);
            this.panelControl2.Controls.Add(this.btSave);
            this.panelControl2.Controls.Add(this.btLoad);
            this.panelControl2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl2.Location = new System.Drawing.Point(0, 0);
            this.panelControl2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(844, 93);
            this.panelControl2.TabIndex = 4;
            // 
            // ceTestOk
            // 
            this.ceTestOk.Location = new System.Drawing.Point(54, 30);
            this.ceTestOk.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ceTestOk.Name = "ceTestOk";
            this.ceTestOk.Size = new System.Drawing.Size(99, 16);
            this.ceTestOk.TabIndex = 45;
            this.ceTestOk.Text = "인증 검사 중";
            this.ceTestOk.CheckedChanged += new System.EventHandler(this.ceTestOk_CheckedChanged);
            // 
            // cbKind2
            // 
            this.cbKind2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbKind2.DisplayMember = "name";
            this.cbKind2.Items.AddRange(new object[] {
            "일반",
            "영세",
            "위수탁",
            "수입",
            "영세율위수탁",
            "수입납부유예"});
            this.cbKind2.Location = new System.Drawing.Point(627, 30);
            this.cbKind2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbKind2.Name = "cbKind2";
            this.cbKind2.Size = new System.Drawing.Size(121, 20);
            this.cbKind2.TabIndex = 44;
            this.cbKind2.Text = "일반";
            this.cbKind2.ValueMember = "code";
            // 
            // cbKind1
            // 
            this.cbKind1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbKind1.DisplayMember = "name";
            this.cbKind1.Items.AddRange(new object[] {
            "세금계산서",
            "수정세금계산서",
            "계산서",
            "수정계산서"});
            this.cbKind1.Location = new System.Drawing.Point(465, 30);
            this.cbKind1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbKind1.Name = "cbKind1";
            this.cbKind1.Size = new System.Drawing.Size(117, 20);
            this.cbKind1.TabIndex = 43;
            this.cbKind1.Text = "세금계산서";
            this.cbKind1.ValueMember = "code";
            // 
            // lbKind2
            // 
            this.lbKind2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbKind2.Location = new System.Drawing.Point(586, 32);
            this.lbKind2.Name = "lbKind2";
            this.lbKind2.Size = new System.Drawing.Size(34, 14);
            this.lbKind2.TabIndex = 42;
            this.lbKind2.Text = "구분";
            // 
            // uLabelControl3
            // 
            this.uLabelControl3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.uLabelControl3.Location = new System.Drawing.Point(423, 32);
            this.uLabelControl3.Name = "uLabelControl3";
            this.uLabelControl3.Size = new System.Drawing.Size(37, 14);
            this.uLabelControl3.TabIndex = 41;
            this.uLabelControl3.Text = "종류";
            // 
            // tbTaxInvoiceSubmitUrl
            // 
            this.tbTaxInvoiceSubmitUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTaxInvoiceSubmitUrl.Location = new System.Drawing.Point(56, 4);
            this.tbTaxInvoiceSubmitUrl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbTaxInvoiceSubmitUrl.Name = "tbTaxInvoiceSubmitUrl";
            this.tbTaxInvoiceSubmitUrl.Size = new System.Drawing.Size(693, 21);
            this.tbTaxInvoiceSubmitUrl.TabIndex = 33;
            this.tbTaxInvoiceSubmitUrl.Text = "http://www.taxcerti.or.kr/etax/er/SubmitEtaxInvoiceService/07855a68-d5a2-4e58-983" +
    "3-ea76b7703826";
            // 
            // uLabelControl2
            // 
            this.uLabelControl2.Location = new System.Drawing.Point(9, 6);
            this.uLabelControl2.Name = "uLabelControl2";
            this.uLabelControl2.Size = new System.Drawing.Size(41, 12);
            this.uLabelControl2.TabIndex = 32;
            this.uLabelControl2.Text = "제출URL";
            // 
            // sbCheckSign
            // 
            this.sbCheckSign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sbCheckSign.Location = new System.Drawing.Point(575, 58);
            this.sbCheckSign.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.sbCheckSign.Name = "sbCheckSign";
            this.sbCheckSign.Size = new System.Drawing.Size(97, 26);
            this.sbCheckSign.TabIndex = 30;
            this.sbCheckSign.Text = "CheckSign";
            this.sbCheckSign.Click += new System.EventHandler(this.sbCheckSign_Click);
            // 
            // sbFirstRefTarget
            // 
            this.sbFirstRefTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sbFirstRefTarget.Location = new System.Drawing.Point(756, 4);
            this.sbFirstRefTarget.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.sbFirstRefTarget.Name = "sbFirstRefTarget";
            this.sbFirstRefTarget.Size = new System.Drawing.Size(79, 46);
            this.sbFirstRefTarget.TabIndex = 29;
            this.sbFirstRefTarget.Text = "INVOICE SUBMIT";
            this.sbFirstRefTarget.Click += new System.EventHandler(this.sbInvoiceSubmit_Click);
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
            this.btLoad.Location = new System.Drawing.Point(677, 58);
            this.btLoad.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btLoad.Name = "btLoad";
            this.btLoad.Size = new System.Drawing.Size(80, 26);
            this.btLoad.TabIndex = 22;
            this.btLoad.Text = "LOAD";
            this.btLoad.Click += new System.EventHandler(this.btLoad_Click);
            // 
            // pnBackGround
            // 
            this.pnBackGround.Controls.Add(this.pnTop);
            this.pnBackGround.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnBackGround.Location = new System.Drawing.Point(0, 93);
            this.pnBackGround.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnBackGround.Name = "pnBackGround";
            this.pnBackGround.Size = new System.Drawing.Size(844, 468);
            this.pnBackGround.TabIndex = 5;
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
            this.tbTargetXml.Location = new System.Drawing.Point(412, 18);
            this.tbTargetXml.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbTargetXml.Name = "tbTargetXml";
            this.tbTargetXml.Size = new System.Drawing.Size(432, 450);
            this.tbTargetXml.TabIndex = 14;
            this.tbTargetXml.Text = "";
            // 
            // spTopLeft
            // 
            this.spTopLeft.Location = new System.Drawing.Point(407, 18);
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
            this.tbSourceXml.Size = new System.Drawing.Size(407, 450);
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
            // eTaxInvoice
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(844, 561);
            this.Controls.Add(this.pnBackGround);
            this.Controls.Add(this.panelControl2);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "eTaxInvoice";
            this.Text = "단위 기능별 검증 -> 웹서비스메시징 -> 전자(세금)계산서 제출";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.eXmlCreator_FormClosing);
            this.Load += new System.EventHandler(this.eXmlCreator_Load);
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
        private System.Windows.Forms.Button sbFirstRefTarget;
        private System.Windows.Forms.OpenFileDialog xmlLoadDlg;
        private System.Windows.Forms.Button sbCheckSign;
		private System.Windows.Forms.TextBox tbTaxInvoiceSubmitUrl;
		private System.Windows.Forms.Label uLabelControl2;
        private System.Windows.Forms.ComboBox cbKind2;
        private System.Windows.Forms.ComboBox cbKind1;
        private System.Windows.Forms.Label lbKind2;
        private System.Windows.Forms.Label uLabelControl3;
        private System.Windows.Forms.CheckBox ceTestOk;
    }
}