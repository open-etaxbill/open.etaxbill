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
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using OdinSdk.eTaxBill.Net.Mime;
using OdinSdk.eTaxBill.Security.Encrypt;

namespace OpenETaxBill.Engine.Mailer
{
    /// <summary>
    /// 
    /// </summary>
    public class AsyncWorker : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_issuingTbl"></param>
        /// <param name="p_resultTbl"></param>
        public AsyncWorker(DataTable p_issuingTbl, DataTable p_resultTbl)
        {
            m_issuingTbl = p_issuingTbl;
            m_resultTbl = p_resultTbl;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenETaxBill.Channel.Interface.IMailer m_imailer = null;
        private OpenETaxBill.Channel.Interface.IMailer IMailer
        {
            get
            {
                if (m_imailer == null)
                    m_imailer = new OpenETaxBill.Channel.Interface.IMailer();

                return m_imailer;
            }
        }

        private OpenETaxBill.Engine.Library.UAppHelper m_appHelper = null;
        public OpenETaxBill.Engine.Library.UAppHelper UAppHelper
        {
            get
            {
                if (m_appHelper == null)
                    m_appHelper = new OpenETaxBill.Engine.Library.UAppHelper(IMailer.Manager);

                return m_appHelper;
            }
        }

        private OpenETaxBill.Engine.Library.UCertHelper m_certHelper = null;
        public OpenETaxBill.Engine.Library.UCertHelper UCertHelper
        {
            get
            {
                if (m_certHelper == null)
                    m_certHelper = new OpenETaxBill.Engine.Library.UCertHelper(IMailer.Manager);

                return m_certHelper;
            }
        }

        private OdinSdk.OdinLib.Logging.QFileWriter m_qfwriter = null;
        private OdinSdk.OdinLib.Logging.QFileWriter QFWriter
        {
            get
            {
                if (m_qfwriter == null)
                    m_qfwriter = new OdinSdk.OdinLib.Logging.QFileWriter();

                return m_qfwriter;
            }
        }

        private DataTable m_issuingTbl, m_resultTbl;

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private string m_invoiceeId = null;
        private X509Certificate2 m_providerKey = null;

        private X509Certificate2 GetProviderKey(string p_invoiceeId)
        {
            if (m_providerKey == null || m_invoiceeId != p_invoiceeId)
            {
                m_providerKey = UCertHelper.GetProviderCertByCustomer(p_invoiceeId);
                m_invoiceeId = p_invoiceeId;
            }

            return m_providerKey;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_istreams"></param>
        /// <param name="p_filenames"></param>
        /// <param name="p_password"></param>
        /// <returns></returns>
        public byte[] ZipStreams(Stream[] p_istreams, string[] p_filenames, string p_password)
        {
            byte[] _result = null;

            MemoryStream _ostream = new MemoryStream();

            ZipOutputStream _ozipStream = new ZipOutputStream(_ostream);                 // create zip stream
            {
                if (String.IsNullOrEmpty(p_password) == false)
                    _ozipStream.Password = p_password;

                _ozipStream.UseZip64 = UseZip64.Off;
                _ozipStream.SetLevel(9);                                                // maximum compression
            }

            for (int i = 0; i < p_istreams.Length; i++)
            {
                ZipEntry _ozipEntry = new ZipEntry(p_filenames[i]);
                _ozipStream.PutNextEntry(_ozipEntry);

                Stream _istream = p_istreams[i];
                byte[] _ibuffer = new byte[_istream.Length];

                _istream.Read(_ibuffer, 0, _ibuffer.Length);
                _ozipStream.Write(_ibuffer, 0, _ibuffer.Length);
            }

            _ozipStream.Finish();

            _ostream.Position = 0;
            _result = new byte[_ostream.Length];
            Array.Copy(_ostream.ToArray(), _result, _result.Length);

            _ozipStream.Close();

            return _result;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private string GetInvoiceeBody(string p_etaxurl, DataRow p_workingRow)
        {
            var _body = new StringBuilder();

            _body.AppendLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
            _body.AppendLine("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
            _body.AppendLine("    <head>");
            _body.AppendLine("        <title>OdinETAX 전자세금계산서를 발급하였습니다.</title>");
            _body.AppendLine("        <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
            _body.AppendLine("    </head>");
            _body.AppendLine("    <body style='padding-top:20px; text-align:center'>");
            _body.AppendLine(String.Format("<img src='{0}outside/chkMailOpen.aspx?issueId={1}' width='0' height='0'>", p_etaxurl, Convert.ToString(p_workingRow["issueId"])));
            _body.AppendLine("        <table cellpadding='0' cellspacing='0' style='border:1px #c3c2c2 double; width:670px; text-align:cente'>");
            _body.AppendLine("             <tr>");
            _body.AppendLine("             <td>");
            _body.AppendLine("             <table cellpadding='0' cellspacing='0' style='margin:20px 20px 5px 20px; width:630px; text-align:center;'>");
            _body.AppendLine("                 <tr>");
            _body.AppendLine("                 <td style='font-size:18px; font-weight:bold; font-family:Arial; color:#474747; padding-bottom:2px;letter-spacing:-1px; text-align:left;'>OdinETAX</td>");
            _body.AppendLine("                 <td style='font-size:11px; font-weight:bold; font-family:Arial; color:#474747; padding-bottom:2px; text-align:right; vertical-align:bottom'>Mailing Service</td>");
            _body.AppendLine("                 </tr>");
            _body.AppendLine("                 <tr>");
            _body.AppendLine("                 <td colspan='2' style='background-color:#efefef; height:5px; padding-top:0px; padding-bottom:0px; width:640px'></td>");
            _body.AppendLine("                 </tr>");
            _body.AppendLine("             </table>");
            _body.AppendLine("             </td>");
            _body.AppendLine("             </tr>");
            _body.AppendLine("             <tr>");
            _body.AppendLine("             <td>");
            _body.AppendLine("             <table cellpadding='0' cellspacing='0' style='padding:30px 20px 30px 40px; width:100%; border:0; text-align:left; vertical-align:middle'>");
            _body.AppendLine("                 <tr>");
            _body.AppendLine("                 <td style=' font-size:14px; font-weight:bold; font-family:굴림; color:#474747;'>빠르고 편리한 <span style='font-size:14px; font-weight:bold; font-family:Arial; color:#474747; letter-spacing:-1px;'>uBixEtax</span> <span style='font-size:18px; font-weight:bold; font-family:돋움; color:#ff6600;letter-spacing:-1px;'>전자세금계산서</span></td>");
            _body.AppendLine("                 </tr>");
            _body.AppendLine("             </table>");
            _body.AppendLine("             </td>");
            _body.AppendLine("             </tr>");
            _body.AppendLine("             <tr>");
            _body.AppendLine("             <td>");
            _body.AppendLine("             <table cellpadding='0' cellspacing='0' style='border:2px #eeeeee solid; width:620px;padding:0px 20px 0px 20px; margin:0px 20px 0px 20px'>");
            _body.AppendLine("                 <tr>");
            _body.AppendLine("                 <td style='font-size:11px; font-weight:bold; font-family:돋움; color:#474747; padding-top:20px; text-align:right; vertical-align:bottom; padding-bottom:5px'>본 메일은 보안메일 입니다.</td>");
            _body.AppendLine("                 </tr>");
            _body.AppendLine("                 <tr>");
            _body.AppendLine("                 <td>");
            _body.AppendLine("                   <table cellpadding='0' cellspacing='0' style='width:580px; border-top:2px #efeeee dotted; border-bottom:2px #efeeee dotted; text-align:center; vertical-align:middle'>");
            _body.AppendLine("                      <tr>");
            _body.AppendLine("                      <td style='background-color:#f7f7f7;text-align:center; font-family:굴림; font-size:12px; color:#7b7b7b; letter-spacing:-1px; padding-top:20px; padding-bottom:20px' >");
            _body.AppendLine(String.Format("<span style='font-weight:bold; color:#5474c6'>{0}</span> 사업자가 <span style='font-weight:bold; color:#5474c6'>{1}</span> 사업자에게 전자세금계산서를 발급하였습니다.", Convert.ToString(p_workingRow["invoicerName"]), Convert.ToString(p_workingRow["invoiceeName"])));
            _body.AppendLine("                      </td>");
            _body.AppendLine("                      </tr>");
            _body.AppendLine("                      <tr>");
            _body.AppendLine("                      <td style='background-color:#f7f7f7;text-align:center; font-family:굴림; font-size:12px; color:#7b7b7b; letter-spacing:-1px; padding-top:20px; padding-bottom:20px' >");
            _body.AppendLine("                     본 메일은 고객님의 데이터를 보호하기 위해 메일내용이 암호화 되어 있습니다.<br />고객님의 사업자번호를 입력하시면 첨부파일을 확인하실 수 있습니다.<br>");
            _body.AppendLine(String.Format("만약 문서를 바로확인하시려면 [문서확인] 클릭하시기 바랍니다. <a href='{0}outside/chkpass.aspx?issueId={1}&securityId={2}' style='color:#ff6600;font-weight:bold; text-decoration: underline;'>[문서확인]</a>", p_etaxurl, Convert.ToString(p_workingRow["issueId"]), Convert.ToString(p_workingRow["securityId"])));
            _body.AppendLine("                     <br><br>");
            _body.AppendLine(String.Format("이전 발급 내역을 확인하시려면 [발급내역] 클릭하시기 바랍니다. <a href='{0}outside/NoMemberList.aspx' style='color:#ff6600;font-weight:bold; text-decoration: underline;'>[발급내역]</a>", p_etaxurl));
            _body.AppendLine("                      </td>");
            _body.AppendLine("                      </tr>");
            _body.AppendLine("                   </table>");
            _body.AppendLine("                 </td>");
            _body.AppendLine("                 </tr>");
            _body.AppendLine("                 <tr>");
            _body.AppendLine("                 <td style='font-size:11px; font-weight:bold; font-family:돋움; color:#989898;padding-top:20px; padding-bottom:20px; letter-spacing:-1px; text-align:center;'>메일 내용을 확인하기 위해서는 메일의 <span style='color:#474747;'>첨부파일</span>을 다운로드하여 확인하시기 바랍니다.</td>");
            _body.AppendLine("                 </tr>");
            _body.AppendLine("             </table>");
            _body.AppendLine("             </td>");
            _body.AppendLine("             </tr>");
            _body.AppendLine("             <tr>");
            _body.AppendLine("             <td style='height:30px'>");
            _body.AppendLine("             </td>");
            _body.AppendLine("             </tr>");
            _body.AppendLine("         </table>");
            _body.AppendLine("         <table cellpadding='0' cellspacing='0' style='width:670px; padding-top:5px; padding-bottom:40px'>");
            _body.AppendLine("         <tr>");
            _body.AppendLine("             <td>");
            _body.AppendLine("             <table cellpadding='0' cellspacing='0' style='border-top:5px #efeeee solid; width:100%;padding:0px;background-color:#a8a8a8;text-align:center; vertical-align:middle; font-family:돋움; font-size:11px; color:#e9e9e9;'>");
            _body.AppendLine("             <tr>");
            _body.AppendLine("             <td style='height: 21px; padding-top:5px'>이 메일은 발신전용 메일이므로 회신이 되지 않습니다. 문의사항은 상담센터를 이용하시기 바랍니다.</td>");
            _body.AppendLine("             </tr>");
            _body.AppendLine("             <tr>");
            _body.AppendLine(String.Format("<td style='height: 21px; padding-bottom:5px'>{0}</td>", UAppHelper.OfficeAddress));
            _body.AppendLine("             </tr>");
            _body.AppendLine("             </table>");
            _body.AppendLine("         </td>");
            _body.AppendLine("         </tr>");
            _body.AppendLine("     </table>");
            _body.AppendLine("     </body>");
            _body.AppendLine("</html>");

            return _body.ToString();
        }

        private string GetProviderBody(DataRow p_workingRow)
        {
            var _body = new StringBuilder();

            _body.AppendLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
            _body.AppendLine("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
            _body.AppendLine("    <head>");
            _body.AppendLine("        <title>OdinETAX 전자세금계산서를 발급하였습니다.</title>");
            _body.AppendLine("        <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
            _body.AppendLine("    </head>");
            _body.AppendLine("    <body style='padding-top:20px; text-align:center'>");
            _body.AppendLine("        <table cellpadding='0' cellspacing='0' style='border:1px #c3c2c2 double; width:670px; text-align:cente'>");
            _body.AppendLine("             <tr>");
            _body.AppendLine("             <td>");
            _body.AppendLine("             <table cellpadding='0' cellspacing='0' style='margin:20px 20px 5px 20px; width:630px; text-align:center;'>");
            _body.AppendLine("                 <tr>");
            _body.AppendLine("                 <td style='font-size:18px; font-weight:bold; font-family:Arial; color:#474747; padding-bottom:2px;letter-spacing:-1px; text-align:left;'>OdinETAX</td>");
            _body.AppendLine("                 <td style='font-size:11px; font-weight:bold; font-family:Arial; color:#474747; padding-bottom:2px; text-align:right; vertical-align:bottom'>Mailing Service</td>");
            _body.AppendLine("                 </tr>");
            _body.AppendLine("                 <tr>");
            _body.AppendLine("                 <td colspan='2' style='background-color:#efefef; height:5px; padding-top:0px; padding-bottom:0px; width:640px'></td>");
            _body.AppendLine("                 </tr>");
            _body.AppendLine("             </table>");
            _body.AppendLine("             </td>");
            _body.AppendLine("             </tr>");
            _body.AppendLine("             <tr>");
            _body.AppendLine("             <td>");
            _body.AppendLine("             <table cellpadding='0' cellspacing='0' style='padding:30px 20px 30px 40px; width:100%; border:0; text-align:left; vertical-align:middle'>");
            _body.AppendLine("                 <tr>");
            _body.AppendLine("                 <td style=' font-size:14px; font-weight:bold; font-family:굴림; color:#474747;'>빠르고 편리한 <span style='font-size:14px; font-weight:bold; font-family:Arial; color:#474747; letter-spacing:-1px;'>uBixEtax</span> <span style='font-size:18px; font-weight:bold; font-family:돋움; color:#ff6600;letter-spacing:-1px;'>전자세금계산서</span></td>");
            _body.AppendLine("                 </tr>");
            _body.AppendLine("             </table>");
            _body.AppendLine("             </td>");
            _body.AppendLine("             </tr>");
            _body.AppendLine("             <tr>");
            _body.AppendLine("             <td>");
            _body.AppendLine("             <table cellpadding='0' cellspacing='0' style='border:2px #eeeeee solid; width:620px;padding:0px 20px 0px 20px; margin:0px 20px 0px 20px'>");
            _body.AppendLine("                 <tr>");
            _body.AppendLine("                 <td style='font-size:11px; font-weight:bold; font-family:돋움; color:#474747; padding-top:20px; text-align:right; vertical-align:bottom; padding-bottom:5px'>본 메일은 보안메일 입니다.</td>");
            _body.AppendLine("                 </tr>");
            _body.AppendLine("                 <tr>");
            _body.AppendLine("                 <td>");
            _body.AppendLine("                   <table cellpadding='0' cellspacing='0' style='width:580px; border-top:2px #efeeee dotted; border-bottom:2px #efeeee dotted; text-align:center; vertical-align:middle'>");
            _body.AppendLine("                      <tr>");
            _body.AppendLine("                      <td style='background-color:#f7f7f7;text-align:center; font-family:굴림; font-size:12px; color:#7b7b7b; letter-spacing:-1px; padding-top:20px; padding-bottom:20px' >");
            _body.AppendLine(String.Format(" <span style='font-weight:bold; color:#5474c6'>{0}</span> 사업자가 <span style='font-weight:bold; color:#5474c6'>{1}</span> 사업자에게 발급한 전자세금계산서를 ASP사업자에게 송부합니다.", Convert.ToString(p_workingRow["invoicerName"]), Convert.ToString(p_workingRow["invoiceeName"])));
            _body.AppendLine("                      </td>");
            _body.AppendLine("                      </tr>");
            _body.AppendLine("                   </table>");
            _body.AppendLine("                 </td>");
            _body.AppendLine("                 </tr>");
            _body.AppendLine("                 <tr>");
            _body.AppendLine("                 <td style='font-size:11px; font-weight:bold; font-family:돋움; color:#989898;padding-top:20px; padding-bottom:20px; letter-spacing:-1px; text-align:center;'>메일 내용을 확인하기 위해서는 메일의 <span style='color:#474747;'>첨부파일</span>을 다운로드하여 확인하시기 바랍니다.</td>");
            _body.AppendLine("                 </tr>");
            _body.AppendLine("             </table>");
            _body.AppendLine("             </td>");
            _body.AppendLine("             </tr>");
            _body.AppendLine("             <tr>");
            _body.AppendLine("             <td style='height:30px'>");
            _body.AppendLine("             </td>");
            _body.AppendLine("             </tr>");
            _body.AppendLine("         </table>");
            _body.AppendLine("         <table cellpadding='0' cellspacing='0' style='width:670px; padding-top:5px; padding-bottom:40px'>");
            _body.AppendLine("         <tr>");
            _body.AppendLine("             <td>");
            _body.AppendLine("             <table cellpadding='0' cellspacing='0' style='border-top:5px #efeeee solid; width:100%;padding:0px;background-color:#a8a8a8;text-align:center; vertical-align:middle; font-family:돋움; font-size:11px; color:#e9e9e9;'>");
            _body.AppendLine("             <tr>");
            _body.AppendLine("             <td style='height: 21px; padding-top:5px'>이 메일은 발신전용 메일이므로 회신이 되지 않습니다. 문의사항은 상담센터를 이용하시기 바랍니다.</td>");
            _body.AppendLine("             </tr>");
            _body.AppendLine("             <tr>");
            _body.AppendLine(String.Format("<td style='height: 21px; padding-bottom:5px'>{0}</td>", UAppHelper.OfficeAddress));
            _body.AppendLine("             </tr>");
            _body.AppendLine("             </table>");
            _body.AppendLine("         </td>");
            _body.AppendLine("         </tr>");
            _body.AppendLine("     </table>");
            _body.AppendLine("     </body>");
            _body.AppendLine("</html>");

            return _body.ToString();
        }

        private string GetHtmlBody(string p_etaxurl, DataRow p_workingRow, DateTime p_sentTime)
        {
            var _body = new StringBuilder();

            // 첨부파일 내역
            string _typeCode = Convert.ToString(p_workingRow["typeCode"]);
            switch (_typeCode.Substring(2, 2))
            {
                case "01":
                    _typeCode = "일반";
                    break;
                case "02":
                    _typeCode = "영세율";
                    break;
                case "03":
                    _typeCode = "위수탁";
                    break;
                case "04":
                    _typeCode = "수입";
                    break;
                case "05":
                    _typeCode = "영세율위수탁";
                    break;
                case "06":
                    _typeCode = "수입납부유예";
                    break;
                default:
                    _typeCode = "일반";
                    break;
            }

            _body.AppendLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
            _body.AppendLine("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
            _body.AppendLine("    <head>");
            _body.AppendLine("        <title>OdinETAX 전자세금계산서를 발급하였습니다.</title>");
            _body.AppendLine("        <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
            _body.AppendLine("    </head>");
            _body.AppendLine("    <body style='text-align:center; margin-top:30px'>");
            _body.AppendLine("    <table cellpadding='0' cellspacing='0' border='0' width='665px' align='center'>");
            _body.AppendLine("      <tr>");
            _body.AppendLine("        <td>");
            _body.AppendLine("<!-- 로고 및 주소 -->    ");
            _body.AppendLine("    		 <table cellpadding='0' cellspacing='0' border='0' width='100%'>");
            _body.AppendLine("    		   <tr>");
            _body.AppendLine(String.Format("<td style='font-size:30px; color:#484848; font-family:UniversConBold; text-align:left'><a href='{0}' style='color:#484848;text-decoration: none;'>eTax.uBizware</a></td>", p_etaxurl));
            _body.AppendLine(String.Format("<td style='font-size:12px; color:#7b7b7b; font-family:Arial; text-align:right'><a href='{0}' style='color:#7b7b7b;text-decoration: none;'>http://etax.odinsoftware.co.kr</a></td>", p_etaxurl));
            _body.AppendLine("    		   </tr>");
            _body.AppendLine("    		 </table>");
            _body.AppendLine("<!-- 텍스트 내용 -->    ");
            _body.AppendLine("    		 <table cellpadding='0' cellspacing='0' border='0' width='100%'>");
            _body.AppendLine("    		   <tr>");
            _body.AppendLine("    		     <td style='font-size:18px; font-weight:bold; color:#484848; font-family:바탕; line-height:25px; padding-top:60px;text-align:center;'>거래처에서 보낸 ");
            _body.AppendLine("    			 <span style='color: #254A8D;' >전자세금계산서</span>가 도착했습니다.</td>");
            _body.AppendLine("    		   </tr>");
            _body.AppendLine("    		   <tr>");
            _body.AppendLine("    		     <td style='font-size:12px; color:#7b7b7b; font-family:굴림; text-align:center; padding:15px 0px 30px 0px'>지금 받으신 전자세금계산서는 eTax.uBizware에서 다시 확인할 수 있습니다. ");
            _body.AppendLine("    		     </td>");
            _body.AppendLine("    		   </tr>");
            _body.AppendLine("    		 </table>");
            _body.AppendLine("<!-- 전자세금계산서 승인 번호 -->    ");
            _body.AppendLine("    		 <table cellpadding='0' cellspacing='0' style='border-top: 2px #cccccc solid;border-left: 1px #cccccc solid; font-size:12px; color:#7b7b7b; font-family:굴림; line-height:25px; text-align:center; width:100% ' >");
            _body.AppendLine("    		   <tr>");
            _body.AppendLine("    		     <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid; background-color:#f2f1ed; width:180px'>전자세금계산서 승인번호</td>");
            _body.AppendLine(String.Format("<td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;'>{0}</td>", Convert.ToString(p_workingRow["issueId"])));
            _body.AppendLine("    		   </tr>");
            _body.AppendLine("    		 </table>");
            _body.AppendLine("<!-- 전자세금계산서 분류 -->    ");
            _body.AppendLine("    		 <table cellpadding='0' cellspacing='0' style='border-top: 2px #cccccc solid;border-left: 1px #cccccc solid; font-size:12px; color:#7b7b7b; font-family:굴림; line-height:25px; text-align:center; width:100%;margin-top:10px  ' >");
            _body.AppendLine("    		   <tr>");
            _body.AppendLine("    		     <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid; background-color:#f2f1ed; width:180px'>전자세금계산서 분류</td>");
            _body.AppendLine("    		     <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid; width:150px'>전자세금계산서</td>");
            _body.AppendLine("    		     <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid; background-color:#f2f1ed; width:180px'>전자세금계산서 종류</td>");
            _body.AppendLine(String.Format("<td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid; width:150px'>{0}</td>", _typeCode));
            _body.AppendLine("    		   </tr>");
            _body.AppendLine("    		 </table>");
            _body.AppendLine("<!-- 공급자/공급받는자 테이블 -->    ");
            _body.AppendLine("    		 <table cellpadding='0' cellspacing='0' border='0' width='100%' style='margin-top:10px '>");
            _body.AppendLine("    		   <tr>");
            _body.AppendLine("	  <!-- 공급자 내용 -->    ");
            _body.AppendLine("    		     <td>");
            _body.AppendLine("    		       <table cellpadding='0' cellspacing='0' style='border-top: 2px #cccccc solid;border-left: 1px #cccccc solid; font-size:12px; color:#7b7b7b; font-family:굴림; line-height:25px; text-align:center; width:330px ' >");
            _body.AppendLine("    		         <tr>");
            _body.AppendLine("    		           <td colspan='2' style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid; background-color:#f2f1ed'>공급자</td>");
            _body.AppendLine("    		         </tr>");
            _body.AppendLine("    		         <tr>");
            _body.AppendLine("    		           <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;'>회사명</td>");
            _body.AppendLine(String.Format("    		           <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;'>{0}</td>", Convert.ToString(p_workingRow["invoicerName"])));
            _body.AppendLine("    		         </tr>");
            _body.AppendLine("    		         <tr>");
            _body.AppendLine("    		           <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;'>대표자</td>");
            _body.AppendLine(String.Format("    		           <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;'>{0}</td>", Convert.ToString(p_workingRow["invoicerPerson"])));
            _body.AppendLine("    		         </tr>");
            _body.AppendLine("    		         <tr>");
            _body.AppendLine("    		           <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;'>연락처</td>");
            _body.AppendLine(String.Format("    		           <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;'>{0}</td>", Convert.ToString(p_workingRow["invoicerPhone"])));
            _body.AppendLine("    		         </tr>");
            _body.AppendLine("    		       </table>");
            _body.AppendLine("    		     </td>");
            _body.AppendLine("     <!-- 공급받는자 내용 -->    ");
            _body.AppendLine("    		     <td style='padding-left:5px'>");
            _body.AppendLine("    		       <table cellpadding='0' cellspacing='0' style='border-top: 2px #cccccc solid;border-left: 1px #cccccc solid; font-size:12px; color:#7b7b7b; font-family:굴림; line-height:25px; text-align:center; width:330px ' >");
            _body.AppendLine("    		         <tr>");
            _body.AppendLine("    		           <td colspan='2' style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid; background-color:#f2f1ed'>공급받는자</td>");
            _body.AppendLine("    		         </tr>");
            _body.AppendLine("    		         <tr>");
            _body.AppendLine("    		           <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;'>회사명</td>");
            _body.AppendLine(String.Format("    		           <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;'>{0}</td>", Convert.ToString(p_workingRow["invoiceeName"])));
            _body.AppendLine("    		         </tr>");
            _body.AppendLine("    		         <tr>");
            _body.AppendLine("    		           <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;'>대표자</td>");
            _body.AppendLine(String.Format("    		           <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;'>{0}</td>", Convert.ToString(p_workingRow["invoiceePerson"])));
            _body.AppendLine("    		         </tr>");
            _body.AppendLine("    		         <tr>");
            _body.AppendLine("    		           <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;'>연락처</td>");
            _body.AppendLine(String.Format("    		           <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;'>{0}</td>", Convert.ToString(p_workingRow["invoiceePhone"])));
            _body.AppendLine("    		         </tr>");
            _body.AppendLine("    		       </table>");
            _body.AppendLine("    		     </td>");
            _body.AppendLine("    		   </tr>");
            _body.AppendLine("    		 </table>");
            _body.AppendLine("    <!-- 세금계산서 금액 -->");
            _body.AppendLine("    		 <table cellpadding='0' cellspacing='0' style='border-top: 2px #cccccc solid;border-left: 1px #cccccc solid; font-size:12px; color:#7b7b7b; font-family:굴림; line-height:25px; text-align:center; width:100%; margin-top:10px '>");
            _body.AppendLine("    		   <tr>");
            _body.AppendLine("    		     <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;background-color:#f2f1ed; width:85px'>합계금액</td>");
            _body.AppendLine(String.Format("<td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid; width:135px'>{0:#,##0}</td>", Convert.ToInt64(p_workingRow["grandTotal"])));
            _body.AppendLine("    		     <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;background-color:#f2f1ed; width:85px'>공급가액</td>");
            _body.AppendLine(String.Format("<td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid; width:134px'>{0:#,##0}</td>", Convert.ToInt64(p_workingRow["chargeTotal"])));
            _body.AppendLine("    		     <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;background-color:#f2f1ed; width:85px'>세액</td>");
            _body.AppendLine(String.Format("<td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid; width:134px'>{0:#,##0}</td>", Convert.ToInt64(p_workingRow["taxTotal"])));
            _body.AppendLine("    		   </tr>");
            _body.AppendLine("    		 </table> ");
            _body.AppendLine("<!-- 세금계산서 발행일 -->    ");
            _body.AppendLine("    		 <table cellpadding='0' cellspacing='0' style='border-top: 2px #cccccc solid;border-left: 1px #cccccc solid; font-size:12px; color:#7b7b7b; font-family:굴림; line-height:25px; text-align:center; width:100%; margin-top:10px '>");
            _body.AppendLine("    		   <tr>");
            _body.AppendLine("    		     <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;background-color:#f2f1ed; width:85px'>작성일</td>");
            _body.AppendLine(String.Format("<td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid; width:135px'>{0:yyyy-MM-dd}</td>", Convert.ToDateTime(p_workingRow["issueDate"])));
            _body.AppendLine("    		     <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;background-color:#f2f1ed; width:85px'>발행일자</td>");
            _body.AppendLine(String.Format("<td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid; width:134px'>{0:yyyy-MM-dd}</td>", p_sentTime));
            _body.AppendLine("    		     <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;background-color:#f2f1ed; width:85px'>수정사유</td>");
            _body.AppendLine(String.Format("<td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid; width:134px'>{0}</td>", Convert.ToString(p_workingRow["description"])));
            _body.AppendLine("    		   </tr>");
            _body.AppendLine("    		 </table>");
            _body.AppendLine("    <!-- 인증키 -->");
            _body.AppendLine("    		 <table cellpadding='0' cellspacing='0' style='border-top: 2px #cccccc solid;border-left: 1px #cccccc solid; font-size:12px; color:#7b7b7b; font-family:굴림; line-height:25px; text-align:center; width:100%; margin-top:10px '>");
            _body.AppendLine("    		   <tr style='background-color:#f2f1ed'>");
            _body.AppendLine("    		     <td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid;'>전자세금계산서 확인시 필요한 인증키</td>");
            _body.AppendLine("    		   </tr>");
            _body.AppendLine("       		   <tr>");
            _body.AppendLine(String.Format("<td style='border-right: 1px #cccccc solid;border-bottom: 1px #cccccc solid; color:#023b63; font-weight:bold'>{0}</td>", Convert.ToString(p_workingRow["securityId"])));
            _body.AppendLine("    		   </tr>");
            _body.AppendLine("    		 </table>");
            _body.AppendLine("    		 <table cellpadding='0' cellspacing='0' border='0' style='width:100%; padding-top:20px; padding-bottom:30px; text-align:center'>");
            _body.AppendLine("    		   <tr>");
            _body.AppendLine(String.Format("<td><input name='Button1' type='button' value='지금 바로 확인하기' onclick=\"window.open('{0}outside/ebillView_outer.aspx?invoiceeId={1}&securityId={2}', 'viewer', 'toolbar=0,menubar=0,resizable=no,scrollbars=1,width=820,height=550,top=50,left=50');\"  style='font-size:12px; font-family:굴림;color:#7b7b7b ' /></td>", p_etaxurl, Convert.ToString(p_workingRow["invoiceeId"]), Convert.ToString(p_workingRow["securityId"])));
            _body.AppendLine("    		   </tr>");
            _body.AppendLine("    		 </table>");
            _body.AppendLine("    <!-- 하단 주소 -->");
            _body.AppendLine("    		 <table cellpadding='0' cellspacing='0' border='0' style=' background-color:#eaeaea;font-size:11px; color:#b1b0b0; font-family:돋움; line-height:15px; text-align:left; width:100%; padding:10px 0px 10px 0px '> ");
            _body.AppendLine("    		   <tr>");
            _body.AppendLine("    		     <td style='font-size:18px; font-family:UniversConBold; padding-left:20px; padding-right:30px'>eTax.uBizware</td>");
            _body.AppendLine(String.Format("<td>이 메일은 발신전용 메일이므로 회신이 되지 않습니다. 문의사항은 상담센터를 이용하시기 바랍니다.<br />{0}</td>", UAppHelper.OfficeAddress));
            _body.AppendLine("    		   </tr>");
            _body.AppendLine("    		 </table>");
            _body.AppendLine("        </td>");
            _body.AppendLine("      </tr>");
            _body.AppendLine("    </table>");
            _body.AppendLine("    </body>");
            _body.AppendLine("</html>");

            return _body.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_issuedRow"></param>
        public void MailerCallback(Object p_issuedRow)
        {
            var _sentTime = DateTime.Now;

            DataRow _issuedRow = (DataRow)p_issuedRow;
            DataRow _issuingRow = null;

            string _isMailSending = "F";
            string _isInvoiceeMail = "F";
            string _isProviderMail = "F";

            try
            {
                string _issueId = Convert.ToString(_issuedRow["issueId"]);
                string _document = Convert.ToString(_issuedRow["document"]);

                string _invoicerId = Convert.ToString(_issuedRow["invoicerId"]);
                string _invoicerEMail = Convert.ToString(_issuedRow["invoicerEMail"]);
                string _invoicerName = Convert.ToString(_issuedRow["invoicerName"]);

                _isInvoiceeMail = Convert.ToString(_issuedRow["isInvoiceeMail"]);
                string _invoiceeKind = Convert.ToString(_issuedRow["invoiceeKind"]);
                string _invoiceeId = Convert.ToString(_issuedRow["invoiceeId"]);
                string _invoiceeEMail = Convert.ToString(_issuedRow["invoiceeEMail"]);
                string _invoiceeName = Convert.ToString(_issuedRow["invoiceeName"]);

                _isProviderMail = Convert.ToString(_issuedRow["isProviderMail"]);
                string _providerId = Convert.ToString(_issuedRow["providerId"]);
                string _providerEMail = Convert.ToString(_issuedRow["providerEMail"]);

                lock (m_issuingTbl)
                {
                    _issuingRow = m_issuingTbl.NewRow();

                    _issuingRow["issueId"] = _issuedRow["issueId"];
                    _issuingRow["securityId"] = "";

                    _issuingRow["sendMailCount"] = 0;
                    _issuingRow["isMailSending"] = "X";      // update deltaset에서 original과 current를 비교하는 관계로 제3의 값을 할당 하여야 함.
                    _issuingRow["isInvoiceeMail"] = "X";
                    _issuingRow["isProviderMail"] = "X";
                    _issuingRow["mailSendingDate"] = DBNull.Value;
                }

                if (_isProviderMail != "T" && String.IsNullOrEmpty(_providerEMail) == false && String.IsNullOrEmpty(_providerId) == false)
                {
                    try
                    {
                        var _client = new MailRequestor(UAppHelper.DnsServers, UAppHelper.MailSniffing);

                        var _mime = new MimeConstructor
                                                {
                                                    From = _invoicerEMail,
                                                    To = _providerEMail.Split(';'),
                                                    Subject = String.Format("전자세금계산서가 {0}에서 발급되었습니다.", _invoicerName),
                                                    BodyHtml = GetProviderBody(_issuedRow)
                                                };

                        byte[] _obuffer = CmsManager.SNG.GetEncryptedContent(GetProviderKey(_invoiceeId), Encoding.UTF8.GetBytes(_document));
                        _mime.Attachments.Add(new Attachment(String.Format("{0}.msg", _issueId), _obuffer));

                        if (_client.Send(_mime.To, _mime.From, _mime.ConstructBinaryMime()) == false)
                        {
                            _isProviderMail = "E";

                            string _error = _client.ErrorMessage;
                            int _maxLength = m_resultTbl.Columns["message"].MaxLength;

                            if (_error.Length > _maxLength)
                                _error = _error.Substring(0, _maxLength);

                            lock (m_resultTbl)
                            {
                                var _resultRow = m_resultTbl.NewRow();

                                _resultRow["issueId"] = _issueId;
                                _resultRow["resultStatus"] = "SND002";

                                _resultRow["isDone"] = "F";
                                _resultRow["message"] = _error;
                                _resultRow["created"] = _sentTime;

                                m_resultTbl.Rows.Add(_resultRow);
                            }
                        }
                        else
                        {
                            _isProviderMail = "T";
                        }
                    }
                    catch (MailerException ex)
                    {
                        ELogger.SNG.WriteLog("X",
                            String.Format(
                                "raised error while send email: {0}, issueId->'{1}', isInvoiceeMail->'{2}', invoiceeEMail->{3}, isProviderMail->'{4}', providerEMail->{5}",
                                ex.Message, _issuedRow["issueId"],
                                _issuedRow["isInvoiceeMail"], _issuedRow["invoiceeEMail"],
                                _issuedRow["isProviderMail"], _issuedRow["providerEMail"]
                                )
                            );
                    }
                    catch (Exception ex)
                    {
                        ELogger.SNG.WriteLog(ex);
                    }
                }
                else if (String.IsNullOrEmpty(_providerEMail) == true)  // ASP 사업자 메일주소가 없는 경우
                {
                    _isProviderMail = "T";      // 메일 주소가 없는 경우 계속 발송되는 문제점이 있습니다.
                }

                if (_isInvoiceeMail != "T" && String.IsNullOrEmpty(_invoiceeEMail) == false)
                {
                    try
                    {
                        MailRequestor _client = new MailRequestor(UAppHelper.DnsServers, UAppHelper.MailSniffing);

                        var _mime = new MimeConstructor
                                                {
                                                    From = _invoicerEMail,
                                                    To = _invoiceeEMail.Split(';'),
                                                    Subject = String.Format("{0}님 전자세금계산서가 {1}에서 발급되었습니다.", _invoiceeName, _invoicerName),
                                                    BodyHtml = GetInvoiceeBody(UAppHelper.WebSiteUrl, _issuedRow)
                                                };

                        string _htmlBody = GetHtmlBody(UAppHelper.WebSiteUrl, _issuedRow, _sentTime);
                        using (var _etaxHtml = new MemoryStream(Encoding.UTF8.GetBytes(_htmlBody)))
                        {
                            using (var _etaxXml = new MemoryStream(Encoding.UTF8.GetBytes(_document)))
                            {
                                byte[] _obuffer = ZipStreams
                                    (
                                        new Stream[] { _etaxHtml, _etaxXml },
                                        new string[] { String.Format("요약({0}).htm", _issueId), String.Format("원본({0}).xml", _issueId) },
                                        _invoiceeId
                                    );

                                _mime.Attachments.Add(new Attachment(String.Format("전자세금계산서({0}).zip", _issueId), _obuffer));
                            }
                        }

                        if (_client.Send(_mime.To, _mime.From, _mime.ConstructBinaryMime()) == false)
                        {
                            _isInvoiceeMail = "E";          // error 

                            string _error = _client.ErrorMessage;
                            int _maxLength = m_resultTbl.Columns["message"].MaxLength;

                            if (_error.Length > _maxLength)
                                _error = _error.Substring(0, _maxLength);

                            lock (m_resultTbl)
                            {
                                var _resultRow = m_resultTbl.NewRow();

                                _resultRow["issueId"] = _issueId;
                                _resultRow["resultStatus"] = "SND001";

                                _resultRow["isDone"] = "F";
                                _resultRow["message"] = _error;
                                _resultRow["created"] = _sentTime;

                                m_resultTbl.Rows.Add(_resultRow);
                            }
                        }
                        else
                        {
                            _isInvoiceeMail = "T";

                            if (UAppHelper.MailSniffing == true)
                            {
                                string _directory = Path.Combine(UAppHelper.eMailFolder, IMailer.Proxy.ProductId);
                                {
                                    _directory = Path.Combine(_directory, _mime.To[0]);
                                    _directory = Path.Combine(_directory, "outbox");
                                    _directory = Path.Combine(_directory, _sentTime.ToString("yyyyMMdd"));
                                }

                                string _filename = Guid.NewGuid() + ".eml";
                                QFWriter.QueueWrite(_directory, _filename, _mime.ConstructBinaryMime().ToArray());
                            }
                        }
                    }
                    catch (MailerException ex)
                    {
                        ELogger.SNG.WriteLog("X",
                            String.Format(
                                "raised error while send email: {0}, issueId->'{1}', isInvoiceeMail->'{2}', invoiceeEMail->{3}, isProviderMail->'{4}', providerEMail->{5}",
                                ex.Message, _issuedRow["issueId"],
                                _issuedRow["isInvoiceeMail"], _issuedRow["invoiceeEMail"],
                                _issuedRow["isProviderMail"], _issuedRow["providerEMail"]
                                )
                            );
                    }
                    catch (Exception ex)
                    {
                        ELogger.SNG.WriteLog(ex);
                    }
                }

                _isMailSending = "T";
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            finally
            {
                lock (m_issuingTbl)
                {
                    m_issuingTbl.Rows.Add(_issuingRow);
                    _issuingRow.AcceptChanges();

                    int _sendMailCount = Convert.ToInt32(_issuedRow["sendMailCount"]);

                    _issuingRow["securityId"] = _issuedRow["securityId"];
                    _issuingRow["sendMailCount"] = _sendMailCount >= 99 ? _sendMailCount : _sendMailCount + 1;

                    _issuingRow["mailSendingDate"] = _sentTime;
                    _issuingRow["isInvoiceeMail"] = _isInvoiceeMail;
                    _issuingRow["isProviderMail"] = _isProviderMail;
                    _issuingRow["isMailSending"] = _isMailSending;
                }
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
                if (m_imailer != null)
                {
                    m_imailer.Dispose();
                    m_imailer = null;
                }
                if (m_issuingTbl != null)
                {
                    m_issuingTbl.Dispose();
                    m_issuingTbl = null;
                }
                if (m_resultTbl != null)
                {
                    m_resultTbl.Dispose();
                    m_resultTbl = null;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~AsyncWorker()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}