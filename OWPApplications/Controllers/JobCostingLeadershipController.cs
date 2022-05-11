using Aspose.Cells;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OWPApplications.Data;
using OWPApplications.Models.JobCostingLeadership;
using OWPApplications.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Controllers
{
    public class JobCostingLeadershipController : Controller
    {
        DbHandler _db;
        IConfiguration _configuration;
        ILogger _logger;

        public JobCostingLeadershipController(DbHandler dbHandler, IConfiguration configuration, ILoggerFactory logFactory)
        {
            _db = dbHandler;
            _configuration = configuration;
            _logger = logFactory.CreateLogger(nameof(JobCostingLeadershipController));
        }

        public IActionResult Index()
        {
            ViewData["Customers"] = _db.JobCostingHandler.LoadCustomers();
            ViewData["SelectedCustomers"] = Array.Empty<string>();
            ViewData["SelectedFromDate"] = null;
            ViewData["SelectedToDate"] = null;
            ViewData["Region"] = "OWP";
            ViewData["SelectedProject"] = null;
            ViewData["SelectedLoc"] = "";
            ViewData["Locations"] = _db.JobCostingHandler.GetLocations();

            var vm = new JobCostingLeadershipViewModel
            {
                Summaries = new List<Summary>()
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult Index(string fromDate, string toDate, string projectid, string region, string[] customerid, string location)
        {
            
            ViewData["SelectedCustomers"] = customerid;
            ViewData["SelectedFromDate"] = fromDate ?? "";
            ViewData["SelectedToDate"] = toDate ?? "";
            ViewData["Region"] = region;
            ViewData["SelectedProject"] = projectid;
            ViewData["SelectedLoc"] = location ?? "";
            ViewData["Locations"] = _db.JobCostingHandler.GetLocations();

            List<Summary> rep = _db.JobCostingHandler.GetSummary(fromDate, toDate, projectid, region, customerid, location);

            var vm = new JobCostingLeadershipViewModel
            {
                Customers = _db.JobCostingHandler.LoadCustomers(),
                Summaries = rep
            };

            ViewData["Customers"] = vm.Customers;

            return View(vm);
        }

        public IActionResult GetDetails(string fromDate, string toDate, string projectid, string region, string[] customerid, string customer, string location)
        {
            ViewData["SelectedCustomers"] = customerid;
            ViewData["SelectedFromDate"] = fromDate ?? "";
            ViewData["SelectedToDate"] = toDate ?? "";
            ViewData["Region"] = region;
            ViewData["SelectedProject"] = projectid;
            ViewData["SelectedLoc"] = location ?? "";
            ViewData["Locations"] = _db.JobCostingHandler.GetLocations();

            List<Detail> pd = _db.JobCostingHandler.GetLeadershipProjectDetails(projectid, region, fromDate, toDate);

            var vm = new JobCostingLeadershipViewModel
            {
                Customers = _db.JobCostingHandler.LoadCustomers(),
                Details = pd,
                Customer = customer,
                ProjectId = projectid
            };

            ViewData["Customers"] = vm.Customers;

            return View("~/Views/JobCostingLeadership/Index.cshtml", vm);
        }

        [HttpPost]
        public IActionResult ExportExcel(string fromDate, string toDate, string projectid, string[] customers, string region, string type, string location)
        {
            try
            {
                List<Summary> summaryReport = new List<Summary>();
                List<Detail> detailsReport = new List<Detail>();
                if (type == "summary")
                {
                    summaryReport = _db.JobCostingHandler.GetSummary(fromDate, toDate, projectid, region, customers, location);
                } else
                {
                    detailsReport = _db.JobCostingHandler.GetLeadershipProjectDetails(projectid, region, fromDate, toDate);
                }

                // Create a License object 
                License license = new License();
                // Set the license of Aspose.Cells to avoid the evaluation limitations 
                license.SetLicense("Aspose.Cells.lic");

                Workbook workbook = new Workbook();
                workbook.Worksheets.Clear();
                Worksheet worksheet = workbook.Worksheets.Add("Report");
                Cells cells = worksheet.Cells;

                Style style = workbook.Styles[workbook.Styles.Add()];
                style.Font.IsBold = true;
                style.Font.Size = 10;
                StyleFlag styleFlag = new StyleFlag();
                styleFlag.All = true;

                Range headerRow = cells.CreateRange("A2", "AC2");
                headerRow.ApplyStyle(style, styleFlag);

                if (summaryReport.Count() > 0)
                {
                    cells[1, 1].Value = "Company";
                    cells[1, 2].Value = "Customer";
                    cells[1, 3].Value = "PID";
                    cells[1, 4].Value = "Scheduled Date(s)";
                    cells[1, 5].Value = "Labor Quote #";
                    cells[1, 6].Value = "Labor Quote $";
                    cells[1, 7].Value = "Budget $";
                    cells[1, 8].Value = "Actual $";
                    cells[1, 9].Value = "GP $";
                    cells[1, 10].Value = "GP %";
                    cells[1, 11].Value = "Open/Closed Labor Lines";

                    int rowCount = 2;
                    foreach (var item in summaryReport)
                    {
                        cells[rowCount, 1].Value = item.Company_Code;
                        cells[rowCount, 2].Value = item.Customer;
                        cells[rowCount, 3].Value = item.Project_ID;
                        cells[rowCount, 4].Value = item.MinSchDate + " - " + item.MaxSchDate;
                        cells[rowCount, 5].Value = item.FullLaborQuoteCode;
                        cells[rowCount, 6].Value = item.LaborQuoteCost.ToString("N2");
                        cells[rowCount, 7].Value = item.BudgetCost.ToString("N2");
                        cells[rowCount, 8].Value = item.ActualCost.ToString("N2");
                        cells[rowCount, 9].Value = item.GPCost.ToString("N2");
                        cells[rowCount, 10].Value = item.GPPct;
                        cells[rowCount, 11].Value = item.ProjectOpenLines;

                        rowCount++;
                    }

                    workbook.Save("wwwroot/files/JobCostingLeadershipSummaryReport.xls");
                } else if (detailsReport.Count() > 0)
                {
                    cells[1, 1].Value = "Order #";
                    cells[1, 2].Value = "Line #";
                    cells[1, 3].Value = "Lead";
                    cells[1, 4].Value = "Scheduled Date";
                    cells[1, 5].Value = "Labor Line Total";
                    cells[1, 6].Value = "Actual";
                    cells[1, 7].Value = "Delivered";
                    cells[1, 8].Value = "Invoiced";

                    int rowCount = 2;
                    foreach (var item in detailsReport)
                    {
                        cells[rowCount, 1].Value = item.Order_No;
                        cells[rowCount, 2].Value = item.Lines;
                        cells[rowCount, 3].Value = item.Lead;
                        cells[rowCount, 4].Value = item.SchDate;
                        cells[rowCount, 5].Value = item.LaborLineTotal.ToString("N2");
                        cells[rowCount, 6].Value = item.ActualCost.ToString("N2");
                        cells[rowCount, 7].Value = item.Delivered ? "X" : "";
                        cells[rowCount, 8].Value = item.Invoiced ? "X" : "";

                        rowCount++;
                    }

                    workbook.Save("wwwroot/files/JobCostingLeadershipDetailsReport.xls");
                } else {
                    return Ok("empty");
                }
                return Ok("success");
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }

        }
    }
}
