using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.LaborQuoteAutomationCommons
{
    public class MiscCharges
	{
		public int LaborQuoteHeaderID { get; set; }
		public string Description { get; set; }
		public int Quantity { get; set; }
		public double Rate { get; set; }
		public double Cost { get; set; }
		public double TotalCost { get; set; }
	}
}
