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
    public partial class eTaxRequest : Form
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private MainForm __parent_form = null;

        public eTaxRequest(Form p_parent_form)
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

        private void CreateRequest(string p_type_code)
        {
            var _time_stamp = DateTime.Now;

            var _soap_header = new Header()
            {
                ToAddress = tbResultsReqSubmitUrl.Text.Trim(),
                Action = Request.eTaxRequestSubmit,
                Version = UCfgHelper.SNG.eTaxVersion,

                FromParty = new Party(UCfgHelper.SNG.SenderBizNo, UCfgHelper.SNG.SenderBizName),
                ToParty = new Party(UCfgHelper.SNG.ReceiverBizNo, UCfgHelper.SNG.ReceiverBizName),

                OperationType = Request.OperationType_RequestSubmit,
                MessageType = Request.MessageType_Request,

                TimeStamp = _time_stamp,
                MessageId = Packing.SNG.GetMessageId(_time_stamp)
            };

            var _soap_body = new Body()
            {
                RefSubmitID = UCfgHelper.SNG.RegisterId + "-20160719-c82073dfeff344348f07e032cc8c313c"
            };

            //-------------------------------------------------------------------------------------------------------------------------
            // Signature
            //-------------------------------------------------------------------------------------------------------------------------
            var _signed_xml = Packing.SNG.GetSignedSoapEnvelope(null, UCertHelper.SNG.AspSignCert.X509Cert2, _soap_header, _soap_body);

            var _save_file = Path.Combine(UCfgHelper.SNG.OutputFolder, $"security\\9-{p_type_code}-ResultsReqsubmit.txt");
            {
                File.WriteAllText(_save_file, _signed_xml.OuterXml, Encoding.UTF8);

                tbSourceXml.Text = File.ReadAllText(_save_file, Encoding.UTF8);
                WriteLine("transforms write on the " + _save_file);
            }
        }

        private void btRequest_Click(object sender, EventArgs e)
        {
            var _type_code = String.Format("{0:00}{1:00}", (cbKind1.SelectedIndex + 1), (cbKind2.SelectedIndex + 1));

            if (String.IsNullOrEmpty(tbResultsReqSubmitUrl.Text.Trim()) == true)
			{
				MessageBox.Show("처리결과 요청을 위한 웹서비스 URL을 입력해 주십시오.");
				tbResultsReqSubmitUrl.Focus();
				return;
			}

            var _end_point = tbResultsReqSubmitUrl.Text.Trim();

            MessageBox.Show(String.Format("전자세금계산서 처리 결과 요청 메시지를,\n\rENDPOINT: {0}를\n\r 통해 인증 시스템으로 전송 합니다. ", _end_point));
            CreateRequest(_type_code);

            var _load_file = Path.Combine(UCfgHelper.SNG.OutputFolder, $"security\\9-{_type_code}-ResultsReqSubmit.txt");
            {
                tbSourceXml.Text = File.ReadAllText(_load_file, Encoding.UTF8);
                WriteLine("after transform read from " + _load_file);
            }

            var _soap_part = File.ReadAllBytes(_load_file);

            var _mime_content = Request.SNG.TaxRequestSubmit(_soap_part, _end_point);
            if (_mime_content.StatusCode == 0)
            {
                var _save_file = Path.Combine(UCfgHelper.SNG.OutputFolder, $"security\\10-{_type_code}-ResultsReqRecvAck.txt");
                {
                    string _response = _mime_content.GetContentAsString();
                    File.WriteAllText(_save_file, _response, Encoding.UTF8);

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

        private void ceTestOk_CheckedChanged(object sender, EventArgs e)
        {
            tbResultsReqSubmitUrl.Text = UCfgHelper.SNG.RequestResultsSubmitUrl(ceTestOk.Checked, false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}