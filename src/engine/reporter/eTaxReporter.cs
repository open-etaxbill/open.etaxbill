using System.ServiceProcess;

namespace OpenETaxBill.Engine.Reporter
{
    public partial class eTaxReporter : ServiceBase
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        public eTaxReporter()
        {
            InitializeComponent();
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenETaxBill.Engine.Reporter.Worker m_reportWorker = null;
        private OpenETaxBill.Engine.Reporter.Worker ReportWorker
        {
            get
            {
                if (m_reportWorker == null)
                    m_reportWorker = new OpenETaxBill.Engine.Reporter.Worker();

                return m_reportWorker;
            }
        }

        private OpenETaxBill.Engine.Reporter.Host m_reportHoster = null;
        private OpenETaxBill.Engine.Reporter.Host ReportHoster
        {
            get
            {
                if (m_reportHoster == null)
                    m_reportHoster = new OpenETaxBill.Engine.Reporter.Host();

                return m_reportHoster;
            }
        }
        
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        protected override void OnStart(string[] args)
        {
            ELogger.SNG.WriteLog("server service start...");

            ReportHoster.Start();
            ReportWorker.Start();

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            base.OnStop();

            ReportWorker.Stop();
            ReportHoster.Stop();

            ELogger.SNG.WriteLog("server service stop...");
        }
    
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}