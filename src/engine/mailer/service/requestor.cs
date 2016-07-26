using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using OpenETaxBill.Channel.Library.Net;
using OpenETaxBill.Channel.Library.Net.Dns;
using OpenETaxBill.Channel.Library.Net.Smtp.Client;

namespace OpenETaxBill.Engine.Mailer
{
    /// <summary>
    /// SMTP Client.
    /// </summary>
    public class MailRequestor : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_dnsServers"></param>
        /// <param name="p_mailSniffing"></param>
        public MailRequestor(string[] p_dnsServers, bool p_mailSniffing)
        {
            DnsServers = p_dnsServers;
            MailSniffing = p_mailSniffing;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        public bool MailSniffing
        {
            get;
            set;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private ArrayList m_errors = null;
        private ArrayList SendErrors
        {
            get
            {
                if (m_errors == null)
                    m_errors = new ArrayList();

                return m_errors;
            }
        }

        private NetCore m_core = null;
        private NetCore SmtpCore
        {
            get
            {
                if (m_core == null)
                    m_core = new NetCore();

                return m_core;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private Hashtable GetSameDomain(string[] p_receipients)
        {
            Hashtable _result = new Hashtable();

            foreach (string _receipient in p_receipients)
            {
                string _emailDomain = _receipient.Substring(_receipient.IndexOf("@"));//*******

                if (_result.Contains(_emailDomain))
                {
                    // User eAddress is in same server
                    ArrayList _sameAddresses = (ArrayList)_result[_emailDomain];
                    _sameAddresses.Add(_receipient);
                }
                else
                {
                    ArrayList _sameAddresses = new ArrayList();
                    _sameAddresses.Add(_receipient);

                    _result.Add(_emailDomain, _sameAddresses);
                }
            }

            return _result;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Sends data to socket.
        /// </summary>
        /// <param name="p_socket"></param>
        /// <param name="p_sessionid"></param>
        /// <param name="p_connectedip"></param>
        /// <param name="p_lineData"></param>
        private void SendLine(Socket p_socket, string p_sessionid, string p_connectedip, string p_lineData, bool p_writeLog)
        {
            SmtpCore.SendLine(p_socket, p_lineData);

            if (MailSniffing == true && p_writeLog == true)
                ELogger.SNG.WriteLog("E", String.Format("session[{0}] >>> server[{1}]: {2}", p_sessionid, p_connectedip, p_lineData.Replace("\r\n", "<CRLF>")));
        }

        /// <summary>
        /// Reads data from socket.
        /// </summary>
        /// <param name="p_socket"></param>
        /// <param name="p_sessionid"></param>
        /// <param name="p_connectedip"></param>
        /// <returns></returns>
        private string ReadLine(Socket p_socket, string p_sessionid, string p_connectedip, bool p_writeLog)
        {
            string _result = SmtpCore.ReadLine(p_socket);

            if (MailSniffing == true && p_writeLog == true)
                ELogger.SNG.WriteLog("E", String.Format("session[{0}] <<< server[{1}]: {2}", p_sessionid, p_connectedip, _result + "<CRLF>"));

            return _result;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_receipients"></param>
        /// <param name="p_reversePath"></param>
        /// <param name="p_message"></param>
        /// <returns></returns>
        private bool SendMessageToServer(string[] p_receipients, string p_reversePath, Stream p_message)
        {
            ArrayList _defectiveEmails = new ArrayList();

            Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                string _reply = "";

                var _SIZE_Support = false;
                var _8BIT_Support = false;
                var _BDAT_Support = false;

                if (UseSmartHost == true)
                {
                    IPEndPoint _ipdestination = new IPEndPoint(System.Net.Dns.GetHostEntry(SmartHost).AddressList[0], Port);
                    _socket.Connect(_ipdestination);
                }
                else
                {
                    //---- Parse e-domain -------------------------------//
                    string _emailDomain = p_receipients[0];

                    // eg. Ivx <ivx@lumisoft.ee>
                    if (_emailDomain.IndexOf("<") > -1 && _emailDomain.IndexOf(">") > -1)
                        _emailDomain = _emailDomain.Substring(_emailDomain.IndexOf("<") + 1, _emailDomain.IndexOf(">") - _emailDomain.IndexOf("<") - 1);

                    if (_emailDomain.IndexOf("@") > -1)
                        _emailDomain = _emailDomain.Substring(_emailDomain.LastIndexOf("@") + 1);

                    //--- Get MX record -------------------------------------------//
                    DnsEx _dnsex = new DnsEx();
                    DnsEx.DnsServers = DnsServers;

                    MX_Record[] _mxRecords = null;
                    DnsReplyCode _replyCode = _dnsex.GetMXRecords(_emailDomain, out _mxRecords);

                    switch (_replyCode)
                    {
                        case DnsReplyCode.Ok:
                            var _isConnected = false;

                            // Try all available hosts by MX preference order, if can't connect specified host.
                            foreach (MX_Record _mx in _mxRecords)
                            {
                                try
                                {
                                    _socket.Connect(new IPEndPoint(System.Net.Dns.GetHostEntry(_mx.Host).AddressList[0], Port));

                                    _isConnected = true;
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    // Just skip and let for to try next host.
                                    OnFaulted(SmtpErrorType.ConnectionError, p_receipients, "While retriveing MX-Servers, raise exception error: " + ex.Message);
                                }
                            }

                            if (_isConnected == false)
                                return false;

                            break;

                        case DnsReplyCode.NoEntries:
                            /* Rfc 2821 5
                            If   no MX records are found, but an A RR is found, the A RR is treated as
                            if it was associated with an implicit MX RR, with a preference of 0,
                            pointing to that host.

                            */
                            try // Try to connect with A record
                            {
                                IPHostEntry _ipEntry = System.Net.Dns.GetHostEntry(_emailDomain);
                                _socket.Connect(new IPEndPoint(_ipEntry.AddressList[0], Port));
                            }
                            catch
                            {
                                OnFaulted(SmtpErrorType.InvalidEmailAddress, p_receipients, String.Format("email domain <{0}> is invalid", _emailDomain));

                                _defectiveEmails.AddRange(p_receipients);
                                return false;
                            }
                            break;

                        case DnsReplyCode.TempError:
                            string _dnsServers = "";
                            foreach (string s in DnsServers)
                                _dnsServers += s + ";";

                            throw new Exception(String.Format("Error retrieving MX record for domain '{0}' with dns servers:{{{1}}}.", _emailDomain, _dnsServers));
                    }
                }

                string _sessionId = _socket.GetHashCode().ToString();
                string _connectedip = SmtpCore.ParseIP_from_EndPoint(_socket.RemoteEndPoint.ToString());

                // Get 220 reply from server
                /* NOTE: reply may be multiline
				   220 xx ready
				    or
				   220-someBull
				   200 xx
				*/

                // Server must reply 220 - Server OK
                _reply = ReadLine(_socket, _sessionId, _connectedip, true);
                if (IsReplyCode("220", _reply) == false)
                {
                    OnFaulted(SmtpErrorType.UnKnown, p_receipients, _reply);
                    SendLine(_socket, _sessionId, _connectedip, "QUIT", true);

                    return false;
                }
                else
                {
                    // 220-xxx<CRLF>
                    // 220 aa<CRLF> - means end
                    // reply isn't complete, get more
                    while (_reply.IndexOf("220 ") == -1)
                        _reply += ReadLine(_socket, _sessionId, _connectedip, false);
                }

                // cmd EHLO/HELO

                // Send greeting to server
                SendLine(_socket, _sessionId, _connectedip, "EHLO " + SmtpCore.GetHostName(), true);

                _reply = ReadLine(_socket, _sessionId, _connectedip, true);
                if (IsReplyCode("250", _reply) == false)
                {
                    // EHLO failed, maybe server doesn't support it, try HELO

                    SendLine(_socket, _sessionId, _connectedip, "HELO " + SmtpCore.GetHostName(), true);

                    _reply = ReadLine(_socket, _sessionId, _connectedip, false);
                    if (IsReplyCode("250", _reply) == false)
                    {
                        OnFaulted(SmtpErrorType.UnKnown, p_receipients, _reply);
                        SendLine(_socket, _sessionId, _connectedip, "QUIT", true);

                        _defectiveEmails.AddRange(p_receipients);
                        return false;
                    }
                    //	else
                    //  {
                    //		supports_ESMTP = false;
                    //	}
                }
                else
                {
                    // 250-xxx<CRLF>
                    // 250 aa<CRLF> - means end
                    // reply isn't complete, get more
                    while (_reply.IndexOf("250 ") == -1)
                    {
                        _reply += ReadLine(_socket, _sessionId, _connectedip, false);
                    }

                    // Check if SIZE argument is supported
                    if (_reply.ToUpper().IndexOf("SIZE") > -1)
                    {
                        _SIZE_Support = true;
                    }

                    // Check if 8BITMIME argument is supported
                    if (_reply.ToUpper().IndexOf("8BITMIME") > -1)
                    {
                        _8BIT_Support = true;
                    }

                    // Check if CHUNKING argument is supported
                    if (_reply.ToUpper().IndexOf("CHUNKING") > -1)
                    {
                        _BDAT_Support = true;
                    }
                }

                // If server doesn't support 8bit mime, check if message is 8bit.
                // If is we MAY NOT send this message or loss of data
                if (_8BIT_Support == false)
                {
                    if (Is8BitMime(p_message) == true)
                    {
                        OnFaulted(SmtpErrorType.NotSupported, p_receipients, "Message is 8-Bit mime and server doesn't support it.");
                        SendLine(_socket, _sessionId, _connectedip, "QUIT", true);

                        return false;
                    }
                }

                // cmd MAIL
                // NOTE: Syntax:{MAIL FROM:<ivar@lumisoft.ee> [SIZE=msgSize]<CRLF>}

                // Send Mail From
                if (_SIZE_Support == true)
                {
                    SendLine(_socket, _sessionId, _connectedip, String.Format("MAIL FROM:<{0}> SIZE={1}", p_reversePath, p_message.Length), true);
                }
                else
                {
                    SendLine(_socket, _sessionId, _connectedip, String.Format("MAIL FROM:<{0}>", p_reversePath), true);
                }

                _reply = ReadLine(_socket, _sessionId, _connectedip, false);
                if (IsReplyCode("250", _reply) == false)
                {
                    // To Do: Check if size exceeded error:

                    OnFaulted(SmtpErrorType.UnKnown, p_receipients, _reply);
                    SendLine(_socket, _sessionId, _connectedip, "QUIT", true);

                    _defectiveEmails.AddRange(p_receipients);
                    return false;
                }

                // cmd RCPT
                // NOTE: Syntax:{RCPT TO:<ivar@lumisoft.ee><CRLF>}

                var _isAnyValidEmail = false;
                foreach (string _receipt in p_receipients)
                {
                    // Send Mail To
                    SendLine(_socket, _sessionId, _connectedip, String.Format("RCPT TO:<{0}>", _receipt), true);

                    _reply = ReadLine(_socket, _sessionId, _connectedip, false);
                    if (IsReplyCode("250", _reply) == false)
                    {
                        // Is unknown user
                        if (IsReplyCode("550", _reply))
                        {
                            OnFaulted(SmtpErrorType.InvalidEmailAddress, new string[] { _receipt }, _reply);
                        }
                        else
                        {
                            OnFaulted(SmtpErrorType.UnKnown, new string[] { _receipt }, _reply);
                        }

                        _defectiveEmails.Add(_receipt);
                    }
                    else
                    {
                        _isAnyValidEmail = true;
                    }
                }

                // If there isn't any valid email - quit.
                if (_isAnyValidEmail == false)
                {
                    SendLine(_socket, _sessionId, _connectedip, "QUIT", true);
                    return false;
                }

                // cmd DATA

                if (_BDAT_Support == false)
                {
                    // Notify Data Start
                    SendLine(_socket, _sessionId, _connectedip, "DATA", false);

                    _reply = ReadLine(_socket, _sessionId, _connectedip, false);
                    if (IsReplyCode("354", _reply) == false)
                    {
                        OnFaulted(SmtpErrorType.UnKnown, p_receipients, _reply);
                        SendLine(_socket, _sessionId, _connectedip, "QUIT", true);

                        _defectiveEmails.AddRange(p_receipients);
                        return false;
                    }

                    //------- Do period handling -----------------------------------------//
                    // If line starts with '.', add additional '.'.(Read rfc for more info)
                    MemoryStream _periodOk = SmtpCore.DoPeriodHandling(p_message, true, false);
                    //--------------------------------------------------------------------//

                    // Check if message ends with <CRLF>, if not add it. -------//
                    if (_periodOk.Length >= 2)
                    {
                        byte[] _byteEnd = new byte[2];
                        _periodOk.Position = _periodOk.Length - 2;
                        _periodOk.Read(_byteEnd, 0, 2);

                        if (_byteEnd[0] != (byte)'\r' && _byteEnd[1] != (byte)'\n')
                            _periodOk.Write(new byte[] { (byte)'\r', (byte)'\n' }, 0, 2);
                    }

                    _periodOk.Position = 0;
                    //-----------------------------------------------------------//

                    //---- Send message --------------------------------------------//
                    long _totalSent = 0;
                    long _totalSize = _periodOk.Length;

                    while (_totalSent < _totalSize)
                    {
                        byte[] _buffer = new byte[4000];

                        int _readSize = _periodOk.Read(_buffer, 0, _buffer.Length);
                        int _sentSize = _socket.Send(_buffer, 0, _readSize, SocketFlags.None);

                        _totalSent += _sentSize;
                        if (_sentSize != _readSize)
                            _periodOk.Position = _totalSent;

                        OnProgress(_sentSize, _totalSent, _totalSize);
                    }

                    //-------------------------------------------------------------//
                    _periodOk.Close();

                    // Notify End of Data
                    SendLine(_socket, _sessionId, _connectedip, ".", false);

                    _reply = ReadLine(_socket, _sessionId, _connectedip, false);
                    if (IsReplyCode("250", _reply) == false)
                    {
                        OnFaulted(SmtpErrorType.UnKnown, p_receipients, _reply);
                        SendLine(_socket, _sessionId, _connectedip, "QUIT", true);

                        _defectiveEmails.AddRange(p_receipients);
                        return false;
                    }
                }

                // cmd BDAT

                if (_BDAT_Support)
                {
                    SendLine(_socket, _sessionId, _connectedip, String.Format("BDAT {0} LAST", (p_message.Length - p_message.Position)), false);

                    //---- Send message --------------------------------------------//
                    long _totalSent = 0;
                    long _totalSize = p_message.Length - p_message.Position;

                    while (_totalSent < _totalSize)
                    {
                        byte[] _buffer = new byte[4000];

                        int _readSize = p_message.Read(_buffer, 0, _buffer.Length);
                        int _sentSize = _socket.Send(_buffer, 0, _readSize, SocketFlags.None);

                        _totalSent += _sentSize;
                        if (_sentSize != _readSize)
                            p_message.Position = _totalSent;

                        OnProgress(_sentSize, _totalSent, _totalSize);
                    }
                    //-------------------------------------------------------------//

                    // Get store result
                    _reply = ReadLine(_socket, _sessionId, _connectedip, false);
                    if (_reply.StartsWith("250") == false)
                    {
                        OnFaulted(SmtpErrorType.UnKnown, p_receipients, _reply);
                        SendLine(_socket, _sessionId, _connectedip, "QUIT", true);

                        _defectiveEmails.AddRange(p_receipients);
                        return false;
                    }
                }

                // cmd QUIT

                // Notify server - server can exit now
                SendLine(_socket, _sessionId, _connectedip, "QUIT", true);

                _reply = ReadLine(_socket, _sessionId, _connectedip, true);
            }
            catch (Exception ex)
            {
                OnFaulted(SmtpErrorType.UnKnown, p_receipients, ex.Message);

                _defectiveEmails.AddRange(p_receipients);
                return false;
            }
            finally
            {
                // Raise event
                OnCompleted(Thread.CurrentThread.GetHashCode().ToString(), p_receipients, _defectiveEmails);

                _socket.Close();
            }

            return true;
        }

        /// <summary>
        /// Checks if reply code.
        /// </summary>
        /// <param name="replyCode">Replay code to check.</param>
        /// <param name="reply">Full repaly.</param>
        /// <returns>Retruns true if reply is as specified.</returns>
        private bool IsReplyCode(string p_replyCode, string p_replyText)
        {
            if (p_replyText.IndexOf(p_replyCode) > -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_istream"></param>
        /// <returns></returns>
        private bool Is8BitMime(Stream p_istream)
        {
            var _result = false;

            long _position = p_istream.Position;
            TextReader _reader = new StreamReader(p_istream);

            string _line = _reader.ReadLine();
            while (_line != null)
            {
                if (_line.ToUpper().IndexOf("CONTENT-TR") > -1)
                {
                    if (_line.ToUpper().IndexOf("8BIT") > -1)
                    {
                        _result = true;
                        break; // Contains 8bit mime
                    }
                }

                _line = _reader.ReadLine();
            }

            // Restore stream position
            p_istream.Position = _position;

            return _result;
        }

        private string m_smartHost = "";

        /// <summary>
        /// Gets or sets smart host. Eg. 'mail.yourserver.net'.
        /// </summary>
        public string SmartHost
        {
            get
            {
                return m_smartHost;
            }

            set
            {
                m_smartHost = value;
            }
        }

        /// <summary>
        /// Gets or sets dns servers(IP addresses).
        /// </summary>
        public string[] DnsServers
        {
            get;
            set;
        }

        private bool m_useSmartHost = false;

        /// <summary>
        /// Gets or sets if mail is sent through smart host or using dns.
        /// </summary>
        public bool UseSmartHost
        {
            get
            {
                return m_useSmartHost;
            }

            set
            {
                m_useSmartHost = value;
            }
        }

        private int m_tcpPort = 25;

        /// <summary>
        /// Gets or sets SMTP port.
        /// </summary>
        public int Port
        {
            get
            {
                return m_tcpPort;
            }

            set
            {
                m_tcpPort = value;
            }
        }

        private int m_maxThreads = 16;

        /// <summary>
        /// Gets or sets maximum sender Threads.
        /// </summary>
        public int MaxSenderThreads
        {
            get
            {
                return m_maxThreads;
            }

            set
            {
                if (value < 1)
                {
                    m_maxThreads = 1;
                }
                else
                {
                    m_maxThreads = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                var _result = "";

                foreach (SmtpError _error in SendErrors)
                {
                    _result += _error.ErrorType.ToString();

                    foreach (string _recipient in _error.AffectedEmails)
                        _result += ", " + _recipient;

                    _result += ", " + _error.ErrorText;
                    _result += Environment.NewLine;
                }

                return _result;
            }
        }

        /// <summary>
        /// Raises PartOfMessageIsSent event.
        /// </summary>
        /// <param name="sentBlockSize"></param>
        /// <param name="totalSent"></param>
        /// <param name="messageSize"></param>
        protected void OnProgress(long sentBlockSize, long totalSent, long messageSize)
        {
        }

        /// <summary>
        /// Raises SendJobCompleted event.
        /// </summary>
        /// <param name="jobID"></param>
        /// <param name="to"></param>
        /// <param name="defectiveEmails"></param>
        protected void OnCompleted(string jobID, string[] to, ArrayList defectiveEmails)
        {
        }

        /// <summary>
        /// Raises Error event.
        /// </summary>
        /// <param name="type">Error type.</param>
        /// <param name="affectedAddresses">Affected email addresses.</param>
        /// <param name="errorText">Error text.</param>
        protected void OnFaulted(SmtpErrorType type, string[] affectedAddresses, string errorText)
        {
            // we must lock write(add), becuse multiple Threads may raise OnError same time.
            lock (SendErrors)
            {
                SendErrors.Add(new SmtpError(type, affectedAddresses, errorText));
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Send(string[] p_receipients, string p_sender, Stream p_message)
        {
            var _result = true;

            SendErrors.Clear();

            Hashtable _rcptPerServer = GetSameDomain(p_receipients);

            // Loop through the list of servers where we must send messages
            foreach (ArrayList _sameAddresses in _rcptPerServer.Values)
            {
                string[] _reciepients = new string[_sameAddresses.Count];
                _sameAddresses.CopyTo(_reciepients);

                p_message.Seek(0, SeekOrigin.Begin);
                if (SendMessageToServer(_reciepients, p_sender, p_message) == false)
                {
                    _result = false;
                    break;
                }
            }

            return _result;
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
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~MailRequestor()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}
