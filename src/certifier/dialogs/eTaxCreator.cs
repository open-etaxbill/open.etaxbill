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
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using NpgsqlTypes;
using OdinSdk.OdinLib.Data.POSTGRESQL;
using OdinSdk.eTaxBill.Security.Issue;

namespace OpenETaxBill.Certifier
{
    public partial class eTaxCreator : Form
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private MainForm __parent_form = null;

        public eTaxCreator(Form p_parent_form)
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
        // inner functions
        //-------------------------------------------------------------------------------------------------------------------------
        private void WriteLine(string p_message)
        {
            if (__parent_form != null)
            {
                var _main = __parent_form;
                _main.WriteOutput(p_message, this.Name);
            }
        }

        private DataSet getMasterDataSet(string p_type_code = "0101")
        {
            var _sqlstr = "SELECT * FROM TB_eTAX_INVOICE WHERE typeCode=@typeCode ORDER BY issueId DESC LIMIT 1";

            var _dbps = new PgDatParameters();
            {
                _dbps.Add("@typeCode", NpgsqlDbType.Varchar, p_type_code);
            }

            return LSQLHelper.SelectDataSet(UCfgHelper.SNG.ConnectionString, _sqlstr, _dbps);
        }

        private DataSet getDetailDataSet(string p_issue_id)
        {
            var _sqlstr = "SELECT * FROM TB_eTAX_LINEITEM WHERE issueId=@issueId";

            var _dbps = new PgDatParameters();
            {
                _dbps.Add("@issueId", NpgsqlDbType.Varchar, p_issue_id);
            }

            return LSQLHelper.SelectDataSet(UCfgHelper.SNG.ConnectionString, _sqlstr, _dbps);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private void XmlCreator_Load(object sender, EventArgs e)
        {
            __parent_form.RestoreFormLayout(this);

            tbInvoicerId.Text = UCfgHelper.SNG.InvoicerBizNo;
        }

        private void XmlCreator_FormClosing(object sender, FormClosingEventArgs e)
        {
            __parent_form.SaveFormLayout(this);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // events
        //-------------------------------------------------------------------------------------------------------------------------
        private void btCreate_Click(object sender, EventArgs e)
        {
            var _type_code = String.Format("{0:00}{1:00}", (cbKind1.SelectedIndex + 1), (cbKind2.SelectedIndex + 1));

            var _masterSet = getMasterDataSet(_type_code);  // 전자(세금)계산서
            if (LSQLHelper.IsNullOrEmpty(_masterSet) == false)
            {
                DataRow _masterRow = _masterSet.Tables[0].Rows[0];
                string _issue_id = (string)_masterRow["issueId"];

                while (UCertHelper.SNG.UserSignCert.VerifyVID(_masterRow["invoicerId"].ToString()) == false)
                {
                    if (MessageBox.Show(String.Format("사업자번호가 데이터와 다릅니다. '{0}'으로 변경 할까요?", tbInvoicerId.Text), "사업자번호검증", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        _masterRow["invoicerId"] = tbInvoicerId.Text;
                    else
                    {
                        this.Close();
                        return;
                    }
                }

                var _detailSet = getDetailDataSet(_issue_id);
                _masterSet.Merge(_detailSet);

                var _creator = new Writer(_masterSet);

                XmlDocument _targetDoc = Convertor.SNG.TransformToDocument(_creator.TaxDocument);
                _targetDoc = Convertor.SNG.CanonicalizationToDocument(_targetDoc, "\t");

                var _save_file = Path.Combine(UCfgHelper.SNG.OutputFolder, $"unitest\\2-{_type_code}.xml");
                {
                    File.WriteAllText(_save_file, _targetDoc.OuterXml, Encoding.UTF8);

                    tbSourceXml.Text = File.ReadAllText(_save_file, Encoding.UTF8);
                    WriteLine("result docuement write on the " + _save_file);
                }

                MessageBox.Show("가상의 전자(세금)계산서를 작성 하였습니다. 다음 단계인 전자서명을 하세요.");
            }
            else
            {
                MessageBox.Show("데이터베이스에 (세금)계산서 자료가 없습니다.");
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

        private void btLoad_Click(object sender, EventArgs e)
        {
            var _result = xmlLoadDlg.ShowDialog();
            if (_result == DialogResult.OK)
            {
                if (String.IsNullOrEmpty(xmlLoadDlg.FileName) == false)
                {
                    tbSourceXml.Text = (new StreamReader(xmlLoadDlg.FileName, Encoding.UTF8)).ReadToEnd();
                }
            }
        }

        private void btClear_Click(object sender, EventArgs e)
        {
            tbSourceXml.Text = "";
        }

        private void tbCanonical_Click(object sender, EventArgs e)
        {
            var _xmldoc = new XmlDocument();
            _xmldoc.LoadXml(tbSourceXml.Text);

            tbSourceXml.Text = Convertor.SNG.CanonicalizationToDocument(_xmldoc, "\t").OuterXml;
        }

        private void sbTransform_Click(object sender, EventArgs e)
        {
            var _xmldoc = new XmlDocument();
            _xmldoc.LoadXml(tbSourceXml.Text);

            tbSourceXml.Text = Convertor.SNG.TransformToDocument(_xmldoc).OuterXml;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}
