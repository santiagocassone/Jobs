using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OWPApplications.Data;
using OWPApplications.Models.LaborQuoteAutomation;
using OWPApplications.Models.LaborQuoteAutomationCommons;

namespace OWPApplications.Controllers
{
    public class EstimatorStatsReportController : Controller
    {
		DbHandler _db;

		public EstimatorStatsReportController(DbHandler dbHandler)
		{
			_db = dbHandler;
		}

		public IActionResult Index(List<string> estimators, string dateFrom, string dateTo, string type)
        {
			var estimatorList = _db.LaborQuoteAutomationHandler.LoadEstimatorList("ALL");

			ViewData["EstimatorList"] = estimatorList;
			ViewData["SelectedEstimators"] = estimators;
			ViewData["SRDateFrom"] = dateFrom;
			ViewData["SRDateTo"] = dateTo;
			ViewData["IsPostBack"] = false;

			LaborQuoteAutomationViewModel vm = new LaborQuoteAutomationViewModel();
			
			if (type == "search")
			{				
				ViewData["IsPostBack"] = true;
			}
			else
			{
				DateTime date = DateTime.Today;
				estimators = estimatorList.Select(x => x.ID).ToList();
				dateFrom = new DateTime(date.Year, date.Month, 1).ToString("MM/dd/yyyy");
				dateTo = DateTime.Now.ToString("MM/dd/yyyy");
			}

			vm.CompletedQuotesByMonth = _db.LaborQuoteAutomationHandler.GetCompletedQuotes("M", estimators, dateFrom, dateTo, "ALL") as List<CompletedQuotesByMonth>;
			vm.CompletedQuotesByServiceProviderOWP = _db.LaborQuoteAutomationHandler.GetCompletedQuotes("SP", estimators, dateFrom, dateTo, "OWP") as List<CompletedQuotesByServiceProvider>;
			vm.CompletedQuotesByServiceProviderOSQ = _db.LaborQuoteAutomationHandler.GetCompletedQuotes("SP", estimators, dateFrom, dateTo, "OSQ") as List<CompletedQuotesByServiceProvider>;
			vm.CompletedQuotesByCustomer = _db.LaborQuoteAutomationHandler.GetCompletedQuotes("C", estimators, dateFrom, dateTo, "ALL") as List<CompletedQuotesByCustomer>;
			vm.CompletedQuotesByRequestor = _db.LaborQuoteAutomationHandler.GetCompletedQuotes("R", estimators, dateFrom, dateTo, "ALL") as List<CompletedQuotesByRequestor>;
			vm.CompletedQuotesByEstimator = _db.LaborQuoteAutomationHandler.GetCompletedQuotes("AT", estimators, dateFrom, dateTo, "ALL") as List<CompletedQuotesByEstimator>;
			vm.RevisionReasons = _db.LaborQuoteAutomationHandler.GetCompletedQuotes("REV", estimators, dateFrom, dateTo, "ALL") as List<RevisionReasons>;

			return View(vm);
        }
    }
}