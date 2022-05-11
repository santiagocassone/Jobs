using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.VendorDepositRequest
{
    public class VDRViewModel
    {
        public IEnumerable<VDRCost> VDRCost;

        public IEnumerable<int> Suffix;

        public static string CreateEmailBody(VDREmailForm body, DateTime genDate, string depTerm)
        {
            string template =
            @"<html>
                <style>
                    body { font:12px arial, sans-serif;color:#666; }
                    p { margin: 0; padding: 0; }
                    table { border-collapse: collapse; }
                    td, th { border: 1px solid black; padding-left 20px; padding-right: 10px; }
                    tdHead { font-weight: bold; }
                    td > span { font-weight: bold; }
                </style>
                <body>
                    <table>
                        <thead>
                            <tr>
                                <th colspan='2' style='color: white; background-color: red;'><b>Request Details<b></th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>##TABLEDETAILS##</tr>
                        </tbody>
                    </table>
                    <br />
                    <table>
                        <thead>
                            <tr>
                                <th colspan='2' style='color: white; background-color: red;'><b>Vendor Deposit Request<b></th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>##TABLEREQUEST##</tr>
                        </tbody>
                    </table>
                    <br/>
                    ##NOTES##          
                    <br/>
                    <p>Thank you,<p>
                    <br/>
                    <p>##YOURNAME##</p>
                </body>
            </html>";

            return FillTemplate(template, body, genDate, depTerm);
        }

        public static string CreateSubjectEmail(VDREmailForm body)
        {
            string subject = "Vendor Deposit Request ";
            subject += String.Format("| {0} | ", body.Order);

            return subject;
        }

        private static string FillTemplate(string template, VDREmailForm body, DateTime genDate, string depTerm)
        {
            string formattedTotal = body.POTotal.Trim(new Char[] { ' ', '$', ',' });
            if (body.PreviouslyPaid == "") body.PreviouslyPaid = "0";
            if (body.CostVerified == "") body.CostVerified = "0";
            string formattedPreviously = body.PreviouslyPaid.Trim(new Char[] { ' ', '$', ',' });
            float currentBalance = float.Parse(formattedTotal) - float.Parse(formattedPreviously);
            string tableData = String.Format("<tr><td><span>Requested By</span></td><td style='text-align: left;'>{0}</td></tr>" +
                "<tr><td><span>Date Requested</span></td><td style='text-align: left;'>{1}</td></tr>" +
                "<tr><td><span>Vendor</span></td><td style='text-align: left;'>{2}</td></tr>" +
                "<tr><td><span>Order</span></td><td style='text-align: left;'>{3}</td></tr>" +
                "<tr><td><span>Payment Type</span></td><td style='text-align: left;'>{4}</td></tr>" +
                "<tr><td><span>Currency</span></td><td style='text-align: left;'>{5}</td></tr>"
                , body.YourName, genDate, body.Vendor, body.Order, body.PaymentType, body.CurrencyType == "Other" ? body.CurrencyCustom : body.CurrencyType);

            template = template.Replace("##TABLEDETAILS##", tableData);

            tableData = String.Format("<tr><td><span>PO Total</span></td><td style='text-align: left;'>{0}</td></tr>" +
                "<tr><td><span>Previously Paid</span></td><td style='text-align: left;'>{2}</td></tr>" +
                "<tr><td><span>Cost Verified</span></td><td style='text-align: left;'>{1}</td></tr>" +
                "<tr><td><span>Current Balance</span></td><td style='text-align: left;'>$ {3}</td></tr>" +
                "<tr><td><span>Deposit % Requested</span></td><td style='text-align: left;'>{4}</td></tr>" +
                "<tr><td><span>Requested Amount Due</span></td><td style='text-align: left;'>{5}</td></tr>" +
                "<tr><td><span>Date Needed</span></td><td style='text-align: left;'>{6}</td></tr>"
                , body.POTotal, body.CostVerified, body.PreviouslyPaid, currentBalance.ToString("#,##0.00"), depTerm, body.AmtDue, body.DueDate);

            template = template.Replace("##TABLEREQUEST##", tableData);

            template = template.Replace("##YOURNAME##", body.YourName);
            if (body.Notes != "")
            {
                template = template.Replace("##NOTES##", String.Format("<p>Notes: </p><br /><p>{0}</p>", body.Notes));
            } else
            {
                template = template.Replace("##NOTES##", "");
            }
            

            return template;
        }

    }
}

