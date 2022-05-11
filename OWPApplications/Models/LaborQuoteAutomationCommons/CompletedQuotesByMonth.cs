using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.LaborQuoteAutomationCommons
{
    public class CompletedQuotesByMonth
	{
		public string Month { get; set; }
		public double Total_Budget { get; set; }
		public int Count { get; set; }
		public int RevisionCount { get; set; }
	}
}
