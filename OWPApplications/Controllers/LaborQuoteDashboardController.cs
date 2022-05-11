using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OWPApplications.Data;
using OWPApplications.Models.LaborQuoteAutomation;

namespace OWPApplications.Controllers
{
	public class LaborQuoteDashboardController : Controller
	{
		private readonly DbHandler _db;

		public LaborQuoteDashboardController(DbHandler dbHandler)
		{
			_db = dbHandler;
		}

		public IActionResult Index(string dateFrom, string dateTo, string status, string customer, string code, string requestor, string projectId, string originalCode, string customerText, string region)
		{
			region = region ?? "OWP";
			var statusList = _db.LaborQuoteAutomationHandler.LoadLaborQuoteStatuses(region);
			var customerList = _db.LaborQuoteAutomationHandler.LoadCustomerList(region);
			
			ViewData["Region"] = region;
			ViewData["StatusList"] = statusList;
			ViewData["CustomerList"] = customerList;
			ViewData["DateFrom"] = dateFrom;
			ViewData["DateTo"] = dateTo;
			ViewData["Status"] = status;
			ViewData["CustomerText"] = customerText;
			ViewData["Code"] = code;
			ViewData["Requestor"] = requestor;
			ViewData["ProjectID"] = projectId;
			ViewData["OriginalLaborQuoteNo"] = originalCode;

			LaborQuoteAutomationViewModel vm = new LaborQuoteAutomationViewModel();

			if (dateFrom == null && dateTo == null && status == null && customer == null && code == null && requestor == null && projectId == null && originalCode == null && customerText == null)
			{
				vm.ShowResults = false;
			} else
            {
				vm.ShowResults = true;
				vm.LaborQuotes = _db.LaborQuoteAutomationHandler.GetLaborQuoteHeaders(dateFrom, dateTo, code, status, requestor, customer, projectId, originalCode, null, null, region);
			}	

			
			if (!string.IsNullOrEmpty(code))
			{
				ViewData["OriginalLaborQuoteNo"] = _db.LaborQuoteAutomationHandler.GetLaborQuoteHeaders(null, null, code, null, null, null, null, null, null, null, region).Select(x => x.OriginalLaborQuoteCode).FirstOrDefault();
			}

			return View(vm);
		}
	}
}