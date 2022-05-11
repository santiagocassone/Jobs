using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.InternalStatusReport
{
    public class EmailFormVendorInvoice : EmailForm
    {
        public string Comments { get; set; }
        public string Region { get; set; }
        public List<OrderList> Orders { get; set; }
        public string Subject(string customerName, string vendorNo, string orderno, string PO, string ACK, string region)
        {
            if (!string.IsNullOrEmpty(ACK))
            {
                return string.Format("Invoice Needed:  {0} | {1} | {2}{3}-{4} | Ack # {5}", customerName, vendorNo, region == "OWP" ? "W" : "S", orderno, PO, ACK);
            }
            else
            {
                return string.Format("Invoice Needed:  {0} | {1} | {2}{3}-{4}", customerName, vendorNo, region == "OWP" ? "W" : "S", orderno, PO);
            }
        }

        public string Body(string comments, string name, string region)
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
	                            ##COMMENTS##
                                ##NAME##
	                            </body>
	                            </html>";

            if (region == "OWP")
            {
                bodyHTML = bodyHTML.Replace("##BODY##", @"<p>Dear Vendor:</p></br> 
							<p>Our records have indicated that an invoice has not yet been received for this order. 
							Please send over a copy with a cc to <a href='mailto: ap@oneworkplace.com'>ap@oneworkplace.com</a> as soon as possible.</p>");
            }
            else
            {
                bodyHTML = bodyHTML.Replace("##BODY##", @"<p>Dear Vendor:</p></br> 
							<p>Our records have indicated that an invoice has not yet been received for this order. 
							Please send over a copy with a cc to <a href='mailto: accountspayable@open-sq.com'>accountspayable@open-sq.com</a> as soon as possible.</p>");
            }

            bodyHTML = bodyHTML.Replace("##COMMENTS##", string.Format(@"<p>Additional Comments:</p><p>{0}</p>", !string.IsNullOrEmpty(comments) ? comments : ""));

            bodyHTML = bodyHTML.Replace("##NAME##", !string.IsNullOrEmpty(name) ? name : "");

            return bodyHTML;
        }
        public class OrderList
        {
            public string OrderNo { get; set; }
            public List<Vendor> Vendors { get; set; }
        }
    }
}
