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
using System.Security.Cryptography.X509Certificates;
using OdinSdk.eTaxBill.Security.Signature;

namespace OpenETaxBill.Certifier
{
    /// <summary>
    /// 
    /// </summary>
    public class UCertHelper : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private readonly static Lazy<UCertHelper> m_lzayHelper = new Lazy<UCertHelper>(() =>
        {
            return new UCertHelper();
        });

        /// <summary>
        /// 
        /// </summary>
        public static UCertHelper SNG
        {
            get
            {
                return m_lzayHelper.Value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        private X509CertMgr m_userSignCert = null;

        /// <summary>
        /// 사용자의 전자서명용 인증서, 테스트시 ASP/ERP 사업자의 인증서와 동일한 것을 사용 하게 되며,
        /// 라이브 서비스에서는 DB에 저장된 고객(customer)위 인증서를 사용 한다.
        /// </summary>
        public X509CertMgr UserSignCert
        {
            get
            {
                if (m_userSignCert == null)
                {
                    string _publicFile = Path.Combine(UCfgHelper.SNG.UserCertFolder, "signCert.der");
                    string _privatFile = Path.Combine(UCfgHelper.SNG.UserCertFolder, "signPri.key");
                    string _password = UCfgHelper.SNG.UserCertPassword;

                    m_userSignCert = new X509CertMgr(_publicFile, _privatFile, _password);
                }

                return m_userSignCert;
            }
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
                    string _publicFile = Path.Combine(UCfgHelper.SNG.AspCertFolder, "signCert.der");
                    string _privatFile = Path.Combine(UCfgHelper.SNG.AspCertFolder, "signPri.key");
                    string _password = UCfgHelper.SNG.AspCertPassword;

                    m_aspSignCert = new X509CertMgr(_publicFile, _privatFile, _password);
                }

                return m_aspSignCert;
            }
        }

        private X509Certificate2 m_ntsPublicKey = null;

        /// <summary>
        /// 진흥원에서 제공하는 암호화용 공캐키
        /// </summary>
        public X509Certificate2 NtsPublicKey
        {
            get
            {
                if (m_ntsPublicKey == null)
                {
                    var _public_cert_file = Path.Combine(UCfgHelper.SNG.NtsCertFolder, "진흥원.der");
                    m_ntsPublicKey = new X509Certificate2(_public_cert_file);
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