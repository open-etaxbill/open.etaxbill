namespace OpenETaxBill.Engine.Mailer
{
    partial class eTaxInstaller
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

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            eTaxSPI = new System.ServiceProcess.ServiceProcessInstaller();
            eTaxSVI = new System.ServiceProcess.ServiceInstaller();
            // 
            // eTaxSPI
            // 
            eTaxSPI.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            eTaxSPI.Password = null;
            eTaxSPI.Username = null;
            // 
            // eTaxSVI
            // 
            eTaxSVI.Description = "사용자가 발행한 전자세금계산서를 메일 송신하는 서비스 입니다.";
            eTaxSVI.DisplayName = "Open-eTaxBill Mailer";
            eTaxSVI.ServiceName = "Open-eTaxBill Mailer";
            // 
            // eTaxInstaller
            // 
            Installers.AddRange(new System.Configuration.Install.Installer[] {
            eTaxSPI,
            eTaxSVI});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller eTaxSPI;
        private System.ServiceProcess.ServiceInstaller eTaxSVI;
    }
}