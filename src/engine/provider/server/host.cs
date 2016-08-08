/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program.If not, see<http://www.gnu.org/licenses/>.
*/

using System;
using System.Messaging;
using OdinSoft.SDK.Data.POSTGRESQL;
using OdinSoft.SDK.Queue;
using OdinSoft.SDK.Security;

namespace OpenETaxBill.Engine.Provider
{
    public class Host : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenETaxBill.Channel.Interface.IProvider m_iprovider = null;
        private OpenETaxBill.Channel.Interface.IProvider IProvider
        {
            get
            {
                if (m_iprovider == null)
                    m_iprovider = new OpenETaxBill.Channel.Interface.IProvider();

                return m_iprovider;
            }
        }

        private OpenETaxBill.Engine.Library.UAppHelper m_appHelper = null;
        public OpenETaxBill.Engine.Library.UAppHelper UAppHelper
        {
            get
            {
                if (m_appHelper == null)
                    m_appHelper = new OpenETaxBill.Engine.Library.UAppHelper(IProvider.Manager);

                return m_appHelper;
            }
        }

        private OdinSoft.SDK.Queue.QWriter p_qwriter = null;
        private OdinSoft.SDK.Queue.QWriter QWriter
        {
            get
            {
                if (p_qwriter == null)
                    p_qwriter = new OdinSoft.SDK.Queue.QWriter();

                return p_qwriter;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OdinSoft.SDK.Communication.WcfServer m_wcf_service = null;
        private OdinSoft.SDK.Communication.WcfServer WcfService
        {
            get
            {
                if (m_wcf_service == null)
                {
                    IProvider.Proxy.SetServerPortSharing();

                    m_wcf_service = new OdinSoft.SDK.Communication.WcfServer(
                        typeof(ProviderService), typeof(IProviderService),
                        IProvider.Proxy.BindingNames,
                        IProvider.Proxy.ProductName,
                        IProvider.Proxy.IpAddress,
                        IProvider.Proxy.ServicePorts,
                        IProvider.Proxy.BehaviorPort,
                        IProvider.Proxy.IsPortSharing,
                        IProvider.Proxy.SharingPort
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
            IProvider.WriteDebug("server channel opened....");
        }

        private void ServerHost_Closed(object sender, EventArgs e)
        {
            IProvider.WriteDebug("server channel closed....");
        }

        private void ServerHost_Faulted(object sender, EventArgs e)
        {
            IProvider.WriteDebug("server channel faulted....");
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
            IProvider.WriteDebug(String.Format("server address {0}...", WcfService.WcfAddress));

            try
            {
                WcfService.ServerHost.Open();

                QReader.QReadEvents += QReader_QReadEvents;
                QReader.QRemoveEvents += QReader_QRemoveEvents;

                //CPermit.QStart();
                QWriter.QStart(IProvider.Manager);
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
        }

        void QReader_QRemoveEvents(object sender, ReceiveCompletedEventArgs e)
        {
            QMessage _qmessage = e.Message.Body as QMessage;
            IProvider.WriteDebug(String.Format("remove: {0}, {1}, {2}, {3}, {4}", _qmessage.ProductId, _qmessage.Command, _qmessage.ProductId, _qmessage.IpAddress, _qmessage.Message));

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
                    IProvider.WriteDebug(String.Format("READ: '{0}', {1}, {2}, {3}, {4}, {5}, {6}", e.Message.Label, _qmessage.Command, _qmessage.ProductId, _qmessage.pVersion, _qmessage.IpAddress, _qmessage.HostName, _message));
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

                    IProvider.WriteDebug(String.Format("READ: '{0}', {1}, {2}, {3}, {4}, {5}, {6}", e.Message.Label, _companyId, _corporateId, _productId, _pVersion, _appkey, _appvalue));
                }
            }

            if (e.Message.Label == "CMD")         // command
            {
                string _product = _qmessage.ProductId;

                if (_product != IProvider.Manager.ProductId)
                {
                    if (_command == "pong")
                    {
                        QWriter.SetPingFlag(new QClient(_qmessage));
                    }
                    else if (_command == "signin")
                    {
                        QWriter.AddAgency(IProvider.Manager, _qmessage);
                    }
                    else if (_command == "signout")
                    {
                        QWriter.RemoveAgency(IProvider.Manager, new Guid(_message));
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            IProvider.WriteDebug("Stop");

            try
            {
                QWriter.QStop(IProvider.Manager);
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
                if (m_iprovider != null)
                {
                    m_iprovider.Dispose();
                    m_iprovider = null;
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