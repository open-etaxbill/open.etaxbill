using System;
using System.Data;
using System.ServiceModel;
using OpenETaxBill.Engine.Library;
using OpenETaxBill.SDK.Data;
using OpenETaxBill.SDK.Data.Collection;

namespace OpenETaxBill.Engine.Mailer
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerSession, IncludeExceptionDetailInFaults = true)]
    public class MailerService : IMailerService, IDisposable
    {
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

        private OpenETaxBill.Engine.Mailer.Engine m_emailer = null;
        private OpenETaxBill.Engine.Mailer.Engine EMailer
        {
            get
            {
                if (m_emailer == null)
                    m_emailer = new OpenETaxBill.Engine.Mailer.Engine();

                return m_emailer;
            }
        }

        private OpenETaxBill.Engine.Library.USvcHelper m_appHelper = null;
        public OpenETaxBill.Engine.Library.USvcHelper UAppHelper
        {
            get
            {
                if (m_appHelper == null)
                    m_appHelper = new OpenETaxBill.Engine.Library.USvcHelper(IMailer.Manager);

                return m_appHelper;
            }
        }

        private OpenETaxBill.SDK.Data.DataHelper m_dataHelper = null;
        private OpenETaxBill.SDK.Data.DataHelper LDataHelper
        {
            get
            {
                if (m_dataHelper == null)
                    m_dataHelper = new OpenETaxBill.SDK.Data.DataHelper();
                return m_dataHelper;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // logger
        //-------------------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_certapp"></param>
        /// <param name="p_exception"></param>
        /// <param name="p_message"></param>
        public void WriteLog(Guid p_certapp, string p_exception, string p_message)
        {
            if (IMailer.CheckValidApplication(p_certapp) == true)
                ELogger.SNG.WriteLog(p_exception, p_message);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_certapp"></param>
        /// <param name="p_invoicerId"></param>
        /// <param name="p_fromDay"></param>
        /// <param name="p_tillDay"></param>
        /// <returns></returns>
        public int SendWithDateRange(Guid p_certapp, string p_invoicerId, DateTime p_fromDay, DateTime p_tillDay)
        {
            int _result = 0;

            try
            {
                if (IMailer.CheckValidApplication(p_certapp) == true)
                {
                    UTextHelper.SNG.GetSendingRange(ref p_fromDay, ref p_tillDay);

                    string _sqlstr
                        = "SELECT b.invoicerId, COUNT(b.invoicerId) as norec "
                        + "  FROM TB_eTAX_ISSUING a INNER JOIN TB_eTAX_INVOICE b "
                        + "    ON a.issueId=b.issueId "
                        + " WHERE (a.isInvoiceeMail != @isInvoiceeMail OR a.isProviderMail != @isProviderMail) "
                        + "   AND ( "
                        + "         (RIGHT(b.typeCode, 2) IN ('01', '02', '04') AND b.invoicerId=@invoicerId) "
                        + "         OR "
                        + "         (RIGHT(b.typeCode, 2) IN ('03', '05') AND b.brokerId=@invoicerId) "
                        + "       ) "
                        + "   AND b.issueDate>=@fromDate AND b.issueDate<=@tillDate "
                        + " GROUP BY b.invoicerId";

                    var _dbps = new DatParameters();
                    _dbps.Add("@invoicerId", SqlDbType.NVarChar, p_invoicerId);
                    _dbps.Add("@isInvoiceeMail", SqlDbType.NVarChar, "T");
                    _dbps.Add("@isProviderMail", SqlDbType.NVarChar, "T");
                    _dbps.Add("@fromDate", SqlDbType.DateTime, p_fromDay);
                    _dbps.Add("@tillDate", SqlDbType.DateTime, p_tillDay);

                    var _ds = LDataHelper.SelectDataSet(UAppHelper.ConnectionString, _sqlstr, _dbps);
                    if (LDataHelper.IsNullOrEmpty(_ds) == false)
                    {
                        var _rows = _ds.Tables[0].Rows;
                        for (int i = 0; i < _rows.Count; i++)
                        {
                            string _invoicerId = Convert.ToString(_rows[i]["invoicerId"]);
                            int _noIssuing = Convert.ToInt32(_rows[i]["norec"]);

                            _result += EMailer.DoMailSend(_invoicerId, _noIssuing, p_fromDay, p_tillDay);
                        }
                    }
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

            return _result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_certapp"></param>
        /// <param name="p_invoicerId"></param>
        /// <param name="p_issueIds"></param>
        /// <returns></returns>
        public int SendWithIssueIDs(Guid p_certapp, string p_invoicerId, string[] p_issueIds)
        {
            int _result = 0;

            try
            {
                if (IMailer.CheckValidApplication(p_certapp) == true)
                {
                    if (p_issueIds.Length > 100)
                        throw new MailerException(String.Format("Issue-ids can not exceed 100-records. invoiceId->'{0}', length->{1})", p_invoicerId, p_issueIds.Length));

                    _result = EMailer.DoMailSend(p_invoicerId, p_issueIds);
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

            return _result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_certapp"></param>
        /// <param name="p_invoicerId"></param>
        /// <param name="p_issue_id"></param>
        /// <param name="p_newMailAddress"></param>
        /// <returns></returns>
        public int ReSendWithIssueID(Guid p_certapp, string p_invoicerId, string p_issue_id, string p_newMailAddress)
        {
            int _result = 0;

            try
            {
                if (IMailer.CheckValidApplication(p_certapp) == true)
                    _result = EMailer.DoMailReSend(p_invoicerId, p_issue_id, p_newMailAddress);
            }
            catch (MailerException ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }

            return _result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_certapp"></param>
        /// <param name="p_invoicerId"></param>
        /// <returns></returns>
        public int ClearXFlag(Guid p_certapp, string p_invoicerId)
        {
            int _result = 0;

            try
            {
                if (IMailer.CheckValidApplication(p_certapp) == true)
                    _result = EMailer.ClearXFlag(p_invoicerId);
            }
            catch (MailerException ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
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
                if (m_imailer != null)
                {
                    m_imailer.Dispose();
                    m_imailer = null;
                }
        }

        /// <summary>
        /// 
        /// </summary>
        ~MailerService()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}