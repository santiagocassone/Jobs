using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.POPDashboard
{
    /*
    public class PastCRD
    {
        public int OpenOrders { get; set; }
        public double OpenAmountSell { get; set; }
        public double OpenAmountCost { get; set; }

        public IEnumerable<PastCRDLines> Detail { get; set; }

    }

    public class PastCRDLines
    {
        public string OrderNo { get; set; }
        public string OrderDate { get; set; }
        public string Customer { get; set; }
        public string OrderTitle { get; set; }
        public string CRD { get; set; }
        public int OpenLines { get; set; }
        public double OpenSell { get; set; }
        public double OpenCost { get; set; }
        public string CompletionDate { get; set; }
        public string Comments { get; set; }
    }
    */

    public class PastCRDModel
    {
        public string Order_No { get; set; }
        public DateTime? Order_Date { get; set; }
        public string Customer_No { get; set; }
        public string Customer_Name { get; set; }
        public string Order_Title { get; set; }
        public string CRD { get; set; }
        public int? OpenLinesCount { get; set; }
        public double? OpenLinesSell { get; set; }
        public double? OpenLinesCost { get; set; }
        public string CompletionDate { get; set; }
        public string Comment { get; set; }
        public int? OpenOrdersCount { get; set; }
        public double? OpenSellAmount { get; set; }
        public double? OpenCostAmount { get; set; }
        public string Project_ID { get; set; }
        public double? SellEligibleForPartialInvoicing { get; set; }

        public double? OpenLines { get; set; }
        public double? OpenSell { get; set; }
        public double? OpenCost { get; set; }

        public double? TotalOpenSell { get; set; }
        public double? TotalOpenCost { get; set; }


    }
}
