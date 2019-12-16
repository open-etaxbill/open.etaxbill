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
using OdinSdk.OdinLib.Queue;

namespace OpenETaxBill.Engine.Library
{
    /// <summary>
    /// 
    /// </summary>
    public class UAppHelper : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_manager"></param>
        public UAppHelper(QService p_manager)
        {
            m_qmaster = (QService)p_manager.Clone();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_manager"></param>
        /// <param name="p_isService"></param>
        public UAppHelper(QService p_manager, bool p_isService)
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
        private OpenETaxBill.Channel.Interface.ICollector m_icollector = null;
        private OpenETaxBill.Channel.Interface.ICollector ICollector
        {
            get
            {
                if (m_icollector == null)
                    m_icollector = new OpenETaxBill.Channel.Interface.ICollector(false);

                return m_icollector;
            }
        }

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
            lock (QMaster)
            {
                if (String.IsNullOrEmpty(p_default) == true)
                    p_default = ConfigurationManager.AppSettings[p_appkey];

                return RegHelper.SNG.GetServer(ICollector.Manager.CategoryId, ICollector.Manager.ProductId, p_appkey, p_default);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        private static string m_isLiveServer = null;

        /// <summary>
        /// LIVE 서버에서 수행 되는지 여부를 설정 합니다.
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

        private static string m_isUpdateCert = null;

        /// <summary>
        /// LIVE 서버에서 수행 되는지 여부를 설정 합니다.
        /// </summary>
        public bool UpdateCert
        {
            get
            {
                if (String.IsNullOrEmpty(m_isUpdateCert) == true)
                    m_isUpdateCert = GetAppValue("UpdateCert", "false");

                return m_isUpdateCert.ToLower() == "true";
            }
        }

        private static string m_isMailSniffing = null;
        public bool MailSniffing
        {
            get
            {
                if (String.IsNullOrEmpty(m_isMailSniffing) == true)
                    m_isMailSniffing = GetAppValue("MailSniffing", "false");

                return m_isMailSniffing.ToLower() == "true";
            }
        }

        private static string m_isSoapFiltering = null;
        public bool SoapFiltering
        {
            get
            {
                if (String.IsNullOrEmpty(m_isSoapFiltering) == true)
                    m_isSoapFiltering = GetAppValue("SoapFiltering", "false");

                return m_isSoapFiltering.ToLower() == "true";
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
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

        private static string m_tax_invoice_submit_url = "";
        public string TaxInvoiceSubmitUrl
        {
            get
            {
                if (String.IsNullOrEmpty(m_tax_invoice_submit_url) == true)
                        m_tax_invoice_submit_url = GetAppValue("TaxInvoiceSubmitUrl", "http://webservice.esero.go.kr/services/SubmitEtaxInvoice");

                return m_tax_invoice_submit_url;
            }
        }

        private static string m_request_results_submit_url = "";
        public string RequestResultsSubmitUrl
        {
            get
            {
                if (String.IsNullOrEmpty(m_request_results_submit_url) == true)
                        m_request_results_submit_url = GetAppValue("ReqResultsSubmitUrl", "http://webservice.esero.go.kr/services/RequestResults");

                return m_request_results_submit_url;
            }
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

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private static string m_resultRecvAckToAddress = "";
        public string ResultRecvAckToAddress
        {
            get
            {
                if (String.IsNullOrEmpty(m_resultRecvAckToAddress) == true)
                    m_resultRecvAckToAddress = GetAppValue("ResultRecvAckToAddress", "http://webservice.esero.go.kr/services");

                return m_resultRecvAckToAddress;
            }
        }

        private static string m_acceptedRequestUrl = "";
        public string AcceptedRequestUrl
        {
            get
            {
                if (String.IsNullOrEmpty(m_acceptedRequestUrl) == true)
                    m_acceptedRequestUrl = GetAppValue("AcceptedRequestUrl", "/ResultSubmit");

                return m_acceptedRequestUrl.ToLower();
            }
        }

        public string ReplyAddress
        {
            get
            {
                return String.Format("http://{0}:{1}{2}", HostAddress, PortNumber, AcceptedRequestUrl);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------

        private static string m_dnsServers = "";
        public string[] DnsServers
        {
            get
            {
                if (String.IsNullOrEmpty(m_dnsServers) == true)
                    m_dnsServers = GetAppValue("DnsServers", "8.8.8.8;8.8.4.4");

                return m_dnsServers.Split(';');
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

        public string WebSiteUrl
        {
            get
            {
                return String.Format(@"http://{0}/", HostAddress);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private static string m_defaultPage = "";
        public string DefaultPage
        {
            get
            {
                if (String.IsNullOrEmpty(m_defaultPage) == true)
                    m_defaultPage = GetAppValue("DefaultPage", "default.htm");

                return m_defaultPage;
            }
        }

        private static string m_senderBizNo = "";

        /// <summary>
        /// ASP/ERP 사업자 번호
        /// </summary>
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
                    m_senderBizName = GetAppValue("SenderBizName", "(주)세금계산서라이브");

                return m_senderBizName;
            }
        }

        private static string m_receiverBizNo = "";

        /// <summary>
        /// 국세청 사업자 번호 - "9999999999"
        /// </summary>
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

        private static string m_officeAddress = "";
        public string OfficeAddress
        {
            get
            {
                if (String.IsNullOrEmpty(m_officeAddress) == true)
                    m_officeAddress = GetAppValue("OfficeAddress", @"(우)05855 서울시 송파구 법원로8길 13, 문정헤리움써밋타워 1317호 Tel : 02-6959-3790");

                return m_officeAddress;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // for provider service  
        //-------------------------------------------------------------------------------------------------------------------------
        private static string m_root_folder = "";
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

        private static string m_webFolder = "";
        public string WebFolder
        {
            get
            {
                if (String.IsNullOrEmpty(m_webFolder) == true)
                {
                    m_webFolder = Path.Combine(RootFolder, "web-folder");

                    if (Directory.Exists(m_webFolder) == false)
                        Directory.CreateDirectory(m_webFolder);
                }

                return m_webFolder;
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

        private static string m_root_in_folder = "";
        public string RootInFolder
        {
            get
            {
                if (String.IsNullOrEmpty(m_root_in_folder) == true)
                {
                    m_root_in_folder = Path.Combine(WorkFolder, "input");

                    if (Directory.Exists(m_root_in_folder) == false)
                        Directory.CreateDirectory(m_root_in_folder);
                }

                return m_root_in_folder;
            }
        }

        private static string m_eMailFolder = "";
        public string eMailFolder
        {
            get
            {
                if (String.IsNullOrEmpty(m_eMailFolder) == true)
                {
                    m_eMailFolder = Path.Combine(RootInFolder, "eMail");

                    if (Directory.Exists(m_eMailFolder) == false)
                        Directory.CreateDirectory(m_eMailFolder);
                }

                return m_eMailFolder;
            }
        }


        private static string m_nts_folder = "";
        public string NTSFolder
        {
            get
            {
                if (String.IsNullOrEmpty(m_nts_folder) == true)
                {
                    m_nts_folder = Path.Combine(RootInFolder, "NTS");

                    if (Directory.Exists(m_nts_folder) == false)
                        Directory.CreateDirectory(m_nts_folder);
                }

                return m_nts_folder;
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

        private static string m_keySize = "";
        public int KeySize
        {
            get
            {
                if (String.IsNullOrEmpty(m_keySize) == true)
                    m_keySize = GetAppValue("KeySize", "2048");

                return Convert.ToInt32(m_keySize);
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

        //-------------------------------------------------------------------------------------------------------------------------
        // duration
        //-------------------------------------------------------------------------------------------------------------------------
        private static string m_collectorDueTime = "";
        public int CollectorDueTime
        {
            get
            {
                if (String.IsNullOrEmpty(m_collectorDueTime) == true)
                    m_collectorDueTime = GetAppValue("CollectorDueTime", "24");

                return Convert.ToInt32(m_collectorDueTime) * 1000 * 60 * 60;        // seconds
            }
        }

        private static string m_signerDueTime = "";
        public int SignerDueTime
        {
            get
            {
                if (String.IsNullOrEmpty(m_signerDueTime) == true)
                    m_signerDueTime = GetAppValue("SignerDueTime", "1");

                return Convert.ToInt32(m_signerDueTime) * 1000 * 60 * 60;        // seconds
            }
        }

        private static string m_reporterDueTime = "";
        public int ReporterDueTime
        {
            get
            {
                if (String.IsNullOrEmpty(m_reporterDueTime) == true)
                    m_reporterDueTime = GetAppValue("ReporterDueTime", "1");

                return Convert.ToInt32(m_reporterDueTime) * 1000 * 60 * 60;        // seconds
            }
        }

        private static string m_mailerDueTime = "";

        /// <summary>
        /// 
        /// </summary>
        public int MailerDueTime
        {
            get
            {
                if (String.IsNullOrEmpty(m_mailerDueTime) == true)
                    m_mailerDueTime = GetAppValue("MailerDueTime", "1");

                return Convert.ToInt32(m_mailerDueTime) * 1000 * 60 * 60;        // seconds
            }
        }

        private static string m_availablePeriod = "";

        /// <summary>
        /// 
        /// </summary>
        public int AvailablePeriod
        {
            get
            {
                if (String.IsNullOrEmpty(m_availablePeriod) == true)
                    m_availablePeriod = GetAppValue("AvailablePeriod", "35");

                return Convert.ToInt32(m_availablePeriod);         // days
            }
        }

        private static string m_noThreadOfReporter = "";

        /// <summary>
        /// 
        /// </summary>
        public int NoThreadOfReporter
        {
            get
            {
                if (String.IsNullOrEmpty(m_noThreadOfReporter) == true)
                    m_noThreadOfReporter = GetAppValue("NoThreadOfReporter", "32");

                return Convert.ToInt32(m_noThreadOfReporter);         // number of thread for reporting
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
                if (m_icollector != null)
                {
                    m_icollector.Dispose();
                    m_icollector = null;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~UAppHelper()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}
