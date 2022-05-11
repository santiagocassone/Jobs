using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OWPApplications.Data;
using OWPApplications.Models;
using OWPApplications.Models.JobCosting;
using OWPApplications.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspose.Cells;
using System.IO;

namespace OWPApplications.Controllers
{
    public class JobCostingController : Controller
    {
        DbHandler _db;
        IConfiguration _configuration;
        EmailHelper _emailHelper;
        ILogger _logger;

        public JobCostingController(DbHandler dbHandler, IConfiguration configuration, ILoggerFactory logFactory, EmailHelper emailHelper)
        {
            _db = dbHandler;
            _configuration = configuration;
            _logger = logFactory.CreateLogger(nameof(JobCostingController));
            _emailHelper = emailHelper;
        }

        public IActionResult Index()
        {
            ViewData["Customers"] = _db.JobCostingHandler.LoadCustomers();
            ViewData["Leads"] = _db.JobCostingHandler.LoadLeads();
            ViewData["Warehouses"] = _db.JobCostingHandler.LoadWarehouses();
            ViewData["SelectedCustomers"] = Array.Empty<string>();
            ViewData["SelectedLead"] = "";
            ViewData["SelectedWarehouse"] = "";
            ViewData["SelectedFromDate"] = null;
            ViewData["SelectedToDate"] = null;

            var vm = new JobCostingViewModel
            {
                Reports = new List<Report>()
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult Index(string fromDate, string toDate, string projectid, string orderno, string[] customerid, string warehoseid, string leadid)
        {
            ViewData["Customers"] = _db.JobCostingHandler.LoadCustomers();
            ViewData["Leads"] = _db.JobCostingHandler.LoadLeads();
            ViewData["Warehouses"] = _db.JobCostingHandler.LoadWarehouses();
            ViewData["SelectedCustomers"] = customerid;
            ViewData["SelectedLead"] = leadid;
            ViewData["SelectedWarehouse"] = warehoseid;
            ViewData["SelectedFromDate"] = fromDate;
            ViewData["SelectedToDate"] = toDate;
            ViewData["SelectedOrder"] = orderno;
            ViewData["SelectedProject"] = projectid;

            List<Report> rep = _db.JobCostingHandler.GetReport(fromDate, toDate, projectid, orderno, customerid, warehoseid, leadid);

            var vm = new JobCostingViewModel
            {
                Reports = rep,
                DateFrom = fromDate,
                DateTo = toDate
            };

            return View(vm);
        }

        public IActionResult ProjectDetails(string projectid, string orderdatefrom, string orderdateto)
        {
            ViewData["Customers"] = _db.JobCostingHandler.LoadCustomers();
            ViewData["Leads"] = _db.JobCostingHandler.LoadLeads();
            ViewData["Warehouses"] = _db.JobCostingHandler.LoadWarehouses();
            ViewData["SelectedCustomers"] = Array.Empty<string>();
            ViewData["SelectedLead"] = "";
            ViewData["SelectedWarehouse"] = "";
            ViewData["SelectedFromDate"] = null;
            ViewData["SelectedToDate"] = null;

            List<ProjectDetail> pd = _db.JobCostingHandler.GetProjectDetails(projectid);

            var vm = new JobCostingViewModel
            {

                ProjectDetails = pd,
                DateFrom = orderdatefrom,
                DateTo = orderdateto
            };

            return View("~/Views/JobCosting/Index.cshtml", vm);
        }

        public IActionResult OrderDetails(string orderindex, DateTime orderdatefrom, DateTime orderdateto)
        {
            ViewData["Customers"] = _db.JobCostingHandler.LoadCustomers();
            ViewData["Leads"] = _db.JobCostingHandler.LoadLeads();
            ViewData["Warehouses"] = _db.JobCostingHandler.LoadWarehouses();
            ViewData["SelectedCustomers"] = Array.Empty<string>();
            ViewData["SelectedLead"] = "";
            ViewData["SelectedWarehouse"] = "";
            ViewData["SelectedFromDate"] = null;
            ViewData["SelectedToDate"] = null;

            List<OrderDetail> pd = _db.JobCostingHandler.GetOrderDetails(orderindex, orderdatefrom, orderdateto);

            var vm = new JobCostingViewModel
            {
                OrderDetails = pd
            };

            return View("~/Views/JobCosting/Index.cshtml", vm);
        }

        [HttpPost]
        public IActionResult ExportExcel(JobCostingFilterData jobCostFilters)
        {
            try
            {
                List<Report> report = _db.JobCostingHandler.GetReport(jobCostFilters.FromDate, jobCostFilters.ToDate, jobCostFilters.ProjectId, jobCostFilters.OrderNo, jobCostFilters.CustomerId, jobCostFilters.WarehouseId, jobCostFilters.LeadId);

                if (report != null)
                {
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

                    cells[1, 1].Value = "Customer";
                    cells[1, 2].Value = "Lead";
                    cells[1, 3].Value = "PID";
                    cells[1, 4].Value = "Date Range(s)";
                    cells[1, 5].Value = "Labor Quotes";
                    cells[1, 6].Value = "Quote/Budget";
                    cells[1, 7].Value = "Additional Cost";
                    cells[1, 8].Value = "Vehicle Quote Breakout";
                    cells[1, 9].Value = "H&S Quote Breakout";
                    cells[1, 10].Value = "Change Order";
                    cells[1, 11].Value = "Budget Total";
                    cells[1, 12].Value = "Hourly Rate";
                    cells[1, 13].Value = "Labor Cost";
                    cells[1, 14].Value = "Vehicle Cost";
                    cells[1, 15].Value = "Additional Expenses";
                    cells[1, 16].Value = "Total Cost";
                    cells[1, 17].Value = "GPS";
                    cells[1, 18].Value = "GP %";
                    cells[1, 19].Value = "Order(s)";
                    cells[1, 20].Value = "Delivered";
                    cells[1, 21].Value = "Invoiced";
                    cells[1, 22].Value = "Additional Order(s)";
                    cells[1, 23].Value = "Additional Line(s)";
                    cells[1, 24].Value = "Delivered";
                    cells[1, 25].Value = "Invoiced";
                    cells[1, 26].Value = "Notes";
                    cells[1, 27].Value = "Billing Team Initials";
                    cells[1, 28].Value = "Billing Status";

                    int rowCount = 2;
                    string minDate = "";
                    string maxDate = "";
                    foreach (var item in report)
                    {
                        minDate = item.MinSchDate;
                        maxDate = item.MaxSchDate;
                        cells[rowCount, 1].Value = item.Customer;
                        cells[rowCount, 2].Value = item.Lead;
                        cells[rowCount, 3].Value = item.Project;
                        cells[rowCount, 4].Value = minDate + "-" + maxDate;
                        cells[rowCount, 5].Value = item.LaborQuoteNumbers;
                        cells[rowCount, 6].Value = item.QuoteBudget.ToString("N2");
                        cells[rowCount, 7].Value = item.AdditionalCost.ToString("N2");
                        cells[rowCount, 8].Value = item.VehicleQuoteBreakout.ToString("N2");
                        cells[rowCount, 9].Value = item.HSQuoteBreakout.ToString("N2");
                        cells[rowCount, 10].Value = item.ChangeOrder.ToString("N2");
                        cells[rowCount, 11].Value = item.BudgetTotal.ToString("N2");
                        cells[rowCount, 12].Value = "Reg Hs:" + item.RegHrsRate + "|OT Hs:" + item.OTHrsRate + "|DT Hs:" + item.DTHrsRate + "|PW Reg Hs:" + item.PWRegHrsRate + "|PW OT Hs:" + item.PWOTHrsRate + "|PW DT Hs:" + item.PWDTHrsRate;
                        cells[rowCount, 13].Value = item.LaborCost.ToString("N2");
                        cells[rowCount, 14].Value = item.VehicleCost.ToString("N2");
                        cells[rowCount, 15].Value = item.AdditionalExpenses.ToString("N2");
                        cells[rowCount, 16].Value = item.TotalCost.ToString("N2");
                        cells[rowCount, 17].Value = item.GPDollar.ToString("N2");
                        cells[rowCount, 18].Value = item.GPPct;
                        cells[rowCount, 19].Value = item.Order_Nos;
                        cells[rowCount, 20].Value = item.FullyDeliveredProject == 1 ? "X" : "";
                        cells[rowCount, 21].Value = item.FullyInvoicedProject == 1 ? "X" : "";
                        cells[rowCount, 22].Value = item.AdditionalOrders;
                        cells[rowCount, 23].Value = item.AdditionalLines;
                        cells[rowCount, 24].Value = item.Delivered;
                        cells[rowCount, 25].Value = item.Invoiced;
                        cells[rowCount, 26].Value = item.Notes;
                        cells[rowCount, 27].Value = item.BillingTeamInitials;
                        cells[rowCount, 28].Value = item.BillingStatus;

                        rowCount++;
                    }

                    workbook.Save("wwwroot/files/JobCostingReport.xls");

                    return Ok("success");
                }

                return Ok("empty");
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }

        }
    }
}
