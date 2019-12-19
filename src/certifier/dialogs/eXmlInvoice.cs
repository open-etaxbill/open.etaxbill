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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Npgsql;
using NpgsqlTypes;
using OdinSdk.OdinLib.Data.POSTGRESQL;
using OdinSdk.eTaxBill.Security.Encrypt;
using OdinSdk.eTaxBill.Security.Issue;
using OdinSdk.eTaxBill.Security.Notice;
using OdinSdk.eTaxBill.Security.Signature;

namespace OpenETaxBill.Certifier
{
    public partial class eXmlInvoice : Form
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private MainForm __parent_form = null;

        public eXmlInvoice(Form p_parent_form)
        {
            InitializeComponent();

            __parent_form = (MainForm)p_parent_form;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

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

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private void WriteLine(string p_message)
        {
            if (__parent_form != null)
            {
                var _main = __parent_form;
                _main.WriteOutput(p_message, this.Name);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private DataSet getMasterDataSet(string p_issue_id)
        {
            var _sqlstr = "SELECT * FROM TB_eTAX_INVOICE WHERE issueId=@issueId";

            var _dbps = new PgDatParameters();
            _dbps.Add("@issueId", NpgsqlDbType.Varchar, p_issue_id);

            return LSQLHelper.SelectDataSet(UCfgHelper.SNG.ConnectionString, _sqlstr, _dbps);
        }

        private DataSet getDetailDataSet(string p_issue_id)
        {
            var _sqlstr = "SELECT * FROM TB_eTAX_LINEITEM WHERE issueId=@issueId";

            var _dbps = new PgDatParameters();
            _dbps.Add("@issueId", NpgsqlDbType.Varchar, p_issue_id);

            return LSQLHelper.SelectDataSet(UCfgHelper.SNG.ConnectionString, _sqlstr, _dbps);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private void eXmlInterop_FormClosing(object sender, FormClosingEventArgs e)
        {
            __parent_form.SaveFormLayout(this);
        }

        private void eXmlInterop_Load(object sender, EventArgs e)
        {
            __parent_form.RestoreFormLayout(this);

            tbInvoicerId.Text = UCfgHelper.SNG.InvoicerBizNo;
            ceTestOk_CheckedChanged(sender, e);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private void sbSubmit_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tbTaxInvoiceSubmitUrl.Text.Trim()) == true)
            {
                MessageBox.Show("전자세금계산서 제출을 위한 웹서비스 URL을 입력해 주십시오.");
                tbTaxInvoiceSubmitUrl.Focus();
                return;
            }

            var _end_point = tbTaxInvoiceSubmitUrl.Text.Trim();

            MessageBox.Show(String.Format("상호운영성 테스트 검증을 위한 웹서비스 메시지를,\n\r{0} ENDPOINT를\n\r 통해 인증 시스템으로 전송 합니다.", _end_point));
            if (CreateInvoce() == true)
            {
                var _reference_id = "";

                var _load_file = Path.Combine(UCfgHelper.SNG.OutputFolder, @"interop\17.전자서명후.txt");
                {
                    tbSourceXml.Text = File.ReadAllText(_load_file, Encoding.UTF8);
                    WriteLine("after transform read from " + _load_file);

                    var _xd = new XmlDocument();
                    _xd.Load(_load_file);

                    _reference_id = _xd.SelectSingleNode("descendant::kec:ReferenceID", Packing.SNG.SoapNamespaces).InnerText;
                    WriteLine(String.Format("retrieve reference-id :<{0}>", _reference_id));
                }

                var _soap_part = File.ReadAllBytes(_load_file);
                var _attachment = File.ReadAllBytes(Path.Combine(UCfgHelper.SNG.OutputFolder, @"interop\14.두번째ReferenceTarget.asn"));
                {
                    WriteLine("read encrypt data " + _attachment.Length);
                }

                var _mime_content = Request.SNG.TaxInvoiceSubmit(_soap_part, _attachment, _reference_id, _end_point);
                if (_mime_content.StatusCode == 0)
                {
                    var _save_file = Path.Combine(UCfgHelper.SNG.OutputFolder, @"interop\21.TaxInvoiceRecvAck.txt");
                    {
                        File.WriteAllText(_save_file, _mime_content.Parts[1].GetContentAsString(), Encoding.UTF8);

                        tbTargetXml.Text = File.ReadAllText(_save_file, Encoding.UTF8);
                        WriteLine("response write on the " + _save_file);
                    }

                    MessageBox.Show("전송 되었습니다.");
                }
                else
                {
                    MessageBox.Show(_mime_content.ErrorMessage);
                }
            }
        }

        /// <summary>
        /// 2048비트 인증서로 서명된 일반 전자세금계산서(1건)와 당초승인번호가 기입된 수정 전자세금계산서(1건) 
        /// 그리고 2048비트 인증서로 서명된 일반 계산서(1건)와 당초승인번호가 기입된 수정 계산서(1건)는 반드시 첨부해야 합니다.(총 4건)
        /// </summary>
        /// <returns></returns>
        private bool CreateInvoce()
        {
            var _ntsCert2 = UCertHelper.SNG.NtsPublicKey;

            //-------------------------------------------------------------------------------------------------------------------//
            // 세금계산서 작성
            //-------------------------------------------------------------------------------------------------------------------//

            // 2048_전자세금계산서
            var _read_file = Path.Combine(UCfgHelper.SNG.OutputFolder, $"unitest\\2-0101.xml");

            var _c_0101 = new XmlDocument();
            _c_0101.LoadXml((new StreamReader(_read_file, Encoding.UTF8)).ReadToEnd());
            var _t_0101 = Convertor.SNG.CanonicalizationToDocument(_c_0101, "\t");

            // 2048_전자계산서
            _read_file = Path.Combine(UCfgHelper.SNG.OutputFolder, $"unitest\\2-0301.xml");

            var _c_0301 = new XmlDocument();
            _c_0301.LoadXml((new StreamReader(_read_file, Encoding.UTF8)).ReadToEnd());
            var _t_0301 = Convertor.SNG.CanonicalizationToDocument(_c_0301, "\t");

            // 2048_수정전자세금계산서
            _read_file = Path.Combine(UCfgHelper.SNG.OutputFolder, $"unitest\\2-0201.xml");

            var _c_0201 = new XmlDocument();
            _c_0201.LoadXml((new StreamReader(_read_file, Encoding.UTF8)).ReadToEnd());
            var _t_0201 = Convertor.SNG.CanonicalizationToDocument(_c_0201, "\t");

            // 2048_수정전자계산서
            _read_file = Path.Combine(UCfgHelper.SNG.OutputFolder, $"unitest\\2-0401.xml");

            var _c_0401 = new XmlDocument();
            _c_0401.LoadXml((new StreamReader(_read_file, Encoding.UTF8)).ReadToEnd());
            var _t_0401 = Convertor.SNG.CanonicalizationToDocument(_c_0401, "\t");

            //-------------------------------------------------------------------------------------------------------------------//
            // 전자서명
            //-------------------------------------------------------------------------------------------------------------------//
            var _read_stream = new MemoryStream(Encoding.UTF8.GetBytes(_t_0101.OuterXml));
            var _p_0101 = XSignature.SNG.GetSignedXmlStream(_read_stream, UCertHelper.SNG.UserSignCert.X509Cert2);

            _read_stream = new MemoryStream(Encoding.UTF8.GetBytes(_t_0301.OuterXml));
            var _p_0301 = XSignature.SNG.GetSignedXmlStream(_read_stream, UCertHelper.SNG.UserSignCert.X509Cert2);

            _read_stream = new MemoryStream(Encoding.UTF8.GetBytes(_t_0201.OuterXml));
            var _p_0201 = XSignature.SNG.GetSignedXmlStream(_read_stream, UCertHelper.SNG.UserSignCert.X509Cert2);

            _read_stream = new MemoryStream(Encoding.UTF8.GetBytes(_t_0401.OuterXml));
            var _p_0401 = XSignature.SNG.GetSignedXmlStream(_read_stream, UCertHelper.SNG.UserSignCert.X509Cert2);

            var _save_file = Path.Combine(UCfgHelper.SNG.OutputFolder, @"interop\12.전자세금계산서.xml");
            {
                string _xmltxt = (new StreamReader(_p_0101)).ReadToEnd() + "\n";
                _p_0101.Seek(0, SeekOrigin.Begin);

                _xmltxt += (new StreamReader(_p_0301)).ReadToEnd() + "\n";
                _p_0301.Seek(0, SeekOrigin.Begin);

                _xmltxt += (new StreamReader(_p_0201)).ReadToEnd() + "\n";
                _p_0201.Seek(0, SeekOrigin.Begin);

                _xmltxt += (new StreamReader(_p_0401)).ReadToEnd() + "\n";
                _p_0401.Seek(0, SeekOrigin.Begin);

                File.WriteAllText(_save_file, _xmltxt);
                WriteLine("write invoice document on the " + _save_file);
            }

            //-------------------------------------------------------------------------------------------------------------------//
            // 암호화
            //-------------------------------------------------------------------------------------------------------------------//
            var _rvalue = UCertHelper.SNG.UserSignCert.RandomNumber;

            var _taxinvoice = new ArrayList();
            {
                var _s_0301 = new TaxInvoiceStruct
                {
                    SignerRValue = _rvalue,
                    TaxInvoice = _p_0301.ToArray()
                };
                _taxinvoice.Add(_s_0301);

                var _s_0101 = new TaxInvoiceStruct
                {
                    SignerRValue = _rvalue,
                    TaxInvoice = _p_0101.ToArray()
                };
                _taxinvoice.Add(_s_0101);

                var _s_0401 = new TaxInvoiceStruct
                {
                    SignerRValue = _rvalue,
                    TaxInvoice = _p_0401.ToArray()
                };
                _taxinvoice.Add(_s_0401);

                var _s_0201 = new TaxInvoiceStruct
                {
                    SignerRValue = _rvalue,
                    TaxInvoice = _p_0201.ToArray()
                };
                _taxinvoice.Add(_s_0201);
            }

            var _encrypted = CmsManager.SNG.GetContentInfo(_ntsCert2, _taxinvoice);

            _save_file = Path.Combine(UCfgHelper.SNG.OutputFolder, @"interop\14.두번째ReferenceTarget.asn");
            {
                File.WriteAllBytes(_save_file, _encrypted);
                WriteLine("write encrypted on the " + _save_file);
            }

            //-------------------------------------------------------------------------------------------------------------------//
            // SOAP Envelope
            //-------------------------------------------------------------------------------------------------------------------//
            var _time_stamp = DateTime.Now;

            var _soap_header = new Header()
            {
                ToAddress = tbTaxInvoiceSubmitUrl.Text.Trim(),

                Action = Request.eTaxInvoiceSubmit,
                Version = UCfgHelper.SNG.eTaxVersion,

                FromParty = new Party(UCfgHelper.SNG.SenderBizNo, UCfgHelper.SNG.SenderBizName),
                ToParty = new Party(UCfgHelper.SNG.ReceiverBizNo, UCfgHelper.SNG.ReceiverBizName),

                ReplyTo = UCfgHelper.SNG.ReplyAddress,
                OperationType = Request.OperationType_InvoiceSubmit,
                MessageType = Request.MessageType_Request,

                TimeStamp = _time_stamp,
                MessageId = Packing.SNG.GetMessageId(_time_stamp)
            };

            var _soap_body = new Body()
            {
                SubmitID = Packing.SNG.GetSubmitID(_soap_header.TimeStamp, UCfgHelper.SNG.RegisterId),
                ReferenceID = Guid.NewGuid().ToString(),

                TotalCount = 4   // 전자세금계산서의 총 개수
            };

            //-------------------------------------------------------------------------------------------------------------------//
            // SOAP Signature
            //-------------------------------------------------------------------------------------------------------------------//
            var _signed_xml = Packing.SNG.GetSignedSoapEnvelope(_encrypted, UCertHelper.SNG.AspSignCert.X509Cert2, _soap_header, _soap_body);

            _save_file = Path.Combine(UCfgHelper.SNG.OutputFolder, @"interop\17.전자서명후.txt");
            {
                File.WriteAllText(_save_file, _signed_xml.OuterXml, Encoding.UTF8);

                tbSourceXml.Text = File.ReadAllText(_save_file, Encoding.UTF8);
                WriteLine("transforms write on the " + _save_file);
            }

            return true;
        }

        private void ceTestOk_CheckedChanged(object sender, EventArgs e)
        {
            tbTaxInvoiceSubmitUrl.Text = UCfgHelper.SNG.TaxInvoiceSubmitUrl(ceTestOk.Checked, true);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}
