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
using OdinSdk.eTaxBill.Security.Signature;
using OpenETaxBill.Engine.Library;

namespace OpenETaxBill.Engine.Signer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerSession, IncludeExceptionDetailInFaults=true)]
    public class SignService : ISignerService, IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
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

        private OpenETaxBill.Engine.Library.UCertHelper m_certHelper = null;
        public OpenETaxBill.Engine.Library.UCertHelper UCertHelper
        {
            get
            {
                if (m_certHelper == null)
                    m_certHelper = new OpenETaxBill.Engine.Library.UCertHelper(ISigner.Manager);

                return m_certHelper;
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

        private OpenETaxBill.Engine.Signer.Engine m_eSigner = null;
        private OpenETaxBill.Engine.Signer.Engine ESigner
        {
            get
            {
                if (m_eSigner == null)
                    m_eSigner = new OpenETaxBill.Engine.Signer.Engine();

                return m_eSigner;
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
            if (ISigner.CheckValidApplication(p_certapp) == true)
                ELogger.SNG.WriteLog(p_exception, p_message);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_certapp"></param>
        /// <param name="p_certifier"></param>
        /// <param name="p_invoicerId"></param>
        /// <param name="p_fromDay"></param>
        /// <param name="p_tillDay"></param>
        /// <returns></returns>
        public int SignatureWithDateRange(Guid p_certapp, string[] p_certifier, string p_invoicerId, DateTime p_fromDay, DateTime p_tillDay)
        {
            int _result = 0;

            try
            {
                if (ISigner.CheckValidApplication(p_certapp) == true)
                {
                    UTextHelper.SNG.GetSigningRange(ref p_fromDay, ref p_tillDay);

                    var _sqlstr
                            = "SELECT invoicerId, COUNT(invoicerId) as norec "
                            + "  FROM TB_eTAX_INVOICE "
                            + " WHERE isSuccess != @isSuccess "       // for resignning, do check 'isSuccess' here 
                            + "   AND ( "
                            + "         (RIGHT(typeCode, 2) IN ('01', '02', '04') AND invoicerId=@invoicerId) "
                            + "         OR "
                            + "         (RIGHT(typeCode, 2) IN ('03', '05') AND brokerId=@invoicerId) "
                            + "       ) "
                            + "   AND issueDate>=@fromDate AND issueDate<=@tillDate "
                            + " GROUP BY invoicerId";

                    var _dbps = new PgDatParameters();
                    {
                        _dbps.Add("@isSuccess", NpgsqlDbType.Varchar, "T");
                        _dbps.Add("@invoicerId", NpgsqlDbType.Varchar, p_invoicerId);
                        _dbps.Add("@fromDate", NpgsqlDbType.TimestampTz, p_fromDay);
                        _dbps.Add("@tillDate", NpgsqlDbType.TimestampTz, p_tillDay);
                    }

                    var _ds = LSQLHelper.SelectDataSet(UAppHelper.ConnectionString, _sqlstr, _dbps);
                    if (LSQLHelper.IsNullOrEmpty(_ds) == false)
                    {
                        X509CertMgr _invoicerCert = UCertHelper.GetCustomerCertMgr(p_invoicerId, p_certifier[0], p_certifier[1], p_certifier[2]);

                        var _rows = _ds.Tables[0].Rows;
                        for (int i = 0; i < _rows.Count; i++)
                        {
                            string _invoicerId = Convert.ToString(_rows[i]["invoicerId"]);
                            int _noInvoice = Convert.ToInt32(_rows[i]["norec"]);

                            _result += ESigner.DoSignInvoice(_invoicerCert, _invoicerId, _noInvoice, p_fromDay, p_tillDay);
                        }
                    }
                }
            }
            catch (SignerException ex)
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
        /// <param name="p_certifier"></param>
        /// <param name="p_invoicerId"></param>
        /// <param name="p_issueIds"></param>
        /// <returns></returns>
        public int SignatureWithIDs(Guid p_certapp, string[] p_certifier, string p_invoicerId, string[] p_issueIds)
        {
            int _result = 0;

            try
            {
                if (ISigner.CheckValidApplication(p_certapp) == true)
                {
                    if (p_issueIds.Length > 100)
                        throw new SignerException(String.Format("Issue-ids can not exceed 100-records. invoiceId->'{0}', length->{1})", p_invoicerId, p_issueIds.Length));

                    X509CertMgr _invoicerCert = UCertHelper.GetCustomerCertMgr(p_invoicerId, p_certifier[0], p_certifier[1], p_certifier[2]);
                    _result = ESigner.DoSignInvoice(_invoicerCert, p_invoicerId, p_issueIds);
                }
            }
            catch (SignerException ex)
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
                if (ISigner.CheckValidApplication(p_certapp) == true)
                    _result = ESigner.ClearXFlag(p_invoicerId);
            }
            catch (SignerException ex)
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
                if (m_iSigner != null)
                {
                    m_iSigner.Dispose();
                    m_iSigner = null;
                }
        }

        /// <summary>
        /// 
        /// </summary>
        ~SignService()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}