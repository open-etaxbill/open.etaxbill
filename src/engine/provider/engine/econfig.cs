using System;
using System.Configuration;
using System.Net.Mail;
using OpenETaxBill.SDK.Configuration;

namespace OpenETaxBill.Engine.Provider
{
    /// <summary>
    /// 
    /// </summary>
    public class EConfig : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private readonly static Lazy<EConfig> m_lzyHelper = new Lazy<EConfig>(() => new EConfig());

        /// <summary>
        /// 
        /// </summary>
        public static EConfig SNG
        {
            get
            {
                return m_lzyHelper.Value;
            }
        }

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

        private OpenETaxBill.Engine.Library.USvcHelper m_svcHelper = null;
        public OpenETaxBill.Engine.Library.USvcHelper USvcHelper
        {
            get
            {
                if (m_svcHelper == null)
                    m_svcHelper = new OpenETaxBill.Engine.Library.USvcHelper(IProvider.Manager);

                return m_svcHelper;
            }
        }

        private string GetAppValue(string p_appkey)
        {
            return ConfigurationManager.AppSettings[p_appkey];
        }

        private string GetRegValue(string p_regkey, string p_default = "")
        {
            var _appValue = GetAppValue(p_regkey) ?? p_default;
            return RegHelper.SNG.GetServer(IProvider.Manager.CategoryId, IProvider.Manager.ProductId, p_regkey, _appValue);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private int? m_remind_history_term;

        /// <summary>
        /// 
        /// </summary>
        public int RemindHistoryTerm
        {
            get
            {
                if (m_remind_history_term == null)
                    m_remind_history_term = Convert.ToInt32(GetRegValue("RemindHistoryTerm"));

                return m_remind_history_term.Value;
            }
        }

        private int? m_rangeOfOrderMonth;

        /// <summary>
        /// 
        /// </summary>
        public int RangeOfOrderMonth
        {
            get
            {
                if (m_rangeOfOrderMonth == null)
                    m_rangeOfOrderMonth = Convert.ToInt32(GetRegValue("RangeOfOrderMonth"));

                return m_rangeOfOrderMonth.Value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        #region IDisposable Members

        /// <summary>
        ///
        /// </summary>
        private bool IsDisposed
        {
            get;
            set;
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the backing store before garbage collection.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> if disposing; otherwise, <see langword="false"/>.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources. 
                    if (m_iprovider != null)
                    {
                        m_iprovider.Dispose();
                        m_iprovider = null;
                    }
                }

                // Dispose unmanaged resources. 

                // Note disposing has been done. 
                IsDisposed = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~EConfig()
        {
            Dispose(false);
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}