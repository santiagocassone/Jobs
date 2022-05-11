using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.LaborQuoteAutomationCommons
{
    public class RevisionReasons
	{
		public string LaborQuoteCode { get; set; }
		public string OriginalLaborQuoteCode { get; set; }
		public string RequestorName { get; set; }
		public string CustomerName { get; set; }
		public string RevisionReason { get; set; }
	}
}
