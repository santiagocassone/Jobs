using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OWPApplications.Data;
using OWPApplications.Models.ISRManagerView;

namespace OWPApplications.Controllers
{
	public class ISRManagerViewController : Controller
	{
		DbHandler _db;

		public ISRManagerViewController(DbHandler dbHandler)
		{
			_db = dbHandler;
		}

		public IActionResult Index(string salesDirector, string salesSupportManager, string cuttOffDate)
		{
			var allSalesDirectors = _db.ISRManagerViewHandler.LoadSalesDirectors();
			var allSalesSupportManagers = _db.ISRManagerViewHandler.LoadSalesSupportManagers();

			ViewData["SalesDirectors"] = allSalesDirectors;
			ViewData["SalesSupportManagers"] = allSalesSupportManagers;
			ViewData["SalesDirector"] = salesDirector;
			ViewData["SalesSupportManager"] = salesSupportManager;
			ViewData["CutOffDate"] = cuttOffDate;

			ISRManagerViewViewModel vm = new ISRManagerViewViewModel
			{
				CutOffDate = cuttOffDate,
				SalespersonLink = !string.IsNullOrEmpty(salesDirector) ? "salesdirector_" + salesDirector : "salessupportmanager_" + salesSupportManager
			};

			if (!string.IsNullOrEmpty(salesDirector))
			{
				ViewData["SalespersonTitleGrid"] = "Sales Directors Dashboard";
				vm.SalespersonInfo = _db.ISRManagerViewHandler.GetSalespersonInfo(salesDirector, "", cuttOffDate ?? "").ToList();
			}
			else if (!string.IsNullOrEmpty(salesSupportManager))
			{
				ViewData["SalespersonTitleGrid"] = "Sales Support Managers Dashboard";
				vm.SalespersonInfo = _db.ISRManagerViewHandler.GetSalespersonInfo("", salesSupportManager, cuttOffDate ?? "").ToList();
			}

			vm.ShowDisclaimer = vm.SalespersonInfo?.Where(x => x.IsSplitted).Select(x => x).Count() > 0;

			return View(vm);
        }
    }
}