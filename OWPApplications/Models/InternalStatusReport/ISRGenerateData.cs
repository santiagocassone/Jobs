using System;
using System.Collections.Generic;

namespace OWPApplications.Models.InternalStatusReport
{
    public class ISRPDFData
    {
        public List<ISRGenerateData> DataReport { get; set; }
    }
    public class ISRGenerateData
    {
        public string OrderNo { get; set; }
        public DateTime? EstimatedArrivalDate { get; set; }
        public string SalespersonName { get; set; }
        public string ProjectID { get; set; }
        public string Accountability { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public string OrderTitle { get; set; }
        public DateTime CustomerRequestDate { get; set; }
        public double PercentageSellAvailablePartialInvoicing { get; set; }
        public double SellEligibleforPartialInvoicing { get; set; }
        public double TotalOpenSell { get; set; }
        public double TotalOpenCost { get; set; }
        public int QtyOpen { get; set; }
        public List<ScheduledDate> ScheduledDates { get; set; }
        public string RequestedCRD { get; set; }
        public string Comment { get; set; }
    }
}
