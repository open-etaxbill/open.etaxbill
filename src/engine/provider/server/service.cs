using System;
using System.ServiceModel;

namespace OpenETaxBill.Engine.Provider
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerSession, IncludeExceptionDetailInFaults = true)]
    public class ProviderService : IProviderService, IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenETaxBill.Channel.Interface.IProvider m_iprovider = null;
        private OpenETaxBill.Channel.Interface.IProvider IProvider
        {
            get
            {
                if (m_iprovider == null)
                    m_iprovider = new OpenETaxBill.Channel.Interface.IProvider();

                return m_iprovider;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // logger
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_certapp"></param>
        /// <param name="p_exception"></param>
        /// <param name="p_message"></param>
        public void WriteLog(Guid p_certapp, string p_exception, string p_message)
        {
            if (IProvider.CheckValidApplication(p_certapp) == true)
                ELogger.SNG.WriteLog(p_exception, p_message);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_certapp"></param>
        /// <param name="p_greeting"></param>
        /// <returns></returns>
        public string HelloWorld(Guid p_certapp, string p_greeting)
        {
            return p_greeting + " Hello World!";
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
                if (m_iprovider != null)
                {
                    m_iprovider.Dispose();
                    m_iprovider = null;
                }
        }

        /// <summary>
        /// 
        /// </summary>
        ~ProviderService()
        {
            Dispose(false);
        }
 
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
}
}