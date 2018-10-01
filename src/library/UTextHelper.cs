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
using OdinSdk.OdinLib.Configuration;

namespace OpenETaxBill.Engine.Library
{
    /// <summary>
    /// 
    /// </summary>
    public class UTextHelper
    {
        //-------------------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------------------
        private readonly static Lazy<UTextHelper> m_txtHelper = new Lazy<UTextHelper>(() =>
        {
            return new UTextHelper();
        });

        /// <summary>
        /// 
        /// </summary>
        public static UTextHelper SNG
        {
            get
            {
                return m_txtHelper.Value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------
        public int SigningDay = 10;
        public int ReportingDay = 15;

        //-------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DateTime GetFirstDayOfThisMonth()
        {
            return GetFirstDayOfThisMonth(DateTime.Now);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_dayOfTarget"></param>
        /// <returns></returns>
        public DateTime GetFirstDayOfThisMonth(DateTime p_dayOfTarget)
        {
            return CfgHelper.SNG.GetFirstDayOfMonth(p_dayOfTarget);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DateTime GetFirstDayOfLastMonth()
        {
            return GetFirstDayOfLastMonth(DateTime.Now);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_dayOfTarget"></param>
        /// <returns></returns>
        public DateTime GetFirstDayOfLastMonth(DateTime p_dayOfTarget)
        {
            return CfgHelper.SNG.GetFirstDayOfMonth(p_dayOfTarget.AddMonths(-1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DateTime GetLastDayOfThisMonth()
        {
            return GetLastDayOfThisMonth(DateTime.Now);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_dayOfTarget"></param>
        /// <returns></returns>
        public DateTime GetLastDayOfThisMonth(DateTime p_dayOfTarget)
        {
            return CfgHelper.SNG.GetLastDayOfMonth(p_dayOfTarget);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DateTime GetLastDayOfLastMonth()
        {
            return GetLastDayOfLastMonth(DateTime.Now);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_dayOfTarget"></param>
        /// <returns></returns>
        public DateTime GetLastDayOfLastMonth(DateTime p_dayOfTarget)
        {
            return CfgHelper.SNG.GetLastDayOfMonth(p_dayOfTarget.AddMonths(-1));
        }

        //-------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 메일 발송기한은 전송 기한 전까지 입니다.
        /// </summary>
        /// <param name="p_fromDay"></param>
        /// <param name="p_tillDay"></param>
        public void GetSendingRange(ref DateTime p_fromDay, ref DateTime p_tillDay)
        {
            GetSendingRange(DateTime.Now, ref p_fromDay, ref p_tillDay);
        }

        /// <summary>
        /// 메일 발송기한은 전송 기한 전까지 입니다.
        /// </summary>
        /// <param name="p_fromDay"></param>
        /// <param name="p_tillDay"></param>
        public void GetSendingRange(DateTime p_today, ref DateTime p_fromDay, ref DateTime p_tillDay)
        {
            if (p_fromDay > p_tillDay)
                p_tillDay = p_fromDay;

            DateTime _thisMonthFirstDay = GetFirstDayOfThisMonth(p_today);
            DateTime _lastMonthFirstDay = GetFirstDayOfLastMonth(p_today);

            if (p_today.Day <= ReportingDay)
            {
                if (p_fromDay < _lastMonthFirstDay)
                    p_fromDay = _lastMonthFirstDay;
            }
            else
            {
                if (p_fromDay < _thisMonthFirstDay)
                    p_fromDay = _thisMonthFirstDay;
            }

            if (p_tillDay > p_today)
                p_tillDay = p_today;
        }

        public void GetSigningRange(ref DateTime p_fromDay, ref DateTime p_tillDay)
        {
            GetSigningRange(DateTime.Now, ref p_fromDay, ref p_tillDay);
        }

        /// <summary>
        /// 서명기한은 익월 10일 까지 입니다.
        /// </summary>
        /// <param name="p_fromDay"></param>
        /// <param name="p_tillDay"></param>
        public void GetSigningRange(DateTime p_today, ref DateTime p_fromDay, ref DateTime p_tillDay)
        {
            if (p_fromDay > p_tillDay)
                p_tillDay = p_fromDay;

            DateTime _thisMonthFirstDay = GetFirstDayOfThisMonth(p_today);
            DateTime _lastMonthFirstDay = GetFirstDayOfLastMonth(p_today);

            if (p_today.Day <= SigningDay)
            {
                if (p_fromDay < _lastMonthFirstDay)
                    p_fromDay = _lastMonthFirstDay;
            }
            else
            {
                if (p_fromDay < _thisMonthFirstDay)
                    p_fromDay = _thisMonthFirstDay;
            }

            if (p_tillDay > p_today)
                p_tillDay = p_today;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_fromDay"></param>
        /// <param name="p_tillDay"></param>
        public void GetReportRange(ref DateTime p_fromDay, ref DateTime p_tillDay)
        {
            GetReportRange(DateTime.Now, ref p_fromDay, ref p_tillDay);
        }

        /// <summary>
        /// 신고기간은 익월 15일까지 입니다.
        /// </summary>
        /// <param name="p_fromDay"></param>
        /// <param name="p_tillDay"></param>
        public void GetReportRange(DateTime p_today, ref DateTime p_fromDay, ref DateTime p_tillDay)
        {
            if (p_fromDay > p_tillDay)
                p_tillDay = p_fromDay;

            DateTime _thisMonthFirstDay = GetFirstDayOfThisMonth(p_today);
            DateTime _lastMonthFirstDay = GetFirstDayOfLastMonth(p_today);

            if (p_today.Day <= ReportingDay)
            {
                if (p_fromDay < _lastMonthFirstDay)
                    p_fromDay = _lastMonthFirstDay;
            }
            else
            {
                if (p_fromDay < _thisMonthFirstDay)
                    p_fromDay = _thisMonthFirstDay;
            }

            if (p_tillDay > p_today)
                p_tillDay = p_today;
        }

        //-------------------------------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------------------------------
    }
}