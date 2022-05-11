using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using OWPApplications.Data;
using OWPApplications.Models;
using OWPApplications.Models.LaborQuoteAutomation;
using OWPApplications.Utils;
using static OWPApplications.Utils.EmailHelper;
using OWPApplications.Models.LaborQuoteAutomationCommons;

namespace OWPApplications.Controllers
{
	public class LaborQuoteRequestFormController : Controller
	{
		readonly DbHandler _db;
		readonly IConfiguration _configuration;
		readonly EmailHelper _emailHelper;
		ILogger _logger;


		public LaborQuoteRequestFormController(DbHandler dbHandler, IConfiguration configuration, EmailHelper emailHelper, ILoggerFactory logFactory)
		{
			_db = dbHandler;
			_configuration = configuration;
			_emailHelper = emailHelper;
			_logger = logFactory.CreateLogger(nameof(LaborQuoteRequestFormController));

		}

		public async Task<IActionResult> Index(string submit, string draft, bool isRevision, string revisionCode, string[] laborQuoteTypes, string laborQuoteRequestorName,
			string laborQuoteRequestorEmail, string laborQuoteHedbergProjectID, string laborQuoteCustomer, string laborQuoteStreetAddress, string laborQuoteBRF,
			string laborQuoteCity, char laborQuoteOrOrder, string laborQuoteSalesRep, string laborQuoteCoordinator, string laborQuoteProjectManager,
			int laborQuoteDirectoryCreated, string laborQuoteNoDaysForInstall, string originalLaborQuoteNo, string quoteOrOrderNumber,
			char laborQuoteProjectIs, string[] laborQuoteUnionVendors, DateTime? laborQuoteTargetDate, string laborQuoteSupplyRequestorEmail,
			string laborQuoteScopeOfWork, string laborQuoteCode, int? laborQuoteDeliver, int? laborQuoteDeliverTime, int? laborQuoteInstallation,
			int? laborQuoteInstallationTime, string[] laborQuoteProductForm, int? laborQuoteUnloadAt, int? laborQuoteSiteIs, string[] laborQuoteWallsAre,
			string[] laborQuoteProtection, string laborQuoteFeetMasonite, string[] laborQuoteElevator, int? laborQuoteCarryUp, int? laborQuoteNoOfFlights,
			string[] laborQuotePower, int? laborQuoteSecurity, int? laborQuoteElectrical, string[] laborQuoteProduct, string laborQuoteManufacturer,
			string laborQuoteSeries, string laborQuoteWorkstations, string laborQuotePanels, string laborQuotePrivOffices, string laborQuoteConfRooms,
			string laborQuoteExamRooms, string laborQuoteClassRooms, string laborQuoteOtherAncillary, string[] laborQuoteProductIs, string laborQuoteAddInfo,
			string[] laborQuoteTypeOfScreening, int? laborQuoteElevatorNeeded, int? laborQuoteProductCleaned, string laborQuoteRevisionReason, int? laborQuoteElectricalRequired,
			string laborQuoteElectricalRequiredDetails, string laborQuoteRequestorEmailCC1, string laborQuoteRequestorEmailCC2, string removeFiles, string projectIs_OutOfState_Emails)
		{
			var region = "OSQ";
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

			ViewData["IsRevision"] = false;
			ViewData["IsDraft"] = Convert.ToBoolean(draft);
			ViewData["IsReadOnly"] = false;
			ViewData["LaborQuoteCode"] = laborQuoteCode;

			LaborQuoteAutomationOSQViewModel vm = new LaborQuoteAutomationOSQViewModel
			{
				LaborQuoteTypes = laborQuoteTypeList,
				Customers = customerList,
				Salespersons = salespersonList,
				LaborQuoteUnionVendors = unionVendorList,
				IsRevision = false,
				IsDraft = Convert.ToBoolean(draft),
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
				CurrentDealer = xmlConfig.Dealer
			};

			if (!string.IsNullOrEmpty(submit))
			{
				List<IFormFile> files = new List<IFormFile>();
				string code = "";
				switch (submit)
				{
					case "SendLaborQuote":
						if (isRevision) code = revisionCode;
						else if (!string.IsNullOrEmpty(laborQuoteCode)) code = laborQuoteCode;
						else code = _db.LaborQuoteAutomationHandler.GetLaborQuoteCode(region);

						string subject = GetFormattedSubject(laborQuoteTypeList, laborQuoteTypes, code);
						string body = GetFormattedBody(isRevision, revisionCode, laborQuoteTypes, laborQuoteRequestorName, laborQuoteRequestorEmail, laborQuoteHedbergProjectID,
									laborQuoteCustomer, laborQuoteStreetAddress, laborQuoteBRF, laborQuoteCity, laborQuoteOrOrder, laborQuoteSalesRep,
									laborQuoteCoordinator, laborQuoteProjectManager, laborQuoteDirectoryCreated, laborQuoteNoDaysForInstall,
									originalLaborQuoteNo, quoteOrOrderNumber, laborQuoteProjectIs, laborQuoteUnionVendors, ((DateTime)laborQuoteTargetDate).ToString("MM/dd/yyyy"),
									laborQuoteSupplyRequestorEmail, laborQuoteScopeOfWork, laborQuoteDeliver, laborQuoteDeliverTime, laborQuoteInstallation,
									laborQuoteInstallationTime, laborQuoteProductForm, laborQuoteUnloadAt, laborQuoteSiteIs, laborQuoteWallsAre,
									laborQuoteProtection, laborQuoteFeetMasonite, laborQuoteElevator, laborQuoteCarryUp, laborQuoteNoOfFlights,
									laborQuotePower, laborQuoteSecurity, laborQuoteElectrical, laborQuoteProduct, laborQuoteManufacturer,
									laborQuoteSeries, laborQuoteWorkstations, laborQuotePanels, laborQuotePrivOffices, laborQuoteConfRooms,
									laborQuoteExamRooms, laborQuoteClassRooms, laborQuoteOtherAncillary, laborQuoteProductIs, laborQuoteAddInfo,
									laborQuoteTypeOfScreening, laborQuoteElevatorNeeded, laborQuoteProductCleaned, laborQuoteRevisionReason,
									laborQuoteElectricalRequired, laborQuoteElectricalRequiredDetails, laborQuoteRequestorEmailCC1, laborQuoteRequestorEmailCC2, projectIs_OutOfState_Emails);

						foreach (var item in Request.Form.Files)
						{
							if (item != null) files.Add(item);
						}

						foreach (var file in files)
						{
							if (file != null || file.Length > 0)
							{
								using (var stream = new FileStream("wwwroot/files/lqa/" + code + "_" + file.Name + "_" + file.FileName, FileMode.Create, FileAccess.ReadWrite))
								{
									await file.CopyToAsync(stream);
								}
							}
						}

						EmailProperties email = new EmailProperties
						{
							Attachments = files.ToArray(),
							To = new string[] { laborQuoteRequestorEmail },
							CC = (laborQuoteRequestorEmailCC1?.Replace(",", ";") + ";" + laborQuoteRequestorEmailCC2 + ";" + xmlConfig.getLaborQuoteEstimatorEmail()).Split(';', StringSplitOptions.RemoveEmptyEntries),
							Subject = subject,
							Body = body,
							IsHTMLBody = true
						};

						try
						{
							_logger.LogWarning("Create Labor Quote Header {code}", code);
							string status = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("Status", region).Where(x => x.Value == "Pending").Select(x => x.LookupGeneralID).FirstOrDefault();
							string done = _db.LaborQuoteAutomationHandler.CreateLaborQuoteHeaderOSQ(status, code, laborQuoteTypes, laborQuoteRequestorName, laborQuoteRequestorEmail,
							laborQuoteHedbergProjectID, laborQuoteCustomer, laborQuoteStreetAddress, laborQuoteBRF, laborQuoteCity, laborQuoteOrOrder,
							laborQuoteSalesRep, laborQuoteCoordinator, laborQuoteProjectManager, laborQuoteDirectoryCreated, laborQuoteNoDaysForInstall,
							originalLaborQuoteNo, quoteOrOrderNumber, laborQuoteProjectIs, laborQuoteUnionVendors, laborQuoteTargetDate,
							laborQuoteSupplyRequestorEmail, laborQuoteScopeOfWork, laborQuoteDeliver, laborQuoteDeliverTime, laborQuoteInstallation,
							laborQuoteInstallationTime, laborQuoteProductForm, laborQuoteUnloadAt, laborQuoteSiteIs, laborQuoteWallsAre,
							laborQuoteProtection, laborQuoteFeetMasonite, laborQuoteElevator, laborQuoteCarryUp, laborQuoteNoOfFlights,
							laborQuotePower, laborQuoteSecurity, laborQuoteElectrical, laborQuoteProduct, laborQuoteManufacturer,
							laborQuoteSeries, laborQuoteWorkstations, laborQuotePanels, laborQuotePrivOffices, laborQuoteConfRooms,
							laborQuoteExamRooms, laborQuoteClassRooms, laborQuoteOtherAncillary, laborQuoteProductIs, laborQuoteAddInfo,
							laborQuoteTypeOfScreening, laborQuoteElevatorNeeded, laborQuoteProductCleaned, laborQuoteRevisionReason,
							laborQuoteElectricalRequired, laborQuoteElectricalRequiredDetails, laborQuoteRequestorEmailCC1, laborQuoteRequestorEmailCC2, projectIs_OutOfState_Emails, _logger);

							_logger.LogWarning("Done Create Labor Quote Header {code}", code);
							if (done == "1")
							{
								_emailHelper.SendEmailV2(_configuration, email, region);
								vm.Success = "success";
							}
							else
							{
								vm.Success = "error";
							}

							return View(vm);
						}
						catch (Exception ex)
						{
							vm.Success = "error";
							throw ex;
						}

					case "SaveLaborQuote":
						code = !string.IsNullOrEmpty(laborQuoteCode) ? laborQuoteCode : _db.LaborQuoteAutomationHandler.GetLaborQuoteCode(region);

						if (removeFiles == "yes")
						{
							string sourcePath = @"wwwroot/files/lqa/";
							if (System.IO.Directory.Exists(sourcePath))
							{
								string[] filesToDelete = System.IO.Directory.GetFiles(sourcePath);

								// Copy the files and overwrite destination files if they already exist.
								foreach (string s in filesToDelete)
								{
									if (System.IO.Path.GetFileName(s).Contains(laborQuoteCode))
									{
										try
										{
											System.IO.File.Delete(s);
										}
										catch (Exception ex)
										{
											vm.Success = "error";
											throw ex;
										}
									}
								}
							}
						}
						foreach (var item in Request.Form.Files)
						{
							if (item != null) files.Add(item);
						}

						foreach (var file in files)
						{
							if (file != null || file.Length > 0)
							{
								using (var stream = new FileStream("wwwroot/files/lqa/" + code + "_" + file.FileName, FileMode.Create, FileAccess.ReadWrite))
								{
									await file.CopyToAsync(stream);
								}
							}
						}

						try
						{
							string status = _db.LaborQuoteAutomationHandler.LoadLookupGeneralWithValue("Status", region).Where(x => x.Value == "Draft").Select(x => x.LookupGeneralID).FirstOrDefault();
							_logger.LogTrace("Create Labor Quote Header", status, code, laborQuoteTypes, laborQuoteRequestorName, laborQuoteRequestorEmail,
							laborQuoteHedbergProjectID, laborQuoteCustomer, laborQuoteStreetAddress, laborQuoteBRF, laborQuoteCity, laborQuoteOrOrder,
							laborQuoteSalesRep, laborQuoteCoordinator, laborQuoteProjectManager, laborQuoteDirectoryCreated, laborQuoteNoDaysForInstall,
							originalLaborQuoteNo, quoteOrOrderNumber, laborQuoteProjectIs, laborQuoteUnionVendors, laborQuoteTargetDate,
							laborQuoteSupplyRequestorEmail, laborQuoteScopeOfWork, laborQuoteDeliver, laborQuoteDeliverTime, laborQuoteInstallation,
							laborQuoteInstallationTime, laborQuoteProductForm, laborQuoteUnloadAt, laborQuoteSiteIs, laborQuoteWallsAre,
							laborQuoteProtection, laborQuoteFeetMasonite, laborQuoteElevator, laborQuoteCarryUp, laborQuoteNoOfFlights,
							laborQuotePower, laborQuoteSecurity, laborQuoteElectrical, laborQuoteProduct, laborQuoteManufacturer,
							laborQuoteSeries, laborQuoteWorkstations, laborQuotePanels, laborQuotePrivOffices, laborQuoteConfRooms,
							laborQuoteExamRooms, laborQuoteClassRooms, laborQuoteOtherAncillary, laborQuoteProductIs, laborQuoteAddInfo,
							laborQuoteTypeOfScreening, laborQuoteElevatorNeeded, laborQuoteProductCleaned, laborQuoteRevisionReason,
							laborQuoteElectricalRequired, laborQuoteElectricalRequiredDetails, laborQuoteRequestorEmailCC1, laborQuoteRequestorEmailCC2);

							string done = _db.LaborQuoteAutomationHandler.CreateLaborQuoteHeaderOSQ(status, code, laborQuoteTypes, laborQuoteRequestorName, laborQuoteRequestorEmail,
							laborQuoteHedbergProjectID, laborQuoteCustomer, laborQuoteStreetAddress, laborQuoteBRF, laborQuoteCity, laborQuoteOrOrder,
							laborQuoteSalesRep, laborQuoteCoordinator, laborQuoteProjectManager, laborQuoteDirectoryCreated, laborQuoteNoDaysForInstall,
							originalLaborQuoteNo, quoteOrOrderNumber, laborQuoteProjectIs, laborQuoteUnionVendors, laborQuoteTargetDate,
							laborQuoteSupplyRequestorEmail, laborQuoteScopeOfWork, laborQuoteDeliver, laborQuoteDeliverTime, laborQuoteInstallation,
							laborQuoteInstallationTime, laborQuoteProductForm, laborQuoteUnloadAt, laborQuoteSiteIs, laborQuoteWallsAre,
							laborQuoteProtection, laborQuoteFeetMasonite, laborQuoteElevator, laborQuoteCarryUp, laborQuoteNoOfFlights,
							laborQuotePower, laborQuoteSecurity, laborQuoteElectrical, laborQuoteProduct, laborQuoteManufacturer,
							laborQuoteSeries, laborQuoteWorkstations, laborQuotePanels, laborQuotePrivOffices, laborQuoteConfRooms,
							laborQuoteExamRooms, laborQuoteClassRooms, laborQuoteOtherAncillary, laborQuoteProductIs, laborQuoteAddInfo,
							laborQuoteTypeOfScreening, laborQuoteElevatorNeeded, laborQuoteProductCleaned, laborQuoteRevisionReason,
							laborQuoteElectricalRequired, laborQuoteElectricalRequiredDetails, laborQuoteRequestorEmailCC1, laborQuoteRequestorEmailCC2, projectIs_OutOfState_Emails, _logger);

							_logger.LogTrace("Done Create Labor Quote Header", "draft", code, laborQuoteTypes, laborQuoteRequestorName,
							laborQuoteHedbergProjectID, laborQuoteOrOrder);

							if (done == "1")
							{
								vm.Success = "saved";
							}
							else
							{
								vm.Success = "error";
							}

							return View(vm);

						}
						catch (Exception ex)
						{
							vm.Success = "error";
							throw ex;
						}

					case "CreateRevision":
						vm.IsRevision = true;
						ViewData["IsRevision"] = true;
						vm.RevisionCode = _db.LaborQuoteAutomationHandler.GetLaborQuoteCode(region);
						vm = LoadLaborQuote(vm, originalLaborQuoteNo);
						break;
				}
			}

			//READONLY/DRAFT MODE
			if (!string.IsNullOrEmpty(laborQuoteCode))
			{
				var isDraft = !string.IsNullOrEmpty(draft) && Convert.ToBoolean(draft);
				ViewData["IsReadOnly"] = !isDraft;
				vm.IsReadOnly = !isDraft;

				vm = LoadLaborQuote(vm, laborQuoteCode);

				//ATTACHED FILES
				string codeName = laborQuoteCode;
				DirectoryInfo di = new DirectoryInfo(@"wwwroot/files/lqa");
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

		[Route("/LaborQuoteRequestForm/GetOriginalLaborQuotes")]
		public JsonResult GetOriginalLaborQuotes()
		{
			return new JsonResult(_db.LaborQuoteAutomationHandler.GetOriginalLaborQuotes("OSQ"));
		}

		public async Task<IActionResult> Download(string filename)
		{
			if (filename != null)
			{
				var path = Path.Combine("wwwroot/files/lqa/", filename);

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

		[Route("/LaborQuoteRequestForm/GetLaborQuoteAddress")]
		public JsonResult GetLaborQuoteAddress(int orderNo, char orderType)
		{
			return new JsonResult(_db.LaborQuoteAutomationHandler.GetLaborQuoteAddress(orderNo, orderType, "OSQ"));
		}

		private LaborQuoteAutomationOSQViewModel LoadLaborQuote(LaborQuoteAutomationOSQViewModel vm, string laborQuoteCode)
		{
			vm.LaborQuote = _db.LaborQuoteAutomationHandler.GetLaborQuoteHeaders(null, null, laborQuoteCode, null, null, null, null, null, null, null, "OSQ").FirstOrDefault();

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
			string ret = "New submission from Labor Quote Form | Labor Quote #" + code;

			if (selectedTypes.Count() > 0)
			{
				ret += " - for ";

				foreach (var type in types)
				{
					if (selectedTypes.Contains(type.LookupGeneralID.ToString()))
					{
						ret += type.TranslateValue + ", ";
					}
				}

				ret = ret.Remove(ret.Length - 2);
			}

			return ret;
		}

		private string GetFormattedBody(bool isRevision, string revisionCode, string[] laborQuoteTypes, string laborQuoteRequestorName, string laborQuoteRequestorEmail, string laborQuoteHedbergProjectID,
			string laborQuoteCustomer, string laborQuoteStreetAddress, string laborQuoteBRF, string laborQuoteCity, char laborQuoteOrOrder,
			string laborQuoteSalesRep, string laborQuoteCoordinator, string laborQuoteProjectManager, int laborQuoteDirectoryCreated, string laborQuoteNoDaysForInstall,
			string originalLaborQuoteNo, string quoteOrOrderNumber, char laborQuoteProjectIs, string[] laborQuoteUnionVendors, string laborQuoteTargetDate,
			string laborQuoteSupplyRequestorEmail, string laborQuoteScopeOfWork, int? laborQuoteDeliver, int? laborQuoteDeliverTime, int? laborQuoteInstallation,
			int? laborQuoteInstallationTime, string[] laborQuoteProductForm, int? laborQuoteUnloadAt, int? laborQuoteSiteIs, string[] laborQuoteWallsAre,
			string[] laborQuoteProtection, string laborQuoteFeetMasonite, string[] laborQuoteElevator, int? laborQuoteCarryUp, int? laborQuoteNoOfFlights,
			string[] laborQuotePower, int? laborQuoteSecurity, int? laborQuoteElectrical, string[] laborQuoteProduct, string laborQuoteManufacturer,
			string laborQuoteSeries, string laborQuoteWorkstations, string laborQuotePanels, string laborQuotePrivOffices, string laborQuoteConfRooms,
			string laborQuoteExamRooms, string laborQuoteClassRooms, string laborQuoteOtherAncillary, string[] laborQuoteProductIs, string laborQuoteAddInfo,
			string[] laborQuoteTypeOfScreening, int? laborQuoteElevatorNeeded, int? laborQuoteProductCleaned, string laborQuoteRevisionReason,
			int? laborQuoteElectricalRequired, string laborQuoteElectricalRequiredDetails, string laborQuoteRequestorEmailCC1, string laborQuoteRequestorEmailCC2, string projectIs_OutOfState_Emails)
		{
			var template = xmlConfig.getBody("request", "lqa");

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
			if (laborQuoteProjectIs == 'E')
            {
				template = template.Replace("##PROJECTIS##", "<p style='margin-left: 20px;margin-top: -10px;'>Out of State</p><p style='margin-left: 20px;margin-top: -10px;'>Email: " + projectIs_OutOfState_Emails + "</p>");
			} else
            {
				template = template.Replace("##PROJECTIS##", "<p style='margin-left: 20px;margin-top: -10px;'>" + (laborQuoteProjectIs == 'N' ? "Non-Union" : (laborQuoteProjectIs == 'U' ? "Union" : "Prevailing Wage")) + "</p>");
			}
			
			if (laborQuoteUnionVendors.Count() > 0)
			{
				template = template.Replace("##UNIONVENDORS##", GetUnionVendors(laborQuoteUnionVendors));
			}
			else
			{
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
			if (laborQuoteElectricalRequired != null)
			{
				template = template.Replace("##ELECTRICALREQ##", "<p><strong>Electrical Required?</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + (laborQuoteElectricalRequired == 1 ? "Yes" : "No") + "</p>");
			}
			else
			{
				template = template.Replace("##ELECTRICALREQ##", "");
			}
			if (!string.IsNullOrEmpty(laborQuoteElectricalRequiredDetails))
			{
				template = template.Replace("##ELECTRICALREQDETS##", "<p><strong>Electrical Required Additional Details</strong></p><p style='margin-left: 20px;margin-top: -10px;'>" + laborQuoteElectricalRequiredDetails + "</p>");
			}
			else
			{
				template = template.Replace("##ELECTRICALREQDETS##", "");
			}
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
			var laborQuoteTypeList = _db.LaborQuoteAutomationHandler.LoadLookupGeneral("Type", "OSQ");
			string ret = "<p><strong>Do you need a labor quote for:</strong></p>";

			foreach (var type in types)
			{
				string typeText = laborQuoteTypeList.Where(x => x.LookupGeneralID.ToString() == type).Select(x => x.Value).FirstOrDefault();
				ret += "<p style='margin-left: 20px;margin-top: -10px;'>" + typeText + "</p>";
			}

			return ret;
		}

		private string GetUnionVendors(string[] vendors)
		{
			var laborQuoteUnionVendorList = _db.LaborQuoteAutomationHandler.LoadLookupGeneral("UnionVendor", "OSQ");
			string ret = "<p><strong>Please select the Union Vendors:</strong></p>";

			foreach (var vendor in vendors)
			{
				string vendorText = laborQuoteUnionVendorList.Where(x => x.LookupGeneralID.ToString() == vendor).Select(x => x.Value).FirstOrDefault();
				ret += "<p style='margin-left: 20px;margin-top: -10px;'>" + vendorText + "</p>";
			}

			return ret;
		}

		private string GetTypeOfScreenings(string[] typeOfScreenings)
		{
			var laborQuoteTypeOfScreeningList = _db.LaborQuoteAutomationHandler.LoadLookupGeneral("TypeOfScreening", "OSQ");
			string ret = "<p><strong>Type of Screening</strong></p>";

			foreach (var tos in typeOfScreenings)
			{
				string tosText = laborQuoteTypeOfScreeningList.Where(x => x.LookupGeneralID.ToString() == tos).Select(x => x.Value).FirstOrDefault();
				ret += "<p style='margin-left: 20px;margin-top: -10px;'>" + tosText + "</p>";
			}

			return ret;
		}

		private string GetCustomerName(string customer)
		{
			var customerList = _db.LaborQuoteAutomationHandler.LoadCustomerList("OSQ");
			return customerList.Where(x => x.ID == customer).Select(x => x.Label).FirstOrDefault(); ;
		}

		private string GetSalesRepName(string salesRep)
		{
			var salespersonList = _db.LaborQuoteAutomationHandler.LoadSalespersonList("OSQ");
			return salespersonList.Where(x => x.ID == salesRep).Select(x => x.Label).FirstOrDefault();
		}

		private string GetLookupGeneralValues(string[] values, string name, string title)
		{
			var list = _db.LaborQuoteAutomationHandler.LoadLookupGeneral(name, "OSQ");
			string ret = "<p><strong>" + title + "</strong></p>";

			foreach (var val in values)
			{
				string text = list.Where(x => x.LookupGeneralID.ToString() == val).Select(x => x.Value).FirstOrDefault();
				ret += "<p style='margin-left: 20px;margin-top: -10px;'>" + text + "</p>";
			}

			return ret;
		}

		private string GetLookupGeneralValues(int? value, string name, string title)
		{
			var list = _db.LaborQuoteAutomationHandler.LoadLookupGeneral(name, "OSQ");
			string ret = "<p><strong>" + title + "</strong></p>";
			string text = list.Where(x => x.LookupGeneralID.ToString() == value.ToString()).Select(x => x.Value).FirstOrDefault();
			ret += "<p style='margin-left: 20px;margin-top: -10px;'>" + text + "</p>";

			return ret;
		}

		[Route("/LaborQuoteRequestForm/GetProjectData")]
		public JsonResult GetProjectData(string projectId)
		{
			return new JsonResult(_db.LaborQuoteAutomationHandler.GetProjectData(projectId, "OSQ"));
		}
	}
}
