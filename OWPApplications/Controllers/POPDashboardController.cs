using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OWPApplications.Data;
using OWPApplications.Models.POPDashboard;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;

namespace OWPApplications.Controllers
{
    public class POPDashboardController : Controller
    {
        private readonly DbHandler _db;

        public POPDashboardController(DbHandler dbHandler)
        {
            _db = dbHandler;
        }


        public IActionResult Index(string region)
        {
            ViewData["SalesPersons"] = _db.POPDashboardHandler.LoadSalesPerson(region);
            ViewData["Customers"] = new List<Models.SelectValues>();
            ViewData["Region"] = region;

            var vm = new POPDashboardViewModel
            {
                SalesPersons = _db.POPDashboardHandler.LoadSalesPerson(region),
                Customers = Enumerable.Empty<Models.SelectValues>().ToList(),
                Budget = Enumerable.Empty<BudgetActualModel>(),
                PastCRDs = Enumerable.Empty<PastCRDModel>(),
                FutureCRDs = Enumerable.Empty<FutureCRDModel>(),
                OpenQuotes = Enumerable.Empty<OpenQuotes>()
            };

            return View(vm);
        }

        [HttpPost, DisableRequestSizeLimit, RequestFormLimits(ValueCountLimit = 15000)]
        public IActionResult Index(string salesperson, string customerno, string projectid, string[] selectedCustomers, string region)
        {
            var allSalespersons = _db.POPDashboardHandler.LoadSalesPerson(region);
            var salespersonCustomers = _db.POPDashboardHandler.LoadCustomerList(salesperson, region);

            // NOTICE: many of these ViewData values are later repeated into the model (POPDashboardViewModel)
            // the reason they are kept in ViewData is because of the design related to the use of "_FormSearcher" 
            // which is a generic partial view that does not have a strongly typed model
            // the recomendation will be to move all the "forms" in _FormSearcher to their especific view objects.
            // the above assumes that there is NO good reason to keep all the Forms in _FormSearcher 
            ViewData["Region"] = region;
            ViewData["SalesPersons"] = allSalespersons;
            ViewData["Customers"] = salespersonCustomers;
            ViewData["SelectedCustomers"] = selectedCustomers;
            ViewData["SalesPersonsFormValue"] = salesperson;
            ViewData["CustomerNoFormValue"] = customerno;
            ViewData["ProjectIDFormValue"] = projectid;

            var vm = new POPDashboardViewModel
            {
                Salesperson = salesperson,
                SelectedCustomers = selectedCustomers,
                CustomerNo = customerno,
                ProjectId = projectid,
                SalesPersons = allSalespersons,
                Customers = salespersonCustomers,
                Summary = _db.POPDashboardHandler.Get_PostOrderDashboard_Summary(salesperson, selectedCustomers, customerno, projectid, region).FirstOrDefault(),
                Budget = _db.POPDashboardHandler.Get_PostOrderDashboard_BudgetVsActual(salesperson, selectedCustomers, customerno, projectid, region),
                PastCRDs = _db.POPDashboardHandler.Get_PostOrderDashboard_PastCRDs(salesperson, selectedCustomers, customerno, projectid, region),
                FutureCRDs = _db.POPDashboardHandler.Get_PostOrderDashboard_FutureCRDs(salesperson, selectedCustomers, customerno, projectid, region),
                OpenQuotes = _db.POPDashboardHandler.Get_PostOrderDashboard_OpenQuotes(salesperson, selectedCustomers, customerno, projectid, region).Where(x => x.Date_Entered > DateTime.Now.AddMonths(-6)).Select(x => x).OrderByDescending(x => x.Date_Entered),
                CustomerView = _db.POPDashboardHandler.Get_PostOrderDashboard_CustomerView(salesperson, selectedCustomers, customerno, projectid, region)
            };

            return View(vm);
        }


        /// <summary>
        /// Method used to reload the Customer select box after a new Salesperson is selected
        /// </summary>
        /// <param name="salespersonid"></param>
        /// <returns></returns>
        [Route("/POPDashboard/SalespersonCustomers")]
        public JsonResult SalespersonCustomers(string salespersonid, string region)
        {
            return new JsonResult(_db.POPDashboardHandler.LoadCustomerList(salespersonid, region));
        }

