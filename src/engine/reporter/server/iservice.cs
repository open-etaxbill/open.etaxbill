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
using System.ServiceModel;

namespace OpenETaxBill.Engine.Reporter
{
    [ServiceContract(Name = "IReportService", Namespace = "http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07", SessionMode = SessionMode.Allowed)]
    public interface IReportService
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
        /// 공급자(수탁자)가 발행한 세금계산서를 기간별로 수동으로 국세청에 신고한다.
        /// </summary>
        /// <param name="p_invoicerId">공급자 또는 수탁자 사업자번호</param>
        /// <param name="p_fromDay">계산서발행 시작일자</param>
        /// <param name="p_tillDay">계산서발행 종료일자</param>
        /// <returns>성공 true, 실패 false</returns>
        [OperationContract(Name = "ReportWithDateRange")]
        int ReportWithDateRange(Guid p_certapp, string p_invoicerId, DateTime p_fromDay, DateTime p_tillDay);

        /// <summary>
        /// 공급자(수탁자)가 발행한 세금계산서를 issueId별로 수동으로 국세청에 신고한다.
        /// 결과적으로 p_issueIds는 국세청 전송 묶음이 된다.
        /// </summary>
        /// <param name="p_invoicerId">공급자 또는 수탁자 사업자번호</param>
        /// <param name="p_issueIds">승인번호(1~100)</param>
        /// <returns>성공 true, 실패 false</returns>
        [OperationContract(Name = "ReportWithIssueIDs")]
        int ReportWithIssueIDs(Guid p_certapp, string p_invoicerId, string[] p_issueIds);

        /// <summary>
        /// 국세청에 수신결과를 수동으로 요청한다.
        /// </summary>
        /// <param name="p_submitId">제출아이디</param>
        /// <returns>성공 true, 실패 false</returns>
        [OperationContract(Name = "RequestResult")]
        bool RequestResult(Guid p_certapp, string p_submitId);

        /// <summary>
        /// clear X flag
        /// </summary>
        /// <param name="p_invoicerId"></param>
        /// <returns></returns>
        [OperationContract(Name = "ClearXFlag")]
        int ClearXFlag(Guid p_certapp, string p_invoicerId);
    }
}