using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.InternalStatusReport
{
	public class ISRClosedOrderSummaryViewModel
	{
		public List<ClosedOrders> ClosedOrders { get; set; }
	}

	public class ClosedOrders
	{
		public string OrderIndex { get; set; }
		public string OrderNo { get; set; }
		public string ProjectID { get; set; }
		public string OrderTitle { get; set; }
		public string Customer { get; set; }
		public string DateEntered { get; set; }
		public string Comments { get; set; }
		public double TotalSell { get; set; }
		public double TotalCost { get; set; }
		public string MaxEstimatedArrivalDate { get; set; }
		public string CustRequestDate { get; set; }
		public List<EmailLog> EmailLogs { get; set; }

		public string Tooltip(List<EmailLog> EmailLogs)
		{
			string TipText = @"<div class='rTable'><div class='rTableHeading'><div class='rTableHead'>Date/Time</div><div class='rTableHead'>Your Email</div><div class='rTableHead'>Email Type</div><div class='rTableHead'>Vendor</div><div class='rTableHead'>Line</div></div>";
			foreach (var item in EmailLogs)
			{
				TipText += @"<div class='rTableRow'><div class='rTableCell'>" + item.CreatedDate.ToString() + "</div><div class='rTableCell'>" + item.YourEmail + "</div><div class='rTableCell'>" + item.CreatedBy + "</div><div class='rTableCell'>" + item.Vendor + "</div><div class='rTableCell'>" + item.Line + "</div></div>";
			}

			TipText += @"</div>";

			return TipText;
		}
	}
}