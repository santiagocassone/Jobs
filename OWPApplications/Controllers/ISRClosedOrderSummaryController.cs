using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OWPApplications.Data;
using OWPApplications.Models.InternalStatusReport;

namespace OWPApplications.Controllers
{
	public class ISRClosedOrderSummaryController : Controller
	{
		DbHandler _db;

		public ISRClosedOrderSummaryController(DbHandler dbHandler)
		{
			_db = dbHandler;
		}

		public IActionResult Index(string dateFrom, string dateTo, string orderNo, string projectId, List<string> customers, string csvSalesperson, string csvCustomers, string csvCuttOffDate, string csvSalespersonType, string csvRegion)
		{
			var allSalespersons = _db.InternalStatusReportHandler.LoadSalesPerson(csvRegion ?? "OWP");
			var salesperson = new List<string>();
			if (csvSalesperson != null && csvSalesperson != "")
			{
				salesperson = csvSalesperson.Split(",").ToList();
			}
			if (!string.IsNullOrEmpty(csvCustomers))
			{
				customers = csvCustomers.Split(",").ToList();
			}

			var allCustomers = _db.InternalStatusReportHandler.LoadCustomerList(salesperson, csvRegion);
			ViewData["Customers"] = allCustomers;
			ViewData["SalesPersons"] = allSalespersons;
			ViewData["DateFrom"] = dateFrom;
			ViewData["DateTo"] = csvCuttOffDate;
			ViewData["OrderNo"] = orderNo;
			ViewData["ProjectID"] = projectId;
			ViewData["SelectedCustomers"] = customers;
			ViewData["Salesperson"] = salesperson;
			ViewData["CutOffDate"] = csvCuttOffDate;
			ViewData["SPType"] = csvSalespersonType;
			ViewData["Region"] = csvRegion;

			ISRClosedOrderSummaryViewModel vm = new ISRClosedOrderSummaryViewModel();
			vm.ClosedOrders = _db.InternalStatusReportHandler.GetClosedOrderSummary(dateFrom, dateTo, orderNo, projectId, customers, salesperson, csvRegion);

			return View(vm);
		}
	}
}