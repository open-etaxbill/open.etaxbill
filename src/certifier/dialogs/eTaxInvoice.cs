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
using System.Text;
using System.Windows.Forms;
using System.Xml;
using OdinSdk.eTaxBill.Security.Issue;
using OdinSdk.eTaxBill.Security.Notice;

namespace OpenETaxBill.Certifier
{
    public partial class eTaxInvoice : Form
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private MainForm __parent_form = null;

        public eTaxInvoice(Form p_parent_form)
        {
            InitializeComponent();

            __parent_form = (MainForm)p_parent_form;
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
        private void eXmlCreator_FormClosing(object sender, FormClosingEventArgs e)
        {
            __parent_form.SaveFormLayout(this);
        }

        private void eXmlCreator_Load(object sender, EventArgs e)
        {
            __parent_form.RestoreFormLayout(this);

            ceTestOk_CheckedChanged(sender, e);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private void CreateInvoce(string p_type_code)
        {
            var _time_stamp = DateTime.Now;

            var _soap_header = new Header
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

            var _soap_body = new Body
            {
                SubmitID = Packing.SNG.GetSubmitID(_soap_header.TimeStamp, UCfgHelper.SNG.RegisterId),
                ReferenceID = Guid.NewGuid().ToString(),
                TotalCount = 1 /* 전자세금계산서의 총 개수*/
            };

            var _load_file = Path.Combine(UCfgHelper.SNG.OutputFolder, $"unitest\\7-{p_type_code}.asn");
            {
                tbSourceXml.Text = File.ReadAllText(_load_file, Encoding.UTF8);
                WriteLine("encrypted file load from " + _load_file);
            }

            var _encrypted = File.ReadAllBytes(_load_file);

            //-------------------------------------------------------------------------------------------------------------------------
            // Signature
            //-------------------------------------------------------------------------------------------------------------------------
            var _signed_xml = Packing.SNG.GetSignedSoapEnvelope(_encrypted, UCertHelper.SNG.AspSignCert.X509Cert2, _soap_header, _soap_body);

            var _save_file = Path.Combine(UCfgHelper.SNG.OutputFolder, $"security\\7-{p_type_code}-전자서명후.txt");
            {
                File.WriteAllText(_save_file, _signed_xml.OuterXml, Encoding.UTF8);

                tbSourceXml.Text = File.ReadAllText(_save_file, Encoding.UTF8);
                WriteLine("transforms write on the " + _save_file);
            }
        }

        private void sbInvoiceSubmit_Click(object sender, EventArgs e)
        {
            var _type_code = String.Format("{0:00}{1:00}", (cbKind1.SelectedIndex + 1), (cbKind2.SelectedIndex + 1));

            if (String.IsNullOrEmpty(tbTaxInvoiceSubmitUrl.Text.Trim()) == true)
			{
				MessageBox.Show("전자세금계산서 제출을 위한 웹서비스 URL을 입력해 주십시오.");
				tbTaxInvoiceSubmitUrl.Focus();
				return;
			}

			MessageBox.Show(String.Format("전자세금계산서 제출을 위한 웹서비스 메시지를,\n\r{0} ENDPOINT를\n\r 통해 인증 시스템으로 전송 합니다.", tbTaxInvoiceSubmitUrl.Text.Trim()));
            CreateInvoce(_type_code);

            var _reference_id = "";

            var _load_file = Path.Combine(UCfgHelper.SNG.OutputFolder, $"security\\7-{_type_code}-전자서명후.txt");
            {
                tbSourceXml.Text = File.ReadAllText(_load_file, Encoding.UTF8);
                WriteLine("after transform read from " + _load_file);

                XmlDocument _xd = new XmlDocument();
                _xd.Load(_load_file);

                _reference_id = _xd.SelectSingleNode("descendant::kec:ReferenceID", Packing.SNG.SoapNamespaces).InnerText;
                WriteLine(String.Format("retrieve reference-id :<{0}>", _reference_id));
            }

            var _soap_part = File.ReadAllBytes(_load_file);
            var _attachment = File.ReadAllBytes(Path.Combine(UCfgHelper.SNG.OutputFolder, $"unitest\\7-{_type_code}.asn"));
            {
                WriteLine("read encrypt data " + _attachment.Length);
            }

            var _mime_content = Request.SNG.TaxInvoiceSubmit(_soap_part, _attachment, _reference_id, tbTaxInvoiceSubmitUrl.Text.Trim());
            if (_mime_content.StatusCode == 0)
            {
                var _save_file = Path.Combine(UCfgHelper.SNG.OutputFolder, $"security\\8-{_type_code}-TaxInvoiceRecvAck.txt");
                {
                    File.WriteAllText(_save_file, _mime_content.Parts[1].GetContentAsString(), Encoding.ASCII);

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

        private void btLoad_Click(object sender, EventArgs e)
        {
            var _result = xmlLoadDlg.ShowDialog();
            if (_result == DialogResult.OK)
            {
                if (String.IsNullOrEmpty(xmlLoadDlg.FileName) == false)
                {
                }
            }
        }

        private void sbCheckSign_Click(object sender, EventArgs e)
        {
            var _signed_file = Path.Combine(UCfgHelper.SNG.OutputFolder, @"security\7. 전자서명후.txt");

            var _xmldoc = new XmlDocument(Packing.SNG.SoapNamespaces.NameTable)
            {
                PreserveWhitespace = true
            };
            _xmldoc.Load(_signed_file);

            var _binarySecurityToken = (XmlElement)_xmldoc.DocumentElement.SelectSingleNode("descendant::wsse:BinarySecurityToken", Packing.SNG.SoapNamespaces);
            var _token = Convert.FromBase64String(_binarySecurityToken.InnerText);

            var _x509cert2 = new X509Certificate2(_token);

            var _signed_info = (XmlElement)_xmldoc.DocumentElement.SelectSingleNode("descendant::ds:SignedInfo", Packing.SNG.SoapNamespaces);
            var _content = Encoding.UTF8.GetBytes(
                            _signed_info.OuterXml.Replace(
                                    "<ds:SignedInfo xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\">",
                                    "<ds:SignedInfo xmlns:SOAP=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:kec=\"http://www.kec.or.kr/standard/Tax/\" xmlns:wsa=\"http://www.w3.org/2005/08/addressing\" xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                                )
                             );

            var _signature_value = (XmlElement)_xmldoc.DocumentElement.SelectSingleNode("descendant::ds:SignatureValue", Packing.SNG.SoapNamespaces);
            var _signature = Convert.FromBase64String(_signature_value.InnerText);

            if (Validator.SNG.VerifySignature(_content, _signature, _x509cert2.PublicKey.Key) == true)
                WriteLine("verify success");
            else
                WriteLine("verify failure");
        }

        private void ceTestOk_CheckedChanged(object sender, EventArgs e)
        {
            tbTaxInvoiceSubmitUrl.Text = UCfgHelper.SNG.TaxInvoiceSubmitUrl(ceTestOk.Checked, false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}