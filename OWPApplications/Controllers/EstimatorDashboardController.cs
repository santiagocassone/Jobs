using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OWPApplications.Data;
using OWPApplications.Models.LaborQuoteAutomation;
using OWPApplications.Models.LaborQuoteAutomationCommons;

namespace OWPApplications.Controllers
{
    public class EstimatorDashboardController : Controller
    {
		DbHandler _db;

		public EstimatorDashboardController(DbHandler dbHandler)
		{
			_db = dbHandler;
		}

		public IActionResult Index(string code, string originalCode, string customer, string requestedBy, string assignedTo, string projectId, string dateFrom, string dateTo, string quoOrdNo, string region)
        {
			region = region ?? "OWP";
			var customerList = _db.LaborQuoteAutomationHandler.LoadCustomerList(region);
			if (Request.Method != "POST")
            {
				dateFrom = dateFrom ?? DateTime.Now.AddMonths(-2).ToShortDateString();
				dateTo = dateTo ?? DateTime.Now.ToShortDateString();
			}			

			ViewData["CustomerList"] = customerList;
			ViewData["LaborQuoteNo"] = code;
			ViewData["OriginalLaborQuoteNo"] = originalCode;
			ViewData["Customer"] = customer;
			ViewData["RequestedBy"] = requestedBy;
			ViewData["AssignedTo"] = assignedTo;
			ViewData["ProjectID"] = projectId;
			ViewData["RequestedDateFrom"] = dateFrom;
			ViewData["RequestedDateTo"] = dateTo;
			ViewData["QuoteOrderNo"] = quoOrdNo;
			ViewData["Region"] = region;

			LaborQuoteAutomationViewModel vm = new LaborQuoteAutomationViewModel();
			vm.EstimatedByList = _db.LaborQuoteAutomationHandler.LoadEstimatorList(region);
			vm.EstimatorSummary = _db.LaborQuoteAutomationHandler.GetEstimatorSummary(region);
			var status = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("Status", region);
			vm.EstimatorPendingLaborQuotes = _db.LaborQuoteAutomationHandler.GetLaborQuoteHeaders(dateFrom, dateTo, code, null, requestedBy, customer, projectId, originalCode, assignedTo, quoOrdNo, region)
				.Where(x => x.Status == "Pending");
			vm.EstimatorCompletedLaborQuotes = _db.LaborQuoteAutomationHandler.GetLaborQuoteHeaders(dateFrom, dateTo, code, null, requestedBy, customer, projectId, originalCode, assignedTo, quoOrdNo, region)
				.Where(x => x.Status == "Complete" || x.Status == "Canceled");

			return View(vm);
        }

		[Route("/EstimatorDashboard/SetEstimator")]
		public void SetEstimator(string code, string estimatorName, string region)
		{
			region = region ?? "OWP";
			_db.LaborQuoteAutomationHandler.SetEstimatorFromDashboard(code, estimatorName, region);
		}
    }
}