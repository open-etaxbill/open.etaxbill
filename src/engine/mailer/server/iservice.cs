using System;
using System.ServiceModel;

namespace OpenETaxBill.Engine.Mailer
{
    [ServiceContract(Name = "IMailerService", Namespace = "http://www.odinsoftware.co.kr/open/etaxbill/mailer/2016/07", SessionMode = SessionMode.Allowed)]
    public interface IMailerService
    {
        /// <summary>
        /// 로그 작성기
        /// </summary>
        /// <param name="p_certkey"></param>
        /// <param name="p_exception"></param>
        /// <param name="p_message"></param>
        [OperationContract(Name = "WriteLog")]
        void WriteLog(Guid p_certapp, string p_exception, string p_message);

        /// <summary>
        /// 공급자(수탁자)가 서명한 세금계산서를 기간별로 수동으로 발송한다.
        /// </summary>
        /// <param name="p_invoicerId">공급자 또는 수탁자 사업자번호</param>
        /// <param name="p_fromDay">서명 시작일자</param>
        /// <param name="p_tillDay">서명 종료일자</param>
        /// <returns>성공 true, 실패 false</returns>
        [OperationContract(Name = "SendWithDateRange")]
        int SendWithDateRange(Guid p_certapp, string p_invoicerId, DateTime p_fromDay, DateTime p_tillDay);

        /// <summary>
        /// 공급자(수탁자)가 서명한 세금계산서를 issueId별로 수동으로 발송한다.
        /// </summary>
        /// <param name="p_invoicerId">공급자 또는 수탁자 사업자번호</param>
        /// <param name="p_issueIDs">승인번호(1~100)</param>
        /// <returns>성공 true, 실패 false</returns>
        [OperationContract(Name = "SendWithIssueIDs")]
        int SendWithIssueIDs(Guid p_certapp, string p_invoicerId, string[] p_issueIDs);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_invoicerId"></param>
        /// <param name="p_issue_id"></param>
        /// <param name="p_newMailAddress"></param>
        /// <returns></returns>
        [OperationContract(Name = "ReSendWithIssueID")]
        int ReSendWithIssueID(Guid p_certapp, string p_invoicerId, string p_issue_id, string p_newMailAddress);

        /// <summary>
        /// clear X flag
        /// </summary>
        /// <param name="p_invoicerId"></param>
        /// <returns></returns>
        [OperationContract(Name = "ClearXFlag")]
        int ClearXFlag(Guid p_certapp, string p_invoicerId);
    }
}