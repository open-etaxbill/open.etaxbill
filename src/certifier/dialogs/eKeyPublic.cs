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
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using Npgsql;
using NpgsqlTypes;
using OdinSdk.OdinLib.Data.POSTGRESQL;
using OdinSdk.eTaxBill.Security.Encrypt;
using OdinSdk.eTaxBill.Security.Notice;

namespace OpenETaxBill.Certifier
{
    public partial class eKeyPublic : Form
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private MainForm __parent_form = null;

        public eKeyPublic(Form p_parent_form)
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
        private void eXmlPublicKey_Load(object sender, EventArgs e)
        {
            __parent_form.RestoreFormLayout(this);

            tbBizId.Text = UCfgHelper.SNG.SenderBizNo;
            tbBizName.Text = UCfgHelper.SNG.SenderBizName;
            tbRegId.Text = UCfgHelper.SNG.RegisterId;
        }

        private void eXmlPublicKey_FormClosing(object sender, FormClosingEventArgs e)
        {
            __parent_form.SaveFormLayout(this);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private void CreateRequest()
        {
            var _time_stamp = DateTime.Now;

            var _soap_header = new Header()
            {
                ToAddress = UCfgHelper.SNG.RequestCertUrl,
                Action = Request.eTaxRequestCertSubmit,
                Version = UCfgHelper.SNG.eTaxVersion,

                FromParty = new Party(UCfgHelper.SNG.SenderBizNo, UCfgHelper.SNG.SenderBizName),
                ToParty = new Party(UCfgHelper.SNG.ReceiverBizNo, UCfgHelper.SNG.ReceiverBizName),

                OperationType = Request.OperationType_RequestSubmit,
                MessageType = Request.MessageType_Request,

                TimeStamp = _time_stamp
            };

            var _soap_body = new Body()
            {
                RequestParty = new Party(tbBizId.Text, tbBizName.Text, tbRegId.Text),
                FileType = tbFileType.Text
            };

            //-------------------------------------------------------------------------------------------------------------------------
            // Signature
            //-------------------------------------------------------------------------------------------------------------------------
            var _signed_xml = Packing.SNG.GetSignedSoapEnvelope(null, UCertHelper.SNG.AspSignCert.X509Cert2, _soap_header, _soap_body);

            var _save_file = Path.Combine(UCfgHelper.SNG.RootOutFolder, @"pubkey\31.CertReqSubmit.txt");
            {
                File.WriteAllText(_save_file, _signed_xml.OuterXml, Encoding.UTF8);

                tbSourceXml.Text = File.ReadAllText(_save_file, Encoding.UTF8);
                WriteLine("transforms write on the " + _save_file);
            }
        }

        private void sbPublicKeyReqSubmit_Click(object sender, EventArgs e)
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

        private void sbUnZip_Click(object sender, EventArgs e)
        {
            var _zip_file = Path.Combine(UCfgHelper.SNG.RootOutFolder, @"pubkey\32.CertReqRecvAck.zip");
            {
                WriteLine("read zip file from " + _zip_file);

                var _izipStream = new ZipInputStream(File.OpenRead(_zip_file));

                WriteLine("unzipping...");

                ZipEntry _izipEntry;
                while ((_izipEntry = _izipStream.GetNextEntry()) != null)
                {
                    if (_izipEntry.Name.IndexOf(".ini") >= 0)
                        continue;

                    var _ostream = new MemoryStream();
                    {
                        var _size = 2048;
                        var _obuffer = new byte[_size];

                        while (true)
                        {
                            _size = _izipStream.Read(_obuffer, 0, _obuffer.Length);
                            if (_size <= 0)
                                break;

                            _ostream.Write(_obuffer, 0, _size);
                        }

                        _ostream.Seek(0, SeekOrigin.Begin);
                    }

                    var _file_name = Path.GetFileNameWithoutExtension(_izipEntry.Name);

                    var _register_id = _file_name.Substring(0, 8);
                    var _new_email = _file_name.Substring(9);

                    var _public_bytes = _ostream.ToArray();
                    string _public_key = Encryptor.SNG.PlainBytesToChiperBase64(_public_bytes);

                    var _publicCert2 = new X509Certificate2(_public_bytes);
                    var _expiration = Convert.ToDateTime(_publicCert2.GetExpirationDateString());

                    var _user_name = _publicCert2.GetNameInfo(X509NameType.SimpleName, false);

                    var _sqlstr
                        = "SELECT publicKey, aspEMail "
                        + "  FROM TB_eTAX_PROVIDER "
                        + " WHERE registerId=@registerId AND aspEMail=@aspEMail";

                    var _dbps = new PgDatParameters();
                    {
                        _dbps.Add("@registerId", NpgsqlDbType.Varchar, _register_id);
                        _dbps.Add("@aspEMail", NpgsqlDbType.Varchar, _new_email);
                    }

                    var _ds = LSQLHelper.SelectDataSet(UCfgHelper.SNG.ConnectionString, _sqlstr, _dbps);
                    if (LSQLHelper.IsNullOrEmpty(_ds) == true)
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

                        _dbps.Add("@registerId", NpgsqlDbType.Varchar, _register_id);
                        _dbps.Add("@aspEMail", NpgsqlDbType.Varchar, _new_email);
                        _dbps.Add("@name", NpgsqlDbType.Varchar, _user_name);
                        _dbps.Add("@person", NpgsqlDbType.Varchar, "");
                        _dbps.Add("@publicKey", NpgsqlDbType.Varchar, _public_key);
                        _dbps.Add("@userName", NpgsqlDbType.Varchar, _user_name);
                        _dbps.Add("@expiration", NpgsqlDbType.TimestampTz, _expiration);
                        _dbps.Add("@lastUpdate", NpgsqlDbType.TimestampTz, DateTime.Now);
                        _dbps.Add("@providerId", NpgsqlDbType.Varchar, "");

                        if (LSQLHelper.ExecuteText(UCfgHelper.SNG.ConnectionString, _sqlstr, _dbps) < 1)
                        {
                            WriteLine(String.Format("INSERT FAILURE: {0}, {1}, {2}, {3}", _user_name, _register_id, _new_email, _expiration));
                        }
                        else
                        {
                            WriteLine(String.Format("INSERT SUCCESS: {0}, {1}, {2}, {3}", _user_name, _register_id, _new_email, _expiration));
                        }
                    }
                    else
                    {
                        var _dr = _ds.Tables[0].Rows[0];

                        var _public_old_key = Convert.ToString(_dr["publicKey"]);
                        var _public_old_bytes = Encryptor.SNG.ChiperBase64ToPlainBytes(_public_old_key);

                        var _public_oldcert2 = new X509Certificate2(_public_old_bytes);
                        if (_public_oldcert2.Equals(_publicCert2) == false)
                        {
                            _sqlstr
                                = "UPDATE TB_eTAX_PROVIDER "
                                + "   SET publicKey=@publicKey, userName=@userName, expiration=@expiration, lastUpdate=@lastUpdate "
                                + " WHERE registerId=@registerId AND aspEMail=@aspEMail";

                            _dbps.Add("@publicKey", NpgsqlDbType.Varchar, _public_key);
                            _dbps.Add("@userName", NpgsqlDbType.Varchar, _user_name);
                            _dbps.Add("@expiration", NpgsqlDbType.TimestampTz, _expiration);
                            _dbps.Add("@lastUpdate", NpgsqlDbType.TimestampTz, DateTime.Now);

                            if (LSQLHelper.ExecuteText(UCfgHelper.SNG.ConnectionString, _sqlstr, _dbps) > 0)
                                WriteLine(String.Format("UPDATE SUCCESS: {0}, {1}, {2}, {3}", _user_name, _register_id, _new_email, _expiration));
                        }
                        else
                        {
                            //WriteLine(String.Format("SAME-KEY: {0}, {1}, {2}, {3}", _user_name, _register_id, _new_email, _expiration));
                        }
                    }

                    _ostream.Close();
                }

                _izipStream.Close();

                WriteLine("unzipped to " + UCfgHelper.SNG.RootOutFolder);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}