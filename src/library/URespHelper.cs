using System;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using OpenETaxBill.Channel.Library.Security.Issue;
using OpenETaxBill.Channel.Library.Security.Signature;
using OpenETaxBill.SDK.Data;
using OpenETaxBill.SDK.Queue;

namespace OpenETaxBill.Engine.Library
{
    /// <summary>
    /// 
    /// </summary>
    public class URespHelper : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_manager"></param>
        public URespHelper(QService p_manager)
        {
            m_qmaster = (QService)p_manager.Clone();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_manager"></param>
        /// <param name="p_isService"></param>
        public URespHelper(QService p_manager, bool p_isService)
            : this(p_manager)
        {
            QMaster.IsService = p_isService;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private QService m_qmaster = null;

        /// <summary>
        /// 
        /// </summary>
        public QService QMaster
        {
            get
            {
                return m_qmaster;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenETaxBill.Engine.Library.UAppHelper m_appHelper = null;
        public OpenETaxBill.Engine.Library.UAppHelper UAppHelper
        {
            get
            {
                if (m_appHelper == null)
                    m_appHelper = new OpenETaxBill.Engine.Library.UAppHelper(QMaster);

                return m_appHelper;
            }
        }

        private OpenETaxBill.SDK.Data.DeltaHelper m_dltaHelper = null;
        private OpenETaxBill.SDK.Data.DeltaHelper LDltaHelper
        {
            get
            {
                if (m_dltaHelper == null)
                    m_dltaHelper = new OpenETaxBill.SDK.Data.DeltaHelper();

                return m_dltaHelper;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private DataSet m_responseSet = null;
        private DataSet ResponseSet
        {
            get
            {
                if (m_responseSet == null)
                    m_responseSet = Schema.SNG.GetTaxSchema();

                return m_responseSet;
            }
        }

        private DataTable m_responseTbl = null;
        private DataTable ResponseTbl
        {
            get
            {
                if (m_responseTbl == null)
                    m_responseTbl = ResponseSet.Tables["TB_eTAX_RESPONSE"];

                return m_responseTbl;
            }
        }

        private DataTable m_resultTbl = null;
        private DataTable ResultTbl
        {
            get
            {
                if (m_resultTbl == null)
                    m_resultTbl = ResponseSet.Tables["TB_eTAX_RESULT"];

                return m_resultTbl;
            }
        }

        private DataTable m_issuingTbl = null;
        private DataTable IssuingTbl
        {
            get
            {
                if (m_issuingTbl == null)
                    m_issuingTbl = Schema.SNG.GetTaxModifiedDataTable
                        (
                            ResponseSet, "TB_eTAX_ISSUING",
                            new string[] { "issueId", "isNTSConfirm", "ntsConfirmDate", "isNTSSuccess" }
                        );

                return m_issuingTbl;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 국세청으로 부터 전달 된 메시지를 기반으로 ISSUING, RESPONSE, RESULT 테이블을 Update 합니다.
        /// </summary>
        /// <param name="p_xmldoc"></param>
        /// <param name="p_requestDate"></param>
        /// <param name="o_error"></param>
        /// <returns></returns>
        public bool DoSaveRequestAck(XmlDocument p_xmldoc, DateTime p_requestDate, out string o_error)
        {
            var _result = false;

            o_error = "";

            try
            {
                IssuingTbl.Clear();
                ResponseTbl.Clear();
                ResultTbl.Clear();

                XmlNamespaceManager _nsmgr = new XmlNamespaceManager(p_xmldoc.NameTable);
                _nsmgr.AddNamespace("etax", XSignature.SNG.SignNameCollections[""]);

                XPathExpression _xexpr = XPathExpression.Compile("//etax:TaxInvoiceResponse/etax:ResultDocument");
                _xexpr.SetContext(_nsmgr);

                DataRow _responseRow = ResponseTbl.NewRow();
                _responseRow["totalCount"] = 0;
                _responseRow["successCount"] = 0;
                _responseRow["failCount"] = 0;

                XPathNavigator _nav = p_xmldoc.CreateNavigator().SelectSingleNode(_xexpr);
                if (_nav.MoveToChild(XPathNodeType.Element) == true)
                {
                    do
                    {
                        if (_nav.Name == "ValidationDocument")
                        {
                            DataRow _resultRow = ResultTbl.NewRow();
                            {
                                XPathNodeIterator _iter = _nav.SelectChildren(XPathNodeType.Element);
                                while (_iter.MoveNext() == true)
                                {
                                    string _name = _iter.Current.Name;
                                    {
                                        if (_name == "IssueID")
                                            _name = "issueId";
                                        else if (_name == "ResultStatusCode")
                                            _name = "resultStatus";
                                    }

                                    if (_resultRow.Table.Columns.IndexOf(_name) >= 0)
                                    {
                                        string _value = _iter.Current.Value;

                                        if (_resultRow.Table.Columns[_name].DataType == typeof(DateTime))
                                            _resultRow[_name] = DateTime.ParseExact(_value, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                                        else
                                            _resultRow[_name] = _value;
                                    }
                                }

                                _resultRow["isDone"] = "F";
                                _resultRow["created"] = p_requestDate;

                                ResultTbl.Rows.Add(_resultRow);
                            }

                            var _issuingRow = IssuingTbl.NewRow();
                            {
                                // row.RowState Add상태를 제거하기 위해서 Temp자료를 넣는다.
                                _issuingRow["issueId"] = _resultRow["issueId"];
                                _issuingRow["isNTSConfirm"] = "X";
                                _issuingRow["isNTSSuccess"] = "X";
                                _issuingRow["ntsConfirmDate"] = DateTime.MinValue;

                                IssuingTbl.Rows.Add(_issuingRow);
                                _issuingRow.AcceptChanges();

                                _issuingRow["isNTSConfirm"] = "T";
                                _issuingRow["isNTSSuccess"] = "F";

                                string _status = Convert.ToString(_resultRow["resultStatus"]);
                                if (_status == "SUC001" || _status == "SYN003")
                                {
                                    _issuingRow["isNTSSuccess"] = "T";
                                    _resultRow["isDone"] = "T";
                                }

                                _issuingRow["ntsConfirmDate"] = p_requestDate;
                            }
                        }
                        else
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
                                else if (_name == "TotalCountQuantity")
                                    _name = "totalCount";
                                else if (_name == "SuccessCountQuantity")
                                    _name = "successCount";
                                else if (_name == "FailCountQuantity")
                                    _name = "failCount";
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
                    }
                    while (_nav.MoveToNext(XPathNodeType.Element));

                    ResponseTbl.Rows.Add(_responseRow);

                    LDltaHelper.InsertDeltaSet(UAppHelper.ConnectionString, ResponseSet);
                }

                o_error = String.Format("Update result deltaSet: {0}, {1} record(s)", ResponseTbl.Rows[0]["submitId"], ResultTbl.Rows.Count);
                _result = true;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    o_error = ex.InnerException.Message;
                else
                    o_error = ex.Message;
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
                if (m_qmaster != null)
                {
                    m_qmaster.Dispose();
                    m_qmaster = null;
                }
                if (m_appHelper != null)
                {
                    m_appHelper.Dispose();
                    m_appHelper = null;
                }
                if (m_responseSet != null)
                {
                    m_responseSet.Dispose();
                    m_responseSet = null;
                }
                if (m_responseTbl != null)
                {
                    m_responseTbl.Dispose();
                    m_responseTbl = null;
                }
                if (m_resultTbl != null)
                {
                    m_resultTbl.Dispose();
                    m_resultTbl = null;
                }
                if (m_issuingTbl != null)
                {
                    m_issuingTbl.Dispose();
                    m_issuingTbl = null;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~URespHelper()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}