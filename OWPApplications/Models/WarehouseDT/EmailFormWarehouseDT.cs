using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.WarehouseDT
{
    public class EmailFormWarehouseDT
    {
        public string From { get; set; }
        public string Name { get; set; }
        public string CC1 { get; set; }
        public string CC2 { get; set; }
        public string Comments { get; set; }
        public string InstallDate { get; set; }
        public List<EmailData> DataToSend { get; set; }
        public string Subject(string orderNo, string custName, string installDate)
        {
            string bodyHTML = @"CANCELLATION NOTIFICATION: ##ORDER## | ##CUSTOMER## | ##INSTALLDATE##";

            if (!string.IsNullOrEmpty(orderNo))
            {
                bodyHTML = bodyHTML.Replace("##ORDER##", orderNo);
            }
            else
            {
                bodyHTML = bodyHTML.Replace("##ORDER##", "");
            }

            if (!string.IsNullOrEmpty(installDate))
            {
                bodyHTML = bodyHTML.Replace("##INSTALLDATE##", installDate);
            }
            else
            {
                bodyHTML = bodyHTML.Replace("##INSTALLDATE##", "");
            }

            if (!string.IsNullOrEmpty(custName))
            {
                bodyHTML = bodyHTML.Replace("##CUSTOMER##", custName);
            }
            else
            {
                bodyHTML = bodyHTML.Replace("##CUSTOMER##", "");
            }

            return bodyHTML;
        }
        public string Body(string orderNo, string installDate, string comments, string name)
        {
            string bodyHTML = @"<html><style>body {font:12px arial, sans-serif; color:#666;}p {margin: 0; padding: 0;} th, td {width: 30%; text-align: center;}</style>
	                            <body>
							        </br><p>Order # ##ORDER## has been cancelled for date ##INSTALLDATE##.</p><br/>
	                                ##COMMENTS##
                                    ##NAME##
	                            </body>
	                            </html>";

            if (!string.IsNullOrEmpty(orderNo))
            {
                bodyHTML = bodyHTML.Replace("##ORDER##", orderNo);
            }
            else
            {
                bodyHTML = bodyHTML.Replace("##ORDER##", "");
            }

            if (!string.IsNullOrEmpty(installDate))
            {
                bodyHTML = bodyHTML.Replace("##INSTALLDATE##", installDate);
            }
            else
            {
                bodyHTML = bodyHTML.Replace("##INSTALLDATE##", "");
            }

            if (!string.IsNullOrEmpty(comments))
            {
                bodyHTML = bodyHTML.Replace("##COMMENTS##", string.Format(@"<p>{0}</p><br/>", comments));
            }
            else
            {
                bodyHTML = bodyHTML.Replace("##COMMENTS##", "");
            }

            if (!string.IsNullOrEmpty(name))
            {
                bodyHTML = bodyHTML.Replace("##NAME##", string.Format(@"<p>{0}</p><br/>", name));
            }
            else
            {
                bodyHTML = bodyHTML.Replace("##NAME##", "");
            }

            return bodyHTML;
        }
    }

    public class EmailData
    {
        public string OrderNo { get; set; }
        public string CustName { get; set; }
    }

    
}
