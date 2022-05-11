using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OWPApplications.Data;
using OWPApplications.Models.PostOrderPlacement;
using OWPApplications.Utils;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using System.IO;
using System.Text;
using Syncfusion.Drawing;
using Syncfusion.Pdf.Graphics;

namespace OWPApplications.Controllers
{
    public class PostOrderPlacementOSQController : Controller
    {
        ILogger _logger;
        DbHandler _db;
        IConfiguration _configuration;
        EmailHelper _emailHelper;


        public PostOrderPlacementOSQController(DbHandler dbHandler, IConfiguration configuration, ILoggerFactory logFactory, EmailHelper emailHelper)
        {
            _db = dbHandler;
            _configuration = configuration;
            _logger = logFactory.CreateLogger(nameof(PostOrderPlacementOSQController));
            _emailHelper = emailHelper;

        }

        public IActionResult Index(string companyCode, int? orderno, string projectid, List<int> locations)
        {
            string ret = "";
            try
            {
                PostOrderPlacementViewModel vm = new PostOrderPlacementViewModel();
                vm.ShowResults = false;

                var usedLocations = _db.PostOrderPlacementHandler.GetUsedLocations();

                ViewData["FormValue"] = orderno;
                ViewData["ProjectId"] = projectid;
                ViewData["SelectedUsedLocations"] = locations;
                ViewData["UsedLocations"] = usedLocations;
                ViewData["CompanyCode"] = companyCode;// xmlConfig.getCompanyCode();
                vm.OrderNo = orderno == null ? "" : orderno.ToString();
                ViewData["selectedCompany"] = companyCode;

                if (orderno != null || projectid != null)
                {
                    vm.ShowResults = true;

                    vm.HeaderInfo = _db.PostOrderPlacementHandler.LoadHeaderInfo(orderno ?? 0, projectid, "OSQ");
                    if (vm.HeaderInfo == null)
                    {
                        vm.ShowResults = false;
                    }
                    else
                    {
                        vm.SummaryInfo = _db.PostOrderPlacementHandler.GetLinesInfo(orderno, projectid, locations, "OSQ")?.OrderBy(x => x.OrderNo);
                        SetPOPDates(vm.SummaryInfo);
                    }

                    vm.EmailTypes = xmlConfig.GetEmailTypes();
                    vm.EmailFields = xmlConfig.GetEmailFields();
                    vm.HeaderHtml = ReplaceContents(xmlConfig.getPageTemplate("pop", "header"), vm);
                    vm.SummaryHeaderHtml = xmlConfig.getPageTemplate("pop", "summary_header");
                }

                return View(vm);
            }
            catch (Exception ex)
            {
                ret = ex.Message;
                _logger.LogError(ex, "Index Error");
            }
            return Ok(ret);
        }
        private string ReplaceContents(string template, PostOrderPlacementViewModel vm)
        {
            if (vm.HeaderInfo != null)
            {
                template = template.Replace("##HeaderInfo.ProjectId##", vm.HeaderInfo.ProjectId);
                template = template.Replace("##HeaderInfo.OrderTitle##", vm.HeaderInfo.OrderTitle);
                template = template.Replace("##HeaderInfo.CustomerNo##", vm.HeaderInfo.CustomerNo);
                template = template.Replace("##HeaderInfo.CustomerName##", vm.HeaderInfo.CustomerName);
                template = template.Replace("##HeaderInfo.SalesIDs##", vm.HeaderInfo.SalesIDs);
                template = template.Replace("##HeaderInfo.CustomerPONo##", vm.HeaderInfo.CustomerPONo);
                template = template.Replace("##HeaderInfo.ShipToAddress##", vm.HeaderInfo.ShipToAddress);
                template = template.Replace("##HeaderInfo.OrderStatus##", vm.HeaderInfo.OrderStatus);
                template = template.Replace("##HeaderInfo.DeliveryInstructions##", vm.HeaderInfo.DeliveryInstructions);
                template = template.Replace("##HeaderInfo.MFG_PO_Info##", vm.HeaderInfo.MFG_PO_Info);
                template = template.Replace("##HeaderInfo.Location_Code##", vm.HeaderInfo.Location_Code);
                template = template.Replace("##HeaderInfo.Location_Name##", vm.HeaderInfo.Location_Name);
            }
            return template;
        }
        public void SetPOPDates(IEnumerable<SummaryPostOrder> summaryPostOrders)
        {
            if (summaryPostOrders?.Count() > 0)
            {
                IEnumerable<SummaryPostOrder> pops = summaryPostOrders.GroupBy(x => new { x.OrderNo, x.VendorNo }).Select(y => y.First());
                foreach (var pop in pops)
                {
                    List<string> shipDate = new List<string>();
                    List<string> estimatedArrival = new List<string>();
                    List<string> requestedArrival = new List<string>();
                    var currShipDate = "";
                    var currEstimatedArrivalDate = "";
                    var currRequestedArrival = "";

                    foreach (var line in pop.Lines)
                    {
                        if (!string.IsNullOrEmpty(line.ShipDate) && line.ShipDate != currShipDate)
                            shipDate.Add(line.ShipDate.Replace(@" - MULTI", ""));
                        if (!string.IsNullOrEmpty(line.EstimatedArrivalDate) && line.EstimatedArrivalDate != currEstimatedArrivalDate)
                            estimatedArrival.Add(line.EstimatedArrivalDate.Replace(@" - MULTI", ""));
                        if (!string.IsNullOrEmpty(line.RequestedArrival) && line.RequestedArrival != currRequestedArrival)
                            requestedArrival.Add(line.RequestedArrival.Replace(@" - MULTI", ""));
                        currShipDate = line.ShipDate;
                        currEstimatedArrivalDate = line.EstimatedArrivalDate;
                        currRequestedArrival = line.RequestedArrival;
                    }

                    pop.ShipDate = String.Join(Environment.NewLine, shipDate.Distinct().OrderByDescending(x => x));
                    pop.EstimatedArrival = String.Join(Environment.NewLine, estimatedArrival.Distinct().OrderByDescending(x => x));
                    pop.RequestedArrival = String.Join(Environment.NewLine, requestedArrival.Distinct().OrderByDescending(x => x));
                }
            }
        }

