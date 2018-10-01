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
using System.Threading;
using OpenETaxBill.Engine.Library;
using OdinSdk.OdinLib.Configuration;

namespace OpenETaxBill.Engine.Collector
{
    /// <summary>
    /// 
    /// </summary>
    public class Worker : IDisposable
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

        private OpenETaxBill.Engine.Library.UAppHelper m_appHelper = null;
        public OpenETaxBill.Engine.Library.UAppHelper UAppHelper
        {
            get
            {
                if (m_appHelper == null)
                    m_appHelper = new OpenETaxBill.Engine.Library.UAppHelper(ICollector.Manager);

                return m_appHelper;
            }
        }
        
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private System.Threading.Timer CollectTimer;

        private void CollectorWorking()
        {   
            // Do not use using statement
            AutoResetEvent _autoEvent = new AutoResetEvent(false);
            {
                CollectTimer = new Timer(CollectorWakeup, _autoEvent, TimeSpan.FromSeconds(1).Milliseconds, Timeout.Infinite);
                
                int _iteration = 0;

                ELogger.SNG.WriteLog
                    (
                        String.Format
                        (
                            "productid->{0}, liveServer->{1}, collectorDueTime->{2}, debugMode->{3}, mailSniffing->{4}, soapFiltering->{5}", 
                            UAppHelper.QMaster.ProductId, UAppHelper.LiveServer, UAppHelper.CollectorDueTime, CfgHelper.SNG.DebugMode, 
                            UAppHelper.MailSniffing, UAppHelper.SoapFiltering
                        )
                    );

                while (_autoEvent.WaitOne() == true && ShouldStop == false)
                    ICollector.WriteDebug(String.Format("waiting: {0}...", ++_iteration));

                CollectTimer.Dispose();
            }
        }

        private void CollectorWakeup(object stateInfo)
        {
            var _autoEvent = (AutoResetEvent)stateInfo;

            try
            {
                ICollector.WriteDebug("wakeup...");

                int _norec = 1;

                var _doneEvents = new ThreadPoolWait[_norec];
                for (int i = 0; i < _norec; i++)
                {
                    _doneEvents[i] = new ThreadPoolWait();
                    _doneEvents[i].QueueUserWorkItem(CollectorCallback, null);

                    if (Environment.UserInteractive == true)
                        _doneEvents[i].WaitOne();
                }

                ThreadPoolWait.WaitForAll(_doneEvents);
            }
            catch (CollectException ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            finally
            {
                ICollector.WriteDebug("sleep...");

                CollectTimer.Change(UAppHelper.CollectorDueTime, Timeout.Infinite);
                _autoEvent.Set();
            }
        }
        
        private void CollectorCallback(Object p_invoicer)
        {
            try
            {
                if (UAppHelper.LiveServer == true && UAppHelper.UpdateCert == true)
                {
                    int _noProvider = ECollector.DoUpdateCert();
                    ELogger.SNG.WriteLog(String.Format("have updated {0} provider's certification(s)", _noProvider));
                }
                else
                {
                    ELogger.SNG.WriteLog("for testing, could not update provider's certification(s)");
                }
            }
            catch (CollectException ex)
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
        private System.Threading.Thread CollectThread;
        private volatile bool ShouldStop;

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            ShouldStop = false;

            CollectThread = new Thread(CollectorWorking);
            CollectThread.Start();

            while (CollectThread.IsAlive == false)
                Thread.Sleep(100);

            Thread.Sleep(1);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            ShouldStop = true;

            if (CollectThread != null)
            {
                CollectThread.Abort();
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
                if (CollectTimer != null)
                {
                    CollectTimer.Dispose();
                    CollectTimer = null;
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