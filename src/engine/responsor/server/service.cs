using System;
using System.ServiceModel;

namespace OpenETaxBill.Engine.Responsor
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerSession, IncludeExceptionDetailInFaults=true)]
    public class ResponseService : IResponseService, IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenETaxBill.Channel.Interface.IResponsor m_iresponsor = null;
        private OpenETaxBill.Channel.Interface.IResponsor IResponsor
        {
            get
            {
                if (m_iresponsor == null)
                    m_iresponsor = new OpenETaxBill.Channel.Interface.IResponsor();

                return m_iresponsor;
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
            if (IResponsor.CheckValidApplication(p_certapp) == true)
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
                if (m_iresponsor != null)
                {
                    m_iresponsor.Dispose();
                    m_iresponsor = null;
                }
        }

        /// <summary>
        /// 
        /// </summary>
        ~ResponseService()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}
