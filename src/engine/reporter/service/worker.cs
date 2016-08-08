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
using NpgsqlTypes;
using OdinSoft.SDK.Configuration;
using OdinSoft.SDK.Data.POSTGRESQL;

namespace OpenETaxBill.Engine.Reporter
{
    public class Worker : IDisposable
    {
        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private OpenETaxBill.Channel.Interface.IReporter m_ireporter = null;
        private OpenETaxBill.Channel.Interface.IReporter IReporter
        {
            get
            {
                if (m_ireporter == null)
                    m_ireporter = new OpenETaxBill.Channel.Interface.IReporter();

                return m_ireporter;
            }
        }
        
        private OpenETaxBill.Engine.Library.UAppHelper m_appHelper = null;
        public OpenETaxBill.Engine.Library.UAppHelper UAppHelper
        {
            get
            {
                if (m_appHelper == null)
                    m_appHelper = new OpenETaxBill.Engine.Library.UAppHelper(IReporter.Manager);

                return m_appHelper;
            }
        }

        private OdinSoft.SDK.Data.POSTGRESQL.PgDataHelper m_dataHelper = null;
        private OdinSoft.SDK.Data.POSTGRESQL.PgDataHelper LDataHelper
        {
            get
            {
                if (m_dataHelper == null)
                    m_dataHelper = new OdinSoft.SDK.Data.POSTGRESQL.PgDataHelper();

                return m_dataHelper;
            }
        }

        private OpenETaxBill.Engine.Reporter.Engine m_ereporter = null;
        private OpenETaxBill.Engine.Reporter.Engine EReporter
        {
            get
            {
                if (m_ereporter == null)
                    m_ereporter = new OpenETaxBill.Engine.Reporter.Engine();

                return m_ereporter;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------
        // 
        //-------------------------------------------------------------------------------------------------------------------------
        private System.Threading.Timer ReportingTimer;

        private void ReporterWorking()
        {
            // clear 'X' flag
            {
                int _norec = EReporter.ClearXFlag();
                ELogger.SNG.WriteLog(String.Format("While starting, updated to 'F' {0} record(s) that isNTSSending was 'X'.", _norec));
            }

            // Do not use using statement
            AutoResetEvent _autoEvent = new AutoResetEvent(false);
            {
				ReportingTimer = new Timer(ReporterWakeup, _autoEvent, TimeSpan.FromSeconds(1).Milliseconds, Timeout.Infinite);

                int _iteration = 0;

                ELogger.SNG.WriteLog
                      (
                          String.Format
                          (
                              "productid->{0}, liveServer->{1}, reporterDueTime->{2}, debugMode->{3}, mailSniffing->{4}, soapFiltering->{5}, noThreadOfReporter->{6}",
                              UAppHelper.QMaster.ProductId, UAppHelper.LiveServer, UAppHelper.ReporterDueTime, CfgHelper.SNG.DebugMode,
                              UAppHelper.MailSniffing, UAppHelper.SoapFiltering, UAppHelper.NoThreadOfReporter
                          )
                      );
                
                while (_autoEvent.WaitOne() == true && ShouldStop == false)
                    IReporter.WriteDebug(String.Format("waiting: {0}...", ++_iteration));
                
                ReportingTimer.Dispose();
            }
        }

        private void ReporterWakeup(object stateInfo)
        {
            var _autoEvent = (AutoResetEvent)stateInfo;

            try
            {
				IReporter.WriteDebug("wakeup...");

				var _nday = DateTime.Now;
				DateTime _pday = _nday.AddDays(-1);

				DateTime _fromDay = new DateTime(_pday.Year, _pday.Month, _pday.Day);
				DateTime _tillDay = new DateTime(_nday.Year, _nday.Month, _nday.Day);

				// check table for auto-reporting
				string _sqlstr
					= "SELECT b.invoicerId, COUNT(b.invoicerId) as norec, @fromDay as fromDay, @tillDay as tillDay "
					+ "  FROM TB_eTAX_ISSUING a INNER JOIN TB_eTAX_INVOICE b "
					+ "    ON a.issueId=b.issueId "
					+ " WHERE a.isNTSReport != @isNTSReport "
					+ "   AND b.issueDate>=@fromDay AND b.issueDate<@tillDay "
					+ " GROUP BY b.invoicerId";

				var _dbps = new PgDatParameters();
                {
                    _dbps.Add("@isNTSReport", NpgsqlDbType.Varchar, "T");
                    _dbps.Add("@fromDay", NpgsqlDbType.TimestampTZ, _fromDay);
                    _dbps.Add("@tillDay", NpgsqlDbType.TimestampTZ, _tillDay);
                }

				var _ds = LDataHelper.SelectDataSet(UAppHelper.ConnectionString, _sqlstr, _dbps);
				if (LDataHelper.IsNullOrEmpty(_ds) == false)
				{
					var _rows = _ds.Tables[0].Rows;
					ELogger.SNG.WriteLog(String.Format("selected invoicer(s): {0} ", _rows.Count));

					var _doneEvents = new ThreadPoolWait[_rows.Count];
					for (int i = 0; i < _rows.Count; i++)
					{
						_doneEvents[i] = new ThreadPoolWait();
						_doneEvents[i].QueueUserWorkItem(ReporterBackWork, _rows[i]);

                        if (Environment.UserInteractive == true)
                            _doneEvents[i].WaitOne();
					}

					ThreadPoolWait.WaitForAll(_doneEvents);
				}
            }
            catch (ReporterException ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            catch (Exception ex)
            {
                ELogger.SNG.WriteLog(ex);
            }
            finally
            {
                IReporter.WriteDebug("sleep...");

                ReportingTimer.Change(UAppHelper.ReporterDueTime, Timeout.Infinite);
                _autoEvent.Set();
            }
        }
        
        private void ReporterBackWork(Object p_invoicer)
        {
			try
			{
				DataRow _invoicerRow = (DataRow)p_invoicer;

				string _invoicerId = Convert.ToString(_invoicerRow["invoicerId"]);
				int _noIssuing = Convert.ToInt32(_invoicerRow["norec"]);

				DateTime _fromDay = Convert.ToDateTime(_invoicerRow["fromDay"]);
				DateTime _tillDay = Convert.ToDateTime(_invoicerRow["tillDay"]);

				IReporter.WriteDebug(String.Format("invoicer '{0}' is repoting {1} record(s) ", _invoicerId, _noIssuing));
				EReporter.DoReportInvoicer(_invoicerId, _noIssuing, _fromDay, _tillDay);
			}
			catch (ReporterException ex)
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
        private System.Threading.Thread ReportThread;
        private volatile bool ShouldStop;

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            ShouldStop = false;

            ReportThread = new Thread(ReporterWorking);
            ReportThread.Start();

            while (ReportThread.IsAlive == false)
                Thread.Sleep(100);

            Thread.Sleep(1);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            ShouldStop = true;

            if (ReportThread != null)
            {
                ReportThread.Abort();
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
                if (m_ireporter != null)
                {
                    m_ireporter.Dispose();
                    m_ireporter = null;
                }

				if (m_ereporter != null)
				{
					m_ereporter.Dispose();
					m_ereporter = null;
				}

				if (m_appHelper != null)
				{
					m_appHelper.Dispose();
					m_appHelper = null;
				}
				
                if (ReportingTimer != null)
                {
                    ReportingTimer.Dispose();
                    ReportingTimer = null;
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
