using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OWPApplications.Data;
using OWPApplications.Models;
using OWPApplications.Models.FastTrack;
using OWPApplications.Models.InternalStatusReport;
using OWPApplications.Models.PostOrderPlacement;
using OWPApplications.Models.QuoteInquiry;
using OWPApplications.Models.StandardPrice;
using OWPApplications.Models.WarehouseDT;
using OWPApplications.Utils;
using static OWPApplications.Utils.EmailHelper;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.HtmlToPdf;

namespace OWPApplications.Controllers
{
    public class HomeController : Controller
    {
        ILogger _logger;
        DbHandler _db;
        IConfiguration _configuration;
        EmailHelper _emailHelper;

        public HomeController(DbHandler dbHandler, IConfiguration configuration, ILoggerFactory logFactory, EmailHelper emailHelper)
        {
            _db = dbHandler;
            _configuration = configuration;
            _logger = logFactory.CreateLogger(nameof(HomeController));
            _emailHelper = emailHelper;
        }

        public IActionResult Index()
        {
            ViewBag.ComingSoon = _configuration["CurrentEnvironment"] != "P";
            var announcements = _db.GetAnnouncements();

            return View(announcements);
        }

        #region quoteinquiry

        public IActionResult QuoteInquiry(string orderno)
        {
            //if (!CheckIfIsInProduction("QuoteInquiry"))
            //{
            //    return Redirect("/home");
            //}
            QuoteInquiryViewModel vm = new QuoteInquiryViewModel();
            ViewData["FormValue"] = orderno;
            if (orderno == null)
            {
                vm.ShowResults = false;

            }
            else
            {
                vm.ShowResults = true;
                vm.QuoteNo = orderno;

                // Get Header Info
                vm.HeaderInfo = _db.QuoteInquiryHandler.LoadHeaderInfo(orderno, "OWP");

                if (vm.HeaderInfo != null)
                {
                    //Get Lines Info
                    vm.linesInfos = _db.QuoteInquiryHandler.GetLinesInfo(orderno, new List<int>(), "OWP");
                    vm.MiscLines = _db.QuoteInquiryHandler.GetMiscLines(orderno, new List<int>(), "OWP");
                    string[] filterVendors = new string[] { "STE01", "BRA00", "ONE20", "ONE22", "ONE23", "ONE24", "ONE26", "ONE27", "ONE28", "AME01" };
                    List<VendorEmail> emails = _db.GetVendorEmails("QI", "OWP", false);

                    vm.Vendors = vm.linesInfos
                            .Select<LineInfoQuoteInquiry, VendorEmail>(x => new VendorEmail
                            {
                                VendorNo = x.VendorNo.Trim(),
                                Name = x.VendorName,
                                Addresses = emails?.FirstOrDefault(e => e.VendorNo == x.VendorNo.Trim())?.Addresses
                            })
                            .Distinct(new VendorComparer())
                            .Where(x => !filterVendors.Contains(x.VendorNo))
                            .OrderBy(x => x.VendorNo);

                    vm.TotalGP = _db.QuoteInquiryHandler.GetTotalGP(orderno, 0, "OWP");

                }
                else
                {
                    vm.ShowResults = false;
                }

            }
            return View(vm);
        }

        [HttpPost]
        public IActionResult UpdateQuoteInquiryComment(int quoteNo, int lineNo, string comment)
        {
            _db.QuoteInquiryHandler.UpdateLinesComments(quoteNo, lineNo, comment);

            return Ok("Comment was saved");
        }

        [HttpPost]
        public IActionResult SendEmail_QI([FromForm] string rawdata, IFormFile file)
        {
            IEnumerable<EmailFormQuoteInquiry> data = JsonConvert.DeserializeObject<IEnumerable<EmailFormQuoteInquiry>>(rawdata);
            foreach (var email in data)
            {
                string mailBody = QuoteInquiryViewModel.CreateEmailBody(email);
                string subject = QuoteInquiryViewModel.CreateSubjectEmail(email);


                _emailHelper.SendEmailWithReply(email.From, "", String.Join(",", email.To), email.From + ";" + email.CC1, email.CC2, "", subject, mailBody, file, null, "QI");


                _db.SaveActivity(new ActivityLog
                {
                    YourEmail = email.From,
                    ToEmail = String.Join(",", email.To),
                    Body = mailBody,
                    Subject = subject,
                    Order = email.Quote_OrderNo,
                    Vendor = email.VendorName,
                    CreatedBy = "QI",
                    CompanyCode = "W"
                });

            }

            return Json(true);
        }


        [HttpPost]
        public IActionResult WrikeQuoteInquiryDeletion(QuoteInquiryDeleteViewModel data)
        {
            string emailSubject = $"Quote Deletion Request: [{data.QuoteNoToDelete}] [{data.OrderCustomerName}] [{data.OrderSalespersonID}]";
            string emailBody = BuildQIDeletionEmailBody(data);

            var result = _emailHelper.SendEmailV2(_configuration, new EmailProperties
            {
                FromAddress = "wrikeintegrate@oneworkplace.com",
                FromName = "Wrike Integrate",
                BCC = new string[] { "wrike+into449943526@wrike.com" },
                CC = new string[] { data.DeletionRequestorEmail },
                Body = emailBody,
                Subject = emailSubject,
                Attachments = data.BOMSIFFile == null ? null : new IFormFile[] { data.BOMSIFFile }
            }, "QI_WrikeDeletion");

            _db.SaveActivity(new ActivityLog
            {
                YourEmail = data.DeletionRequestorEmail,
                ToEmail = "wrike+into449943526@wrike.com",
                Body = emailBody,
                Subject = emailSubject,
                Order = data.QuoteNoToDelete,
                Vendor = string.Empty,
                CreatedBy = "QIDeletionRequest",
                CompanyCode = "W"
            });

            return RedirectToAction("QuoteInquiry", new { orderno = data.QuoteNoToDelete });
        }

