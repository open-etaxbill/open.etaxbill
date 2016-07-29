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
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;
using System.Text.RegularExpressions;

namespace OpenETaxBill.Engine.Responsor
{
    //-------------------------------------------------------------------------------------------------------------------------
    // 
    //-------------------------------------------------------------------------------------------------------------------------
    public enum ParsingState
    {
        METHOD,
        URL,
        URLPARM,
        URLVALUE,
        VERSION,
        HEADERKEY,
        HEADERVALUE,
        BODY,
        OK
    };

    public enum ResponseState
    {
        OK = 200,
        BAD_REQUEST = 400,
        NOT_FOUND = 404
    }

    //-------------------------------------------------------------------------------------------------------------------------
    // 
    //-------------------------------------------------------------------------------------------------------------------------
    public struct HttpRequestStruct
    {
        public string Method;
        public string URL;
        public string Version;
        
        public Hashtable Args;
        
        public bool Execute;
        public Hashtable Headers;
        
        public int BodySize;
        public byte[] BodyData;
    }

    public struct HttpResponseStruct
    {
        public int Status;

        public string StartId;
        public string SoapAction;
        public string ContentType;
        public string TransferEncoding;
        public int ContentLength;

        public string Version;
        
        public Hashtable Headers;
        
        public int BodySize;
        public byte[] BodyData;

        public Stream Content;
    }

    //-------------------------------------------------------------------------------------------------------------------------
    // 
    //-------------------------------------------------------------------------------------------------------------------------
    public class Requestor : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private byte[] ReadBuffer;
        private ParsingState ParserState;

        private HttpRequestStruct Request;
        private HttpResponseStruct Response;

        private readonly TcpClient m_client;
        private TcpClient Client
        {
            get
            {
                return m_client;
            }
        }

