using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.InternalStatusReport
{
	public class EmailFormInvoice
	{
        public string InvoiceType { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Name { get; set; }
        public string CC1 { get; set; }
        public string CC2 { get; set; }
        public string BCC { get; set; }
        public string Comments { get; set; }
        public List<LineData> LinesData { get; set; }
        public string CompletionDate { get; set; }
        public string ReceivedOrPaid { get; set; }
        public string Region { get; set; }
    }

    public class LineData
	{
        public string OrderNo { get; set; }
        public string OrderTitle { get; set; }
        public string ProjectID { get; set; }
        public string Accountability { get; set; }
        public string CustomerName { get; set; }
        public string Vendor { get; set; }
        public string TotalSell { get; set; }
        public string TotalCost { get; set; }
        public string SellEligibleForPartialInvoicing { get; set; }
        public string CompanyCode { get; set; }
    }
}
