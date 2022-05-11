using OWPApplications;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.QuoteInquiry
{
    public class QuoteInquiryViewModel
    {
        public bool ShowResults { get; set; }

        public string QuoteNo { get; set; }

        public HeaderInfoQouteInquiry HeaderInfo { get; set; }

        public IEnumerable<LineInfoQuoteInquiry> linesInfos { get; set; }

        public IEnumerable<MiscLinesQuoteInquiry> MiscLines { get; set; }

        public string TotalGP { get; set; }

        public IEnumerable<VendorEmail> Vendors { get; set; }


        public static string CreateEmailBody(EmailFormQuoteInquiry body)
        {
            string template =
            @"<html>
            <style>
            body {font:12px arial, sans-serif;color:#666;}
            table {border-collapse:collapse;}
            table, td, th {border: 1px solid #ccc; padding:10px;}
            th {font-size:14px; background-color:#ff4d4d; border:1px solid #ff4d4d; color:#fff;}
            p {
             margin: 0;
             padding: 0;
            }
            </style>
            <body>
            <p>Hello!<p>
            <br/>
            <p><strong>Project Name:</strong> ##QuoteHeader## - ##CustomerName##<p>
            <p><strong>Specified by and location:</strong> ##SpecifiedAndLocation##<p>
            <p><strong>Ship to zip:</strong> ##ZipCode##<p>
            <p><strong>Anticipated Order Date:</strong> ##AnticipatedDate##<p>
            ##BudgetaryPricing##
            <br/>
            ##FormalQuoteLegend##
            
            ##LINES##
            <br/>
            <p>If there is a current price increase, please base quote on order date.</p>

            ##ITEMS## 

            <p>Notes: </p>
            <p>##NOTES##</p>
            <br/>
            <p>##BID##</p>
            <p><strong>***Please be sure to respond within 24 hours and be sure to reply all to the e-mail thread to ensure that all necessary parties receive your response.*** Thank you!</strong></p>
            <br/>
            <p>##YOURNAME##</p>
            </body>
            </html>";

            return FillTemplate(template, body);
        }

        public static string CreateSubjectEmail(EmailFormQuoteInquiry body)
        {
            string subject = body.ItemBudgetaryPricing? "Budgetary Pricing Requested:" : "Quote Request:";
            if (!body.ExcludeTitle)
            {
                subject += String.Format(" {0} |", body.QuoteHeader);
            }
            subject += String.Format(" {0} | {1} | {2}", body.VendorName, body.Quote_OrderNo, body.CustomerName);

            return subject;
        }

        private static string FillTemplate(string template, EmailFormQuoteInquiry body)
        {
            template = template.Replace("##QuoteHeader##", body.QuoteHeader);
            template = template.Replace("##CustomerName##", body.CustomerName);
            template = template.Replace("##SpecifiedAndLocation##", body.SpecifiedAndLocation);
            template = template.Replace("##ZipCode##", !string.IsNullOrEmpty(body.FormattedZipCode) ? body.FormattedZipCode : body.ZipCode);
            template = template.Replace("##AnticipatedDate##", body.AnticipatedDate);
            template = template.Replace("##BudgetaryPricing##", body.ItemBudgetaryPricing ? "<p><strong>Budgetary Pricing</strong><p>" : "");
            template = template.Replace("##FormalQuoteLegend##", !body.ItemBudgetaryPricing ? "<p>Please provide a formal quote for the specs below:<p>" : "");
            template = template.Replace("##NOTES##", body.Notes);
            template = template.Replace("##YOURNAME##", body.YourName);

            string lines = "<table><thead><tr><th>Line #</th><th>Catalog #</th><th>Qty</th><th>General Tagging</th><th>Description</th><tr></thead><tbody>";
            if (body.Lines != null)
            {
                foreach (var line in body.Lines)
                {
                    lines += $"<tr><td>{line.LineNo}</td><td>{line.Catalog}</td><td>{line.QtyOrdered}</td><td>{line.GeneralTagging}</td><td>{line.Description}</td></tr>";
                }
            }

            lines += "</tbody></table>";

            template = template.Replace("##LINES##", lines);

            string items = "";
            if (body.ItemPricing)
                items += @"<li>PRICING (list & net)</li>";
            if (body.ItemFreight)
                items += @"<li>FREIGHT</li>";
            if (body.ItemLeadTime)
                items += @"<li>LEAD TIME</li>";
            if (body.ItemComYardage)
                items += @"<li>COM yardage </li>";
            if (body.ItemComApproval)
                items += @"<li>COM approval (please advise if testing is necessary and please also confirm COM Ship-To address)</li>";
            if (body.ItemConfirmComShip)
                items += @"<li>Confirm COM Ship-To Address</li>";
            if (body.ItemConfirmComAcrylic)
                items += @"<li>Confirm if COM Acrylic Backing is Required</li>";

            if (items != "")
			{
                string itemsToInclude = $"<p>Please be sure to include:</p><ul>{items}</ul> ";
               template = template.Replace("##ITEMS##", itemsToInclude);
            }
			else
			{
                template = template.Replace("##ITEMS##", "");
            }

            template = template.Replace("##BID##", (body.RFP_BID) ? "<strong>*Any additional Project/Bid discounting is needed and appreciated.</strong>" : "");

            return template;
        }

    }

    public class HeaderInfoQouteInquiry
    {
        public string CustomerName { get; set; }
        public string ProjectId { get; set; }
        public string OrderTitle { get; set; }
        public string LocationCode { get; set; }
        public string CustomerNo { get; set; }
        public string SalesIDs { get; set; }
        public string TermsCode { get; set; }
        public string CustomerPONo { get; set; }
        public string STCInvoiceType { get; set; }
        public string ShipToAddress { get; set; }
        public string SoldToAddress { get; set; }
        public string MailToAddress { get; set; }
        public string InvoiceAddress { get; set; }
        public string SalesTeam { get; set; }
        public string OrderStatus { get; set; }
        public string DeliveryInstructions { get; set; }
        public string InvoiceInstructions { get; set; }
        public string MFG_PO_Info { get; set; }


        public HeaderInfoQouteInquiry(DataRow row, DataRow row1, DataRow row2, DataRow inst_del, DataRow inst_inv, DataTable MFG)
        {
            CustomerName = clsLibrary.dBReadString(row["CustomerName"]);
            ProjectId = clsLibrary.dBReadString(row["Project_Id"]);
            OrderTitle = clsLibrary.dBReadString(row["Title"]);
            LocationCode = clsLibrary.dBReadString(row["Location_Code"]);
            CustomerNo = clsLibrary.dBReadString(row["Customer_No"]);
            SalesIDs = $"{clsLibrary.dBReadString(row["Salesperson_Id_1"])} ({clsLibrary.dBReadString(row["Salesperson_1_Pct"])}%)";
            SalesIDs += (clsLibrary.dBReadString(row["Salesperson_Id_2"]) == "") ? "" : $",{clsLibrary.dBReadString(row["Salesperson_Id_2"])} ({100 - clsLibrary.dBReadInt(row["Salesperson_1_Pct"])}%)";
            TermsCode = clsLibrary.dBReadString(row["Customer_Terms_Code"]);
            CustomerPONo = clsLibrary.dBReadString(row["Customer_PO_Number"]);

            STCInvoiceType = "";
            switch (clsLibrary.dBReadString(row["STI_Invoice_Type"]))
            {
                case "S":
                    STCInvoiceType = "Stealcase To Invoice";
                    break;
                case "L":
                    STCInvoiceType = "Dealer Bill";
                    break;
                default: break;
            }

            ShipToAddress = clsLibrary.dBReadString(row["Address_Line_1"]);
            if (clsLibrary.dBReadString(row["Address_Line_2"]) != "")
            {
                ShipToAddress += "<br>" + clsLibrary.dBReadString(row["Address_Line_2"]);
            }
            ShipToAddress += "<br>" + clsLibrary.dBReadString(row["City"]);

            SoldToAddress = "";
            if (row2 != null)
            {
                SoldToAddress = clsLibrary.dBReadString(row2["Address_Line_1"]);
                if (clsLibrary.dBReadString(row2["Address_Line_2"]) != "")
                {
                    SoldToAddress += "<br>" + clsLibrary.dBReadString(row2["Address_Line_2"]);
                }
                SoldToAddress += "<br>" + clsLibrary.dBReadString(row2["City"]) + ", " + clsLibrary.dBReadString(row2["Region"]) + "&nbsp;&nbsp" + clsLibrary.dBReadString(row2["Postal_Code"]);

            }

            MailToAddress = clsLibrary.dBReadString(row1["Address_Line_1"]);
            if (clsLibrary.dBReadString(row1["Address_Line_2"]) != "")
            {
                MailToAddress += "<br>" + clsLibrary.dBReadString(row1["Address_Line_2"]);
            }
            MailToAddress += "<br>" + clsLibrary.dBReadString(row1["City"]);

            InvoiceAddress = "";
            SalesTeam = "";
            OrderStatus = clsLibrary.dBReadString(row["Order_Status"]);

            if (inst_del != null)
            {
                DeliveryInstructions = $"<pre>{clsLibrary.dBReadString(inst_del["Instructions"])}</pre>";
            }
            if (inst_inv != null)
            {
                InvoiceInstructions = $"<pre>{clsLibrary.dBReadString(inst_inv["Instructions"])}</pre>";
            }

            string template = @"<b>##name##</b><br/>
                    Dealer Contact: <i class='##clsDealer##'>##Dealer##</i><br/>
                    Vendor Contact: <i>##Vendor##</i><br/>
                    Carrier Contact: <i>##Carrier##</i><br/><br/>";
            MFG_PO_Info = "";
            foreach (DataRow r in MFG.Rows)
            {
                string copy = template;
                string vnd_no = clsLibrary.dBReadString(r["Vnd_No"]).Trim().ToLower();
                if (vnd_no == "ste01") copy = copy.Replace("##name##", "Steelcase (STE01)");
                if (vnd_no == "bra00") copy = copy.Replace("##name##", "Coalesse (BRA00)");
                string carrierContactPhone = "";
                string formatted = clsLibrary.dBReadString(r["Carr_Cont_Formatted_Phone_No"]).ToString();
                if (formatted != "") carrierContactPhone = formatted;
                string extension = clsLibrary.dBReadString(r["Carr_Cont_Extension"]);
                if (extension != "") carrierContactPhone += " x" + extension;
                string dealer = clsLibrary.dBReadString(r["Dlr_First_Name"]).Trim() + " " + clsLibrary.dBReadString(r["Dlr_Last_Name"]).Trim();
                string clsDealer = (dealer == "One Workplace") ? "text-red" : "";
                string vendor = clsLibrary.dBReadString(r["STC_First_Name"]).Trim() + " " + clsLibrary.dBReadString(r["STC_Last_Name"]).Trim();
                string carrier = clsLibrary.dBReadString(r["Carr_First_Name"]).Trim() + " " + clsLibrary.dBReadString(r["Carr_Last_Name"]).Trim()
                        + " | " + carrierContactPhone + clsLibrary.dBReadString(r["Carrier_Email"]).Trim();
                copy = copy.Replace("##clsDealer##", clsDealer);
                copy = copy.Replace("##Dealer##", dealer);
                copy = copy.Replace("##Vendor##", vendor);
                copy = copy.Replace("##Carrier##", carrier);

                MFG_PO_Info += copy;
            }
        }
    }

    public class LineInfoQuoteInquiry
    {
        public int LineNo { get; set; }
        public string VendorNo { get; set; }
        public string VendorName { get; set; }
        public string CatalogNo { get; set; }
        public string QtyOrdered { get; set; }
        public string Description { get; set; }
        public string Cost { get; set; }
        public string GP { get; set; }
        public string Color { get; set; }
        public bool HasColor { get; set; }
        public string GeneralTagging { get; set; }
        public bool IsBo { get; set; }
        public string Sell { get; set; }
        public string ListPricing { get; set; }
        public string MergedComments { get; set; }
        public bool IsOM { get; set; }
        public string FormattedPostalCode { get; set; }
		public string GPDlls { get; set; }
		public string List { get; set; }
		public string LineSell { get; set; }
		public string Comments { get; set; }
        public bool IsSteelcaseVendor { get; set; }
    }

    public class MiscLinesQuoteInquiry
    {
        public int MiscCharge { get; set; }
        public string VendorNo { get; set; }
        public string SalesCode { get; set; }
        public string ChargeCode { get; set; }
        public string Sell { get; set; }
        public string Cost { get; set; }
        public string RaceivingCostPercent { get; set; }
        public string TaxCode { get; set; }

    }
}