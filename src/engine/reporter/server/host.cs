using System;
using System.Messaging;
using OpenETaxBill.SDK.Communication;
using OpenETaxBill.SDK.Data.Collection;
using OpenETaxBill.SDK.Queue;
using OpenETaxBill.SDK.Security;

namespace OpenETaxBill.Engine.Reporter
{
    public class Host : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenETaxBill.Channel.Interface.IReporter m_ireporter = null;
        private OpenETaxBill.Channel.Interface.IReporter IReporter
        {
            get
            {
                if (m_ireporter == null)
                    m_ireporter = new OpenETaxBill.Channel.Interface.IReporter();

                return m_ireporter;
            }
        }

        private OpenETaxBill.Engine.Library.USvcHelper m_appHelper = null;
        public OpenETaxBill.Engine.Library.USvcHelper UAppHelper
        {
            get
            {
                if (m_appHelper == null)
                    m_appHelper = new OpenETaxBill.Engine.Library.USvcHelper(IReporter.Manager);

                return m_appHelper;
            }
        }

        private OpenETaxBill.SDK.Queue.QWriter p_qwriter = null;
        private OpenETaxBill.SDK.Queue.QWriter QWriter
        {
            get
            {
                if (p_qwriter == null)
                    p_qwriter = new OpenETaxBill.SDK.Queue.QWriter();

                return p_qwriter;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenETaxBill.SDK.Communication.WcfServer m_wcf_service = null;
        private OpenETaxBill.SDK.Communication.WcfServer WcfService
        {
            get
            {
                if (m_wcf_service == null)
                {
                    IReporter.Proxy.SetServerPortSharing();

                    m_wcf_service = new OpenETaxBill.SDK.Communication.WcfServer(
                        typeof(ReportService), typeof(IReportService), 
                        IReporter.Proxy.BindingNames,
                        IReporter.Proxy.ProductName,
                        IReporter.Proxy.IpAddress,
                        IReporter.Proxy.ServicePorts,
                        IReporter.Proxy.BehaviorPort,
                        IReporter.Proxy.IsPortSharing,
                        IReporter.Proxy.SharingPort
                        )
                    {
                        ReceiveTimeout = TimeSpan.FromDays(7),
                        SendTimeout = TimeSpan.FromDays(7)
                    };

                    m_wcf_service.Start();

                    m_wcf_service.ServerHost.Opened += ServerHost_Opened;
                    m_wcf_service.ServerHost.Closed += ServerHost_Closed;
                    m_wcf_service.ServerHost.Faulted += ServerHost_Faulted;
                }

                return m_wcf_service;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private void ServerHost_Opened(object sender, EventArgs e)
        {
            IReporter.WriteDebug("server channel opened....");
        }

        private void ServerHost_Closed(object sender, EventArgs e)
        {
            IReporter.WriteDebug("server channel closed....");
        }

        private void ServerHost_Faulted(object sender, EventArgs e)
        {
            IReporter.WriteDebug("server channel faulted....");
            Stop();
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            IReporter.WriteDebug(String.Format("server address {0}...", WcfService.WcfAddress));

            try
            {
                WcfService.ServerHost.Open();

                QReader.QReadEvents += QReader_QReadEvents;
                QReader.QRemoveEvents += QReader_QRemoveEvents;

                //CPermit.QStart();
                QWriter.QStart(IReporter.Manager);
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
        }

        void QReader_QRemoveEvents(object sender, ReceiveCompletedEventArgs e)
        {
            QMessage _qmessage = e.Message.Body as QMessage;
            IReporter.WriteDebug(String.Format("remove: {0}, {1}, {2}, {3}, {4}", _qmessage.ProductId, _qmessage.Command, _qmessage.ProductId, _qmessage.IpAddress, _qmessage.Message));

            //if (_qmessage.ProductId == CPermit.QSlave.ProductId)
            //{
            //    CPermit.Stop();
            //}
        }

        void QReader_QReadEvents(object sender, ReceiveCompletedEventArgs e)
        {
            QMessage _qmessage = e.Message.Body as QMessage;
            QClient _client = new QClient(_qmessage);
            string _command = _qmessage.Command.ToLower();

            string _message = _qmessage.Message;
            if (_qmessage.UsePackage == true)
                _message = Serialization.SNG.ReadPackage<string>(_qmessage.Package);

            if (Environment.UserInteractive == true)
            {
                if (e.Message.Label != "CFG")
                {
                    IReporter.WriteDebug(String.Format("READ: '{0}', {1}, {2}, {3}, {4}, {5}, {6}", e.Message.Label, _qmessage.Command, _qmessage.ProductId, _qmessage.pVersion, _qmessage.IpAddress, _qmessage.HostName, _message));
                }
                else
                {
                    var _dbps = _qmessage.Package.ToParameters();
                    string _companyId = _dbps["companyId"].ToString();
                    string _corporateId = _dbps["corporateId"].ToString();
                    string _productId = _dbps["productId"].ToString();
                    string _pVersion = _dbps["pVersion"].ToString();
                    string _appkey = _dbps["appkey"].ToString();
                    string _appvalue = _dbps["appValue"].ToString();

                    IReporter.WriteDebug(String.Format("READ: '{0}', {1}, {2}, {3}, {4}, {5}, {6}", e.Message.Label, _companyId, _corporateId, _productId, _pVersion, _appkey, _appvalue));
                }
            }

            if (e.Message.Label == "CMD")         // command
            {
                string _product = _qmessage.ProductId;
            
                if (_product != IReporter.Manager.ProductId)
                {
                    if (_command == "pong")
                    {
                        QWriter.SetPingFlag(new QClient(_qmessage));
                    }
                    else if (_command == "signin")
                    {
                        QWriter.AddAgency(IReporter.Manager, _qmessage);
                    }
                    else if (_command == "signout")
                    {
                        QWriter.RemoveAgency(IReporter.Manager, new Guid(_message));
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            IReporter.WriteDebug("Stop");

            try
            {
                QWriter.QStop(IReporter.Manager);
                //CPermit.QStop();

                if (m_wcf_service != null)
                {
                    m_wcf_service.Stop();
                    m_wcf_service = null;
                }
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_ireporter != null)
                {
                    m_ireporter.Dispose();
                    m_ireporter = null;
                }
                if (m_wcf_service != null)
                {
                    m_wcf_service.Dispose();
                    m_wcf_service = null;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~Host()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}