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
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using OdinSoft.SDK.eTaxBill.Security.Encrypt;
using OdinSoft.SDK.eTaxBill.Security.Issue;
using OdinSoft.SDK.eTaxBill.Security.Mime;
using OdinSoft.SDK.eTaxBill.Security.Notice;
using OdinSoft.SDK.eTaxBill.Utility;
using OpenETaxBill.Engine.Library;
using OdinSoft.SDK.Configuration;
using OdinSoft.SDK.Data;
using OdinSoft.SDK.Data.Collection;

namespace OpenETaxBill.Engine.Collector
{
    /// <summary>
    /// 
    /// </summary>
    public class Engine : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
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

        private OpenETaxBill.Channel.Interface.ICollector m_icollector = null;
        private OpenETaxBill.Channel.Interface.ICollector ICollector
        {
            get
            {
                if (m_icollector == null)
                    m_icollector = new OpenETaxBill.Channel.Interface.ICollector();

                return m_icollector;
            }
        }
        
        private OpenETaxBill.Engine.Library.UAppHelper m_appHelper = null;
        public OpenETaxBill.Engine.Library.UAppHelper UAppHelper
        {
            get
            {
                if (m_appHelper == null)
                    m_appHelper = new OpenETaxBill.Engine.Library.UAppHelper(ICollector.Manager);

                return m_appHelper;
            }
        }

