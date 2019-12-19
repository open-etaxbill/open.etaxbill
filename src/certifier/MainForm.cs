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

using OdinSdk.FormLib.Control.Library;
using OdinSdk.FormLib.Extension;
using System;
using System.Windows.Forms;

namespace OpenETaxBill.Certifier
{
    public partial class MainForm : Form
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // attributes
        //-------------------------------------------------------------------------------------------------------------------------

        private Form ActiveMDIForm
        {
            get
            {
                return ActiveMdiChild;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // methodes
        //-------------------------------------------------------------------------------------------------------------------------
        public void RestoreFormLayout(Form p_child_form)
        {
            LayoutHelper.SNG.RestoreFormLayout(p_child_form, this.GetType().GUID.ToString());
        }

        public void SaveFormLayout(Form p_child_form)
        {
            LayoutHelper.SNG.SaveFormLayout(p_child_form, this.GetType().GUID.ToString());
        }

        public void WriteOutput(string p_message, string p_category = "")
        {
            if (String.IsNullOrEmpty(p_category) == true)
                p_category = this.Name;

            this.InvokeEx(f =>
            {
                f.rtOutput.AppendText(String.Format("'{0}' => {1}{2}", p_category, p_message, Environment.NewLine));
                f.rtOutput.Select(f.rtOutput.Text.Length, 0);
                f.rtOutput.ScrollToCaret();
            });
        }

        private bool ActivateChildForm(Form p_newForm)
        {
            var _found = false;

            foreach (Form _form in MdiChildren)
            {
                if (_form.Name == p_newForm.Name)
                {
                    _form.Activate();
                    _found = true;

                    break;
                }
            }

            //if (_found == false)
            //{
            //    p_newForm.Parent = this;
            //    p_newForm.Show();
            //}

            return _found;
        }

        private void ToggleTabbedMDI()
        {
            //tabbedMdiMgr.MdiParent = IsTabbedMdi ? this : null;
            //biCascade.Visibility = biHorizontal.Visibility = biVertical.Visibility = IsTabbedMdi ? BarItemVisibility.Never : BarItemVisibility.Always;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        public MainForm()
        {
            InitializeComponent();

            ToggleTabbedMDI();
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // events
        //-------------------------------------------------------------------------------------------------------------------------
        private void MainForm_Load(object sender, EventArgs e)
        {
            LayoutHelper.SNG.RestoreFormLayout(this);
            this.Text = String.Format("표준 전자세금계산서 인증시스템 - {0}", UCfgHelper.SNG.KeySize);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            LayoutHelper.SNG.SaveFormLayout(this);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private void miClose_Click(object sender, EventArgs e)
        {
            if (ActiveMDIForm != null)
                ActiveMDIForm.Close();
        }

        private void miExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void miTaxCerti_Click(object sender, EventArgs e)
        {
            using (System.Diagnostics.Process _process = new System.Diagnostics.Process())
            {
                _process.StartInfo.FileName = "http://www.taxcerti.or.kr";
                _process.StartInfo.Verb = "Open";
                _process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                _process.Start();
            }
        }

        private void miAbout_Click(object sender, EventArgs e)
        {
            using (System.Diagnostics.Process _process = new System.Diagnostics.Process())
            {
                _process.StartInfo.FileName = "http://www.odinsoftware.co.kr";
                _process.StartInfo.Verb = "Open";
                _process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                _process.Start();
            }
        }

        private void miTabbedMdi_Click(object sender, EventArgs e)
        {
            ToggleTabbedMDI();
        }

        private void miCascade_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void miHorizontal_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void miVertical_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void miTaxCreator_Click(object sender, EventArgs e)
        {
            var _creator = new eTaxCreator(this);

            if (ActivateChildForm(_creator) == false)
            {
                _creator.MdiParent = this;
                _creator.Show();
            }
        }

        private void miTaxSigning_Click(object sender, EventArgs e)
        {
            var _signatureForm = new eTaxSigning(this);

            if (ActivateChildForm(_signatureForm) == false)
            {
                _signatureForm.MdiParent = this;
                _signatureForm.Show();
            }
        }

        private void miTaxEncrypt_Click(object sender, EventArgs e)
        {
            var _envelope = new eTaxEncrypt(this);

            if (ActivateChildForm(_envelope) == false)
            {
                _envelope.MdiParent = this;
                _envelope.Show();
            }
        }

        private void miTaxInvoice_Click(object sender, EventArgs e)
        {
            var _creator = new eTaxInvoice(this);

            if (ActivateChildForm(_creator) == false)
            {
                _creator.MdiParent = this;
                _creator.Show();
            }
        }

        private void miTaxReport_Click(object sender, EventArgs e)
        {
            string _end_point = UCfgHelper.SNG.ReplyAddress;

            string _message
                = "처리결과 전송 검증은 테스트베드가 임의의 전자(세금)계산서 처리결과를 사업자 시스템에 전송하고, 이에 대한 응답메시지를 검증합니다.\n"
                + "사업자 시스템의 ENDPOINT: {0}로 가상의 처리결과를 전송한 후 이를 검증 합니다.\n\n"
                + "국세청 인증 서버에서 Responsor 서비스로 메시지를 전달하는 단계 입니다.\n"
                + "국세청 인증 사이트에서 전송 버튼을 클릭 후 log를 확인 하세요.\n\n"
                + "** Windows 서버인 경우 방화벽 인바운드규칙에서 8080포트를 열어 줘야 합니다. **";

            MessageBox.Show(String.Format(_message, _end_point));
        }

        private void miTaxRequest_Click(object sender, EventArgs e)
        {
            var _creator = new eTaxRequest(this);

            if (ActivateChildForm(_creator) == false)
            {
                _creator.MdiParent = this;
                _creator.Show();
            }
        }

        private void miXmlInvoice_Click(object sender, EventArgs e)
        {
            var _interop = new eXmlInvoice(this);

            if (ActivateChildForm(_interop) == false)
            {
                _interop.MdiParent = this;
                _interop.Show();
            }
        }

        private void miXmlRequest_Click(object sender, EventArgs e)
        {
            var _interop = new eXmlRequest(this);

            if (ActivateChildForm(_interop) == false)
            {
                _interop.MdiParent = this;
                _interop.Show();
            }
        }

        private void miKeyPublic_Click(object sender, EventArgs e)
        {
            var _certkey = new eKeyPublic(this);

            if (ActivateChildForm(_certkey) == false)
            {
                _certkey.MdiParent = this;
                _certkey.Show();
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}