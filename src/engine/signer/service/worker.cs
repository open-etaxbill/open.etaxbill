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
using System.Threading;
using OpenETaxBill.Engine.Library;
using OpenETaxBill.Channel.Library.Security.Signature;
using OdinSoft.SDK.Configuration;
using OdinSoft.SDK.Data;
using OdinSoft.SDK.Data.Collection;

namespace OpenETaxBill.Engine.Signer
{
    public class Worker : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenETaxBill.Channel.Interface.ISigner m_isigner = null;
        private OpenETaxBill.Channel.Interface.ISigner ISigner
        {
            get
            {
                if (m_isigner == null)
                    m_isigner = new OpenETaxBill.Channel.Interface.ISigner();

                return m_isigner;
            }
        }
        
        private OpenETaxBill.Engine.Library.UAppHelper m_appHelper = null;
        public OpenETaxBill.Engine.Library.UAppHelper UAppHelper
        {
            get
            {
                if (m_appHelper == null)
                    m_appHelper = new OpenETaxBill.Engine.Library.UAppHelper(ISigner.Manager);

                return m_appHelper;
            }
        }

        private OpenETaxBill.Engine.Library.UCertHelper m_certHelper = null;
        public OpenETaxBill.Engine.Library.UCertHelper UCertHelper
        {
            get
            {
                if (m_certHelper == null)
                    m_certHelper = new OpenETaxBill.Engine.Library.UCertHelper(ISigner.Manager);

                return m_certHelper;
            }
        }

        private OdinSoft.SDK.Data.DataHelper m_dataHelper = null;
        private OdinSoft.SDK.Data.DataHelper LDataHelper
        {
            get
            {
                if (m_dataHelper == null)
                    m_dataHelper = new OdinSoft.SDK.Data.DataHelper();
                return m_dataHelper;
            }
        }

