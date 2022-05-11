using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.QuoteInquiry
{
    public class QuoteInquiryOSQViewModel
    {
        public bool ShowResults { get; set; }

        public int? QuoteNo { get; set; }

        public HeaderInfoQouteInquiry HeaderInfo { get; set; }

        public IEnumerable<LineInfoQuoteInquiry> linesInfos { get; set; }

        public IEnumerable<MiscLinesQuoteInquiry> MiscLines { get; set; }

        public string TotalGP { get; set; }

        public IEnumerable<VendorEmail> Vendors { get; set; }


        public static string CreateSubjectEmail(EmailFormQuoteInquiryOSQ body)
        {
            string subject = body.VendorName.Trim() + " Quote Request:";
            subject += String.Format(" {0} | {1} | {2}", body.Quote_OrderNo.Trim(), body.ProjectID.Trim(), body.ProjectNameAndLocation.Trim());
            
            return subject;
        }

        public static string CreateStandardEmailBody(EmailFormQuoteInquiryOSQ body)
        {
            string template =
            @"<html>
            <style>
            body {font:12px arial, sans-serif;color:#666;}
            table {border-collapse:collapse;}
            table, td, th {border: 1px solid #ccc; padding:10px;}
            th {font-size:14px; background-color:#0A497E; border:1px solid #0A497E; color:#fff;}
            p {
             margin: 0;
             padding: 0;
            }
            </style>
            <body>

            <p><strong>Project Name and Location:</strong> ##PROJECTNAMEANDLOCATION##</p>
            <p><strong>Specified by and Location:</strong> ##SPECIFIEDANDLOCATION##</p>
            <p><strong>Ship to zip:</strong> ##ZIPCODE##</p>
            ##ANTICIPATEDORDERDATE##
            <br/>

            <p>Hello!</p>
            <br/>
            <p>We have a new project with the following item(s) specified.</p>
            <br/>

            <p>Please verify that all details have been fully specified for complete order fulfillment.</p>
            <ul><li>Please advise immediately, (prior to quoting), if any of the fabrics do not pass or need testing. If testing is required, please provide testing instructions.</li></ul>
            <br/>

            ##ITEMS##
            <br/>

            ##NOTES##
            <br/>

            ##RETURNINFOBY##
            <br/>

            <p>Please confirm receipt of this request for quote within 24 hours.</p>
            <br/>

            <p>***** Please <b>Reply All</b> to this e-mail thread when sending quote to ensure that all necessary parties receive your response.</p>
            <br/>
            
            ##LINES##
            <br/>

            <p>Thank you!</p>
            <br/>

            <p><strong>##YOURNAME##</strong></p>
            ##TITLE##
            ##PHONENO##
            ##COMPANY##
            <br/><br/>
            </body>
            </html>";

            return FillTemplate(template, body);
        }

        public static string CreateAmazonEmailBody(EmailFormQuoteInquiryOSQ body)
        {
            string template =
            @"<html>
            <style>
            body {font:12px arial, sans-serif;color:#666;}
            table {border-collapse:collapse;}
            table, td, th {border: 1px solid #ccc; padding:10px;}
            th {font-size:14px; background-color:#0A497E; border:1px solid #0A497E; color:#fff;}
            p {
             margin: 0;
             padding: 0;
            }
            </style>
            <body>

            <p><strong>Project Name and Location:</strong> ##PROJECTNAMEANDLOCATION##</p>
            <p><strong>Specified by and Location:</strong> ##SPECIFIEDANDLOCATION##</p>
            <p><strong>Ship to zip:</strong> ##ZIPCODE##</p>
            <br/>

            <p>Hello!<p>
            <br/>
            <p>We have a new Amazon Project with the following item(s) specified.</p>
            <br/>

            <p>Please verify that all details have been fully specified for complete order fulfillment.</p>
            <ul><li>Please advise immediately, (prior to quoting), if any of the fabrics do not pass or need testing.  If testing is required, please provide testing instructions.</li></ul>
            <br/>

            ##ITEMS##
            <br/>

            ##NOTES##
            <br/>

            ##RETURNINFOBY##
            <br/>

            <p>Please confirm receipt of this request for quote within 24 hours.</p>
            <br/>

            <p>***** Please <b>Reply All</b> to this e-mail thread when sending quote to ensure that all necessary parties receive your response.</p>
            <br/>
            
            ##LINES##
            <br/>

            <p>Thank you!</p>
            <br/>

            <p><strong>##YOURNAME##</strong></p>
            ##TITLE##
            ##PHONENO##
            ##COMPANY##
            <br/><br/>
            </body>
            </html>";

            return FillTemplate(template, body);
        }

        private static string FillTemplate(string template, EmailFormQuoteInquiryOSQ body)
        {
            template = template.Replace("##PROJECTNAMEANDLOCATION##", body.ProjectNameAndLocation);
            template = template.Replace("##SPECIFIEDANDLOCATION##", body.SpecifiedAndLocation);
            template = template.Replace("##ZIPCODE##", !string.IsNullOrEmpty(body.FormattedZipCode) ? body.FormattedZipCode : body.ZipCode);
            if (!string.IsNullOrEmpty(body.AnticipatedDate))
                template = template.Replace("##ANTICIPATEDORDERDATE##", "<p><strong>Anticipated Order Date:</strong> " + body.AnticipatedDate + "<p>");
            else
                template = template.Replace("##ANTICIPATEDORDERDATE##", "");

            string items = "";
            if (body.InputListPrice) items += @"<li>List Price</li>";
            if (body.InputNetOrDiscountOffList) items += @"<li>Net or discount off List</li>";
            if (body.InputFreightEstimate) items += @"<li>Freight Estimate</li>";
            if (body.InputCOMApproval) items += @"<li>COM approval</li>";
            if (body.InputQuoteAsCOM) items += @"<li>Quote as COM</li>";
            if (body.InputQuoteAsGradedIn) items += @"<li>Quote as Graded in, when available</li>";
            if (body.InputCOMYardageRequirements) items += @"<li>COM yardage requirements, based on the repeat of the fabric specified</li>";
            if (body.InputAnyAdditionalCharges) items += @"<li>Any additional charges for shipping & handling, cartoning / crating, call before delivery, etc.</li>";
            if (body.InputCurrentLeadTime) items += @"<li>Current Lead-time and estimated transit time from origin</li>";
            if (body.InputWarrantyInfo) items += @"<li>Warranty info</li>";
            if (body.InputUpcomingPriceChangesAnticipated) items += @"<li>Upcoming Price Changes anticipated?</li>";
            if (body.InputWhatIsMissingForACompleteSpec) items += @"<li>What is missing for a complete spec?</li>";
            if (body.InputFabricTestingRequired) items += @"<li>Fabric testing required, please include testing instructions</li>";
            if (body.InputShipToForCOM) items += @"<li>Ship to for COM</li>";
            if (body.InputConfirmComShip) items += @"<li>If finishes not standard please provide upcharge</li>";
            if (body.InputEnvironmentalDataOrCertifications) items += @"<li>Environmental data or certifications</li>";
            if (body.InputCostsAndLeadTimeForAirFreight) items += @"<li>Costs and lead time for air freight and sea freight</li>";
            if (body.InputDepositRequirements) items += @"<li>Deposit requirements</li>";
            if (body.InputPaymentTerms) items += @"<li>Payment terms</li>";
            if (items != "")
            {
                string itemsToInclude = $"<p>PLEASE INCLUDE IN YOUR QUOTE:</p><ul>{items}</ul> ";
                template = template.Replace("##ITEMS##", itemsToInclude);
            }
            else
            {
                template = template.Replace("##ITEMS##", "");
            }

            if (!string.IsNullOrEmpty(body.Notes))
                template = template.Replace("##NOTES##", "<p><strong>Notes:</strong> " + body.Notes + "<p>");
            else
                template = template.Replace("##NOTES##", "");

            if (!string.IsNullOrEmpty(body.ReturnInfoBy))
                template = template.Replace("##RETURNINFOBY##", "<p><strong>Please return info by:</strong> " + body.ReturnInfoBy + "<p>");
            else
                template = template.Replace("##RETURNINFOBY##", "");
            string lines = "";
            if (body.Is61 == "no")
            {
                lines = "<table><thead><tr><th>Line #</th><th>Catalog #</th><th>Qty</th><th>General Tagging</th><th>Description</th><tr></thead><tbody>";
                if (body.Lines != null)
                {
                    foreach (var line in body.Lines)
                    {
                        lines += $"<tr><td>{line.LineNo}</td><td>{line.Catalog}</td><td>{line.QtyOrdered}</td><td>{line.GeneralTagging}</td><td>{line.Description}</td></tr>";
                    }
                }
                lines += "</tbody></table>";
            } else
            {
                lines = "<table><thead><tr><th>Line #</th><th>Catalog #</th><th>Qty</th><th>General Tagging</th><tr></thead><tbody>";
                if (body.Lines != null)
                {
                    foreach (var line in body.Lines)
                    {
                        lines += $"<tr><td>{line.LineNo}</td><td>{line.Catalog}</td><td>{line.QtyOrdered}</td><td>{line.GeneralTagging}</td></tr>";
                    }
                }
                lines += "</tbody></table><p>See Attached Spec.</p>";
            }
            

            template = template.Replace("##LINES##", lines);

            template = template.Replace("##YOURNAME##", body.YourName);

            if (!string.IsNullOrEmpty(body.Title))
                template = template.Replace("##TITLE##", $"<p>{body.Title}<p>");
            else
                template = template.Replace("##TITLE##", "");

            if (!string.IsNullOrEmpty(body.PhoneNo))
                template = template.Replace("##PHONENO##", $"<p>{body.PhoneNo}<p>");
            else
                template = template.Replace("##PHONENO##", "");

            if (!string.IsNullOrEmpty(body.Company))
                template = template.Replace("##COMPANY##", $"<p>{body.Company}<p>");
            else
                template = template.Replace("##COMPANY##", "");

            return template;
        }
    }
}
