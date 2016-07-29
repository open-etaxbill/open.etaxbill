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
using System.ServiceModel;

namespace OpenETaxBill.Engine.Collector
{
    [ServiceContract(Name = "ICollectorService", Namespace = "http://www.odinsoftware.co.kr/open/etaxbill/collector/2016/07", SessionMode = SessionMode.Allowed)]
    public interface ICollectorService
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
        /// 엑셀에서 추출한 테이블을 저장한다. 1,000개까지 처리한다. 
        /// </summary>
        /// <param name="m_UploadTable">Upload Excel Table</param>
        /// <param name="_createdBy">created id</param>
        /// <returns></returns>
        [OperationContract(Name = "DoExcelUpload")]
        bool DoExcelUpload(Guid p_certapp, DataTable p_uploadTable, string p_createdBy);

        /// <summary>
        /// 선택한 일자의 IssueId를 구한다.
        /// </summary>
        /// <param name="p_createDate">선택일자</param>
        /// <returns>New IssueId</returns>
        [OperationContract(Name = "GetIssueId")]
        string GetIssueId(Guid p_certapp, DateTime p_createDate);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_appkey"></param>
        /// <returns></returns>
        [OperationContract(Name = "GetCfgValue")]
        string GetCfgValue(Guid p_certapp, string p_appkey, string p_default);
    }
}