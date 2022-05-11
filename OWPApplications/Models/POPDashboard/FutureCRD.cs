using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.POPDashboard
{
    /*
    public class FutureCRD
    {
        public int OrdersNotFullyACK { get; set; }
        public int OrdersNotScheduled { get; set; }

        public IEnumerable<PastCRDLines> Detail { get; set; }

    }

    public class FutureCRDLines
    {
        public string OrderNo { get; set; }
        public string OrderDate { get; set; }
        public string Customer { get; set; }
        public string OrderTitle { get; set; }
        public string CRD { get; set; }
        public string RequestArrivalDate { get; set; }
        public bool HasFullyACK { get; set; }
        public string ScheduledDate { get; set; }
        public string Comments { get; set; }
    }
    */

    public class FutureCRDModel
    {
        public string Order_No { get; set; }
        public DateTime? Order_Date { get; set; }
        public string Customer_No { get; set; }
        public string Customer_Name { get; set; }
        public string Order_Title { get; set; }
        public string CRD { get; set; }
        public string OrderRequestedArrivalDate { get; set; }
        public string Scheduled_Date { get; set; }
        public string Comment { get; set; }
        public string FullyACKed { get; set; }
        public int? OrdersNotFullyACKed { get; set; }
        public int? OrdersFullyACKedNotSched { get; set; }
        public int? NotSched { get; set; }
        public decimal? TotalSell { get; set; }
        public bool One22Line { get; set; }
        public string Project_ID { get; set; }
    }

}