        [HttpPost]
        public IActionResult SendEmail_PO(EmailFormPostOrder body)
        {
            clsLog log = new clsLog(_configuration);
            try
            {
                if (body.DataToSend == null) return BadRequest("No lines selected");
                if (body.DataToSend?.Count() <= 0) return BadRequest("DataToSend value is invalid.");

                foreach (var email in body.DataToSend)
                {
                    //string to = email.To.Replace("(H)", "").Replace("(A)", "").Trim();
                    string to = email.To.Trim();
                    string emailsAdded = ";" + to.ToLower() + ";";
                    int lastSpace = to.LastIndexOf(' ');
                    if (lastSpace > 0)
                    {
                        to = to.Substring(lastSpace).Trim();
                    }
                    string cc = "";
                    if (emailsAdded.IndexOf(";" + body.From.ToLower().Trim() + ";") < 0)
                    {
                        cc += body.From;
                        emailsAdded += body.From.ToLower().Trim() + ";";
                    }
                    if (body.CC1 != null)
                        if (emailsAdded.IndexOf(";" + clsLibrary.dBReadString(body.CC1).ToLower().Trim() + ";") < 0)
                        {
                            if (cc != "") cc += ";";
                            cc += body.CC1;
                            emailsAdded += clsLibrary.dBReadString(body.CC1).ToLower().Trim() + ";";
                        }
                    if (body.CC2 != null)
                        if (emailsAdded.IndexOf(";" + clsLibrary.dBReadString(body.CC2).ToLower().Trim() + ";") < 0)
                        {
                            if (cc != "") cc += ";";
                            cc += body.CC2;
                            emailsAdded += clsLibrary.dBReadString(body.CC2).ToLower().Trim() + ";";
                        }

                    _emailHelper.SendEmailWithReply(
                        body.From,
                        body.FromName,
                        to,
                        cc,
                        "",
                        "",
                        email.Subject(body),
                        email.Body(body),
                        null,
                        null,
                        "POP_OSQ");

                    _db.SaveActivity(new ActivityLog
                    {
                        YourEmail = body.From,
                        ToEmail = to,
                        Vendor = email.VendorNo,
                        Subject = email.Subject(body),
                        Body = email.Body(body),
                        CreatedBy = "POP_OSQ",
                        Order = body.Quote_OrderNo,
                        CompanyCode = "S"
                    });
                }
            }
            catch (Exception ex)
            {
                log.WriteError("POP LineDetailsPDF", ex.Message, ex);
                throw ex;
            }
            return Json(true);
        }

