using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OWPApplications.Data;
using OWPApplications.Models;
using OWPApplications.Models.LaborQuoteAutomation;
using OWPApplications.Models.LaborQuoteAutomationCommons;
using OWPApplications.Utils;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;

namespace OWPApplications.Controllers
{
	public class EstimatorProjectTotalsController : Controller
	{
		DbHandler _db;
		IConfiguration _configuration;
		EmailHelper _emailHelper;

		public EstimatorProjectTotalsController(DbHandler dbHandler, IConfiguration configuration, EmailHelper emailHelper)
		{
			_db = dbHandler;
			_configuration = configuration;
			_emailHelper = emailHelper;
		}

		public IActionResult Index(string laborQuoteCode, string mode, string submit, string lqCode, bool fromDashboard,
			string paramlqno, string paramoriglqno, string paramcust, string paramreqby, string paramassignto,
			string paramprojectid, string paramdatefrom, string paramdateto, string paramstatus, string region)
		{
			region = region ?? "OWP";
			LaborQuoteAutomationViewModel vm = new LaborQuoteAutomationViewModel();
			vm.LaborQuoteStatuses = _db.LaborQuoteAutomationHandler.LoadLaborQuoteStatuses(region);
			vm.EstimatedByList = _db.LaborQuoteAutomationHandler.LoadEstimatorList(region);
			vm.HourTypes = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("LaborType", region);
			vm.HourTypesValues = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("LaborTypeValue", region);
			vm.Breakdown = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("Breakdown", region);
			vm.IsReadOnly = mode == "readonly";
			vm.FromDashboard = fromDashboard;
			ViewData["PDFReport"] = "n";
			ViewData["IsReadOnly"] = mode == "readonly";
			ViewData["ParamLaborQuoteNo"] = paramlqno;
			ViewData["ParamOriginalLaborQuoteNo"] = paramoriglqno;
			ViewData["ParamCustomer"] = paramcust;
			ViewData["ParamRequestedBy"] = paramreqby;
			ViewData["ParamAssignedTo"] = paramassignto;
			ViewData["ParamProjectID"] = paramprojectid;
			ViewData["ParamDateFrom"] = paramdatefrom;
			ViewData["ParamDateTo"] = paramdateto;
			ViewData["ParamStatus"] = paramstatus;
			ViewData["FromDashboard"] = fromDashboard;
			ViewData["DisableGeneratePDF"] = string.IsNullOrEmpty(laborQuoteCode) && string.IsNullOrEmpty(lqCode);
			ViewData["LaborQuoteCode"] = laborQuoteCode ?? lqCode;
			ViewData["Region"] = region;
			string lqStatus = "";

			//READONLY MODE
			if (!string.IsNullOrEmpty(laborQuoteCode))
			{
				vm.LaborQuote = _db.LaborQuoteAutomationHandler.GetLaborQuoteHeaders(null, null, laborQuoteCode, null, null, null, null, null, null, null, region)?.FirstOrDefault();
				vm.LaborQuote.HourAndRates = _db.LaborQuoteAutomationHandler.GetHourAndRates(region, vm.LaborQuote.LaborQuoteHeaderID);
				vm.LaborQuoteVendors = _db.LaborQuoteAutomationHandler.LoadLaborQuoteVendors(region);
				vm.LaborQuote.FurnitureInstallationList = _db.LaborQuoteAutomationHandler.GetFurnitureInstallations(vm.LaborQuote.LaborQuoteHeaderID, region)?.Where(x => x.Quantity > 0 && x.Hours > 0).Select(x => x);
				vm.LaborQuote.AddonsList = _db.LaborQuoteAutomationHandler.GetAddons(vm.LaborQuote.LaborQuoteHeaderID, region)?.Where(x => x.Quantity > 0 && x.Hours > 0).Select(x => x);
				vm.LaborQuote.MiscChargesList = _db.LaborQuoteAutomationHandler.GetMiscCharges(vm.LaborQuote.LaborQuoteHeaderID, region)?.Where(x => x.Cost > 0).Select(x => x);
				vm.LaborQuote.CustomFurnitureInstallationList = vm.LaborQuote.FurnitureInstallationList?.Where(x => !(new string[] { "SYS", "DAT", "SAM", "SEA" }).Contains(x.Description.Trim())).Select(x => x);
				vm.LaborQuote.CustomAddonsList = vm.LaborQuote.AddonsList?.Where(x => !(new string[] { "SCDAS", "WSCSD", "WSCSC", "GHCUT", "WSCUT", "DTIME", "WAREH" }).Contains(x.Description.Trim())).Select(x => x);
				vm.LaborQuote.CustomMiscChargesList = vm.LaborQuote.MiscChargesList?.Where(x => !(new string[] { "TR", "MP", "SP", "LC", "DP" }).Contains(x.Description.Trim())).Select(x => x);
				vm.ShowForm = true;
				lqStatus = vm.LaborQuote.Status.Trim();
				ViewData["LaborQuoteNo"] = laborQuoteCode;

				//ATTACHED FILES
				string codeName = vm.LaborQuote.LaborQuoteCode;
				DirectoryInfo di = new DirectoryInfo(@"wwwroot/files");
				FileInfo[] fis = di.GetFiles(codeName + "_att_*.*");
				if (fis?.Count() > 0)
				{
					ViewData["IsAttached"] = true;
					vm.LaborQuote.AttachedFiles = new List<AttachedFile>();
					foreach (var f in fis)
					{
						vm.LaborQuote.AttachedFiles.Add(new AttachedFile() { FullPath = f.FullName, FileName = f.Name.Replace(codeName + "_att_", ""), FullFileName = f.Name });
					}
				}
			}

			//SEARCH BY CODE TO ESTIMATE/EDIT
			if ((Request.Method == "POST" && submit == "Search") || !string.IsNullOrEmpty(lqCode))
			{
				vm.LaborQuote = _db.LaborQuoteAutomationHandler.GetLaborQuoteHeaders(null, null, lqCode, null, null, null, null, null, null, null, region).FirstOrDefault();
				if (vm.LaborQuote != null)
				{
					vm.LaborQuote.HourAndRates = _db.LaborQuoteAutomationHandler.GetHourAndRates(region, vm.LaborQuote.LaborQuoteHeaderID);
					vm.LaborQuoteVendors = _db.LaborQuoteAutomationHandler.LoadLaborQuoteVendors(region);
					vm.LaborQuote.FurnitureInstallationList = _db.LaborQuoteAutomationHandler.GetFurnitureInstallations(vm.LaborQuote.LaborQuoteHeaderID, region)?.Where(x => x.Quantity > 0 && x.Hours > 0).Select(x => x);
					vm.LaborQuote.AddonsList = _db.LaborQuoteAutomationHandler.GetAddons(vm.LaborQuote.LaborQuoteHeaderID, region)?.Where(x => x.Quantity > 0 && x.Hours > 0).Select(x => x);
					vm.LaborQuote.MiscChargesList = _db.LaborQuoteAutomationHandler.GetMiscCharges(vm.LaborQuote.LaborQuoteHeaderID, region)?.Where(x => x.Cost > 0).Select(x => x);
					vm.LaborQuote.CustomFurnitureInstallationList = vm.LaborQuote.FurnitureInstallationList?.Where(x => !(new string[] { "SYS", "DAT", "SAM", "SEA" }).Contains(x.Description.Trim())).Select(x => x);
					vm.LaborQuote.CustomAddonsList = vm.LaborQuote.AddonsList?.Where(x => !(new string[] { "SCDAS", "WSCSD", "WSCSC", "GHCUT", "WSCUT", "DTIME", "WAREH" }).Contains(x.Description.Trim())).Select(x => x);
					vm.LaborQuote.CustomMiscChargesList = vm.LaborQuote.MiscChargesList?.Where(x => !(new string[] { "TR", "MP", "SP", "LC", "DP" }).Contains(x.Description.Trim())).Select(x => x);
					vm.ShowForm = true;
					lqStatus = vm.LaborQuote.Status.Trim();
					ViewData["LaborQuoteNo"] = lqCode;

					//ATTACHED FILES
					string codeName = vm.LaborQuote.LaborQuoteCode;
					DirectoryInfo di = new DirectoryInfo(@"wwwroot/files");
					FileInfo[] fis = di.GetFiles(codeName + "_att_*.*");
					if (fis?.Count() > 0)
					{
						ViewData["IsAttached"] = true;
						vm.LaborQuote.AttachedFiles = new List<AttachedFile>();
						foreach (var f in fis)
						{
							vm.LaborQuote.AttachedFiles.Add(new AttachedFile() { FullPath = f.FullName, FileName = f.Name.Replace(codeName + "_att_", ""), FullFileName = f.Name });
						}
					}

					if (!(Request.Method == "POST" && submit == "Search"))
					{
						ViewData["FromURL"] = true;
					}
				}
				else
				{
					vm.ShowForm = false;
					vm.NoLaborQuoteFound = true;
				}
			}

			vm.CurrentStatus = lqStatus;

			return View(vm);
		}

		[HttpGet]
		public IActionResult PDFReport(string quoteno)
		{
			var region = "OWP";
			LaborQuoteAutomationViewModel vm = new LaborQuoteAutomationViewModel();
			vm.LaborQuoteStatuses = _db.LaborQuoteAutomationHandler.LoadLaborQuoteStatuses(region);
			vm.EstimatedByList = _db.LaborQuoteAutomationHandler.LoadEstimatorList(region);
			vm.HourTypes = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("LaborType", region);
			vm.HourTypesValues = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("LaborTypeValue", region);
			vm.Breakdown = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("Breakdown", region);
			vm.IsReadOnly = false;
			ViewData["IsReadOnly"] = false;
			ViewData["FromURL"] = false;
			ViewData["ParamLaborQuoteNo"] = quoteno;
			ViewData["ParamOriginalLaborQuoteNo"] = quoteno;
			ViewData["ParamCustomer"] = "";
			ViewData["ParamRequestedBy"] = "";
			ViewData["ParamAssignedTo"] = "";
			ViewData["ParamProjectID"] = "";
			ViewData["ParamDateFrom"] = "";
			ViewData["ParamDateTo"] = "";
			ViewData["ParamStatus"] = "";
			ViewData["FromDashboard"] = false;
			ViewData["PDFReport"] = "y";
			ViewData["LaborQuoteCode"] = quoteno;
			ViewData["Region"] = region;
			string lqStatus = "";
			vm.LaborQuote = _db.LaborQuoteAutomationHandler.GetLaborQuoteHeaders(null, null, quoteno, null, null, null, null, null, null, null, region)?.FirstOrDefault();
			vm.LaborQuote.HourAndRates = _db.LaborQuoteAutomationHandler.GetHourAndRates(region, vm.LaborQuote.LaborQuoteHeaderID);
			vm.LaborQuoteVendors = _db.LaborQuoteAutomationHandler.LoadLaborQuoteVendors(region); 
			vm.LaborQuote.FurnitureInstallationList = _db.LaborQuoteAutomationHandler.GetFurnitureInstallations(vm.LaborQuote.LaborQuoteHeaderID, region)?.Where(x => x.Quantity > 0 && x.Hours > 0).Select(x => x);
			vm.LaborQuote.AddonsList = _db.LaborQuoteAutomationHandler.GetAddons(vm.LaborQuote.LaborQuoteHeaderID, region)?.Where(x => x.Quantity > 0 && x.Hours > 0).Select(x => x);
			vm.LaborQuote.MiscChargesList = _db.LaborQuoteAutomationHandler.GetMiscCharges(vm.LaborQuote.LaborQuoteHeaderID, region)?.Where(x => x.Cost > 0).Select(x => x);
			vm.LaborQuote.CustomFurnitureInstallationList = vm.LaborQuote.FurnitureInstallationList?.Where(x => !(new string[] { "SYS", "DAT", "SAM", "SEA" }).Contains(x.Description.Trim())).Select(x => x);
			vm.LaborQuote.CustomAddonsList = vm.LaborQuote.AddonsList?.Where(x => !(new string[] { "SCDAS", "WSCSD", "WSCSC", "GHCUT", "WSCUT", "DTIME", "WAREH" }).Contains(x.Description.Trim())).Select(x => x);
			vm.LaborQuote.CustomMiscChargesList = vm.LaborQuote.MiscChargesList?.Where(x => !(new string[] { "TR", "MP", "SP", "LC", "DP" }).Contains(x.Description.Trim())).Select(x => x);
			vm.ShowForm = true;
			lqStatus = vm.LaborQuote.Status.Trim();

			//ATTACHED FILES
			string codeName = vm.LaborQuote.LaborQuoteCode;
			DirectoryInfo di = new DirectoryInfo(@"wwwroot/files");
			FileInfo[] fis = di.GetFiles(codeName + "_att_*.*");
			if (fis?.Count() > 0)
			{
				ViewData["IsAttached"] = true;
				vm.LaborQuote.AttachedFiles = new List<AttachedFile>();
				foreach (var f in fis)
				{
					vm.LaborQuote.AttachedFiles.Add(new AttachedFile() { FullPath = f.FullName, FileName = f.Name.Replace(codeName + "_att_", ""), FullFileName = f.Name });
				}
			}

			vm.CurrentStatus = lqStatus;

			return View("Index", vm);
		}

