using System.ServiceProcess;

namespace OpenETaxBill.Engine.Responsor
{
    public partial class eTaxResponsor : ServiceBase
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        public eTaxResponsor()
        {
            InitializeComponent();
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenETaxBill.Channel.Interface.IResponsor m_iresponsor = null;
        private OpenETaxBill.Channel.Interface.IResponsor IResponsor
        {
            get
            {
                if (m_iresponsor == null)
                    m_iresponsor = new OpenETaxBill.Channel.Interface.IResponsor();

                return m_iresponsor;
            }
        }

        private OpenETaxBill.Engine.Library.USvcHelper m_appHelper = null;
        public OpenETaxBill.Engine.Library.USvcHelper UAppHelper
        {
            get
            {
                if (m_appHelper == null)
                    m_appHelper = new OpenETaxBill.Engine.Library.USvcHelper(IResponsor.Manager);

                return m_appHelper;
            }
        }

        private OpenETaxBill.Engine.Responsor.WebListener m_responseWorker = null;
        private OpenETaxBill.Engine.Responsor.WebListener ResponseWorker
        {
            get
            {
                if (m_responseWorker == null)
                    m_responseWorker = new OpenETaxBill.Engine.Responsor.Worker(UAppHelper.HostAddress, UAppHelper.PortNumber, UAppHelper.WebFolder);

                return m_responseWorker;
            }
        }

        private OpenETaxBill.Engine.Responsor.Host m_responseHoster = null;
        private OpenETaxBill.Engine.Responsor.Host ResponseHoster
        {
            get
            {
                if (m_responseHoster == null)
                    m_responseHoster = new OpenETaxBill.Engine.Responsor.Host();

                return m_responseHoster;
            }
        }
        
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        protected override void OnStart(string[] args)
        {
            ELogger.SNG.WriteLog("server service start...");

            ResponseHoster.Start();
            ResponseWorker.Start();
 
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            base.OnStop();

            ResponseWorker.Stop();
            ResponseHoster.Stop();

            ELogger.SNG.WriteLog("server service stop...");
        }
    
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}