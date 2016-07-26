using System;
using System.Data;
using System.Reflection;
using System.ServiceModel;

namespace OpenETaxBill.Engine.Collector
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerSession, IncludeExceptionDetailInFaults = true)]
    public class CollectService : ICollectorService, IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenETaxBill.Channel.Interface.ICollector m_icollector = null;
        private OpenETaxBill.Channel.Interface.ICollector ICollector
        {
            get
            {
                if (m_icollector == null)
                    m_icollector = new OpenETaxBill.Channel.Interface.ICollector();

                return m_icollector;
            }
        }

        private OpenETaxBill.Engine.Collector.Engine m_ecollector = null;
        private OpenETaxBill.Engine.Collector.Engine ECollector
        {
            get
            {
                if (m_ecollector == null)
                    m_ecollector = new OpenETaxBill.Engine.Collector.Engine();

                return m_ecollector;
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
            if (ICollector.CheckValidApplication(p_certapp) == true)
                ELogger.SNG.WriteLog(p_exception, p_message);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_certapp"></param>
        /// <param name="p_uploadTable"></param>
        /// <param name="p_createdBy"></param>
        /// <returns></returns>
        public bool DoExcelUpload(Guid p_certapp, DataTable p_uploadTable, string p_createdBy)
        {
            var _result = false;

            try
            {
                if (ICollector.CheckValidApplication(p_certapp) == true)
                    _result = ECollector.DoExcelUpload(p_uploadTable, p_createdBy);
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }

            return _result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_certapp"></param>
        /// <param name="p_createDate"></param>
        /// <returns></returns>
        public string GetIssueId(Guid p_certapp, DateTime p_createDate)
        {
            var _result = "";

            try
            {
                if (ICollector.CheckValidApplication(p_certapp) == true)
                    _result = ECollector.GetIssueId(p_createDate);
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }

            return _result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_certapp"></param>
        /// <param name="p_appkey"></param>
        /// <returns></returns>
        public string GetCfgValue(Guid p_certapp, string p_appkey, string p_default)
        {
            var _result = "";

            try
            {
                if (ICollector.CheckValidApplication(p_certapp) == true)
                    _result = ECollector.GetCfgValue(p_appkey, p_default);
                }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            
            return _result;
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
                if (m_icollector != null)
                {
                    m_icollector.Dispose();
                    m_icollector = null;
                }
                if (m_ecollector != null)
                {
                    m_ecollector.Dispose();
                    m_ecollector = null;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~CollectService()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
    }
}