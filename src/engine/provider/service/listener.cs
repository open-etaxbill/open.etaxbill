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
using System.Collections;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;
using OpenETaxBill.Channel.Library.Net.Mime.Parser;
using OpenETaxBill.Channel.Library.Security.Encrypt;
using OpenETaxBill.Channel.Library.Security.Issue;
using OdinSoft.SDK.Configuration;
using OdinSoft.SDK.Data;
using OdinSoft.SDK.Data.Collection;

namespace OpenETaxBill.Engine.Provider
{
    public class MailListener : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private TcpListener smtpListener;
        private System.Threading.Thread ListenThread;

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
        
        private OpenETaxBill.Engine.Library.UAppHelper m_svcHelper = null;
        public OpenETaxBill.Engine.Library.UAppHelper USvcHelper
        {
            get
            {
                if (m_svcHelper == null)
                    m_svcHelper = new OpenETaxBill.Engine.Library.UAppHelper(IProvider.Manager);

                return m_svcHelper;
            }
        }
        
        private OpenETaxBill.Engine.Library.UCertHelper m_certHelper = null;
        public OpenETaxBill.Engine.Library.UCertHelper UCertHelper
        {
            get
            {
                if (m_certHelper == null)
                    m_certHelper = new OpenETaxBill.Engine.Library.UCertHelper(IProvider.Manager);

                return m_certHelper;
            }
        }

        private OdinSoft.SDK.Data.DataHelper m_dataHelper = null;
        private OdinSoft.SDK.Data.DataHelper LDataHelper
        {
            get
            {
                if (m_dataHelper == null)
                    m_dataHelper = new OdinSoft.SDK.Data.DataHelper();
                return m_dataHelper;
            }
        }

        private OdinSoft.SDK.Data.DeltaHelper m_dltaHelper = null;
        private OdinSoft.SDK.Data.DeltaHelper LDltaHelper
        {
            get
            {
                if (m_dltaHelper == null)
                    m_dltaHelper = new OdinSoft.SDK.Data.DeltaHelper();

                return m_dltaHelper;
            }
        }