        private readonly WebListener m_parent = null;
        public WebListener Parent
        {
            get
            {
                return m_parent;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_client"></param>
        /// <param name="p_parent"></param>
        public Requestor(TcpClient p_client, WebListener p_parent)
        {
            m_client = p_client;
            m_parent = p_parent;

            Response.BodySize = 0;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private void WriteLog(string p_message)
        {
            string[] _lines = Regex.Split(p_message, "\n");
            if (_lines.Length > 0)
                Parent.WriteLog("E", _lines[0]);
        }

        /// <summary>
        /// 국세청으로 부터 처리결과를 수신하여, DATABASE 처리와 답신(response)을 작성 함
        /// </summary>
        public void Process()
        {
            NetworkStream _network = Client.GetStream();

			try
			{				
				WriteLog("Connection accepted. Buffer: " + Client.ReceiveBufferSize);

				ReadBuffer = new byte[Client.ReceiveBufferSize];

				string _completeMessage = "";

				string _value = "";
				string _key = "";

				int _noBytesRead = 0;
				int _noRetry = 0;

				// binary data buffer index
				int _dataNdx = 0;

				// Incoming message may be larger than the buffer size.
				while (true)
				{
					_noBytesRead = _network.Read(ReadBuffer, 0, ReadBuffer.Length);
					_completeMessage = String.Concat(_completeMessage, Encoding.UTF8.GetString(ReadBuffer, 0, _noBytesRead));

					// read buffer index
					int _readNdx = 0;

					do
					{
						switch (ParserState)
						{
							case ParsingState.METHOD:
								if (ReadBuffer[_readNdx] != ' ')
								{
									Request.Method += (char)ReadBuffer[_readNdx++];
								}
								else
								{
									_readNdx++;
									ParserState = ParsingState.URL;
								}
								break;

							case ParsingState.URL:
								if (ReadBuffer[_readNdx] == '?')
								{
									_readNdx++;
									_key = "";

									Request.Execute = true;
									Request.Args = new Hashtable();

									ParserState = ParsingState.URLPARM;
								}
								else if (ReadBuffer[_readNdx] != ' ')
								{
									Request.URL += (char)ReadBuffer[_readNdx++];
								}
								else
								{
									_readNdx++;

									Request.URL = HttpUtility.UrlDecode(Request.URL);
									ParserState = ParsingState.VERSION;
								}
								break;

							case ParsingState.URLPARM:
								if (ReadBuffer[_readNdx] == '=')
								{
									_readNdx++;
									_value = "";

									ParserState = ParsingState.URLVALUE;
								}
								else if (ReadBuffer[_readNdx] == ' ')
								{
									_readNdx++;
									Request.URL = HttpUtility.UrlDecode(Request.URL);

									ParserState = ParsingState.VERSION;
								}
								else
								{
									_key += (char)ReadBuffer[_readNdx++];
								}
								break;

							case ParsingState.URLVALUE:
								if (ReadBuffer[_readNdx] == '&')
								{
									_readNdx++;

									_key = HttpUtility.UrlDecode(_key);
									_value = HttpUtility.UrlDecode(_value);

									Request.Args[_key] = Request.Args[_key] != null ? String.Format("{0}, {1}", Request.Args[_key], _value) : _value;
									_key = "";

									ParserState = ParsingState.URLPARM;
								}
								else if (ReadBuffer[_readNdx] == ' ')
								{
									_readNdx++;

									_key = HttpUtility.UrlDecode(_key);
									_value = HttpUtility.UrlDecode(_value);

									Request.Args[_key] = Request.Args[_key] != null ? String.Format("{0}, {1}", Request.Args[_key], _value) : _value;
									Request.URL = HttpUtility.UrlDecode(Request.URL);

									ParserState = ParsingState.VERSION;
								}
								else
								{
									_value += (char)ReadBuffer[_readNdx++];
								}
								break;

							case ParsingState.VERSION:
								if (ReadBuffer[_readNdx] == '\r')
								{
									_readNdx++;
								}
								else if (ReadBuffer[_readNdx] != '\n')
								{
									Request.Version += (char)ReadBuffer[_readNdx++];
								}
								else
								{
									_readNdx++;

									_key = "";
									Request.Headers = new Hashtable();

									ParserState = ParsingState.HEADERKEY;
								}
								break;

							case ParsingState.HEADERKEY:
								if (ReadBuffer[_readNdx] == '\r')
								{
									_readNdx++;
								}
								else if (ReadBuffer[_readNdx] == '\n')
								{
									_readNdx++;

									if (Request.Headers["Content-Length"] != null)
									{
										Request.BodySize = Convert.ToInt32(Request.Headers["Content-Length"]);
										Request.BodyData = new byte[Request.BodySize];
										ParserState = ParsingState.BODY;
									}
									else
										ParserState = ParsingState.OK;

								}
								else if (ReadBuffer[_readNdx] == ':')
								{
									_readNdx++;
								}
								else if (ReadBuffer[_readNdx] != ' ')
								{
									_key += (char)ReadBuffer[_readNdx++];
								}
								else
								{
									_readNdx++;
									_value = "";

									ParserState = ParsingState.HEADERVALUE;
								}
								break;

							case ParsingState.HEADERVALUE:
								if (ReadBuffer[_readNdx] == '\r')
								{
									_readNdx++;
								}
								else if (ReadBuffer[_readNdx] != '\n')
								{
									_value += (char)ReadBuffer[_readNdx++];
								}
								else
								{
									_readNdx++;
									Request.Headers.Add(_key, _value);
									_key = "";

									ParserState = ParsingState.HEADERKEY;
								}
								break;

							case ParsingState.BODY:
								// Append to request BodyData
								Array.Copy(ReadBuffer, _readNdx, Request.BodyData, _dataNdx, _noBytesRead - _readNdx);
								_dataNdx += _noBytesRead - _readNdx;
								_readNdx = _noBytesRead;
								if (Request.BodySize <= _dataNdx)
								{
									ParserState = ParsingState.OK;
								}
								break;

							//default:
							//	ndx++;
							//	break;

						}
					}
					while (_readNdx < _noBytesRead);

					if (_network.DataAvailable == false)
					{
						_noRetry++;
						if (ParserState == ParsingState.OK || _noRetry > 16)
						{
							WriteLog(_completeMessage);
							break;
						}
					}
				}

				// Send headers	
				{
					Response.Version = "HTTP/1.1";

					if (ParserState != ParsingState.OK)
						Response.Status = (int)ResponseState.BAD_REQUEST;
					else
						Response.Status = (int)ResponseState.OK;

					Response.Headers = new Hashtable();

					Response.Headers.Add("Server", Parent.ServerName);
					Response.Headers.Add("Date", DateTime.Now.ToString("r"));

					if (Response.Status == (int)ResponseState.OK)
						Parent.OnResponse(ref Request, ref Response);                   // DATABASE 처리

					string _headersString = String.Format("{0} {1}\n", Response.Version, Parent.ResponseStatus[Response.Status]);

					foreach (DictionaryEntry _header in Response.Headers)
						_headersString += String.Format("{0}: {1}\n", _header.Key, _header.Value);

					_headersString += "\n";

					byte[] _headersBytes = Encoding.UTF8.GetBytes(_headersString);
					{
						_network.Write(_headersBytes, 0, _headersBytes.Length);
						WriteLog(_headersString);
					}
				}

				// Send body
				{
					if (Response.BodyData != null)
					{
						_network.Write(Response.BodyData, 0, Response.BodyData.Length);
						//WriteLog(Encoding.UTF8.GetString(Response.BodyData));
					}

					if (Response.Content != null)
					{
						using (Response.Content)
						{
							byte[] _buffer = new byte[Client.SendBufferSize];
							Response.Content.Seek(0, SeekOrigin.Begin);

							int _bytesRead;
							while ((_bytesRead = Response.Content.Read(_buffer, 0, _buffer.Length)) > 0)
								_network.Write(_buffer, 0, _bytesRead);

							//Response.Content.Seek(0, SeekOrigin.Begin);
							//WriteLog((new StreamReader(Response.Content)).ReadToEnd());
						}
					}
				}
			}
			catch (IOException ex)
			{
				Parent.WriteLog(ex.Message);    // exception 오류 처리하면 db에 로그를 남기게 되므로 메시지 처리함.
			}
			catch (Exception ex)
			{
				Parent.WriteLog(ex);
			}
            finally
            {
                _network.Close();
                Client.Close();

                if (Response.Content != null)
                    Response.Content.Close();

                Thread.CurrentThread.Abort();
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
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~Requestor()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}