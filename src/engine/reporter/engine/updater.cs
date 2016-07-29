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
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using OpenETaxBill.Engine.Library;
using OdinSoft.SDK.eTaxBill.Security.Encrypt;
using OdinSoft.SDK.eTaxBill.Security.Mime;
using OdinSoft.SDK.eTaxBill.Security.Notice;
using OdinSoft.SDK.eTaxBill.Security.Signature;

namespace OpenETaxBill.Engine.Reporter
{
    public class Updater : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        public Updater(DataTable p_issuingTbl, DataTable p_responseTbl)
        {
            m_issuingTbl = p_issuingTbl;
            m_responseTbl = p_responseTbl;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private DataTable m_issuingTbl, m_responseTbl;

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenETaxBill.Channel.Interface.IReporter m_ireporter = null;
        private OpenETaxBill.Channel.Interface.IReporter IReporter
        {
            get
            {
                if (m_ireporter == null)
                    m_ireporter = new OpenETaxBill.Channel.Interface.IReporter();

                return m_ireporter;
            }
        }

        private OpenETaxBill.Engine.Library.UAppHelper m_appHelper = null;
        public OpenETaxBill.Engine.Library.UAppHelper UAppHelper
        {
            get
            {
                if (m_appHelper == null)
                    m_appHelper = new OpenETaxBill.Engine.Library.UAppHelper(IReporter.Manager);

                return m_appHelper;
            }
        }