        private OpenETaxBill.Engine.Signer.Engine m_esigner = null;
        private OpenETaxBill.Engine.Signer.Engine ESigner
        {
            get
            {
                if (m_esigner == null)
                    m_esigner = new OpenETaxBill.Engine.Signer.Engine();

                return m_esigner;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private System.Threading.Timer SignatureTimer;

        private void SignerWorking()
        {
            // clear 'X' flag
            {
                int _norec = ESigner.ClearXFlag();
                ELogger.SNG.WriteLog(String.Format("While starting, updated to 'F' {0} record(s) that isIssued was 'X'.", _norec));
            }

            // Do not use using statement
            AutoResetEvent _autoEvent = new AutoResetEvent(false);
            {
                SignatureTimer = new Timer(SignerWakeup, _autoEvent, TimeSpan.FromSeconds(1).Milliseconds, Timeout.Infinite);
                
                int _iteration = 0;
                ELogger.SNG.WriteLog
                    (
                        String.Format
                        (
                            "productid->{0}, liveServer->{1}, signerDueTime->{2}, debugMode->{3}, mailSniffing->{4}, soapFiltering->{5}, rootCertFolder->{6}",
                            UAppHelper.QMaster.ProductId, UAppHelper.LiveServer, UAppHelper.SignerDueTime, CfgHelper.SNG.DebugMode,
                            UAppHelper.MailSniffing, UAppHelper.SoapFiltering, UAppHelper.RootCertFolder
                        )
                    );
                
                while (_autoEvent.WaitOne() == true && ShouldStop == false)
                    ISigner.WriteDebug(String.Format("waiting: {0}...", ++_iteration));
                
                SignatureTimer.Dispose();
            }
        }

        private void SignerWakeup(object stateInfo)
        {
            var _autoEvent = (AutoResetEvent)stateInfo;

            try
            {
                ISigner.WriteDebug("wakeup...");

                DateTime _fromDay = UTextHelper.SNG.GetFirstDayOfLastMonth();
                DateTime _tillDay = UTextHelper.SNG.GetLastDayOfThisMonth();

                UTextHelper.SNG.GetSigningRange(ref _fromDay, ref _tillDay);

                // check table for auto-signing
                string _sqlstr
                        = "SELECT invoicerId, COUNT(invoicerId) as norec, @fromDay as fromDay, @tillDay as tillDay "
                        + "  FROM TB_eTAX_INVOICE "
                        + " WHERE isSuccess != @isSuccess "                              // for resignning, do check 'isSuccess' here 
                        + "   AND issueDate>=@fromDay AND issueDate<=@tillDay "
                        + " GROUP BY invoicerId";

                var _dbps = new DatParameters();
                _dbps.Add("@isSuccess", SqlDbType.NVarChar, "T");
                _dbps.Add("@fromDay", SqlDbType.DateTime, _fromDay);
                _dbps.Add("@tillDay", SqlDbType.DateTime, _tillDay);

                var _ds = LDataHelper.SelectDataSet(UAppHelper.ConnectionString, _sqlstr, _dbps);
                if (LDataHelper.IsNullOrEmpty(_ds) == false)
                {
                    var _rows = _ds.Tables[0].Rows;
                    ELogger.SNG.WriteLog(String.Format("selected invoicer(s): {0} ", _rows.Count));

                    var _doneEvents = new ThreadPoolWait[_rows.Count];
                    for (int i = 0; i < _rows.Count; i++)
                    {
                        _doneEvents[i] = new ThreadPoolWait();
                        _doneEvents[i].QueueUserWorkItem(SignerCallback, _rows[i]);

                        if (Environment.UserInteractive == true)
                            _doneEvents[i].WaitOne();
                    }

                    ThreadPoolWait.WaitForAll(_doneEvents);
                }
            }
            catch (SignerException ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            finally
            {
                ISigner.WriteDebug("sleep...");

                SignatureTimer.Change(UAppHelper.SignerDueTime, Timeout.Infinite);
                _autoEvent.Set();
            }
        }

        private void SignerCallback(Object p_invoicer)
        {
            var _signingDay = DateTime.Now;

            try
            {
                DataRow _invoicerRow = (DataRow)p_invoicer;

                string _invoicerId = Convert.ToString(_invoicerRow["invoicerId"]);
                int _noInvoicee = Convert.ToInt32(_invoicerRow["norec"]);

                DateTime _fromDay = Convert.ToDateTime(_invoicerRow["fromDay"]);
                DateTime _tillDay = Convert.ToDateTime(_invoicerRow["tillDay"]);

                DataSet _customerSet = ESigner.GetCustomerSet(_invoicerId);
                DataRow _customerRow = _customerSet.Tables[0].Rows[0];

                decimal _fromSigningDay = Convert.ToDecimal(_customerRow["signFromDay"]);
                decimal _tillSigningDay = Convert.ToDecimal(_customerRow["signTillDay"]);

                string _signingType = Convert.ToString(_customerRow["signingType"]);
                if (_signingType == "01" || _signingType == "03")
                {
                    if (_signingType == "01")
                    {
                        decimal _today = _signingDay.Day;
                        if (_today < _fromSigningDay || _today > _tillSigningDay)
                            throw new SignerException(
                                    String.Format(
                                        "out of range sign-period: invoicerId->'{0}', fromDay->{1}, tillDay->{2}, toDay->{3}",
                                        _invoicerId, _fromSigningDay, _tillSigningDay, _today
                                    )
                                );
                    }

                    X509CertMgr _invoicerCert = UCertHelper.GetCustomerCertMgr(_invoicerId);
                    ESigner.DoSignInvoice(_invoicerCert, _invoicerId, _noInvoicee, _fromDay, _tillDay);
                }
                else
                {
                    ISigner.WriteDebug(String.Format("invoicer '{0}' is skipped {1} record(s) because sign-type is {2}.", _invoicerId, _noInvoicee, _signingType));
                }
            }
            catch (SignerException ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private System.Threading.Thread SignatureThread;
        private volatile bool ShouldStop;

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            ShouldStop = false;

            SignatureThread = new Thread(SignerWorking);
            SignatureThread.Start();

            while (SignatureThread.IsAlive == false)
                Thread.Sleep(100);

            Thread.Sleep(1);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            ShouldStop = true;

            if (SignatureThread != null)
            {
                SignatureThread.Abort();
                Thread.Sleep(1000);
            }
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
                if (m_isigner != null)
                {
                    m_isigner.Dispose();
                    m_isigner = null;
                }
                if (SignatureTimer != null)
                {
                    SignatureTimer.Dispose();
                    SignatureTimer = null;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~Worker()
        {
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
    }
}