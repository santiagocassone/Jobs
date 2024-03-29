﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Utils
{
    public class DateTimeSupport
    {

        public static double GetBusinessDays(string startD, string endD)
        {
            if(DateTime.TryParse(startD, out DateTime startDatetime))
            {
                if(DateTime.TryParse(endD, out DateTime endDatetime))
                {
                    return GetBusinessDays(startDatetime, endDatetime);
                }
            }
            
            return double.MaxValue;
        }

        public static double GetBusinessDays(DateTime startD, DateTime endD)
        {
            double calcBusinessDays =
                1 + ((endD - startD).TotalDays * 5 -
                (startD.DayOfWeek - endD.DayOfWeek) * 2) / 7;

            if (endD.DayOfWeek == DayOfWeek.Saturday) calcBusinessDays--;
            if (startD.DayOfWeek == DayOfWeek.Sunday) calcBusinessDays--;

            return calcBusinessDays;
        }
    }
}
