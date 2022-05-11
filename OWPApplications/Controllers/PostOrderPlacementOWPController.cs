using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OWPApplications.Data;
using OWPApplications.Models.PostOrderPlacement;
using OWPApplications.Utils;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OWPApplications.Controllers
{
    public class PostOrderPlacementOWPController : Controller
    {
        DbHandler _db;
        IConfiguration _configuration;
        EmailHelper _emailHelper;
        ILogger _logger;

        public PostOrderPlacementOWPController(DbHandler dbHandler, IConfiguration configuration, ILoggerFactory logFactory, EmailHelper emailHelper)
        {
            _db = dbHandler;
            _configuration = configuration;
            _logger = logFactory.CreateLogger(nameof(PostOrderPlacementOWPController));
            _emailHelper = emailHelper;

        }
        public IActionResult Index(int? orderno, string projectid)
        {
            PostOrderPlacementViewModel vm = new PostOrderPlacementViewModel();
            ViewData["FormValue"] = orderno;
            ViewData["ProjectId"] = projectid;
            vm.OrderNo = orderno.ToString();

            if (orderno == null && projectid == null)
            {
                vm.ShowResults = false;
            }
            else
            {
                vm.ShowResults = true;
                vm.HeaderInfo = _db.PostOrderPlacementHandler.LoadHeaderInfo(orderno ?? 0, projectid, "OWP");
                if (vm.HeaderInfo == null)
                {
                    vm.ShowResults = false;
                }
                else
                {
                    vm.SummaryInfo = _db.PostOrderPlacementHandler.GetSummaryInfo(orderno, projectid).OrderBy(x => x.OrderNo);
                    vm.LineDetail = _db.PostOrderPlacementHandler.GetLineDetailInfo(orderno, projectid).OrderBy(x => x.LineNo);
                    SetPOPDates(vm.SummaryInfo);
                }

            }
            return View(vm);
        }

        public void SetPOPDates(IEnumerable<SummaryPostOrder> summaryPostOrders)
        {
            var pops = summaryPostOrders.GroupBy(x => new { x.OrderNo, x.VendorNo }).Select(y => y.First());
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

        [HttpPost]
        public IActionResult SendEmail_PO(EmailFormPostOrder body)
        {

            if (body.DataToSend == null)
            {
                return BadRequest("DataToSend value is invalid.");
            }

            foreach (var email in body.DataToSend)
            {
                _emailHelper.SendEmailWithReply(body.From, body.FromName, email.To, body.CC1, body.CC2, "", email.Subject(body), email.Body(body), null, null, "POP_OWP");

                _db.SaveActivity(new ActivityLog
                {
                    YourEmail = body.From,
                    ToEmail = email.To,
                    Body = email.Body(body),
                    Subject = email.Subject(body),
                    Order = body.Quote_OrderNo,
                    Vendor = email.VendorNo,
                    CreatedBy = "POP_OWP",
                    CompanyCode = "W"
                });
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
            try
            {
                _logger.LogError("Start LineDetailsPDF {0}",DateTime.Now.ToString());
                removeOldFiles();
                _logger.LogError("Old files removed {0}", DateTime.Now.ToString());
                string[] cols = report.Cols.Split(',');
                string displayNone = "style='display:none'";
                string guid = Guid.NewGuid().ToString();
                using (StreamWriter stream = new StreamWriter("wwwroot/files/POPReport" + guid + ".html", false, System.Text.Encoding.UTF8))
                {
                    stream.Write(@"<html>" + stream.NewLine + "<head>" + stream.NewLine);
                    stream.Write(@"
                    <style>.table td, .table th {text-align:center;border:1px solid gray;}</style>" + stream.NewLine + "</head>" + stream.NewLine);
                    stream.Write(@"<body>" + stream.NewLine);
                    if (report.Type == "LD")
                    {
                        //stream.Write(@"<h2>" + report.Data.FirstOrDefault().OrderNo + "</h2>" + stream.NewLine + "<br />" + stream.NewLine);
                        stream.Write(@"
                    <table id='postorderTableDetails' class='table table-sm table-bordered text-center sortable'>
                        <thead>
                            <tr>
                                <th " + (!cols.Contains("1") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>PO #</th>
                                <th " + (!cols.Contains("2") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Vendor #</th>
                                <th " + (!cols.Contains("3") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Ordered(Date)</th>
                                <th " + (!cols.Contains("4") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Ack #</th>
                                <th " + (!cols.Contains("5") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Ack Date</th>
                                <th " + (!cols.Contains("6") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Ship Date</th>
                                <th " + (!cols.Contains("7") ? displayNone : "") + @" scope='col' class='table-header' style='width: 25%;'>Requested Arrival Date</th>
                                <th " + (!cols.Contains("8") ? displayNone : "") + @" scope='col' class='table-header' style='width: 7%;'>Lastest Received Date</th>
                                <th " + (!cols.Contains("9") ? displayNone : "") + @" scope='col' class='table-header' style='width: 7%;'>Estimated Arrival Date</th>
                                <th " + (!cols.Contains("10") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Scheduled Arrival Date</th>
                                <th " + (!cols.Contains("11") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Carrier</th>
                                <th " + (!cols.Contains("12") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Tracking/PRO #/BOL</th>
                                <th " + (!cols.Contains("13") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Comments</th>
                            </tr>
                        </thead>
                        <tbody>");
                    }
                    else if (report.Type == "POS")
                    {
                        //stream.Write(@"<h2>" + report.Data.FirstOrDefault().OrderNo + "</h2>" + stream.NewLine + "<br />" + stream.NewLine);
                        stream.Write(@"
                    <table id='postorderTableDetails' class='table table-sm table-bordered text-center sortable'>
                        <thead>
                            <tr>
                                <th " + (!cols.Contains("1") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>PO #</th>
                                <th " + (!cols.Contains("2") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Line #</th>
                                <th " + (!cols.Contains("3") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Vendor #</th>
                                <th " + (!cols.Contains("15") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Ordered(Date)</th>
                                <th " + (!cols.Contains("4") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Process Code</th>
                                <th " + (!cols.Contains("5") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Line Sales Code</th>
                                <th " + (!cols.Contains("6") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Warehouse/Network Installer</th>
                                <th " + (!cols.Contains("7") ? displayNone : "") + @" scope='col' class='table-header' style='width: 6%;'>Catalog</th>
                                <th " + (!cols.Contains("8") ? displayNone : "") + @" scope='col' class='table-header' style='width: 16%;'>Description</th>
                                <th " + (!cols.Contains("9") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Qty Ordered</th>
                                <th " + (!cols.Contains("10") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Qty Received</th>
                                <th " + (!cols.Contains("11") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Received Date</th>
                                <th " + (!cols.Contains("12") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Requested Arrival Date</th>
                                <th " + (!cols.Contains("13") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Estimated Arrival Date</th>
                                <th " + (!cols.Contains("14") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Ship Date</th>
                            </tr>
                        </thead>
                        <tbody>");
                    }
                    else if (report.Type == "DET")
                    {
                        //stream.Write(@"<img src='images/owp-logo2.png' alt='One Workplace' style='float:right;margin-top:10px;margin-right:25px;margin-bottom:20px'>");
                        stream.Write(@"<h2>" + report.Data.FirstOrDefault().Salesperson + " - " + DateTime.Now.ToString("MM/dd/yyyy") + "</h2>" + stream.NewLine + "<br />" + stream.NewLine);
                        stream.Write(@"
                    <table id='postorderTableDetails' class='table table-sm table-bordered text-center sortable'>
                        <thead>
                            <tr>
                                <th " + (!cols.Contains("1") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>PO #</th>
                                <th " + (!cols.Contains("2") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Line #</th>
                                <th " + (!cols.Contains("3") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Process Code</th>
                                <th " + (!cols.Contains("4") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Order/PO Suffix</th>
                                <th " + (!cols.Contains("5") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Vendor ID</th>
                                <th " + (!cols.Contains("6") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Vendor Name</th>
                                <th " + (!cols.Contains("7") ? displayNone : "") + @" scope='col' class='table-header' style='width: 6%;'>Catalog #</th>
                                <th " + (!cols.Contains("8") ? displayNone : "") + @" scope='col' class='table-header' style='width: 16%;'>Description</th>
                                <th " + (!cols.Contains("9") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>QTY Ordered</th>
                                <th " + (!cols.Contains("10") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>General Tagging</th>
                                <th " + (!cols.Contains("11") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Ordered Date</th>
                                <th " + (!cols.Contains("12") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Requested Arrival Date</th>
                                <th " + (!cols.Contains("13") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Ack #</th>
                                <th " + (!cols.Contains("14") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>Ship Date</th>
                                <th " + (!cols.Contains("15") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Estimated Arrival Date</th>
                                <th " + (!cols.Contains("16") ? displayNone : "") + @" scope='col' class='table-header' style='width: 8%;'>QTY Received</th>
                                <th " + (!cols.Contains("17") ? displayNone : "") + @" scope='col' class='table-header' style='width: 6%;'>Latest Received Date</th>
                                <th " + (!cols.Contains("18") ? displayNone : "") + @" scope='col' class='table-header' style='width: 16%;'>Carrier</th>
                                <th " + (!cols.Contains("19") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Tracking #</th>
                                <th " + (!cols.Contains("20") ? displayNone : "") + @" scope='col' class='table-header' style='width: 5%;'>Freeform Notes</th>
                            </tr>
                        </thead>
                        <tbody>");
                    }

                    if (report.Type == "LD")
                    {
                        foreach (var item in report.Data)
                        {
                            string clsACK = item.ACKNotReceived ? @"style='background-color:lightcoral'" : "";
                            string clsPO = item.PONotReceived ? @"style='background-color:yellow'" : "";
                            string clsRow = "ONE20, ONE22, ONE23, ONE24, ONE26, ONE27, ONE28,".Contains(item.VendorNo.Trim()) ? @"style='background-color:gray'" : "";
                            string clsEstArrDate = !string.IsNullOrEmpty(item.EstimatedArrivalColor) ? @"style='background-color:lightcoral'" : "";

                            stream.Write(@"<tr " + clsRow + "><td " + (!cols.Contains("1") ? displayNone : "") + ">" + string.Format("W{0}-{1}", item.OrderNo, item.PoSuffix) + "</td>" +
                                          "<td " + (!cols.Contains("2") ? displayNone : "") + ">" + item.VendorName + "(" + item.VendorNo.Trim() + ")</td>" +
                                          "<td " + (!cols.Contains("3") ? displayNone : "") + ">" + item.OrderedDate + "</td>" +
                                          "<td " + (!cols.Contains("4") ? displayNone : "") + " " + clsACK + ">" + item.AckNo + "</td>" +
                                          "<td " + (!cols.Contains("5") ? displayNone : "") + " " + clsACK + ">" + item.AckDate + "</td>" +
                                          "<td " + (!cols.Contains("6") ? displayNone : "") + ">" + item.ShipDate + "</td>" +
                                          "<td " + (!cols.Contains("7") ? displayNone : "") + ">" + item.RequestedArrival + "</td>" +
                                          "<td " + (!cols.Contains("8") ? displayNone : "") + " " + clsPO + ">" + item.LastReceivedDate + "</td>" +
                                          "<td " + (!cols.Contains("9") ? displayNone : "") + " " + clsEstArrDate + ">" + item.EstimatedArrival + "</td>" +
                                          "<td " + (!cols.Contains("10") ? displayNone : "") + ">" + item.ScheduledArrivalDate + "</td>" +
                                          "<td " + (!cols.Contains("11") ? displayNone : "") + ">" + item.Carrier + "</td>" +
                                          "<td " + (!cols.Contains("12") ? displayNone : "") + ">" + item.Tracking + "</td>" +
                                          "<td " + (!cols.Contains("13") ? displayNone : "") + ">" + item.Comments + "</td></tr>");
                        }
                    }
                    else if (report.Type == "POS")
                    {
                        var orderedData = report.Data.SelectMany(x => x.Lines, (parent, child) => new { parent.VendorNo, parent.VendorName, parent.OrderNo, parent.PoSuffix, child.LineNo, child.ProcessCode, child.LineSalesCode, child.Werehouse_Network, child.Catalog, child.Description, child.Ordered, child.Received, child.ReceivedDate, child.RequestedArrival, child.EstimatedArrivalDate, child.ShipDate, parent.OrderedDate }).OrderBy(x => x.OrderNo).ThenBy(x => x.LineNo).ToList();
                        foreach (var item in orderedData)
                        {
                            stream.Write(@"<tr><td " + (!cols.Contains("1") ? displayNone : "") + ">" + item.OrderNo + (item.PoSuffix == "" ? "" : "-" + item.PoSuffix) + "</td>" +
                                              "<td " + (!cols.Contains("2") ? displayNone : "") + ">" + item.LineNo + "</td>" +
                                              "<td " + (!cols.Contains("3") ? displayNone : "") + ">" + item.VendorName + "(" + item.VendorNo.Trim() + ")</td>" +
                                              "<td " + (!cols.Contains("15") ? displayNone : "") + ">" + item.OrderedDate + "</td>" +
                                              "<td " + (!cols.Contains("4") ? displayNone : "") + ">" + item.ProcessCode + "</td>" +
                                              "<td " + (!cols.Contains("5") ? displayNone : "") + ">" + item.LineSalesCode + "</td>" +
                                              "<td " + (!cols.Contains("6") ? displayNone : "") + ">" + item.Werehouse_Network + "</td>" +
                                              "<td " + (!cols.Contains("7") ? displayNone : "") + ">" + item.Catalog + "</td>" +
                                              "<td " + (!cols.Contains("8") ? displayNone : "") + ">" + item.Description + "</td>" +
                                              "<td " + (!cols.Contains("9") ? displayNone : "") + ">" + item.Ordered + "</td>" +
                                              "<td " + (!cols.Contains("10") ? displayNone : "") + ">" + item.Received + "</td>" +
                                              "<td " + (!cols.Contains("11") ? displayNone : "") + ">" + item.ReceivedDate + "</td>" +
                                              "<td " + (!cols.Contains("12") ? displayNone : "") + ">" + item.RequestedArrival + "</td>" +
                                              "<td " + (!cols.Contains("13") ? displayNone : "") + ">" + item.EstimatedArrivalDate + "</td>" +
                                              "<td " + (!cols.Contains("14") ? displayNone : "") + ">" + item.ShipDate + "</td></tr>");
                        }
                    }
                    else if (report.Type == "DET")
                    {
                        foreach (var item in report.Data)
                        {
                            stream.Write(@"<tr><td " + (!cols.Contains("1") ? displayNone : "") + ">" + item.POReference + "</td>" +
                                          "<td " + (!cols.Contains("2") ? displayNone : "") + ">" + item.LineNo + "</td>" +
                                          "<td " + (!cols.Contains("3") ? displayNone : "") + ">" + item.ProcessingCode + "</td>" +
                                          "<td " + (!cols.Contains("4") ? displayNone : "") + ">" + item.Order_POSuffix + "</td>" +
                                          "<td " + (!cols.Contains("5") ? displayNone : "") + ">" + item.VendorID + "</td>" +
                                          "<td " + (!cols.Contains("6") ? displayNone : "") + ">" + item.VendorName + "</td>" +
                                          "<td " + (!cols.Contains("7") ? displayNone : "") + ">" + item.CatalogNo + "</td>" +
                                          "<td " + (!cols.Contains("8") ? displayNone : "") + ">" + item.Description + "</td>" +
                                          "<td " + (!cols.Contains("9") ? displayNone : "") + ">" + item.QtyOrdered + "</td>" +
                                          "<td " + (!cols.Contains("10") ? displayNone : "") + ">" + item.GeneralTagging + "</td>" +
                                          "<td " + (!cols.Contains("11") ? displayNone : "") + ">" + ((DateTime)item.OrderedDate).ToString("MM/dd/yyyy") + "</td>" +
                                          "<td " + (!cols.Contains("12") ? displayNone : "") + ">" + item.RequestedArrivalDate.ToString("MM/dd/yyyy") + "</td>" +
                                          "<td " + (!cols.Contains("13") ? displayNone : "") + ">" + item.AckNo + "</td>" +
                                          "<td " + (!cols.Contains("14") ? displayNone : "") + ">" + ((DateTime)item.ShipDate).ToString("MM/dd/yyyy") + "</td>" +
                                          "<td " + (!cols.Contains("15") ? displayNone : "") + ">" + item.EstimatedArrivalDate.ToString("MM/dd/yyyy") + "</td>" +
                                          "<td " + (!cols.Contains("16") ? displayNone : "") + ">" + item.QtyReceived + "</td>" +
                                          "<td " + (!cols.Contains("17") ? displayNone : "") + ">" + (item.LatestReceivedDate.Year == 1 ? "N/A" : item.LatestReceivedDate.ToShortDateString()) + "</td>" +
                                          "<td " + (!cols.Contains("18") ? displayNone : "") + ">" + item.Carrier + "</td>" +
                                          "<td " + (!cols.Contains("19") ? displayNone : "") + ">" + item.TrackingNo + "</td>" +
                                          "<td " + (!cols.Contains("20") ? displayNone : "") + ">" + item.FreeformNotes + "</td></tr>");
                        }
                    }
                }

                _logger.LogError("Html modified {0}", DateTime.Now.ToString());

                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjA3NTg3QDMxMzcyZTM0MmUzMEhhRDNnL1Z6TTBtTVR4VW5CaExZSkd0RVNDeVA3M2ZoYVoxaUEyakVWaGc9");

                _logger.LogError("Licence done {0}", DateTime.Now.ToString());

                HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();
                WebKitConverterSettings settings = new WebKitConverterSettings();
                settings.WebKitPath = "Libraries/QtBinariesDotNetCore";
                settings.EnableRepeatTableHeader = true;
                htmlConverter.ConverterSettings = settings;
                htmlConverter.ConverterSettings.Orientation = PdfPageOrientation.Landscape;
                htmlConverter.ConverterSettings.Margin.All = 10;

                _logger.LogError("Settings done {0}", DateTime.Now.ToString());

                string docName = "";

                PdfDocument document = htmlConverter.Convert("wwwroot/files/POPReport" + guid + ".html");
                
                    switch (report.Type)
                    {
                        case "LD":
                            {
                                docName = "wwwroot/files/POPReportSummary" + guid + ".pdf"; break;
                            }
                        case "POS":
                            {
                                docName = "wwwroot/files/POPReportPOSuffixLineDetails" + guid + ".pdf"; break;
                            }
                        case "DET":
                            {
                                docName = "wwwroot/files/POPReportLineDetail" + guid + ".pdf"; break;
                            }

                        default:
                            return Json(null);
                    }

                    _logger.LogError("Html opened {0}", DateTime.Now.ToString());

                    using (FileStream fileStream = new FileStream(docName, FileMode.Create, FileAccess.ReadWrite))
                    {
                        document.Save(fileStream);

                        _logger.LogError("Document saved {0}", DateTime.Now.ToString());

                        document.Close(true);

                        _logger.LogError("Document closed {0}", DateTime.Now.ToString());

                    }

                _logger.LogError("PDF created {0}", DateTime.Now.ToString());

                return Json(docName.Replace("wwwroot", ".."));

            }
            catch (Exception ex)
            {
                return Json(ex);
            }
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
    }
}
