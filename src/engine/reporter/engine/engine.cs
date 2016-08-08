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
using System.Data;
using System.Text;
using System.Xml;
using NpgsqlTypes;
using OdinSoft.SDK.Configuration;
using OdinSoft.SDK.Data.POSTGRESQL;
using OdinSoft.SDK.eTaxBill.Security.Issue;
using OdinSoft.SDK.eTaxBill.Security.Mime;
using OdinSoft.SDK.eTaxBill.Security.Notice;
using OpenETaxBill.Channel.Interface;
using OpenETaxBill.Engine.Library;

namespace OpenETaxBill.Engine.Reporter
{
    public class Engine : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private readonly static object SyncEngine = new object();

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private OdinSoft.SDK.Data.POSTGRESQL.PgDataHelper m_dataHelper = null;
        private OdinSoft.SDK.Data.POSTGRESQL.PgDataHelper LDataHelper
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

        private OpenETaxBill.Engine.Library.URespHelper m_responsor = null;
        private OpenETaxBill.Engine.Library.URespHelper Responsor
        {
            get
            {
                if (m_responsor == null)
                    m_responsor = new OpenETaxBill.Engine.Library.URespHelper(IReporter.Manager);

                return m_responsor;
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

        private DataTable m_issuingTbl = null;
        private DataTable IssuingTbl
        {
            get
            {
                if (m_issuingTbl == null)
                    m_issuingTbl = Schema.SNG.GetTaxModifiedDataTable
                        (
                            ResponseSet, "TB_eTAX_ISSUING", 
                            new string[] { "issueId", "isNTSSending", "isNTSReport", "ntsReportingDate", "submitId" }
                        );

                return m_issuingTbl;
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
        private MimeContent DoSendRequest(string p_refSubmitID, DateTime p_requestDate)
        {
            //-------------------------------------------------------------------------------------------------------------------//
            // SOAP Envelope
            //-------------------------------------------------------------------------------------------------------------------//
            Header _soapHeader = new Header();
            {
                _soapHeader.ToAddress = UAppHelper.RequestResultsSubmitUrl;
                _soapHeader.Action = Request.eTaxRequestSubmit;
                _soapHeader.Version = UAppHelper.eTaxVersion;

                _soapHeader.FromParty = new Party(UAppHelper.SenderBizNo, UAppHelper.SenderBizName);
                _soapHeader.ToParty = new Party(UAppHelper.ReceiverBizNo, UAppHelper.ReceiverBizName);

                _soapHeader.OperationType = Request.OperationType_RequestSubmit;
                _soapHeader.MessageType = Request.MessageType_Request;

                _soapHeader.TimeStamp = p_requestDate;
                _soapHeader.MessageId = Packing.SNG.GetMessageId(_soapHeader.TimeStamp);
            }

            Body _soapBody = new Body();
            {
                _soapBody.RefSubmitID = p_refSubmitID;
            }

            //-------------------------------------------------------------------------------------------------------------------//
            // SOAP Signature
            //-------------------------------------------------------------------------------------------------------------------//
            XmlDocument _signedXml = Packing.SNG.GetSignedSoapEnvelope(null, UCertHelper.AspSignCert.X509Cert2, _soapHeader, _soapBody);

            //-------------------------------------------------------------------------------------------------------------------//
            // Request
            //-------------------------------------------------------------------------------------------------------------------//
            return Request.SNG.TaxRequestSubmit(
                            Encoding.UTF8.GetBytes(_signedXml.OuterXml),
                            UAppHelper.RequestResultsSubmitUrl
                        );
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private struct ReportArgs
        {
            public string invoicerId;
            public int noIssuing;
            public int noReporting;
            public string where;
            public PgDatParameters dbps;
        }

        private int CheckReEnter(string p_invoicerId, string p_where, PgDatParameters p_dbps)
        {
            int _result = -1;

            // 동일한 조건으로 실행 중인 record가 1개 이상 있는지 확인 한다.
            var _sqlstr
                = "SELECT COUNT(a.isNTSSending) as norec "
                + "  FROM TB_eTAX_ISSUING a INNER JOIN TB_eTAX_INVOICE b "
                + "    ON a.issueId=b.issueId "
                + " WHERE a.isNTSSending=@isNTSSendingX "
                + "   AND ( "
                + "         (RIGHT(b.typeCode, 2) IN ('01', '02', '04') AND b.invoicerId=@invoicerId) "
                + "         OR "
                + "         (RIGHT(b.typeCode, 2) IN ('03', '05') AND b.brokerId=@invoicerId) "
                + "       ) "
                + p_where;

            p_dbps.Add("@isNTSSendingX", NpgsqlDbType.Varchar, "X");
            p_dbps.Add("@invoicerId", NpgsqlDbType.Varchar, p_invoicerId);

            var _ds = LDataHelper.SelectDataSet(UAppHelper.ConnectionString, _sqlstr, p_dbps);

            int _norec = Convert.ToInt32(_ds.Tables[0].Rows[0]["norec"]);
            if (_norec < 1)
            {
                // 국세청으로 전송 되지 않은 레코드를 선택한다.
                string _updstr
                        = "UPDATE TB_eTAX_ISSUING "
                        + "   SET isNTSSending=@isNTSSendingX "
                        + "  FROM TB_eTAX_ISSUING a INNER JOIN TB_eTAX_INVOICE b "
                        + "    ON a.issueId=b.issueId "
                        + " WHERE a.isNTSReport != @isNTSReport "
                        + "   AND ( "
                        + "         (RIGHT(b.typeCode, 2) IN ('01', '02', '04') AND b.invoicerId=@invoicerId) "
                        + "         OR "
                        + "         (RIGHT(b.typeCode, 2) IN ('03', '05') AND b.brokerId=@invoicerId) "
                        + "       ) "
                        + p_where;

                p_dbps.Add("@isNTSSendingX", NpgsqlDbType.Varchar, "X");
                p_dbps.Add("@isNTSReport", NpgsqlDbType.Varchar, "T");
                p_dbps.Add("@invoicerId", NpgsqlDbType.Varchar, p_invoicerId);

                _result = LDataHelper.ExecuteText(UAppHelper.ConnectionString, _updstr, p_dbps);
            }
            else
            {
                if (LogCommands == true)
                    ELogger.SNG.WriteLog(String.Format("re-enter: invoicerId->'{0}', working-row(s)->{1}", p_invoicerId, _norec));
            }

            return _result;
        }

        private int CheckReporting(string p_invoicerId, int p_noIssuing, string p_where, PgDatParameters p_dbps)
        {
            int _noReporting = 0;

            lock (SyncEngine)
                _noReporting = CheckReEnter(p_invoicerId, p_where, p_dbps);

            if (_noReporting > 0)
            {
                ReportArgs _args = new ReportArgs()
                {
                    invoicerId = p_invoicerId,
                    noIssuing = p_noIssuing,
                    noReporting = _noReporting,
                    where = p_where,
                    dbps = p_dbps
                };

                // Do not use using statement
                ThreadPoolWait _doneEvent = new ThreadPoolWait();
				_doneEvent.QueueUserWorkItem(DoReporting, _args);

                if (Environment.UserInteractive == true)
                    _doneEvent.WaitOne();
            }

            return _noReporting;
        }

        private void DoReporting(object p_args)
        {
            ReportArgs _args = (ReportArgs)p_args;
            _args.noIssuing = _args.noReporting;
            _args.noReporting = 0;

            try
            {
                int _toprow = 100;

                int _chunkCount = _args.noIssuing / _toprow + 1;
                if (_chunkCount > UAppHelper.NoThreadOfReporter)
                    _chunkCount = UAppHelper.NoThreadOfReporter;

                string _issueid = "";

                var _sqlstr
                        = "SELECT a.issueId, a.document, a.rvalue "
                        + "  FROM TB_eTAX_ISSUING a INNER JOIN TB_eTAX_INVOICE b "
                        + "    ON a.issueId=b.issueId "
                        + " WHERE a.isNTSSending=@isNTSSendingX "
                        + "   AND ( "
                        + "         (RIGHT(b.typeCode, 2) IN ('01', '02', '04') AND b.invoicerId=@invoicerId) "
                        + "         OR "
                        + "         (RIGHT(b.typeCode, 2) IN ('03', '05') AND b.brokerId=@invoicerId) "
                        + "       ) "
                        + "   AND a.issueId > @issueId "
                        + _args.where
                        + " ORDER BY a.issueId"
                        + " LIMIT " + _toprow;
                {
                    _args.dbps.Add("@isNTSSendingX", NpgsqlDbType.Varchar, "X");
                    _args.dbps.Add("@invoicerId", NpgsqlDbType.Varchar, _args.invoicerId);
                }

                //if (LogCommands == true)
                //    ELogger.SNG.WriteLog(String.Format("begin: invoicerId->'{0}', noIssuing->{1}", _args.invoicerId, _args.noIssuing));

                // 만약 InsertDeltaSet을 처리하는 중에 오류가 발생하면 무한 loop를 발생 하게 되므로,
                // 'X'로 marking한 레코드의 총 갯수를 감소하여 '0'보다 큰 경우에만 반복한다.
                while (_args.noIssuing > 0)
                {
                    int _rowsCount = 0;

                    IssuingTbl.Clear();
                    ResponseTbl.Clear();

                    var _doneEvents = new ThreadPoolWait[_chunkCount];
                    for (int i = 0; i < _chunkCount; i++)
                    {
                        _args.dbps.Add("@issueId", NpgsqlDbType.Varchar, _issueid);       // 100건 까지를 한 묶음으로 전송하기 위해 기준이 되는 승인번호

                        var _workingSet = LDataHelper.SelectDataSet(UAppHelper.ConnectionString, _sqlstr, _args.dbps);
                        if (LDataHelper.IsNullOrEmpty(_workingSet) == true)
                            break;

                        var _rows = _workingSet.Tables[0].Rows;
                        _issueid = Convert.ToString(_rows[_rows.Count - 1]["issueId"]); // 다음 100건의 기준 (>) 승인번호

                        _doneEvents[i] = new ThreadPoolWait();

                        Updater _worker = new Updater(IssuingTbl, ResponseTbl);
                        _doneEvents[i].QueueUserWorkItem(_worker.ReporterCallback, _rows);

                        if (Environment.UserInteractive == true)
                            _doneEvents[i].WaitOne();

                        _rowsCount += _rows.Count;
                    }

                    ThreadPoolWait.WaitForAll(_doneEvents);

                    // 처리된 레코드가 한개 이하 인 경우는 종료한다. (문제가 있는 경우로 보여 짐)
                    if (_rowsCount < 1)
                        break;

                    //if (LogCommands == true)
                    //    ELogger.SNG.WriteLog(String.Format("loop: invoicerId->'{0}', noIssuing->{1}, noReporting->{2}", _args.invoicerId, _args.noIssuing, _rowsCount));

                    _args.noIssuing -= _rowsCount;
                    _args.noReporting += IssuingTbl.Rows.Count;

                    LDltaHelper.InsertDeltaSet(UAppHelper.ConnectionString, ResponseSet);
                }
            }
            catch (ReporterException ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            finally
            {
                if (LogCommands == true)
                    ELogger.SNG.WriteLog(String.Format("end: invoicerId->'{0}', noIssuing->{1}, noReporting->{2}", _args.invoicerId, _args.noIssuing, _args.noReporting));

                int _noClearing = ClearXFlag(_args.invoicerId);
                if (_noClearing > 0)
                {
                    if (LogCommands == true)
                        ELogger.SNG.WriteLog(String.Format("clearX: invoicerId->'{0}', noClear->{1}", _args.invoicerId, _noClearing));
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_invoicerId"></param>
        /// <returns></returns>
        public DataSet GetCustomerSet(string p_invoicerId)
        {
            IReporter.WriteDebug(p_invoicerId);

            var _sqlstr
                    = "SELECT reportingType, reportFromDay, reportTillDay "
                    + "  FROM TB_eTAX_CUSTOMER "
                    + " WHERE customerId=@customerId";

            var _dbps = new PgDatParameters();
            {
                _dbps.Add("@customerId", NpgsqlDbType.Varchar, p_invoicerId);
            }

            var _customer_set = LDataHelper.SelectDataSet(UAppHelper.ConnectionString, _sqlstr, _dbps);
            if (LDataHelper.IsNullOrEmpty(_customer_set) == true)
                throw new ReporterException(String.Format("not exist customer: invoicerId->'{0}'", p_invoicerId));

            return _customer_set;
        }

        //public int DoReportInvoicer(string p_invoicerId, int p_noIssuing)
        //{
        //    IReporter.WriteDebug(p_invoicerId);
        //
        //    string _where = "";
        //    var _dbps = new PgDatParameters();

        //    return CheckReporting(p_invoicerId, p_noIssuing, _where, _dbps);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_invoicerId"></param>
        /// <param name="p_issueIds"></param>
        /// <returns></returns>
        public int DoReportInvoicer(string p_invoicerId, string[] p_issueIds)
        {
            IReporter.WriteDebug(p_invoicerId);

            int _result = 0;

            string _issueCols = "";
            foreach (string _issueId in p_issueIds)
            {
                if (String.IsNullOrEmpty(_issueCols) == false)
                    _issueCols += ", ";

                _issueCols += String.Format("'{0}'", _issueId);
            }

            if (String.IsNullOrEmpty(_issueCols) == false)
            {
                string _where = String.Format(" AND a.issueId IN ({0})", _issueCols);
                var _dbps = new PgDatParameters();

                _result = CheckReporting(p_invoicerId, p_issueIds.Length, _where, _dbps);
            }

            return _result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_invoicerId"></param>
        /// <param name="p_noInvoicee"></param>
        /// <param name="p_fromDay"></param>
        /// <param name="p_tillDay"></param>
        /// <returns></returns>
        public int DoReportInvoicer(string p_invoicerId, int p_noInvoicee, DateTime p_fromDay, DateTime p_tillDay)
        {
            IReporter.WriteDebug(p_invoicerId);

            var _where = " AND b.issueDate>=@fromDay AND b.issueDate<@tillDay ";

            var _dbps = new PgDatParameters();
            {
                _dbps.Add("@fromDay", NpgsqlDbType.TimestampTZ, p_fromDay);
                _dbps.Add("@tillDay", NpgsqlDbType.TimestampTZ, p_tillDay);
            }

            return CheckReporting(p_invoicerId, p_noInvoicee, _where, _dbps);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_refSubmitID"></param>
        /// <returns></returns>
        public bool DoRequestSubmit(string p_refSubmitID)
        {
            IReporter.WriteDebug(p_refSubmitID);

            var _result = true;

            var _requestDate = DateTime.Now;
            {
                MimeContent _mimeContent = DoSendRequest(p_refSubmitID, _requestDate);
                if (_mimeContent.StatusCode == 0)
                {
                    var _xmldoc = new XmlDocument();
                    _xmldoc.LoadXml(_mimeContent.Parts[1].GetContentAsString());

                    string _message;

                    _result = Responsor.DoSaveRequestAck(_xmldoc, _requestDate, out _message);
                    if (LogCommands == true || _result == false)
                        ELogger.SNG.WriteLog("X", _message);
                }
            }

            return _result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_invoicerId"></param>
        /// <returns></returns>
        public int ClearXFlag(string p_invoicerId)
        {
            IReporter.WriteDebug(p_invoicerId);

            var _sqlstr
                    = "UPDATE TB_eTAX_ISSUING "
                    + "   SET isNTSSending=@isNTSSending, isNTSReport=@isNTSReport "
                    + "  FROM TB_eTAX_ISSUING a INNER JOIN TB_eTAX_INVOICE b "
                    + "    ON a.issueId=b.issueId "
                    + " WHERE a.isNTSSending=@isNTSSendingX "
                    + "   AND ( "
                    + "         (RIGHT(b.typeCode, 2) IN ('01', '02', '04') AND b.invoicerId=@invoicerId) "
                    + "         OR "
                    + "         (RIGHT(b.typeCode, 2) IN ('03', '05') AND b.brokerId=@invoicerId) "
                    + "       ) ";

            var _dbps = new PgDatParameters();
            {
                _dbps.Add("@isNTSSending", NpgsqlDbType.Varchar, "F");
                _dbps.Add("@isNTSReport", NpgsqlDbType.Varchar, "F");
                _dbps.Add("@isNTSSendingX", NpgsqlDbType.Varchar, "X");
                _dbps.Add("@invoicerId", NpgsqlDbType.Varchar, p_invoicerId);
            }

            return LDataHelper.ExecuteText(UAppHelper.ConnectionString, _sqlstr, _dbps);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int ClearXFlag()
        {
            IReporter.WriteDebug("*");

            var _sqlstr
                    = "UPDATE TB_eTAX_ISSUING "
                    + "   SET isNTSSending=@isNTSSending, isNTSReport=@isNTSReport "
                    + " WHERE isNTSSending=@isNTSSendingX";

            var _dbps = new PgDatParameters();
            {
                _dbps.Add("@isNTSSending", NpgsqlDbType.Varchar, "F");
                _dbps.Add("@isNTSReport", NpgsqlDbType.Varchar, "F");
                _dbps.Add("@isNTSSendingX", NpgsqlDbType.Varchar, "X");
            }

            return LDataHelper.ExecuteText(UAppHelper.ConnectionString, _sqlstr, _dbps);
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
                if (m_ireporter != null)
                {
                    m_ireporter.Dispose();
                    m_ireporter = null;
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
        ~Engine()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}