        private OpenETaxBill.Engine.Library.UCertHelper m_certHelper = null;
        public OpenETaxBill.Engine.Library.UCertHelper UCertHelper
        {
            get
            {
                if (m_certHelper == null)
                    m_certHelper = new OpenETaxBill.Engine.Library.UCertHelper(ICollector.Manager);

                return m_certHelper;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private DataSet m_InvoinceSet = null;
        private DataSet g_InvoinceSet
        {
            get
            {
                if (m_InvoinceSet == null)
                    m_InvoinceSet = Schema.SNG.GetTaxSchema();

                return m_InvoinceSet;
            }
        }

        private DataTable m_invoiceTbl = null;
        private DataTable g_invoiceTbl
        {
            get
            {
                if (m_invoiceTbl == null)
                    m_invoiceTbl = g_InvoinceSet.Tables["TB_eTAX_INVOICE"];

                return m_invoiceTbl;
            }
        }

        private DataTable m_lineitemTbl = null;
        private DataTable g_lineitemTbl
        {
            get
            {
                if (m_lineitemTbl == null)
                    m_lineitemTbl = g_InvoinceSet.Tables["TB_eTAX_LINEITEM"];

                return m_lineitemTbl;
            }
        }

        private DataTable m_customerTbl = null;
        private DataTable g_customerTbl
        {
            get
            {
                if (m_customerTbl == null)
                    m_customerTbl = g_InvoinceSet.Tables["TB_eTAX_CUSTOMER"];

                return m_customerTbl;
            }
        }

        private DataTable m_partnerTbl = null;
        private DataTable g_partnerTbl
        {
            get
            {
                if (m_partnerTbl == null)
                    m_partnerTbl = g_InvoinceSet.Tables["TB_eTAX_PARTNER"];

                return m_partnerTbl;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private readonly static Object SyncEngine = new Object();

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        public bool LogCommands
        {
            get
            {
                return CfgHelper.SNG.DebugMode;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_appkey"></param>
        /// <param name="p_default"></param>
        /// <returns></returns>
        public string GetCfgValue(string p_appkey, string p_default)
        {
            return UAppHelper.GetAppValue(p_appkey, p_default);
        }
        
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private IDictionary<string, Int32> m_saveIssueIds = null;

        private MimeContent RequestCert()
        {
            //-------------------------------------------------------------------------------------------------------------------//
            // SOAP Envelope
            //-------------------------------------------------------------------------------------------------------------------//
            Header _soapHeader = new Header();
            {
                _soapHeader.ToAddress = UAppHelper.RequestCertUrl;
                _soapHeader.Action = Request.eTaxRequestCertSubmit;
                _soapHeader.Version = UAppHelper.eTaxVersion;

                _soapHeader.FromParty = new Party(UAppHelper.SenderBizNo, UAppHelper.SenderBizName);
                _soapHeader.ToParty = new Party(UAppHelper.ReceiverBizNo, UAppHelper.ReceiverBizName);

                _soapHeader.OperationType = Request.OperationType_RequestSubmit;
                _soapHeader.MessageType = Request.MessageType_Request;

                _soapHeader.TimeStamp = DateTime.Now;
            }

            Body _soapBody = new Body();
            {
                _soapBody.RequestParty = new Party(UAppHelper.SenderBizNo, UAppHelper.SenderBizName, UAppHelper.RegisterId);
                _soapBody.FileType = OdinSoft.SDK.eTaxBill.Security.Notice.Request.FileType_ZIP;
            }

            //-------------------------------------------------------------------------------------------------------------------//
            // SOAP Signature
            //-------------------------------------------------------------------------------------------------------------------//
            XmlDocument _signedXml = Packing.SNG.GetSignedSoapEnvelope(null, UCertHelper.AspSignCert.X509Cert2, _soapHeader, _soapBody);

            //-------------------------------------------------------------------------------------------------------------------//
            // Request
            //-------------------------------------------------------------------------------------------------------------------//
            return Request.SNG.TaxRequestCertSubmit(
                            Encoding.UTF8.GetBytes(_signedXml.OuterXml),
                            UAppHelper.RequestCertUrl
                        );
        }
        
        //-------------------------------------------------------------------------------------------------------------------------
        // Public functions - 파일을 저장한다. 1000개까지 처리한다.
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 엑셀에서 추출한 테이블을 저장한다. 1,000개까지 처리한다. 
        /// </summary>
        /// <param name="p_uploadTable">Upload Excel Table</param>
        /// <param name="p_createdBy">created id</param>
        /// <returns></returns>
        public bool DoExcelUpload(DataTable p_uploadTable, string p_createdBy)
        {
            ICollector.WriteDebug(p_createdBy);

            var _result = false;

            try
            {
                if (p_uploadTable.Rows.Count > 1000)
                    throw new ProxyException(String.Format("Number of records can not exceed 1,000: '{0}'", p_uploadTable.Rows.Count));

                // 데이터 Clear..
                g_InvoinceSet.Clear();
                g_invoiceTbl.Clear();
                g_lineitemTbl.Clear();
                g_customerTbl.Clear();
                g_partnerTbl.Clear();

                foreach (DataRow _row in p_uploadTable.Rows)
                {
                    string _IssueId = GetIssueId(Convert.ToDateTime(_row[1]));

                    // TB_eTAX_INVOICE 테이블에 입력
                    DataRow _invoiceRow = g_invoiceTbl.NewRow();
                    DataRow _customerRow = g_customerTbl.NewRow();
                    DataRow _partnerRow = g_partnerTbl.NewRow();

                    _invoiceRow["exchangeId"] = _IssueId;
                    _invoiceRow["exchangeDate"] = Convert.ToDateTime(_row[1]);
                    _invoiceRow["isIssued"] = "F";
                    _invoiceRow["isSuccess"] = "F";
                    _invoiceRow["refIssueId"] = "";
                    _invoiceRow["creator"] = p_createdBy;

                    _invoiceRow["issueId"] = _IssueId;
                    _invoiceRow["issueDate"] = Convert.ToDateTime(_row[1]);

                    _invoiceRow["typeCode"] = "01" + Convert.ToString(_row[0]);
                    _invoiceRow["purposeCode"] = Convert.ToString(_row[58]);
                    _invoiceRow["amendmentCode"] = "";
                    _invoiceRow["description"] = Convert.ToString(_row[21]);

                    _invoiceRow["importId"] = "";
                    _invoiceRow["importQuantity"] = Convert.ToDecimal("0");

                    _invoiceRow["invoicerId"] = Convert.ToString(_row[2]);
                    _invoiceRow["invoicerOrgId"] = Convert.ToString(_row[3]);
                    _invoiceRow["invoicerName"] = Convert.ToString(_row[4]);
                    _invoiceRow["invoicerPerson"] = Convert.ToString(_row[5]);
                    _invoiceRow["invoicerAddress"] = Convert.ToString(_row[6]);
                    _invoiceRow["invoicerType"] = Convert.ToString(_row[7]);
                    _invoiceRow["invoicerClass"] = Convert.ToString(_row[8]);
                    _invoiceRow["invoicerDepartment"] = "";
                    _invoiceRow["invoicerContactor"] = "";
                    _invoiceRow["invoicerPhone"] = "";
                    _invoiceRow["invoicerEMail"] = Convert.ToString(_row[9]);
                    _invoiceRow["invoiceeId"] = Convert.ToString(_row[10]);

                    if (_row[10].ToString() == "9999999999999")
                        _invoiceRow["invoiceeKind"] = "03";
                    else if (_row[10].ToString().Length == 10)
                        _invoiceRow["invoiceeKind"] = "01";
                    else
                        _invoiceRow["invoiceeKind"] = "02";

                    _invoiceRow["invocieeOrgId"] = Convert.ToString(_row[11]);
                    _invoiceRow["invoiceeName"] = Convert.ToString(_row[12]);
                    _invoiceRow["invoiceePerson"] = Convert.ToString(_row[13]);
                    _invoiceRow["invoiceeAddress"] = Convert.ToString(_row[14]);
                    _invoiceRow["invoiceeType"] = Convert.ToString(_row[15]);
                    _invoiceRow["invoiceeClass"] = Convert.ToString(_row[16]);
                    _invoiceRow["invoiceeDepartment1"] = "";
                    _invoiceRow["invoiceeContactor1"] = "";
                    _invoiceRow["invoiceePhone1"] = "";
                    _invoiceRow["invoiceeEMail1"] = Convert.ToString(_row[17]);
                    _invoiceRow["invoiceeDepartment2"] = "";
                    _invoiceRow["invoiceeContactor2"] = "";
                    _invoiceRow["invoiceePhone2"] = "";
                    _invoiceRow["invoiceeEMail2"] = Convert.ToString(_row[18]);                       

                    _invoiceRow["brokerId"] = "";
                    _invoiceRow["brokerOrgId"] = "";
                    _invoiceRow["brokerName"] = "";
                    _invoiceRow["brokerPerson"] = "";
                    _invoiceRow["brokerAddress"] = "";
                    _invoiceRow["brokerType"] = "";
                    _invoiceRow["brokerClass"] = "";
                    _invoiceRow["brokerDepartment"] = "";
                    _invoiceRow["brokerContactor"] = "";
                    _invoiceRow["brokerPhone"] = "";
                    _invoiceRow["brokerEMail"] = "";
                    _invoiceRow["isUploaded"] = "T";

                    _invoiceRow["paidCash"] = Convert.ToDecimal(_row[54]);
                    _invoiceRow["paidCheck"] = Convert.ToDecimal(_row[55]);
                    _invoiceRow["paidNote"] = Convert.ToDecimal(_row[56]);
                    _invoiceRow["paidCredit"] = Convert.ToDecimal(_row[57]);
                    _invoiceRow["chargeTotal"] = Convert.ToDecimal(_row[19]);
                    _invoiceRow["taxTotal"] = Convert.ToDecimal(_row[20]);
                    _invoiceRow["grandTotal"] = Convert.ToDecimal(_row[19]) + Convert.ToDecimal(_row[20]);

                    // TB_eTAX_CUSTOMER 정보를 넣는다.
                    string _filterCustomer = String.Format("customerId='{0}'", Convert.ToString(_row[10]));
                    DataRow[] _oldcustomerow = g_customerTbl.Select(_filterCustomer);
                    if (_oldcustomerow.Length == 0)
                    {                        
                        _customerRow["customerId"] = Convert.ToString(_row[10]);

                        if (_row[10].ToString() == "9999999999999")
                            _customerRow["kind"] = "03";
                        else if (_row[10].ToString().Length == 10)
                            _customerRow["kind"] = "01";
                        else
                            _customerRow["kind"] = "02";

                        _customerRow["name"] = Convert.ToString(_row[12]);
                        _customerRow["person"] = Convert.ToString(_row[13]);
                        _customerRow["address"] = Convert.ToString(_row[14]);
                        _customerRow["type"] = Convert.ToString(_row[15]);
                        _customerRow["class"] = Convert.ToString(_row[16]);
                        _customerRow["department1"] = "";
                        _customerRow["contactor1"] = "";
                        _customerRow["phone1"] = "";
                        _customerRow["eMail1"] = Convert.ToString(_row[17]);
                        _customerRow["department2"] = "";
                        _customerRow["contactor2"] = "";
                        _customerRow["phone2"] = "";
                        _customerRow["eMail2"] = Convert.ToString(_row[18]);

                        _customerRow["bizregAttach"] = "F";
                        _customerRow["bankbookAttach"] = "F";
                        _customerRow["providerId"] = "";
                        _customerRow["headOffice"] = "";
                        _customerRow["taxRegId"] = "";
                        
                        _customerRow["closingDay"] = 1;

                        _customerRow["signingType"] = "02";
                        _customerRow["signFromDay"] = 1;
                        _customerRow["signTillDay"] = 31;
                        
                        _customerRow["sendingType"] = "02";
                        _customerRow["sendFromDay"] = 1;
                        _customerRow["sendTillDay"] = UTextHelper.SNG.SigningDay;
                        
                        _customerRow["reportingType"] = "02";
                        _customerRow["reportFromDay"] = 1;
                        _customerRow["reportTillDay"] = UTextHelper.SNG.ReportingDay;
                        _customerRow["reportCondition"] = "01";

                        g_customerTbl.Rows.Add(_customerRow);
                    }

                    // TB_eTAX_PARTNER 정보를 넣는다.
                    string _filterPartner = String.Format("userId='{0}' AND customerId='{1}'", p_createdBy, Convert.ToString(_row[10]));
                    DataRow[] _oldpartnerRow = g_partnerTbl.Select(_filterPartner);
                    if (_oldpartnerRow.Length == 0)
                    {
                        _partnerRow["userId"] = p_createdBy;
                        _partnerRow["customerId"] = Convert.ToString(_row[10]);
                        _partnerRow["name"] = Convert.ToString(_row[12]);
                        _partnerRow["person"] = Convert.ToString(_row[13]);
                        _partnerRow["address"] = Convert.ToString(_row[14]);
                        _partnerRow["type"] = Convert.ToString(_row[15]);
                        _partnerRow["class"] = Convert.ToString(_row[16]);
                        _partnerRow["department1"] = "";
                        _partnerRow["contactor1"] = "";
                        _partnerRow["phone1"] = "";
                        _partnerRow["eMail1"] = Convert.ToString(_row[17]);
                        _partnerRow["department2"] = "";
                        _partnerRow["contactor2"] = "";
                        _partnerRow["phone2"] = "";
                        _partnerRow["eMail2"] = Convert.ToString(_row[18]);

                        g_partnerTbl.Rows.Add(_partnerRow);
                    }

                    g_invoiceTbl.Rows.Add(_invoiceRow);

                    // TB_eTAX_LINEITEM 1Row 입력
                    if (String.IsNullOrEmpty(_row[27].ToString()) == false && Convert.ToDecimal(_row[27].ToString()) > 0)
                    {
                        DataRow _lineitemRow = g_lineitemTbl.NewRow();

                        _lineitemRow["issueId"] = _IssueId;
                        _lineitemRow["seqNo"] = 1;
                        _lineitemRow["purchaseDate"] = Convert.ToDateTime(Convert.ToDateTime(_row[1]).ToString("yyyy-MM-") + Convert.ToInt32(_row[22]).ToString("00"));
                        _lineitemRow["itemName"] = Convert.ToString(_row[23]);
                        _lineitemRow["information"] = Convert.ToString(_row[24]);
                        _lineitemRow["quantity"] = Convert.ToDecimal(_row[25]);
                        _lineitemRow["unitPrice"] = Convert.ToDecimal(_row[26]);
                        _lineitemRow["invoiceAmount"] = Convert.ToDecimal(_row[27]);
                        _lineitemRow["taxAmount"] = Convert.ToDecimal(_row[28]);
                        _lineitemRow["description"] = Convert.ToString(_row[29]);

                        g_lineitemTbl.Rows.Add(_lineitemRow);
                    }

                    // TB_eTAX_LINEITEM 2Row 입력
                    if (String.IsNullOrEmpty(_row[35].ToString()) == false && Convert.ToDecimal(_row[35].ToString()) > 0)
                    {
                        DataRow _lineitemRow = g_lineitemTbl.NewRow();

                        _lineitemRow["issueId"] = _IssueId;
                        _lineitemRow["seqNo"] = 2;
                        _lineitemRow["purchaseDate"] = Convert.ToDateTime(Convert.ToDateTime(_row[1]).ToString("yyyy-MM-") + Convert.ToInt32(_row[30]).ToString("00"));
                        _lineitemRow["itemName"] = Convert.ToString(_row[31]);
                        _lineitemRow["information"] = Convert.ToString(_row[32]);
                        _lineitemRow["quantity"] = Convert.ToDecimal(_row[33]);
                        _lineitemRow["unitPrice"] = Convert.ToDecimal(_row[34]);
                        _lineitemRow["invoiceAmount"] = Convert.ToDecimal(_row[35]);
                        _lineitemRow["taxAmount"] = Convert.ToDecimal(_row[36]);
                        _lineitemRow["description"] = Convert.ToString(_row[37]);

                        g_lineitemTbl.Rows.Add(_lineitemRow);
                    }

                    // TB_eTAX_LINEITEM 3Row 입력
                    if (String.IsNullOrEmpty(_row[43].ToString()) == false && Convert.ToDecimal(_row[43].ToString()) > 0)
                    {
                        DataRow _lineitemRow = g_lineitemTbl.NewRow();

                        _lineitemRow["issueId"] = _IssueId;
                        _lineitemRow["seqNo"] = 3;
                        _lineitemRow["purchaseDate"] = Convert.ToDateTime(Convert.ToDateTime(_row[1]).ToString("yyyy-MM-") + Convert.ToInt32(_row[38]).ToString("00"));
                        _lineitemRow["itemName"] = Convert.ToString(_row[39]);
                        _lineitemRow["information"] = Convert.ToString(_row[40]);
                        _lineitemRow["quantity"] = Convert.ToDecimal(_row[41]);
                        _lineitemRow["unitPrice"] = Convert.ToDecimal(_row[42]);
                        _lineitemRow["invoiceAmount"] = Convert.ToDecimal(_row[43]);
                        _lineitemRow["taxAmount"] = Convert.ToDecimal(_row[44]);
                        _lineitemRow["description"] = Convert.ToString(_row[45]);

                        g_lineitemTbl.Rows.Add(_lineitemRow);
                    }

                    // TB_eTAX_LINEITEM 4Row 입력
                    if (String.IsNullOrEmpty(_row[51].ToString()) == false && Convert.ToDecimal(_row[51].ToString()) > 0)
                    {
                        DataRow _lineitemRow = g_lineitemTbl.NewRow();

                        _lineitemRow["issueId"] = _IssueId;
                        _lineitemRow["seqNo"] = 4;
                        _lineitemRow["purchaseDate"] = Convert.ToDateTime(Convert.ToDateTime(_row[1]).ToString("yyyy-MM-") + Convert.ToInt32(_row[46]).ToString("00"));
                        _lineitemRow["itemName"] = Convert.ToString(_row[47]);
                        _lineitemRow["information"] = Convert.ToString(_row[48]);
                        _lineitemRow["quantity"] = Convert.ToDecimal(_row[49]);
                        _lineitemRow["unitPrice"] = Convert.ToDecimal(_row[50]);
                        _lineitemRow["invoiceAmount"] = Convert.ToDecimal(_row[51]);
                        _lineitemRow["taxAmount"] = Convert.ToDecimal(_row[52]);
                        _lineitemRow["description"] = Convert.ToString(_row[53]);

                        g_lineitemTbl.Rows.Add(_lineitemRow);
                    }
                }

                LDltaHelper.InsertDeltaTbl(UAppHelper.ConnectionString, g_customerTbl);
                LDltaHelper.InsertDeltaTbl(UAppHelper.ConnectionString, g_partnerTbl);
                LDltaHelper.InsertDeltaTbl(UAppHelper.ConnectionString, g_invoiceTbl);
                LDltaHelper.InsertDeltaTbl(UAppHelper.ConnectionString, g_lineitemTbl);

                _result = true;

            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }

            return _result;
        }

        /// <summary>
        /// 선택한 일자의 IssueId를 구한다.
        /// </summary>
        /// <param name="p_createDate"></param>
        /// <returns></returns>
        public string GetIssueId(DateTime p_createDate)
        {
            ICollector.WriteDebug(p_createDate.ToString());

            Int32 _maxIssueId = 1;

            if (m_saveIssueIds == null)
                m_saveIssueIds = new Dictionary<string, Int32>();

            string _issueDay = p_createDate.ToString("yyyyMMdd");
            if (m_saveIssueIds.ContainsKey(_issueDay) == false)
            {
                string _fromId = String.Format("{0}{1}{2:D8}", _issueDay, UAppHelper.RegisterId, 0);
                string _tillId = String.Format("{0}{1}{2:D8}", _issueDay, UAppHelper.RegisterId, 99999999);

                string _sqlstr
                    = "SELECT ISNULL(MAX(CONVERT(decimal(8), RIGHT(issueId, 8))), 0) as maxSeqNo "
                    + "  FROM TB_eTAX_INVOICE "
                    + " WHERE issueId>=@fromId AND issueId<=@tillId";

                var _dbps = new DatParameters();
                _dbps.Add("@fromId", SqlDbType.NVarChar, _fromId);
                _dbps.Add("@tillId", SqlDbType.NVarChar, _tillId);

                var _ds = LDataHelper.SelectDataSet(UAppHelper.ConnectionString, _sqlstr, _dbps);
                if (_ds.Tables[0].Rows.Count > 0)
                    _maxIssueId = Convert.ToInt32(_ds.Tables[0].Rows[0]["maxSeqNo"]) + 1;

                m_saveIssueIds.Add(_issueDay, _maxIssueId);
            }
            else
            {
                _maxIssueId = m_saveIssueIds[_issueDay];
                
                _maxIssueId++;

                m_saveIssueIds[_issueDay] = _maxIssueId;
            }

            return String.Format("{0}{1}{2:D8}", _issueDay, UAppHelper.RegisterId, _maxIssueId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int DoUpdateCert()
        {
            ICollector.WriteDebug("");

            int _result = 0;

            MimeContent _certContent = RequestCert();
            if (_certContent.Parts.Count < 2 || _certContent.StatusCode != 0)
                throw new CollectException(_certContent.ErrorMessage);

            ZipInputStream _izipStream = new ZipInputStream(_certContent.Parts[1].GetContentAsStream());

            ZipEntry _izipEntry;
            while ((_izipEntry = _izipStream.GetNextEntry()) != null)
            {
                if (_izipEntry.Name.IndexOf(".ini") >= 0)
                    continue;

                MemoryStream _ostream = new MemoryStream();
                {
                    int _size = 2048;
                    byte[] _obuffer = new byte[_size];

                    while (true)
                    {
                        _size = _izipStream.Read(_obuffer, 0, _obuffer.Length);
                        if (_size <= 0)
                            break;

                        _ostream.Write(_obuffer, 0, _size);
                    }

                    _ostream.Seek(0, SeekOrigin.Begin);
                }

                string _fileName = Path.GetFileNameWithoutExtension(_izipEntry.Name);

                string _registerid = _fileName.Substring(0, 8);
                string _newEMail = _fileName.Substring(9);

                byte[] _publicBytes = _ostream.ToArray();
                string _publicStr = Encryptor.SNG.PlainBytesToChiperBase64(_publicBytes);

                X509Certificate2 _publicCert2 = new X509Certificate2(_publicBytes);
                DateTime _expiration = Convert.ToDateTime(_publicCert2.GetExpirationDateString());

                string _userName = _publicCert2.GetNameInfo(X509NameType.SimpleName, false);

                string _sqlstr
                    = "SELECT publicKey, aspEMail "
                    + "  FROM TB_eTAX_PROVIDER "
                    + " WHERE registerId=@registerId AND aspEMail=@aspEMail";

                var _dbps = new DatParameters();
                {
                    _dbps.Add("@registerId", SqlDbType.NVarChar, _registerid);
                    _dbps.Add("@aspEMail", SqlDbType.NVarChar, _newEMail);
                }

                var _ds = LDataHelper.SelectDataSet(UAppHelper.ConnectionString, _sqlstr, _dbps);
                if (LDataHelper.IsNullOrEmpty(_ds) == true)
                {
                    _sqlstr
                        = "INSERT TB_eTAX_PROVIDER "
                        + "( "
                        + " registerId, aspEMail, name, person, publicKey, userName, expiration, lastUpdate, providerId "
                        + ") "
                        + "VALUES "
                        + "( "
                        + " @registerId, @aspEMail, @name, @person, @publicKey, @userName, @expiration, @lastUpdate, @providerId "
                        + ")";

                    _dbps.Add("@registerId", SqlDbType.NVarChar, _registerid);
                    _dbps.Add("@aspEMail", SqlDbType.NVarChar, _newEMail);
                    _dbps.Add("@name", SqlDbType.NVarChar, _userName);
                    _dbps.Add("@person", SqlDbType.NVarChar, "");
                    _dbps.Add("@publicKey", SqlDbType.NVarChar, _publicStr);
                    _dbps.Add("@userName", SqlDbType.NVarChar, _userName);
                    _dbps.Add("@expiration", SqlDbType.DateTime, _expiration);
                    _dbps.Add("@lastUpdate", SqlDbType.DateTime, DateTime.Now);
                    _dbps.Add("@providerId", SqlDbType.NVarChar, "");

                    if (LDataHelper.ExecuteText(UAppHelper.ConnectionString, _sqlstr, _dbps) < 1)
                    {
                        if (LogCommands == true)
                            ELogger.SNG.WriteLog(String.Format("INSERT FAILURE: {0}, {1}, {2}, {3}", _userName, _registerid, _newEMail, _expiration));
                    }
                    else
                    {
                        if (LogCommands == true)
                            ELogger.SNG.WriteLog(String.Format("INSERT SUCCESS: {0}, {1}, {2}, {3}", _userName, _registerid, _newEMail, _expiration));

                        _result++;
                    }
                }
                else
                {
                    DataRow _dr = _ds.Tables[0].Rows[0];

                    string _publicKey = Convert.ToString(_dr["publicKey"]);
                    byte[] _puboldBytes = Encryptor.SNG.ChiperBase64ToPlainBytes(_publicKey);

                    X509Certificate2 _puboldCert2 = new X509Certificate2(_puboldBytes);
                    if (_puboldCert2.Equals(_publicCert2) == false)
                    {
                        _sqlstr
                            = "UPDATE TB_eTAX_PROVIDER "
                            + "   SET publicKey=@publicKey, userName=@userName, expiration=@expiration, lastUpdate=@lastUpdate "
                            + " WHERE registerId=@registerId AND aspEMail=@aspEMail";

                        _dbps.Add("@publicKey", SqlDbType.NVarChar, _publicStr);
                        _dbps.Add("@userName", SqlDbType.NVarChar, _userName);
                        _dbps.Add("@expiration", SqlDbType.DateTime, _expiration);
                        _dbps.Add("@lastUpdate", SqlDbType.DateTime, DateTime.Now);

                        if (LDataHelper.ExecuteText(UAppHelper.ConnectionString, _sqlstr, _dbps) < 1)
                        {
                            if (LogCommands == true)
                                ELogger.SNG.WriteLog(String.Format("UPDATE FAILURE: {0}, {1}, {2}, {3}", _userName, _registerid, _newEMail, _expiration));
                        }
                        else
                        {
                            if (LogCommands == true)
                                ELogger.SNG.WriteLog(String.Format("UPDATE SUCCESS: {0}, {1}, {2}, {3}", _userName, _registerid, _newEMail, _expiration));

                            _result++;
                        }
                    }
                    else
                    {
                        //if (LogCommands == true)
                        //    ELogger.SNG.WriteLog(String.Format("SAME-KEY: {0}, {1}, {2}, {3}", _userName, _registerid, _newEMail, _expiration));
                    }
                }

                _ostream.Close();
            }

            _izipStream.Close();

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
            {
                if (m_icollector != null)
                {
                    m_icollector.Dispose();
                    m_icollector = null;
                }
                if (m_InvoinceSet != null)
                {
                    m_InvoinceSet.Dispose();
                    m_InvoinceSet = null;
                }
                if (m_invoiceTbl != null)
                {
                    m_invoiceTbl.Dispose();
                    m_invoiceTbl = null;
                }
                if (m_lineitemTbl != null)
                {
                    m_lineitemTbl.Dispose();
                    m_lineitemTbl = null;
                }
                if (m_customerTbl != null)
                {
                    m_customerTbl.Dispose();
                    m_customerTbl = null;
                }
                if (m_partnerTbl != null)
                {
                    m_partnerTbl.Dispose();
                    m_partnerTbl = null;
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