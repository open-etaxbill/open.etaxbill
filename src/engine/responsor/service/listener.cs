using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using OpenETaxBill.SDK.Configuration;

namespace OpenETaxBill.Engine.Responsor
{
    public abstract class WebListener : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        public string ServerName = "OdinSoftTaxServer/1.0";

        private string m_host_name = null;
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

        private int m_portNumber = 80;
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

        public bool SoapFiltering
        {
            get
            {
                return UAppHelper.SoapFiltering;
            }
        }

        private Hashtable m_responseStatus = null;
        public Hashtable ResponseStatus
        {
            get
            {
                if (m_responseStatus == null)
                    m_responseStatus = new Hashtable();

                return m_responseStatus;
            }
        }

        public bool IsAlive
        {
            get
            {
                return ListenThread.IsAlive;
            }
        }

        private IPAddress m_localAddress = null;
        public IPAddress LocalAddress
        {
            get
            {
                if (m_localAddress == null)
                {
                    IPAddress[] _ipadrs = System.Net.Dns.GetHostEntry(HostName).AddressList;
                    if (_ipadrs.Length > 0)
                    {
                        foreach (IPAddress _ip in _ipadrs)
                        {
                            if (_ip.AddressFamily == AddressFamily.InterNetwork)
                            {
                                m_localAddress = _ip;
                                break;
                            }
                        }
                    }
                    else
                        m_localAddress = IPAddress.Parse("127.0.0.1");
                }

                return m_localAddress;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private TcpListener tcpListener;
        private System.Threading.Thread ListenThread;

        private OpenETaxBill.Channel.Interface.IResponsor m_iresponsor = null;
        protected OpenETaxBill.Channel.Interface.IResponsor IResponsor
        {
            get
            {
                if (m_iresponsor == null)
                    m_iresponsor = new OpenETaxBill.Channel.Interface.IResponsor();

                return m_iresponsor;
            }
        }

        private OpenETaxBill.Engine.Library.UAppHelper m_appHelper = null;
        protected OpenETaxBill.Engine.Library.UAppHelper UAppHelper
        {
            get
            {
                if (m_appHelper == null)
                    m_appHelper = new OpenETaxBill.Engine.Library.UAppHelper(IResponsor.Manager);

                return m_appHelper;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private void ResponseStatusInit()
        {
            ResponseStatus.Add(200, "200 OK");
            ResponseStatus.Add(201, "201 Created");
            ResponseStatus.Add(202, "202 Accepted");
            ResponseStatus.Add(204, "204 No Content");

            ResponseStatus.Add(301, "301 Moved Permanently");
            ResponseStatus.Add(302, "302 Redirection");
            ResponseStatus.Add(304, "304 Not Modified");

            ResponseStatus.Add(400, "400 Bad Request");
            ResponseStatus.Add(401, "401 Unauthorized");
            ResponseStatus.Add(403, "403 Forbidden");
            ResponseStatus.Add(404, "404 Not Found");

            ResponseStatus.Add(500, "500 Internal Server Error");
            ResponseStatus.Add(501, "501 Not Implemented");
            ResponseStatus.Add(502, "502 Bad Gateway");
            ResponseStatus.Add(503, "503 Service Unavailable");
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        public WebListener()
        {
            ResponseStatusInit();
        }

        public WebListener(int p_port_number)
            : this()
        {
            PortNumber = p_port_number;
        }
        
        public WebListener(string p_local_address, int p_port_number)
            : this(p_port_number)
        {
            HostName = p_local_address;
        }

        public void Listen()
        {
            try
            {
                ELogger.SNG.WriteLog(String.Format("starting for connection: {0}, {1}...", LocalAddress, PortNumber));

                tcpListener = new TcpListener(LocalAddress, PortNumber);
                tcpListener.Start();

                int _connection = 0;

                while (true)
                {
                    // 국세청으로 부터 통신이 시작 되기를 기다린다.
                    {
                        WriteLog("E", String.Format("waiting for connection: {0}...", ++_connection));

                        while (tcpListener.Pending() == false)
                            Thread.Sleep(1000);
                    }

                    // 국세청으로 부터 신호가 수신 되었다
                    {
                        WriteLog("E", String.Format("listener accept tcp client : {0}...", _connection));

                        Requestor _newRequest = new Requestor(tcpListener.AcceptTcpClient(), this);

                        // request를 thread로 처리 합니다.
                        Thread _requestor = new Thread(_newRequest.Process)
                        {
                            Name = "HTTP Requestor"
                        };

                        _requestor.Start();
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
                ELogger.SNG.WriteLog(ex);
            }
            catch (ResponseException ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            finally
            {
                tcpListener.Stop();
            }
        }

        public void Start()
        {
            ListenThread = new Thread(Listen)
            {
                Name = "HTTP Listener"
            };
            ListenThread.Start();
        }

        public void Stop()
        {
            tcpListener.Stop();
            ListenThread.Abort();
        }

        public abstract void OnResponse(ref HttpRequestStruct rq, ref HttpResponseStruct rp);

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
        /// <param name="p_exception"></param>
        /// <param name="p_message"></param>
        public void WriteLog(string p_exception, string p_message)
        {
            if (Environment.UserInteractive == true)
                IResponsor.WriteDebug(p_exception, p_message);
            else
            {
                if (p_exception != "E" || SoapFiltering == true)
                    ELogger.SNG.WriteLog(p_exception, p_message);
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
                if (m_iresponsor != null)
                {
                    m_iresponsor.Dispose();
                    m_iresponsor = null;
                }
        }

        /// <summary>
        /// 
        /// </summary>
        ~WebListener()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}