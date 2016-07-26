using System;
using System.ServiceProcess;

namespace OpenETaxBill.Engine.Provider
{
    public partial class eTaxProvider : ServiceBase
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        public eTaxProvider()
        {
            InitializeComponent();
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenETaxBill.Engine.Provider.MailListener m_provideWorker = null;
        private OpenETaxBill.Engine.Provider.MailListener ProvideWorker
        {
            get
            {
                if (m_provideWorker == null)
                    m_provideWorker = new OpenETaxBill.Engine.Provider.MailListener();

                return m_provideWorker;
            }
        }

        private OpenETaxBill.Engine.Provider.Host m_provideHoster = null;
        private OpenETaxBill.Engine.Provider.Host ProvideHoster
        {
            get
            {
                if (m_provideHoster == null)
                    m_provideHoster = new OpenETaxBill.Engine.Provider.Host();

                return m_provideHoster;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        protected override void OnStart(string[] args)
        {
            ELogger.SNG.WriteLog("server service start...");

            ProvideHoster.Start();               // Starting WCF server.
            ProvideWorker.Start();               // Open and waiting to listen the signal through the SMTP port.

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            base.OnStop();

            ProvideWorker.Stop();
            ProvideHoster.Stop();

            ELogger.SNG.WriteLog("server service stop...");
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}