        public JsonResult GetCustomerView(string salesperson, string selectedCustomers, string customerno, string projectid, string region)
        {
            return new JsonResult(_db.POPDashboardHandler.Get_PostOrderDashboard_CustomerView(salesperson, selectedCustomers.Split(','), customerno, projectid, region));
        }

        [HttpPost]
        public ActionResult CustomerViewPDF([FromBody] CustomerViewPdfReport report)
        {
            try
            {
                removeOldFiles();
                string guid = Guid.NewGuid().ToString();
                using (StreamWriter stream = new StreamWriter("wwwroot/files/PDBCustomerViewReport" + guid + ".html", false, System.Text.Encoding.UTF8))
                {
                    stream.Write(@"<html>" + stream.NewLine + "<head>" + stream.NewLine);
                    stream.Write(@"
                    <style>.table td {text-align: center;}</style>
                    <link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/css/bootstrap.min.css'>
                    <script src='https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/js/bootstrap.min.js'></script>" + stream.NewLine + "</head>" + stream.NewLine);
                    stream.Write(@"<body>" + stream.NewLine);
                    stream.Write(@"<img src='images/owp-logo2.png' alt='One Workplace' style='float:right;margin-top:10px;margin-right:25px'>");
                    stream.Write(@"<h2 style='margin-top:15px;margin-left:15px'>Customer View</h2>");
                    foreach (var cust in report.Customers)
                    {
                        string custCode = cust.Split(" (")[1].Replace(")", "");
                        string custName = cust.Split(" (")[0];
                        stream.Write(@"<p style='margin-left:20px;font-size:14px;line-height:12px'>" + custCode + " - " + custName + "</p>");
                    }
                    stream.Write(@"
                    <table id='customerViewTableDetails' class='table table-sm table-bordered text-center sortable'>
                        <thead>
                            <tr>
                                <th scope = 'col' class='table-header' style='width: 5%;'>Quote #</th>
                                <th scope = 'col' class='table-header' style='width: 5%;'>Order #</th>
                                <th scope = 'col' class='table-header' style='width: 5%;'>Customer PO #</th>
                                <th scope = 'col' class='table-header' style='width: 10%;'>Order Title</th>
                                <th scope = 'col' class='table-header' style='width: 8%;'>Estimated Arrival Date</th>
                                <th scope = 'col' class='table-header' style='width: 8%;'>Install/Delivery Date</th>
                                <th scope = 'col' class='table-header' style='width: 10%;'>Note 1</th>
                                <th scope = 'col' class='table-header' style='width: 10%;'>Note 2</th>
                                <th scope = 'col' class='table-header' style='width: 10%;'>Note 3</th>
                            </tr>
                        </thead>
                    <tbody>");

                    foreach (var item in report.Data)
                    {
                        stream.Write(@"<tr><td>" + item.Quote_No + "</td>" +
                                          "<td>" + item.Order_No + "</td>" +
                                          "<td>" + item.Customer_PO + "</td>" +
                                          "<td>" + item.Order_Title + "</td>" +
                                          "<td>" + item.Estimated_Arr_Date + "</td>" +
                                          "<td>" + item.Install_Delivery_Date + "</td>" +
                                          "<td>" + item.Note1 + "</td>" +
                                          "<td>" + item.Note2 + "</td>" +
                                          "<td>" + item.Note3 + "</td></tr>");
                    }

                    stream.Write(@"</tbody></table>" + stream.NewLine + "</body>" + stream.NewLine + "</html>");
                }

                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjA3NTg3QDMxMzcyZTM0MmUzMEhhRDNnL1Z6TTBtTVR4VW5CaExZSkd0RVNDeVA3M2ZoYVoxaUEyakVWaGc9");

                HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();
                WebKitConverterSettings settings = new WebKitConverterSettings();

                settings.WebKitPath = "Libraries/QtBinariesDotNetCore";

                htmlConverter.ConverterSettings = settings;
                htmlConverter.ConverterSettings.Orientation = PdfPageOrientation.Landscape;
                htmlConverter.ConverterSettings.Margin.All = 10;

                using (PdfDocument document = htmlConverter.Convert("wwwroot/files/PDBCustomerViewReport" + guid + ".html"))
                {
                    using (FileStream fileStream = new FileStream("wwwroot/files/PDBCustomerViewDetails" + guid + ".pdf", FileMode.Create, FileAccess.ReadWrite))
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