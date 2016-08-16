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
using System.Collections.Specialized;
using System.Data;
using System.IO;
using NpgsqlTypes;
using OdinSoft.SDK.Data.POSTGRESQL;
using OdinSoft.SDK.eTaxBill.Security.Issue;
using OdinSoft.SDK.eTaxBill.Security.Signature;

namespace OpenETaxBill.Engine.Signer
{
    public class Updater : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_doneEvent"></param>
        /// <param name="p_invoicerCert">매출자의 공인인증서</param>
        /// <param name="p_issuingTbl"></param>
        /// <param name="p_invoiceTbl"></param>
        public Updater(X509CertMgr p_invoicerCert, DataTable p_issuingTbl, DataTable p_invoiceTbl)
        {
            m_invoicerCert = p_invoicerCert;
            m_issuingTbl = p_issuingTbl;
            m_invoiceTbl = p_invoiceTbl;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private readonly static object SyncEngine = new object();

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private readonly X509CertMgr m_invoicerCert;
        private DataTable m_invoiceTbl, m_issuingTbl;

        private OpenETaxBill.Channel.Interface.ISigner m_iSigner = null;
        private OpenETaxBill.Channel.Interface.ISigner ISigner
        {
            get
            {
                if (m_iSigner == null)
                    m_iSigner = new OpenETaxBill.Channel.Interface.ISigner();

                return m_iSigner;
            }
        }

        private OpenETaxBill.Engine.Library.UAppHelper m_appHelper = null;
        public OpenETaxBill.Engine.Library.UAppHelper UAppHelper
        {
            get
            {
                if (m_appHelper == null)
                    m_appHelper = new OpenETaxBill.Engine.Library.UAppHelper(ISigner.Manager);

                return m_appHelper;
            }
        }

        private OdinSoft.SDK.Data.POSTGRESQL.PgDataHelper m_dataHelper = null;
        private OdinSoft.SDK.Data.POSTGRESQL.PgDataHelper LSQLHelper
        {
            get
            {
                if (m_dataHelper == null)
                    m_dataHelper = new OdinSoft.SDK.Data.POSTGRESQL.PgDataHelper();
                return m_dataHelper;
            }
        }

        private OdinSoft.SDK.Data.POSTGRESQL.PgDeltaHelper m_dltaHelper = null;
        private OdinSoft.SDK.Data.POSTGRESQL.PgDeltaHelper LDltaHelper
        {
            get
            {
                if (m_dltaHelper == null)
                    m_dltaHelper = new OdinSoft.SDK.Data.POSTGRESQL.PgDeltaHelper();

                return m_dltaHelper;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private bool CheckBroker(string p_issue_id, string p_customerId, string p_brokerId, DateTime p_signTime, DataTable p_resultTbl)
        {
            var _result = true;

            var _sqlstr
                    = "SELECT * FROM TB_eTAX_BROKER "
                    + " WHERE customerId=@customerId AND brokerId=@brokerId";

            var _dbps = new PgDatParameters();
            {
                _dbps.Add("@customerId", NpgsqlDbType.Varchar, p_customerId);
                _dbps.Add("@brokerId", NpgsqlDbType.Varchar, p_brokerId);
            }

            var _brokerSet = LSQLHelper.SelectDataSet(UAppHelper.ConnectionString, _sqlstr, _dbps);
            if (LSQLHelper.IsNullOrEmpty(_brokerSet) == true)
            {
                const string _resultStatus = "INV900";
                string _message = String.Format("broker({0}) is undefined by invoicer({1})", p_brokerId, p_customerId);

                string _filterExpression = String.Format("issueId='{0}' AND resultStatus='{1}'", p_issue_id, _resultStatus);
                DataRow[] _oldrow = p_resultTbl.Select(_filterExpression);

                if (_oldrow.Length == 0)
                {
                    DataRow _resultRow = p_resultTbl.NewRow();

                    _resultRow["issueId"] = p_issue_id;
                    _resultRow["resultStatus"] = _resultStatus;

                    _resultRow["isDone"] = "F";
                    _resultRow["message"] = _message;
                    _resultRow["created"] = p_signTime;

                    p_resultTbl.Rows.Add(_resultRow);
                }
                else
                {
                    DataRow _resultRow = _oldrow[0];

                    _resultRow["isDone"] = "F";
                    _resultRow["message"] = _message;
                    _resultRow["created"] = p_signTime;
                }

                _result = false;
            }

            return _result;
        }

        /// <summary>
        /// issueid에 해당하는 invoice테이블의 레코드를 읽어서 전자서명한 xml String을 리턴한다.
        /// 전자서명 도중 오류가 발생한 내역은 result 테이블에 기록 한다.
        /// </summary>
        /// <param name="p_issue_id">전자서명 할 invoice테이블의 issueId</param>
        /// <returns></returns>
        private string GetSignedXml(string p_issue_id, DateTime p_signTime, DataTable p_resultTbl)
        {
            var _result = "";

            var _sqlstr = "SELECT * FROM TB_eTAX_INVOICE WHERE issueId=@issueId";

            var _dbps = new PgDatParameters();
            {
                _dbps.Add("@issueId", NpgsqlDbType.Varchar, p_issue_id);
            }

            var _invoiceSet = LSQLHelper.SelectDataSet(UAppHelper.ConnectionString, _sqlstr, _dbps);
            if (LSQLHelper.IsNullOrEmpty(_invoiceSet) == false)
            {
                _sqlstr = "SELECT * FROM TB_eTAX_LINEITEM WHERE issueId=@issueId";
                var _lineitemSet = LSQLHelper.SelectDataSet(UAppHelper.ConnectionString, _sqlstr, _dbps);

                _invoiceSet.Merge(_lineitemSet);

                if (Validator.SNG.CheckInvoiceDataTable(_invoiceSet) <= 0)
                {
                    //-------------------------------------------------------------------------------------------------------------------//
                    // 세금계산서 작성
                    //-------------------------------------------------------------------------------------------------------------------//
                    Writer _etaxbill = new Writer(_invoiceSet);

                    //-------------------------------------------------------------------------------------------------------------------//
                    // 전자서명
                    //-------------------------------------------------------------------------------------------------------------------//
                    MemoryStream _signed = XSignature.SNG.GetSignedXmlStream(_etaxbill.TaxStream, m_invoicerCert.X509Cert2);
                    string _signedXml = (new StreamReader(_signed)).ReadToEnd();

                    string _validator = Validator.SNG.DoValidation(_signedXml);
                    if (String.IsNullOrEmpty(_validator) == false)
                    {
                        const string _resultStatus = "INV800";

                        string _filterExpression = String.Format("issueId='{0}' AND resultStatus='{1}'", p_issue_id, _resultStatus);
                        DataRow[] _oldrow = p_resultTbl.Select(_filterExpression);

                        if (_oldrow.Length == 0)
                        {
                            DataRow _resultRow = p_resultTbl.NewRow();

                            _resultRow["issueId"] = p_issue_id;
                            _resultRow["resultStatus"] = _resultStatus;

                            _resultRow["isDone"] = "F";
                            _resultRow["message"] = _validator;
                            _resultRow["created"] = p_signTime;

                            p_resultTbl.Rows.Add(_resultRow);
                        }
                        else
                        {
                            DataRow _resultRow = _oldrow[0];

                            _resultRow["isDone"] = "F";
                            _resultRow["message"] = _validator;
                            _resultRow["created"] = p_signTime;
                        }
                    }
                    else
                    {
                        // SUCCESS
                        _result = _signedXml;
                    }
                }
                else
                {
                    DataTable _invoiceTbl = _invoiceSet.Tables["TB_eTAX_INVOICE"];

                    foreach (DataRow _dr in _invoiceTbl.Rows)
                    {
                        NameValueCollection _nvCols = Validator.SNG.GetCollectionFromResult(_dr.RowError);

                        foreach (string _resultStatus in _nvCols.AllKeys)
                        {
                            string _filterExpression = String.Format("issueId='{0}' AND resultStatus='{1}'", p_issue_id, _resultStatus);
                            DataRow[] _oldrow = p_resultTbl.Select(_filterExpression);

                            if (_oldrow.Length == 0)
                            {
                                DataRow _resultRow = p_resultTbl.NewRow();

                                _resultRow["issueId"] = p_issue_id;
                                _resultRow["resultStatus"] = _resultStatus;

                                _resultRow["isDone"] = "F";
                                _resultRow["message"] = _nvCols[_resultStatus];
                                _resultRow["created"] = p_signTime;

                                p_resultTbl.Rows.Add(_resultRow);
                            }
                            else
                            {
                                DataRow _resultRow = _oldrow[0];

                                _resultRow["isDone"] = "F";
                                _resultRow["message"] = _nvCols[_resultStatus];
                                _resultRow["created"] = p_signTime;
                            }
                        }
                    }
                }
            }

            return _result;
        }

        /// <summary>
        /// invoice 테이블의 해당 issueid 레코드를 읽어서 전자서명하여 issuing 테이블에 추가 합니다.
        /// </summary>
        /// <param name="p_invoiceeRow">invoicerId, issueId, invoicerEMail, invoiceeEMail, brokerEMail, providerEMail</param>
        public void SignatureCallBack(Object p_invoiceeRow)
        {
            var _signTime = DateTime.Now;

            DataRow _invoiceeRow = (DataRow)p_invoiceeRow;
            DataRow _invoiceRow = null;

            string _isIssued = "F";
            string _isPassed = "F";
            string _issueid = Convert.ToString(_invoiceeRow["issueId"]);

            try
            {
                lock (m_invoiceTbl)
                {
                    _invoiceRow = m_invoiceTbl.NewRow();

                    _invoiceRow["issueId"] = _issueid;

                    _invoiceRow["isIssued"] = "X";          // update deltaset에서 original과 current를 비교하는 관계로 제3의 값을 할당 하여야 함.
                    _invoiceRow["isSuccess"] = "X";
                }

                while (true)
                {
                    string _signedXml = "";
                    {
                        var _sqlstr = "SELECT * FROM TB_eTAX_RESULT WHERE issueId=@issueId";

                        var _dbps = new PgDatParameters();
                        {
                            _dbps.Add("@issueId", NpgsqlDbType.Varchar, _issueid);
                        }

                        var _resultSet = LSQLHelper.SelectDataSet(UAppHelper.ConnectionString, _sqlstr, _dbps);

                        var _resultTbl = _resultSet.Tables[0];
                        foreach (DataRow _dr in _resultTbl.Rows)
                        {
                            if (Convert.ToString(_dr["isDone"]) != "T")
                                _dr["isDone"] = "T";
                        }

                        var _isSuccess = true;

                        string _typeCode = Convert.ToString(_invoiceeRow["typeCode"]).Substring(2, 2);
                        if (_typeCode == "03" || _typeCode == "05")
                        {
                            string _invoicerId = Convert.ToString(_invoiceeRow["invoicerId"]);
                            string _brokerId = Convert.ToString(_invoiceeRow["brokerId"]);

                            if (CheckBroker(_issueid, _invoicerId, _brokerId, _signTime, _resultTbl) == false)
                            {
                                ELogger.SNG.WriteLog(String.Format("while checking issuing({0}), broker({1}) is undefined by invoicer({2}).", _issueid, _brokerId, _invoicerId));
                                _isSuccess = false;
                            }
                        }

                        lock (SyncEngine)
                            _signedXml = GetSignedXml(_issueid, _signTime, _resultTbl);

                        if (String.IsNullOrEmpty(_signedXml) == true)
                        {
                            ELogger.SNG.WriteLog(String.Format("because signed-xml is empty, could not update issuing table -> '{0}'.", _issueid));
                            _isSuccess = false;
                        }

                        LDltaHelper.InsertDeltaSet(UAppHelper.ConnectionString, _resultSet);
                        if (_isSuccess == false)
                            break;
                    }

                    lock (m_issuingTbl)
                    {
                        var _issuingRow = m_issuingTbl.NewRow();

                        _issuingRow["issueId"] = _issueid;
                        //_issuingRow["typeCode"] = _invoiceeRow["typeCode"];

                        //_issuingRow["invoicerId"] = _invoiceeRow["invoicerId"];
                        //_issuingRow["invoiceeId"] = _invoiceeRow["invoiceeId"];
                        //_issuingRow["brokerId"] = _invoiceeRow["brokerId"];
                        _issuingRow["providerId"] = _invoiceeRow["providerId"];

                        _issuingRow["document"] = _signedXml;
                        _issuingRow["rvalue"] = Convert.ToBase64String(m_invoicerCert.RandomNumber);
                        _issuingRow["signingDate"] = _signTime;

                        //_issuingRow["invoicerEMail"] = _invoiceeRow["invoicerEMail"];
                        //_issuingRow["invoiceeEMail"] = _invoiceeRow["invoiceeEMail"];
                        //_issuingRow["brokerEMail"] = _invoiceeRow["brokerEMail"];
                        _issuingRow["providerEMail"] = _invoiceeRow["providerEMail"];

                        foreach (DataColumn _dc in m_issuingTbl.Columns)
                        {
                            if (_dc.AllowDBNull == false && _issuingRow[_dc] == DBNull.Value)
                            {
                                if (_dc.DataType == typeof(String))
                                {
                                    if (_dc.MaxLength == 1)
                                        _issuingRow[_dc] = "F";
                                }
                                else if (_dc.DataType == typeof(Decimal))
                                {
                                    _issuingRow[_dc] = 0;
                                }
                            }
                        }

                        m_issuingTbl.Rows.Add(_issuingRow);
                    }

                    _isPassed = "T";
                    break;
                }

                _isIssued = "T";
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            finally
            {
                lock (m_invoiceTbl)
                {
                    m_invoiceTbl.Rows.Add(_invoiceRow);
                    _invoiceRow.AcceptChanges();

                    _invoiceRow["isIssued"] = _isIssued;
                    _invoiceRow["isSuccess"] = _isPassed;
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
                if (m_invoiceTbl != null)
                {
                    m_invoiceTbl.Dispose();
                    m_invoiceTbl = null;
                }
                if (m_issuingTbl != null)
                {
                    m_issuingTbl.Dispose();
                    m_issuingTbl = null;
                }
                if (m_iSigner != null)
                {
                    m_iSigner.Dispose();
                    m_iSigner = null;
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