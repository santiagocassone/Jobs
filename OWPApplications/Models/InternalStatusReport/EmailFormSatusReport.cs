using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.InternalStatusReport
{
    public class EmailFormSatusReport : EmailForm
    {
        public IEnumerable<EmailDataInternalStatus> DataToSend { get; set; }
		public string OrderTitle { get; set; }
		public string InvoiceReceived { get; set; }
		public string TotalSellIncludingTax { get; set; }
        public string Accountability { get; set; }
        public string Region { get; set; }
        public string CompanyCode { get; set; }
    }

    public class EmailDataInternalStatus
    {
        public string LineNo { get; set; }
        public string VendorNo { get; set; }
        public string ACK { get; set; }
        public string OWP_PO { get; set; }
        public string EmailType { get; set; }
        public string To { get; set; }
        public string Location { get; set; }
        public string Comments { get; set; }
        public string OrderTitle { get; set; }

        public string Sell { get; set; }

        public string CompletionDate { get; set; }
        public string CustomerName { get; set; }

        public string TotalSell { get; set; }
        public string SellEligibleForPartialInvoicing { get; set; }
        public string Carrier { get; set; }
        public string Tracking { get; set; }
        public string Delivered { get; set; }
        public string CompanyCode { get; set; }
        public string ProcessCode { get; set; }

		public string Subject(string orderno, string ordertitle, string invoicereceived, string processcode)
		{
            switch (this.EmailType)
            {
                case "Received": return string.Format("Line Needing Receipt: {0} | {1} | Line {2} | {3}", orderno, VendorNo, LineNo, Sell);
                case "OSQReceived": return string.Format("Line Needing Receipt: {0} | {1} | Line {2} | {3}", orderno, VendorNo, LineNo, Sell);
                case "Ticketed": if (processcode == "D1") { return string.Format("D1 Line Needing Ticketing: {0} | {1} | Line {2} | {3}", orderno, VendorNo, LineNo, Sell); } else { return string.Format("Line Needing Ticketing: {0} | {1} | Line {2} | {3}", orderno, VendorNo, LineNo, Sell); }
                case "OSQProduct": return string.Format("Line Needing Ticketing: {0} | {1} | Line {2} | {3}", orderno, VendorNo, LineNo, Sell);
                case "OSQLabor": return string.Format("Line Needing Ticketing: {0} | {1} | Line {2} | {3}", orderno, VendorNo, LineNo, Sell);
                case "Delivered": return string.Format("Line Needing DT: {0} | {1} | Line {2} | {3}", orderno, VendorNo, LineNo, Sell);
                case "OSQInvoicing": return string.Format("Line Needing DT: {0} | {1} | Line {2} | {3}", orderno, VendorNo, LineNo, Sell);
                case "BillOrder": return string.Format("Invoice Complete: {0} | {1} | {2} ", orderno, ordertitle, CompletionDate);
				case "BillPartial": return string.Format("Change to Bill Partial: {0} | {1} | {2} ", orderno, ordertitle, invoicereceived);
				case "VendorInvoice":
                    if (!string.IsNullOrEmpty(ACK))
                    {
                        return string.Format("Invoice Needed:  {0} | {1} | W{2}-{3} | Ack # {4}", CustomerName, VendorNo, orderno, OWP_PO, ACK);
                    }
                    else
                    {
                        return string.Format("Invoice Needed:  {0} | {1} | W{2}-{3}", CustomerName, VendorNo, orderno, OWP_PO);
                    }
                case "OSQVendorInvoice":
                    if (!string.IsNullOrEmpty(ACK))
                    {
                        return string.Format("Invoice Needed:  {0} | {1} | W{2}-{3} | Ack # {4}", CustomerName, VendorNo, orderno, OWP_PO, ACK);
                    }
                    else
                    {
                        return string.Format("Invoice Needed:  {0} | {1} | W{2}-{3}", CustomerName, VendorNo, orderno, OWP_PO);
                    }
                case "NoCostProduct": return string.Format("Cost Verification Needed:  {0} | W{1}-{2} | Line {3}", VendorNo, orderno, OWP_PO, LineNo);
                case "Freeform": return string.Format("Information Needed:  {0} | {1} | {2} | {3} | Line {4} | {5}", OWP_PO, OrderTitle, orderno, VendorNo, LineNo, Sell);
                case "OSQFreeform": return string.Format("Information Needed:  {0} | {1} | {2} | {3} | Line {4} | {5}", OWP_PO, OrderTitle, orderno, VendorNo, LineNo, Sell);
                default: return "";
            }
        }

        public string Body(string orderno, List<string> linesNotEligibleForInvoicing, double totalSell, double sellEligibleForPartialInvoicing, string invoiceType, string invoiceReceived, string name, string processcode)
        {
            string bodyHTML = @"<html>
	                            <style>
	                            body {font:12px arial, sans-serif;color:#666;}            
	                            p {
	                             margin: 0;
	                             padding: 0;
	                            }
	                            </style>
	                            <body>
	                            <br/>            
	                            ##BODY## 
	                            <br/>
	                            ##LINES##
	                            <br/>
	                            ##ADDITIONALDATA##
	                            <br/>
	                            ##COMMENTS##
                                ##NAME##
	                            </body>
	                            </html>";

            switch (this.EmailType)
            {
                case "Received":
                    bodyHTML = bodyHTML.Replace("##BODY##", string.Format(@"<p>Receiving Team:</p></br> 
							<p>The vendor has reported that this line(s) has been shipped and delivered to our warehouse.
							Please receive the line in Hedberg or provide additional details.</p>
                            <p>Carrier: {0}</p><p>Tracking #/BOL: {1}</p><p>Delivered Date: {2}</p>", this.Carrier, this.Tracking, this.Delivered));
                    break;
                case "OSQReceived":
                    bodyHTML = bodyHTML.Replace("##BODY##", string.Format(@"<p>Receiving Team:</p></br> 
							<p>The vendor has reported that this line(s) has been shipped and delivered to our warehouse.
							Please receive the line in Hedberg or provide additional details.</p>
                            <p>Carrier: {0}</p><p>Tracking #/BOL: {1}</p><p>Delivered Date: {2}</p>", this.Carrier, this.Tracking, this.Delivered));
                    break;
                case "Ticketed":
                    if (processcode == "D1")
                    {
                        bodyHTML = bodyHTML.Replace("##BODY##", string.Format(@"<p>Warehouse Team:</p></br> 
                                    <p>Please confirm that D1 product on line {0} is located in location {1}. If not, please ship this ticket. </p>", this.LineNo, this.Location));
                    } else
                    {
                        bodyHTML = bodyHTML.Replace("##BODY##", string.Format(@"<p>Warehouse Team:</p></br> 
                                    <p>Please confirm that product on line {0} is located in location {1}. If not, please ship this ticket. </p>", this.LineNo, this.Location));
                    }                    
                    break;
                case "OSQProduct":
                    bodyHTML = bodyHTML.Replace("##BODY##", string.Format(@"<p>Warehouse Team:</p></br> 
                                    <p>Please confirm that product on line {0} is located in location {1}. If not, please ship this ticket. </p>", this.LineNo, this.Location));
                    break;
                case "OSQLabor":
                    bodyHTML = bodyHTML.Replace("##BODY##", string.Format(@"<p>Warehouse Team:</p></br> 
                                    <p>Please confirm that product on line {0} is located in location {1}. If not, please ship this ticket. </p>", this.LineNo, this.Location));
                    break;
                case "Delivered":
                    bodyHTML = bodyHTML.Replace("##BODY##", string.Format(@"<p>Billing Team:</p></br> 
							<p>Please confirm that the labor from line {0} is was completed. If so, please print/ship the ticket. 
								If labor was not completed, please provide additional details. </p>", this.LineNo));
                    break;
                case "OSQInvoicing":
                    bodyHTML = bodyHTML.Replace("##BODY##", string.Format(@"<p>Billing Team:</p></br> 
							<p>Please confirm that the labor from line {0} is was completed. If so, please print/ship the ticket. 
								If labor was not completed, please provide additional details. </p>", this.LineNo));
                    break;
                case "BillOrder":
                    bodyHTML = bodyHTML.Replace("##BODY##", string.Format(@"<p>Dear Dana:</p></br> 
							<p>Please invoice Order #{0}, complete. Feel free to contact me with any additional questions.</p>", orderno));
                    break;
                case "BillPartial":
					if (invoiceReceived == "Received")
					{
						bodyHTML = bodyHTML.Replace("##BODY##", string.Format(@"<p>Dear Dana:</p></br> 
							<p>Please bill lines on order #{0} that have been received in the system. Feel free to contact me with any additional questions.</p>", orderno));
					}
					else
					{
						bodyHTML = bodyHTML.Replace("##BODY##", string.Format(@"<p>Dear Dana:</p></br> 
							<p>Please bill lines on order #{0} that have been paid/cost verified. Feel free to contact me with any additional questions.</p>", orderno));
					}
                    break;
                case "VendorInvoice":
                    bodyHTML = bodyHTML.Replace("##BODY##", @"<p>Dear Vendor:</p></br> 
							<p>Our records have indicated that an invoice has not yet been received for this order. 
							Please send over a copy with a cc to <a href='mailto: ap@oneworkplace.com'>ap@oneworkplace.com</a> as soon as possible.</p>");
                    break;
                case "OSQVendorInvoice":
                    bodyHTML = bodyHTML.Replace("##BODY##", @"<p>Dear Vendor:</p></br> 
							<p>Our records have indicated that an invoice has not yet been received for this order. 
							Please send over a copy with a cc to <a href='mailto: accountspayable@open-sq.com'>accountspayable@open-sq.com</a> as soon as possible.</p>");
                    break;
                case "NoCostProduct":
                    bodyHTML = bodyHTML.Replace("##BODY##", @"<p>Dear Accounts Payable Team:</p>");
                    break;
                case "Freeform":
                    bodyHTML = bodyHTML.Replace("##BODY##", "");
                    break;
                case "OSQFreeform":
                    bodyHTML = bodyHTML.Replace("##BODY##", "");
                    break;
                default: return "";
            }

            if (linesNotEligibleForInvoicing.Count > 0 && this.EmailType != "VendorInvoice" && this.EmailType != "OSQVendorInvoice")
            {
                string linesNo = String.Join(", ", linesNotEligibleForInvoicing);
                bodyHTML = bodyHTML.Replace("##LINES##", @"<p>Open Lines# that are not eligible for partial invoicing: " + linesNo + "</p>");
            }
            else
            {
                bodyHTML = bodyHTML.Replace("##LINES##", "");
            }

			if (this.EmailType != "VendorInvoice" && this.EmailType != "OSQVendorInvoice")
			{
				if (invoiceType == "Complete")
				{
					bodyHTML = bodyHTML.Replace("##ADDITIONALDATA##", @"Total Sell: " + totalSell.ToString("C"));
				}
				else if (invoiceType == "Partial")
				{
					bodyHTML = bodyHTML.Replace("##ADDITIONALDATA##", @"Sell Eligible for Partial Invoicing: " + sellEligibleForPartialInvoicing.ToString("C"));
				}
				else
				{
					bodyHTML = bodyHTML.Replace("##ADDITIONALDATA##", @"Total Sell: " + totalSell.ToString("C") + @" | Sell Eligible for Partial Invoicing: " + sellEligibleForPartialInvoicing.ToString("C"));
				}
			}
			else
            {
                bodyHTML = bodyHTML.Replace("##ADDITIONALDATA##", "");
            }

            if (!string.IsNullOrEmpty(this.Comments))
            {
                if (this.EmailType == "Freeform" || this.EmailType == "NoCostProduct" || this.EmailType == "OSQFreeform")
                {
                    bodyHTML = bodyHTML.Replace("##COMMENTS##", string.Format(@"<p>{0}</p>", this.Comments));
                }
                else
                {
                    bodyHTML = bodyHTML.Replace("##COMMENTS##", string.Format(@"<p>Additional Comments:</p><p>{0}</p>", this.Comments));
                }
            }
            else
            {

                bodyHTML = bodyHTML.Replace("##COMMENTS##", "");
            }

            bodyHTML = bodyHTML.Replace("##NAME##", !string.IsNullOrEmpty(name) ? name : "");

            return bodyHTML;
        }

    }
}
