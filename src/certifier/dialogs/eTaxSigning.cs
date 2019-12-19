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
using System.Security.Cryptography.Xml;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using OdinSdk.eTaxBill.Security.Issue;
using OdinSdk.eTaxBill.Security.Signature;

namespace OpenETaxBill.Certifier
{
    public partial class eTaxSigning : Form
    {

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private MainForm __parent_form = null;

        public eTaxSigning(Form p_parent_form)
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
        private void XmlSignature_Load(object sender, EventArgs e)
        {
            __parent_form.RestoreFormLayout(this);
        }

        private void XmlSignature_FormClosing(object sender, FormClosingEventArgs e)
        {
            __parent_form.SaveFormLayout(this);
        }

        private void btClear_Click(object sender, EventArgs e)
        {
            tbSourceXml.Text = "";
        }

        private void btLoad_Click(object sender, EventArgs e)
        {
            var _result = xmlLoadDlg.ShowDialog();
            if (_result == DialogResult.OK)
            {
                if (String.IsNullOrEmpty(xmlLoadDlg.FileName) == false)
                {
                    tbSourceXml.Text = File.ReadAllText(xmlLoadDlg.FileName, Encoding.UTF8);
                }
            }
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            var _result = xmlSaveDlg.ShowDialog();
            if (_result == DialogResult.OK)
            {
                if (String.IsNullOrEmpty(xmlSaveDlg.FileName) == false)
                {
                    File.WriteAllText(xmlSaveDlg.FileName, tbSourceXml.Text, Encoding.UTF8);
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private void FixupNamespaceNodes(XmlElement p_srcNode, XmlElement p_dstNode)
        {
            // remove namespace nodes
            foreach (XmlAttribute _attr1 in p_dstNode.SelectNodes("namespace::*"))
            {
                if (_attr1.LocalName == "xml")
                    continue;

                p_dstNode.RemoveAttributeNode(p_dstNode.OwnerDocument.ImportNode(_attr1, true) as XmlAttribute);
            }

            // add namespace nodes
            foreach (XmlAttribute _attr1 in p_srcNode.SelectNodes("namespace::*"))
            {
                if (_attr1.LocalName == "xml")
                    continue;
                if (_attr1.OwnerElement == p_srcNode)
                    continue;

                p_dstNode.SetAttributeNode(p_dstNode.OwnerDocument.ImportNode(_attr1, true) as XmlAttribute);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private void sbXPath_Click(object sender, EventArgs e)
        {
            var _source = Encoding.UTF8.GetBytes(tbSourceXml.Text);
            var _target = Encoding.UTF8.GetBytes(tbTargetXml.Text);

            var _maxLength = _source.Length > _target.Length ? _source.Length : _target.Length;
            var _pos = 0;

            for (; _pos < _maxLength; _pos++)
            {
                if (_pos >= _source.Length || _pos >= _target.Length)
                    break;

                if (_source[_pos] != _target[_pos])
                    break;
            }

            if (_pos < _maxLength)
            {
                WriteLine("missmatch: " + tbSourceXml.Text.Substring(_pos, 80));
            }
            else
            {
                WriteLine("completed comapare is same.");
            }
        }

        private void sbCreate_Click(object sender, EventArgs e)
        {
            var _type_code = String.Format("{0:00}{1:00}", (cbKind1.SelectedIndex + 1), (cbKind2.SelectedIndex + 1));

            var _read_file = Path.Combine(UCfgHelper.SNG.OutputFolder, $"unitest\\2-{_type_code}.xml");
            WriteLine("read source file: " + _read_file);

            var _target_xml = XSignature.SNG.GetSignedXmlStream(_read_file, UCertHelper.SNG.UserSignCert.X509Cert2);

            var _save_file = Path.Combine(UCfgHelper.SNG.OutputFolder, $"unitest\\6-{_type_code}.xml");
            {
                File.WriteAllBytes(_save_file, _target_xml.ToArray());

                tbSourceXml.Text = File.ReadAllText(_save_file, Encoding.UTF8);
                WriteLine("result docuement write on the " + _save_file);
            }

            MessageBox.Show(String.Format("암호화 되지 않은 XML형식의(전자서명 포함) 전자세금계산서\n\r{0} 파일을\n\r국세청 인증사이트에 업로드 하세요.", _save_file));
        }

        private void sbVerify_Click(object sender, EventArgs e)
        {
            var _signed_file = Path.Combine(UCfgHelper.SNG.OutputFolder, @"unitest\6.xml");

            var _xmlmgr = new XmlNamespaceManager(new NameTable());
            _xmlmgr.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);

            var _xmldoc = new XmlDocument(_xmlmgr.NameTable);
            _xmldoc.PreserveWhitespace = true;
            _xmldoc.Load(_signed_file);

            var _binarySecurityToken = (XmlElement)_xmldoc.DocumentElement.SelectSingleNode("descendant::ds:X509Certificate", _xmlmgr);
            var _token = Convert.FromBase64String(_binarySecurityToken.InnerText);

            var _x509cert2 = new X509Certificate2(_token);

            var _signed_info = (XmlElement)_xmldoc.DocumentElement.SelectSingleNode("descendant::ds:SignedInfo", _xmlmgr);
            var _content = Encoding.UTF8.GetBytes(
                        _signed_info.OuterXml.Replace(
                            "<ds:SignedInfo xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\">", 
                            "<ds:SignedInfo xmlns=\"urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                            )
                        );
            //var _content = Encoding.UTF8.GetBytes(_signed_info.OuterXml);

            var _signature_value = (XmlElement)_xmldoc.DocumentElement.SelectSingleNode("descendant::ds:SignatureValue", _xmlmgr);
            var _signature = Convert.FromBase64String(_signature_value.InnerText);

            if (Validator.SNG.VerifySignature(_content, _signature, _x509cert2.PublicKey.Key) == true)
                WriteLine("verify success");
            else
                WriteLine("verify failure");
        }

        private void btValidate_Click(object sender, EventArgs e)
        {
            var _signed_file = Path.Combine(UCfgHelper.SNG.OutputFolder, @"unitest\6.xml");

            var _ms = new MemoryStream(File.ReadAllBytes(_signed_file));

            WriteLine("Validating..." + Environment.NewLine);

            Validator.SNG.DoValidation(_ms);

            WriteLine(Validator.SNG.Result + Environment.NewLine);
            WriteLine("Done..." + Environment.NewLine);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}
