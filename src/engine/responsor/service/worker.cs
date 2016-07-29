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
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Microsoft.Win32;
using OpenETaxBill.Channel.Library.Security.Mime;
using OpenETaxBill.Channel.Library.Security.Notice;

namespace OpenETaxBill.Engine.Responsor
{
    public class Worker : WebListener
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        public Worker()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_port_number"></param>
        public Worker(int p_port_number)
            : base(p_port_number)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_local_address"></param>
        /// <param name="p_port_number"></param>
        public Worker(string p_local_address, int p_port_number)
            : base(p_local_address, p_port_number)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_host_address"></param>
        /// <param name="p_port_number"></param>
        /// <param name="p_web_folder"></param>
        public Worker(string p_host_address, int p_port_number, string p_web_folder)
            : base(p_host_address, p_port_number)
        {
            m_web_folder = p_web_folder;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private string m_web_folder = null;
        public string WebFolder
        {
            get
            {
                if (m_web_folder == null)
                    m_web_folder = UAppHelper.WebFolder;

                return m_web_folder;
            }
            set
            {
                m_web_folder = value;
            }
        }

        private string m_defaultPage = null;
        public string DefaultPage
        {
            get
            {
                if (m_defaultPage == null)
                    m_defaultPage = UAppHelper.DefaultPage;

                return m_defaultPage;
            }
        }

        private ResponseEngine m_engine = null;
        public ResponseEngine REngine
        {
            get
            {
                if (m_engine == null)
                    m_engine = new ResponseEngine();

                return m_engine;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private static Queue m_syncQueue = null;
        private static Queue SyncQueue
        {
            get
            {
                if (m_syncQueue == null)
                    m_syncQueue = Queue.Synchronized(new Queue());

                return m_syncQueue;
            }
        }

        private static Thread QueueThread = null;

        private void Parser()
        {
            while (true)
            {
                lock (SyncQueue.SyncRoot)
                {
                    if (SyncQueue.Count > 0)
                    {
                        object _dequeue = SyncQueue.Dequeue();
                        if (_dequeue != null)
                        {
                            MimeContent _receiveMime = (MimeContent)_dequeue;

                            var _xmldoc = new XmlDocument();
                            _xmldoc.LoadXml(_receiveMime.Parts[1].GetContentAsString());

                            // 큐에서 메시지를 추출하여 DB 처리 함
                            REngine.ResultDataProcess(_xmldoc, DateTime.Now);
                        }
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }

                Thread.Sleep(100);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 국세청으로 부터 전자(세금)계산서 처리결과를 전송 받아서 처리 합니다.
        /// </summary>
        /// <param name="p_request">국세청이 보내준 메시지</param>
        /// <param name="p_response">국세청으로 회신 할 메시지</param>
        public override void OnResponse(ref HttpRequestStruct p_request, ref HttpResponseStruct p_response)
        {
            if (p_request.URL.ToLower() == UAppHelper.AcceptedRequestUrl)
            {
                try
                {
                    MimeContent _receiveMime = (new MimeParser()).DeserializeMimeContent(p_request.Headers["Content-Type"].ToString(), p_request.BodyData);

                    var _xmldoc = new XmlDocument();
                    _xmldoc.LoadXml(_receiveMime.Parts[0].GetContentAsString());

                    MimeContent _returnMime = REngine.ResultRcvAck(_xmldoc);
                    {
                        p_response.BodyData = _returnMime.GetContentAsBytes();

                        p_response.SoapAction = Request.eTaxResultRecvAck;
                        p_response.ContentType = _returnMime.ContentType;
                        p_response.ContentLength = p_response.BodyData.Length;

                        p_response.Headers.Add("SOAPAction", p_response.SoapAction);
                        p_response.Headers.Add("Content-Type", p_response.ContentType);
                        p_response.Headers.Add("Content-Length", p_response.ContentLength.ToString());

                        p_response.Status = (int)ResponseState.OK;
                    }

                    lock (SyncQueue.SyncRoot)
                    {
                        // 처리 할 메시지를 큐에 추가 함
                        SyncQueue.Enqueue(_receiveMime);

                        if (QueueThread == null || (QueueThread != null && QueueThread.IsAlive == false))
                        {
                            QueueThread = new Thread(Parser)
                            {
                                IsBackground = true
                            };
                            QueueThread.Start();
                        }
                    }
                }
                catch (Exception ex)
                {
                    IResponsor.WriteDebug(ex);

                    p_response.Status = (int)ResponseState.BAD_REQUEST;

                    string _bodyString
                        = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">\n"
                        + "<HTML><HEAD>\n"
                        + "<META http-equiv=Content-Type content=\"text/html; charset=UTF-8\">\n"
                        + "</HEAD>\n"
                        + "<BODY>" + ex.Message + "</BODY></HTML>\n";

                    p_response.BodyData = Encoding.UTF8.GetBytes(_bodyString);
                }
            }
            else
            {
                string _filepath = (String.Format(@"{0}\{1}", WebFolder, p_request.URL.Replace("/", @"\"))).Replace(@"\\", @"\");

                if (Directory.Exists(_filepath) == true)
                {
                    if (File.Exists(_filepath + DefaultPage) == true)
                    {
                        _filepath = Path.Combine(_filepath, DefaultPage);
                    }
                    else
                    {
                        string[] _folders = Directory.GetDirectories(_filepath);
                        string[] _files = Directory.GetFiles(_filepath);

                        string _bodyString
                            = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">\n"
                            + "<HTML><HEAD>\n"
                            + "<META http-equiv=Content-Type content=\"text/html; charset=UTF-8\">\n"
                            + "</HEAD>\n"
                            + "<BODY><p>Folder listing, to do not see this add a '" + DefaultPage + "' document\n<p>\n";

                        for (int i = 0; i < _folders.Length; i++)
                            _bodyString += String.Format("<br><a href = \"{0}{1}/\">[{1}]</a>\n", p_request.URL, Path.GetFileName(_folders[i]));

                        for (int i = 0; i < _files.Length; i++)
                            _bodyString += String.Format("<br><a href = \"{0}{1}\">{1}</a>\n", p_request.URL, Path.GetFileName(_files[i]));

                        _bodyString += "</BODY></HTML>\n";

                        p_response.BodyData = Encoding.UTF8.GetBytes(_bodyString);
                        return;
                    }
                }

                if (File.Exists(_filepath) == true)
                {
                    RegistryKey _regkey = Registry.ClassesRoot.OpenSubKey(Path.GetExtension(_filepath), false);

                    // Get the data from a specified item in the key.
                    string _type = (string)_regkey.GetValue("Content Type");

                    // Open the stream and read it back.
                    p_response.Content = File.Open(_filepath, FileMode.Open, FileAccess.Read);
                    if (String.IsNullOrEmpty(_type) == false)
                        p_response.Headers["Content-type"] = _type;
                }
                else
                {
                    p_response.Status = (int)ResponseState.NOT_FOUND;

                    string _bodyString
                        = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">\n"
                        + "<HTML><HEAD>\n"
                        + "<META http-equiv=Content-Type content=\"text/html; charset=UTF-8\">\n"
                        + "</HEAD>\n"
                        + "<BODY>File not found!!</BODY></HTML>\n";

                    p_response.BodyData = Encoding.UTF8.GetBytes(_bodyString);
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}