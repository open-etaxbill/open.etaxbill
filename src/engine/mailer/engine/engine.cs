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
using OpenETaxBill.Channel.Library.Security.Issue;
using OdinSoft.SDK.Configuration;
using OdinSoft.SDK.Data;
using OdinSoft.SDK.Data.Collection;

namespace OpenETaxBill.Engine.Mailer
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
        private OpenETaxBill.Channel.Interface.IMailer m_imailer = null;
        private OpenETaxBill.Channel.Interface.IMailer IMailer
        {
            get
            {
                if (m_imailer == null)
                    m_imailer = new OpenETaxBill.Channel.Interface.IMailer();

                return m_imailer;
            }
        }

        private OpenETaxBill.Engine.Library.UAppHelper m_appHelper = null;
        public OpenETaxBill.Engine.Library.UAppHelper UAppHelper
        {
            get
            {
                if (m_appHelper == null)
                    m_appHelper = new OpenETaxBill.Engine.Library.UAppHelper(IMailer.Manager);

                return m_appHelper;
            }
        }

        private OdinSoft.SDK.Data.DataHelper m_dataHelper = null;
        private OdinSoft.SDK.Data.DataHelper LDataHelper
        {
            get
            {
                if (m_dataHelper == null)
                    m_dataHelper = new OdinSoft.SDK.Data.DataHelper();
                return m_dataHelper;
            }
        }

        private OdinSoft.SDK.Data.DeltaHelper m_dltaHelper = null;
        private OdinSoft.SDK.Data.DeltaHelper LDltaHelper
        {
            get
            {
                if (m_dltaHelper == null)
                    m_dltaHelper = new OdinSoft.SDK.Data.DeltaHelper();

                return m_dltaHelper;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private DataSet m_issuingSet = null;
        private DataSet IssuingSet
        {
            get
            {
                if (m_issuingSet == null)
                    m_issuingSet = Schema.SNG.GetTaxSchema();

                return m_issuingSet;
            }
        }

        private DataTable m_resultTbl = null;
        private DataTable ResultTbl
        {
            get
            {
                if (m_resultTbl == null)
                    m_resultTbl = IssuingSet.Tables["TB_eTAX_RESULT"];

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
                            IssuingSet, "TB_eTAX_ISSUING",
                            new string[] { "issueId", "securityId", "isInvoiceeMail", "isProviderMail", "isMailSending", "sendMailCount", "mailSendingDate" }
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
        private struct MailingArgs
        {
            public bool reSending;
            public string invoiceeEMail;

            public string invoicerId;
            public int noInvoicee;
            public int noSending;

            public string where;
            public DatParameters dbps;
        }

        private int CheckReEnter(string p_invoicerId, string p_where, DatParameters p_dbps)
        {
            int _result = -1;

            // 동일한 조건으로 실행 중인 record가 1개 이상 있는지 확인 한다.
            string _sqlstr
                = "SELECT COUNT(a.isMailSending) as norec "
                + "  FROM TB_eTAX_ISSUING a INNER JOIN TB_eTAX_INVOICE b "
                + "    ON a.issueId=b.issueId "
                + " WHERE a.isMailSending=@isMailSendingX "
                + "   AND ( "
                + "         (RIGHT(b.typeCode, 2) IN ('01', '02', '04') AND b.invoicerId=@invoicerId) "
                + "         OR "
                + "         (RIGHT(b.typeCode, 2) IN ('03', '05') AND b.brokerId=@invoicerId) "
                + "       ) "
                + p_where;

            p_dbps.Add("@invoicerId", SqlDbType.NVarChar, p_invoicerId);
            p_dbps.Add("@isMailSendingX", SqlDbType.NVarChar, "X");

            var _ds = LDataHelper.SelectDataSet(UAppHelper.ConnectionString, _sqlstr, p_dbps);

            int _norec = Convert.ToInt32(_ds.Tables[0].Rows[0]["norec"]);
            if (_norec < 1)
            {
                // 매입처 메일 발송이 안되었거나, ASP사업자에게 메일 발송이 안된경우를 선택한다.
                string _updstr
                        = "UPDATE TB_eTAX_ISSUING "
                        + "   SET isMailSending=@isMailSendingX "
                        + "  FROM TB_eTAX_ISSUING a INNER JOIN TB_eTAX_INVOICE b "
                        + "    ON a.issueId=b.issueId "
                        + " WHERE (a.isInvoiceeMail != @isInvoiceeMail OR a.isProviderMail != @isProviderMail) "
                        + "   AND ( "
                        + "         (RIGHT(b.typeCode, 2) IN ('01', '02', '04') AND b.invoicerId=@invoicerId) "
                        + "         OR "
                        + "         (RIGHT(b.typeCode, 2) IN ('03', '05') AND b.brokerId=@invoicerId) "
                        + "       ) "
                        + p_where;

                p_dbps.Add("@invoicerId", SqlDbType.NVarChar, p_invoicerId);
                p_dbps.Add("@isMailSendingX", SqlDbType.NVarChar, "X");
                p_dbps.Add("@isInvoiceeMail", SqlDbType.NVarChar, "T");
                p_dbps.Add("@isProviderMail", SqlDbType.NVarChar, "T");

                _result = LDataHelper.ExecuteText(UAppHelper.ConnectionString, _updstr, p_dbps);
            }
            else
            {
                if (LogCommands == true)
                    ELogger.SNG.WriteLog(String.Format("re-enter: invoicerId->'{0}', working-row(s)->{1}", p_invoicerId, _norec));
            }

            return _result;
        }

        private int CheckMailing(string p_invoicerId, int p_noInvoicee, string p_where, DatParameters p_dbps)
        {
            int _noSending = 0;

            lock (SyncEngine)
                _noSending = CheckReEnter(p_invoicerId, p_where, p_dbps);

            if (_noSending > 0)
            {
                MailingArgs _args = new MailingArgs()
                {
                    reSending = false,
                    invoiceeEMail = "",
                    invoicerId = p_invoicerId,
                    noInvoicee = p_noInvoicee,
                    noSending = _noSending,
                    where = p_where,
                    dbps = p_dbps
                };

                // Do not use using statement
                ThreadPoolWait _doneEvent = new ThreadPoolWait();
                _doneEvent.QueueUserWorkItem(DoMailing, _args);

                if (Environment.UserInteractive == true)
                    _doneEvent.WaitOne();
            }

            return _noSending;
        }

        private void DoMailing(object p_args)
        {
            MailingArgs _args = (MailingArgs)p_args;
            _args.noInvoicee = _args.noSending;
            _args.noSending = 0;

            try
            {
                int _toprow = 800;

                string _sqlstr
                    = "SELECT TOP " + _toprow + " a.issueId, a.document, a.securityId, b.issueDate, b.typeCode, b.invoiceeKind, "
                    + "               b.chargeTotal, b.taxTotal, b.grandTotal, b.description, a.isMailSending, "
                    + "               b.invoicerId, b.invoicerEMail, b.invoicerName, b.invoicerPerson, b.invoicerPhone, "
                    + "               a.isInvoiceeMail, b.invoiceeId, b.invoiceeEMail1 as invoiceeEMail, b.invoiceeName, "
                    + "               b.invoiceePerson, b.invoiceePhone1 as invoiceePhone, a.isProviderMail, a.providerId, "
                    + "               a.providerEMail, a.sendMailCount, a.mailSendingDate "
                    + "  FROM TB_eTAX_ISSUING a INNER JOIN TB_eTAX_INVOICE b "
                    + "    ON a.issueId=b.issueId "
                    + " WHERE a.isMailSending=@isMailSendingX "       // to avoid infinite loop, do check isMailSending here.
                    + "   AND ( "
                    + "         (RIGHT(b.typeCode, 2) IN ('01', '02', '04') AND b.invoicerId=@invoicerId) "
                    + "         OR "
                    + "         (RIGHT(b.typeCode, 2) IN ('03', '05') AND b.brokerId=@invoicerId) "
                    + "       ) "
                    + _args.where
                    + " ORDER BY a.providerEMail";
                {
                    _args.dbps.Add("@isMailSendingX", SqlDbType.NVarChar, "X");
                    _args.dbps.Add("@invoicerId", SqlDbType.NVarChar, _args.invoicerId);
                }

                //if (LogCommands == true)
                //    ELogger.SNG.WriteLog(String.Format("begin: invoicerId->'{0}', noInvoicee->{1}", _args.invoicerId, _args.noInvoicee));

                var _random = new Random();

                // 만약 InsertDeltaSet을 처리하는 중에 오류가 발생하면 무한 loop를 발생 하게 되므로,
                // 'X'로 marking한 레코드의 총 갯수를 감소하여 '0'보다 큰 경우에만 반복한다.
                while (_args.noInvoicee > 0)
                {
                    IssuingTbl.Clear();
                    ResultTbl.Clear();

                    DataSet _workingSet = LDataHelper.SelectDataSet(UAppHelper.ConnectionString, _sqlstr, _args.dbps);
                    if (LDataHelper.IsNullOrEmpty(_workingSet) == true)
                        break;

                    var _rows = _workingSet.Tables[0].Rows;

                    var _doneEvents = new ThreadPoolWait[_rows.Count];
                    for (int i = 0; i < _rows.Count; i++)
                    {
                        if (String.IsNullOrEmpty(Convert.ToString(_rows[i]["securityId"])) == true)
                            _rows[i]["securityId"] = Convert.ToString(_random.Next(100000, 999999));

                        if (_args.reSending == true)
                            _rows[i]["invoiceeEMail"] = _args.invoiceeEMail;

                        _doneEvents[i] = new ThreadPoolWait();

                        AsyncWorker _worker = new AsyncWorker(IssuingTbl, ResultTbl);
                        _doneEvents[i].QueueUserWorkItem(_worker.MailerCallback, _rows[i]);

                        if (Environment.UserInteractive == true)
                            _doneEvents[i].WaitOne();
                    }

                    ThreadPoolWait.WaitForAll(_doneEvents);

                    // 처리된 레코드가 한개 이하 인 경우는 종료한다. (문제가 있는 경우로 보여 짐)
                    if (_rows.Count < 1)
                        break;
                    
                    //if (LogCommands == true)
                    //    ELogger.SNG.WriteLog(String.Format("loop: invoicerId->'{0}', noInvoicee->{1}, noSending->{2}", _args.invoicerId, _args.noInvoicee, _rows.Count));

                    _args.noInvoicee -= _rows.Count;
                    _args.noSending += IssuingTbl.Rows.Count;

                    LDltaHelper.InsertDeltaSet(UAppHelper.ConnectionString, IssuingSet);
                }
            }
            catch (MailerException ex)
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
                    ELogger.SNG.WriteLog(String.Format("end: invoicerId->'{0}', noInvoicee->{1}, noSending->{2}", _args.invoicerId, _args.noInvoicee, _args.noSending));

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
            IMailer.WriteDebug(p_invoicerId);

            string _sqlstr
                    = "SELECT sendingType, sendFromDay, sendTillDay "
                    + "  FROM TB_eTAX_CUSTOMER "
                    + " WHERE customerId=@customerId";

            var _dbps = new DatParameters();
            _dbps.Add("@customerId", SqlDbType.NVarChar, p_invoicerId);

            DataSet _customerSet = LDataHelper.SelectDataSet(UAppHelper.ConnectionString, _sqlstr, _dbps);
            if (LDataHelper.IsNullOrEmpty(_customerSet) == true)
                throw new MailerException(String.Format("not exist customer: invoicerId->'{0}'", p_invoicerId));

            return _customerSet;
        }

        //public int DoMailSend(string p_invoicerId, int p_noInvoicee)
        //{
        //    string _where = "";
        //    var _dbps = new DatParameters();

        //    return CheckMailing(p_invoicerId, p_noInvoicee, _where, _dbps);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_invoicerId"></param>
        /// <param name="p_issueIds"></param>
        /// <returns></returns>
        public int DoMailSend(string p_invoicerId, string[] p_issueIds)
        {
            IMailer.WriteDebug(p_invoicerId);

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
                var _dbps = new DatParameters();

                _result = CheckMailing(p_invoicerId, p_issueIds.Length, _where, _dbps);
            }

            return _result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_invoicerId"></param>
        /// <param name="p_issue_id"></param>
        /// <param name="p_newMailAddress"></param>
        /// <returns></returns>
        public int DoMailReSend(string p_invoicerId, string p_issue_id, string p_newMailAddress)
        {
            IMailer.WriteDebug(p_invoicerId);

            int _result = -1;

            string _sqlstr
                    = "UPDATE TB_eTAX_ISSUING "
                    + "   SET isInvoiceeMail=@isInvoiceeMail "
                    + "  FROM TB_eTAX_ISSUING a INNER JOIN TB_eTAX_INVOICE b "
                    + "    ON a.issueId=b.issueId "
                    + " WHERE a.isMailSending!=@isMailSendingX "
                    + "   AND ( "
                    + "         (RIGHT(b.typeCode, 2) IN ('01', '02', '04') AND b.invoicerId=@invoicerId) "
                    + "         OR "
                    + "         (RIGHT(b.typeCode, 2) IN ('03', '05') AND b.brokerId=@invoicerId) "
                    + "       ) "
                    + "   AND a.issueId=@issueId";

            var _dbps = new DatParameters();
            _dbps.Add("@isMailSendingX", SqlDbType.NVarChar, "X");
            _dbps.Add("@invoicerId", SqlDbType.NVarChar, p_invoicerId);

            _dbps.Add("@isInvoiceeMail", SqlDbType.NVarChar, "X");
            _dbps.Add("@issueId", SqlDbType.NVarChar, p_issue_id);

            if (LDataHelper.ExecuteText(UAppHelper.ConnectionString, _sqlstr, _dbps) > 0)
            {
                string _where = " AND a.issueId=@issueId ";

                _dbps.Clear();
                _dbps.Add("@issueId", SqlDbType.NVarChar, p_issue_id);

                lock (SyncEngine)
                    _result = CheckReEnter(p_invoicerId, _where, _dbps);

                if (_result > 0)
                {
                    MailingArgs _args = new MailingArgs()
                    {
                        reSending = true,
                        invoiceeEMail = p_newMailAddress,
                        invoicerId = p_invoicerId,
                        noInvoicee = 1,
                        noSending = _result,
                        where = _where,
                        dbps = _dbps
                    };

                    DoMailing(_args);
                }
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
        public int DoMailSend(string p_invoicerId, int p_noInvoicee, DateTime p_fromDay, DateTime p_tillDay)
        {
            IMailer.WriteDebug(p_invoicerId);

            string _where = " AND b.issueDate>=@fromDate AND b.issueDate<=@tillDate ";

            var _dbps = new DatParameters();
            _dbps.Add("@fromDate", SqlDbType.DateTime, p_fromDay);
            _dbps.Add("@tillDate", SqlDbType.DateTime, p_tillDay);

            return CheckMailing(p_invoicerId, p_noInvoicee, _where, _dbps);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_invoicerId"></param>
        /// <returns></returns>
        public int ClearXFlag(string p_invoicerId)
        {
            IMailer.WriteDebug(p_invoicerId);

            string _sqlstr
                    = "UPDATE TB_eTAX_ISSUING "
                    + "   SET isMailSending=@isMailSending, isInvoiceeMail=@isInvoiceeMail, isProviderMail=@isProviderMail "
                    + "  FROM TB_eTAX_ISSUING a INNER JOIN TB_eTAX_INVOICE b "
                    + "    ON a.issueId=b.issueId "
                    + " WHERE a.isMailSending=@isMailSendingX "
                    + "   AND ( "
                    + "         (RIGHT(b.typeCode, 2) IN ('01', '02', '04') AND b.invoicerId=@invoicerId) "
                    + "         OR "
                    + "         (RIGHT(b.typeCode, 2) IN ('03', '05') AND b.brokerId=@invoicerId) "
                    + "       ) ";

            var _dbps = new DatParameters();
            _dbps.Add("@isMailSending", SqlDbType.NVarChar, "F");
            _dbps.Add("@isInvoiceeMail", SqlDbType.NVarChar, "F");
            _dbps.Add("@isProviderMail", SqlDbType.NVarChar, "F");
            _dbps.Add("@isMailSendingX", SqlDbType.NVarChar, "X");
            _dbps.Add("@invoicerId", SqlDbType.NVarChar, p_invoicerId);

            return LDataHelper.ExecuteText(UAppHelper.ConnectionString, _sqlstr, _dbps);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int ClearXFlag()
        {
            IMailer.WriteDebug("*");

            string _sqlstr
                    = "UPDATE TB_eTAX_ISSUING "
                    + "   SET isMailSending=@isMailSending, isInvoiceeMail=@isInvoiceeMail, isProviderMail=@isProviderMail "
                    + " WHERE isMailSending=@isMailSendingX";

            var _dbps = new DatParameters();
            {
                _dbps.Add("@isMailSending", SqlDbType.NVarChar, "F");
                _dbps.Add("@isInvoiceeMail", SqlDbType.NVarChar, "F");
                _dbps.Add("@isProviderMail", SqlDbType.NVarChar, "F");
                _dbps.Add("@isMailSendingX", SqlDbType.NVarChar, "X");
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
                if (m_imailer != null)
                {
                    m_imailer.Dispose();
                    m_imailer = null;
                }
                if (m_issuingSet != null)
                {
                    m_issuingSet.Dispose();
                    m_issuingSet = null;
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
        ~Engine()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}