        public IActionResult DownloadCSVPostOrder(string orderno)
        {
            string header = "Line #, Vendor #, PO Suffix #, Process Code, Line Sales Code, Warehouse/ Network Installer, Ordered (Date), Catalog, Description, Ack #, Ack Date, Ship Date, Estimated Arrival, Received Date, Comments";
            var linescsv = new StringBuilder();
            byte[] buffer = Encoding.ASCII.GetBytes($"{header}\r\n{linescsv.ToString()}");

            return File(buffer, "text/csv", $"PostOrderPlacement_{orderno}_{DateTime.Now.ToString("yyyyMMdd")}.csv");
        }

        [HttpPost]
        public ActionResult LineDetailsPDF([FromBody] POPReportData report)
        {
            clsLog log = new clsLog(_configuration);

            try
            {
                string[] cols = report.Cols.Split(',');
                string displayNone = "style='display:none'";

                using (StreamWriter stream = new StreamWriter("wwwroot/files/pop/POPReport.html", false, System.Text.Encoding.UTF8))
                {



                    stream.Write(@"<html>" + stream.NewLine + "<head>" + stream.NewLine);
                    stream.Write(@"<style>.table td, .table th {text-align:center;border:1px solid gray;}</style></head>" + stream.NewLine);
                    stream.Write(@"<body>" + stream.NewLine);
                    stream.Write(@"" + stream.NewLine + "<br />" + stream.NewLine);
                    if (report.Type == "LD")
                    {
                        stream.Write(@"
                <table id='postorderTableDetails' class='table table-sm table-bordered text-center sortable' style='font-size:10px;'>
                    <thead>
                        <tr>
                            <th " + (!cols.Contains("1") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>PO #</th>
                            <th " + (!cols.Contains("2") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Vendor #</th>
                            <th " + (!cols.Contains("3") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Ordered(Date)</th>
                            <th " + (!cols.Contains("4") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Ack #</th>
                            <th " + (!cols.Contains("5") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Ack Date</th>
                            <th " + (!cols.Contains("6") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Ship Date</th>
                            <th " + (!cols.Contains("7") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Requested Arrival Date</th>
                            <th " + (!cols.Contains("8") ? displayNone : "") + @" scope='col' class='table-header' style='width: 7%;'>Lastest Received Date</th>
                            <th " + (!cols.Contains("14") ? displayNone : "") + @" scope='col' class='table-header' style='width: 7%;'>Soft Scheduled Date (Latest)</th>
                            <th " + (!cols.Contains("9") ? displayNone : "") + @" scope='col' class='table-header' style='width: 7%;'>Estimated Arrival Date</th>
                            <th " + (!cols.Contains("10") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Scheduled Arrival Date</th>
                            <th " + (!cols.Contains("11") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Carrier</th>
                            <th " + (!cols.Contains("12") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Tracking/PRO #/BOL</th>
                            <th " + (!cols.Contains("13") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Comments</th>
                        </tr>
                    </thead>
                    <tbody>");
                    }
                    else
                    {
                        stream.Write(@"
                <table id='postorderTableDetails' class='table table-sm table-bordered text-center sortable' style='font-size:10px;'>
                    <thead>
                        <tr>
                            <th " + (!cols.Contains("1") ? displayNone : "") + @" scope='col' class='table-header' style='width: 3%;'>PO #</th>
                            <th " + (!cols.Contains("2") ? displayNone : "") + @" scope='col' class='table-header' style='width: 3%;'>Line #</th>
                            <th " + (!cols.Contains("4") ? displayNone : "") + @" scope='col' class='table-header' style='width: 3%;'>Process Code</th>
                            <th " + (!cols.Contains("5") ? displayNone : "") + @" scope='col' class='table-header' style='width: 3%;'>Line Sales Code</th>
                            <th " + (!cols.Contains("2") ? displayNone : "") + @" scope='col' class='table-header' style='width: 11%;'>Vendor #</th>
                            <th " + (!cols.Contains("7") ? displayNone : "") + @" scope='col' class='table-header' style='width: 6%;'>Catalog</th>
                            <th " + (!cols.Contains("8") ? displayNone : "") + @" scope='col' class='table-header' style='width: 29%;'>Description</th>
                            <th " + (!cols.Contains("3") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Ordered Date</th>
                            <th " + (!cols.Contains("9") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Qty Ordered</th>
                            <th " + (!cols.Contains("10") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Qty Received</th>
                            <th " + (!cols.Contains("11") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Received Date</th>
                            <th " + (!cols.Contains("12") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Requested Arrival Date</th>
                            <th " + (!cols.Contains("13") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Estimated Arrival Date</th>
                            <th " + (!cols.Contains("14") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Ship Date</th>
                            <th " + (!cols.Contains("16") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Genearal Tagging</th>
                            <th " + (!cols.Contains("15") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Last Schedule Date</th>
                        </tr>
                    </thead>
                    <tbody>");
                    }

                    if (report.Type == "LD") //PO Summary report
                    {
                        foreach (var item in report.Data)
                        {
                            string clsACK = item.ACKNotReceived ? @"style='background-color:lightcoral'" : "";
                            string clsPO = item.PONotReceived ? @"style='background-color:yellow'" : "";
                            string clsRow = "ONE20, ONE22, ONE23, ONE24, ONE26, ONE27, ONE28,".Contains(item.VendorNo.Trim()) ? @"style='background-color:gray'" : "";
                            string clsEstArrDate = !string.IsNullOrEmpty(item.EstimatedArrivalColor) ? @"style='background-color:lightcoral'" : "";
                            string companyCode = xmlConfig.getCompanyCode();

                            stream.Write(@"<tr " + clsRow + "><td " + (!cols.Contains("1") ? displayNone : "") + ">" + string.Format("{0}{1}-{2}", companyCode, item.OrderNo, item.PoSuffix) + "</td>" +
                                          "<td " + (!cols.Contains("2") ? displayNone : "") + ">" + item.VendorName + "(" + item.VendorNo.Trim() + ")</td>" +
                                          "<td " + (!cols.Contains("3") ? displayNone : "") + ">" + (item.OrderedDate?.ToString("d") ?? string.Empty) + "</td>" +
                                          "<td " + (!cols.Contains("4") ? displayNone : "") + " " + clsACK + ">" + item.AckNo + "</td>" +
                                          "<td " + (!cols.Contains("5") ? displayNone : "") + " " + clsACK + ">" + item.AckDate + "</td>" +
                                          "<td " + (!cols.Contains("6") ? displayNone : "") + ">" + (item.ShipDate?.ToString("d") ?? string.Empty) + "</td>" +
                                          "<td " + (!cols.Contains("7") ? displayNone : "") + ">" + item.RequestedArrival + "</td>" +
                                          "<td " + (!cols.Contains("8") ? displayNone : "") + " " + clsPO + ">" + item.LastReceivedDate + "</td>" +
                                          "<td " + (!cols.Contains("9") ? displayNone : "") + " " + clsEstArrDate + ">" + item.LatestSoftSchDt + "</td>" +
                                          "<td " + (!cols.Contains("9") ? displayNone : "") + " " + clsEstArrDate + ">" + item.EstimatedArrival + "</td>" +
                                          "<td " + (!cols.Contains("10") ? displayNone : "") + ">" + item.ScheduledArrivalDate + "</td>" +
                                          "<td " + (!cols.Contains("11") ? displayNone : "") + ">" + item.Carrier + "</td>" +
                                          "<td " + (!cols.Contains("12") ? displayNone : "") + ">" + item.Tracking + "</td>" +
                                          "<td " + (!cols.Contains("13") ? displayNone : "") + ">" + item.Comments + "</td></tr>");
                        }
                    }
                    else
                    {
                        var orderedData = report.Data.SelectMany(x => x.Lines, (parent, child) => new { parent.VendorNo, parent.VendorName, parent.OrderNo, parent.PoSuffix, child.LineNo, child.ProcessCode, child.LineSalesCode, child.Werehouse_Network, child.Catalog, child.Description, child.Ordered, child.Received, child.ReceivedDate, child.RequestedArrival, child.EstimatedArrivalDate, child.ShipDate, parent.LatestSoftSchDt, child.GeneralTagging, parent.OrderedDate }).OrderBy(x => x.OrderNo).ThenBy(x => x.LineNo).ToList();
                        foreach (var item in orderedData)
                        {
                            if (!report.ExcludeLines || (item.ProcessCode != "BO" && item.ProcessCode != "OM" && item.ProcessCode != "PA"))
                            {
                                stream.Write(@"<tr><td " + (!cols.Contains("1") ? displayNone : "") + ">" + string.Format("{0}-{1}", item.OrderNo, item.PoSuffix) + "</td>" +
                                              "<td " + (!cols.Contains("2") ? displayNone : "") + ">" + item.LineNo + "</td>" +
                                              "<td " + (!cols.Contains("4") ? displayNone : "") + ">" + item.ProcessCode + "</td>" +
                                              "<td " + (!cols.Contains("5") ? displayNone : "") + ">" + item.LineSalesCode + "</td>" +
                                              "<td " + (!cols.Contains("6") ? displayNone : "") + ">" + item.VendorName + "(" + item.VendorNo.Trim() + ")</td>" +
                                              "<td " + (!cols.Contains("7") ? displayNone : "") + ">" + item.Catalog + "</td>" +
                                              "<td " + (!cols.Contains("8") ? displayNone : "") + ">" + item.Description + "</td>" +
                                              "<td " + (!cols.Contains("3") ? displayNone : "") + ">" + (item.OrderedDate?.ToString("d") ?? string.Empty) + "</td>" +
                                              "<td " + (!cols.Contains("9") ? displayNone : "") + ">" + item.Ordered + "</td>" +
                                              "<td " + (!cols.Contains("10") ? displayNone : "") + ">" + item.Received + "</td>" +
                                              "<td " + (!cols.Contains("11") ? displayNone : "") + ">" + item.ReceivedDate + "</td>" +
                                              "<td " + (!cols.Contains("12") ? displayNone : "") + ">" + item.RequestedArrival + "</td>" +
                                              "<td " + (!cols.Contains("13") ? displayNone : "") + ">" + item.EstimatedArrivalDate + "</td>" +
                                              "<td " + (!cols.Contains("14") ? displayNone : "") + ">" + item.ShipDate + "</td>" +
                                              "<td " + (!cols.Contains("16") ? displayNone : "") + ">" + item.GeneralTagging + "</td>" +
                                              "<td " + (!cols.Contains("15") ? displayNone : "") + ">" + item.LatestSoftSchDt + "</td></tr>");
                            }

                        }
                    }
                    stream.Write(@"</tbody></table>" + stream.NewLine + "</body>" + stream.NewLine + "</html>");
                }

                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjA3NTg3QDMxMzcyZTM0MmUzMEhhRDNnL1Z6TTBtTVR4VW5CaExZSkd0RVNDeVA3M2ZoYVoxaUEyakVWaGc9");

                HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();

                WebKitConverterSettings settings = new WebKitConverterSettings();

                settings.WebKitPath = "Libraries/QtBinariesDotNetCore";

                settings.WebKitViewPort = new Size(1024, 0);
                settings.EnableRepeatTableHeader = true;

                htmlConverter.ConverterSettings = settings;
                htmlConverter.ConverterSettings.Orientation = PdfPageOrientation.Landscape;
                htmlConverter.ConverterSettings.Margin.All = 10;
                htmlConverter.ConverterSettings.PdfHeader = AddPdfHeader(report.SearchBy, htmlConverter.ConverterSettings.PdfPageSize.Width, report.Data.FirstOrDefault().OrderNo, report.ProjectID, report.CustomerName, report.Location_Code == "52" ? "porter.jpg" : (report.Location_Code == "15" ? "splice.png" : "opensquare.png"));

                using (PdfDocument document = htmlConverter.Convert("wwwroot/files/pop/POPReport.html"))
                {
                    if (report.Type == "LD")
                    {
                        using (FileStream fileStream = new FileStream("wwwroot/files/pop/POPReportSummary.pdf", FileMode.Create, FileAccess.ReadWrite))
                        {
                            document.Save(fileStream);
                            document.Close(true);
                        }

                        return Json("LD");
                    }
                    else
                    {
                        using (FileStream fileStream = new FileStream("wwwroot/files/pop/POPReportPOSuffixLineDetails.pdf", FileMode.Create, FileAccess.ReadWrite))
                        {
                            document.Save(fileStream);
                            document.Close(true);
                        }

                        return Json("POS");
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteError("POP LineDetailsPDF", ex.Message, ex);
                return Json(ex);
            }
        }

        public IActionResult GetVendorByNo(string vendorNo)
        {
            return Json(_db.PostOrderPlacementHandler.GetVendorByNo(vendorNo));
        }

        private PdfPageTemplateElement AddPdfHeader(string searchby, float width, string order, string project, string customer, string logo)
        {
            float imgWidth = 0;
            float imgHeight = 0;
            switch (logo)
            {
                case "splice.png":
                    imgWidth = 100;
                    imgHeight = 60;
                    break;
                case "porter.jpg":
                    imgWidth = 100;
                    imgHeight = 70;
                    break;
                case "opensquare.png":
                    imgWidth = 100;
                    imgHeight = 60;
                    break;
                default:
                    break;
            }

            RectangleF rect = new RectangleF(0, 0, width, imgHeight);

            //Create a new instance of PdfPageTemplateElement class.     
            PdfPageTemplateElement header = new PdfPageTemplateElement(rect);
            PdfGraphics g = header.Graphics;
            string finalTitle = "Order # " + order + " - Project # " + project + " - Customer: " + customer;

            if (searchby != "")
            {
                finalTitle = "Project # " + project + " - Customer: " + customer;
            }

            //Draw logo
            using (FileStream fileImg = new FileStream("wwwroot/files/pop/" + logo, FileMode.Open))
            {
                PdfBitmap img = new PdfBitmap(fileImg);
                g.DrawImage(img, 0, 0, imgWidth, imgHeight);
            }

            //Draw title.
            PdfFont tfont = new PdfStandardFont(PdfFontFamily.Helvetica, 8);
            PdfSolidBrush brush = new PdfSolidBrush(Color.FromArgb(0,0,0));
            g.DrawString(finalTitle, tfont, brush, new RectangleF(imgWidth + 20, imgHeight / 2 - (tfont.MeasureString(finalTitle).Height / 2), tfont.MeasureString(finalTitle).Width, tfont.MeasureString(finalTitle).Height));            

            return header;
        }

    }
}
