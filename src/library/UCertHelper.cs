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
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Npgsql;
using NpgsqlTypes;
using OdinSoft.SDK.Data.POSTGRESQL;
using OdinSoft.SDK.eTaxBill.Security.Encrypt;
using OdinSoft.SDK.eTaxBill.Security.Signature;
using OdinSoft.SDK.eTaxBill.Utility;
using OdinSoft.SDK.Queue;

namespace OpenETaxBill.Engine.Library
{
    /// <summary>
    /// 
    /// </summary>
    public class UCertHelper : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_manager"></param>
        public UCertHelper(QService p_manager)
        {
            m_qmaster = (QService)p_manager.Clone();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_manager"></param>
        /// <param name="p_isService"></param>
        public UCertHelper(QService p_manager, bool p_isService)
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
        private OpenETaxBill.Engine.Library.UAppHelper m_svcHelper = null;
        public OpenETaxBill.Engine.Library.UAppHelper USvcHelper
        {
            get
            {
                if (m_svcHelper == null)
                    m_svcHelper = new OpenETaxBill.Engine.Library.UAppHelper(QMaster);

                return m_svcHelper;
            }
        }

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

        private X509CertMgr GetCertManager(string p_condition, string p_publicStr, string p_privateStr, string p_passwordStr)
        {
            byte[] _privateKey = Encryptor.SNG.ChiperBase64ToPlainBytes(p_privateStr);
            byte[] _publicKey = Encryptor.SNG.ChiperBase64ToPlainBytes(p_publicStr);
            string _passwordTxt = Encoding.UTF8.GetString(Encryptor.SNG.ChiperBase64ToPlainBytes(p_passwordStr));

            X509CertMgr _result = new X509CertMgr(_publicKey, _privateKey, _passwordTxt);
            {
                DateTime _expiration = Convert.ToDateTime(_result.X509Cert2.GetExpirationDateString());
                if (_expiration.Subtract(DateTime.Now).Days < USvcHelper.AvailablePeriod)
                    throw new ProxyException(String.Format("certifier expired: '{0}', '{1}'", _expiration, p_condition));
            }

            return _result;
        }

        private X509CertMgr GetCertManager(string p_condition, string p_sqlstr, PgDatParameters p_dbps)
        {
            var _ds = LDataHelper.SelectDataSet(USvcHelper.ConnectionString, p_sqlstr, p_dbps);
            if (LDataHelper.IsNullOrEmpty(_ds) == true)
                throw new ProxyException(String.Format("not exist cert-record: '{0}'", p_condition));

            DataRow _cr = _ds.Tables[0].Rows[0];

            string _publicStr = Convert.ToString(_cr["publicKey"]);
            if (String.IsNullOrEmpty(_publicStr) == true)
                throw new ProxyException(String.Format("empty public-key: '{0}'", p_condition));

            string _privateStr = Convert.ToString(_cr["privateKey"]);
            if (String.IsNullOrEmpty(_privateStr) == true)
                throw new ProxyException(String.Format("empty private-key: '{0}'", p_condition));

            string _passwordStr = Convert.ToString(_cr["password"]);
            if (String.IsNullOrEmpty(_passwordStr) == true)
                throw new ProxyException(String.Format("empty password: '{0}'", p_condition));

            return GetCertManager(p_condition, _publicStr, _privateStr, _passwordStr);
        }

        private X509Certificate2 GetCertificate(string p_condition, string p_sqlstr, PgDatParameters p_dbps)
        {
            var _ds = LDataHelper.SelectDataSet(USvcHelper.ConnectionString, p_sqlstr, p_dbps);

            if (LDataHelper.IsNullOrEmpty(_ds) == true)
                throw new ProxyException(String.Format("not exist cert-record: '{0}'", p_condition));

            string _publicStr = Convert.ToString(_ds.Tables[0].Rows[0]["publicKey"]);
            if (String.IsNullOrEmpty(_publicStr) == true)
                throw new ProxyException(String.Format("empty public-key: '{0}'", p_condition));

            byte[] _publicKey = Encryptor.SNG.ChiperBase64ToPlainBytes(_publicStr);
            return new X509Certificate2(_publicKey);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // ASP 사업자 인증서 읽어오기
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_providerId"></param>
        /// <returns></returns>
        public X509Certificate2 GetProviderCertByProvider(string p_providerId)
        {
            string _sqlstr = "SELECT publicKey FROM TB_eTAX_PROVIDER WHERE providerId=@providerId";

            var _dbps = new PgDatParameters();
            {
                _dbps.Add("@providerId", NpgsqlDbType.Varchar, p_providerId);
            }

            return GetCertificate(
                String.Format("by provider table with providerId: '{0}'", p_providerId),
                _sqlstr, _dbps
                );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_invoiceeId"></param>
        /// <returns></returns>
        public X509Certificate2 GetProviderCertByCustomer(string p_invoiceeId)
        {
            string _sqlstr
                = "SELECT b.publicKey FROM TB_eTAX_CUSTOMER a INNER JOIN TB_eTAX_PROVIDER b "
                + "    ON a.providerId=b.providerId AND NULLIF(b.providerId, '') IS NOT NULL "
                + " WHERE a.customerId=@customerId";

            var _dbps = new PgDatParameters();
            {
                _dbps.Add("@customerId", NpgsqlDbType.Varchar, p_invoiceeId);
            }

            return GetCertificate(
                String.Format("by provider table with customerId: '{0}'", p_invoiceeId),
                _sqlstr, _dbps
                );
        }

        //-----------------------------------------------------------------------------------------------------------------//
        // 고객 공인인증서 읽어 오기
        //-----------------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 고객의 공인인증서를 DB에서 가져온다.
        /// </summary>
        /// <param name="p_customerId">고객ID</param>
        /// <returns></returns>
        public X509CertMgr GetCustomerCertMgr(string p_customerId)
        {
            string _sqlstr
                = "SELECT privateKey, publicKey, password "
                + "  FROM TB_eTAX_CUSTOMER "
                + " WHERE customerId=@customerId";

            var _dbps = new PgDatParameters();
            {
                _dbps.Add("@customerId", NpgsqlDbType.Varchar, p_customerId);
            }

            return GetCertManager(
                String.Format("by customer table with customerid: '{0}'", p_customerId),
                _sqlstr, _dbps
                );
        }

        /// <summary>
        /// 고객의 공인인증서를 DB에서 가져온다.
        /// </summary>
        /// <param name="p_customerId"></param>
        /// <param name="p_publicStr"></param>
        /// <param name="p_privateStr"></param>
        /// <param name="p_passwordStr"></param>
        /// <returns></returns>
        public X509CertMgr GetCustomerCertMgr(string p_customerId, string p_publicStr, string p_privateStr, string p_passwordStr)
        {
            return GetCertManager(
                String.Format("by user-certification with customerId: '{0}'", p_customerId),
                p_publicStr, p_privateStr, p_passwordStr
            );
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        private X509CertMgr m_aspSignCert = null;

        /// <summary>
        /// ASP/ERP 사업자의 전자서명용 인증서 (테스트 기간에는 진흥원에서 제공한 인증서 사용)
        /// </summary>
        public X509CertMgr AspSignCert
        {
            get
            {
                if (m_aspSignCert == null)
                {
                    string _publicFile = Path.Combine(USvcHelper.AspCertFolder, "signCert.der");
                    string _privatFile = Path.Combine(USvcHelper.AspCertFolder, "signPri.key");
                    string _password = USvcHelper.AspCertPassword;

                    m_aspSignCert = new X509CertMgr(_publicFile, _privatFile, _password);
                }

                return m_aspSignCert;
            }
        }

        private X509CertMgr m_aspKmCert = null;

        /// <summary>
        /// ASP/ERP 사업자의 암호화용 인증서, 국세청 사이트에 ASP사업자의 공개키를 등록 하면,
        /// 다른 사업자가 공개키로 암호화하여 메일 발송을 하게 되며,
        /// 개인키로 복호화 한다.
        /// </summary>
        public X509CertMgr AspKmCert
        {
            get
            {
                if (m_aspKmCert == null)
                {
                    string _publicFile = Path.Combine(USvcHelper.AspCertFolder, "kmCert.der");
                    string _privatFile = Path.Combine(USvcHelper.AspCertFolder, "kmPri.key");
                    string _password = USvcHelper.AspCertPassword;

                    m_aspKmCert = new X509CertMgr(_publicFile, _privatFile, _password);
                }

                return m_aspKmCert;
            }
        }

        private X509Certificate2 m_ntsPublicKey = null;

        /// <summary>
        /// 국세청에서 제공하는 암호화용 공캐키
        /// </summary>
        public X509Certificate2 NtsPublicKey
        {
            get
            {
                if (m_ntsPublicKey == null)
                {
                    var _publicFile = Path.Combine(USvcHelper.NtsCertFolder, "국세청.der");
                    m_ntsPublicKey = new X509Certificate2(_publicFile);

                    // DB에 저장 된 국세청 공개키 사용시 아래 문장을 사용 합니다.
                    //m_ntsPublicKey = GetProviderCertByProvider(UAppHelper.ReceiverBizNo);
                }

                return m_ntsPublicKey;
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
                if (m_qmaster != null)
                {
                    m_qmaster.Dispose();
                    m_qmaster = null;
                }
                if (m_svcHelper != null)
                {
                    m_svcHelper.Dispose();
                    m_svcHelper = null;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~UCertHelper()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}