using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.POPDashboard
{
    public class OpenQuotes
    {
        public string QuoteNo { get; set; }
        public string PIDNo { get; set; }
        public string QuoteTitle { get; set; }
        public decimal? SellAmount { get; set; }
        public double? CostAmount { get; set; }
        public double? GP { get; set; }
        public string Notes { get; set; }
        public bool One22Line { get; set; }
        public DateTime Date_Entered { get; set; }
        public DateTime Last_Cust_Request_Date { get; set; }
    }
}
