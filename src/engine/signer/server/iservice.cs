using System;
using System.ServiceModel;

namespace OpenETaxBill.Engine.Signer
{
    [ServiceContract(Name = "ISignerService", Namespace = "http://www.odinsoftware.co.kr/open/etaxbill/signer/2016/07", SessionMode = SessionMode.Allowed)]
    public interface ISignerService
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
        /// 공급자(수탁자)가 작성한 세금계산서를 기간별로 수동으로 서명한다.
        /// </summary>
        /// <param name="p_invoicerId">공급자 또는 수탁자 사업자번호</param>
        /// <param name="p_fromDay">작성 시작일자</param>
        /// <param name="p_tillDay">작성 종료일자</param>
        /// <returns>성공 true, 실패 false</returns>
        [OperationContract(Name = "SignatureWithDateRange")]
        int SignatureWithDateRange(Guid p_certapp, string[] p_certifier, string p_invoicerId, DateTime p_fromDay, DateTime p_tillDay);

        /// <summary>
        /// 공급자(수탁자)가 작성한 세금계산서를 issueId별로 수동으로 서명한다.
        /// </summary>
        /// <param name="p_certifier">공인인증서 공개키,개인키,암호</param>
        /// <param name="p_invoicerId">공급자 또는 수탁자 사업자번호</param>
        /// <param name="p_issueIDs">승인번호(1~100)</param>
        /// <returns>성공 갯수</returns>
        [OperationContract(Name = "SignatureWithIDs")]
        int SignatureWithIDs(Guid p_certapp, string[] p_certifier, string p_invoicerId, string[] p_issueIds);

        /// <summary>
        /// clear X flag
        /// </summary>
        /// <param name="p_invoicerId"></param>
        /// <returns></returns>
        [OperationContract(Name = "ClearXFlag")]
        int ClearXFlag(Guid p_certapp, string p_invoicerId);

    }
}