        private OdinSoft.SDK.Logging.QFileWriter m_qfwriter = null;
        private OdinSoft.SDK.Logging.QFileWriter QFWriter
        {
            get
            {
                if (m_qfwriter == null)
                    m_qfwriter = new OdinSoft.SDK.Logging.QFileWriter();

                return m_qfwriter;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private Hashtable m_sessionTable = null;
        private Hashtable SessionTable
        {
            get
            {
                if (m_sessionTable == null)
                    m_sessionTable = new Hashtable();

                return m_sessionTable;
            }
        }

        private const string m_aspMailAdrs = "etax";

        /// <summary>
        /// Gets etax asp mail address
        /// </summary>
        public string AspMailAddress
        {
            get
            {
                return m_aspMailAdrs;
            }
        }

        private string m_ip_address = "ALL";   // Holds IP Address, which to listen incoming calls.

        /// <summary>
        /// Gets or sets which IP address to listen.
        /// </summary>
        public string IpAddress
        {
            get
            {
                return m_ip_address;
            }

            set
            {
                m_ip_address = value;
            }
        }

        private int m_portNumber = 25;

        /// <summary>
        /// Gets or sets which port to listen.
        /// </summary>
        public int PortNumber
        {
            get
            {
                return m_portNumber;
            }
            set
            {
                m_portNumber = value;
            }
        }

        private int m_maxThreads = 64;      // Holds maximum allowed Worker Threads.

        /// <summary>
        /// Gets or sets maximum session threads.
        /// </summary>
        public int MaxThreads
        {
            get
            {
                return m_maxThreads;
            }
            set
            {
                m_maxThreads = value;
            }
        }

        /// <summary>
        /// Gets or sets if to log commands.
        /// </summary>
        public bool LogCommands
        {
            get
            {
                return CfgHelper.SNG.DebugMode;
            }
        }

        public bool LiveServer
        {
            get
            {
                return USvcHelper.LiveServer;
            }
        }

        public bool MailSniffing
        {
            get
            {
                return USvcHelper.MailSniffing;
            }
        }

        public string ConnectionString
        {
            get
            {
                return USvcHelper.ConnectionString;
            }
        }

        private int m_sessionIdleTimeOut = 80000;           // Holds session idle timeout.

        /// <summary>
        /// Session idle timeout in milliseconds.
        /// </summary>
        public int SessionIdleTimeOut
        {
            get
            {
                return m_sessionIdleTimeOut;
            }

            set
            {
                m_sessionIdleTimeOut = value;
            }
        }

        private int m_commandIdleTimeOut = 60000;   // Holds command ilde timeout.

        /// <summary>
        /// Command idle timeout in milliseconds.
        /// </summary>
        public int CommandIdleTimeOut
        {
            get
            {
                return m_commandIdleTimeOut;
            }

            set
            {
                m_commandIdleTimeOut = value;
            }
        }

        private int m_maxMessageSize = int.MaxValue;

        /// <summary>
        /// Maximum message size.
        /// </summary>
        public int MaxMessageSize
        {
            get
            {
                return m_maxMessageSize;
            }

            set
            {
                m_maxMessageSize = value;
            }
        }

        private int m_maxRecipients = 128;

        /// <summary>
        /// Maximum recipients per message.
        /// </summary>
        public int MaxRecipients
        {
            get
            {
                return m_maxRecipients;
            }

            set
            {
                m_maxRecipients = value;
            }
        }

        private int m_maxBadCommands = 8;

        /// <summary>
        /// Gets or sets maximum bad commands allowed to session.
        /// </summary>
        public int MaxBadCommands
        {
            get
            {
                return m_maxBadCommands;
            }

            set
            {
                m_maxBadCommands = value;
            }
        }

        private string m_host_name = null;

        /// <summary>
        /// 
        /// </summary>
        public string HostName
        {
            get
            {
                if (m_host_name == null)
                    m_host_name = Dns.GetHostName();

                return m_host_name;
            }
            set
            {
                m_host_name = value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private byte[] GetTopLines(Stream p_istream, int p_nrLines)
        {
            TextReader _reader = (TextReader)new StreamReader(p_istream);

            var _builder = new StringBuilder();

            int _lineCounter = 0;
            int _messageLine = -1;
            var _messageLines = false;

            p_istream.Position = 0;

            while (true)
            {
                string _line = _reader.ReadLine();

                // Reached end of message
                if (_line == null)
                {
                    break;
                }
                else
                {
                    // End of header reached
                    if (_messageLines == false && _line.Length == 0)
                    {
                        // Set flag, that message lines reading start.
                        _messageLines = true;
                    }

                    // Check that wanted message lines count isn't exceeded
                    if (_messageLines == true)
                    {
                        if (_messageLine > p_nrLines)
                            break;

                        _messageLine++;
                    }

                    _builder.Append(_line + "\r ");
                }

                // Don't allow read more than 150 lines
                if (_lineCounter > 150)
                    break;

                _lineCounter++;
            }

            return Encoding.UTF8.GetBytes(_builder.ToString());
        }

        private void WriteLog(string p_message, bool p_writeLog)
        {
            if (MailSniffing == true && p_writeLog == true)
                ELogger.SNG.WriteLog("E", p_message);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        public void Listen()
        {
            try
            {
                ELogger.SNG.WriteLog(String.Format("starting for connection: {0}, {1}...", "IPAddress.Any", PortNumber));

                smtpListener = new TcpListener(IPAddress.Any, PortNumber);
                smtpListener.Start();

                while (true)
                {
                    // Check if maximum allowed thread count isn't exceeded
                    if (SessionTable.Count <= MaxThreads)
                    {
                        WriteLog(String.Format("waiting for connection: {0}/{1}...", SessionTable.Count, MaxThreads), true);

                        // Thread is sleeping, until a client connects
                        Socket _clientSocket = smtpListener.AcceptSocket();

                        string _sessionId = _clientSocket.GetHashCode().ToString();
                        WriteLog(String.Format("session[{0}]: listener accepted...", _sessionId), true);

                        MailResponsor _newRequestor = new MailResponsor(this, _clientSocket, _sessionId);

                        Thread _requestor = new Thread(_newRequestor.Process)
                        {
                            Name = "SMTP Requestor"
                        };

                        // Add session to session list
                        AddSession(_sessionId, _newRequestor);

                        // Start proccessing
                        _requestor.Start();
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch (ThreadInterruptedException ex)
            {
                ELogger.SNG.WriteLog(ex);
                Thread.CurrentThread.Abort();
            }
            catch (ThreadAbortException ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            catch (SocketException ex)
            {
                ELogger.SNG.WriteLog("X", String.Format("{0}: {1}:{2}", ex.Message, HostName, PortNumber));
            }
            catch (ProviderException ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            finally
            {
                smtpListener.Stop();
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            ListenThread = new Thread(Listen)
            {
                Name = "SMTP Listener"
            };
            ListenThread.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (smtpListener != null)
                smtpListener.Stop();

            ListenThread.Abort();
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Adds session.
        /// </summary>
        /// <param name="p_session">Session ID.</param>
        /// <param name="p_responsor">Session object.</param>
        /// <param name="logWriter">Log writer.</param>
        public void AddSession(string p_session, MailResponsor p_responsor)
        {
            SessionTable.Add(p_session, p_responsor);

            WriteLog(String.Format("session[{0}]: {1}", p_session, "added"), true);
        }

        /// <summary>
        /// Removes session.
        /// </summary>
        /// <param name="p_session">Session ID.</param>
        /// <param name="logWriter">Log writer.</param>
        public void RemoveSession(string p_session)
        {
            lock (SessionTable)
            {
                if (SessionTable.Contains(p_session) == false)
                {
                    WriteLog(String.Format("session[{0}]: {1}", p_session, "doesn't exist"), true);
                    return;
                }

                SessionTable.Remove(p_session);
            }

            WriteLog(String.Format("session[{0}]: {1}", p_session, "removed"), true);
        }

        /// <summary>
        /// To validate mail address
        /// </summary>
        /// <param name="p_address"></param>
        /// <returns></returns>
        public bool ValidatingAddress(string p_session, string p_address)
        {
            WriteLog(String.Format("session[{0}]: {1} '{2}'", p_session, "validating address", p_address), false);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_userName"></param>
        /// <param name="p_password"></param>
        /// <returns></returns>
        public bool ValidatingUserName(string p_session, string p_userName, string p_password)
        {
            WriteLog(String.Format("session[{0}]: {1} '{2}'", p_session, "validating user name", p_userName), false);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_mailFrom"></param>
        /// <returns></returns>
        public bool ValidatingMailFrom(string p_session, string p_mailFrom)
        {
            WriteLog(String.Format("session[{0}]: {1} '{2}'", p_session, "validating from", p_mailFrom), false);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_mailTo"></param>
        /// <returns></returns>
        public bool ValidatingMailTo(string p_session, string p_mailTo)
        {
            var _result = false;

            WriteLog(String.Format("session[{0}]: {1} '{2}'", p_session, "validating to", p_mailTo), false);

            string _mailbox = p_mailTo.ToLower();
            if (_mailbox.IndexOf("@") != -1)
                _mailbox = _mailbox.Substring(0, _mailbox.IndexOf("@"));

            if (_mailbox == AspMailAddress || _mailbox == "test")
                _result = true;

            return _result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eAddress"></param>
        /// <param name="p_size"></param>
        /// <returns></returns>
        public bool ValidatingMailBoxSize(string p_session, string p_eAddress, long p_size)
        {
            WriteLog(String.Format("session[{0}]: {1} {2}", p_session, "validating size of message", p_size), false);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_mailfrom"></param>
        /// <param name="p_mailto"></param>
        /// <param name="p_folder"></param>
        /// <param name="p_message"></param>
        public void StoreMessage(string p_mailfrom, string[] p_mailto, string p_folder, MemoryStream p_message)
        {
            foreach (string _mailto in p_mailto)
                StoreMessage(p_mailfrom, _mailto, p_folder, p_message);
        }

        /// <summary>
        /// Stores message to specified mailbox.
        /// </summary>
        /// <param name="mailbox">Mailbox name.</param>
        /// <param name="folder">Folder where to store message. Eg. 'Inbox'.</param>
        /// <param name="msgStream">Stream where message has stored.</param>
        /// <param name="to">Recipient email address.</param>
        /// <param name="from">Sendred email address.</param>
        /// <param name="isRelay">Specifies if message must be relayed.</param>
        /// <param name="date">Recieve date.</param>
        /// <param name="flags">Message flags.</param>
        public void StoreMessage(string p_mailfrom, string p_mailto, string p_folder, MemoryStream p_message)
        {
            //byte[] _topLines = null;
            //_topLines = GetTopLines(p_message, 50);

            try
            {
                var _today = DateTime.Now;
                string _fileid = Guid.NewGuid().ToString();

                string _mailbox = p_mailto;
                {
                    if (p_mailto.IndexOf("@") != -1)
                        _mailbox = p_mailto.Substring(0, p_mailto.IndexOf("@"));

                    string _sqlstr
                        = "INSERT INTO TB_eTAX_MAILBOX "
                        + "( "
                        + " fileid, mailbox, folder, data, size, sender, createtime "
                        + ") "
                        + "VALUES "
                        + "( "
                        + " @fileid, @mailbox, @folder, @data, @size, @sender, @createtime "
                        + ")";

                    var _dbps = new DatParameters();
                    {
                        _dbps.Add("@fileid", SqlDbType.NVarChar, _fileid);
                        _dbps.Add("@mailbox", SqlDbType.NVarChar, _mailbox);
                        _dbps.Add("@folder", SqlDbType.NVarChar, p_folder);
                        _dbps.Add("@data", SqlDbType.Image, p_message.ToArray());
                        _dbps.Add("@size", SqlDbType.Decimal, p_message.Length);
                        _dbps.Add("@sender", SqlDbType.NVarChar, p_mailfrom);
                        _dbps.Add("@createtime", SqlDbType.DateTime, _today);
                    }

                    if (LDataHelper.ExecuteText(ConnectionString, _sqlstr, _dbps) < 1)
                    {
                        if (LogCommands == true)
                            ELogger.SNG.WriteLog(String.Format("mail insert failure: fileid: {0}, mailbox: {1}, folder: {2}, sender: {3}", _fileid, _mailbox, p_folder, p_mailfrom));
                    }
                    else
                    {
                        if (LogCommands == true && LiveServer == true)
                            ELogger.SNG.WriteLog(String.Format("mail insert success: fileid: {0}, mailbox: {1}, folder: {2}, sender: {3}", _fileid, _mailbox, p_folder, p_mailfrom));
                    }
                }

                if (MailSniffing == true)
                {
                    p_message.Seek(0, SeekOrigin.Begin);

                    string _directory = Path.Combine(USvcHelper.eMailFolder, IProvider.Proxy.ProductId);
                    {
                        _directory = Path.Combine(_directory, _mailbox);
                        _directory = Path.Combine(_directory, p_folder);
                        _directory = Path.Combine(_directory, _today.ToString("yyyyMMdd"));
                    }

                    string _filename = _fileid + ".eml";
                    QFWriter.QueueWrite(_directory, _filename, p_message.ToArray());
                }

                if (_mailbox == AspMailAddress)
                {
                    p_message.Seek(0, SeekOrigin.Begin);

                    MimeParser _parser = new MimeParser(p_message.ToArray());
                    if (_parser.MimeEntries.Count > 1)
                    {
                        MimeEntry _attachment = (MimeEntry)_parser.MimeEntries[1];
                        {
                            byte[] _obuffer = CmsManager.SNG.GetDecryptedContent(UCertHelper.AspKmCert.X509Cert2, _attachment.Data);

                            XmlDocument _taxdoc = new XmlDocument();
                            _taxdoc.LoadXml(Encoding.UTF8.GetString(_obuffer));

                            Reader _reader = new Reader(_taxdoc);
                            DataSet _purchaseSet = _reader.TaxPurchase();

                            LDltaHelper.InsertDeltaSet(ConnectionString, _purchaseSet);
                        }

                        // update [issueid] for compare with purchase-table.
                        string _issueid = Path.GetFileNameWithoutExtension(_attachment.FileName);
                        {
                            string _sqlstr
                                = "UPDATE TB_eTAX_MAILBOX "
                                + "   SET filler0=@issueid "
                                + " WHERE fileid=@fileid";

                            var _dbps = new DatParameters();
                            _dbps.Add("@fileid", SqlDbType.NVarChar, _fileid);
                            _dbps.Add("@issueid", SqlDbType.NVarChar, _issueid);

                            if (LDataHelper.ExecuteText(ConnectionString, _sqlstr, _dbps) < 1)
                            {
                                if (LogCommands == true)
                                    ELogger.SNG.WriteLog(String.Format("mail update failure: fileid: {0}, issueid: {1}, sender: {2}", _fileid, _issueid, p_mailfrom));
                            }
                            else
                            {
                                if (LogCommands == true && LiveServer == true)
                                    ELogger.SNG.WriteLog(String.Format("mail update success: fileid: {0}, issueid: {1}, sender: {2}", _fileid, _issueid, p_mailfrom));
                            }
                        }
                    }
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
                if (m_iprovider != null)
                {
                    m_iprovider.Dispose();
                    m_iprovider = null;
                }
        }

        /// <summary>
        /// 
        /// </summary>
        ~MailListener()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}