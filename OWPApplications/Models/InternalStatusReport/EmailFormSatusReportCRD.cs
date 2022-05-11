using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.InternalStatusReport
{
    public class EmailFormSatusReportCRD : EmailForm
    {
        public List<CRDData> DataToSend { get; set; }
        public string Comments { get; set; }
        public string Customer { get; set; }
        public string Region { get; set; }

        public string Subject()
        {            
            return string.Format("Requested CRD Changes | {0}", this.Name);
        }

        public string Body(List<CRDData> DataToSend)
        {
            string bodyHTML = @"<html><style>body {font:12px arial, sans-serif; color:#666;}p {margin: 0; padding: 0;} th, td {width: 30%; text-align: center;}</style>
	                            <body>            
	                                <br/><p>Dear Studio West:</p></br> 
							        </br><p>Please complete the following changes to the following Customer Requested Dates:</p><br/>
	                                ##ORDERS##
	                                <br/><p>Please feel free to contact me with any other questions.</p><br/>
	                                ##COMMENTS##
                                    ##NAME##
	                            </body>
	                            </html>";
            

            if (DataToSend != null && DataToSend.Count > 0)
            {
                string orders = @"<table><tr><th>Order</th><th>Current CRD</th><th>Requested CRD</th><th>Customer</th></tr>";
                    foreach (var item in DataToSend)
                {
                    orders += @"<tr><td>" + item.ProjectID + "</td><td>" + String.Format("{0:MM/dd/yyyy}", item.CRD) + "</td><td>" + String.Format("{0:MM/dd/yyyy}", item.ReqCRD) + "</td><td>" + item.Customer + "</td></tr>";
                }
                orders += @"</table>";

                bodyHTML = bodyHTML.Replace("##ORDERS##", orders);
            }
            else
            {
                bodyHTML = bodyHTML.Replace("##ORDERS##", "");
            }


            if (!string.IsNullOrEmpty(this.Comments))
            {
               bodyHTML = bodyHTML.Replace("##COMMENTS##", string.Format(@"<p>{0}</p><br/>", this.Comments));               
            }
            else
            {
                bodyHTML = bodyHTML.Replace("##COMMENTS##", "");
            }

            if (!string.IsNullOrEmpty(this.Name))
            {
                bodyHTML = bodyHTML.Replace("##NAME##", string.Format(@"<p>{0}.</p>", this.Name));
            }
            else
            {
                bodyHTML = bodyHTML.Replace("##NAME##", "");
            }

            return bodyHTML;
        }
    }
}