        [HttpPost]
        public IActionResult SendRedlineEmail([FromForm] string rawdata, IFormFile file)
        {
            IEnumerable<RedlineEmailForm> data_ = JsonConvert.DeserializeObject<IEnumerable<RedlineEmailForm>>(rawdata);
            RedlineEmailForm data = data_.ToList()[0];

            string body = BuildRedlineEmailBody(data);
            string subject = BuildRedlineEmailSubject(data);


            _emailHelper.SendEmailWithReply(data.YourEmail, "", data.To, data.YourEmail + ";" + data.CC1, data.CC2, "", subject, body, file, null, "QI_RedLine");


            _db.SaveActivity(new ActivityLog
            {
                YourEmail = data.YourEmail,
                ToEmail = data.To,
                Body = body,
                Subject = subject,
                Order = data.QuoteNO,
                Vendor = "",
                CreatedBy = "QIRedlineRequest",
                CompanyCode = "W"
            });

            return Json(true);
        }

        private string BuildQIDeletionEmailBody(QuoteInquiryDeleteViewModel data)
        {
            string body = $@"<html>
            <style>
            body {{font:12px arial, sans-serif;color:#666;}}
            table {{border - collapse:collapse;}}
            table, td, th {{border: 1px solid #ccc; padding:10px;}}
            th {{font - size:14px; background-color:#ff4d4d; border:1px solid #ff4d4d; color:#fff;}}
            p {{ margin: 0; padding: 0;}}
            </style>
            <body>
            <p style=""font-size: 15px; arial, sans-serif;color:#666;""><strong>Quote Inquiry Deletion Request</strong><p>
            <br/>
            <p><strong>Quote #:</strong> {data.QuoteNoToDelete}<p>
            <p><strong>Customer Name:</strong> {data.OrderCustomerName}<p>
            <p><strong>Salesperson ID #:</strong> {data.OrderSalespersonID}<p>
            <p><strong>Requested by:</strong> {data.DeletionRequestorName} [{data.DeletionRequestorEmail}]<p>
            <p><strong>Notes:</strong> {data.OtherNotes}<p>
            <br/>
            </body>
            </html>";

            return body;
        }

        private string BuildRedlineEmailSubject(RedlineEmailForm data)
        {
            string subject = @"Quote Redlines Request: " + data.QuoteNO + " " + data.CustomerName + " - " + data.OrderTitle;

            return subject;
        }

        private string BuildRedlineEmailBody(RedlineEmailForm data)
        {
            string body = @"
				<html>
				<style>
				body {font:12px arial, sans-serif;color:#666;}
				table {border-collapse:collapse;}
				table, td, th {border: 1px solid #ccc; padding:10px;}
				th {font-size:14px; background-color:#ff4d4d; border:1px solid #ff4d4d; color:#fff;}
				p {margin: 0;padding: 0;}
				</style>
				<body>
				<p>Please see the below requested quote changes:</p>
				<br />
				##LINES##
				<br /><br />
				##LEGEND##
				<b>***Please be sure to reply all to e-mail thread to ensure that all necessary parties receive your response***</b>
				</body>
				</html>
				";

            string lines = "";
            if (data.LinesData?.Count > 0)
            {
                lines = @"<table><thead><tr><th>Line #</th><th>Vendor #</th><th>Catalog #</th><th>General Tagging</th><th>Qty</th><th>Description</th><th>GP $</th><th>List</th><th>Sell</th><th>Cost</th><th>Comments</th><tr></thead><tbody>";
                foreach (var line in data.LinesData)
                {
                    if (!string.IsNullOrEmpty(line.Comment?.Trim()))
                    {
                        lines += $"<tr><td>{line.LineNo}</td><td>{line.VendorNo}</td><td>{line.CatalogNo}</td><td>{line.GeneralTagging}</td><td>{line.QtyOrdered}</td><td>{line.Description}</td><td>{line.GPDlls}</td><td>{line.List}</td><td>{line.LineSell}</td><td>{line.Cost}</td><td>{line.Comment.Trim()}</td></tr>";
                    }
                }
            }

            lines += @"</tbody></table>";

            if (!string.IsNullOrEmpty(lines))
            {
                body = body.Replace("##LINES##", lines);
            }
            else
            {
                body = body.Replace("##LINES##", "No lines checked.");
            }

            if (data.RFP_Bid)
            {
                body = body.Replace("##LEGEND##", @"<b>*Any additional Project/Bid discounting is needed and appreciated.</b><br />");
            }
            else
            {
                body = body.Replace("##LEGEND##", "");
            }

            return body;
        }


        #endregion

        #region fasttrack

        public IActionResult FastTrack(string from, string to, List<string> warehouse)
        {
            if (string.IsNullOrEmpty(from))
            {
                from = clsLibrary.GetWorkingDay(DateTime.Now, -1).ToString("MM/dd/yyyy");
                to = clsLibrary.GetWorkingDay(DateTime.Now, 1).ToString("MM/dd/yyyy");
            }

            FastTrackViewModel vm = new FastTrackViewModel
            {
                From = from,
                To = to,
                Warehouse = warehouse
            };

            ViewData["FromValue"] = vm.From;
            ViewData["ToValue"] = vm.To;
            ViewData["Warehouse"] = vm.Warehouse;

            if (vm.From == null)
            {
                vm.ShowFastTrackResults = false;
            }
            else
            {
                vm.ShowFastTrackResults = true;
                var rows = _db.FastTrackHandler.GetLinesInfo(vm.From, vm.To, vm.Warehouse);
                vm.Graphics = _db.FastTrackHandler.GetGraphicFastTrack(vm.Warehouse)?.OrderBy(x => x.Week).ToList();

                if (rows == null)
                {
                    vm.ShowFastTrackResults = false;
                }
                else
                {
                    vm.SummaryInfo = rows.Where(x => x.ReceivedStatus != "N").OrderByDescending(x => x.ReceivedStatus);
                }
            }

            return View(vm);
        }

        public IActionResult GetWeekInfoDetails(string dateFrom, string dateTo, List<string> warehouse)
        {
            return Json(_db.FastTrackHandler.GetWeekInfo(dateFrom, dateTo, warehouse));
        }


        #endregion

        #region standardprice

        public IActionResult StandardPrice(string orderno)
        {
            //if (!CheckIfIsInProduction("StandardPrice"))
            //{
            //    return Redirect("/home");
            //}
            StandardPriceViewModel vm = new StandardPriceViewModel
            {
                OrderNo = orderno
            };
            ViewData["FormValue"] = orderno;

            if (string.IsNullOrEmpty(orderno))
            {
                vm.ShowResults = false;
            }
            else
            {
                vm.ShowResults = true;
                vm.HeaderInfo = _db.StandardPriceHandler.LoadHeaderInfo(orderno);
                if (vm.HeaderInfo == null)
                {
                    vm.ShowResults = false;
                }
                else
                {
                    vm.LinesInfos = _db.StandardPriceHandler.GetLinesInfo(orderno);
                    vm.Totals = _db.StandardPriceHandler.GetTotals(orderno, 1);
                }

            }
            return View(vm);
        }

        public IActionResult GetTotalsStandardPrice(string orderno, int includeBO)
        {
            return Json(_db.StandardPriceHandler.GetTotals(orderno, includeBO));
        }

        public IActionResult DownloadCSVStandardPrice(string orderno)
        {
            string header = "Line #, General Tagging, Lines Notes, Vendor, Catalog, Product Description, Qty. Ordered, Unit Sell, Extended Sell, Unit Cost, Extended Cost, Unit List, Extended List, Cost Discount, GP %, GP $, Auto Priced";
            IEnumerable<LineInfoStandardPrice> lines = _db.StandardPriceHandler.GetLinesInfo(orderno);
            var records = (from line in lines
                           select new object[]{
                                line.LineNo,
                                ConvertToCsvCell(line.GeneralTagging),
                                ConvertToCsvCell(line.LineNotes),
                                line.VendorNo,
                                ConvertToCsvCell(line.CatalogNo),
                                ConvertToCsvCell(line.ProductDesc),
                                line.QtyOrdered,
                                line.UnitSell,
                                line.ExtendedSell,
                                line.UnitCost,
                                line.ExtendedCost,
                                line.UnitList,
                                line.ExtendedList,
                                line.CostDiscount,
                                line.GPPct,
                                line.GPDollars,
                                line.AutoPriced
                           }).ToList();

            var linescsv = new StringBuilder();
            records.ForEach(line =>
            {
                linescsv.AppendLine(string.Join(",", line));
            });

            byte[] buffer = Encoding.ASCII.GetBytes($"{header}\r\n{linescsv.ToString()}");
            return File(buffer, "text/csv", $"SalesforceStandardPriceList_{orderno}_{DateTime.Now.ToString("yyyyMMdd")}.csv");
        }

        private string ConvertToCsvCell(string value)
        {
            var mustQuote = value.Any(x => x == ',' || x == '\"' || x == '\r' || x == '\n');

            if (!mustQuote)
            {
                return value;
            }

            value = value.Replace("\"", "\"\"");

            return string.Format("\"{0}\"", value);
        }

        #endregion

        #region internalstatusreport

        [RequestFormLimits(ValueCountLimit = int.MaxValue)]
        public IActionResult InternalStatusReport(string orderno, List<string> salesperson, string cutoffdate, List<string> selectedcustomers,
            string salespersonfrommv, string c, string s, string view, string salespersontype, string regionISR, string o, string projfolder)
        {
            regionISR = regionISR ?? "OWP";
            var allSalespersons = _db.InternalStatusReportHandler.LoadSalesPerson(regionISR);

            bool comesFromMV = !string.IsNullOrEmpty(salespersonfrommv);

            if (comesFromMV)
            {
                selectedcustomers = _db.InternalStatusReportHandler.LoadCustomerList(salesperson, regionISR).Select(x => x.ID).ToList();
            }

            ViewData["ProjectFolder"] = projfolder;
            ViewData["CutOffDate"] = cutoffdate;
            ViewData["SalesPersons"] = allSalespersons;
            ViewData["SPType"] = salespersontype ?? "";
            ViewData["Region"] = regionISR ?? "OWP";
            ViewData["Vendors"] = _db.GetVendorEmails("POP", regionISR, true).ToList();

            if (s != null && s != "")
            {
                salesperson = s.Split(",").ToList();
            }
            if (c != null && c != "")
            {
                selectedcustomers = c.Split(",").ToList();
            }
            for (var i = 0; i < selectedcustomers.Count(); i++)
            {
                while (selectedcustomers[i].Length < 6)
                {
                    selectedcustomers[i] += " ";
                }
            }
            var allCustomers = _db.InternalStatusReportHandler.LoadCustomerList(salesperson, regionISR);
            ViewData["Customers"] = allCustomers;
            ViewData["SalesPerson"] = salesperson;
            ViewData["SelectedCustomers"] = selectedcustomers;
            ViewData["OrderNo"] = o;


            InternalStatusReportViewModel vm = new InternalStatusReportViewModel
            {
                OrderNo = o,
                CutOffDate = cutoffdate,
                SalesPersons = allSalespersons,
                Customers = allCustomers,
                SelectedCustomers = selectedcustomers,
                ComesFromMV = comesFromMV,
                SalespersonType = !string.IsNullOrEmpty(salespersonfrommv) ? salespersonfrommv.Split('_')[0] : "",
                SalespersonCode = !string.IsNullOrEmpty(salespersonfrommv) ? salespersonfrommv.Split('_')[1] : "",
                View = view
        };

            if (view == "det")
            {
                vm.ShowDetails = true;
            }
            else
            {
                vm.ShowSummary = true;
            }

            if (!string.IsNullOrEmpty(projfolder))
            {
                vm.SummaryInfo = _db.InternalStatusReportHandler.GetLinesInfo(orderno ?? o, new List<string>(), "", null, salespersontype, regionISR, projfolder).ToList();
                vm.VendorMiscCharges = _db.InternalStatusReportHandler.GetVendorMiscCharges(orderno ?? o, "", "", null, regionISR, projfolder).ToList();
            }
            else if (!string.IsNullOrEmpty(orderno) || !string.IsNullOrEmpty(o))
            {
                vm.SummaryInfo = _db.InternalStatusReportHandler.GetLinesInfo(orderno ?? o, new List<string>(), "", null, salespersontype, regionISR, "").ToList();
                vm.VendorMiscCharges = _db.InternalStatusReportHandler.GetVendorMiscCharges(orderno ?? o, "", "", null, regionISR, "").ToList();
            }
            else if (salesperson != null && salesperson.Any())
            {
                vm.SummaryInfo = _db.InternalStatusReportHandler.GetLinesInfo("", salesperson, cutoffdate ?? "", selectedcustomers, salespersontype, regionISR, "").ToList();
                vm.ShowViewClosedOrdersLink = true;
            }

            if (vm.SummaryInfo != null && vm.SummaryInfo.Count > 0)
            {
                double totalSellEligibleforPartialInvoicing = 0;

                vm.SummaryInfo = vm.SummaryInfo.Distinct().OrderByDescending(x => x.PercentageSellAvailablePartialInvoicing).ToList();

                vm.Vendors = new List<Vendor>();

                foreach (var si in vm.SummaryInfo)
                {
                    si.SellEligibleforPartialInvoicing = si.Lines.Where(x => x.InvoicedColor == "green").Select(x => x.OpenSellWithoutFormat).Sum();
                    si.TotalOpenSell = si.Lines.Select(x => x.OpenSellWithoutFormat).Sum();
                    si.ScheduledDates = new List<ScheduledDate>();
                    var schedDates = new List<ScheduledDate>();
                    foreach (var line in si.Lines)
                    {
                        if (!string.IsNullOrEmpty(line.ScheduleDate))
                        {
                            schedDates.Add(new ScheduledDate
                            {
                                Date = line.ScheduleDate,
                                Color = line.ScheduledDateColor
                            });
                        }
                    }
                    si.ScheduledDates.AddRange(schedDates.GroupBy(x => x.Date).Select(x => x.First()));

                    //Vendor vnd = new Vendor();
                    //string[] preventVendors = { "AME01", "ONE20", "ONE22", "ONE23", "ONE24", "ONE26", "ONE27", "ONE28", "ONE2W" };
                    //foreach (var item in si.Lines)
                    //{
                    //    if (!Array.Exists(preventVendors, element => element == item.VendorNo.Trim()))
                    //    {
                    //        vnd = new Vendor { VendorNo = item.VendorNo, VendorEmail = item.VendorEmail };
                    //        if (!vm.Vendors.ToList().Contains(new Vendor { VendorNo = item.VendorNo, VendorEmail = item.VendorEmail, Order = item.OrderNo }))
                    //        {
                    //            vm.Vendors.Add(new Vendor { VendorNo = item.VendorNo, VendorEmail = item.VendorEmail, Order = item.OWP_PO });
                    //        }
                    //    }                        
                    //}

                    //vm.Vendors = vm.Vendors.OrderBy(x => x.VendorNo).ToList();
                }

                foreach (var si in vm.SummaryInfo.GroupBy(p => p.OrderNo).Select(grp => grp.FirstOrDefault()).ToList())
                {
                    totalSellEligibleforPartialInvoicing += si.Lines.Where(x => x.InvoicedColor == "green").Select(x => x.OpenSellWithoutFormat).Sum();
                }

                vm.TotalOrdersOpen = vm.SummaryInfo.GroupBy(p => p.OrderNo).Select(grp => grp.FirstOrDefault()).ToList().Count.ToString();
                vm.TotalOpenCost = vm.SummaryInfo.GroupBy(p => p.OrderNo).Select(grp => grp.FirstOrDefault()).ToList().Select(x => x.TotalOpenCost).Sum().ToString("C");
                vm.TotalOpenSell = vm.SummaryInfo.GroupBy(p => p.OrderNo).Select(grp => grp.FirstOrDefault()).ToList().Select(x => x.TotalOpenSell).Sum().ToString("C");
                vm.TotalSellEligibleforPartialInvoicing = totalSellEligibleforPartialInvoicing.ToString("C");

                if (!string.IsNullOrEmpty(orderno) || !string.IsNullOrEmpty(o))
                {
                    vm.ActiveIndex = vm.SummaryInfo.IndexOf(new SummaryInfoInternalStatus { OrderNo = orderno != null ? orderno.Trim() : o.Trim() });
                    if (vm.ActiveIndex >= 0)
                    {
                        vm.SummaryInfo[vm.ActiveIndex].HeaderInfo = _db.InternalStatusReportHandler.LoadHeaderInfo(orderno ?? o, regionISR);
                    }
                }

                vm.ResultSalespersons = vm.SummaryInfo.Select(x => x.SalespersonName).Distinct().ToList();


            }            

            return View(vm);
        }

        [HttpPost]
        public IActionResult SendEmails_IS(EmailFormSatusReport body)
        {
            if (body.DataToSend?.Count() <= 0) return BadRequest("DataToSend value is invalid.");

            string orderNo = body.Quote_OrderNo;
            string orderTitle = body.OrderTitle.Trim();
            bool isInvoice = Convert.ToBoolean(body.HasToInvoice);
            string invoiceReceived = body.InvoiceReceived;
            string invoiceType = body.InvoiceType;
            var summaryInfo = _db.InternalStatusReportHandler.GetLinesInfo(orderNo, new List<string>(), "", null, null, body.Region, "").ToList();
            double totalSell = summaryInfo[0].TotalOpenSell;
            double sellPartialInv = Convert.ToDouble(body.TotalSellIncludingTax);
            var linesNotForInv = new List<string>();
            string[] filterOWPVendors = { "A1M01", "ADV02", "ALL22", "ANN06", "CHI15", "CHI10", "COR13", "COM08", "CPI01", "COR30", "FIR20", "INS13", "MAN05", "MCG04", "ONT02", "PAC08", "PRO46", "QUA06", "SER01", "TAM02", "TOP95" };
            foreach (var email in body.DataToSend)
            {
                string createdBy = _configuration[email.EmailType];
                string to = isInvoice ? body.From : email.To;
                string cc2 = string.IsNullOrEmpty(body.CC2) ? "" : body.CC2 + ";";
                if (isInvoice)
                {
                    if (email.CompanyCode == "W" && Array.Exists(filterOWPVendors, x => x == email.VendorNo))
                    {
                        cc2 += "cbrossmer@oneworkplace.com";
                    }

                    if (invoiceType == "Partial")
                    {
                        summaryInfo = summaryInfo.Where(x => x.TotalOpenSell > 10000 && x.PercentageSellAvailablePartialInvoicing >= 0.7).Select(x => x).ToList();
                        linesNotForInv = GetLinesNotEligibleForInvoicing(summaryInfo);
                    }
                }

                _emailHelper.SendEmailWithReply(
                        @"wrikeintegrate@oneworkplace.com", body.Name, to, body.CC1, cc2, invoiceType == "VendorInvoice" ? _db.InternalStatusReportHandler.GetEmailsByEmailType("Invoice").WrikeCC : null,
                        email.Subject(orderNo, orderTitle, invoiceReceived, email.ProcessCode), 
                        email.Body(orderNo, linesNotForInv, totalSell, sellPartialInv, invoiceType, invoiceReceived, body.Name, email.ProcessCode),
                        null, null, "ISR");

                _db.SaveActivity(new ActivityLog

                {
                    YourEmail = @"wrikeintegrate@oneworkplace.com",
                    Name = body.Name,
                    ToEmail = email.To,
                    Body = email.Body(orderNo, linesNotForInv, totalSell, sellPartialInv, invoiceType, invoiceReceived, body.Name, email.ProcessCode),
                    Subject = email.Subject(orderNo, orderTitle, invoiceReceived, email.ProcessCode),
                    Order = orderNo,
                    Vendor = email.VendorNo,
                    CreatedBy = createdBy,
                    Line = email.LineNo,
                    CompanyCode = body.Region == "OWP" ? "W" : "S"
                });

                _db.InternalStatusReportHandler.InsertAccountabilityIntoActivityLog(Convert.ToInt32(orderNo), body.Accountability, body.CompanyCode);

                if (isInvoice && (createdBy == "InvoiceComplete" || createdBy == "InvoicePartial"))
                {
                    _db.InternalStatusReportHandler.InsertWrikeTask("W", orderNo,
                        invoiceType == "Partial" ? sellPartialInv : totalSell,
                        body.From, email.Subject(orderNo, orderTitle, invoiceReceived, email.ProcessCode), "", createdBy);
                }
            }

            return Json(true);
        }

        [HttpPost]
        public IActionResult SendEmails_IS_CRD(EmailFormSatusReportCRD body)
        {
            if (body.DataToSend.Count() <= 0)
            {
                return BadRequest("DataToSend value is invalid.");
            }

            _emailHelper.SendEmailWithReply(
                @"wrikeintegrate@oneworkplace.com",
                body.Name,
                body.From,
                body.CC1,
                body.CC2,
                _db.InternalStatusReportHandler.GetEmailsByEmailType("CRD").WrikeCC,
                body.Subject(), body.Body(body.DataToSend), null, null, "ISR_CRD");

            _db.SaveActivity(new ActivityLog
            {
                YourEmail = @"wrikeintegrate@oneworkplace.com",
                Name = body.Name,
                ToEmail = body.From,
                Body = body.Body(body.DataToSend),
                Subject = body.Subject(),
                CreatedBy = "ISR Summary",
                CompanyCode = body.Region == "OWP" ? "W" : "S"
            });

            return Json(true);
        }

        [HttpPost]
        public IActionResult SendEmails_IS_VI(EmailFormVendorInvoice body)
        {
            if (body.Orders.Count() <= 0)
            {
                return BadRequest("DataToSend value is invalid.");
            }

            foreach (var ord in body.Orders)
            {
                if (ord.Vendors.Count() <= 0)
                {
                    return BadRequest("DataToSend value is invalid.");
                }

                SummaryInfoInternalStatus order = _db.InternalStatusReportHandler.GetLinesInfo(ord.OrderNo, new List<string>(), "", null, null, body.Region, "").FirstOrDefault();

                foreach (var vnd in ord.Vendors)
                {
                    foreach (var line in order.Lines)
                    {
                        if (vnd.VendorNo.Split("-")[0] == line.VendorNo.Trim())
                        {
                            foreach (var item in vnd.VendorEmail)
                            {
                                _emailHelper.SendEmailWithReply(
                                body.From,
                                body.Name,
                                item,
                                body.From + ";" + body.CC1,
                                body.CC2,
                                "",
                                body.Subject(order.CustomerName, vnd.VendorNo.Split("-")[0], order.OrderNo, vnd.Suffix, line.ACK, body.Region),
                                body.Body(body.Comments, body.Name, body.Region),
                                null,
                                null,
                                "ISR_VIN"
                            );

                                _db.SaveActivity(new ActivityLog
                                {
                                    YourEmail = body.From,
                                    Name = body.Name,
                                    ToEmail = item,
                                    Body = body.Body(body.Comments, body.Name, body.Region),
                                    Subject = body.Subject(order.CustomerName, vnd.VendorNo.Split("-")[0], order.OrderNo, line.OWP_PO, line.ACK, body.Region),
                                    CreatedBy = "ISR Vendor Invoice",
                                    CompanyCode = body.Region == "OWP" ? "W" : "S"
                                });
                            }
                            

                            break;
                        }
                    }
                }
            }

            return Json(true);
        }

        [HttpPost]
        public ActionResult GenerateISRPDF([FromBody] List<SummaryInfoInternalStatus> report)

        {
            try
            {
                removeOldFiles();
                string guid = Guid.NewGuid().ToString();
                using (StreamWriter stream = new StreamWriter("wwwroot/files/POPReport" + guid + ".html", false, Encoding.UTF8))
                {

                    stream.Write(@"<html>" + stream.NewLine + "<head>" + stream.NewLine);
                    stream.Write(@"
                    <style>.table td, .table th {text-align:center;border-bottom:1px solid gray;border-right:1px solid gray;padding:2px} 
                        .table {border-left:1px solid gray;border-top:1px solid gray;border-spacing:0}</style>
                    <link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/css/bootstrap.min.css'>
                    <script src='https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/js/bootstrap.min.js'></script>" + stream.NewLine + "</head>" + stream.NewLine);
                    stream.Write(@"<body>" + stream.NewLine);

                    stream.Write(@"<img src='../images/owp-logo2.png' alt='One Workplace' style='float:right;margin-top:10px;margin-right:25px;margin-bottom:20px'>");
                    stream.Write(@"<h2>" + report.FirstOrDefault().SalespersonName + " - " + DateTime.Now.ToString("MM/dd/yyyy") + "</h2>" + stream.NewLine + "<br />" + stream.NewLine);
                    stream.Write(@"
                        <table id='InternalStatusReportTable' class='table table-sm table-bordered text-center sortable'>
                            <thead>
                                <tr>
                                <th scope='col' class='table-header' style='width:5%'>Project ID</th>
                                <th scope='col' class='table-header' style='width:7%'>Accountability</th>
                                <th scope='col' class='table-header' style='width:4%'>Order #</th>
                                <th scope='col' class='table-header' style='width:6%'>Order Date</th>
                                <th scope='col' class='table-header' style='width:6%'>Customer</th>
                                <th scope='col' class='table-header' style='width:7%'>Order Title</th>
                                <th scope='col' class='table-header' style='width:6%'>Last Estimated Arrival Date</th>
                                <th scope='col' class='table-header' style='width:6%'>Customer Requested Date</th>
                                <th scope='col' class='table-header' style='width:5%'>Percentage of Sell Available for Partial Invoicing</th>
                                <th scope='col' class='table-header' style='width:7%'>Sell Eligible for Partial Invoicing</th>
                                <th scope='col' class='table-header' style='width:7%'>Total Open Sell</th>
                                <th scope='col' class='table-header' style='width:7%'>Total Open Cost</th>
                                <th scope='col' class='table-header' style='width:6%'>Qty Lines Open</th>
                                <th scope='col' class='table-header' style='width:6%'>Scheduled Dates</th>
                                <th scope='col' class='table-header' style='width:6%'>Requested CRD</th>
                                <th scope='col' class='table-header' style='width:7%'>Comments</th>
                            </tr>
                        </thead>
                        <tbody>");

                    foreach (var item in report)
                    {
                        stream.Write(@"<tr>" +
                                 "<td>" + item.ProjectID + "</td>" +
                                 "<td>" + item.Accountability + "</td>" +
                                 "<td>" + item.OrderNo + "</td>" +
                                 "<td>" + item.OrderDate + "</td>" +
                                 "<td>" + item.CustomerName + "</td>" +
                                 "<td>" + item.OrderTitle + "</td>" +
                                 "<td>" + item.EstimatedArrivalDate + "</td>" +
                                 "<td>" + item.CustomerRequestDate + "</td>" +
                                 "<td>" + item.PercentageSellAvailablePartialInvoicing + "</td>" +
                                 "<td>" + item.SellEligibleforPartialInvoicing + "</td>" +
                                 "<td>" + item.TotalOpenSell + "</td>" +
                                 "<td>" + item.TotalOpenCost + "</td>" +
                                 "<td>" + item.QtyOpen + "</td>" +
                                 "<td>" + item.ScheduledDates.FirstOrDefault()?.Date + "</td>" +
                                 "<td>" + item.RequestedCRD + "</td>" +
                                 "<td>" + item.Comment + "</td>" +
                                 "</ tr > "
                               );
                    }
                }

                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjA3NTg3QDMxMzcyZTM0MmUzMEhhRDNnL1Z6TTBtTVR4VW5CaExZSkd0RVNDeVA3M2ZoYVoxaUEyakVWaGc9");

                HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();

                WebKitConverterSettings settings = new WebKitConverterSettings();

                settings.WebKitPath = "Libraries/QtBinariesDotNetCore";

                htmlConverter.ConverterSettings = settings;
                htmlConverter.ConverterSettings.Orientation = PdfPageOrientation.Landscape;
                htmlConverter.ConverterSettings.Margin.All = 10;

                using (PdfDocument document = htmlConverter.Convert("wwwroot/files/POPReport" + guid + ".html"))

                {
                    string docName = "wwwroot/files/ISRDetail" + guid + ".pdf";

                    using (FileStream fileStream = new FileStream(docName, FileMode.Create, FileAccess.ReadWrite))
                    {
                        document.Save(fileStream);
                        document.Close(true);
                    }
                }

                return Json(guid);

            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }


        [HttpPost]
        public IActionResult SendInvoiceEmails([FromForm] string rawdata, IFormFile file)
        {
            EmailFormInvoice data = JsonConvert.DeserializeObject<EmailFormInvoice>(rawdata);
            
            if (data.LinesData.Count() <= 0)
            {
                return BadRequest("LinesData value is invalid.");
            }

            EmailAddressesList dbEmails = new EmailAddressesList();

            if (data.Region == "OWP")
            {
                dbEmails = _db.InternalStatusReportHandler.GetEmailsByEmailType("Invoice");
            }
            else
            {
                dbEmails = _db.InternalStatusReportHandler.GetEmailsByEmailType("OSQ-Invoicing");
            }
            data.To = data.From;

            if (data.CC1 != null)
            {
                if (dbEmails.Standard != null)
                {
                    data.CC1 += "," + dbEmails.Standard;
                }
            }
            else
            {
                if (dbEmails.Standard != null)
                {
                    data.CC1 = dbEmails.Standard;
                }
                else
                {
                    data.CC1 = "";
                }
            }

            data.CC2 += string.IsNullOrEmpty(data.CC2) ? "" : ";";
            data.BCC = dbEmails.WrikeCC;

            string createdBy = "";
            switch (data.InvoiceType)
            {
                case "Complete":
                    createdBy = "InvoiceComplete";
                    break;
                case "Partial":
                    createdBy = "InvoicePartial";
                    break;
                default:
                    break;
            }

            foreach (var lineData in data.LinesData)
            {
                string subject = GetInvoiceEmailSubject(lineData, data);
                string body = GetInvoiceEmailBody(lineData, data);
                _emailHelper.SendEmailWithReply(@"wrikeintegrate@oneworkplace.com", data.Name, data.To, data.CC1, data.CC2, data.BCC, subject, body, file, null, "ISR_Invoice");

                _db.SaveActivity(new ActivityLog
                {
                    YourEmail = data.From,
                    Name = data.Name,
                    ToEmail = data.To,
                    Body = body,
                    Subject = subject,
                    Order = lineData.OrderNo,
                    Vendor = "",
                    CreatedBy = createdBy,
                    Line = "",
                    CompanyCode = data.Region == "OWP" ? "W" : "S"
                });

                _db.InternalStatusReportHandler.InsertAccountabilityIntoActivityLog(Convert.ToInt32(lineData.OrderNo), lineData.Accountability, lineData.CompanyCode);

                _db.InternalStatusReportHandler.InsertWrikeTask(
                    "W",
                    lineData.OrderNo,
                    data.InvoiceType == "Partial" ? Convert.ToDouble(lineData.SellEligibleForPartialInvoicing) : Convert.ToDouble(lineData.TotalSell),
                    data.From,
                    subject,
                    "",
                    createdBy);
            }

            return Json(true);
        }

        [HttpGet]
        public string GetEmailByEmailType(string emailType)
        {
            return _db.InternalStatusReportHandler.GetEmailsByEmailType(emailType).Standard;
        }

        [HttpGet]
        public List<string> GetEmailByEmailTypeList(string emailType)
        {
            return _db.InternalStatusReportHandler.GetEmailsByEmailTypeList(emailType).Select(x => x.Standard).ToList();
        }

        private string GetInvoiceEmailSubject(LineData lineData, EmailFormInvoice data)
        {
            switch (data.InvoiceType)
            {
                case "Complete": return string.Format("Invoice Complete: {0} | {1} | {2}", lineData.OrderNo, lineData.OrderTitle, data.CompletionDate);
                case "Partial": return string.Format("Change to Bill Partial: {0} | {1} | {2}", lineData.OrderNo, lineData.OrderTitle, data.ReceivedOrPaid);
                default: return "";
            }
        }

        private string GetInvoiceEmailBody(LineData lineData, EmailFormInvoice data)
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

            switch (data.InvoiceType)
            {
                case "Complete":
                    bodyHTML = bodyHTML.Replace("##BODY##", string.Format(@"<p>Dear Dana:</p></br> 
							<p>Please invoice Order #{0}, complete. Feel free to contact me with any additional questions.</p>", lineData.OrderNo));
                    break;
                case "Partial":
                    if (data.ReceivedOrPaid == "Received")
                    {
                        bodyHTML = bodyHTML.Replace("##BODY##", string.Format(@"<p>Dear Dana:</p></br> 
							<p>Please bill lines on order #{0} that have been received in the system. Feel free to contact me with any additional questions.</p>", lineData.OrderNo));
                    }
                    else
                    {
                        bodyHTML = bodyHTML.Replace("##BODY##", string.Format(@"<p>Dear Dana:</p></br> 
							<p>Please bill lines on order #{0} that have been paid/cost verified. Feel free to contact me with any additional questions.</p>", lineData.OrderNo));
                    }
                    break;
                default: return "";
            }

            if (!string.IsNullOrEmpty(data.Comments))
            {
                bodyHTML = bodyHTML.Replace("##COMMENTS##", string.Format(@"<p>Comments: {0}</p>", data.Comments));
            }
            else
            {
                bodyHTML = bodyHTML.Replace("##COMMENTS##", "");
            }

            bodyHTML = bodyHTML.Replace("##NAME##", !string.IsNullOrEmpty(data.Name) ? data.Name : "");

            return bodyHTML;
        }

        private string GetSelectedCustomersUri(string[] selectedCustomers)
        {
            string ret = "";

            foreach (var cst in selectedCustomers)
            {
                ret += "c=" + cst + "&";
            }

            return ret.TrimEnd('&');
        }

        private List<string> GetLinesNotEligibleForInvoicing(List<SummaryInfoInternalStatus> summaryInfo)
        {
            List<string> ret = new List<string>();

            if (summaryInfo.Count > 0)
            {
                foreach (var si in summaryInfo)
                {
                    if (si.Lines.Count > 0)
                    {
                        foreach (var line in si.Lines)
                        {
                            if (line.InvoicedColor == "Red")
                            {
                                ret.Add(line.LineNo.ToString());
                            }
                        }
                    }
                }
            }

            return ret;
        }

        #endregion

        #region warehousedt
        public IActionResult WarehouseDT(string date, string scheduleType, List<string> warehouse)
        {
            WarehouseDTViewModel vm = new WarehouseDTViewModel()
            {
                Date = date,
                ScheduleType = scheduleType,
                Warehouse = warehouse
            };

            ViewData["FormValue"] = vm.Date;
            ViewData["ScheduleType"] = vm.ScheduleType;
            ViewData["ScheduleTypes"] = _db.WarehouseDTHandler.GetScheduleTypes();
            ViewData["Warehouse"] = vm.Warehouse;

            if (vm.Date == null)
            {
                vm.ShowResults = false;
            }
            else
            {
                vm.ShowResults = true;
                vm.SummaryInfo = _db.WarehouseDTHandler.GetLinesInfo(vm.Date, vm.Warehouse);
                vm.StagingNames = _db.WarehouseDTHandler.GetStagingNames();

                if (vm.SummaryInfo == null)
                {
                    vm.ShowResults = false;
                }
                else
                {
                    if (vm.ScheduleType != null && vm.ScheduleType != "ALL")
                        vm.SummaryInfo = vm.SummaryInfo.Where(x => x.ScheduleType == vm.ScheduleType).Select(x => x);

                    foreach (var si in vm.SummaryInfo)
                    {
                        int lines = si.LinesInfo.Count();
                        int stagedLines = si.LinesInfo.Where(x => x.Staged).Count();
                        int loadedLines = si.LinesInfo.Where(x => x.Loaded).Count();
                        int dtShippedLines = si.LinesInfo.Where(x => x.Location.Trim() == "DT SHIPPED").Count();

                        if (lines == stagedLines) si.Location = "1";
                        else if (lines > stagedLines && stagedLines > 0) si.Location = "Partial";
                        else si.Location = "";

                        if (lines == loadedLines) si.LoadedValue = "1";
                        else if (lines > stagedLines && stagedLines > 0) si.LoadedValue = "Partial";
                        else si.LoadedValue = "";

                        if (lines == dtShippedLines) si.IsDtShipped = true;
                    }

                    vm.SummaryInfo = vm.SummaryInfo.OrderBy(x => x.IsDtShipped).ThenByDescending(x => x.OrderNo);
                }
            }

            return View(vm);
        }

        [HttpPost]
        public IActionResult SendEmails_WDT(EmailFormWarehouseDT data)
        {
            if (data.DataToSend.Count() <= 0)
            {
                return BadRequest("DataToSend value is invalid.");
            }
            foreach (var item in data.DataToSend)
            {
                _emailHelper.SendEmailWithReply(
                data.From,
                data.Name,
                @"scwarehouse@oneworkplace.com",
                data.From + "," + data.CC1,
                data.CC2,
                "",
                data.Subject(item.OrderNo, item.CustName, data.InstallDate), data.Body(item.OrderNo, data.InstallDate, data.Comments, data.Name), null, null, "WDT");

                _db.SaveActivity(new ActivityLog
                {
                    YourEmail = data.From,
                    Name = data.Name,
                    ToEmail = @"scwarehouse@oneworkplace.com",
                    Body = data.Body(item.OrderNo, data.InstallDate, data.Comments, data.Name),
                    Subject = data.Subject(item.OrderNo, item.CustName, data.InstallDate),
                    CreatedBy = "WDT",
                    CompanyCode = "W"
                });
            }            

            return Json(true);
        }

        [HttpPost]
        public IActionResult UpdateStagingNames(string[] names, string orderno)
        {
            int[] namesInt = Array.ConvertAll(names, int.Parse);
            _db.WarehouseDTHandler.UpdateStagingNames(namesInt, orderno);

            return Ok("Ok");
        }

        [HttpPost]
        public IActionResult UpdateLoadedLine(string orderNo, string lineNo, bool isLoaded)
        {
            _db.WarehouseDTHandler.UpdateLoadedLine(orderNo, lineNo, isLoaded);

            return Ok("Ok");
        }

        public IActionResult DownloadCSVWarehouseDT(CSVWarehouse csv)
        {
            string header = "Line #, Qty Ordered, Qty Received, Qty Scheduled, Vendor, Catalog, Description, Location";
            WarehouseDTViewModel vm = new WarehouseDTViewModel();
            vm.SummaryInfo = _db.WarehouseDTHandler.GetLinesInfo(csv.Date, csv.Warehouse.Split(',').ToList());
            InfoWarehouseDT info = vm.SummaryInfo.Where(x => x.OrderNo == csv.OrderNo).FirstOrDefault();
            List<LineInfoWarehouseDT> lines = info.LinesInfo;
            var records = (from line in lines
                           select new object[]{
                                line.LineNo,
                                line.QtyOrdered,
                                line.QtyReceived,
                                line.QtyScheduled,
                                line.Vendor,
                                line.CatalogNo,
                                line.Description,
                                line.Location
                           }).ToList();

            var linescsv = new StringBuilder();
            records.ForEach(line =>
            {
                linescsv.AppendLine(string.Join(",", line));
            });

            byte[] buffer = Encoding.ASCII.GetBytes($"{header}\r\n{linescsv.ToString()}");
            return File(buffer, "text/csv", $"WarehouseDT_{csv.OrderNo}_{DateTime.Now.ToString("yyyyMMdd")}.csv");
        }



        #endregion

        #region generic


        [HttpPost]
        public IActionResult UpdateValues(OrderValues data)
        {

            if (string.IsNullOrEmpty(data.Source))
            {
                return BadRequest("Source could not be empty.");
            }

            _db.UpdateValues(data, _logger);            

            return Ok("Comment was saved");
        }

        [HttpGet]
        public IActionResult GetVendorsEmails(string app, string region, bool isLiveISR)
        {
            return Json(_db.GetVendorEmails(app, region, isLiveISR));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string msg)
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Msg = msg });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Success()
        {
            return View();
        }

        private bool CheckIfIsInProduction(string page)
        {
            string apps = _configuration["AppsOnProduction"];
            string env = _configuration["CurrentEnvironment"];
            if (env == "P") return true;
            return apps.ToLower().Contains(page.ToLower());
        }


        public ActionResult TestEmail()
        {
            return Ok(_emailHelper.SendEmailWithReply(
                        "swit@studiowest.ph", "swit@studiowest.ph",
                        "santiago.cassone@empactit.com", "",
                        null,
                        "",
                        "Test Reply To studiowest.ph [2]",
                        "Testing sending email from noreply@oneworkplace, with FromName and ReplyTo as studiowest.ph", null, null, "TEST"));

        }

        [Route("/Home/InternalStatusReport/SalespersonCustomers")]
        [HttpPost]
        public JsonResult SalespersonCustomers(List<string> salespersonid, string regionISR)
        {
            return new JsonResult(_db.InternalStatusReportHandler.LoadCustomerList(salespersonid.ToList(), regionISR));
        }

        [Route("/Home/InternalStatusReport/Salespersons")]
        [HttpPost]
        public JsonResult Salespersons(string regionISR)
        {
            return new JsonResult(_db.InternalStatusReportHandler.LoadSalesPerson(regionISR));
        }

        private void removeOldFiles()
        {
            try
            {
                string[] files = Directory.GetFiles("wwwroot/files");
                foreach (var item in files)
                {
                    DateTime creation = System.IO.File.GetLastWriteTime(item);
                    if (creation.AddDays(1) < DateTime.Now)
                    {
                        //remove file
                        System.IO.File.Delete(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


    }


}
