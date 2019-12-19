namespace OpenETaxBill.Certifier
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.ilMenuBar = new System.Windows.Forms.ImageList(this.components);
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.status_message = new System.Windows.Forms.ToolStripStatusLabel();
            this.buttom_message = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.rtOutput = new System.Windows.Forms.RichTextBox();
            this.outputBar = new System.Windows.Forms.ToolStrip();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tsFile = new System.Windows.Forms.ToolStripMenuItem();
            this.miClose = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.miExit = new System.Windows.Forms.ToolStripMenuItem();
            this.tsCertify = new System.Windows.Forms.ToolStripMenuItem();
            this.miSystem = new System.Windows.Forms.ToolStripMenuItem();
            this.전자세금계산서검증ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miTaxCreator = new System.Windows.Forms.ToolStripMenuItem();
            this.miTaxSigning = new System.Windows.Forms.ToolStripMenuItem();
            this.miTaxEncrypt = new System.Windows.Forms.ToolStripMenuItem();
            this.웹서비스메시징ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miTaxInvoice = new System.Windows.Forms.ToolStripMenuItem();
            this.miTaxReport = new System.Windows.Forms.ToolStripMenuItem();
            this.miTaxRequest = new System.Windows.Forms.ToolStripMenuItem();
            this.miEvent = new System.Windows.Forms.ToolStripMenuItem();
            this.miXmlInvoice = new System.Windows.Forms.ToolStripMenuItem();
            this.miXmlRequest = new System.Windows.Forms.ToolStripMenuItem();
            this.miKeyPublic = new System.Windows.Forms.ToolStripMenuItem();
            this.miRimList = new System.Windows.Forms.ToolStripMenuItem();
            this.sMTPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hTTPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mAILToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.miTabbedMdi = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.miCascade = new System.Windows.Forms.ToolStripMenuItem();
            this.miHorizontal = new System.Windows.Forms.ToolStripMenuItem();
            this.miVertical = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.miWindows = new System.Windows.Forms.ToolStripMenuItem();
            this.tsHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.miTaxCerti = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.miAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBar.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ilMenuBar
            // 
            this.ilMenuBar.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilMenuBar.ImageStream")));
            this.ilMenuBar.TransparentColor = System.Drawing.Color.Magenta;
            this.ilMenuBar.Images.SetKeyName(0, "");
            this.ilMenuBar.Images.SetKeyName(1, "");
            this.ilMenuBar.Images.SetKeyName(2, "");
            this.ilMenuBar.Images.SetKeyName(3, "");
            this.ilMenuBar.Images.SetKeyName(4, "");
            this.ilMenuBar.Images.SetKeyName(5, "");
            this.ilMenuBar.Images.SetKeyName(6, "");
            this.ilMenuBar.Images.SetKeyName(7, "");
            this.ilMenuBar.Images.SetKeyName(8, "");
            this.ilMenuBar.Images.SetKeyName(9, "");
            this.ilMenuBar.Images.SetKeyName(10, "");
            this.ilMenuBar.Images.SetKeyName(11, "");
            this.ilMenuBar.Images.SetKeyName(12, "");
            this.ilMenuBar.Images.SetKeyName(13, "");
            this.ilMenuBar.Images.SetKeyName(14, "");
            this.ilMenuBar.Images.SetKeyName(15, "");
            this.ilMenuBar.Images.SetKeyName(16, "");
            this.ilMenuBar.Images.SetKeyName(17, "");
            this.ilMenuBar.Images.SetKeyName(18, "");
            // 
            // imageList2
            // 
            this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
            this.imageList2.TransparentColor = System.Drawing.Color.Magenta;
            this.imageList2.Images.SetKeyName(0, "");
            this.imageList2.Images.SetKeyName(1, "");
            this.imageList2.Images.SetKeyName(2, "");
            this.imageList2.Images.SetKeyName(3, "");
            this.imageList2.Images.SetKeyName(4, "");
            this.imageList2.Images.SetKeyName(5, "");
            this.imageList2.Images.SetKeyName(6, "");
            this.imageList2.Images.SetKeyName(7, "");
            this.imageList2.Images.SetKeyName(8, "");
            this.imageList2.Images.SetKeyName(9, "");
            this.imageList2.Images.SetKeyName(10, "");
            this.imageList2.Images.SetKeyName(11, "");
            this.imageList2.Images.SetKeyName(12, "");
            this.imageList2.Images.SetKeyName(13, "");
            this.imageList2.Images.SetKeyName(14, "");
            this.imageList2.Images.SetKeyName(15, "");
            this.imageList2.Images.SetKeyName(16, "");
            this.imageList2.Images.SetKeyName(17, "");
            this.imageList2.Images.SetKeyName(18, "");
            // 
            // statusBar
            // 
            this.statusBar.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.status_message,
            this.buttom_message,
            this.tsProgressBar});
            this.statusBar.Location = new System.Drawing.Point(0, 659);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(944, 22);
            this.statusBar.TabIndex = 27;
            this.statusBar.Text = "statusStrip1";
            // 
            // status_message
            // 
            this.status_message.Name = "status_message";
            this.status_message.Size = new System.Drawing.Size(363, 17);
            this.status_message.Spring = true;
            this.status_message.Text = "Ready";
            this.status_message.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttom_message
            // 
            this.buttom_message.Name = "buttom_message";
            this.buttom_message.Size = new System.Drawing.Size(363, 17);
            this.buttom_message.Spring = true;
            this.buttom_message.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tsProgressBar
            // 
            this.tsProgressBar.Name = "tsProgressBar";
            this.tsProgressBar.Size = new System.Drawing.Size(200, 16);
            // 
            // tabControl1
            // 
            this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tabControl1.HotTrack = true;
            this.tabControl1.Location = new System.Drawing.Point(0, 529);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(944, 130);
            this.tabControl1.TabIndex = 32;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.rtOutput);
            this.tabPage1.Controls.Add(this.outputBar);
            this.tabPage1.ImageIndex = 29;
            this.tabPage1.Location = new System.Drawing.Point(4, 4);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage1.Size = new System.Drawing.Size(936, 104);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Output";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // rtOutput
            // 
            this.rtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtOutput.Location = new System.Drawing.Point(3, 27);
            this.rtOutput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rtOutput.Name = "rtOutput";
            this.rtOutput.Size = new System.Drawing.Size(930, 75);
            this.rtOutput.TabIndex = 3;
            this.rtOutput.Text = "";
            // 
            // outputBar
            // 
            this.outputBar.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.outputBar.Location = new System.Drawing.Point(3, 2);
            this.outputBar.Name = "outputBar";
            this.outputBar.Size = new System.Drawing.Size(930, 25);
            this.outputBar.TabIndex = 1;
            this.outputBar.Text = "toolStrip2";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.richTextBox1);
            this.tabPage2.Controls.Add(this.toolStrip1);
            this.tabPage2.ImageIndex = 18;
            this.tabPage2.Location = new System.Drawing.Point(4, 4);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage2.Size = new System.Drawing.Size(996, 104);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Find Results";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(3, 27);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(990, 75);
            this.richTextBox1.TabIndex = 4;
            this.richTextBox1.Text = "";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Location = new System.Drawing.Point(3, 2);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(990, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip2";
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsFile,
            this.tsCertify,
            this.tsWindow,
            this.tsHelp});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(944, 24);
            this.menuStrip1.TabIndex = 33;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tsFile
            // 
            this.tsFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miClose,
            this.toolStripSeparator1,
            this.miExit});
            this.tsFile.Name = "tsFile";
            this.tsFile.Size = new System.Drawing.Size(57, 20);
            this.tsFile.Tag = "imdc,iedc";
            this.tsFile.Text = "파일(&F)";
            // 
            // miClose
            // 
            this.miClose.Name = "miClose";
            this.miClose.Size = new System.Drawing.Size(125, 22);
            this.miClose.Tag = "imdc,iedc";
            this.miClose.Text = "닫기(&C)";
            this.miClose.Click += new System.EventHandler(this.miClose_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(122, 6);
            // 
            // miExit
            // 
            this.miExit.Name = "miExit";
            this.miExit.Size = new System.Drawing.Size(125, 22);
            this.miExit.Tag = "imdc,iedc";
            this.miExit.Text = "끝내기(&X)";
            this.miExit.Click += new System.EventHandler(this.miExit_Click);
            // 
            // tsCertify
            // 
            this.tsCertify.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miSystem,
            this.miEvent,
            this.miKeyPublic,
            this.miRimList});
            this.tsCertify.Name = "tsCertify";
            this.tsCertify.Size = new System.Drawing.Size(83, 20);
            this.tsCertify.Tag = "imdc,iedc";
            this.tsCertify.Text = "인증작업(&V)";
            // 
            // miSystem
            // 
            this.miSystem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.전자세금계산서검증ToolStripMenuItem,
            this.miTaxEncrypt,
            this.웹서비스메시징ToolStripMenuItem});
            this.miSystem.Name = "miSystem";
            this.miSystem.Size = new System.Drawing.Size(178, 22);
            this.miSystem.Tag = "imdc,iedc";
            this.miSystem.Text = "단위 기능별 검증";
            // 
            // 전자세금계산서검증ToolStripMenuItem
            // 
            this.전자세금계산서검증ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miTaxCreator,
            this.miTaxSigning});
            this.전자세금계산서검증ToolStripMenuItem.Name = "전자세금계산서검증ToolStripMenuItem";
            this.전자세금계산서검증ToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.전자세금계산서검증ToolStripMenuItem.Text = "전자세금계산서 검증";
            // 
            // miTaxCreator
            // 
            this.miTaxCreator.Name = "miTaxCreator";
            this.miTaxCreator.Size = new System.Drawing.Size(201, 22);
            this.miTaxCreator.Text = "전자세금계산서 작성(&X)";
            this.miTaxCreator.Click += new System.EventHandler(this.miTaxCreator_Click);
            // 
            // miTaxSigning
            // 
            this.miTaxSigning.Name = "miTaxSigning";
            this.miTaxSigning.Size = new System.Drawing.Size(201, 22);
            this.miTaxSigning.Text = "전자서명 (&S)";
            this.miTaxSigning.Click += new System.EventHandler(this.miTaxSigning_Click);
            // 
            // miTaxEncrypt
            // 
            this.miTaxEncrypt.Name = "miTaxEncrypt";
            this.miTaxEncrypt.Size = new System.Drawing.Size(186, 22);
            this.miTaxEncrypt.Text = "보안검증 (&E)";
            this.miTaxEncrypt.Click += new System.EventHandler(this.miTaxEncrypt_Click);
            // 
            // 웹서비스메시징ToolStripMenuItem
            // 
            this.웹서비스메시징ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miTaxInvoice,
            this.miTaxReport,
            this.miTaxRequest});
            this.웹서비스메시징ToolStripMenuItem.Name = "웹서비스메시징ToolStripMenuItem";
            this.웹서비스메시징ToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.웹서비스메시징ToolStripMenuItem.Text = "웹서비스메시징";
            // 
            // miTaxInvoice
            // 
            this.miTaxInvoice.Name = "miTaxInvoice";
            this.miTaxInvoice.Size = new System.Drawing.Size(205, 22);
            this.miTaxInvoice.Text = "전자세금계산서 제출(&W)";
            this.miTaxInvoice.Click += new System.EventHandler(this.miTaxInvoice_Click);
            // 
            // miTaxReport
            // 
            this.miTaxReport.Name = "miTaxReport";
            this.miTaxReport.Size = new System.Drawing.Size(205, 22);
            this.miTaxReport.Text = "처리결과 전송";
            this.miTaxReport.Click += new System.EventHandler(this.miTaxReport_Click);
            // 
            // miTaxRequest
            // 
            this.miTaxRequest.Name = "miTaxRequest";
            this.miTaxRequest.Size = new System.Drawing.Size(205, 22);
            this.miTaxRequest.Text = "처리결과 요청";
            this.miTaxRequest.Click += new System.EventHandler(this.miTaxRequest_Click);
            // 
            // miEvent
            // 
            this.miEvent.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miXmlInvoice,
            this.miXmlRequest});
            this.miEvent.Name = "miEvent";
            this.miEvent.Size = new System.Drawing.Size(178, 22);
            this.miEvent.Tag = "imdc,iedc";
            this.miEvent.Text = "상호운용성 검증";
            // 
            // miXmlInvoice
            // 
            this.miXmlInvoice.Name = "miXmlInvoice";
            this.miXmlInvoice.Size = new System.Drawing.Size(162, 22);
            this.miXmlInvoice.Text = "세금계산서 제출";
            this.miXmlInvoice.Click += new System.EventHandler(this.miXmlInvoice_Click);
            // 
            // miXmlRequest
            // 
            this.miXmlRequest.Name = "miXmlRequest";
            this.miXmlRequest.Size = new System.Drawing.Size(162, 22);
            this.miXmlRequest.Text = "처리결과 요청";
            this.miXmlRequest.Click += new System.EventHandler(this.miXmlRequest_Click);
            // 
            // miKeyPublic
            // 
            this.miKeyPublic.Name = "miKeyPublic";
            this.miKeyPublic.Size = new System.Drawing.Size(178, 22);
            this.miKeyPublic.Tag = "imdc,iedc";
            this.miKeyPublic.Text = "사업자 공개키 획득";
            this.miKeyPublic.Click += new System.EventHandler(this.miKeyPublic_Click);
            // 
            // miRimList
            // 
            this.miRimList.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sMTPToolStripMenuItem,
            this.hTTPToolStripMenuItem,
            this.mAILToolStripMenuItem});
            this.miRimList.Name = "miRimList";
            this.miRimList.Size = new System.Drawing.Size(178, 22);
            this.miRimList.Tag = "imdc,iedc";
            this.miRimList.Text = "유통시스템 검증";
            // 
            // sMTPToolStripMenuItem
            // 
            this.sMTPToolStripMenuItem.Name = "sMTPToolStripMenuItem";
            this.sMTPToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.sMTPToolStripMenuItem.Text = "SMTP";
            // 
            // hTTPToolStripMenuItem
            // 
            this.hTTPToolStripMenuItem.Name = "hTTPToolStripMenuItem";
            this.hTTPToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.hTTPToolStripMenuItem.Text = "HTTP";
            // 
            // mAILToolStripMenuItem
            // 
            this.mAILToolStripMenuItem.Name = "mAILToolStripMenuItem";
            this.mAILToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.mAILToolStripMenuItem.Text = "MAIL";
            // 
            // tsWindow
            // 
            this.tsWindow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miTabbedMdi,
            this.toolStripSeparator2,
            this.miCascade,
            this.miHorizontal,
            this.miVertical,
            this.toolStripSeparator3,
            this.miWindows});
            this.tsWindow.Name = "tsWindow";
            this.tsWindow.Size = new System.Drawing.Size(50, 20);
            this.tsWindow.Text = "창(&W)";
            // 
            // miTabbedMdi
            // 
            this.miTabbedMdi.Name = "miTabbedMdi";
            this.miTabbedMdi.Size = new System.Drawing.Size(163, 22);
            this.miTabbedMdi.Text = "Use Tabbed MDI";
            this.miTabbedMdi.Click += new System.EventHandler(this.miTabbedMdi_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(160, 6);
            // 
            // miCascade
            // 
            this.miCascade.Name = "miCascade";
            this.miCascade.Size = new System.Drawing.Size(163, 22);
            this.miCascade.Text = "&Cascade";
            this.miCascade.Click += new System.EventHandler(this.miCascade_Click);
            // 
            // miHorizontal
            // 
            this.miHorizontal.Name = "miHorizontal";
            this.miHorizontal.Size = new System.Drawing.Size(163, 22);
            this.miHorizontal.Text = "Tile &Horizontal";
            this.miHorizontal.Click += new System.EventHandler(this.miHorizontal_Click);
            // 
            // miVertical
            // 
            this.miVertical.Name = "miVertical";
            this.miVertical.Size = new System.Drawing.Size(163, 22);
            this.miVertical.Text = "Tile &Vertical";
            this.miVertical.Click += new System.EventHandler(this.miVertical_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(160, 6);
            // 
            // miWindows
            // 
            this.miWindows.Name = "miWindows";
            this.miWindows.Size = new System.Drawing.Size(163, 22);
            this.miWindows.Text = "Windows";
            // 
            // tsHelp
            // 
            this.tsHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miTaxCerti,
            this.toolStripSeparator4,
            this.miAbout});
            this.tsHelp.Name = "tsHelp";
            this.tsHelp.ShortcutKeyDisplayString = "H";
            this.tsHelp.Size = new System.Drawing.Size(72, 20);
            this.tsHelp.Text = "도움말(&H)";
            // 
            // miTaxCerti
            // 
            this.miTaxCerti.Name = "miTaxCerti";
            this.miTaxCerti.Size = new System.Drawing.Size(158, 22);
            this.miTaxCerti.Text = "정보통신진흥원";
            this.miTaxCerti.Click += new System.EventHandler(this.miTaxCerti_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(155, 6);
            // 
            // miAbout
            // 
            this.miAbout.Name = "miAbout";
            this.miAbout.Size = new System.Drawing.Size(158, 22);
            this.miAbout.Text = "&About";
            this.miAbout.Click += new System.EventHandler(this.miAbout_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(944, 681);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MainForm";
            this.Text = "전자세금계산서 ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ImageList ilMenuBar;
        private System.Windows.Forms.ImageList imageList2;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel status_message;
        private System.Windows.Forms.ToolStripStatusLabel buttom_message;
        private System.Windows.Forms.ToolStripProgressBar tsProgressBar;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox rtOutput;
        private System.Windows.Forms.ToolStrip outputBar;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsFile;
        private System.Windows.Forms.ToolStripMenuItem miClose;
        private System.Windows.Forms.ToolStripMenuItem miExit;
        private System.Windows.Forms.ToolStripMenuItem tsCertify;
        private System.Windows.Forms.ToolStripMenuItem miSystem;
        private System.Windows.Forms.ToolStripMenuItem miEvent;
        private System.Windows.Forms.ToolStripMenuItem miKeyPublic;
        private System.Windows.Forms.ToolStripMenuItem miRimList;
        private System.Windows.Forms.ToolStripMenuItem tsHelp;
        private System.Windows.Forms.ToolStripMenuItem miAbout;
        private System.Windows.Forms.ToolStripMenuItem tsWindow;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem miTabbedMdi;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem miCascade;
        private System.Windows.Forms.ToolStripMenuItem miHorizontal;
        private System.Windows.Forms.ToolStripMenuItem miVertical;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem miWindows;
        private System.Windows.Forms.ToolStripMenuItem miTaxCerti;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem 전자세금계산서검증ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem miTaxCreator;
        private System.Windows.Forms.ToolStripMenuItem miTaxSigning;
        private System.Windows.Forms.ToolStripMenuItem miTaxEncrypt;
        private System.Windows.Forms.ToolStripMenuItem 웹서비스메시징ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem miTaxInvoice;
        private System.Windows.Forms.ToolStripMenuItem miTaxReport;
        private System.Windows.Forms.ToolStripMenuItem miTaxRequest;
        private System.Windows.Forms.ToolStripMenuItem miXmlInvoice;
        private System.Windows.Forms.ToolStripMenuItem miXmlRequest;
        private System.Windows.Forms.ToolStripMenuItem sMTPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hTTPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mAILToolStripMenuItem;
    }
}

