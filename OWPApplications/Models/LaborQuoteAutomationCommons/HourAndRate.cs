using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.LaborQuoteAutomationCommons
{
    public class HourAndRate
    {
        public string Company_Code { get; set; }
        public int LaborQuoteHeaderID { get; set; }
        public int RateTypeLookupID { get; set; }
        public float RateValue { get; set; }
        public float Hours { get; set; }

    }
}
