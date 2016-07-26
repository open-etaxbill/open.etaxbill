using System;
using System.Messaging;
using OpenETaxBill.SDK.Data.Collection;
using OpenETaxBill.SDK.Queue;
using OpenETaxBill.SDK.Security;

namespace OpenETaxBill.Engine.Signer
{
    public class Host : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenETaxBill.Channel.Interface.ISigner m_isigner = null;
        private OpenETaxBill.Channel.Interface.ISigner ISigner
        {
            get
            {
                if (m_isigner == null)
                    m_isigner = new OpenETaxBill.Channel.Interface.ISigner();

                return m_isigner;
            }
        }

        private OpenETaxBill.Engine.Library.UAppHelper m_appHelper = null;
        public OpenETaxBill.Engine.Library.UAppHelper UAppHelper
        {
            get
            {
                if (m_appHelper == null)
                    m_appHelper = new OpenETaxBill.Engine.Library.UAppHelper(ISigner.Manager);

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
                    ISigner.Proxy.SetServerPortSharing();

                    m_wcf_service = new OpenETaxBill.SDK.Communication.WcfServer(
                        typeof(SignService), typeof(ISignerService), 
                        ISigner.Proxy.BindingNames,
                        ISigner.Proxy.ProductName,
                        ISigner.Proxy.IpAddress,
                        ISigner.Proxy.ServicePorts,
                        ISigner.Proxy.BehaviorPort,
                        ISigner.Proxy.IsPortSharing,
                        ISigner.Proxy.SharingPort
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
            ISigner.WriteDebug("server channel opened....");
        }

        private void ServerHost_Closed(object sender, EventArgs e)
        {
            ISigner.WriteDebug("server channel closed....");
        }

        private void ServerHost_Faulted(object sender, EventArgs e)
        {
            ISigner.WriteDebug("server channel faulted....");
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
            ISigner.WriteDebug(String.Format("server address {0}...", WcfService.WcfAddress));

            try
            {
                WcfService.ServerHost.Open();

                QReader.QReadEvents += QReader_QReadEvents;
                QReader.QRemoveEvents += QReader_QRemoveEvents;

                //CPermit.QStart();
                QWriter.QStart(ISigner.Manager);
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
        }

        void QReader_QRemoveEvents(object sender, ReceiveCompletedEventArgs e)
        {
            QMessage _qmessage = e.Message.Body as QMessage;
            ISigner.WriteDebug(String.Format("remove: {0}, {1}, {2}, {3}, {4}", _qmessage.ProductId, _qmessage.Command, _qmessage.ProductId, _qmessage.IpAddress, _qmessage.Message));

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
                    ISigner.WriteDebug(String.Format("READ: '{0}', {1}, {2}, {3}, {4}, {5}, {6}", e.Message.Label, _qmessage.Command, _qmessage.ProductId, _qmessage.pVersion, _qmessage.IpAddress, _qmessage.HostName, _message));
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

                    ISigner.WriteDebug(String.Format("READ: '{0}', {1}, {2}, {3}, {4}, {5}, {6}", e.Message.Label, _companyId, _corporateId, _productId, _pVersion, _appkey, _appvalue));
                }
            }

            if (e.Message.Label == "CMD")         // command
            {
                string _product = _qmessage.ProductId;

                if (_product != ISigner.Manager.ProductId)
                {
                    if (_command == "pong")
                    {
                        QWriter.SetPingFlag(new QClient(_qmessage));
                    }
                    else if (_command == "signin")
                    {
                        QWriter.AddAgency(ISigner.Manager, _qmessage);
                    }
                    else if (_command == "signout")
                    {
                        QWriter.RemoveAgency(ISigner.Manager, new Guid(_message));
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            ISigner.WriteDebug("Stop");

            try
            {
                QWriter.QStop(ISigner.Manager);
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
                if (m_isigner != null)
                {
                    m_isigner.Dispose();
                    m_isigner = null;
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