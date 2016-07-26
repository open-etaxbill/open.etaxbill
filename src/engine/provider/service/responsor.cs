using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using OpenETaxBill.Channel.Library.Net;
using OpenETaxBill.Channel.Library.Net.Core;
using OpenETaxBill.Channel.Library.Net.Smtp;

namespace OpenETaxBill.Engine.Provider
{
    //-------------------------------------------------------------------------------------------------------------------------
    // 
    //-------------------------------------------------------------------------------------------------------------------------
    
    /// <summary>
    /// 
    /// </summary>
    public class MailResponsor : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        public MailResponsor(MailListener p_smtpServer, Socket p_clientSocket, string p_seesionId)
        {
            m_smtpServer = p_smtpServer;
            m_clientSocket = p_clientSocket;
            m_sessionId = p_seesionId;

            m_sessionStartTime = DateTime.Now;
        }

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

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        public void Process()
        {
            try
            {
                // Store client ip and host name.
                m_connectedIP = SmtpCore.ParseIP_from_EndPoint(ClientSocket.RemoteEndPoint.ToString());
                m_connectedHostName = SmtpCore.GetHostName(ConnectedIP);

                // Check if ip is allowed to connect this computer
                if (Parent.ValidatingAddress(SessionID, ClientSocket.RemoteEndPoint.ToString()))
                {
                    // Notify that server is ready
                    SendLine(String.Format("220 {0} Service ready\r\n", MyHostName), true);

                    //------ Create command loop --------------------------------//
                    // Loop while QUIT cmd or Session TimeOut.
                    long _lastCmdTime = DateTime.Now.Ticks;

                    while (true)
                    {
                        // If there is any data available, begin command reading.			
                        if (ClientSocket.Available > 0)
                        {
                            try
                            {
                                string _lastCmd = ReadLine(false);
                                if (SwitchCommand(_lastCmd) == true)
                                    break;
                            }
                            catch (ReadException ex)
                            {
                                //---- Check that maximum bad commands count isn't exceeded ---------------//
                                if (BadCmdCounter > Parent.MaxBadCommands - 1)
                                {
                                    SendLine("421 Too many bad commands, closing transmission channel\r\n", false);
                                    break;
                                }

                                BadCmdCounter++;
                                //-------------------------------------------------------------------------//

                                switch (ex.ReadReplyCode)
                                {
                                    case ReadReplyCode.LengthExceeded:
                                        SendLine("500 Line too long.\r\n", false);
                                        break;

                                    case ReadReplyCode.TimeOut:
                                        SendLine("500 Command timeout.\r\n", false);
                                        break;

                                    case ReadReplyCode.UnKnownError:
                                        SendLine("500 Unknown error.\r\n", false);
                                        ELogger.SNG.WriteLog(ex);
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                // Connection lost
                                if (ClientSocket.Connected == false)
                                    break;

                                SendLine("500 Unknown temp error.\r\n", false);
                                ELogger.SNG.WriteLog(ex);
                            }

                            // reset last command time
                            _lastCmdTime = DateTime.Now.Ticks;
                        }
                        else
                        {
                            //----- Session timeout stuff ------------------------------------------------//
                            if (DateTime.Now.Ticks > _lastCmdTime + ((long)(Parent.SessionIdleTimeOut)) * 10000)
                            {
                                // Notify for closing
                                SendLine("421 Session timeout, closing transmission channel\r\n", false);
                                break;
                            }

                            // Wait 100ms to save CPU, otherwise while loop may take 100% CPU. 
                            Thread.Sleep(100);
                            //---------------------------------------------------------------------------//
                        }
                    }
                }
            }
            catch (ThreadInterruptedException ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            catch (Exception ex)
            {
                if (ClientSocket.Connected == true)
                {
                    SendLine("421 Service not available, closing transmission channel\r\n", false);
                    ELogger.SNG.WriteLog(ex);
                }
                else
                {
                    WriteLog(String.Format("session[{0}] >>> client[{1}]: {2}", SessionID, ConnectedIP, "connection is aborted by client machine"));
                }
            }
            finally
            {
                Parent.RemoveSession(SessionID);

                if (ClientSocket.Connected == true)
                    ClientSocket.Close();
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // function SwitchCommand
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Executes SMTP command.
        /// </summary>
        /// <param name="SMTP_commandTxt">Original command text.</param>
        /// <returns>Returns true if must end session(command loop).</returns>
        private bool SwitchCommand(string p_command)
        {
            var _result = false;

            //---- Parse command --------------------------------------------------//
            string[] _parts = p_command.TrimStart().Split(new char[] { ' ' });
            string _command = _parts[0].ToUpper().Trim();
            string _args = SmtpCore.GetArgsText(p_command, _command);
            //---------------------------------------------------------------------//

            switch (_command)
            {
                case "HELO":
                    HELO(_args);
                    break;

                case "EHLO":
                    EHLO(_args);
                    break;

                case "AUTH":
                    AUTH(_args);
                    break;

                case "MAIL":
                    MAIL(_args);
                    break;

                case "RCPT":
                    RCPT(_args);
                    break;

                case "DATA":
                    DATA(_args);
                    break;

                case "BDAT":
                    BDAT(_args);
                    break;

                case "RSET":
                    RSET(_args);
                    break;

                case "VRFY":
                    VRFY();
                    break;

                case "EXPN":
                    EXPN();
                    break;

                case "HELP":
                    HELP();
                    break;

                case "NOOP":
                    NOOP();
                    break;

                case "QUIT":
                    QUIT(_args);
                    _result = true;
                    break;

                default:
                    SendLine("500 command unrecognized\r\n", false);

                    //---- Check that maximum bad commands count isn't exceeded ---------------//
                    if (BadCmdCounter > Parent.MaxBadCommands - 1)
                    {
                        SendLine("421 Too many bad commands, closing transmission channel\r\n", false);
                        return true;
                    }

                    BadCmdCounter++;
                    break;
            }

            return _result;
        }

        /// <summary>
        /// function HELO
        /// </summary>
        /// <param name="argsText"></param>
        private void HELO(string argsText)
        {
            /* Rfc 2821 4.1.1.1
            These commands, and a "250 OK" reply to one of them, confirm that
            both the SMTP client and the SMTP server are in the initial state,
            that is, there is no transaction in progress and all state tables and
            buffers are cleared.
			
            Syntax:
                 "HELO" SP Domain CRLF
            */

            ResetState();

            SendLine(String.Format("250 {0} Hello [{1}]\r\n", MyHostName, ConnectedIP), true);
            CmdValidator.HELO_OK = true;
        }

        /// <summary>
        /// function EHLO
        /// </summary>
        /// <param name="argsText"></param>
        private void EHLO(string argsText)
        {
            /* Rfc 2821 4.1.1.1
            These commands, and a "250 OK" reply to one of them, confirm that
            both the SMTP client and the SMTP server are in the initial state,
            that is, there is no transaction in progress and all state tables and
            buffers are cleared.
            */

            ResetState();

            string _reply
                = "250-" + MyHostName + " Hello [" + ConnectedIP + "]\r\n"
                + "250-PIPELINING\r\n"
                + "250-SIZE " + Parent.MaxMessageSize + "\r\n"
                //+  "250-DSN\r\n"  
                //+  "250-HELP\r\n"                 
                + "250-8BITMIME\r\n"
                + "250-BINARYMIME\r\n"
                + "250-CHUNKING\r\n"
                + "250-AUTH LOGIN CRAM-MD5\r\n" //CRAM-MD5 DIGEST-MD5
                + "250 Ok\r\n";

            SendLine(_reply, false);

            CmdValidator.HELO_OK = true;
        }

        /// <summary>
        /// function AUTH
        /// </summary>
        /// <param name="argsText"></param>
        private void AUTH(string argsText)
        {
            /* Rfc 2554 AUTH --------------------------------------------------//
            Restrictions:
                 After an AUTH command has successfully completed, no more AUTH
                 commands may be issued in the same session.  After a successful
                 AUTH command completes, a server MUST reject any further AUTH
                 commands with a 503 reply.
				 
            Remarks: 
                If an AUTH command fails, the server MUST behave the same as if
                the client had not issued the AUTH command.
            */
            if (Authenticated)
            {
                SendLine("503 already authenticated\r\n", false);
                return;
            }


            //------ Parse parameters -------------------------------------//
            string _userName = "";
            string _password = "";

            string[] _param = argsText.Split(new char[] { ' ' });
            switch (_param[0].ToUpper())
            {
                case "PLAIN":
                    SendLine("504 Unrecognized authentication type.\r\n", false);
                    break;

                case "LOGIN":

                    // LOGIN authentication

                    //---- AUTH = LOGIN ------------------------------
                    /* Login
                    C: AUTH LOGIN-MD5
                    S: 334 VXNlcm5hbWU6
                    C: username_in_base64
                    S: 334 UGFzc3dvcmQ6
                    C: password_in_base64
					
                       VXNlcm5hbWU6 base64_decoded= USERNAME
                       UGFzc3dvcmQ6 base64_decoded= PASSWORD
                    */
                    // Note: all strings are base64 strings eg. VXNlcm5hbWU6 = UserName.


                    // Query UserName
                    SendLine("334 VXNlcm5hbWU6\r\n", false);

                    // Encode username from base64
                    string _userNameLine = ReadLine(true);
                    if (_userNameLine.Length > 0)
                        _userName = Encoding.UTF8.GetString(Convert.FromBase64String(_userNameLine));

                    // Query Password
                    SendLine("334 UGFzc3dvcmQ6\r\n", false);

                    // Encode password from base64
                    string _passwordLine = ReadLine(true);
                    if (_passwordLine.Length > 0)
                        _password = Encoding.UTF8.GetString(Convert.FromBase64String(_passwordLine));

                    if (Parent.ValidatingUserName(SessionID, _userName, _password) == true)
                    {
                        SendLine("235 Authentication successful.\r\n", false);
                        Authenticated = true;

                        m_userName = _userName;
                    }
                    else
                    {
                        SendLine("535 Authentication failed\r\n", false);
                    }
                    break;

                case "CRAM-MD5":

                    // CRAM-MD5 authentication

                    /* Cram-M5
					C: AUTH CRAM-MD5
					S: 334 <md5_calculation_hash_in_base64>
					C: base64(decoded:username password_hash)
					*/

                    string _md5Hash = String.Format("<{0}>", Guid.NewGuid().ToString().ToLower());
                    SendLine(String.Format("334 {0}\r\n", Convert.ToBase64String(Encoding.UTF8.GetBytes(_md5Hash))), false);

                    string _replyLine = ReadLine(true);
                    if (_replyLine.Length > 0)
                    {
                        _replyLine = Encoding.UTF8.GetString(Convert.FromBase64String(_replyLine));

                        string[] _replyArgs = _replyLine.Split(' ');
                        _userName = _replyArgs[0];
                        _password = _replyArgs[1];
                    }

                    if (Parent.ValidatingUserName(SessionID, _userName, _password) == true)
                    {
                        SendLine("235 Authentication successful.\r\n", false);
                        Authenticated = true;

                        m_userName = _userName;
                    }
                    else
                    {
                        SendLine("535 Authentication failed\r\n", false);
                    }
                    break;

                case "DIGEST-MD5":
                    SendLine("504 Unrecognized authentication type.\r\n", false);
                    break;

                default:
                    SendLine("504 Unrecognized authentication type.\r\n", false);
                    break;
            }
        }

        /// <summary>
        /// function MAIL
        /// </summary>
        /// <param name="argsText"></param>
        private void MAIL(string argsText)
        {
            /* RFC 2821 3.3
            NOTE:
                This command tells the SMTP-receiver that a new mail transaction is
                starting and to reset all its state tables and buffers, including any
                recipients or mail data.  The <reverse-path> portion of the first or
                only argument contains the source mailbox (between "<" and ">"
                brackets), which can be used to report errors (see section 4.2 for a
                discussion of error reporting).  If accepted, the SMTP server returns
                 a 250 OK reply.
				 
                MAIL FROM:<reverse-path> [SP <mail-parameters> ] <CRLF>
                reverse-path = "<" [ A-d-l ":" ] Mailbox ">"
                Mailbox = Local-part "@" Domain
				
                body-value ::= "7BIT" / "8BITMIME" / "BINARYMIME"
				
                Examples:
                    C: MAIL FROM:<ned@thor.innosoft.com>
                    C: MAIL FROM:<ned@thor.innosoft.com> SIZE=500000 BODY=8BITMIME
            */

            if (CmdValidator.MAY_HANDLE_MAIL == false)
            {
                if (CmdValidator.MAIL_FROM_OK)
                {
                    SendLine("503 Sender already specified\r\n", false);
                }
                else
                {
                    SendLine("503 Bad sequence of commands\r\n", false);
                }

                return;
            }

            //------ Parse parameters -------------------------------------------------------------------//
            string _reversePath = "";
            string _senderEmail = "";

            long _messageSize = 0;

            BodyType _bodyType = BodyType.x7_bit;
            var _isFromParam = false;

            //--- regex param parse strings
            string[] _exps = new string[3];
            _exps[0] = @"(?<param>FROM)[\s]{0,}:\s{0,}<?\s{0,}(?<value>[\w\@\.\-\*\+\=\#\/]*)\s{0,}>?(\s|$)";
            _exps[1] = @"(?<param>SIZE)[\s]{0,}=\s{0,}(?<value>[\w]*)(\s|$)";
            _exps[2] = @"(?<param>BODY)[\s]{0,}=\s{0,}(?<value>[\w]*)(\s|$)";

            Parameter[] _params = ParamParser.Paramparser_NameValue(argsText, _exps);
            foreach (Parameter _parameter in _params)
            {
                // Possible params:
                // FROM:
                // SIZE=
                // BODY=
                switch (_parameter.ParamName.ToUpper())
                {
                    //------ Required paramters -----//
                    case "FROM":
                        //		if(parameter.ParamValue.Length == 0){
                        //			SendData("501 Sender address isn't specified. Syntax:{MAIL FROM:<address> [SIZE=msgSize]}\r\n");
                        //			return;
                        //		}
                        //		else{
                        _reversePath = _parameter.ParamValue;
                        _isFromParam = true;
                        //		}
                        break;

                    //------ Optional parameters ---------------------//
                    case "SIZE":
                        if (_parameter.ParamValue.Length == 0)
                        {
                            SendLine("501 SIZE parameter value isn't specified. Syntax:{MAIL FROM:<address> [SIZE=msgSize] [BODY=8BITMIME]}\r\n", false);
                            return;
                        }
                        else
                        {
                            if (SmtpCore.IsNumber(_parameter.ParamValue))
                            {
                                _messageSize = Convert.ToInt64(_parameter.ParamValue);
                            }
                            else
                            {
                                SendLine("501 SIZE parameter value is invalid. Syntax:{MAIL FROM:<address> [SIZE=msgSize] [BODY=8BITMIME]}\r\n", false);
                            }
                        }
                        break;

                    case "BODY":
                        if (_parameter.ParamValue.Length == 0)
                        {
                            SendLine("501 BODY parameter value isn't specified. Syntax:{MAIL FROM:<address> [SIZE=msgSize] [BODY=8BITMIME]}\r\n", false);
                            return;
                        }
                        else
                        {
                            switch (_parameter.ParamValue.ToUpper())
                            {
                                case "7BIT":
                                    _bodyType = BodyType.x7_bit;
                                    break;
                                case "8BITMIME":
                                    _bodyType = BodyType.x8_bit;
                                    break;
                                case "BINARYMIME":
                                    _bodyType = BodyType.binary;
                                    break;
                                default:
                                    SendLine("501 BODY parameter value is invalid. Syntax:{MAIL FROM:<address> [BODY=(7BIT/8BITMIME)]}\r\n", false);
                                    return;
                            }
                        }
                        break;

                    default:
                        SendLine("501 Error in parameters. Syntax:{MAIL FROM:<address> [SIZE=msgSize] [BODY=8BITMIME]}\r\n", false);
                        return;
                }
            }

            // If required parameter 'FROM:' is missing
            if (_isFromParam == false)
            {
                SendLine("501 Required param FROM: is missing. Syntax:{MAIL FROM:<address> [SIZE=msgSize] [BODY=8BITMIME]}\r\n", false);
                return;
            }

            // Parse sender's email address
            _senderEmail = _reversePath;
            //---------------------------------------------------------------------------------------------//

            //--- Check message size
            if (Parent.MaxMessageSize > _messageSize)
            {
                // Check if sender is ok
                if (Parent.ValidatingMailFrom(SessionID, _senderEmail) == true)
                {
                    SendLine(String.Format("250 OK <{0}> Sender ok\r\n", _senderEmail), true);

                    // See note above
                    ResetState();

                    // Store reverse path
                    MailFrom = _reversePath;
                    CmdValidator.MAIL_FROM_OK = true;

                    //-- Store params
                    BodyType = _bodyType;
                }
                else
                {
                    SendLine("550 You are refused to send mail here\r\n", false);
                }
            }
            else
            {
                SendLine("552 Message exceeds allowed size\r\n", false);
            }
        }

        /// <summary>
        /// function RCPT
        /// </summary>
        /// <param name="argsText"></param>
        private void RCPT(string argsText)
        {
            /* RFC 2821 4.1.1.3 RCPT
            NOTE:
                This command is used to identify an individual recipient of the mail
                data; multiple recipients are specified by multiple use of this
                command.  The argument field contains a forward-path and may contain
                optional parameters.
				
                Relay hosts SHOULD strip or ignore source routes, and
                names MUST NOT be copied into the reverse-path.  
				
                Example:
                    RCPT TO:<@hosta.int,@jkl.org:userc@d.bar.org>

                    will normally be sent directly on to host d.bar.org with envelope
                    commands

                    RCPT TO:<userc@d.bar.org>
                    RCPT TO:<userc@d.bar.org> SIZE=40000
						
                RCPT TO:<forward-path> [ SP <rcpt-parameters> ] <CRLF>			
            */

            /* RFC 2821 3.3
                If a RCPT command appears without a previous MAIL command, 
                the server MUST return a 503 "Bad sequence of commands" response.
            */
            if (CmdValidator.MAY_HANDLE_RCPT == false)
            {
                SendLine("503 Bad sequence of commands\r\n", false);
                return;
            }

            // Check that recipient count isn't exceeded
            if (ForwardPath.Count > Parent.MaxRecipients)
            {
                SendLine("452 Too many recipients\r\n", false);
                return;
            }


            //------ Parse parameters -------------------------------------------------------------------//
            string _forwardPath = "";
            string _recipientEmail = "";

            long _messageSize = 0;

            var _isToParam = false;

            //--- regex param parse strings
            string[] _exps = new string[2];
            _exps[0] = @"(?<param>TO)[\s]{0,}:\s{0,}<?\s{0,}(?<value>[\w\@\.\-\*\+\=\#\/]*)\s{0,}>?(\s|$)";
            _exps[1] = @"(?<param>SIZE)[\s]{0,}=\s{0,}(?<value>[\w]*)(\s|$)";

            Parameter[] _params = ParamParser.Paramparser_NameValue(argsText, _exps);
            foreach (Parameter _parameter in _params)
            {
                // Possible params:
                // TO:
                // SIZE=				
                switch (_parameter.ParamName.ToUpper()) // paramInf[0] because of param syntax: pramName =/: value
                {
                    //------ Required paramters -----//
                    case "TO":
                        if (_parameter.ParamValue.Length == 0)
                        {
                            SendLine("501 Recipient address isn't specified. Syntax:{RCPT TO:<address> [SIZE=msgSize]}\r\n", false);
                            return;
                        }
                        else
                        {
                            _forwardPath = _parameter.ParamValue;
                            _isToParam = true;
                        }
                        break;

                    //------ Optional parameters ---------------------//
                    case "SIZE":
                        if (_parameter.ParamValue.Length == 0)
                        {
                            SendLine("501 Size parameter isn't specified. Syntax:{RCPT TO:<address> [SIZE=msgSize]}\r\n", false);
                            return;
                        }
                        else
                        {
                            if (SmtpCore.IsNumber(_parameter.ParamValue))
                            {
                                _messageSize = Convert.ToInt64(_parameter.ParamValue);
                            }
                            else
                            {
                                SendLine("501 SIZE parameter value is invalid. Syntax:{RCPT TO:<address> [SIZE=msgSize]}\r\n", false);
                            }
                        }
                        break;

                    default:
                        SendLine("501 Error in parameters. Syntax:{RCPT TO:<address> [SIZE=msgSize]}\r\n", false);
                        return;
                }
            }

            // If required parameter 'TO:' is missing
            if (_isToParam == false)
            {
                SendLine("501 Required param TO: is missing. Syntax:<RCPT TO:{address> [SIZE=msgSize]}\r\n", false);
                return;
            }

            // Parse recipient's email address
            _recipientEmail = _forwardPath;
            //---------------------------------------------------------------------------------------------//

            // Check message size
            if (Parent.MaxMessageSize > _messageSize)
            {
                // Check if email address is ok
                if (Parent.ValidatingMailTo(SessionID, _recipientEmail) == true)
                {
                    // Check if mailbox size isn't exceeded
                    if (Parent.ValidatingMailBoxSize(SessionID, _recipientEmail, _messageSize))
                    {
                        // Store reciptient
                        if (ForwardPath.Contains(_recipientEmail) == false)
                            ForwardPath.Add(_recipientEmail, _forwardPath);

                        SendLine(String.Format("250 OK <{0}> Recipient ok\r\n", _recipientEmail), true);
                        CmdValidator.RCPT_TO_OK = true;
                    }
                    else
                    {
                        SendLine("552 Mailbox size limit exceeded\r\n", false);
                    }
                }
                else
                {
                    SendLine(String.Format("550 <{0}> No such user here\r\n", _recipientEmail), true);
                }
            }
            else
            {
                SendLine("552 Message exceeds allowed size\r\n", false);
            }
        }

        /// <summary>
        /// function DATA
        /// </summary>
        /// <param name="argsText"></param>
        private void DATA(string argsText)
        {
            /* RFC 2821 4.1.1
            NOTE:
                Several commands (RSET, DATA, QUIT) are specified as not permitting
                parameters.  In the absence of specific extensions offered by the
                server and accepted by the client, clients MUST NOT send such
                parameters and servers SHOULD reject commands containing them as
                having invalid syntax.
            */

            if (argsText.Length > 0)
            {
                SendLine("500 Syntax error. Syntax:{DATA}\r\n", false);
                return;
            }


            /* RFC 2821 4.1.1.4 DATA
            NOTE:
                If accepted, the SMTP server returns a 354 Intermediate reply and
                considers all succeeding lines up to but not including the end of
                mail data indicator to be the message text.  When the end of text is
                successfully received and stored the SMTP-receiver sends a 250 OK
                reply.
				
                The mail data is terminated by a line containing only a period, that
                is, the character sequence "<CRLF>.<CRLF>" (see section 4.5.2).  This
                is the end of mail data indication.
					
				
                When the SMTP server accepts a message either for relaying or for
                final delivery, it inserts a trace record (also referred to
                interchangeably as a "time stamp line" or "Received" line) at the top
                of the mail data.  This trace record indicates the identity of the
                host that sent the message, the identity of the host that received
                the message (and is inserting this time stamp), and the date and time
                the message was received.  Relayed messages will have multiple time
                stamp lines.  Details for formation of these lines, including their
                syntax, is specified in section 4.4.
   
            */


            /* RFC 2821 DATA
            NOTE:
                If there was no MAIL, or no RCPT, command, or all such commands
                were rejected, the server MAY return a "command out of sequence"
                (503) or "no valid recipients" (554) reply in response to the DATA
                command.
            */
            if (CmdValidator.MAY_HANDLE_DATA == false)
            {
                SendLine("503 Bad sequence of commands\r\n", false);
                return;
            }

            if (ForwardPath.Count == 0)
            {
                SendLine("554 no valid recipients given\r\n", false);
                return;
            }

            // reply: 354 Start mail input
            SendLine("354 Start mail input; end with <CRLF>.<CRLF>\r\n", false);

            //---- Construct server headers -------------------------------------------------------------------//
            byte[] _headers = null;
            string _header
                = "Received: from " + ConnectedHostName + " (" + ConnectedIP + ")\r\n"
                + "\tby " + MyHostName + " with SMTP; " + DateTime.Now.ToUniversalTime().ToString("r", System.Globalization.DateTimeFormatInfo.InvariantInfo) + "\r\n";

            _headers = Encoding.UTF8.GetBytes(_header.ToCharArray());
            //-------------------------------------------------------------------------------------------------//

            MemoryStream _replyStream = null;

            ReadReplyCode _replyCode = SmtpCore.ReadData(ClientSocket, out _replyStream, _headers, Parent.MaxMessageSize, Parent.CommandIdleTimeOut, "\r\n.\r\n", ".\r\n");
            if (_replyCode == ReadReplyCode.Ok)
            {
                long _recivedCount = _replyStream.Length;

                //------- Do period handling and raise store event  --------//
                // If line starts with '.', mail client adds additional '.',
                // remove them.
                using (MemoryStream _ms = SmtpCore.DoPeriodHandling(_replyStream, false))
                {
                    _replyStream.Close();

                    // Raise NewMail event
                    Parent.StoreMessage(MailFrom, MailTo, "inbox", _ms);

                    // Send ok - got message
                    SendLine("250 OK\r\n", false);
                }
                //----------------------------------------------------------//

                /* RFC 2821 4.1.1.4 DATA
                NOTE:
                    Receipt of the end of mail data indication requires the server to
                    process the stored mail transaction information.  This processing
                    consumes the information in the reverse-path buffer, the forward-path
                    buffer, and the mail data buffer, and on the completion of this
                    command these buffers are cleared.
                */
                ResetState();
            }
            else
            {
                if (_replyCode == ReadReplyCode.LengthExceeded)
                {
                    SendLine("552 Requested mail action aborted: exceeded storage allocation\r\n", false);
                }
                else
                {
                    SendLine("500 Error mail not terminated with '.'\r\n", false);
                }
            }
        }

        /// <summary>
        /// function BDAT
        /// </summary>
        /// <param name="argsText"></param>
        private void BDAT(string argsText)
        {
            /*RFC 3030 2
                The BDAT verb takes two arguments.  The
                first argument indicates the length, in octets, of the binary data
                chunk.  The second optional argument indicates that the data chunk
                is the last.
				
                The message data is sent immediately after the trailing <CR>
                <LF> of the BDAT command line.  Once the receiver-SMTP receives the
                specified number of octets, it will return a 250 reply code.

                The optional LAST parameter on the BDAT command indicates that this
                is the last chunk of message data to be sent.  The last BDAT command
                MAY have a byte-count of zero indicating there is no additional data
                to be sent.  Any BDAT command sent after the BDAT LAST is illegal and
                MUST be replied to with a 503 "Bad sequence of commands" reply code.
                The state resulting from this error is indeterminate.  A RSET command
                MUST be sent to clear the transaction before continuing.
				
                A 250 response MUST be sent to each successful BDAT data block within
                a mail transaction.

                bdat-cmd   ::= "BDAT" SP chunk-size [ SP end-marker ] CR LF
                chunk-size ::= 1*DIGIT
                end-marker ::= "LAST"
            */

            if (CmdValidator.MAY_HANDLE_BDAT == false)
            {
                SendLine("503 Bad sequence of commands\r\n", false);
                return;
            }

            string[] _params = argsText.Split(new char[] { ' ' });
            if (_params.Length > 0 && _params.Length < 3)
            {
                if (SmtpCore.IsNumber(_params[0]))
                {
                    long _chunkSize = Convert.ToInt64(_params[0]);

                    // Check if message size isn't exceeded
                    if (ChunkingErrors == false && ((MsgStream.Length + _chunkSize) > Parent.MaxMessageSize))
                    {
                        ChunkingErrors = true;
                        SendLine("552 Requested mail action aborted: exceeded storage allocation\r\n", false);
                    }

                    // Append chunck part to stream
                    ReadReplyCode _replyCode = SmtpCore.ReadData(ClientSocket, _chunkSize, MsgStream, !ChunkingErrors, Parent.CommandIdleTimeOut);
                    if (ChunkingErrors == false)
                    {
                        switch (_replyCode)
                        {
                            case ReadReplyCode.Ok:
                                SendLine(String.Format("250 {0} octets received\r\n", _chunkSize), false);
                                break;

                            case ReadReplyCode.TimeOut:
                                ChunkingErrors = true;
                                SendLine("500 TimeOut\r\n", false);
                                break;

                            case ReadReplyCode.UnKnownError:
                                ChunkingErrors = true;
                                SendLine("500 UnKnownError\r\n", false);
                                break;
                        }
                    }
                    else
                    {
                        SendLine("500 See previous chuncking errors\r\n", false);
                    }

                    // LAST specified
                    if (_params.Length == 2)
                    {
                        CmdValidator.BDAT_LAST_OK = true;
                        if (_replyCode == ReadReplyCode.Ok != ChunkingErrors)
                        {
                            // Raise NewMail event
                            Parent.StoreMessage(MailFrom, MailTo, "inbox", MsgStream);
                        }

                        // Close stream
                        MsgStream.Close();
                        MsgStream = null;

                        // 
                        ResetState();
                    }
                }
                else
                {
                    SendLine("500 Syntax error. Syntax:{BDAT chunk-size [LAST]}\r\n", false);
                }
            }
            else
            {
                SendLine("500 Syntax error. Syntax:{BDAT chunk-size [LAST]}\r\n", false);
            }

        }

        /// <summary>
        /// function RSET
        /// </summary>
        /// <param name="argsText"></param>
        private void RSET(string argsText)
        {
            /* RFC 2821 4.1.1
            NOTE:
                Several commands (RSET, DATA, QUIT) are specified as not permitting
                parameters.  In the absence of specific extensions offered by the
                server and accepted by the client, clients MUST NOT send such
                parameters and servers SHOULD reject commands containing them as
                having invalid syntax.
            */

            if (argsText.Length > 0)
            {
                SendLine("500 Syntax error. Syntax:{RSET}\r\n", false);
                return;
            }

            /* RFC 2821 4.1.1.5 RESET (RSET)
            NOTE:
                This command specifies that the current mail transaction will be
                aborted.  Any stored sender, recipients, and mail data MUST be
                discarded, and all buffers and state tables cleared.  The receiver
                MUST send a "250 OK" reply to a RSET command with no arguments.
            */

            ResetState();

            SendLine("250 OK\r\n", false);
        }

        /// <summary>
        /// function VRFY
        /// </summary>
        private void VRFY()
        {
            /* RFC 821 VRFY 
            Example:
                S: VRFY Lumi
                R: 250 Ivar Lumi <ivx@lumisoft.ee>
				
                S: VRFY lum
                R: 550 String does not match anything.			 
            */

            // ToDo: Parse user, add new event for cheking user

            //	SendData("250 OK\r\n");

            SendLine("502 Command not implemented\r\n", false);
        }

        /// <summary>
        /// function NOOP
        /// </summary>
        private void NOOP()
        {
            /* RFC 2821 4.1.1.9 NOOP (NOOP)
            NOTE:
                This command does not affect any parameters or previously entered
                commands.  It specifies no action other than that the receiver send
                an OK reply.
            */

            SendLine("250 OK\r\n", false);
        }

        /// <summary>
        /// function QUIT
        /// </summary>
        /// <param name="argsText"></param>
        private void QUIT(string argsText)
        {
            /* RFC 2821 4.1.1
            NOTE:
                Several commands (RSET, DATA, QUIT) are specified as not permitting
                parameters.  In the absence of specific extensions offered by the
                server and accepted by the client, clients MUST NOT send such
                parameters and servers SHOULD reject commands containing them as
                having invalid syntax.
            */

            if (argsText.Length > 0)
            {
                SendLine("500 Syntax error. Syntax:<QUIT>\r\n", false);
                return;
            }

            /* RFC 2821 4.1.1.10 QUIT (QUIT)
            NOTE:
                This command specifies that the receiver MUST send an OK reply, and
                then close the transmission channel.
            */

            // reply: 221 - Close transmission cannel
            SendLine("221 Service closing transmission channel\r\n", false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //---- Optional commands
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// function EXPN
        /// </summary>
        private void EXPN()
        {
            /* RFC 821 EXPN 
            NOTE:
                This command asks the receiver to confirm that the argument
                identifies a mailing list, and if so, to return the
                membership of that list.  The full name of the users (if
                known) and the fully specified mailboxes are returned in a
                multiline reply.
			
            Example:
                S: EXPN lsAll
                R: 250-ivar lumi <ivx@lumisoft.ee>
                R: 250-<willy@lumisoft.ee>
                R: 250 <kaido@lumisoft.ee>
            */

            //		SendData("250 OK\r\n");

            SendLine("502 Command not implemented\r\n", false);
        }

        /// <summary>
        /// function HELP
        /// </summary>
        private void HELP()
        {
            /* RFC 821 HELP
            NOTE:
                This command causes the receiver to send helpful information
                to the sender of the HELP command.  The command may take an
                argument (e.g., any command name) and return more specific
                information as a response.
            */

            //		SendData("250 OK\r\n");

            SendLine("502 Command not implemented\r\n", false);
        }

        /// <summary>
        /// function ResetState
        /// </summary>
        private void ResetState()
        {
            //--- Reset variables
            BodyType = BodyType.x7_bit;
            ForwardPath.Clear();

            MailFrom = "";
            //		Authenticated = false; // ??? must clear or not, no info.

            CmdValidator.RESET();
            CmdValidator.HELO_OK = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_message"></param>
        private void WriteLog(string p_message)
        {
            if (Parent.MailSniffing == true)
                ELogger.SNG.WriteLog("E", p_message);
        }

        /// <summary>
        /// Sends data to socket.
        /// </summary>
        /// <param name="data">String data wich to send.</param>
        private void SendLine(string p_sendLine, bool p_writeLog)
        {
            SmtpCore.SendData(ClientSocket, p_sendLine);

            if (p_writeLog == true)
                WriteLog(String.Format("session[{0}] >>> client[{1}]: {2}", SessionID, ConnectedIP, p_sendLine.Replace(Environment.NewLine, "<CRLF>")));
        }

        /// <summary>
        /// Reads data from socket.
        /// </summary>
        /// <returns></returns>
        private string ReadLine(bool p_writeLog)
        {
            string _result = SmtpCore.ReadLine(ClientSocket, 500, Parent.SessionIdleTimeOut);

            if (p_writeLog == true)
                WriteLog(String.Format("session[{0}] <<< client[{1}]: {2}", SessionID, ConnectedIP, _result + "<CRLF>"));

            return _result;
        }


        //-------------------------------------------------------------------------------------------------------------------------
        // function StreamReadLine
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Reads byte[] line from stream.
        /// </summary>
        /// <returns>Return null if end of stream reached.</returns>
        public byte[] StreamReadLine(Stream p_streamSource)
        {
            byte[] _result = null;

            ArrayList _lineBuffer = new ArrayList();
            byte _prevByte = 0;

            int _currByteLength = p_streamSource.ReadByte();
            while (_currByteLength > -1)
            {
                _lineBuffer.Add((byte)_currByteLength);

                // Line found
                if ((_prevByte == (byte)'\r' && (byte)_currByteLength == (byte)'\n'))
                {
                    _result = new byte[_lineBuffer.Count - 2];    // Remove <CRLF> 
                    _lineBuffer.CopyTo(0, _result, 0, _lineBuffer.Count - 2);

                    break;
                }

                // Store byte
                _prevByte = (byte)_currByteLength;

                // Read next byte
                _currByteLength = p_streamSource.ReadByte();
            }

            // Line isn't terminated with <CRLF> and has some chars left, return them.
            if (_lineBuffer.Count > 0 && _result == null)
            {
                _result = new byte[_lineBuffer.Count];
                _lineBuffer.CopyTo(0, _result, 0, _lineBuffer.Count);
            }

            return _result;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // Properties Implementation
        //-------------------------------------------------------------------------------------------------------------------------
        private MailListener m_smtpServer = null;

        /// <summary>
        /// Referance to SMTP server.
        /// </summary>
        private MailListener Parent
        {
            get
            {
                return m_smtpServer;
            }
        }

        private Hashtable m_forwardPath = null;

        /// <summary>
        /// Holds Mail to.
        /// </summary>
        private Hashtable ForwardPath
        {
            get
            {
                if (m_forwardPath == null)
                    m_forwardPath = new Hashtable();

                return m_forwardPath;
            }
        }

        private int m_badCmdCounter = 0;

        /// <summary>
        /// Holds number of bad commands.
        /// </summary>
        private int BadCmdCounter
        {
            get
            {
                return m_badCmdCounter;
            }

            set
            {
                m_badCmdCounter = value;
            }
        }

        private CommandValidator m_cmdValidator = null;

        /// <summary>
        /// 
        /// </summary>
        private CommandValidator CmdValidator
        {
            get
            {
                if (m_cmdValidator == null)
                    m_cmdValidator = new CommandValidator();

                return m_cmdValidator;
            }
        }

        private bool m_authenticated = false;

        /// <summary>
        /// Gets if session authenticated.
        /// </summary>
        public bool Authenticated
        {
            get
            {
                return m_authenticated;
            }

            set
            {
                m_authenticated = value;
            }
        }

        private string m_userName = "";

        /// <summary>
        /// Gets loggded in user name (session owner).
        /// </summary>
        public string UserName
        {
            get
            {
                return m_userName;
            }

            set
            {
                m_userName = value;
            }
        }

        /// <summary>
        /// Gets body type.
        /// </summary>
        public BodyType BodyType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets recipients.
        /// </summary>
        public string[] MailTo
        {
            get
            {
                string[] to = new string[ForwardPath.Count];
                ForwardPath.Values.CopyTo(to, 0);

                return to;
            }
        }

        private DateTime m_sessionStartTime;

        /// <summary>
        /// Gets session start time.
        /// </summary>
        public DateTime SessionStartTime
        {
            get
            {
                return m_sessionStartTime;
            }
        }

        private string m_sessionId = "";

        /// <summary>
        /// Gets session ID.
        /// </summary>
        public string SessionID
        {
            get
            {
                return m_sessionId;
            }
        }

        private string m_connectedIP = "";

        /// <summary>
        /// Get connected computer's IP.
        /// </summary>
        public string ConnectedIP
        {
            get
            {
                return m_connectedIP;
            }
        }

        private string m_connectedHostName = "";

        /// <summary>
        /// Get connected computer's name.
        /// </summary>
        public string ConnectedHostName
        {
            get
            {
                return m_connectedHostName;
            }
        }

        private string m_myHostName = "";

        /// <summary>
        /// Get connected computer's name.
        /// </summary>
        public string MyHostName
        {
            get
            {
                if (String.IsNullOrEmpty(m_myHostName) == true)
                    m_myHostName = SmtpCore.GetHostName();

                return m_myHostName;
            }
        }

        private Socket m_clientSocket;

        /// <summary>
        /// Get client socket.
        /// </summary>
        public Socket ClientSocket
        {
            get
            {
                return m_clientSocket;
            }
        }

        private string m_defaultDomain = "odinsoftware.co.kr";

        /// <summary>
        /// Get default mail domain.
        /// </summary>
        public string DefaultDomain
        {
            get
            {
                return m_defaultDomain;
            }
        }

        private string m_mailForm = "";

        /// <summary>
        /// Gets sender.
        /// </summary>
        public string MailFrom
        {
            get
            {
                return m_mailForm;
            }

            set
            {
                m_mailForm = value;
            }
        }

        private MemoryStream m_msgStream = null;

        /// <summary>
        /// Get message stream.
        /// </summary>
        private MemoryStream MsgStream
        {
            get
            {
                if (m_msgStream == null)
                    m_msgStream = new MemoryStream();

                return m_msgStream;
            }

            set
            {
                m_msgStream = value;
            }
        }


        private bool m_chunkingErrors = false;

        private bool ChunkingErrors
        {
            get
            {
                return m_chunkingErrors;
            }

            set
            {
                m_chunkingErrors = value;
            }
        }

        private NetCore m_netCore = null;
        private NetCore SmtpCore
        {
            get
            {
                if (m_netCore == null)
                    m_netCore = new NetCore();

                return m_netCore;
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
                if (m_smtpServer != null)
                {
                    m_smtpServer.Dispose();
                    m_smtpServer = null;
                }
                if (m_clientSocket != null)
                {
                    m_clientSocket.Dispose();
                    m_clientSocket = null;
                }
                if (m_msgStream != null)
                {
                    m_msgStream.Dispose();
                    m_msgStream = null;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~MailResponsor()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}