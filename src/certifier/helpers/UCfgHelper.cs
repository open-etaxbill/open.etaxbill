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
using System.Configuration;
using System.IO;
using OdinSdk.OdinLib.Configuration;

namespace OpenETaxBill.Certifier
{
    /// <summary>
    /// 
    /// </summary>
    public class UCfgHelper : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private readonly static Lazy<UCfgHelper> m_lzayHelper = new Lazy<UCfgHelper>(() =>
        {
            return new UCfgHelper();
        });

        /// <summary>
        /// 
        /// </summary>
        public static UCfgHelper SNG
        {
            get
            {
                return m_lzayHelper.Value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        //private OpenETaxBill.Channel.Interface.ICollector m_icollector = null;
        //private OpenETaxBill.Channel.Interface.ICollector ICollector
        //{
        //    get
        //    {
        //        if (m_icollector == null)
        //            m_icollector = new OpenETaxBill.Channel.Interface.ICollector(false);

        //        return m_icollector;
        //    }
        //}

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_appkey"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public string GetAppValue(string p_appkey, string p_default = "")
        {
            if (String.IsNullOrEmpty(p_default) == true)
                p_default = ConfigurationManager.AppSettings[p_appkey];

            return RegHelper.SNG.GetClient("bizapp", "OpenTAX_Certifier", p_appkey, p_default);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private string m_keySize = "";
        public int KeySize
        {
            get
            {
                if (String.IsNullOrEmpty(m_keySize) == true)
                    m_keySize = GetAppValue("KeySize", "2048");

                return Convert.ToInt32(m_keySize);
            }
        }

        private string m_userCertPassword = "";

        /// <summary>
        /// 세금계산서를 발행하는 사업자의 인증서 암호 입니다.
        /// </summary>
        public string UserCertPassword
        {
            get
            {
                if (String.IsNullOrEmpty(m_userCertPassword) == true)
                    m_userCertPassword = GetAppValue("UserCertPassword", "p@ssw0rd");

                return m_userCertPassword;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // client only usage property, probably will remove.
        //-------------------------------------------------------------------------------------------------------------------------
        private string m_testerBizNo = "";
        public string InvoicerBizNo
        {
            get
            {
                if (String.IsNullOrEmpty(m_testerBizNo) == true)
                    m_testerBizNo = GetAppValue("InvoicerBizNo", "4445566666");

                return m_testerBizNo;
            }
        }

        private string m_root_folder = "";
        public string RootFolder
        {
            get
            {
                if (String.IsNullOrEmpty(m_root_folder) == true)
                {
                    m_root_folder = GetAppValue("RootFolder", @"C:\open-etaxbill");

                    if (Directory.Exists(m_root_folder) == false)
                        Directory.CreateDirectory(m_root_folder);
                }

                return m_root_folder;
            }
        }

        private static string m_work_folder = "";
        public string WorkFolder
        {
            get
            {
                if (String.IsNullOrEmpty(m_work_folder) == true)
                {
                    m_work_folder = Path.Combine(RootFolder, "work-folder");

                    if (Directory.Exists(m_work_folder) == false)
                        Directory.CreateDirectory(m_work_folder);
                }

                return m_work_folder;
            }
        }

        private string m_rootOutFolder = "";
        public string RootOutFolder
        {
            get
            {
                if (String.IsNullOrEmpty(m_rootOutFolder) == true)
                {
                    m_rootOutFolder = Path.Combine(WorkFolder, "output");

                    if (Directory.Exists(m_rootOutFolder) == false)
                        Directory.CreateDirectory(m_rootOutFolder);
                }

                return m_rootOutFolder;
            }
        }

        private string m_outputFolder = "";
        public string OutputFolder
        {
            get
            {
                if (String.IsNullOrEmpty(m_outputFolder) == true)
                {
                    m_outputFolder = Path.Combine(RootOutFolder, KeySize.ToString());

                    if (Directory.Exists(m_outputFolder) == false)
                        Directory.CreateDirectory(m_outputFolder);
                }

                return m_outputFolder;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private static string m_isLiveServer = null;

        /// <summary>
        /// live 서버에서 수행 되는지 여부를 설정 합니다.
        /// </summary>
        public bool LiveServer
        {
            get
            {
                if (String.IsNullOrEmpty(m_isLiveServer) == true)
                    m_isLiveServer = GetAppValue("LiveServer", "false");

                return m_isLiveServer.ToLower() == "true";
            }
        }

        private static string m_connection_string = "";
        public string ConnectionString
        {
            get
            {
                if (String.IsNullOrEmpty(m_connection_string) == true)
                    m_connection_string = GetAppValue("ConnectionString", "server=etax-db-server;uid=openetax;pwd=p@ssw0rd;database=OPEN-eTAX-V10");

                return m_connection_string;
            }
        }

        private static string m_endpoint_guid = "";
        public string EndpointGuid
        {
            get
            {
                if (String.IsNullOrEmpty(m_endpoint_guid) == true)
                    m_endpoint_guid = GetAppValue("EndpointGuid", "bfa1fab8-2fcf-4e41-b7c1-95577f106c43");

                return m_endpoint_guid;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_is_testing">인증 심사 중 인지 여부</param>
        /// <param name="p_is_interop">false이면 단위기능별검증, true이면 상호운용성 검증</param>
        /// <returns></returns>
        public string TaxInvoiceSubmitUrl(bool p_is_testing = false, bool p_is_interop = false)
        {
            var _result = "";

            if (p_is_testing == false)
            {
                if (p_is_interop == false)
                    _result = "http://www.taxcerti.or.kr/etax/mr/SubmitEtaxInvoiceService/" + EndpointGuid;            // 단위 기능별 검증
                else
                    _result = "http://www.taxcerti.or.kr/etax/er/SubmitEtaxInvoiceService/" + EndpointGuid;            // 상호 운용성 검즘
            }
            else
            {
                if (p_is_interop == false)
                    _result = "http://www.taxcerti.or.kr/etax/mr/CertSubmitEtaxInvoiceService/" + EndpointGuid;        // 단위 기능별 검증
                else
                    _result = "http://www.taxcerti.or.kr/etax/er/CertSubmitEtaxInvoiceService/" + EndpointGuid;        // 상호 운용성 검즘
            }

            return _result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_is_testing">인증 심사 중 인지 여부</param>
        /// <param name="p_is_interop">false이면 단위기능별검증, true이면 상호운용성 검증</param>
        /// <returns></returns>
        public string RequestResultsSubmitUrl(bool p_is_testing = false, bool p_is_interop = false)
        {
            var _result = "";

            if (p_is_testing == false)
            {
                if (p_is_interop == false)
                    _result = "http://www.taxcerti.or.kr/etax/mr/RequestResultsService/" + EndpointGuid;            // 단위 기능별 검증
                else
                    _result = "http://www.taxcerti.or.kr/etax/er/RequestResultsService/" + EndpointGuid;            // 상호 운용성 검즘
            }
            else
            {
                if (p_is_interop == false)
                    _result = "http://www.taxcerti.or.kr/etax/mr/CertRequestResultsService/" + EndpointGuid;        // 단위 기능별 검증
                else
                    _result = "http://www.taxcerti.or.kr/etax/er/CertRequestResultsService/" + EndpointGuid;        // 상호 운용성 검즘
            }

            return _result;
        }

        private static string m_requestCertUrl = "";
        public string RequestCertUrl
        {
            get
            {
                if (String.IsNullOrEmpty(m_requestCertUrl) == true)
                    m_requestCertUrl = GetAppValue("RequestCertUrl", "http://webservice.esero.go.kr/services/RequestCert");

                return m_requestCertUrl;
            }
        }

        private static string m_rootCertFolder = "";
        public string RootCertFolder
        {
            get
            {
                if (String.IsNullOrEmpty(m_rootCertFolder) == true)
                {
                    m_rootCertFolder = Path.Combine(WorkFolder, "certkey");

                    if (Directory.Exists(m_rootCertFolder) == false)
                        Directory.CreateDirectory(m_rootCertFolder);
                }

                return m_rootCertFolder;
            }
        }

        private static string m_aspCertFolder = "";
        public string AspCertFolder
        {
            get
            {
                if (String.IsNullOrEmpty(m_aspCertFolder) == true)
                {
                    m_aspCertFolder = Path.Combine(RootCertFolder, "ASP", KeySize.ToString());

                    if (Directory.Exists(m_aspCertFolder) == false)
                        Directory.CreateDirectory(m_aspCertFolder);
                }

                return m_aspCertFolder;
            }
        }

        private static string m_userCertFolder = "";
        public string UserCertFolder
        {
            get
            {
                if (String.IsNullOrEmpty(m_userCertFolder) == true)
                {
                    m_userCertFolder = Path.Combine(RootCertFolder, "USER", KeySize.ToString());

                    if (Directory.Exists(m_userCertFolder) == false)
                        Directory.CreateDirectory(m_userCertFolder);
                }

                return m_userCertFolder;
            }
        }

        private static string m_ntsCertFolder = "";
        public string NtsCertFolder
        {
            get
            {
                if (String.IsNullOrEmpty(m_ntsCertFolder) == true)
                {
                    m_ntsCertFolder = Path.Combine(RootCertFolder, "NTS", KeySize.ToString());

                    if (Directory.Exists(m_ntsCertFolder) == false)
                        Directory.CreateDirectory(m_ntsCertFolder);
                }

                return m_ntsCertFolder;
            }
        }

        private static string m_aspCertPassword = "";

        /// <summary>
        /// ASP 또는 ERP 사업자의 인증서 암호 입니다.
        /// </summary>
        public string AspCertPassword
        {
            get
            {
                if (String.IsNullOrEmpty(m_aspCertPassword) == true)
                    m_aspCertPassword = GetAppValue("AspCertPassword", "p@ssw0rd");

                return m_aspCertPassword;
            }
        }

        private static string m_senderBizNo = "";
        public string SenderBizNo
        {
            get
            {
                if (String.IsNullOrEmpty(m_senderBizNo) == true)
                    m_senderBizNo = GetAppValue("SenderBizNo", "1112233333");

                return m_senderBizNo;
            }
        }

        private static string m_senderBizName = "";
        public string SenderBizName
        {
            get
            {
                if (String.IsNullOrEmpty(m_senderBizName) == true)
                    m_senderBizName = GetAppValue("SenderBizName", "(주)전자세금테스트");

                return m_senderBizName;
            }
        }

        private static string m_receiverBizNo = "";
        public string ReceiverBizNo
        {
            get
            {
                if (String.IsNullOrEmpty(m_receiverBizNo) == true)
                    m_receiverBizNo = GetAppValue("ReceiverBizNo", "9999999999");

                return m_receiverBizNo;
            }
        }

        private static string m_receiverBizName = "";
        public string ReceiverBizName
        {
            get
            {
                if (String.IsNullOrEmpty(m_receiverBizName) == true)
                    m_receiverBizName = GetAppValue("ReceiverBizName", "국세청");

                return m_receiverBizName;
            }
        }

        private static string m_eTaxVersion = "";
        public string eTaxVersion
        {
            get
            {
                if (String.IsNullOrEmpty(m_eTaxVersion) == true)
                    m_eTaxVersion = GetAppValue("eTaxVersion", "3.0");

                return m_eTaxVersion;
            }
        }

        private static string m_registerId = "";
        public string RegisterId
        {
            get
            {
                if (String.IsNullOrEmpty(m_registerId) == true)
                    m_registerId = GetAppValue("RegisterId", "40000000");

                return m_registerId;
            }
        }

        private static string m_acceptedRequestUrl = "";
        public string AcceptedRequestUrl
        {
            get
            {
                if (String.IsNullOrEmpty(m_acceptedRequestUrl) == true)
                    m_acceptedRequestUrl = GetAppValue("AcceptedRequestUrl", "/ResultSubmit");

                return m_acceptedRequestUrl;
            }
        }

        public string ReplyAddress
        {
            get
            {
                return String.Format("http://{0}:{1}{2}", HostAddress, PortNumber, AcceptedRequestUrl);
            }
        }

        private static string m_hostAddress = "";
        public string HostAddress
        {
            get
            {
                if (String.IsNullOrEmpty(m_hostAddress) == true)
                    m_hostAddress = GetAppValue("HostAddress", "etax.domain.name");

                return m_hostAddress;
            }
        }

        private static string m_portNumber = "";
        public int PortNumber
        {
            get
            {
                if (String.IsNullOrEmpty(m_portNumber) == true)
                    m_portNumber = GetAppValue("PortNumber", "8080");

                return Convert.ToInt32(m_portNumber);
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
                //if (m_isigner != null)
                //{
                //    m_isigner.Dispose();
                //    m_isigner = null;
                //}
                //if (m_ccollector != null)
                //{
                //    m_ccollector.Dispose();
                //    m_ccollector = null;
                //}
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~UCfgHelper()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}