using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OWPApplications.Data;
using OWPApplications.Models;
using OWPApplications.Models.LaborQuoteAutomation;
using OWPApplications.Models.LaborQuoteAutomationCommons;
using OWPApplications.Utils;
using static OWPApplications.Utils.EmailHelper;

namespace OWPApplications.Controllers
{
	public class OneQuoteFormController : Controller
	{
		DbHandler _db;
		IConfiguration _configuration;
		EmailHelper _emailHelper;

		public OneQuoteFormController(DbHandler dbHandler, IConfiguration configuration, EmailHelper emailHelper)
		{
			_db = dbHandler;
			_configuration = configuration;
			_emailHelper = emailHelper;
		}

		public async Task<IActionResult> Index(string submit, bool isRevision, string revisionCode, string[] laborQuoteTypes, string laborQuoteRequestorName, string laborQuoteRequestorEmail,
			string laborQuoteHedbergProjectID, string laborQuoteCustomer, string laborQuoteStreetAddress, string laborQuoteBRF,
			string laborQuoteCity, char laborQuoteOrOrder, string laborQuoteSalesRep, string laborQuoteCoordinator, string laborQuoteProjectManager,
			int laborQuoteDirectoryCreated, string laborQuoteNoDaysForInstall, string originalLaborQuoteNo, string quoteOrOrderNumber,
			char laborQuoteProjectIs, string[] laborQuoteUnionVendors, string laborQuoteUnionVendorsADD, DateTime laborQuoteTargetDate, string laborQuoteSupplyRequestorEmail,
			string laborQuoteScopeOfWork, string laborQuoteCode, int? laborQuoteDeliver, int? laborQuoteDeliverTime, int? laborQuoteInstallation,
			int? laborQuoteInstallationTime, string[] laborQuoteProductForm, int? laborQuoteUnloadAt, int? laborQuoteSiteIs, string[] laborQuoteWallsAre,
			string[] laborQuoteProtection, string laborQuoteFeetMasonite, string[] laborQuoteElevator, int? laborQuoteCarryUp, int? laborQuoteNoOfFlights,
			string[] laborQuotePower, int? laborQuoteSecurity, int? laborQuoteElectrical, string[] laborQuoteProduct, string laborQuoteManufacturer,
			string laborQuoteSeries, string laborQuoteWorkstations, string laborQuotePanels, string laborQuotePrivOffices, string laborQuoteConfRooms,
			string laborQuoteExamRooms, string laborQuoteClassRooms, string laborQuoteOtherAncillary, string[] laborQuoteProductIs, string laborQuoteAddInfo,
			string[] laborQuoteTypeOfScreening, int? laborQuoteElevatorNeeded, int? laborQuoteProductCleaned, string laborQuoteRevisionReason, 
			bool? laborQuoteYulioPresentation, bool? laborQuoteOutOfStateProject, int? laborQuoteFloorsIncluded, int? laborQuoteScenesPerFloor,
			string laborQuoteRequestorEmailCC1, string laborQuoteRequestorEmailCC2, string draft)
		{
            try
            {            
				var region = "OWP";
				var laborQuoteTypeList = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("Type", region);
				var customerList = _db.LaborQuoteAutomationHandler.LoadCustomerList(region);
				var salespersonList = _db.LaborQuoteAutomationHandler.LoadSalespersonList(region);
				var unionVendorList = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("UnionVendor", region);
				var receiveDeliverList = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("ReceiveDeliver", region);
				var receiveDeliverTimeList = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("DayTimeReceiveDeliver", region);
				var installationList = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("Installation", region);
				var installationTimeList = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("DayTimeInstallation", region);
				var productFromList = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("ProductFrom", region);
				var unloadAtList = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("UnloadAt", region);
				var siteIsList = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("SiteIs", region);
				var elevatorList = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("Elevator", region);
				var powerList = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("Power", region);
				var productTypeList = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("ProductType", region);
				var productIsList = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("ProductIs", region);
				var wallsAreList = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("WallsAre", region);
				var protectionList = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("Protection", region);
				var typeOfScreeningList = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("TypeOfScreening", region);
				var scenesPerFloor = _db.LaborQuoteAutomationHandler.LoadLookupGeneral("ScenesPerFloor", region);

				ViewData["IsRevision"] = false;
				ViewData["IsReadOnly"] = false;

				LaborQuoteAutomationViewModel vm = new LaborQuoteAutomationViewModel
				{
					LaborQuoteTypes = laborQuoteTypeList,
					Customers = customerList,
					Salespersons = salespersonList,
					LaborQuoteUnionVendors = unionVendorList,
					IsRevision = false,
					Success = "",
					LaborQuote = new LaborQuoteHeader(),
					OriginalLaborQuoteNo = originalLaborQuoteNo,
					ReceiveDeliverList = receiveDeliverList,
					ReceiveDeliverTimeList = receiveDeliverTimeList,
					InstallationList = installationList,
					InstallationTimeList = installationTimeList,
					ProductFromList = productFromList,
					UnloadAtList = unloadAtList,
					SiteIsList = siteIsList,
					ElevatorList = elevatorList,
					PowerList = powerList,
					ProductTypeList = productTypeList,
					ProductIsList = productIsList,
					WallsAreList = wallsAreList,
					ProtectionList = protectionList,
					TypeOfScreeningList = typeOfScreeningList,
					ScenesPerFloor = scenesPerFloor
				};

				if (!string.IsNullOrEmpty(submit))
				{
					switch (submit)
					{
						case "SendLaborQuote":
							string code = !string.IsNullOrEmpty(revisionCode) ? revisionCode : _db.LaborQuoteAutomationHandler.GetLaborQuoteCode(region);
							string subject = GetFormattedSubject(laborQuoteTypeList, laborQuoteTypes, code);
							string body = GetFormattedBody(code, isRevision, revisionCode, laborQuoteTypes, laborQuoteRequestorName, laborQuoteRequestorEmail, laborQuoteHedbergProjectID,
										laborQuoteCustomer, laborQuoteStreetAddress, laborQuoteBRF, laborQuoteCity, laborQuoteOrOrder, laborQuoteSalesRep,
										laborQuoteCoordinator, laborQuoteProjectManager, laborQuoteDirectoryCreated, laborQuoteNoDaysForInstall,
										originalLaborQuoteNo, quoteOrOrderNumber, laborQuoteProjectIs, laborQuoteUnionVendors, laborQuoteUnionVendorsADD, laborQuoteTargetDate.ToString("MM/dd/yyyy"),
										laborQuoteSupplyRequestorEmail, laborQuoteScopeOfWork, laborQuoteDeliver, laborQuoteDeliverTime, laborQuoteInstallation,
										laborQuoteInstallationTime, laborQuoteProductForm, laborQuoteUnloadAt, laborQuoteSiteIs, laborQuoteWallsAre,
										laborQuoteProtection, laborQuoteFeetMasonite, laborQuoteElevator, laborQuoteCarryUp, laborQuoteNoOfFlights,
										laborQuotePower, laborQuoteSecurity, laborQuoteElectrical, laborQuoteProduct, laborQuoteManufacturer,
										laborQuoteSeries, laborQuoteWorkstations, laborQuotePanels, laborQuotePrivOffices, laborQuoteConfRooms,
										laborQuoteExamRooms, laborQuoteClassRooms, laborQuoteOtherAncillary, laborQuoteProductIs, laborQuoteAddInfo,
										laborQuoteTypeOfScreening, laborQuoteElevatorNeeded, laborQuoteProductCleaned, laborQuoteRevisionReason, laborQuoteYulioPresentation, 
										laborQuoteOutOfStateProject, laborQuoteFloorsIncluded, laborQuoteScenesPerFloor, laborQuoteRequestorEmailCC1, laborQuoteRequestorEmailCC2);

							List<IFormFile> files = new List<IFormFile>();
						
							foreach (var item in Request.Form.Files)
							{
								if (item != null) files.Add(item);
							}

							foreach (var file in files)
							{
								if (file != null || file.Length > 0)
								{
									using (var stream = new FileStream("wwwroot/files/" + code + "_" + file.FileName, FileMode.Create, FileAccess.ReadWrite))
									{
										await file.CopyToAsync(stream);
									}
								}
							}

							EmailProperties email = new EmailProperties();
							email.Attachments = files.ToArray();
							List<string> tos = new List<string>();
							bool flag = false;
							if (laborQuoteUnionVendors.Count() > 0)
							{
								
								foreach (var item in laborQuoteUnionVendors)
								{
									if (item != "ADD")
									{
										flag = true;
										string vendorEmail = _db.LaborQuoteAutomationHandler.GetVendorEmail(item);
										if (vendorEmail != "")
										{
											tos.Add(vendorEmail);
										}
									} 
									else
									{
										if (laborQuoteUnionVendorsADD != "")
										{
											tos.Add(laborQuoteUnionVendorsADD);
										}
									}							
								}
								email.To = tos.ToArray();
							} 
							else
							{
								email.To = new string[] { @"theestimateteam@oneworkplace.com" };
							}
							if (!(flag && laborQuoteTypes.Contains(laborQuoteTypeList.Where(x => x.Value == "InstallDeliver").Select(x => x.LookupGeneralID).FirstOrDefault()) && laborQuoteTypes.Contains(laborQuoteTypeList.Where(x => x.Value == "ProjMan").Select(x => x.LookupGeneralID).FirstOrDefault())))
                            {
								flag = false;
                            }
							email.CC = (laborQuoteRequestorEmailCC1?.Replace(",", ";") + ";" + laborQuoteRequestorEmailCC2 + ";" + laborQuoteRequestorEmail + ";" + (flag ? "theestimateteam@oneworkplace.com" : "")).Split(';', StringSplitOptions.RemoveEmptyEntries);
							email.Subject = subject;
							email.Body = body;
							email.IsHTMLBody = true;

							try
							{
								var status = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("Status", region).Where(y => y.Value == "Pending").Select(y => y.LookupGeneralID).FirstOrDefault();
								_db.LaborQuoteAutomationHandler.CreateLaborQuoteHeader(code, laborQuoteTypes, laborQuoteRequestorName, laborQuoteRequestorEmail,
								laborQuoteHedbergProjectID, laborQuoteCustomer, laborQuoteStreetAddress, laborQuoteBRF, laborQuoteCity, laborQuoteOrOrder,
								laborQuoteSalesRep, laborQuoteCoordinator, laborQuoteProjectManager, laborQuoteDirectoryCreated, laborQuoteNoDaysForInstall,
								originalLaborQuoteNo, quoteOrOrderNumber, laborQuoteProjectIs, unionVendorList, laborQuoteUnionVendors, laborQuoteUnionVendorsADD, laborQuoteTargetDate,
								laborQuoteSupplyRequestorEmail, laborQuoteScopeOfWork, laborQuoteDeliver, laborQuoteDeliverTime, laborQuoteInstallation,
								laborQuoteInstallationTime, laborQuoteProductForm, laborQuoteUnloadAt, laborQuoteSiteIs, laborQuoteWallsAre,
								laborQuoteProtection, laborQuoteFeetMasonite, laborQuoteElevator, laborQuoteCarryUp, laborQuoteNoOfFlights,
								laborQuotePower, laborQuoteSecurity, laborQuoteElectrical, laborQuoteProduct, laborQuoteManufacturer,
								laborQuoteSeries, laborQuoteWorkstations, laborQuotePanels, laborQuotePrivOffices, laborQuoteConfRooms,
								laborQuoteExamRooms, laborQuoteClassRooms, laborQuoteOtherAncillary, laborQuoteProductIs, laborQuoteAddInfo,
								laborQuoteTypeOfScreening, laborQuoteElevatorNeeded, laborQuoteProductCleaned, laborQuoteRevisionReason,
								laborQuoteYulioPresentation, laborQuoteOutOfStateProject, laborQuoteFloorsIncluded, laborQuoteScenesPerFloor,
								laborQuoteRequestorEmailCC1, laborQuoteRequestorEmailCC2, status);

								_emailHelper.SendEmailV2(_configuration, email, "OQF");

								vm.Success = "success";
							}
							catch (Exception ex)
							{
								vm.Success = "error";
								throw ex;
							}

							break;

						case "CreateRevision":
							vm.IsRevision = true;
							ViewData["IsRevision"] = true;
							vm.RevisionCode = _db.LaborQuoteAutomationHandler.GetLaborQuoteCode(region);
							vm = LoadLaborQuote(vm, originalLaborQuoteNo);
							break;
					}
				}

				//READONLY MODE
				if (!string.IsNullOrEmpty(laborQuoteCode))
				{
					ViewData["IsReadOnly"] = true;
					vm.IsReadOnly = true;

					vm = LoadLaborQuote(vm, laborQuoteCode);

					//ATTACHED FILES
					string codeName = laborQuoteCode;
					DirectoryInfo di = new DirectoryInfo(@"wwwroot/files");
					FileInfo[] fis = di.GetFiles(codeName + "*.*");
					if (fis?.Count() > 0)
					{
						vm.LaborQuote.AttachedFiles = new List<AttachedFile>();
						foreach (var f in fis)
						{
							vm.LaborQuote.AttachedFiles.Add(new AttachedFile() { FullPath = f.FullName, FileName = f.Name.Replace(codeName + "_", ""), FullFileName = f.Name });
						}
					}
				}

				return View(vm);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		[Route("/OneQuoteForm/GetOriginalLaborQuotes")]
		public JsonResult GetOriginalLaborQuotes()
		{
			return new JsonResult(_db.LaborQuoteAutomationHandler.GetOriginalLaborQuotes("OWP"));
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

		[Route("/OneQuoteForm/GetLaborQuoteAddress")]
		public JsonResult GetLaborQuoteAddress(int orderNo, char orderType)
		{
			return new JsonResult(_db.LaborQuoteAutomationHandler.GetLaborQuoteAddress(orderNo, orderType, "OWP"));
		}

		private LaborQuoteAutomationViewModel LoadLaborQuote(LaborQuoteAutomationViewModel vm, string laborQuoteCode)
		{
			vm.LaborQuote = _db.LaborQuoteAutomationHandler.GetLaborQuoteHeaders(null, null, laborQuoteCode, null, null, null, null, null, null, null, "OWP").FirstOrDefault();

			if (vm.LaborQuote.LaborQuoteTypeLookUpIDs != null)
				vm.LaborQuote.LaborQuoteTypes = vm.LaborQuote.LaborQuoteTypeLookUpIDs.Split(',');
			if (vm.LaborQuote.LaborQuoteUnionVendorIDs != null)
				vm.LaborQuote.LaborQuoteUnionVendors = vm.LaborQuote.LaborQuoteUnionVendorIDs.Split(',');
			if (vm.LaborQuote.LaborQuoteProductFromLookUpIDs != null)
				vm.LaborQuote.LaborQuoteProductFroms = vm.LaborQuote.LaborQuoteProductFromLookUpIDs.Split(',');
			if (vm.LaborQuote.LaborQuoteWallsAreLookUpIDs != null)
				vm.LaborQuote.LaborQuoteWallsAres = vm.LaborQuote.LaborQuoteWallsAreLookUpIDs.Split(',');
			if (vm.LaborQuote.LaborQuoteProtectionLookUpIDs != null)
				vm.LaborQuote.LaborQuoteProtections = vm.LaborQuote.LaborQuoteProtectionLookUpIDs.Split(',');
			if (vm.LaborQuote.LaborQuoteElevatorLookUpIDs != null)
				vm.LaborQuote.LaborQuoteElevators = vm.LaborQuote.LaborQuoteElevatorLookUpIDs.Split(',');
			if (vm.LaborQuote.LaborQuotePowerLookUpIDs != null)
				vm.LaborQuote.LaborQuotePowers = vm.LaborQuote.LaborQuotePowerLookUpIDs.Split(',');
			if (vm.LaborQuote.LaborQuoteProductTypeLookUpIDs != null)
				vm.LaborQuote.LaborQuoteProductTypes = vm.LaborQuote.LaborQuoteProductTypeLookUpIDs.Split(',');
			if (vm.LaborQuote.LaborQuoteProductIsLookUpIDs != null)
				vm.LaborQuote.LaborQuoteProductIs = vm.LaborQuote.LaborQuoteProductIsLookUpIDs.Split(',');
			if (vm.LaborQuote.LaborQuoteTypeOfScreeningLookUpIDs != null)
				vm.LaborQuote.LaborQuoteTypeOfScreenings = vm.LaborQuote.LaborQuoteTypeOfScreeningLookUpIDs.Split(',');

			return vm;
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

		private string GetFormattedSubject(List<LookupGeneralWithValue> types, string[] selectedTypes, string code)
		{
			string ret = "New submission from One Quote Form | Labor Quote #" + code;

			if (selectedTypes.Count() > 0)
			{
				ret += " - for ";

				foreach (var type in types)
				{
					if (selectedTypes.Contains(type.LookupGeneralID))
					{
						ret += type.TranslateValue + ", ";
					}
				}

				ret = ret.Remove(ret.Length - 2);
			}

			return ret;
		}

		private string GetFormattedBody(string code, bool isRevision, string revisionCode, string[] laborQuoteTypes, string laborQuoteRequestorName, string laborQuoteRequestorEmail, string laborQuoteHedbergProjectID,
			string laborQuoteCustomer, string laborQuoteStreetAddress, string laborQuoteBRF, string laborQuoteCity, char laborQuoteOrOrder,
			string laborQuoteSalesRep, string laborQuoteCoordinator, string laborQuoteProjectManager, int laborQuoteDirectoryCreated, string laborQuoteNoDaysForInstall,
			string originalLaborQuoteNo, string quoteOrOrderNumber, char laborQuoteProjectIs, string[] laborQuoteUnionVendors, string laborQuoteUnionVendorsADD, string laborQuoteTargetDate,
			string laborQuoteSupplyRequestorEmail, string laborQuoteScopeOfWork, int? laborQuoteDeliver, int? laborQuoteDeliverTime, int? laborQuoteInstallation,
			int? laborQuoteInstallationTime, string[] laborQuoteProductForm, int? laborQuoteUnloadAt, int? laborQuoteSiteIs, string[] laborQuoteWallsAre,
			string[] laborQuoteProtection, string laborQuoteFeetMasonite, string[] laborQuoteElevator, int? laborQuoteCarryUp, int? laborQuoteNoOfFlights,
			string[] laborQuotePower, int? laborQuoteSecurity, int? laborQuoteElectrical, string[] laborQuoteProduct, string laborQuoteManufacturer,
			string laborQuoteSeries, string laborQuoteWorkstations, string laborQuotePanels, string laborQuotePrivOffices, string laborQuoteConfRooms,
			string laborQuoteExamRooms, string laborQuoteClassRooms, string laborQuoteOtherAncillary, string[] laborQuoteProductIs, string laborQuoteAddInfo,
			string[] laborQuoteTypeOfScreening, int? laborQuoteElevatorNeeded, int? laborQuoteProductCleaned, string laborQuoteRevisionReason,
			bool? laborQuoteYulioPresentation, bool? laborQuoteOutOfStateProject, int? laborQuoteFloorsIncluded, int? laborQuoteScenesPerFloor,
			string laborQuoteRequestorEmailCC1, string laborQuoteRequestorEmailCC2)
		{
			string template =
			@"<html>
            <body>
            <p><strong>Please do not reply to this email. If you have any questions please direct your email to the requester noted in the form results below.</strong><p>
            <br/>
			##UNIONQUOTE##
            ##REVISIONTITLE##
            ##TYPES##
			##YULIO##
            <p><strong>Requestor Name</strong></p>
            ##REQUESTORNAME##
            <p><strong>Requestor Email</strong></p>
            ##REQUESTOREMAIL##
			##CC1##
			##CC2##
            <p><strong>Hedberg Project ID</strong></p>
			##HEDBERGPROJECTID##
            ##ORIGINALLABORQUOTE##
            <p><strong>Customer</strong></p>
            ##CUSTOMER##
            <p><strong>Job Address</strong></p>
            ##STREETADDRESS##
            <p><strong>Is this a Quote or Order?</strong></p>
            ##QUOTEORORDER##
            ##QUOTEORORDERNUMBER##
            <p><strong>Sales Rep</strong></p>
            ##SALESREP##
            <p><strong>Coordinator</strong></p>
            ##COORDINATOR##
            ##PROJECTMANAGER##
            <p><strong>Directory Created?</strong></p>
            ##DIRECTORYCREATED##
            ##NODAYSFORINSTALL##
            <p><strong>Target Install Date</strong></p>
            ##TARGETDATE##
            <p><strong>Project Is:</strong></p>
            ##PROJECTIS##
            ##UNIONVENDORS##
            ##SUPPLYREQUESTOREMAIL##
            <p><strong>Scope of Work</strong></p>
			<pre>
            ##SCOPEOFWORK##
			</pre>
            ##DELIVER##
            ##DELIVERTIME##
            ##INSTALLATION##
            ##INSTALLATIONTIME##
            ##PRODUCTFORM##
            ##UNLOADAT##
            ##TYPEOFSCREENING##
            ##ELEVATORNEEDED##
            ##PRODUCTCLEANED##
            ##SITEIS##
            ##WALLSARE##
            ##PROTECTION##
            ##ELEVATOR##
            ##CARRYUP##
            ##POWER##
            ##SECURITY##
            ##ELECTRICAL##
            ##PRODUCT##
            ##MANUFACTURER##
            ##SERIES##
            ##WORKSTATIONS##
            ##PANELSONLY##
            ##PRIVATEOFFICES##
            ##CONFERENCEROOMS##
            ##EXAMROOMS##
            ##CLASSROOMS##
            ##OTHER##
            ##PRODUCTIS##
            ##ADDITIONALINFO##
			##REVISIONREASON##
            </body>
            </html>";

			if (isRevision)
			{
				template = template.Replace("##REVISIONTITLE##", "<h3>Revision of Original Labor Quote " + revisionCode + "</h3>");
			}
			else
			{
				template = template.Replace("##REVISIONTITLE##", "");
			}
			if (laborQuoteTypes.Count() > 0)
			{
				template = template.Replace("##TYPES##", GetTypes(laborQuoteTypes));
			}
			else
			{
				template = template.Replace("##TYPES##", "");
			}
			if (laborQuoteYulioPresentation != null)
            {
                string tempTemplate = "<p><strong>Does this project require a Yulio Presentation?</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + (laborQuoteYulioPresentation == true ? "Yes" : "No") + "</p>";
                if (laborQuoteYulioPresentation == true)
                {
                    tempTemplate += "<p><strong>Is this an out-of-state project?</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + (laborQuoteOutOfStateProject == true ? "Yes" : "No") + "</p>";
				}
				tempTemplate += "<p><strong>Floors Included</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteFloorsIncluded.ToString() + "</p>";
				tempTemplate += GetLookupGeneralValues(laborQuoteScenesPerFloor, "ScenesPerFloor", "Scenes Per Floor");
				template = template.Replace("##YULIO##", tempTemplate);
			}
			else
            {
				template = template.Replace("##YULIO##", "");
			}
			template = template.Replace("##REQUESTORNAME##", "<p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteRequestorName + "</p>");
			template = template.Replace("##REQUESTOREMAIL##", "<p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteRequestorEmail + "</p>");

			if (!string.IsNullOrEmpty(laborQuoteRequestorEmailCC1))
			{
				template = template.Replace("##CC1##", "<p><strong>CC</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteRequestorEmailCC1 + "</p>");
			}
			else
			{
				template = template.Replace("##CC1##", "");
			}
			if (!string.IsNullOrEmpty(laborQuoteRequestorEmailCC2))
			{
				template = template.Replace("##CC2##", "<p><strong>CC</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteRequestorEmailCC2 + "</p>");
			}
			else
			{
				template = template.Replace("##CC2##", "");
			}
			if (isRevision)
			{
				template = template.Replace("##ORIGINALLABORQUOTE##", "<h3>Original Labor Quote # " + originalLaborQuoteNo + "</h3>");
			}
			else
			{
				template = template.Replace("##ORIGINALLABORQUOTE##", "");
			}
			template = template.Replace("##HEDBERGPROJECTID##", "<p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteHedbergProjectID + "</p>");
			template = template.Replace("##CUSTOMER##", "<p style='margin-left: 20px;margin-top: -10px;'>" + GetCustomerName(laborQuoteCustomer) + "</p>");
			template = template.Replace("##STREETADDRESS##", "<p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteStreetAddress + " - " + laborQuoteBRF + " - " + laborQuoteCity + "</p>");
			template = template.Replace("##QUOTEORORDER##", "<p style='margin-left: 20px;margin-top: -10px;'>" + (laborQuoteOrOrder == 'Q' ? "Quote" : "Order") + "</p>");
			template = template.Replace("##QUOTEORORDERNUMBER##", "<p><strong>" + (laborQuoteOrOrder == 'Q' ? "Quote" : "Order") + " Number</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + quoteOrOrderNumber + "</p>");
			template = template.Replace("##SALESREP##", "<p style='margin-left: 20px;margin-top: -10px;'>" + GetSalesRepName(laborQuoteSalesRep) + "</p>");
			//template = template.Replace("##COORDINATOR##", "<p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteCoordinator + "</p>");
			template = template.Replace("##COORDINATOR##", "");
			if (!string.IsNullOrEmpty(laborQuoteProjectManager))
			{
				template = template.Replace("##PROJECTMANAGER##", "<p><strong>Project Manager</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteProjectManager + "</p>");
			}
			else
			{
				template = template.Replace("##PROJECTMANAGER##", "");
			}
			template = template.Replace("##PROJECTMANAGER##", "<p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteProjectManager + "</p>");
			template = template.Replace("##DIRECTORYCREATED##", "<p style='margin-left: 20px;margin-top: -10px;'>" + (laborQuoteDirectoryCreated == 1 ? "Yes" : "No") + "</p>");
			if (!string.IsNullOrEmpty(laborQuoteNoDaysForInstall))
			{
				template = template.Replace("##NODAYSFORINSTALL##", "<p><strong># of Days for Install</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteNoDaysForInstall + "</p>");
			}
			else
			{
				template = template.Replace("##NODAYSFORINSTALL##", "");
			}
			template = template.Replace("##TARGETDATE##", "<p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteTargetDate + "</p>");
			template = template.Replace("##PROJECTIS##", "<p style='margin-left: 20px;margin-top: -10px;'>" + (laborQuoteProjectIs == 'N' ? "Non-Union" : (laborQuoteProjectIs == 'U' ? "Union" : (laborQuoteProjectIs == 'E' ? "Out of State" : "Prevailing Wage"))) + "</p>");
			if (laborQuoteUnionVendors.Count() > 0)
			{
				template = template.Replace("##UNIONQUOTE##", "<p><strong>Union Labor Quote #: "+ code +"</strong></p>");
				template = template.Replace("##UNIONVENDORS##", GetUnionVendors(laborQuoteUnionVendors, laborQuoteUnionVendorsADD));
			}
			else
			{
				template = template.Replace("##UNIONQUOTE##", "");
				template = template.Replace("##UNIONVENDORS##", "");
			}
			if (!string.IsNullOrEmpty(laborQuoteSupplyRequestorEmail))
			{
				template = template.Replace("##SUPPLYREQUESTOREMAIL##", "<p><strong>Union Jobs Email Address</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteSupplyRequestorEmail + "</p>");
			}
			else
			{
				template = template.Replace("##SUPPLYREQUESTOREMAIL##", "");
			}
			template = template.Replace("##SCOPEOFWORK##", "<p style='margin-left: 20px;margin-top: -10px;'><pre>" + laborQuoteScopeOfWork + "</pre></p>");
			if (laborQuoteDeliver != null)
			{
				template = template.Replace("##DELIVER##", GetLookupGeneralValues(laborQuoteDeliver, "ReceiveDeliver", "Receive/Deliver"));
			}
			else
			{
				template = template.Replace("##DELIVER##", "");
			}
			if (laborQuoteDeliverTime != null)
			{
				template = template.Replace("##DELIVERTIME##", GetLookupGeneralValues(laborQuoteDeliverTime, "DayTimeReceiveDeliver", "Monday/Friday Time"));
			}
			else
			{
				template = template.Replace("##DELIVERTIME##", "");
			}
			if (laborQuoteInstallation != null)
			{
				template = template.Replace("##INSTALLATION##", GetLookupGeneralValues(laborQuoteInstallation, "Installation", "Installation"));
			}
			else
			{
				template = template.Replace("##INSTALLATION##", "");
			}
			if (laborQuoteInstallationTime != null)
			{
				template = template.Replace("##INSTALLATIONTIME##", GetLookupGeneralValues(laborQuoteInstallationTime, "DayTimeInstallation", "Installation"));
			}
			else
			{
				template = template.Replace("##INSTALLATIONTIME##", "");
			}
			if (laborQuoteProductForm.Count() > 0)
			{
				template = template.Replace("##PRODUCTFORM##", GetLookupGeneralValues(laborQuoteProductForm, "ProductFrom", "Product Form"));
			}
			else
			{
				template = template.Replace("##PRODUCTFORM##", "");
			}
			if (laborQuoteUnloadAt != null)
			{
				template = template.Replace("##UNLOADAT##", GetLookupGeneralValues(laborQuoteUnloadAt, "UnloadAt", "Unload At"));
			}
			else
			{
				template = template.Replace("##UNLOADAT##", "");
			}
			if (laborQuoteTypeOfScreening.Count() > 0)
			{
				template = template.Replace("##TYPEOFSCREENING##", GetTypeOfScreenings(laborQuoteTypeOfScreening));
			}
			else
			{
				template = template.Replace("##TYPEOFSCREENING##", "");
			}
			if (laborQuoteElevatorNeeded != null)
			{
				template = template.Replace("##ELEVATORNEEDED##", "<p><strong>Will elevator be needed during install?</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + (laborQuoteElevatorNeeded == 1 ? "Yes" : "No") + "</p>");
			}
			else
			{
				template = template.Replace("##ELEVATORNEEDED##", "");
			}
			if (laborQuoteProductCleaned != null)
			{
				template = template.Replace("##PRODUCTCLEANED##", "<p><strong>Does product need to be cleaned/sanitized?</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + (laborQuoteProductCleaned == 1 ? "Yes" : "No") + "</p>");
			}
			else
			{
				template = template.Replace("##PRODUCTCLEANED##", "");
			}
			if (laborQuoteSiteIs != null)
			{
				template = template.Replace("##SITEIS##", GetLookupGeneralValues(laborQuoteSiteIs, "SiteIs", "Site Is"));
			}
			else
			{
				template = template.Replace("##SITEIS##", "");
			}
			if (laborQuoteWallsAre.Count() > 0)
			{
				template = template.Replace("##WALLSARE##", GetLookupGeneralValues(laborQuoteWallsAre, "WallsAre", "Walls Are"));
			}
			else
			{
				template = template.Replace("##WALLSARE##", "");
			}
			if (laborQuoteProtection.Count() > 0)
			{
				template = template.Replace("##PROTECTION##", GetLookupGeneralValues(laborQuoteProtection, "Protection", "Protection"));
			}
			else
			{
				template = template.Replace("##PROTECTION##", "");
			}
			if (laborQuoteElevator.Count() > 0)
			{
				template = template.Replace("##ELEVATOR##", GetLookupGeneralValues(laborQuoteElevator, "Elevator", "Elevator"));
			}
			else
			{
				template = template.Replace("##ELEVATOR##", "");
			}
			if (laborQuoteCarryUp != null)
			{
				template = template.Replace("##CARRYUP##", "<p><strong>Carry Up</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + (laborQuoteCarryUp == 1 ? "Yes" : "No") + "</p>");
			}
			else
			{
				template = template.Replace("##CARRYUP##", "");
			}
			if (laborQuotePower.Count() > 0)
			{
				template = template.Replace("##POWER##", GetLookupGeneralValues(laborQuotePower, "Power", "Power"));
			}
			else
			{
				template = template.Replace("##POWER##", "");
			}
			if (laborQuoteSecurity != null)
			{
				template = template.Replace("##SECURITY##", "<p><strong>Security Clearance Required?</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + (laborQuoteSecurity == 1 ? "Yes" : "No") + "</p>");
			}
			else
			{
				template = template.Replace("##SECURITY##", "");
			}
			if (laborQuoteElectrical != null)
			{
				template = template.Replace("##ELECTRICAL##", "<p><strong>Electrical Permit Required?</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + (laborQuoteElectrical == 1 ? "Yes" : "No") + "</p>");
			}
			else
			{
				template = template.Replace("##ELECTRICAL##", "");
			}
			if (laborQuoteProduct.Count() > 0)
			{
				template = template.Replace("##PRODUCT##", GetLookupGeneralValues(laborQuoteProduct, "ProductType", "Product"));
			}
			else
			{
				template = template.Replace("##PRODUCT##", "");
			}
			if (!string.IsNullOrEmpty(laborQuoteManufacturer))
			{
				template = template.Replace("##MANUFACTURER##", "<p><strong>Manufacturer</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteManufacturer + "</p>");
			}
			else
			{
				template = template.Replace("##MANUFACTURER##", "");
			}
			if (!string.IsNullOrEmpty(laborQuoteSeries))
			{
				template = template.Replace("##SERIES##", "<p><strong>Project Manager</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteSeries + "</p>");
			}
			else
			{
				template = template.Replace("##SERIES##", "");
			}
			if (!string.IsNullOrEmpty(laborQuoteWorkstations))
			{
				template = template.Replace("##WORKSTATIONS##", "<p><strong># Workstations</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteWorkstations + "</p>");
			}
			else
			{
				template = template.Replace("##WORKSTATIONS##", "");
			}
			if (!string.IsNullOrEmpty(laborQuotePanels))
			{
				template = template.Replace("##PANELSONLY##", "<p><strong># Panels Only</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + laborQuotePanels + "</p>");
			}
			else
			{
				template = template.Replace("##PANELSONLY##", "");
			}
			if (!string.IsNullOrEmpty(laborQuotePrivOffices))
			{
				template = template.Replace("##PRIVATEOFFICES##", "<p><strong># Private Offices</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + laborQuotePrivOffices + "</p>");
			}
			else
			{
				template = template.Replace("##PRIVATEOFFICES##", "");
			}
			if (!string.IsNullOrEmpty(laborQuoteConfRooms))
			{
				template = template.Replace("##CONFERENCEROOMS##", "<p><strong># Conference Rooms</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteConfRooms + "</p>");
			}
			else
			{
				template = template.Replace("##CONFERENCEROOMS##", "");
			}
			if (!string.IsNullOrEmpty(laborQuoteExamRooms))
			{
				template = template.Replace("##EXAMROOMS##", "<p><strong>Project Manager</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteExamRooms + "</p>");
			}
			else
			{
				template = template.Replace("##EXAMROOMS##", "");
			}
			if (!string.IsNullOrEmpty(laborQuoteClassRooms))
			{
				template = template.Replace("##CLASSROOMS##", "<p><strong># Class Rooms</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteClassRooms + "</p>");
			}
			else
			{
				template = template.Replace("##CLASSROOMS##", "");
			}
			if (!string.IsNullOrEmpty(laborQuoteOtherAncillary))
			{
				template = template.Replace("##OTHER##", "<p><strong>Other (Ancillary)</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteOtherAncillary + "</p>");
			}
			else
			{
				template = template.Replace("##OTHER##", "");
			}
			if (laborQuoteProductIs.Count() > 0)
			{
				template = template.Replace("##PRODUCTIS##", GetLookupGeneralValues(laborQuoteProductIs, "ProductIs", "Product Is"));
			}
			else
			{
				template = template.Replace("##PRODUCTIS##", "");
			}
			if (!string.IsNullOrEmpty(laborQuoteAddInfo))
			{
				template = template.Replace("##ADDITIONALINFO##", "<p><strong>Additional Info</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteAddInfo + "</p>");
			}
			else
			{
				template = template.Replace("##ADDITIONALINFO##", "");
			}
			if (isRevision)
			{
				template = template.Replace("##REVISIONREASON##", "<p><strong>Revision Reason</strong></p><p style='margin-left: 20px;margin-top: -10px;'><pre>" + laborQuoteRevisionReason + "</pre></p>");
			}
			else
			{
				template = template.Replace("##REVISIONREASON##", "");
			}

			return template;
		}

		private string GetTypes(string[] types)
		{
			var laborQuoteTypeList = _db.LaborQuoteAutomationHandler.LoadLookupGeneral("Type", "OWP");
			string ret = "<p><strong>Do you need a labor quote for:</strong></p>";

			foreach (var type in types)
			{
				string typeText = laborQuoteTypeList.Where(x => x.LookupGeneralID == type).Select(x => x.Value).FirstOrDefault();
				ret += "<p style='margin-left: 20px;margin-top: -10px;'>" + typeText + "</p>";
			}

			return ret;
		}

		private string GetUnionVendors(string[] vendors, string add)
		{
			var laborQuoteUnionVendorList = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("UnionVendor", "OWP");
			string ret = "<p><strong>Please select the Union Vendors:</strong></p>";

			foreach (var vendor in vendors)
			{
				string vendorText = laborQuoteUnionVendorList.Where(x => x.Value == vendor).Select(x => x.TranslateValue).FirstOrDefault();
				ret += "<p style='margin-left: 20px;margin-top: -10px;'>" + vendorText + "</p>";
				if (vendor == "ADD")
                {
					ret += "<p style='margin-left: 20px;margin-top: -10px;'>" + add + "</p>";
				}
			}

			return ret;
		}

		private string GetTypeOfScreenings(string[] typeOfScreenings)
		{
			var laborQuoteTypeOfScreeningList = _db.LaborQuoteAutomationHandler.LoadLookupGeneral("TypeOfScreening", "OWP");
			string ret = "<p><strong>Type of Screening</strong></p>";

			foreach (var tos in typeOfScreenings)
			{
				string tosText = laborQuoteTypeOfScreeningList.Where(x => x.LookupGeneralID == tos).Select(x => x.Value).FirstOrDefault();
				ret += "<p style='margin-left: 20px;margin-top: -10px;'>" + tosText + "</p>";
			}

			return ret;
		}

		private string GetCustomerName(string customer)
		{
			var customerList = _db.LaborQuoteAutomationHandler.LoadCustomerList("OWP");
			return customerList.Where(x => x.ID == customer).Select(x => x.Label).FirstOrDefault(); ;
		}

		private string GetSalesRepName(string salesRep)
		{
			var salespersonList = _db.LaborQuoteAutomationHandler.LoadSalespersonList("OWP");
			return salespersonList.Where(x => x.ID == salesRep).Select(x => x.Label).FirstOrDefault();
		}

		private string GetLookupGeneralValues(string[] values, string name, string title)
		{
			var list = _db.LaborQuoteAutomationHandler.LoadLookupGeneral(name, "OWP");
			string ret = "<p><strong>" + title + "</strong></p>";

			foreach (var val in values)
			{
				string text = list.Where(x => x.LookupGeneralID == val).Select(x => x.Value).FirstOrDefault();
				ret += "<p style='margin-left: 20px;margin-top: -10px;'>" + text + "</p>";
			}

			return ret;
		}

		private string GetLookupGeneralValues(int? value, string name, string title)
		{
			var list = _db.LaborQuoteAutomationHandler.LoadLookupGeneral(name, "OWP");
			string ret = "<p><strong>" + title + "</strong></p>";
			string text = list.Where(x => x.LookupGeneralID == value?.ToString()).Select(x => x.Value).FirstOrDefault();
			ret += "<p style='margin-left: 20px;margin-top: -10px;'>" + text + "</p>";

			return ret;
		}

		[Route("/OneQuoteForm/GetProjectData")]
		public JsonResult GetProjectData(string projectId)
		{
			return new JsonResult(_db.LaborQuoteAutomationHandler.GetProjectData(projectId, "OWP"));
		}
	}
}