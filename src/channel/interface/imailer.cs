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
using OdinSdk.OdinLib.Configuration;
using OdinSdk.OdinLib.Queue;

namespace OpenETaxBill.Channel.Interface
{
    /// <summary>
    /// CProxy에 대한 요약 설명입니다.
    /// </summary>
    public class IMailer : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        public IMailer()
            : this(true)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_isService"></param>
        public IMailer(bool p_isService)
        {
            Manager.IsService = p_isService;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private OdinSdk.OdinLib.Communication.WcfProxy m_wcfProxy = null;

        /// <summary>
        /// 
        /// </summary>
        public OdinSdk.OdinLib.Communication.WcfProxy Proxy
        {
            get
            {
                if (m_wcfProxy == null)
                {
                    m_wcfProxy = new OdinSdk.OdinLib.Communication.WcfProxy
                        (
                        "Open-eTaxBill Mailing Service V1.0",               // Service Description

                        new string[] { "net.tcp", "BasicHttpBinding" },     // Binding Names
                        "",                                                 // Ip Address
                        new int[] { 8464, 8465 },                           // Service Ports
                        8466,                                               // Behavior Port

                        "OpenETaxBill_Mail_V10.soap",                      // Soap Url
                        "mailer",                                           // Event Source

                        "A4529FC4-276C-4C85-A897-EB012E62CDA8",             // Application GUID

                        "OpenTAX_DBC_MAIL",                                    // Application Connection
                        "WcfMailerUrl",                                     // Application Configure

                        "tcp",                                              // MSMQ Protocol Name

                        "bizapp",                                           // Category Id
                        "OpenTAX_Mailer",                                      // Application ServiceId
                        "OpenTAX_Mailer_V10",                                  // Service Name
                        "V1.0.2016.07"                                      // Version
                        );
                }

                return m_wcfProxy;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //                                                                                                     
        //-------------------------------------------------------------------------------------------------------------------------
        private QService m_manager = null;

        /// <summary>
        /// 
        /// </summary>
        public QService Manager
        {
            get
            {
                if (m_manager == null)
                {
                    m_manager = new QService(Proxy.QProtocolName, Proxy.CategoryId, Proxy.ProductName, Proxy.ProductId, Proxy.pVersion);
                    Proxy.SetServerIpAddressByConfigurationName();
                }

                return m_manager;
            }
        }

        private Guid m_certapp = Guid.Empty;

        /// <summary>
        /// 
        /// </summary>
        public Guid g_certapp
        {
            get
            {
                if (m_certapp == Guid.Empty)
                    m_certapp = new Guid(Proxy.ApplicationId);

                return m_certapp;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_certapp"></param>
        /// <returns></returns>
        public bool CheckValidApplication(Guid p_certapp)
        {
            return p_certapp.Equals(g_certapp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_queueName"></param>
        /// <returns></returns>
        public bool CheckValidApplication(string p_queueName)
        {
            return p_queueName.Equals(Manager.QueueName);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private OdinSdk.OdinLib.Logging.QFileLog m_qfilelog = null;
        private OdinSdk.OdinLib.Logging.QFileLog QFileLog
        {
            get
            {
                if (m_qfilelog == null)
                    m_qfilelog = new OdinSdk.OdinLib.Logging.QFileLog();

                return m_qfilelog;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_format"></param>
        /// <param name="p_args"></param>
        public void WriteDebug(string p_format, params object[] p_args)
        {
            var _message = String.Format(p_format, p_args);
            WriteDebug(CfgHelper.SNG.TraceMode ? String.Format("{0} -> {1}", (new StackTrace()).GetFrame(1).GetMethod().Name, _message) : _message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_message">전달하고자 하는 메시지</param>
        public void WriteDebug(string p_message)
        {
            WriteDebug("I", CfgHelper.SNG.TraceMode ? String.Format("{0} -> {1}", (new StackTrace()).GetFrame(1).GetMethod().Name, p_message) : p_message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_exception">exception 에러 값</param>
        /// <param name="p_warnning"></param>
        public void WriteDebug(Exception p_exception, bool p_warnning = false)
        {
            if (p_warnning == false)
                WriteDebug("X", CfgHelper.SNG.TraceMode ? String.Format("{0} -> {1}", (new StackTrace()).GetFrame(1).GetMethod().Name, p_exception.ToString()) : p_exception.Message);
            else
                WriteDebug("L", CfgHelper.SNG.TraceMode ? String.Format("{0} -> {1}", (new StackTrace()).GetFrame(1).GetMethod().Name, p_exception.ToString()) : p_exception.Message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_exception">exception 에러 값</param>
        /// <param name="p_message">전달하고자 하는 메시지</param>
        public void WriteDebug(string p_exception, string p_message)
        {
            if (Environment.UserInteractive == true)
                Console.WriteLine("[{0:yyyy-MM-dd-HH:mm:ss}] {1}, {2}", DateTime.Now, p_exception, p_message);
            else if (CfgHelper.SNG.DebugMode == true)
                QFileLog.WriteLog(Manager.HostName, p_exception, p_message);
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
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> if disposing; otherwise, <see langword="false"/>.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources. 
                    if (m_wcfProxy != null)
                    {
                        m_wcfProxy.Dispose();
                        m_wcfProxy = null;
                    }
                    if (m_manager != null)
                    {
                        m_manager.Dispose();
                        m_manager = null;
                    }
                }

                // Dispose unmanaged resources. 

                // Note disposing has been done. 
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        ~IMailer()
        {
            Dispose(false);
        }

        #endregion
    }
}