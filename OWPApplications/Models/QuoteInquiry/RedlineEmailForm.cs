using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.QuoteInquiry
{
	public class RedlineEmailForm
	{
		public string To { get; set; }
		public string YourEmail { get; set; }
		public string CC1 { get; set; }
		public string CC2 { get; set; }
		public bool RFP_Bid { get; set; }
		public string QuoteNO { get; set; }
		public string CustomerName { get; set; }
		public string OrderTitle { get; set; }
		public List<RedlineEmailLinesData> LinesData { get; set; }
	}

	public class RedlineEmailLinesData
	{
		public string LineNo { get; set; }
		public string VendorNo { get; set; }
		public string CatalogNo { get; set; }
		public string GeneralTagging { get; set; }
		public string QtyOrdered { get; set; }
		public string Description { get; set; }
		public string GPDlls { get; set; }
		public string List { get; set; }
		public string LineSell { get; set; }
		public string Comment { get; set; }
		public string Cost { get; set; }
	}
}
