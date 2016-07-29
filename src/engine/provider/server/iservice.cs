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

namespace OpenETaxBill.Engine.Provider
{
    [ServiceContract(Name = "IProviderService", Namespace = "http://www.odinsoftware.co.kr/open/etaxbill/provider/2016/07", SessionMode = SessionMode.Allowed)]
    public interface IProviderService
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
        /// clear X flag
        /// </summary>
        /// <param name="p_greeting"></param>
        /// <returns></returns>
        [OperationContract(Name = "HelloWorld")]
        string HelloWorld(Guid p_certapp, string p_greeting);
    }
}