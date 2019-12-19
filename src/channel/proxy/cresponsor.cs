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
using System.Diagnostics;
using System.ServiceModel;
using OdinSdk.OdinLib.Configuration;
using OdinSdk.OdinLib.Queue;

namespace OpenETaxBill.Channel
{
    /// <summary>
    /// 
    /// </summary>
    public class CResponsor : QChannel
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_manager"></param>
        public CResponsor(QService p_manager)
            : this(p_manager, "")
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_manager"></param>
        /// <param name="p_ip_address"></param>
        public CResponsor(QService p_manager, string p_ip_address)
            : base(p_manager)
        {
            if (String.IsNullOrEmpty(p_ip_address) == false)
                m_wcf_service_ip = p_ip_address;

            QSlave = (QService)IResponsor.Manager.Clone();
            QSlave.IpAddress = WcfServiceIp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_qmaster"></param>
        /// <param name="p_qslave"></param>
        /// <param name="p_ip_address"></param>
        public CResponsor(QService p_qmaster, QService p_qslave, string p_ip_address)
            : base(p_qmaster, p_qslave, p_ip_address)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenETaxBill.Channel.Interface.IResponsor m_iresponsor = null;

        /// <summary>
        /// 
        /// </summary>
        private OpenETaxBill.Channel.Interface.IResponsor IResponsor
        {
            get
            {
                if (m_iresponsor == null)
                    m_iresponsor = new OpenETaxBill.Channel.Interface.IResponsor();

                return m_iresponsor;
            }
        }

        private static string m_wcf_service_ip = "";

        /// <summary>
        /// 
        /// </summary>
        public string WcfServiceIp
        {
            get
            {
                if (String.IsNullOrEmpty(m_wcf_service_ip) == true)
                    m_wcf_service_ip = IResponsor.Proxy.GetClientIpAddressByConfigurationName();

                return m_wcf_service_ip;
            }
            set
            {
                if (m_wcf_service_ip == value)
                    return;
                m_wcf_service_ip = value;
            }
        }

        private static string m_bindingName = "";

        /// <summary>
        /// 
        /// </summary>
        public string BindingName
        {
            get
            {
                if (String.IsNullOrEmpty(m_bindingName) == true)
                    m_bindingName = IResponsor.Proxy.BindingName;

                return m_bindingName;
            }
            set
            {
                if (m_bindingName == value)
                    return;
                m_bindingName = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private readonly static object SyncChannel = new object();
        private OdinSdk.OdinLib.Communication.WcfClient<OpenETaxBill.WcfResponsor.IResponseService> m_wcf_client = null;

        /// <summary>
        /// 
        /// </summary>
        private OpenETaxBill.WcfResponsor.IResponseService WcfClient
        {
            get
            {
                lock (SyncChannel)
                {
                    if (Object.Equals(m_wcf_client, null) == false && QMaster.ProductId != QSlave.ProductId)
                    {
                        if (QMaster.IsService == true && base.Certkey == Guid.Empty)
                        {
                            m_wcf_client.Stop();
                            m_wcf_client = null;

                            QStart();
                        }
                    }

                    if (Object.Equals(m_wcf_client, null) == true)
                    {
                        IResponsor.Proxy.SetClientPortSharing(WcfServiceIp);

                        m_wcf_client = new OdinSdk.OdinLib.Communication.WcfClient<OpenETaxBill.WcfResponsor.IResponseService>(
                            this.BindingName,
                            IResponsor.Proxy.ProductName,
                            WcfServiceIp,
                            IResponsor.Proxy.ServicePort,
                            true,
                            true,
                            IResponsor.Proxy.IsPortSharing,
                            IResponsor.Proxy.SharingPort
                            )
                        {
                            ReceiveTimeout = TimeSpan.FromDays(7),
                            SendTimeout = TimeSpan.FromDays(7)
                        };

                        m_wcf_client.Start();

                        ((ICommunicationObject)m_wcf_client.InnerChannel).Opened += WcfHelper_Opened;
                        ((ICommunicationObject)m_wcf_client.InnerChannel).Closed += WcfHelper_Closed;
                        ((ICommunicationObject)m_wcf_client.InnerChannel).Faulted += WcfHelper_Faulted;
                    }

                    IResponsor.WriteDebug(String.Format("connect address {0}...", m_wcf_client.WcfAddress));
                }

                return m_wcf_client.InnerChannel;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (Object.Equals(m_wcf_client, null) == false)
            {
                m_wcf_client.Stop();
                m_wcf_client = null;
            }
        }

        private void WcfHelper_Opened(object sender, EventArgs e)
        {
            IResponsor.WriteDebug(String.Format("client channel opened: '{0}'", WcfServiceIp));
        }

        private void WcfHelper_Closed(object sender, EventArgs e)
        {
            IResponsor.WriteDebug(String.Format("client channel closed: '{0}'", WcfServiceIp));
        }

        private void WcfHelper_Faulted(object sender, EventArgs e)
        {
            IResponsor.WriteDebug(String.Format("client channel faulted: '{0}'", WcfServiceIp));
            Stop();
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // logger
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_format"></param>
        /// <param name="p_args"></param>
        public void WriteLog(string p_format, params object[] p_args)
        {
            var _message = String.Format(p_format, p_args);
            WriteLog(CfgHelper.SNG.TraceMode ? String.Format("{0} -> {1}", (new StackTrace()).GetFrame(1).GetMethod().Name, _message) : _message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_message">전달하고자 하는 메시지</param>
        public void WriteLog(string p_message)
        {
            WriteLog("I", CfgHelper.SNG.TraceMode ? String.Format("{0} -> {1}", (new StackTrace()).GetFrame(1).GetMethod().Name, p_message) : p_message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_exception">exception 에러 값</param>
        /// <param name="p_warnning"></param>
        public void WriteLog(Exception p_exception, bool p_warnning = false)
        {
            if (p_warnning == false)
                WriteLog("X", CfgHelper.SNG.TraceMode ? String.Format("{0} -> {1}", (new StackTrace()).GetFrame(1).GetMethod().Name, p_exception.ToString()) : p_exception.Message);
            else
                WriteLog("L", CfgHelper.SNG.TraceMode ? String.Format("{0} -> {1}", (new StackTrace()).GetFrame(1).GetMethod().Name, p_exception.ToString()) : p_exception.Message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_exception"></param>
        /// <param name="p_message"></param>
        public void WriteLog(string p_exception, string p_message)
        {
            if (Environment.UserInteractive == true)
                IResponsor.WriteDebug(p_exception, p_message);
            else
                WcfClient.WriteLog(IResponsor.g_certapp, p_exception, p_message);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // server functions
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_greeting"></param>
        /// <returns></returns>
        public string HelloWorld(string p_greeting)
        {
            return WcfClient.HelloWorld(IResponsor.g_certapp, p_greeting);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        #region IDisposable Members

        /// <summary>
        /// 
        /// </summary>
        private bool IsDisposed
        {
            get;
            set;
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> if disposing; otherwise, <see langword="false"/>.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources. 
                    if (m_iresponsor != null)
                    {
                        m_iresponsor.Dispose();
                        m_iresponsor = null;
                    }
                }

                // Dispose unmanaged resources. 

                // Note disposing has been done. 
                IsDisposed = true;
            }

            // Call Dispose in the base class.
            base.Dispose(disposing);
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}