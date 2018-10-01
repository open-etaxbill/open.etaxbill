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
using OdinSdk.OdinLib.Logging;

namespace OpenETaxBill.Engine.Signer
{
    /// <summary>
    /// 
    /// </summary>
    public class ELogger : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // logger
        //-------------------------------------------------------------------------------------------------------------------------
        private readonly static Lazy<ELogger> m_logger = new Lazy<ELogger>(() => new ELogger());

        /// <summary>
        /// 
        /// </summary>
        public static ELogger SNG
        {
            get
            {
                return m_logger.Value;
            }
        }

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

        private OdinSdk.OdinLib.Logging.OEventLog m_oeventLog = null;
        private OdinSdk.OdinLib.Logging.OEventLog OEventLogger
        {
            get
            {
                if (m_oeventLog == null)
                    m_oeventLog = new OdinSdk.OdinLib.Logging.OEventLog(ISigner.Proxy.EventSource);

                return m_oeventLog;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
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
        /// <param name="p_exception">exception 에러 값</param>
        /// <param name="p_message">전달하고자 하는 메시지</param>
        public void WriteLog(string p_exception, string p_message)
        {
            if (Environment.UserInteractive == true)
                ISigner.WriteDebug(p_exception, p_message);
            else
            {
                try
                {
                    QFileLog.WriteLog(ISigner.Manager.HostName, p_exception, p_message);
                }
                catch (Exception)
                {
                    OEventLogger.WriteEntry(p_message, EventLogEntryType.Information);
                }
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
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~ELogger()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}