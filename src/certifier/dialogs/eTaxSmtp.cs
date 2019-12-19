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
using System.Text;
using System.Windows.Forms;
using OdinSdk.eTaxBill.Security.Notice;

namespace OpenETaxBill.Certifier
{
    public partial class eTaxSmtp : Form
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private MainForm __parent_form = null;

        public eTaxSmtp(Form p_parent_form)
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
        private void eTaxSmtp_Load(object sender, EventArgs e)
        {
            __parent_form.RestoreFormLayout(this);
        }

        private void eTaxSmtp_FormClosing(object sender, FormClosingEventArgs e)
        {
            __parent_form.SaveFormLayout(this);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private void CreateRequest()
        {
            //var _mime = new MimeConstructor
            //{
            //    From = _invoicerEMail,
            //    To = _providerEMail.Split(';'),
            //    Subject = String.Format("전자세금계산서가 {0}에서 발급되었습니다.", _invoicerName),
            //    BodyHtml = GetProviderBody(_issuedRow)
            //};

            //var _obuffer = CmsManager.SNG.GetEncryptedContent(GetProviderKey(_invoiceeId), Encoding.UTF8.GetBytes(_document));
            //_mime.Attachments.Add(new Attachment(String.Format("{0}.msg", _issueId), _obuffer));

            ////-------------------------------------------------------------------------------------------------------------------------
            //// Signature
            ////-------------------------------------------------------------------------------------------------------------------------
            //var _signed_xml = Packing.SNG.GetSignedSoapEnvelope(null, UCertHelper.SNG.AspSignCert.X509Cert2, _soap_header, _soap_body);

            //var _save_file = Path.Combine(UCfgHelper.SNG.RootOutFolder, @"pubkey\31.CertReqSubmit.txt");
            //{
            //    File.WriteAllText(_save_file, _signed_xml.OuterXml, Encoding.UTF8);

            //    tbSourceXml.Text = File.ReadAllText(_save_file, Encoding.UTF8);
            //    WriteLine("transforms write on the " + _save_file);
            //}
        }

        private void sbCreateMsg_Click(object sender, EventArgs e)
        {
            MessageBox.Show(String.Format("공인인증서 요청을 위한 웹서비스 메시지를,\n\r{0} ENDPOINT를\n\r 통해 인증 시스템으로 전송 합니다.", UCfgHelper.SNG.RequestCertUrl));
            CreateRequest();

            var _load_file = Path.Combine(UCfgHelper.SNG.RootOutFolder, @"pubkey\31.CertReqSubmit.txt");
            {
                tbSourceXml.Text = File.ReadAllText(_load_file, Encoding.UTF8);
                WriteLine("read requesting certification soap-message from " + _load_file);
            }

            var _soap_part = Encoding.UTF8.GetBytes(File.ReadAllText(_load_file, Encoding.UTF8));

            var _mime_content = Request.SNG.TaxRequestCertSubmit(_soap_part, UCfgHelper.SNG.RequestCertUrl);
            if (_mime_content.StatusCode == 0)
            {
                var _save_file = Path.Combine(UCfgHelper.SNG.RootOutFolder, @"pubkey\32.CertReqRecvAck.txt");
                {
                    File.WriteAllText(_save_file, _mime_content.Parts[0].GetContentAsString(), Encoding.UTF8);

                    if (_mime_content.StatusCode == 0)
                        tbTargetXml.Text = File.ReadAllText(_save_file, Encoding.UTF8);
                    else
                        tbTargetXml.Text = _mime_content.ErrorMessage;

                    WriteLine("response write on the " + _save_file);
                }

                var _zip_file = Path.Combine(UCfgHelper.SNG.RootOutFolder, @"pubkey\32.CertReqRecvAck.zip");
                {
                    File.WriteAllBytes(_zip_file, _mime_content.Parts[1].GetContentAsStream().ToArray());

                    WriteLine("zip file write on the " + _zip_file);
                }

                MessageBox.Show(String.Format("수신된 Zip 파일이 {0}에 저장 되었습니다.", _zip_file));
            }
            else
            {
                MessageBox.Show(_mime_content.ErrorMessage);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}