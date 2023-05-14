using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class CommonHelper
    {
        /// <summary>
        /// 格式化日期
        /// </summary>
        /// <param name="currdate"></param>
        /// <returns></returns>
        public static string FormatPolarDBDate(this string currdate)
        {
            var finaldate = string.Empty;
            if (string.IsNullOrEmpty(currdate)) { finaldate = string.Empty; return finaldate; }
            if (currdate.Length == 10)
            {
                finaldate = string.Join("-", currdate.Replace("/", "-").Split("-").Reverse());
            }
            else
            {
                var date = currdate.Substring(0, 10);
                var time = currdate.Replace(date, " ");
                finaldate = string.Join("-", date.Replace("/", "-").Split("-").Reverse()) + time;
            }
            return finaldate;
        }
    }
}
