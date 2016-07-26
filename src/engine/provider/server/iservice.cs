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