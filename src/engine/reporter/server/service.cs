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
using System.ServiceModel;
using NpgsqlTypes;
using OdinSdk.OdinLib.Data.POSTGRESQL;

namespace OpenETaxBill.Engine.Reporter
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerSession, IncludeExceptionDetailInFaults = true)]
    public class ReportService : IReportService, IDisposable
    {
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

        private OdinSdk.OdinLib.Data.POSTGRESQL.PgDataHelper m_dataHelper = null;
        private OdinSdk.OdinLib.Data.POSTGRESQL.PgDataHelper LSQLHelper
        {
            get
            {
                if (m_dataHelper == null)
                    m_dataHelper = new OdinSdk.OdinLib.Data.POSTGRESQL.PgDataHelper();
                return m_dataHelper;
            }
        }

        private OpenETaxBill.Engine.Reporter.Engine m_ereporter = null;
        private OpenETaxBill.Engine.Reporter.Engine EReporter
        {
            get
            {
                if (m_ereporter == null)
                    m_ereporter = new OpenETaxBill.Engine.Reporter.Engine();

                return m_ereporter;
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
            if (IReporter.CheckValidApplication(p_certapp) == true)
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
        public int ReportWithDateRange(Guid p_certapp, string p_invoicerId, DateTime p_fromDay, DateTime p_tillDay)
        {
            int _result = 0;

            try
            {
                if (IReporter.CheckValidApplication(p_certapp) == true)
                {
                    var _sqlstr
                            = "SELECT b.invoicerId, COUNT(b.invoicerId) as norec "
                            + "  FROM TB_eTAX_ISSUING a INNER JOIN TB_eTAX_INVOICE b "
                            + "    ON a.issueId=b.issueId "
                            + " WHERE a.isNTSReport != @isNTSReport "
                            + "   AND ( "
                            + "         (RIGHT(b.typeCode, 2) IN ('01', '02', '04') AND b.invoicerId=@invoicerId) "
                            + "         OR "
                            + "         (RIGHT(b.typeCode, 2) IN ('03', '05') AND b.brokerId=@invoicerId) "
                            + "       ) "
                            + "   AND b.issueDate>=@fromDay AND b.issueDate<=@tillDay "
                            + " GROUP BY b.invoicerId";

                    var _dbps = new PgDatParameters();
                    {
                        _dbps.Add("@isNTSReport", NpgsqlDbType.Varchar, "T");
                        _dbps.Add("@invoicerId", NpgsqlDbType.Varchar, p_invoicerId);
                        _dbps.Add("@fromDay", NpgsqlDbType.TimestampTz, p_fromDay);
                        _dbps.Add("@tillDay", NpgsqlDbType.TimestampTz, p_tillDay);
                    }

                    var _ds = LSQLHelper.SelectDataSet(UAppHelper.ConnectionString, _sqlstr, _dbps);
                    if (LSQLHelper.IsNullOrEmpty(_ds) == false)
                    {
                        var _rows = _ds.Tables[0].Rows;
                        for (int i = 0; i < _rows.Count; i++)
                        {
                            string _invoicerId = Convert.ToString(_rows[i]["invoicerId"]);
                            int _noInvoice = Convert.ToInt32(_rows[i]["norec"]);

                            _result += EReporter.DoReportInvoicer(_invoicerId, _noInvoice, p_fromDay, p_tillDay);
                        }
                    }
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

            return _result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_certapp"></param>
        /// <param name="p_invoicerId"></param>
        /// <param name="p_issueIds"></param>
        /// <returns></returns>
        public int ReportWithIssueIDs(Guid p_certapp, string p_invoicerId, string[] p_issueIds)
        {
            int _result = 0;

            try
            {
                if (IReporter.CheckValidApplication(p_certapp) == true)
                {
                    if (p_issueIds.Length > 100)
                        throw new ReporterException(String.Format("Issue-ids can not exceed 100-records. invoiceId->'{0}', length->{1})", p_invoicerId, p_issueIds.Length));

                    _result = EReporter.DoReportInvoicer(p_invoicerId, p_issueIds);
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

            return _result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_certapp"></param>
        /// <param name="p_refSubmitID"></param>
        /// <returns></returns>
        public bool RequestResult(Guid p_certapp, string p_refSubmitID)
        {
            var _result = false;

            try
            {
                if (IReporter.CheckValidApplication(p_certapp) == true)
                    _result = EReporter.DoRequestSubmit(p_refSubmitID);
            }
            catch (ReporterException ex)
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
                if (IReporter.CheckValidApplication(p_certapp) == true)
                    _result = EReporter.ClearXFlag(p_invoicerId);
            }
            catch (ReporterException ex)
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
                if (m_ireporter != null)
                {
                    m_ireporter.Dispose();
                    m_ireporter = null;
                }
        }

        /// <summary>
        /// 
        /// </summary>
        ~ReportService()
        {
            Dispose(false);
        }
 
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}