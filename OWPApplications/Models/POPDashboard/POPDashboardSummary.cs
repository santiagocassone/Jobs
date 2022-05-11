using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.POPDashboard
{
    public class POPDashboardSummary
    {
        public int TotalOpenOrders { get; set; }
        public double TotalOpenSell { get; set; }
        public double TotalOpenCost { get; set; }
    }
}