        private OpenETaxBill.Engine.Library.UCertHelper m_certHelper = null;
        public OpenETaxBill.Engine.Library.UCertHelper UCertHelper
        {
            get
            {
                if (m_certHelper == null)
                    m_certHelper = new OpenETaxBill.Engine.Library.UCertHelper(IReporter.Manager);

                return m_certHelper;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private void DoUpdateTable(DataRowCollection p_workingRows, string p_isNTSReport, string p_submitId, DateTime p_reportTime)
        {
            lock (m_issuingTbl)
            {
                foreach (DataRow _dr in p_workingRows)
                {
                    DataRow _issuingRow = m_issuingTbl.NewRow();

                    // row.RowState Add상태를 제거하기 위해서 Temp자료를 넣는다.                    
                    _issuingRow["issueId"] = _dr["issueId"];
                    _issuingRow["isNTSSending"] = "X";
                    _issuingRow["isNTSReport"] = "X";
                    _issuingRow["submitId"] = "";
                    _issuingRow["ntsReportingDate"] = DateTime.MinValue;

                    m_issuingTbl.Rows.Add(_issuingRow);
                    _issuingRow.AcceptChanges(); // Unchanged row.

                    // Modified row.
                    _issuingRow["isNTSSending"] = "T";
                    _issuingRow["isNTSReport"] = p_isNTSReport;
                    _issuingRow["submitId"] = p_submitId;
                    _issuingRow["ntsReportingDate"] = p_reportTime;
                }
            }
        }

        private void DoSaveReportAck(XmlDocument p_xmldoc, DataRowCollection p_workingRows, DateTime p_reportTime)
        {
            XmlNamespaceManager _nsmgr = new XmlNamespaceManager(p_xmldoc.NameTable);
            _nsmgr.AddNamespace("etax", XSignature.SNG.SignNameCollections[""]);

            XPathExpression _xexpr = XPathExpression.Compile("//etax:TaxInvoiceResponse/etax:ResultDocument");
            _xexpr.SetContext(_nsmgr);

            DataRow _responseRow = m_responseTbl.NewRow();

            XPathNavigator _nav = p_xmldoc.CreateNavigator().SelectSingleNode(_xexpr);
            if (_nav.MoveToChild(XPathNodeType.Element) == true)
            {
                do
                {
                    string _name = _nav.Name;
                    {
                        if (_name == "RefSubmitID")
                            _name = "submitId";
                        else if (_name == "ReceiptID")
                            _name = "receiptId";
                        else if (_name == "TypeCode")
                            _name = "typeCode";
                        else if (_name == "ResponseDateTime")
                            _name = "responseTime";
                        else if (_name == "ProcessStatusCode")
                            _name = "processStatus";
                        else if (_name == "FailReasonStatusCode")
                            _name = "failReason";
                    }

                    if (_responseRow.Table.Columns.IndexOf(_name) >= 0)
                    {
                        string _value = _nav.Value;

                        if (_responseRow.Table.Columns[_name].DataType == typeof(DateTime))
                            _responseRow[_name] = DateTime.ParseExact(_value, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                        else
                            _responseRow[_name] = _value;
                    }
                }
                while (_nav.MoveToNext(XPathNodeType.Element));

                _responseRow["totalCount"] = p_workingRows.Count;
                _responseRow["successCount"] = 0;
                _responseRow["failCount"] = 0;

                m_responseTbl.Rows.Add(_responseRow);
            }
        }

        private MimeContent DoSendReport(DataRowCollection p_workingRows, DateTime p_reportTime)
        {
            //-------------------------------------------------------------------------------------------------------------------//
            // 암호화
            //-------------------------------------------------------------------------------------------------------------------//
            X509Certificate2 _ntsCert2 = UCertHelper.NtsPublicKey;

            ArrayList _invoices = new ArrayList();
            for (int i = 0; i < p_workingRows.Count; i++)
            {
                string _document = Convert.ToString(p_workingRows[i]["document"]);
                string _rvalue = Convert.ToString(p_workingRows[i]["rvalue"]);

                TaxInvoiceStruct _taxStruct = new TaxInvoiceStruct
                {
                    SignerRValue = Convert.FromBase64String(_rvalue),
                    TaxInvoice = Encoding.UTF8.GetBytes(_document)
                };
                _invoices.Add(_taxStruct);
            }

            byte[] _encrypted = CmsManager.SNG.GetContentInfo(_ntsCert2, _invoices);

            //-------------------------------------------------------------------------------------------------------------------//
            // SOAP Envelope
            //-------------------------------------------------------------------------------------------------------------------//
            Header _soapHeader = new Header();
            {
                _soapHeader.ToAddress = UAppHelper.TaxInvoiceSubmitUrl;
                _soapHeader.Action = Request.eTaxInvoiceSubmit;
                _soapHeader.Version = UAppHelper.eTaxVersion;

                _soapHeader.FromParty = new Party(UAppHelper.SenderBizNo, UAppHelper.SenderBizName);
                _soapHeader.ToParty = new Party(UAppHelper.ReceiverBizNo, UAppHelper.ReceiverBizName);

                _soapHeader.ReplyTo = UAppHelper.ReplyAddress;
                _soapHeader.OperationType = Request.OperationType_InvoiceSubmit;
                _soapHeader.MessageType = Request.MessageType_Request;

                _soapHeader.TimeStamp = p_reportTime;
                _soapHeader.MessageId = Packing.SNG.GetMessageId(_soapHeader.TimeStamp);
            }

            Body _soapBody = new Body();
            {
                _soapBody.SubmitID = Packing.SNG.GetSubmitID(_soapHeader.TimeStamp, UAppHelper.RegisterId);
                _soapBody.ReferenceID = Guid.NewGuid().ToString();

                _soapBody.TotalCount = p_workingRows.Count;
            }

            //-------------------------------------------------------------------------------------------------------------------//
            // SOAP Signature
            //-------------------------------------------------------------------------------------------------------------------//
            XmlDocument _signedXml = Packing.SNG.GetSignedSoapEnvelope(_encrypted, UCertHelper.AspSignCert.X509Cert2, _soapHeader, _soapBody);

            //-------------------------------------------------------------------------------------------------------------------//
            // Request
            //-------------------------------------------------------------------------------------------------------------------//
            return Request.SNG.TaxInvoiceSubmit(
                            Encoding.UTF8.GetBytes(_signedXml.OuterXml),
                            _encrypted,
                            _soapBody.ReferenceID,
                            UAppHelper.TaxInvoiceSubmitUrl
                        );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_workingRows"></param>
        public void ReporterCallback(Object p_workingRows)
        {
            var _reportTime = DateTime.Now;
            DataRowCollection _workingRows = (DataRowCollection)p_workingRows;

            string _isNTSReport = "F";
            string _submitId = "";

            try
            {
                MimeContent _mimeContent = DoSendReport(_workingRows, _reportTime);
                if (_mimeContent.StatusCode == 0)
                {
                    var _xmldoc = new XmlDocument();
                    _xmldoc.LoadXml(_mimeContent.Parts[1].GetContentAsString());

                    lock (m_responseTbl)
                        DoSaveReportAck(_xmldoc, _workingRows, _reportTime);

                    DataRow _responseRow = m_responseTbl.Rows[0];
                    _submitId = Convert.ToString(_responseRow["submitId"]);

                    string _typeCode = Convert.ToString(_responseRow["typeCode"]);
                    if (_typeCode == "01")
                        _isNTSReport = "T";
                }
                else
                {
                    ELogger.SNG.WriteLog(String.Format("break: {0}, number of error record(s)->{1}", _mimeContent.ErrorMessage, _workingRows.Count));
                }
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            finally
            {
                DoUpdateTable(_workingRows, _isNTSReport, _submitId, _reportTime);
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
                if (m_issuingTbl != null)
                {
                    m_issuingTbl.Dispose();
                    m_issuingTbl = null;
                }
                if (m_responseTbl != null)
                {
                    m_responseTbl.Dispose();
                    m_responseTbl = null;
                }
                if (m_ireporter != null)
                {
                    m_ireporter.Dispose();
                    m_ireporter = null;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~Updater()
        {
            Dispose(false);
        }
 
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}