		[HttpPost]
		public ActionResult EstProjTotPDF([FromBody] QuoteNo quote)
		{
			try
			{
				var region = quote.region;
				removeOldFiles();
				LaborQuoteAutomationViewModel vm = new LaborQuoteAutomationViewModel();
				vm.LaborQuote = _db.LaborQuoteAutomationHandler.GetLaborQuoteHeaders(null, null, quote.quoteno, null, null, null, null, null, null, null, region)?.FirstOrDefault();
				vm.LaborQuoteVendors = _db.LaborQuoteAutomationHandler.LoadLaborQuoteVendors(region);
				var furnitureInstallationList = _db.LaborQuoteAutomationHandler.GetFurnitureInstallations(vm.LaborQuote.LaborQuoteHeaderID, region)?.Where(x => x.Quantity > 0 && x.Hours > 0).Select(x => x).Distinct();
				vm.LaborQuote.FurnitureInstallationList = furnitureInstallationList?.Where(x => (new string[] { "SYS", "DAT", "SAM", "SEA" }).Contains(x.Description.Trim())).Select(x => x).Distinct();
				vm.LaborQuote.CustomFurnitureInstallationList = furnitureInstallationList?.Where(x => !(new string[] { "SYS", "DAT", "SAM", "SEA" }).Contains(x.Description.Trim())).Select(x => x).Distinct();
				var addonsList = _db.LaborQuoteAutomationHandler.GetAddons(vm.LaborQuote.LaborQuoteHeaderID, region)?.Where(x => x.Quantity > 0 && x.Hours > 0).Select(x => x).Distinct();
				vm.LaborQuote.AddonsList = addonsList?.Where(x => (new string[] { "SCDAS", "WSCSD", "WSCSC", "GHCUT", "WSCUT", "DTIME", "WAREH" }).Contains(x.Description.Trim())).Select(x => x).Distinct();
				vm.LaborQuote.CustomAddonsList = addonsList?.Where(x => !(new string[] { "SCDAS", "WSCSD", "WSCSC", "GHCUT", "WSCUT", "DTIME", "WAREH" }).Contains(x.Description.Trim())).Select(x => x);
				var miscChargesList = _db.LaborQuoteAutomationHandler.GetMiscCharges(vm.LaborQuote.LaborQuoteHeaderID, region)?.Where(x => x.Cost > 0).Select(x => x);
				vm.LaborQuote.MiscChargesList = miscChargesList?.Where(x => (new string[] { "TR", "MP", "SP", "LC", "DP" }).Contains(x.Description.Trim())).Select(x => x).Distinct();
				vm.LaborQuote.CustomMiscChargesList = miscChargesList?.Where(x => !(new string[] { "TR", "MP", "SP", "LC", "DP" }).Contains(x.Description.Trim())).Select(x => x);
				vm.ShowForm = true;
				string lqStatus = "";
				lqStatus = vm.LaborQuote.Status.Trim();
				string guid = Guid.NewGuid().ToString();

				using (StreamWriter stream = new StreamWriter("wwwroot/files/PDFEstProjTot" + guid + ".html", false, System.Text.Encoding.UTF8))
				{
					if (Convert.ToBoolean(quote.min))
					{
						stream.Write(@"
					<html>" + stream.NewLine + @"
					<head>" + stream.NewLine + @"
					<style>
						body { background-color: whitesmoke; font-family: Arial; }
					</style>
					</head>" + stream.NewLine + @"
					<body>" + stream.NewLine + @"
					<h2 style='margin-top:15px;margin-left:15px'>Estimator Project Totals</h2>			
					<hr />
					<div style='margin-top:5px; padding: 5px;'>
						<h4 class='ml-4'>Project Summary</h4>	
						<hr style='margin-left:0;width:25%'/>
						<br /><br />
						<table style='margin-left:auto;margin-right:auto;'>	 
							<tr>
								<td width='200px'><p>Project ID: </p></td>
								<td><p>" + vm.LaborQuote.ProjectID + @"</p></td>
							</tr>	
							<tr>
								<td width='200px'><p>Status: </p></td>
								<td><p>" + vm.LaborQuote.Status + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Labor Quote: </p></td>
								<td><p>" + vm.LaborQuote.FullLaborQuoteCode + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Quote #: </p></td>
								<td><p>" + vm.LaborQuote.QuoteOrOrderNumber + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Requested Date: </p></td>
								<td><p>" + vm.LaborQuote.CreatedOn.ToString("MM/dd/yyyy") + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Original Labor Quote: </p></td>
								<td><p>" + vm.LaborQuote.OriginalLaborQuoteCode + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Customer Name: </p></td>
								<td><p>" + vm.LaborQuote.CustomerName + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Address: </p></td>
								<td><p>" + vm.LaborQuote.AddressLine1 + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>City: </p></td>
								<td><p>" + vm.LaborQuote.AddressCity + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>State: </p></td>
								<td><p></p></td>
							</tr>
							<tr>
								<td width='200px'><p>Zip: </p></td>
								<td><p></p></td>
							</tr>
						</table>
					</div>				
					<hr />
					<div style='margin-top:5px; padding: 5px;'>
						<h4 class='ml-4'>Scope</h4>
						<hr style='margin-left:0;width:25%'/>
						<br /><br />
						<pre style='padding-left: 25%;font-family: Arial;white-space: pre-wrap;max-width: 630px'>" + vm.LaborQuote.Scope + @"</pre>
						<table style='margin-left:auto;margin-right:auto'>
							<tr>
								<td width='200px'>
									<p>Equipments/Tools: </p>
									<p>Vendor #: </p>
									<p>Select Tax: </p>
								</td>
								<td>
									<p>" + vm.LaborQuote.EquipmentTools + @"</p>
									<p>" + vm.LaborQuoteVendors.Where(x => x.LookupGeneralID == vm.LaborQuote.VendorNumberLookupID).Select(x => x.Value).FirstOrDefault() + @"</p>
									<p>" + (vm.LaborQuote.Status == "Pending" ? "Taxable" : (vm.LaborQuote.Taxable ? "Taxable" : "NonTaxable")) + @"</p>
								</td>
							</tr>
						</table>
					</div>
					<hr />
					<div style='margin-top:5px; padding: 5px;'>
						<h4 class='ml-4'>Furniture Installation (quantities)</h4>
						<hr style='margin-left:0;width:25%'/>
						<br /><br />
						<table style='margin-left:auto;margin-right:auto'> 
				");

						var fiList = vm.LaborQuote.FurnitureInstallationList?.ToList();
						if (fiList?.Count() > 0)
						{
							foreach (var fi in fiList)
							{
								string description = "";
								switch (fi.Description)
								{
									case "CON": description = "Context"; break;
									case "SYS": description = "System"; break;
									case "DAT": description = "Casegoods: Desks and Tables"; break;
									case "SAM": description = "Casegoods: Storage and Misc"; break;
									case "SEA": description = "Seating"; break;
								}
								stream.Write(@"
							<tr>
								<td width='400px'><p>" + description + @"</p></td>
								<td width='150px'><p>" + (fi.Quantity == 0 ? "" : fi.Quantity.ToString()) + @"</p></td>
							</tr>
					");
							}
						}
						var cfiList = vm.LaborQuote.CustomFurnitureInstallationList?.ToList();
						if (cfiList?.Count() > 0)
						{
							foreach (var cfi in cfiList)
							{
								stream.Write(@"
							<tr>
								<td width='400px'><p>" + cfi.Description + @"</p></td>
								<td width='150px'><p>" + cfi.Quantity + @"</p></td>
							</tr>
						");
							}
						}

						stream.Write(@"
					</table>
					</div>
					<hr />
					<div style='margin-top:5px; padding: 5px;'>
						<h4 class='ml-4'>Labor Add Ons (quantities)</h4>
						<hr style='margin-left:0;width:25%'/>
						<br /><br />
						<table style='margin-left:auto;margin-right:auto'> 
				");

						var aoList = vm.LaborQuote.AddonsList?.ToList();
						if (aoList?.Count() > 0)
						{
							foreach (var ao in aoList)
							{
								string description = "";
								switch (ao.Description)
								{
									case "SCDAS": description = "Stair Carry/Distance/Add'l Staging"; break;
									case "WSCSD": description = "Wall Strip/Channel/Start Drywall"; break;
									case "WSCSC": description = "Wall Strip/Channel/Start Concrete"; break;
									case "GHCUT": description = "Grommet Hole Cuts"; break;
									case "WSCUT": description = "Worksurface Cuts"; break;
									case "DTIME": description = "Delivery Time"; break;
									case "WAREH": description = "Warehouse"; break;
									default: description = ao.Description; break;
								}
								stream.Write(@"
							<tr>
								<td width='400px'><p>" + description + @"</p></td>
								<td width='150px'><p>" + (ao.Quantity == 0 ? "" : ao.Quantity.ToString()) + @"</p></td>
							</tr>
					");
							}
						}
						var caoList = vm.LaborQuote.CustomAddonsList?.ToList();
						if (caoList?.Count() > 0)
						{
							foreach (var cao in caoList)
							{
								stream.Write(@"
							<tr>
								<td width='400px'><p>" + cao.Description + @"</p></td>
								<td width='150px'><p>" + cao.Quantity + @"</p></td>
							</tr>
						");
							}
						}

						var estimatorName = vm.LaborQuote.EstimatorName.Trim();
						var numberOfDays = (vm.LaborQuote.NumberOfDays ?? 0).ToString();
						var createdOn = vm.LaborQuote.CreatedOn.ToString("MM/dd/yyyy");
						var laborTotal = vm.LaborQuote.ComplexProject ? "See scope above for Breakdown" : (vm.LaborQuote.InstallTotal + vm.LaborQuote.HealthAndSafetySurcharge).ToString("C");
						var expirationDate = vm.LaborQuote.ExpirationDate == null ? "" : ((DateTime)vm.LaborQuote.ExpirationDate).ToString("MM/dd/yyyy");

						stream.Write(@"
					</table>
					</div>
					<hr />
					<div style='margin-top:5px;padding:5px;'>
						<br /><br />
						<table style='margin-left:auto;margin-right:auto'>	 
						<tr>
							<td width='200px'>
								<p>Estimated By:</p>
								<p>This project is quoted for </p>
								<p>Date:</p>
								<p>Labor Total:</p>
								<p>Expiration Date:</p>
							</td>
							<td>
								<p>" + estimatorName + @"</p>
								<p>" + numberOfDays + @" day(s).</p>
								<p>" + createdOn + @"</p>
								<p>" + laborTotal + @"</p>
								<p>" + expirationDate + @"</p>
							</td>
						</tr>
						</table>
					</div>
				");

						stream.Write(stream.NewLine + "</body>" + stream.NewLine + "</html>");
					}
					else
					{
						stream.Write(@"
					<html>" + stream.NewLine + @"
					<head>" + stream.NewLine + @"
					<style>
						body { background-color: whitesmoke; font-family: Arial; }
					</style>
					</head>" + stream.NewLine + @"
					<body>" + stream.NewLine + @"
					<h2 style='margin-top:15px;margin-left:15px'>Estimator Project Totals</h2>			
					<hr />
					<div style='padding-left: 5px;padding-bottom: 20px'>
						<h4 class='ml-4'>Project Summary</h4>	
						<hr style='margin-left:0;width:25%'/>
						<table style='margin-left:auto;margin-right:auto;'>	 
							<tr>
								<td width='200px'><p>Project ID: </p></td>
								<td><p>" + vm.LaborQuote.ProjectID + @"</p></td>
							</tr>	
							<tr>
								<td width='200px'><p>Status: </p></td>
								<td><p>" + vm.LaborQuote.Status + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Labor Quote: </p></td>
								<td><p>" + vm.LaborQuote.FullLaborQuoteCode + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Quote #: </p></td>
								<td><p>" + vm.LaborQuote.QuoteOrOrderNumber + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Requested Date: </p></td>
								<td><p>" + vm.LaborQuote.CreatedOn.ToString("MM/dd/yyyy") + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Original Labor Quote: </p></td>
								<td><p>" + vm.LaborQuote.OriginalLaborQuoteCode + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Customer Name: </p></td>
								<td><p>" + vm.LaborQuote.CustomerName + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Address: </p></td>
								<td><p>" + vm.LaborQuote.AddressLine1 + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>City: </p></td>
								<td><p>" + vm.LaborQuote.AddressCity + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>State: </p></td>
								<td><p></p></td>
							</tr>
							<tr>
								<td width='200px'><p>Zip: </p></td>
								<td><p></p></td>
							</tr>
						</table>
					</div>				
					<hr />
					<div style='padding-left: 5px;padding-bottom: 20px'>
						<h4 class='ml-4'>Scope</h4>
						<hr style='margin-left:0;width:25%'/>
						<pre style='padding-left: 25%;font-family: Arial;white-space: pre-wrap;max-width: 630px'>" + vm.LaborQuote.Scope + @"</pre>
						<table style='margin-left:auto;margin-right:auto'>
							<tr>
								<td width='200px'>
									<p>Equipments/Tools: </p>
									<p>Vendor #: </p>
									<p>Select Tax: </p>
								</td>
								<td>
									<p>" + vm.LaborQuote.EquipmentTools + @"</p>
									<p>" + vm.LaborQuoteVendors.Where(x => x.LookupGeneralID == vm.LaborQuote.VendorNumberLookupID).Select(x => x.Value).FirstOrDefault() + @"</p>
									<p>" + (vm.LaborQuote.Status == "Pending" ? "Taxable" : (vm.LaborQuote.Taxable ? "Taxable" : "NonTaxable")) + @"</p>
								</td>
							</tr>
						</table>
					</div>
					<hr />
					<div style='padding-left: 5px;padding-bottom: 20px'>
						<h4 class='ml-4'>Furniture Installation</h4>
						<hr style='margin-left:0;width:25%'/>
						<table style='margin-left:auto;margin-right:auto'> ");

						stream.Write(@"
							<tr> 
								<th width='400px'></th>  
								<th width='150px'>Qty.</th>
								<th width='150px'>Hours per Qty.</th>
								<th width='150px'>Hours</th>
							</tr>");

						var fiList = vm.LaborQuote.FurnitureInstallationList?.ToList();
						double fiTot = 0;
						if (fiList?.Count() > 0)
						{
							foreach (var fi in fiList)
							{
								string description = "";
								fiTot += fi.Hours;
								switch (fi.Description)
								{
									case "CON": description = "Context"; break;
									case "SYS": description = "System"; break;
									case "DAT": description = "Casegoods: Desks and Tables"; break;
									case "SAM": description = "Casegoods: Storage and Misc"; break;
									case "SEA": description = "Seating"; break;
								}
								stream.Write(@"
							<tr>
								<td width='400px'>" + description + @"</td>
								<td style='text-align:center' width='150px'>" + (fi.Quantity == 0 ? "" : fi.Quantity.ToString()) + @"</td>
								<td style='text-align:center' width='150px'>" + (fi.HoursPerQty == 0 ? "" : fi.HoursPerQty.ToString()) + @"</td>
								<td style='text-align:center' width='150px'>" + (fi.Hours == 0 ? "" : fi.Hours.ToString()) + @"</td>
							</tr>");
							}
						}
						var cfiList = vm.LaborQuote.CustomFurnitureInstallationList?.ToList();
						if (cfiList?.Count() > 0)
						{
							foreach (var cfi in cfiList)
							{
								fiTot += cfi.Hours;
								stream.Write(@"
							<tr>
								<td width='400px'>" + cfi.Description + @"</td>
								<td style='text-align:center' width='150px'>" + (cfi.Quantity == 0 ? "" : cfi.Quantity.ToString()) + @"</td>
								<td style='text-align:center' width='150px'>" + (cfi.HoursPerQty == 0 ? "" : cfi.HoursPerQty.ToString()) + @"</td>
								<td style='text-align:center' width='150px'>" + (cfi.Hours == 0 ? "" : cfi.Hours.ToString()) + @"</td>
							</tr>");
							}
						}

						stream.Write(@"
						<tr> 
							<td width='400px' colspan='2'></td>					  
							<td style='text-align:center' width='150px'><center>Total Hours:</center></td>								
							<td style='text-align:center' width='150px'>" + fiTot + "</td>");
						stream.Write(@"
						</tr>										  
					</table>										  
					</div>
					<hr />
					<div style='padding-left: 5px;padding-bottom: 20px'>
					<h4 class='ml-4'>Labor Add Ons</h4>
					<hr style='margin-left:0;width:25%'/>
					");

						stream.WriteLine(@"
					<table style='margin-left:auto;margin-right:auto'> 
						<tr> 
							<th width='400px'></th>  
							<th width='150px'>Qty.</th>
                            <th width='150px'>Hours</th>
                        </tr>");

						var aoList = vm.LaborQuote.AddonsList?.ToList();
						double aoTot = 0;
						if (aoList?.Count() > 0)
						{
							foreach (var ao in aoList)
							{
								string description = "";
								aoTot += ao.Hours;
								switch (ao.Description)
								{
									case "SCDAS": description = "Stair Carry/Distance/Add'l Staging"; break;
									case "WSCSD": description = "Wall Strip/Channel/Start Drywall"; break;
									case "WSCSC": description = "Wall Strip/Channel/Start Concrete"; break;
									case "GHCUT": description = "Grommet Hole Cuts"; break;
									case "WSCUT": description = "Worksurface Cuts"; break;
									case "DTIME": description = "Delivery Time"; break;
									case "WAREH": description = "Warehouse"; break;
									default: description = ao.Description; break;
								}
								stream.Write(@"
							<tr>
								<td width='400px'>" + description + @"</td>
								<td style='text-align:center' width='150px'>" + (ao.Quantity == 0 ? "" : ao.Quantity.ToString()) + @"</td>
								<td style='text-align:center' width='150px'>" + (ao.Hours == 0 ? "" : ao.Hours.ToString()) + @"</td>
							</tr>
					");
							}
						}
						var caoList = vm.LaborQuote.CustomAddonsList?.ToList();
						if (caoList?.Count() > 0)
						{
							foreach (var cao in caoList)
							{
								aoTot += cao.Hours;
								stream.Write(@"
							<tr>
								<td width='400px'>" + cao.Description + @"</td>
								<td style='text-align:center' width='150px'>" + (cao.Quantity == 0 ? "" : cao.Quantity.ToString()) + @"</td>
								<td style='text-align:center' width='150px'>" + (cao.Hours == 0 ? "" : cao.Hours.ToString()) + @"</td>
							</tr>
						");
							}
						}

						stream.WriteLine(@"
                        <tr>
                            <td width='400px'></td>
							<td style='text-align:center' width='150px'><center>Total Hours: </center></td>
                            <td style='text-align:center' width='150px'>" + aoTot + "</td>"); stream.Write(@"
                        </tr>
                    </table>
				</div>	
				<hr />
				<div style='padding-left:5px;padding-bottom: 20px'>
                    <h4 class='ml-4'>Misc Installation Charges</h4>
					<hr style='margin-left:0;width:25%'/>
					");

						stream.Write(@"
                            <table style='margin-left:auto;margin-right:auto'>
								<tr>
									<th width='400px'></th>
                                    <th width='150px'>Qty.</th>
                                    <th width='150px'>Rate</th>
                                    <th width='150px'>Cost($)</th>
                                </tr>");

						var mcList = vm.LaborQuote.MiscChargesList?.ToList();
						double mcTot = 0;
						if (mcList?.Count() > 0)
						{
							foreach (var mc in mcList)
							{
								string description = "";
								mcTot += mc.Cost;
								switch (mc.Description)
								{
									case "TR": description = "Truck"; break;
									case "MP": description = "Mileage / Parking"; break;
									case "SP": description = "Supplies"; break;
									case "LC": description = "Load Crew"; break;
									case "DP": description = "Disposal"; break;
								}
								stream.Write(@"
							<tr>
								<td width='400px'>" + description + @"</td>
								<td style='text-align:center' width='150px'>" + (mc.Quantity == 0 ? "" : mc.Quantity.ToString()) + @"</td>
								<td style='text-align:center' width='150px'>" + (mc.Rate == 0 ? "" : mc.Rate.ToString()) + @"</td>
								<td style='text-align:center' width='150px'>" + mc.Cost + @"</td>
							</tr>");
							}
						}
						var cmcList = vm.LaborQuote.CustomMiscChargesList?.ToList();
						if (cmcList?.Count() > 0)
						{
							foreach (var cmc in cmcList)
							{
								mcTot += cmc.Cost;
								stream.Write(@"
							<tr>
								<td width='400px'>" + cmc.Description + @"</td>
								<td style='text-align:center' width='150px'>" + (cmc.Quantity == 0 ? "" : cmc.Quantity.ToString()) + @"</td>
								<td style='text-align:center' width='150px'>" + (cmc.Rate == 0 ? "" : cmc.Rate.ToString()) + @"</td>
								<td style='text-align:center' width='150px'>" + cmc.Cost + @"</td>
							</tr>");
							}
						}

						stream.WriteLine(@"
                                <tr>
                                    <td width='400px'></td>
									<td style='text-align:center' width='150px'></td>
									<td style='text-align:center' width='150px'>Total: </td>
                                    <td style='text-align:center' width='150px'>$" + mcTot + "</td>"); stream.Write(@"
                                </tr>
                            </table>
                        </div>
                        <hr />
                        <div style='padding-left:5px;padding-bottom: 20px'>
                            <h4 class='ml-4'>Notes</h4>
							<hr style='margin-left:0;width:25%'/>
                            <table style='margin-left:auto;margin-right:auto'>
								<tr>
									<td><pre style='padding-left: 25%;font-family: Arial;white-space: pre-wrap;max-width: 820px'>" + vm.LaborQuote.Notes + "</pre></td>"); stream.Write(@"
                                </tr>                                
							</table>
						</div>
			<hr />
			<div style='padding-top: 10px; padding-left: 5px;padding-bottom: 20px'>
				<table style='margin-left:auto;margin-right:auto'>	 
					<tr>	 
						<td colspan='5'></td>	  
						<td style='width:85px;'></td>	   
						<td style='width:85px;'>Surcharge</td>");
						float surchargpct = vm.LaborQuote.SurchargePct == null ? 6 : float.Parse(vm.LaborQuote.SurchargePct);
						float surchargeReg = vm.LaborQuote.RegularHsRate == null ? 49 : float.Parse(vm.LaborQuote.RegularHsRate);
						float surchargeOt = vm.LaborQuote.OTHsRate == null ? 147 / 2 : float.Parse(vm.LaborQuote.OTHsRate);
						float surchargeDt = vm.LaborQuote.DTHsRate == null ? 98 : float.Parse(vm.LaborQuote.DTHsRate);
						float dthrs = vm.LaborQuote.DTHours == null || vm.LaborQuote.DTHours == "0" ? 0 : float.Parse(vm.LaborQuote.DTHours);
						float othrs = vm.LaborQuote.OTHours == null || vm.LaborQuote.OTHours == "0" ? 0 : float.Parse(vm.LaborQuote.OTHours);
						float reghrs = vm.LaborQuote.RegularHours == null || vm.LaborQuote.RegularHours == "0" ? 0 : float.Parse(vm.LaborQuote.RegularHours);
						float totlabcost = (surchargeReg * reghrs) + (surchargeOt * othrs) + (surchargeDt * dthrs);
						stream.Write(@"<td style='width:85px;'>" + (vm.LaborQuote.SurchargePct == null ? "6%" : vm.LaborQuote.SurchargePct + "%") + "</td>"); stream.Write(@"
                    </tr>
                    <tr>
                        <td>Estimated By:</td>
                        <td>" + vm.LaborQuote.EstimatorName.Trim() + "</td>"); stream.Write(@"
                        
                        <td>Reg Hrs: </td>
                        <td>" + (vm.LaborQuote.RegularHours != null && vm.LaborQuote.RegularHours == "0" ? "" : vm.LaborQuote.RegularHours) + "</td>"); stream.Write(@"
						<td>Reg Hourly Rate: </td>
                        <td>" + (vm.LaborQuote.RegularHsRate ?? "$49") + "</td>"); stream.Write(@"
                        <td>" + Math.Round(surchargeReg * surchargpct / 100, 2) + "</td>"); stream.Write(@"
                        <td>" + Math.Round(surchargeReg * surchargpct * surchargeReg / 100) + "</td>"); stream.Write(@"
					</tr>
                    <tr>
                        <td>Estimate Date: </td>
                        <td>" + (vm.LaborQuote.ModifiedOn != null ? ((DateTime)vm.LaborQuote.ModifiedOn).ToString("MM/dd/yyyy") : DateTime.Now.ToString("MM/dd/yyyy")) + "</td>"); stream.Write(@"
                        <td>OT Hours: </td>
                        <td>" + (vm.LaborQuote.OTHours != null && vm.LaborQuote.OTHours == "0" ? "" : vm.LaborQuote.OTHours) + "</td>"); stream.Write(@"
                        <td>OT Hourly Rate: </td>
                        <td>" + (vm.LaborQuote.OTHsRate ?? "$73.50") + "</td>"); stream.Write(@"
                        <td>" + Math.Round(surchargeOt * surchargpct / 100, 2) + "</td>"); stream.Write(@"
						<td>" + Math.Round(surchargeOt * surchargpct * surchargeOt / 100) + "</td>"); stream.Write(@"
					</tr>
                    <tr>
                        <td>Expiration Date: </td>
                        <td>" + vm.LaborQuote.CreatedOn.AddMonths(3).ToString("MM/dd/yyyy") + "</td>"); stream.Write(@"
                        <td>DT Hours: </td>
                        <td>" + (vm.LaborQuote.DTHours != null && vm.LaborQuote.DTHours == "0" ? "" : vm.LaborQuote.DTHours) + "</td>"); stream.Write(@"
                        <td>DT Hourly Rate: </td>
                        <td>" + (vm.LaborQuote.DTHsRate ?? "$98") + "</td>"); stream.Write(@"
                        <td>" + Math.Round(surchargeDt * surchargpct / 100, 2) + "</td>"); stream.Write(@"
                        <td>" + Math.Round(surchargeDt * surchargpct * surchargeDt / 100) + "</td>"); stream.Write(@"
                    </tr>
					<tr></tr>
                    <tr>
                        <td>Project Total Hours: </td>
                        <td>" + (fiTot + aoTot) + "</td>"); stream.Write(@"
                        <td>Total: </td>
                        <td>" + (dthrs + othrs + reghrs) + "</td>"); stream.Write(@"
                        <td>Total Labor Costs: </td>
                        <td colspan='3'>" + Math.Round(totlabcost, 2) + "</td>"); stream.Write(@"
                    </tr>
                    <tr>
                        <td>Hours Validation: </td>
                        <td>" + ((dthrs + othrs + reghrs) - (fiTot + aoTot)) + "</td>"); stream.Write(@"
                        <td colspan='3' style='text-align:right'>Install Total(includes Add On's and Misc Charges): </td>
                        <td colspan='3'>" + (Math.Round(totlabcost, 2) + mcTot) + "</td>"); stream.Write(@"
                    </tr>
                    <tr>
                        <td colspan='2'></td>
						<td colspan='3' style='text-align:right'>Health & Safety Surcharge: </td>
                        <td colspan='3'>" + Math.Round(totlabcost * surchargpct / 100, 2) + "</td>"); stream.Write(@"
                    </tr>
					<tr></tr>
                    <tr>
                        <td>Complex Project: </td>
                        <td>" + (vm.LaborQuote.ComplexProject ? "Yes" : "No") + "</td>"); stream.Write(@"
                        <td colspan='3' style='text-align:right'># of Days: </td>
                        <td>" + (vm.LaborQuote.NumberOfDays ?? 1) + "</td>"); stream.Write(@"
                        <td>Truck Capacity: </td>"); stream.Write(@"
						<td>" + vm.LaborQuote.TruckCapacity + "%</td>"); stream.Write(@"
                    </tr>
                </table>
            </div>
			<hr />
			<div style='padding-left:5px;padding-bottom: 20px'>
			<h4 class='ml-4'>Labor Breakdown</h4>
			<hr style='margin-left:0;width:25%'/>
						<table class='table table-sm table-bordered text-center' style='margin-left:auto;margin-right:auto'>
                        <thead>
                            <tr>
                                <th>Labor Breakdown</th>
                                <th>Labor Total $</th>
                                <th>Hourly Rate</th>
                                <th>Total Labor Hours</th>
                                <th>Number Days in Project</th>
                                <th>Total Man Hours Per/Day</th>
                                <th>Number of Men Per Day</th>
                            </tr>
                        </thead>
						"); int laborTotalHs = vm.LaborQuote.NumberOfDays ?? 1; stream.Write(@"
                        <tbody>
                            <tr>
                                <td style='text-align:left;width:260px'>Labor Hours (Regular)</td>
                                <td style='text-align:right'>" + ((double?)Math.Round(totlabcost, 3))?.ToString("C") + "</td>"); stream.Write(@"
                                <td style='text-align:right'>$47.00</td>
                                <td style='text-align:right'>" + Math.Round(totlabcost / 47, 3) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (vm.LaborQuote.NumberOfDays ?? 1) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (laborTotalHs == 0 ? 0 : Math.Round(totlabcost / 47 / laborTotalHs, 3)) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (laborTotalHs == 0 ? 0 : Math.Round(totlabcost / 47 / laborTotalHs / 8, 3)) + "</td>"); stream.Write(@"
                            </tr>
                            <tr>
                                <td style='text-align:left;width:260px'>Labor Hours(Overtime)</td>
                                <td style='text-align:right'>" + ((double?)Math.Round(totlabcost, 3))?.ToString("C") + "</td>"); stream.Write(@"
                                <td style='text-align:right'>$73.50</td>
                                <td style='text-align:right'>" + Math.Round(totlabcost / 73.5, 3) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (vm.LaborQuote.NumberOfDays ?? 1) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (laborTotalHs == 0 ? 0 : Math.Round(totlabcost / 73.5 / laborTotalHs, 3)) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (laborTotalHs == 0 ? 0 : Math.Round(totlabcost / 73.5 / laborTotalHs / 8, 3)) + "</td>"); stream.Write(@"
                            </tr>
                            <tr>
                                <td style='text-align:left;width:260px'>Labor Hours(Doubletime)</td>
                                <td style='text-align:right'>" + ((double?)Math.Round(totlabcost, 3))?.ToString("C") + "</td>"); stream.Write(@"
                                <td style='text-align:right'>$98.00</td>
                                <td style='text-align:right'>" + Math.Round(totlabcost / 98, 3) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (vm.LaborQuote.NumberOfDays ?? 1) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (laborTotalHs == 0 ? 0 : Math.Round(totlabcost / 98 / laborTotalHs, 3)) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (laborTotalHs == 0 ? 0 : Math.Round(totlabcost / 98 / laborTotalHs / 8, 3)) + "</td>"); stream.Write(@"
                            </tr>
                            <tr>
                                <td style='text-align:left;width:260px'>Labor Hours(Prevailing Wage)</td>
                                <td style='text-align:right'>" + ((double?)Math.Round(totlabcost, 3))?.ToString("C") + "</td>"); stream.Write(@"
                                <td style='text-align:right'>$70.00</td>
                                <td style='text-align:right'>" + Math.Round(totlabcost / 70, 3) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (vm.LaborQuote.NumberOfDays ?? 1) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (laborTotalHs == 0 ? 0 : Math.Round(totlabcost / 70 / laborTotalHs, 3)) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (laborTotalHs == 0 ? 0 : Math.Round(totlabcost / 70 / laborTotalHs / 8, 3)) + "</td>"); stream.Write(@"
                            </tr>
                            <tr>
                                <td style='text-align:left;width:260px'>Labor Hours(Prevailing Wage  O.T.)</td>
                                <td style='text-align:right'>" + ((double?)Math.Round(totlabcost, 3))?.ToString("C") + "</td>"); stream.Write(@"
                                <td style='text-align:right'>$105.00</td>
                                <td style='text-align:right'>" + Math.Round(totlabcost / 105, 3) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (vm.LaborQuote.NumberOfDays ?? 1) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (laborTotalHs == 0 ? 0 : Math.Round(totlabcost / 105 / laborTotalHs, 3)) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (laborTotalHs == 0 ? 0 : Math.Round(totlabcost / 105 / laborTotalHs / 8, 3)) + "</td>"); stream.Write(@"
                            </tr>
                            <tr>
                                <td style='text-align:left;width:260px'>Labor Hours(Prevailing Wage  D.T.)</td>
                                <td style='text-align:right'>" + ((double?)Math.Round(totlabcost, 3))?.ToString("C") + "</td>"); stream.Write(@"
                                <td style='text-align:right'>$140.00</td>
                                <td style='text-align:right'>" + Math.Round(totlabcost / 140, 3) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (vm.LaborQuote.NumberOfDays ?? 1) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (laborTotalHs == 0 ? 0 : Math.Round(totlabcost / 140 / laborTotalHs, 3)) + "</td>"); stream.Write(@"
                                <td style='text-align:right'>" + (laborTotalHs == 0 ? 0 : Math.Round(totlabcost / 140 / laborTotalHs / 8, 3)) + "</td>"); stream.Write(@"
                            </tr>
                        </tbody>
                    </table>
                </div>
				");

						stream.Write(stream.NewLine + "</body>" + stream.NewLine + "</html>");
					}

				}

				Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjA3NTg3QDMxMzcyZTM0MmUzMEhhRDNnL1Z6TTBtTVR4VW5CaExZSkd0RVNDeVA3M2ZoYVoxaUEyakVWaGc9");

				HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();
				WebKitConverterSettings settings = new WebKitConverterSettings();

				settings.WebKitPath = "Libraries/QtBinariesDotNetCore";

				htmlConverter.ConverterSettings = settings;
				htmlConverter.ConverterSettings.Margin.All = 5;
				using (PdfDocument document = htmlConverter.Convert("wwwroot/files/PDFEstProjTot" + guid + ".html"))
				{
					using (FileStream fileStream = new FileStream("wwwroot/files/EstProjTot" + guid + ".pdf", FileMode.Create, FileAccess.ReadWrite))
					{
						document.Save(fileStream);
					}
				}


				return Json(guid);
			}
			catch (Exception ex)
			{

				return Json(ex);
			}
		}

		[HttpPost]
		public async Task<IActionResult> SaveQuote([FromForm] string rawData, List<IFormFile> files)
		{
			
			LaborQuoteAutomationViewModel vm = new LaborQuoteAutomationViewModel();
			try
			{
				EstimationData data = JsonConvert.DeserializeObject<IEnumerable<EstimationData>>(rawData).FirstOrDefault();

				var region = data.Region;

				vm.LaborQuote = _db.LaborQuoteAutomationHandler.GetLaborQuoteHeaders(null, null, data.LaborQuoteNo, null, null, null, null, null, null, null, region).FirstOrDefault();
			int laborQuoteId = vm.LaborQuote.LaborQuoteHeaderID;
			string laborQuoteCode = vm.LaborQuote.LaborQuoteCode;
				List<LookupGeneralWithValue> hsTypes = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("LaborType", region).ToList();
				var status = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("Status", region);
				if (_db.LaborQuoteAutomationHandler.EstimateLaborQuoteHeader("Save", vm.LaborQuote.LaborQuoteCode, data.Address, data.City, data.State, data.Zip, data.Scope, data.Equipments,
					data.VendorNo, data.Taxable, data.Notes, data.EstimatedBy, data.CustomerName, data.ComplexProject, data.NoOfDays,
					data.InstallTotal, data.HsSurcharge, data.SurchargePct, data.LaborQuoteStatus, status.Where(y => y.Value == "Complete").Select(y => y.LookupGeneralID).FirstOrDefault(), data.TruckCap, data.ProjectId, data.QuoteNo, region, data.IsScheduled))
				{
					_db.LaborQuoteAutomationHandler.SaveRates(data.HoursAndRates, data.LaborQuoteHeaderID, region);
					EstimateItemDetails(data, laborQuoteId, region);

					await AttachEstimateFiles(files, vm);
					if (data.CurrLaborQuoteStatus == "Pending" && data.LaborQuoteStatus != int.Parse(status.Where(y => y.Value == "Pending").Select(y => y.LookupGeneralID).FirstOrDefault()))
					{
						string subject = "Labor Quote Saved" + (vm.LaborQuote.CustomerName != null ? " - " + vm.LaborQuote.CustomerName : "") + " - LQ" + laborQuoteCode + (data.Taxable == "1" ? "T" : "N") + " - Q" + vm.LaborQuote.QuoteOrOrderNumber + " - P" + vm.LaborQuote.ProjectID;
						string body = GetBodyEmail(data.LaborQuoteNo, region);
						string from = "noreply@oneworkplace.com";
						string to = vm.LaborQuote.RequestorEmail;
						string cc = vm.LaborQuote.RequestorEmailCC + ";" + vm.LaborQuote.RequestorEmailCC2;
						IFormFile file = GenerateEstimationEmailPDF(laborQuoteCode, region);
						SendEmail(from, to, cc, subject, body, file, laborQuoteCode, region == "OWP" ? "W" : "S");
					}

					vm.Saved = "saved";
					vm.Success = "success";

				} else
                {
					vm.Success = "error";
                }
				return Json(true);
			}
			catch (Exception ex)
			{
				vm.Success = "error";
				throw ex;
			}
		}

		[HttpPost]
		public async Task<IActionResult> EstimateQuote([FromForm] string rawData, List<IFormFile> files)
		{
			LaborQuoteAutomationViewModel vm = new LaborQuoteAutomationViewModel();
			try
			{
				EstimationData data = JsonConvert.DeserializeObject<IEnumerable<EstimationData>>(rawData).FirstOrDefault();
				var region = data.Region;
				vm.LaborQuote = _db.LaborQuoteAutomationHandler.GetLaborQuoteHeaders(null, null, data.LaborQuoteNo, null, null, null, null, null, null, null, region).FirstOrDefault();
				int laborQuoteId = vm.LaborQuote.LaborQuoteHeaderID;
				string laborQuoteCode = vm.LaborQuote.LaborQuoteCode;
				List<LookupGeneralWithValue> hsTypes = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("LaborType", region).ToList();
				var status = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("Status", region).Where(y => y.Value == "Complete").Select(y => y.LookupGeneralID).FirstOrDefault();
				if (_db.LaborQuoteAutomationHandler.EstimateLaborQuoteHeader("Estimate", vm.LaborQuote.LaborQuoteCode, data.Address, data.City, data.State, data.Zip, data.Scope, data.Equipments, data.VendorNo, data.Taxable, data.Notes, data.EstimatedBy, data.CustomerName,
					data.ComplexProject, data.NoOfDays, data.InstallTotal, data.HsSurcharge, data.SurchargePct, data.LaborQuoteStatus, status, data.TruckCap, data.ProjectId, data.QuoteNo, region, data.IsScheduled))
                {
					_db.LaborQuoteAutomationHandler.SaveRates(data.HoursAndRates, data.LaborQuoteHeaderID, region);
					EstimateItemDetails(data, laborQuoteId, region);

					await AttachEstimateFiles(files, vm);

					string subject = "Labor Quote Estimated" + (vm.LaborQuote.CustomerName != null ? " - " + vm.LaborQuote.CustomerName : "") + " - LQ" + laborQuoteCode + (data.Taxable == "1" ? "T" : "N") + " - Q" + vm.LaborQuote.QuoteOrOrderNumber + " - P" + vm.LaborQuote.ProjectID;
					string body = GetBodyEmail(data.LaborQuoteNo, region);
					string from = "theestimateteam@oneworkplace.com";
					string to = vm.LaborQuote.RequestorEmail;
					string cc = "theestimateteam@oneworkplace.com;" + vm.LaborQuote.RequestorEmailCC + ";" + vm.LaborQuote.RequestorEmailCC2;
					IFormFile file = GenerateEstimationEmailPDF(laborQuoteCode, region);
					SendEmail(from, to, cc, subject, body, file, laborQuoteCode, region == "OWP" ? "W" : "S");

					vm.ShowForm = false;
					vm.Success = "success";
					ViewData["LaborQuoteNo"] = "";					
				} else
                {
					vm.Success = "error";
				}
				return Json(true);
			}
			catch (Exception ex)
			{
				vm.Success = "error";
				throw ex;
			}
		}

		[HttpPost]
		public void CancelQuote(string quoteNo, string region)
		{
			try
			{
				var status = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("Status", region).Where(y => y.Value == "Canceled").Select(y => y.LookupGeneralID).FirstOrDefault();
				_db.LaborQuoteAutomationHandler.CancelQuote(quoteNo, region, status);

				LaborQuoteAutomationViewModel vm = new LaborQuoteAutomationViewModel();
				vm.LaborQuote = _db.LaborQuoteAutomationHandler.GetLaborQuoteHeaders(null, null, quoteNo, null, null, null, null, null, null, null, region).FirstOrDefault();

				string subject = "Labor Quote Canceled" + (vm.LaborQuote.CustomerName != null ? " - " + vm.LaborQuote.CustomerName : "") + " - LQ" + vm.LaborQuote.LaborQuoteCode + (vm.LaborQuote.Taxable ? "T" : "N") + " - Q" + vm.LaborQuote.QuoteOrOrderNumber + " - P" + vm.LaborQuote.ProjectID;
				string body = GetBodyEmail(quoteNo, region);
				string from = "noreply@oneworkplace.com";
				string to = vm.LaborQuote.RequestorEmail;
				string cc = "theestimateteam@oneworkplace.com;" + vm.LaborQuote.RequestorEmailCC + ";" + vm.LaborQuote.RequestorEmailCC2;
				SendEmail(from, to, cc, subject, body, null, vm.LaborQuote.LaborQuoteCode, region == "OWP" ? "W" : "S");
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		[HttpPost]
		public void DeleteAttachmentFile(string attFile)
		{
			try
			{
				System.IO.File.Delete("wwwroot/files/" + attFile);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public async Task<IActionResult> Download(string filename)
		{
			if (filename != null)
			{
				var path = Path.Combine("wwwroot/files/", filename);

				var memory = new MemoryStream();
				using (var stream = new FileStream(path, FileMode.Open))
				{
					await stream.CopyToAsync(memory);
				}
				memory.Position = 0;
				return File(memory, GetContentType(path), Path.GetFileName(path));
			}
			else
			{
				return null;
			}
		}

		private string GetBodyEmail(string laborQuoteCode, string region)
		{
			LaborQuoteAutomationViewModel vm = new LaborQuoteAutomationViewModel();
			vm.LaborQuote = _db.LaborQuoteAutomationHandler.GetLaborQuoteHeaders(null, null, laborQuoteCode, null, null, null, null, null, null, null, region).FirstOrDefault();
			vm.LaborQuoteVendors = _db.LaborQuoteAutomationHandler.LoadLaborQuoteVendors(region);
			var furnitureInstallationList = _db.LaborQuoteAutomationHandler.GetFurnitureInstallations(vm.LaborQuote.LaborQuoteHeaderID, region)?.Where(x => x.Quantity > 0 && x.Hours > 0).Select(x => x).Distinct();
			vm.LaborQuote.FurnitureInstallationList = furnitureInstallationList?.Where(x => (new string[] { "SYS", "DAT", "SAM", "SEA" }).Contains(x.Description.Trim())).Select(x => x).Distinct();
			vm.LaborQuote.CustomFurnitureInstallationList = furnitureInstallationList?.Where(x => !(new string[] { "SYS", "DAT", "SAM", "SEA" }).Contains(x.Description.Trim())).Select(x => x).Distinct();
			vm.LaborQuote.AddonsList = _db.LaborQuoteAutomationHandler.GetAddons(vm.LaborQuote.LaborQuoteHeaderID, region)?.Where(x => x.Quantity > 0 && x.Hours > 0).Select(x => x);

			string ret =
			@"<html>
            <body>
            <p><strong>Labor Quote</strong><p>
            <p style='margin-left: 20px;margin-top: -10px;'>" + vm.LaborQuote.FullLaborQuoteCode + @"</p>
			<p><strong>Labor Quote Status</strong><p>
			<p style='margin-left: 20px;margin-top: -10px;'>" + vm.LaborQuote.Status + @"</p>
            <p><strong>Project/Quote #</strong><p>
            <p style='margin-left: 20px;margin-top: -10px;'>" + vm.LaborQuote.QuoteOrOrder.ToString() + vm.LaborQuote.QuoteOrOrderNumber + " - P" + vm.LaborQuote.ProjectID + @"</p>
            <p><strong>Quote Date</strong><p>
            <p style='margin-left: 20px;margin-top: -10px;'>" + vm.LaborQuote.CreatedOn.ToString("MM/dd/yyyy") + @"</p>
            <p><strong>Original Quote #</strong><p>
            <p style='margin-left: 20px;margin-top: -10px;'>" + vm.LaborQuote.OriginalLaborQuoteCode + @"</p>
            <p><strong>Customer Name</strong><p>
            <p style='margin-left: 20px;margin-top: -10px;'>" + vm.LaborQuote.CustomerName + @"</p>
            <p><strong>Address</strong><p>
            <p style='margin-left: 20px;margin-top: -10px;'>" + vm.LaborQuote.AddressLine1 + @"</p>
            <p><strong>City</strong><p>
            <p style='margin-left: 20px;margin-top: -10px;'>" + vm.LaborQuote.AddressCity + @"</p>
            <p><strong>Scope</strong><p>
            <p style='margin-left: 20px;margin-top: -10px;'><pre>" + vm.LaborQuote.Scope + @"</pre></p>
            <p><strong>Equipments/Tools</strong><p>
            <p style='margin-left: 20px;margin-top: -10px;'>" + vm.LaborQuote.EquipmentTools + @"</p>
			<p><strong>Vendor #</strong><p>
            <p style='margin-left: 20px;margin-top: -10px;'>" + GetVendorName(vm.LaborQuote.VendorNumberLookupID, region) + @"</p>
            <p><strong>Select Tax</strong><p>
            <p style='margin-left: 20px;margin-top: -10px;'>" + (vm.LaborQuote.Taxable ? "Taxable" : "Non-Taxable") + @"</p>";
			if (vm.LaborQuote.FurnitureInstallationList?.Count() > 0 || vm.LaborQuote.CustomFurnitureInstallationList?.Count() > 0)
			{
				ret += @"<p><strong>Furniture Installation</strong><p>";
			}
			if (vm.LaborQuote.FurnitureInstallationList?.Count() > 0)
			{
				foreach (var fi in vm.LaborQuote.FurnitureInstallationList)
				{
					ret += @"<p style='margin-left: 20px;margin-top: -10px;'>" + fi.Description + " - Qty.: " + fi.Quantity + @"</p>";
				}
			}
			if (vm.LaborQuote.CustomFurnitureInstallationList?.Count() > 0)
			{
				var customFurnQtySum = vm.LaborQuote.CustomFurnitureInstallationList.Select(x => x.Quantity).Sum();
				ret += @"<p style='margin-left: 20px;margin-top: -10px;'>Other - Qty.: " + customFurnQtySum.ToString() + @"</p>";
			}
			if (vm.LaborQuote.AddonsList?.Count() > 0)
			{
				ret += @"<p><strong>Labor Add Ons</strong><p>";
				foreach (var ad in vm.LaborQuote.AddonsList)
				{
					ret += @"<p style='margin-left: 20px;margin-top: -10px;'>" + ad.Description + " - Qty.: " + ad.Quantity + @"</p>";
				}
			}
			ret += @"
            <p><strong>Contingencies</strong><p>
            <p>All work is to be performed during normal business hours unless the scope of work specifies overtime delivery or installation. Normal business hours are from 7:00 AM - 5:00 PM. Any project beginning after 1:00 PM is subject to Overtime. All work is quoted as non-prevailing wage unless otherwise specified. The customer must provide clear and free access to the build area and an adequate staging area. Any work performed in addition to the original scope of work noted above is subject to additional costs to the project. All deliveries on the second floor or higher will require a passenger or freight elevator. Any additional hardware or equipment required to complete the project that is not included in the scope of work noted above will be subject to additional costs.</p>
            <p><strong>Estimated By</strong><p>
            <p style='margin-left: 20px;margin-top: -10px;'>" + vm.LaborQuote.EstimatorName + @"</p>
            <p><strong>Date</strong><p>
            <p style='margin-left: 20px;margin-top: -10px;'>" + vm.LaborQuote.CreatedOn.ToString("MM/dd/yyyy") + @"</p>
            <p><strong>Expiration Date</strong><p>
            <p style='margin-left: 20px;margin-top: -10px;'>" + ((DateTime)vm.LaborQuote.ExpirationDate).ToString("MM/dd/yyyy") + @"</p>
            <p><strong>This project is quoted for</strong><p>
            <p style='margin-left: 20px;margin-top: -10px;'>" + vm.LaborQuote.NumberOfDays + @" day(s).</p>
            <p><strong>Labor Total</strong><p>
            <p style='margin-left: 20px;margin-top: -10px;'>" + (vm.LaborQuote.ComplexProject ? "See scope above for Breakdown" : (vm.LaborQuote.InstallTotal + vm.LaborQuote.HealthAndSafetySurcharge).ToString("C")) + @"</p>                                                                                  
            </body>
            </html>";

			return ret;
		}

		private string GetVendorName(string vendorLookupId, string region)
		{
			var vendorList = _db.LaborQuoteAutomationHandler.LoadLaborQuoteVendors(region);
			return vendorList.Where(x => x.LookupGeneralID.Trim() == vendorLookupId.Trim()).Select(x => x.Value).FirstOrDefault();
		}

		private string GetContentType(string path)
		{
			var types = GetMimeTypes();
			var ext = Path.GetExtension(path).ToLowerInvariant();
			return types[ext];
		}

		private Dictionary<string, string> GetMimeTypes()
		{
			return new Dictionary<string, string>
			{
				{".txt", "text/plain"},
				{".pdf", "application/pdf"},
				{".doc", "application/vnd.ms-word"},
				{".docx", "application/vnd.ms-word"},
				{".xls", "application/vnd.ms-excel"},
				{".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
				{".png", "image/png"},
				{".jpg", "image/jpeg"},
				{".jpeg", "image/jpeg"},
				{".gif", "image/gif"},
				{".csv", "text/csv"}
			};
		}

		private void EstimateItemDetails(EstimationData data, int laborQuoteId, string region)
        {
            try
            {

            
			DateTime dtNowParam = DateTime.Now;
			bool deleteFurns = true;
			bool deleteAddons = true;
			bool deleteMiscs = true;

			if (!string.IsNullOrEmpty(data.SystemQty) && !string.IsNullOrEmpty(data.SystemHsXQty) && !string.IsNullOrEmpty(data.SystemHs))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteFurnitureInstallation(laborQuoteId, "SYS", data.SystemQty, data.SystemHsXQty, data.SystemHs, dtNowParam, region);
				deleteFurns = false;
			}
			if (!string.IsNullOrEmpty(data.DeskAndTablesQty) && !string.IsNullOrEmpty(data.DeskAndTablesHsXQty) && !string.IsNullOrEmpty(data.DeskAndTablesHs))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteFurnitureInstallation(laborQuoteId, "DAT", data.DeskAndTablesQty, data.DeskAndTablesHsXQty, data.DeskAndTablesHs, dtNowParam, region);
				deleteFurns = false;
			}
			if (!string.IsNullOrEmpty(data.StorageAndMiscQty) && !string.IsNullOrEmpty(data.StorageAndMiscHsXQty) && !string.IsNullOrEmpty(data.StorageAndMiscHs))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteFurnitureInstallation(laborQuoteId, "SAM", data.StorageAndMiscQty, data.StorageAndMiscHsXQty, data.StorageAndMiscHs, dtNowParam, region);
				deleteFurns = false;
			}
			if (!string.IsNullOrEmpty(data.SeatingQty) && !string.IsNullOrEmpty(data.SeatingHsXQty) && !string.IsNullOrEmpty(data.SeatingHs))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteFurnitureInstallation(laborQuoteId, "SEA", data.SeatingQty, data.SeatingHsXQty, data.SeatingHs, dtNowParam, region);
				deleteFurns = false;
			}

			foreach (var item in data.NewFurnItems)
			{
				var arr = item.Split("|");
				var desc = arr[0];
				var qty = arr[1];
				var hsxqty = arr[2];
				var hs = arr[3];
				if (!string.IsNullOrEmpty(desc) && !string.IsNullOrEmpty(qty) && !string.IsNullOrEmpty(hsxqty) && !string.IsNullOrEmpty(hs))
				{
					_db.LaborQuoteAutomationHandler.EstimateLaborQuoteFurnitureInstallation(laborQuoteId, desc.Trim(), qty.Trim(), hsxqty.Trim(), hs.Trim(), dtNowParam, region);
					deleteFurns = false;
				}
			}

			if (deleteFurns)
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteFurnitureInstallation(laborQuoteId, null, null, null, null, dtNowParam, region);
			}

			if (!string.IsNullOrEmpty(data.StairDistStagingQty) && !string.IsNullOrEmpty(data.StairDistStagingHs))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteAddOns(laborQuoteId, "SCDAS", data.StairDistStagingQty, data.StairDistStagingHs, dtNowParam, region);
				deleteAddons = false;
			}
			if (!string.IsNullOrEmpty(data.WallChannelDrywallQty) && !string.IsNullOrEmpty(data.WallChannelDrywallHs))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteAddOns(laborQuoteId, "WSCSD", data.WallChannelDrywallQty, data.WallChannelDrywallHs, dtNowParam, region);
				deleteAddons = false;
			}
			if (!string.IsNullOrEmpty(data.WallChannelConcreteQty) && !string.IsNullOrEmpty(data.WallChannelConcreteHs))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteAddOns(laborQuoteId, "WSCSC", data.WallChannelConcreteQty, data.WallChannelConcreteHs, dtNowParam, region);
				deleteAddons = false;
			}
			if (!string.IsNullOrEmpty(data.GrommetHoleCutsQty) && !string.IsNullOrEmpty(data.GrommetHoleCutsHs))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteAddOns(laborQuoteId, "GHCUT", data.GrommetHoleCutsQty, data.GrommetHoleCutsHs, dtNowParam, region);
				deleteAddons = false;
			}
			if (!string.IsNullOrEmpty(data.WorksurfaceCutsQty) && !string.IsNullOrEmpty(data.WorksurfaceCutsHs))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteAddOns(laborQuoteId, "WSCUT", data.WorksurfaceCutsQty, data.WorksurfaceCutsHs, dtNowParam, region);
				deleteAddons = false;
			}
			if (!string.IsNullOrEmpty(data.DeliveryTimeQty) && !string.IsNullOrEmpty(data.DeliveryTimeHs))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteAddOns(laborQuoteId, "DTIME", data.DeliveryTimeQty, data.DeliveryTimeHs, dtNowParam, region);
				deleteAddons = false;
			}
			if (!string.IsNullOrEmpty(data.WarehouseQty) && !string.IsNullOrEmpty(data.WarehouseHs))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteAddOns(laborQuoteId, "WAREH", data.WarehouseQty, data.WarehouseHs, dtNowParam, region);
				deleteAddons = false;
			}
			if (!string.IsNullOrEmpty(data.NewLabor1) && !string.IsNullOrEmpty(data.NewLabor1Qty) && !string.IsNullOrEmpty(data.NewLabor1Hs))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteAddOns(laborQuoteId, data.NewLabor1.Trim(), data.NewLabor1Qty, data.NewLabor1Hs, dtNowParam, region);
				deleteAddons = false;
			}
			if (!string.IsNullOrEmpty(data.NewLabor2) && !string.IsNullOrEmpty(data.NewLabor2Qty) && !string.IsNullOrEmpty(data.NewLabor2Hs))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteAddOns(laborQuoteId, data.NewLabor2.Trim(), data.NewLabor2Qty, data.NewLabor2Hs, dtNowParam, region);
				deleteAddons = false;
			}
			if (!string.IsNullOrEmpty(data.NewLabor3) && !string.IsNullOrEmpty(data.NewLabor3Qty) && !string.IsNullOrEmpty(data.NewLabor3Hs))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteAddOns(laborQuoteId, data.NewLabor3.Trim(), data.NewLabor3Qty, data.NewLabor3Hs, dtNowParam, region);
				deleteAddons = false;
			}
			if (!string.IsNullOrEmpty(data.NewLabor4) && !string.IsNullOrEmpty(data.NewLabor4Qty) && !string.IsNullOrEmpty(data.NewLabor4Hs))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteAddOns(laborQuoteId, data.NewLabor4.Trim(), data.NewLabor4Qty, data.NewLabor4Hs, dtNowParam, region);
				deleteAddons = false;
			}

			if (deleteAddons)
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteAddOns(laborQuoteId, null, null, null, dtNowParam, region);
			}

			if (!string.IsNullOrEmpty(data.Truck) && !string.IsNullOrEmpty(data.TruckQty) && !string.IsNullOrEmpty(data.TruckRate))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteMiscInstallationCharges(laborQuoteId, "TR", data.Truck, data.TruckQty, data.TruckRate, dtNowParam, region);
				deleteMiscs = false;
			}
			if (!string.IsNullOrEmpty(data.MileageParking))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteMiscInstallationCharges(laborQuoteId, "MP", data.MileageParking, null, null, dtNowParam, region);
				deleteMiscs = false;
			}
			if (!string.IsNullOrEmpty(data.Supplies))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteMiscInstallationCharges(laborQuoteId, "SP", data.Supplies, null, null, dtNowParam, region);
				deleteMiscs = false;
			}
			if (!string.IsNullOrEmpty(data.LoadCrew) && !string.IsNullOrEmpty(data.LoadCrewQty) && !string.IsNullOrEmpty(data.LoadCrewRate))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteMiscInstallationCharges(laborQuoteId, "LC", data.LoadCrew, data.LoadCrewQty, data.LoadCrewRate, dtNowParam, region);
				deleteMiscs = false;
			}
			if (!string.IsNullOrEmpty(data.Disposal))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteMiscInstallationCharges(laborQuoteId, "DP", data.Disposal, null, null, dtNowParam, region);
				deleteMiscs = false;
			}
			if (!string.IsNullOrEmpty(data.NewMisc1) && !string.IsNullOrEmpty(data.NewMisc1Dollars))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteMiscInstallationCharges(laborQuoteId, data.NewMisc1.Trim(), data.NewMisc1Dollars, null, null, dtNowParam, region);
				deleteMiscs = false;
			}
			if (!string.IsNullOrEmpty(data.NewMisc2) && !string.IsNullOrEmpty(data.NewMisc2Dollars))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteMiscInstallationCharges(laborQuoteId, data.NewMisc2.Trim(), data.NewMisc2Dollars, null, null, dtNowParam, region);
				deleteMiscs = false;
			}
			if (!string.IsNullOrEmpty(data.NewMisc3) && !string.IsNullOrEmpty(data.NewMisc3Dollars))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteMiscInstallationCharges(laborQuoteId, data.NewMisc3.Trim(), data.NewMisc3Dollars, null, null, dtNowParam, region);
				deleteMiscs = false;
			}
			if (!string.IsNullOrEmpty(data.NewMisc4) && !string.IsNullOrEmpty(data.NewMisc4Dollars))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteMiscInstallationCharges(laborQuoteId, data.NewMisc4.Trim(), data.NewMisc4Dollars, null, null, dtNowParam, region);
				deleteMiscs = false;
			}
			if (!string.IsNullOrEmpty(data.NewMisc5) && !string.IsNullOrEmpty(data.NewMisc5Dollars))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteMiscInstallationCharges(laborQuoteId, data.NewMisc5.Trim(), data.NewMisc5Dollars, null, null, dtNowParam, region);
				deleteMiscs = false;
			}
			if (!string.IsNullOrEmpty(data.NewMisc6) && !string.IsNullOrEmpty(data.NewMisc6Dollars))
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteMiscInstallationCharges(laborQuoteId, data.NewMisc6.Trim(), data.NewMisc6Dollars, null, null, dtNowParam, region);
				deleteMiscs = false;
			}

			if (deleteMiscs)
			{
				_db.LaborQuoteAutomationHandler.EstimateLaborQuoteMiscInstallationCharges(laborQuoteId, null, null, null, null, dtNowParam, region);
			}
				var breakdowns = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("Breakdown", region);
				//_db.LaborQuoteAutomationHandler.EstimateLaborQuoteHours(laborQuoteId, Convert.ToInt32(breakdowns.Where(x => x.Value == "Reg").Select(x => x.LookupGeneralID).FirstOrDefault()), false, data.RegularTotalHs, data.RegularHourlyRate, data.CommonTotalDlls, region);
				//_db.LaborQuoteAutomationHandler.EstimateLaborQuoteHours(laborQuoteId, Convert.ToInt32(breakdowns.Where(x => x.Value == "OT").Select(x => x.LookupGeneralID).FirstOrDefault()), false, data.OvertimeTotalHs, data.OvertimeHourlyRate, data.CommonTotalDlls, region);
				//_db.LaborQuoteAutomationHandler.EstimateLaborQuoteHours(laborQuoteId, Convert.ToInt32(breakdowns.Where(x => x.Value == "DT").Select(x => x.LookupGeneralID).FirstOrDefault()), false, data.DoubletimeTotalHs, data.DoubletimeHourlyRate, data.CommonTotalDlls, region);
				//_db.LaborQuoteAutomationHandler.EstimateLaborQuoteHours(laborQuoteId, Convert.ToInt32(breakdowns.Where(x => x.Value == "PWR").Select(x => x.LookupGeneralID).FirstOrDefault()), true, data.PwTotalHs, data.PwHourlyRate, data.CommonTotalDlls, region);
				//_db.LaborQuoteAutomationHandler.EstimateLaborQuoteHours(laborQuoteId, Convert.ToInt32(breakdowns.Where(x => x.Value == "PWOT").Select(x => x.LookupGeneralID).FirstOrDefault()), true, data.PwOtTotalHs, data.PwOtHourlyRate, data.CommonTotalDlls, region);
				//_db.LaborQuoteAutomationHandler.EstimateLaborQuoteHours(laborQuoteId, Convert.ToInt32(breakdowns.Where(x => x.Value == "PWDT").Select(x => x.LookupGeneralID).FirstOrDefault()), true, data.PwDtTotalHs, data.PwDtHourlyRate, data.CommonTotalDlls, region);
                foreach (var item in data.HoursAndRates)
                {
					_db.LaborQuoteAutomationHandler.EstimateLaborQuoteHours(laborQuoteId, Convert.ToInt32(breakdowns.Where(x => x.Value == item.Type).Select(x => x.LookupGeneralID).FirstOrDefault()), item.Type.Contains("PW"), item.Hours, item.Rate, data.CommonTotalDlls, region);
				}
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		private async Task<IActionResult> AttachEstimateFiles(List<IFormFile> files, LaborQuoteAutomationViewModel vm)
		{
			if (files?.Count > 0)
			{
				foreach (var file in files)
				{
					using (var stream = new FileStream("wwwroot/files/" + vm.LaborQuote.LaborQuoteCode + "_att_" + file.FileName, FileMode.Create, FileAccess.ReadWrite))
					{
						await file.CopyToAsync(stream);
					}
				}
			}

			return View(vm);
		}

		private void SendEmail(string from, string to, string cc, string subject, string body, IFormFile file, string laborQuoteCode, string companyCode)
		{
			_emailHelper.SendEmailWithReply(from, "", to, cc, "", "", subject, body, file, null, "EPT");

			_db.SaveActivity(new ActivityLog
			{
				YourEmail = from,
				ToEmail = to,
				Body = body,
				Subject = subject,
				Order = laborQuoteCode,
				Vendor = "",
				CreatedBy = "LQA",
				CompanyCode = companyCode
			});
		}

		private IFormFile GenerateEstimationEmailPDF(string laborQuoteNo, string region)
		{
			try
			{
				removeOldFiles();
				LaborQuoteAutomationViewModel vm = new LaborQuoteAutomationViewModel();
				vm.LaborQuote = _db.LaborQuoteAutomationHandler.GetLaborQuoteHeaders(null, null, laborQuoteNo, null, null, null, null, null, null, null, region)?.FirstOrDefault();
				vm.LaborQuoteVendors = _db.LaborQuoteAutomationHandler.LoadLaborQuoteVendors(region);
				var furnitureInstallationList = _db.LaborQuoteAutomationHandler.GetFurnitureInstallations(vm.LaborQuote.LaborQuoteHeaderID, region)?.Where(x => x.Quantity > 0 && x.Hours > 0).Select(x => x).Distinct();
				vm.LaborQuote.FurnitureInstallationList = furnitureInstallationList?.Where(x => (new string[] { "SYS", "DAT", "SAM", "SEA" }).Contains(x.Description.Trim())).Select(x => x).Distinct();
				vm.LaborQuote.CustomFurnitureInstallationList = furnitureInstallationList?.Where(x => !(new string[] { "SYS", "DAT", "SAM", "SEA" }).Contains(x.Description.Trim())).Select(x => x).Distinct();
				var addonsList = _db.LaborQuoteAutomationHandler.GetAddons(vm.LaborQuote.LaborQuoteHeaderID, region)?.Where(x => x.Quantity > 0 && x.Hours > 0).Select(x => x).Distinct();
				vm.LaborQuote.AddonsList = addonsList?.Where(x => (new string[] { "SCDAS", "WSCSD", "WSCSC", "GHCUT", "WSCUT", "DTIME", "WAREH" }).Contains(x.Description.Trim())).Select(x => x).Distinct();
				vm.LaborQuote.CustomAddonsList = addonsList?.Where(x => !(new string[] { "SCDAS", "WSCSD", "WSCSC", "GHCUT", "WSCUT", "DTIME", "WAREH" }).Contains(x.Description.Trim())).Select(x => x);
				string guid = Guid.NewGuid().ToString();
				using (StreamWriter stream = new StreamWriter("wwwroot/files/EstimationEmailPDF" + guid + ".html", false, System.Text.Encoding.UTF8))
                {
					stream.Write(@"
					<html>" + stream.NewLine + @"
					<head>" + stream.NewLine + @"
					<style>
						body { background-color: whitesmoke; font-family: Arial; }
					</style>
					</head>" + stream.NewLine + @"
					<body>" + stream.NewLine + @"
					<h2 style='margin-top:15px;margin-left:15px'>Estimator Project Totals</h2>			
					<hr />
					<div style='margin-top:5px; padding: 5px;'>
						<h4 class='ml-4'>Project Summary</h4>	
						<hr style='margin-left:0;width:25%'/>
						<br /><br />
						<table style='margin-left:auto;margin-right:auto;'>	 
							<tr>
								<td width='200px'><p>Project ID: </p></td>
								<td><p>" + vm.LaborQuote.ProjectID + @"</p></td>
							</tr>	
							<tr>
								<td width='200px'><p>Status: </p></td>
								<td><p>" + vm.LaborQuote.Status + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Labor Quote: </p></td>
								<td><p>" + vm.LaborQuote.FullLaborQuoteCode + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Quote #: </p></td>
								<td><p>" + vm.LaborQuote.QuoteOrOrderNumber + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Requested Date: </p></td>
								<td><p>" + vm.LaborQuote.CreatedOn.ToString("MM/dd/yyyy") + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Original Labor Quote: </p></td>
								<td><p>" + vm.LaborQuote.OriginalLaborQuoteCode + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Customer Name: </p></td>
								<td><p>" + vm.LaborQuote.CustomerName + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>Address: </p></td>
								<td><p>" + vm.LaborQuote.AddressLine1 + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>City: </p></td>
								<td><p>" + vm.LaborQuote.AddressCity + @"</p></td>
							</tr>
							<tr>
								<td width='200px'><p>State: </p></td>
								<td><p></p></td>
							</tr>
							<tr>
								<td width='200px'><p>Zip: </p></td>
								<td><p></p></td>
							</tr>
						</table>
					</div>				
					<hr />
					<div style='margin-top:5px; padding: 5px;'>
						<h4 class='ml-4'>Scope</h4>
						<hr style='margin-left:0;width:25%'/>
						<br /><br />
						<pre style='padding-left: 25%;font-family: Arial;white-space: pre-wrap;max-width: 630px'>" + vm.LaborQuote.Scope + @"</pre>
						<table style='margin-left:auto;margin-right:auto'>
							<tr>
								<td width='200px'>
									<p>Equipments/Tools: </p>
									<p>Vendor #: </p>
									<p>Select Tax: </p>
								</td>
								<td>
									<p>" + vm.LaborQuote.EquipmentTools + @"</p>
									<p>" + vm.LaborQuoteVendors.Where(x => x.LookupGeneralID == vm.LaborQuote.VendorNumberLookupID).Select(x => x.Value).FirstOrDefault() + @"</p>
									<p>" + (vm.LaborQuote.Status == "Pending" ? "Taxable" : (vm.LaborQuote.Taxable ? "Taxable" : "NonTaxable")) + @"</p>
								</td>
							</tr>
						</table>
					</div>
					<hr />
					<div style='margin-top:5px; padding: 5px;'>
						<h4 class='ml-4'>Furniture Installation (quantities)</h4>
						<hr style='margin-left:0;width:25%'/>
						<br /><br />
						<table style='margin-left:auto;margin-right:auto'> 
				");

					var fiList = vm.LaborQuote.FurnitureInstallationList?.ToList();
					if (fiList?.Count() > 0)
					{
						foreach (var fi in fiList)
						{
							string description = "";
							switch (fi.Description)
							{
								case "CON": description = "Context"; break;
								case "SYS": description = "System"; break;
								case "DAT": description = "Casegoods: Desks and Tables"; break;
								case "SAM": description = "Casegoods: Storage and Misc"; break;
								case "SEA": description = "Seating"; break;
							}
							stream.Write(@"
							<tr>
								<td width='400px'><p>" + description + @"</p></td>
								<td width='150px'><p>" + (fi.Quantity == 0 ? "" : fi.Quantity.ToString()) + @"</p></td>
							</tr>
					");
						}
					}
					var cfiList = vm.LaborQuote.CustomFurnitureInstallationList?.ToList();
					if (cfiList?.Count() > 0)
					{
						foreach (var cfi in cfiList)
						{
							stream.Write(@"
							<tr>
								<td width='400px'><p>" + cfi.Description + @"</p></td>
								<td width='150px'><p>" + cfi.Quantity + @"</p></td>
							</tr>
						");
						}
					}

					stream.Write(@"
					</table>
					</div>
					<hr />
					<div style='margin-top:5px; padding: 5px;'>
						<h4 class='ml-4'>Labor Add Ons (quantities)</h4>
						<hr style='margin-left:0;width:25%'/>
						<br /><br />
						<table style='margin-left:auto;margin-right:auto'> 
				");

					var aoList = vm.LaborQuote.AddonsList?.ToList();
					if (aoList?.Count() > 0)
					{
						foreach (var ao in aoList)
						{
							string description = "";
							switch (ao.Description)
							{
								case "SCDAS": description = "Stair Carry/Distance/Add'l Staging"; break;
								case "WSCSD": description = "Wall Strip/Channel/Start Drywall"; break;
								case "WSCSC": description = "Wall Strip/Channel/Start Concrete"; break;
								case "GHCUT": description = "Grommet Hole Cuts"; break;
								case "WSCUT": description = "Worksurface Cuts"; break;
								case "DTIME": description = "Delivery Time"; break;
								case "WAREH": description = "Warehouse"; break;
								default: description = ao.Description; break;
							}
							stream.Write(@"
							<tr>
								<td width='400px'><p>" + description + @"</p></td>
								<td width='150px'><p>" + (ao.Quantity == 0 ? "" : ao.Quantity.ToString()) + @"</p></td>
							</tr>
					");
						}
					}
					var caoList = vm.LaborQuote.CustomAddonsList?.ToList();
					if (caoList?.Count() > 0)
					{
						foreach (var cao in caoList)
						{
							stream.Write(@"
							<tr>
								<td width='400px'><p>" + cao.Description + @"</p></td>
								<td width='150px'><p>" + cao.Quantity + @"</p></td>
							</tr>
						");
						}
					}


				var estimatorName = vm.LaborQuote.EstimatorName.Trim();
				var numberOfDays = (vm.LaborQuote.NumberOfDays ?? 0).ToString();
				var createdOn = vm.LaborQuote.CreatedOn.ToString("MM/dd/yyyy");
				var laborTotal = vm.LaborQuote.ComplexProject ? "See scope above for Breakdown" : (vm.LaborQuote.InstallTotal + vm.LaborQuote.HealthAndSafetySurcharge).ToString("C");
				var expirationDate = vm.LaborQuote.ExpirationDate == null ? "" : ((DateTime)vm.LaborQuote.ExpirationDate).ToString("MM/dd/yyyy");

					stream.Write(@"
					</table>
					</div>
					<hr />
					<div style='margin-top:5px;padding:5px;'>
						<br /><br />
						<table style='margin-left:auto;margin-right:auto'>	 
						<tr>
							<td width='200px'>
								<p>Estimated By:</p>
								<p>This project is quoted for </p>
								<p>Date:</p>
								<p>Labor Total:</p>
								<p>Expiration Date:</p>
							</td>
							<td>
								<p>" + estimatorName + @"</p>
								<p>" + numberOfDays + @" day(s).</p>
								<p>" + createdOn + @"</p>
								<p>" + laborTotal + @"</p>
								<p>" + expirationDate + @"</p>
							</td>
						</tr>
						</table>
					</div>
				");

					stream.Write(stream.NewLine + "</body>" + stream.NewLine + "</html>");
				}

				Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjA3NTg3QDMxMzcyZTM0MmUzMEhhRDNnL1Z6TTBtTVR4VW5CaExZSkd0RVNDeVA3M2ZoYVoxaUEyakVWaGc9");

				HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();
				WebKitConverterSettings settings = new WebKitConverterSettings();
				settings.WebKitPath = "Libraries/QtBinariesDotNetCore";
				htmlConverter.ConverterSettings = settings;
				htmlConverter.ConverterSettings.Margin.All = 5;

				using (PdfDocument document = htmlConverter.Convert("wwwroot/files/EstimationEmailPDF" + guid + ".html"))
                {
					using (FileStream fileStream = new FileStream("wwwroot/files/EstimationEmailPDF" + guid + ".pdf", FileMode.Create, FileAccess.ReadWrite))
					{
						document.Save(fileStream);
						document.Close(true);
					}
				}				

				string file = "wwwroot/files/EstimationEmailPDF" + guid + ".pdf";
				MemoryStream ms = new MemoryStream(System.IO.File.ReadAllBytes(file));
				var formFile = new FormFile(ms, 0, ms.Length, "streamFile", file.Split(@"/").Last());
				return formFile;
			}
			catch (Exception ex)
			{
				throw ex;
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