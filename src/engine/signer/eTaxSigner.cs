using System.ServiceProcess;

namespace OpenETaxBill.Engine.Signer
{
    public partial class eTaxSigner : ServiceBase
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        public eTaxSigner()
        {
            InitializeComponent();
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private Worker m_signWorker = null;
        private Worker SignWorker
        {
            get
            {
                if (m_signWorker == null)
                    m_signWorker = new Worker();

                return m_signWorker;
            }
        }

        private Host m_signHoster = null;
        private Host SignHoster
        {
            get
            {
                if (m_signHoster == null)
                    m_signHoster = new Host();

                return m_signHoster;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        protected override void OnStart(string[] args)
        {
            ELogger.SNG.WriteLog("server service start...");

            SignHoster.Start();
            SignWorker.Start();

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            base.OnStop();

            SignWorker.Stop();
            SignHoster.Stop();

            ELogger.SNG.WriteLog("server service stop...");
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}