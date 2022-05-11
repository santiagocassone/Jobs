using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.LaborQuoteAutomationCommons
{
    public class Addons
	{
		public int LaborQuoteHeaderID { get; set; }
		public string Description { get; set; }
		public int Quantity { get; set; }
		public double Hours { get; set; }
		public double TotalHours { get; set; }
	}
}
