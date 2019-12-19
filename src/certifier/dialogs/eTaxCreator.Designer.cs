namespace OpenETaxBill.Certifier
{
    partial class eTaxCreator
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
            this.cbKind2 = new System.Windows.Forms.ComboBox();
            this.cbKind1 = new System.Windows.Forms.ComboBox();
            this.lbKind2 = new System.Windows.Forms.Label();
            this.uLabelControl1 = new System.Windows.Forms.Label();
            this.sbTransform = new System.Windows.Forms.Button();
            this.tbCanonical = new System.Windows.Forms.Button();
            this.tbInvoicerId = new System.Windows.Forms.TextBox();
            this.labelControl2 = new System.Windows.Forms.Label();
            this.btClear = new System.Windows.Forms.Button();
            this.btSave = new System.Windows.Forms.Button();
            this.btLoad = new System.Windows.Forms.Button();
            this.btCreate = new System.Windows.Forms.Button();
            this.panelControl3 = new System.Windows.Forms.Panel();
            this.tbSourceXml = new System.Windows.Forms.RichTextBox();
            this.uPanelControl1 = new System.Windows.Forms.Panel();
            this.labelControl6 = new System.Windows.Forms.Label();
            this.xmlLoadDlg = new System.Windows.Forms.OpenFileDialog();
            this.xmlSaveDlg = new System.Windows.Forms.SaveFileDialog();
            this.xmlSchmaDlg = new System.Windows.Forms.SaveFileDialog();
            this.certLoadDlg = new System.Windows.Forms.OpenFileDialog();
            this.certSaveDlg = new System.Windows.Forms.SaveFileDialog();
            this.panelControl2.SuspendLayout();
            this.panelControl3.SuspendLayout();
            this.uPanelControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelControl2
            // 
            this.panelControl2.Controls.Add(this.cbKind2);
            this.panelControl2.Controls.Add(this.cbKind1);
            this.panelControl2.Controls.Add(this.lbKind2);
            this.panelControl2.Controls.Add(this.uLabelControl1);
            this.panelControl2.Controls.Add(this.sbTransform);
            this.panelControl2.Controls.Add(this.tbCanonical);
            this.panelControl2.Controls.Add(this.tbInvoicerId);
            this.panelControl2.Controls.Add(this.labelControl2);
            this.panelControl2.Controls.Add(this.btClear);
            this.panelControl2.Controls.Add(this.btSave);
            this.panelControl2.Controls.Add(this.btLoad);
            this.panelControl2.Controls.Add(this.btCreate);
            this.panelControl2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl2.Location = new System.Drawing.Point(0, 0);
            this.panelControl2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(844, 85);
            this.panelControl2.TabIndex = 1;
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
            this.cbKind2.Location = new System.Drawing.Point(629, 18);
            this.cbKind2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbKind2.Name = "cbKind2";
            this.cbKind2.Size = new System.Drawing.Size(121, 20);
            this.cbKind2.TabIndex = 36;
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
            this.cbKind1.Location = new System.Drawing.Point(463, 18);
            this.cbKind1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbKind1.Name = "cbKind1";
            this.cbKind1.Size = new System.Drawing.Size(117, 20);
            this.cbKind1.TabIndex = 35;
            this.cbKind1.Text = "세금계산서";
            this.cbKind1.ValueMember = "code";
            // 
            // lbKind2
            // 
            this.lbKind2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbKind2.Location = new System.Drawing.Point(584, 18);
            this.lbKind2.Name = "lbKind2";
            this.lbKind2.Size = new System.Drawing.Size(39, 14);
            this.lbKind2.TabIndex = 34;
            this.lbKind2.Text = "구분";
            // 
            // uLabelControl1
            // 
            this.uLabelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.uLabelControl1.Location = new System.Drawing.Point(419, 18);
            this.uLabelControl1.Name = "uLabelControl1";
            this.uLabelControl1.Size = new System.Drawing.Size(38, 14);
            this.uLabelControl1.TabIndex = 33;
            this.uLabelControl1.Text = "종류";
            // 
            // sbTransform
            // 
            this.sbTransform.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sbTransform.Location = new System.Drawing.Point(738, 54);
            this.sbTransform.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.sbTransform.Name = "sbTransform";
            this.sbTransform.Size = new System.Drawing.Size(95, 26);
            this.sbTransform.TabIndex = 32;
            this.sbTransform.Text = "TRANSFORM";
            this.sbTransform.Click += new System.EventHandler(this.sbTransform_Click);
            // 
            // tbCanonical
            // 
            this.tbCanonical.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCanonical.Location = new System.Drawing.Point(641, 54);
            this.tbCanonical.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbCanonical.Name = "tbCanonical";
            this.tbCanonical.Size = new System.Drawing.Size(91, 26);
            this.tbCanonical.TabIndex = 30;
            this.tbCanonical.Text = "CANONICAL";
            this.tbCanonical.Click += new System.EventHandler(this.tbCanonical_Click);
            // 
            // tbInvoicerId
            // 
            this.tbInvoicerId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbInvoicerId.Location = new System.Drawing.Point(92, 18);
            this.tbInvoicerId.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbInvoicerId.Name = "tbInvoicerId";
            this.tbInvoicerId.Size = new System.Drawing.Size(109, 21);
            this.tbInvoicerId.TabIndex = 26;
            this.tbInvoicerId.Text = "1112233333";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(11, 18);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(75, 14);
            this.labelControl2.TabIndex = 25;
            this.labelControl2.Text = "사업자번호";
            // 
            // btClear
            // 
            this.btClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btClear.Location = new System.Drawing.Point(411, 54);
            this.btClear.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btClear.Name = "btClear";
            this.btClear.Size = new System.Drawing.Size(70, 26);
            this.btClear.TabIndex = 24;
            this.btClear.Text = "CLEAR";
            this.btClear.Click += new System.EventHandler(this.btClear_Click);
            // 
            // btSave
            // 
            this.btSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btSave.Location = new System.Drawing.Point(565, 54);
            this.btSave.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(70, 26);
            this.btSave.TabIndex = 23;
            this.btSave.Text = "SAVE";
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // btLoad
            // 
            this.btLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btLoad.Location = new System.Drawing.Point(488, 54);
            this.btLoad.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btLoad.Name = "btLoad";
            this.btLoad.Size = new System.Drawing.Size(70, 26);
            this.btLoad.TabIndex = 22;
            this.btLoad.Text = "LOAD";
            this.btLoad.Click += new System.EventHandler(this.btLoad_Click);
            // 
            // btCreate
            // 
            this.btCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btCreate.Location = new System.Drawing.Point(755, 2);
            this.btCreate.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btCreate.Name = "btCreate";
            this.btCreate.Size = new System.Drawing.Size(79, 46);
            this.btCreate.TabIndex = 20;
            this.btCreate.Text = "CreateXml";
            this.btCreate.Click += new System.EventHandler(this.btCreate_Click);
            // 
            // panelControl3
            // 
            this.panelControl3.Controls.Add(this.tbSourceXml);
            this.panelControl3.Controls.Add(this.uPanelControl1);
            this.panelControl3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl3.Location = new System.Drawing.Point(0, 85);
            this.panelControl3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelControl3.Name = "panelControl3";
            this.panelControl3.Size = new System.Drawing.Size(844, 476);
            this.panelControl3.TabIndex = 2;
            // 
            // tbSourceXml
            // 
            this.tbSourceXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbSourceXml.Location = new System.Drawing.Point(0, 16);
            this.tbSourceXml.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbSourceXml.Name = "tbSourceXml";
            this.tbSourceXml.Size = new System.Drawing.Size(844, 460);
            this.tbSourceXml.TabIndex = 8;
            this.tbSourceXml.Text = "";
            // 
            // uPanelControl1
            // 
            this.uPanelControl1.Controls.Add(this.labelControl6);
            this.uPanelControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.uPanelControl1.Location = new System.Drawing.Point(0, 0);
            this.uPanelControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.uPanelControl1.Name = "uPanelControl1";
            this.uPanelControl1.Size = new System.Drawing.Size(844, 16);
            this.uPanelControl1.TabIndex = 7;
            // 
            // labelControl6
            // 
            this.labelControl6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelControl6.Location = new System.Drawing.Point(0, 0);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(844, 16);
            this.labelControl6.TabIndex = 3;
            this.labelControl6.Text = "Display XML Code";
            // 
            // xmlLoadDlg
            // 
            this.xmlLoadDlg.FileName = "*.xml";
            this.xmlLoadDlg.Filter = "All Files (*.xml)|*.xml";
            // 
            // xmlSaveDlg
            // 
            this.xmlSaveDlg.FileName = "2.xml";
            this.xmlSaveDlg.Filter = "All Files (*.xml)|*.xml";
            // 
            // xmlSchmaDlg
            // 
            this.xmlSchmaDlg.FileName = "taxSchema.xml";
            this.xmlSchmaDlg.Filter = "All Files (*.xml)|*.xml";
            // 
            // certLoadDlg
            // 
            this.certLoadDlg.FileName = "*.der";
            this.certLoadDlg.Filter = "All Files (*.der)|*.der";
            // 
            // certSaveDlg
            // 
            this.certSaveDlg.FileName = "cryption.key";
            this.certSaveDlg.Filter = "All Files (*.key)|*.key";
            // 
            // eTaxCreator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(844, 561);
            this.Controls.Add(this.panelControl3);
            this.Controls.Add(this.panelControl2);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "eTaxCreator";
            this.Text = "단위 기능별 검증 -> 전자세금계산서 검증 -> 전자(세금)계산서 작성";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.XmlCreator_FormClosing);
            this.Load += new System.EventHandler(this.XmlCreator_Load);
            this.panelControl2.ResumeLayout(false);
            this.panelControl2.PerformLayout();
            this.panelControl3.ResumeLayout(false);
            this.uPanelControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelControl2;
        private System.Windows.Forms.Panel panelControl3;
        private System.Windows.Forms.RichTextBox tbSourceXml;
        private System.Windows.Forms.Panel uPanelControl1;
        private System.Windows.Forms.Label labelControl6;
        private System.Windows.Forms.Button btCreate;
        private System.Windows.Forms.Button btSave;
        private System.Windows.Forms.Button btLoad;
        private System.Windows.Forms.OpenFileDialog xmlLoadDlg;
        private System.Windows.Forms.Button btClear;
        private System.Windows.Forms.TextBox tbInvoicerId;
        private System.Windows.Forms.Label labelControl2;
        private System.Windows.Forms.SaveFileDialog xmlSaveDlg;
        private System.Windows.Forms.Button tbCanonical;
        private System.Windows.Forms.SaveFileDialog xmlSchmaDlg;
        private System.Windows.Forms.Button sbTransform;
        private System.Windows.Forms.OpenFileDialog certLoadDlg;
        private System.Windows.Forms.SaveFileDialog certSaveDlg;
        private System.Windows.Forms.Label lbKind2;
        private System.Windows.Forms.Label uLabelControl1;
        private System.Windows.Forms.ComboBox cbKind2;
        private System.Windows.Forms.ComboBox cbKind1;
    }
}