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
using System.IO;
using System.Text;
using System.Xml;
using OdinSoft.SDK.eTaxBill.Security.Issue;
using OdinSoft.SDK.eTaxBill.Security.Mime;
using OdinSoft.SDK.eTaxBill.Security.Notice;
using OdinSoft.SDK.Configuration;

namespace OpenETaxBill.Engine.Responsor
{
    public class ResponseEngine : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenETaxBill.Channel.Interface.IResponsor m_iResponsor = null;
        private OpenETaxBill.Channel.Interface.IResponsor IResponsor
        {
            get
            {
                if (m_iResponsor == null)
                    m_iResponsor = new OpenETaxBill.Channel.Interface.IResponsor();

                return m_iResponsor;
            }
        }
        
        private OpenETaxBill.Engine.Library.UAppHelper m_appHelper = null;
        public OpenETaxBill.Engine.Library.UAppHelper UAppHelper
        {
            get
            {
                if (m_appHelper == null)
                    m_appHelper = new OpenETaxBill.Engine.Library.UAppHelper(IResponsor.Manager);

                return m_appHelper;
            }
        }

        private OpenETaxBill.Engine.Library.UCertHelper m_certHelper = null;
        public OpenETaxBill.Engine.Library.UCertHelper UCertHelper
        {
            get
            {
                if (m_certHelper == null)
                    m_certHelper = new OpenETaxBill.Engine.Library.UCertHelper(IResponsor.Manager);

                return m_certHelper;
            }
        }

        private OpenETaxBill.Engine.Library.URespHelper m_responsor = null;
        private OpenETaxBill.Engine.Library.URespHelper Responsor
        {
            get
            {
                if (m_responsor == null)
                    m_responsor = new OpenETaxBill.Engine.Library.URespHelper(IResponsor.Manager);

                return m_responsor;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        public bool LogCommands
        {
            get
            {
                return CfgHelper.SNG.DebugMode;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 국세청으로 부터 전달 받은 메시지를 DB에 UPDATE 합니다.
        /// </summary>
        /// <param name="p_xmldoc"></param>
        /// <param name="p_reponse_date"></param>
        public void ResultDataProcess(XmlDocument p_xmldoc, DateTime p_reponse_date)
        {
            IResponsor.WriteDebug(p_xmldoc.Name);

            string _message;

            bool _result = Responsor.DoSaveRequestAck(p_xmldoc, p_reponse_date, out _message);
            if (LogCommands == true || _result == false)
                ELogger.SNG.WriteLog("X", _message);

            if (_result == false)
            {
                var _directory = Path.Combine(UAppHelper.NTSFolder, p_reponse_date.ToString("yyyyMM"));

                if (Directory.Exists(_directory) == false)
                    Directory.CreateDirectory(_directory);

                var _savefile = Path.Combine(_directory, $"response_{p_reponse_date.ToString("yyyyMMddHHmmss")}.xml");
                File.WriteAllText(_savefile, p_xmldoc.OuterXml, Encoding.UTF8);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_xmldoc"></param>
        /// <returns></returns>
        public MimeContent ResultRcvAck(XmlDocument p_xmldoc)
        {
            IResponsor.WriteDebug(p_xmldoc.Name);

            var _resultAckTime = DateTime.Now;

            XmlNode _messageId = p_xmldoc.SelectSingleNode("descendant::wsa:MessageID", Packing.SNG.SoapNamespaces);
            if (_messageId == null)
                throw new ResponseException("not exist <wsa:MessageID>");

            XmlNode _resultId = p_xmldoc.SelectSingleNode("descendant::kec:ResultID", Packing.SNG.SoapNamespaces);
            if (_resultId == null)
                throw new ResponseException("not exist <kec:ResultID>");

            Header _soapHeader = new Header();
            {
                _soapHeader.ToAddress = UAppHelper.ResultRecvAckToAddress;
                _soapHeader.Action = Request.eTaxResultRecvAck;
                _soapHeader.Version = UAppHelper.eTaxVersion;

                _soapHeader.FromParty = new Party(UAppHelper.SenderBizNo, UAppHelper.SenderBizName);
                _soapHeader.ToParty = new Party(UAppHelper.ReceiverBizNo, UAppHelper.ReceiverBizName);

                _soapHeader.OperationType = Request.OperationType_ResultSubmit;
                _soapHeader.MessageType = Request.MessageType_Response;

                _soapHeader.TimeStamp = _resultAckTime;
                _soapHeader.MessageId = Packing.SNG.GetMessageId(_soapHeader.TimeStamp);

                _soapHeader.RelatesTo = _messageId.InnerText;
            }

            Body _soapBody = new Body();
            {
                _soapBody.ResultID = _resultId.InnerText;
            }

            //-------------------------------------------------------------------------------------------------------------------------
            // Signature
            //-------------------------------------------------------------------------------------------------------------------------
            XmlDocument _envelope = Packing.SNG.GetSignedSoapEnvelope(null, UCertHelper.AspSignCert.X509Cert2, _soapHeader, _soapBody);

            byte[] _soappart = Encoding.UTF8.GetBytes(_envelope.OuterXml);

            //-------------------------------------------------------------------------------------------------------------------------
            // MIME
            //-------------------------------------------------------------------------------------------------------------------------
            MimePart _mimeSoap = new MimePart()
            {
                ContentType = "text/xml",
                CharSet = "UTF-8",
                ContentId = String.Format("<{0}>", Convertor.SNG.GetRandHexString(32)),
                Content = _soappart
            };

            MimeContent _responseMime = new MimeContent()
            {
                Boundary = "----=_Part_"
                         + Convertor.SNG.GetRandNumString(5) + "_"
                         + Convertor.SNG.GetRandNumString(8) + "."
                         + Convertor.SNG.GetRandNumString(13)
            };

            _responseMime.Parts.Add(_mimeSoap);
            _responseMime.SetAsStartPart(_mimeSoap);

            return _responseMime;
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
                if (m_iResponsor != null)
                {
                    m_iResponsor.Dispose();
                    m_iResponsor = null;
                }
        }

        /// <summary>
        /// 
        /// </summary>
        ~ResponseEngine()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}