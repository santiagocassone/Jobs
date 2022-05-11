using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.LaborQuoteAutomationCommons
{
    public class EstimatorSummary
	{
		public string TotalQuotes_Qty { get; set; }
		public string CompletedQuotes_Qty { get; set; }
		public string PendingQuotes_Qty { get; set; }
		public string MTD_CompletedQuotes_Qty { get; set; }
		public double MTD_CompletedQuotes_Amount { get; set; }
		public string YTD_CompletedQuotes_Qty { get; set; }
		public double YTD_CompletedQuotes_Amount { get; set; }
	}
}
