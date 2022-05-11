using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OWPApplications.Models;
using OWPApplications.Models.LaborQuoteAutomation;
using OWPApplications.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using OWPApplications.Models.LaborQuoteAutomationCommons;

namespace OWPApplications.Data.LaborQuoteAutomation
{
	public class LaborQuoteAutomationHandler
	{
		private IConfiguration _configuration;
		private ILogger _logger;

		public LaborQuoteAutomationHandler(IConfiguration configuration, ILoggerFactory logFactory)
		{
			_configuration = configuration;
			_logger = logFactory.CreateLogger(nameof(LaborQuoteAutomationHandler));
		}

		public List<SelectValues> LoadLaborQuoteStatuses(string region)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					SqlCommand cmd1 = db.GetCommandSQL(@"EXEC dbo.Get_LaborQuote_LookupGeneral 'Status'," + "'" + region + "'");
					DataTable dt1 = db.SqlCommandToTable(cmd1);

					if (dt1 == null || dt1.Rows.Count == 0)
					{
						return null;
					}

					List<SelectValues> output = new List<SelectValues>();
					output.Add(new SelectValues
					{
						ID = "",
						Label = "Status"
					});

					foreach (DataRow row in dt1.Rows)
					{
						output.Add(new SelectValues
						{
							ID = clsLibrary.dBReadString(row["LookupGeneralID"]),
							Label = clsLibrary.dBReadString(row["Value"])
						});
					}

					return output;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public List<SelectValues> LoadCustomerList(string region)
		{
			using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
			{
				db.Open();
				var r = db.Query<CustomersModel>("dbo.Get_Customer_List", new { SALESPERSONID = "_ALL", Region = region }, commandType: CommandType.StoredProcedure);

				if (r.Count() > 0)
				{
					var output = new List<SelectValues>();

					foreach (var row in r)
					{
						output.Add(new SelectValues
						{
							ID = row.customer_no,
							Label = row.name
						});
					}

					return output.OrderBy(x => x.Label).ToList();
				}
				return null;
			}
		}

		public List<SelectValues> LoadSalespersonList(string region)
		{
			using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
			{
				db.Open();
				var r = db.Query<SalespersonModel>("dbo.Get_InternalStatusReport_Salesperson", new { Region = region }, commandType: CommandType.StoredProcedure);

				if (r.Count() > 0)
				{
					var output = new List<SelectValues>();

					foreach (var row in r)
					{
						output.Add(new SelectValues
						{
							ID = row.salesperson_id,
							Label = row.salesperson_info
						});
					}

					return output.OrderBy(x => x.Label).ToList();
				}
				return null;
			}
		}

		public List<LaborQuoteVendor> LoadLaborQuoteVendors(string region)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					SqlCommand cmd1 = db.GetCommandSQL(@"EXEC dbo.Get_LaborQuote_Vendor '" + region + "'");
					DataTable dt1 = db.SqlCommandToTable(cmd1);

					if (dt1 == null || dt1.Rows.Count == 0)
					{
						return null;
					}

					List<LaborQuoteVendor> output = new List<LaborQuoteVendor>();

					foreach (DataRow row in dt1.Rows)
					{
						output.Add(new LaborQuoteVendor
						{
							Name = clsLibrary.dBReadString(row["Name"]),
							Value = clsLibrary.dBReadString(row["Value"]),
							LookupGeneralID = clsLibrary.dBReadString(row["LookupGeneralID"])
						});
					}

					return output;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public List<LookupGeneral> LoadLookupGeneral(string type, string region)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					SqlCommand cmd1 = db.GetCommandSQL(@"EXEC dbo.Get_LaborQuote_LookupGeneral " + type + ",'" + region + "'");
					DataTable dt1 = db.SqlCommandToTable(cmd1);

					if (dt1 == null || dt1.Rows.Count == 0)
					{
						return null;
					}

					List<LookupGeneral> output = new List<LookupGeneral>();

					foreach (DataRow row in dt1.Rows)
					{
						output.Add(new LookupGeneral
						{
							Name = clsLibrary.dBReadString(row["Name"]),
							Value = clsLibrary.dBReadString(row["TranslateValue"]),
							LookupGeneralID = clsLibrary.dBReadString(row["LookupGeneralID"])
						});
					}

					return output;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public List<LookupGeneralWithValue> LoadLookupGeneralWithValue(string type, string region)
		{
			try
			{
				using (var connection = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
				{
					connection.Open();
					var param = new DynamicParameters();
					param.Add("@Name", type);
					param.Add("@Region", region);

					List<LookupGeneralWithValue> output = connection.Query<LookupGeneralWithValue>("dbo.[Get_LaborQuote_LookupGeneral]", param, commandType: CommandType.StoredProcedure).ToList();

					return output;
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning("Get_LaborQuote_LookupGeneral {0}, {1}", type, region);
				throw ex;
			}
		}

		public List<FurnitureInstallation> GetFurnitureInstallations(int laborQuoteId, string region)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					SqlCommand cmd1 = db.GetCommandSQL(@"EXEC dbo.Get_LaborQuote_FurnitureInstallation " + laborQuoteId.ToString() + "," + region);
					DataTable dt1 = db.SqlCommandToTable(cmd1);

					if (dt1 == null || dt1.Rows.Count == 0)
					{
						return null;
					}

					List<FurnitureInstallation> output = new List<FurnitureInstallation>();

					foreach (DataRow row in dt1.Rows)
					{
						output.Add(new FurnitureInstallation
						{
							Description = clsLibrary.dBReadString(row["Description"]),
							Quantity = clsLibrary.dBReadInt(row["Quantity"]),
							HoursPerQty = clsLibrary.dBReadDouble(row["HoursPerQuantity"]),
							Hours = clsLibrary.dBReadDouble(row["Hours"]),
							TotalHours = clsLibrary.dBReadDouble(row["TotalHours"]),
						});
					}

					return output;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public List<Addons> GetAddons(int laborQuoteId, string region)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					SqlCommand cmd1 = db.GetCommandSQL(@"EXEC dbo.Get_LaborQuote_Addons " + laborQuoteId.ToString() + "," + region);
					DataTable dt1 = db.SqlCommandToTable(cmd1);

					if (dt1 == null || dt1.Rows.Count == 0)
					{
						return null;
					}

					List<Addons> output = new List<Addons>();

					foreach (DataRow row in dt1.Rows)
					{
						output.Add(new Addons
						{
							Description = clsLibrary.dBReadString(row["Description"]),
							Quantity = clsLibrary.dBReadInt(row["Quantity"]),
							Hours = clsLibrary.dBReadDouble(row["Hours"]),
							TotalHours = clsLibrary.dBReadDouble(row["TotalHours"]),
						});
					}

					return output;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public List<MiscCharges> GetMiscCharges(int laborQuoteId, string region)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					SqlCommand cmd1 = db.GetCommandSQL(@"EXEC dbo.Get_LaborQuote_MiscInstallationCharges " + laborQuoteId.ToString() + "," + region);
					DataTable dt1 = db.SqlCommandToTable(cmd1);

					if (dt1 == null || dt1.Rows.Count == 0)
					{
						return null;
					}

					List<MiscCharges> output = new List<MiscCharges>();

					foreach (DataRow row in dt1.Rows)
					{
						output.Add(new MiscCharges
						{
							Description = clsLibrary.dBReadString(row["Description"]),
							Cost = clsLibrary.dBReadDouble(row["Cost"]),
							TotalCost = clsLibrary.dBReadDouble(row["TotalCost"]),
							Quantity = clsLibrary.dBReadInt(row["Quantity"]),
							Rate = clsLibrary.dBReadInt(row["Rate"])
						});
					}

					return output;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public bool CreateLaborQuoteHeader(string code, string[] laborQuoteTypes, string laborQuoteRequestorName, string laborQuoteRequestorEmail,
			string laborQuoteHedbergProjectID, string laborQuoteCustomer, string laborQuoteStreetAddress, string laborQuoteBRF,
			string laborQuoteCity, char laborQuoteOrOrder, string laborQuoteSalesRep, string laborQuoteCoordinator,
			string laborQuoteProjectManager, int laborQuoteDirectoryCreated, string laborQuoteNoDaysForInstall, string originalLaborQuoteNo,
			string quoteOrOrderNumber, char laborQuoteProjectIs, List<LookupGeneralWithValue> unionVendorList, string[] laborQuoteUnionVendors, string laborQuoteUnionVendorsADD, DateTime laborQuoteTargetDate,
			string laborQuoteSupplyRequestorEmail, string laborQuoteScopeOfWork, int? laborQuoteDeliver, int? laborQuoteDeliverTime, int? laborQuoteInstallation,
			int? laborQuoteInstallationTime, string[] laborQuoteProductForm, int? laborQuoteUnloadAt, int? laborQuoteSiteIs, string[] laborQuoteWallsAre,
			string[] laborQuoteProtection, string laborQuoteFeetMasonite, string[] laborQuoteElevator, int? laborQuoteCarryUp, int? laborQuoteNoOfFlights,
			string[] laborQuotePower, int? laborQuoteSecurity, int? laborQuoteElectrical, string[] laborQuoteProduct, string laborQuoteManufacturer,
			string laborQuoteSeries, string laborQuoteWorkstations, string laborQuotePanels, string laborQuotePrivOffices, string laborQuoteConfRooms,
			string laborQuoteExamRooms, string laborQuoteClassRooms, string laborQuoteOtherAncillary, string[] laborQuoteProductIs, string laborQuoteAddInfo,
			string[] laborQuoteTypeOfScreening, int? laborQuoteElevatorNeeded, int? laborQuoteProductCleaned, string laborQuoteRevisionReason, 
			bool? laborQuoteYulioPresentation, bool? laborQuoteOutOfStateProject, int? laborQuoteFloorsIncluded, int? laborQuoteScenesPerFloor,
			string laborQuoteRequestorEmailCC1, string laborQuoteRequestorEmailCC2, string status)
		{
			try
			{
				using (var connection = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
				{
					connection.Open();
					var param = new DynamicParameters();
					param.Add("@LaborQuoteCode", code);
					param.Add("@LaborQuoteTypeLookUpIDs", GetLaborQuoteTypes(laborQuoteTypes));
					param.Add("@LaborQuoteStatusLookUpID", Convert.ToInt32(status));
					param.Add("@RequestorName", laborQuoteRequestorName);
					param.Add("@RequestorEmail", laborQuoteRequestorEmail);
					param.Add("@ProjectID", laborQuoteHedbergProjectID);
					param.Add("@CustomerNumber", laborQuoteCustomer);
					param.Add("@AddressLine1", laborQuoteStreetAddress);
					param.Add("@AddressLine2", laborQuoteBRF);
					param.Add("@AddressCity", laborQuoteCity);
					param.Add("@AddressState", null);
					param.Add("@QuoteOrOrder", laborQuoteOrOrder);
					param.Add("@SalespersonID", laborQuoteSalesRep);
					param.Add("@Coordinator", laborQuoteCoordinator);
					param.Add("@ProjectManager", laborQuoteProjectManager);
					param.Add("@DirectoryCreated", Convert.ToBoolean(laborQuoteDirectoryCreated));
					param.Add("@DaysForInstall", Convert.ToInt32(laborQuoteNoDaysForInstall));
					param.Add("@CreatedBy", "");
					param.Add("@ModifiedBy", null);
					param.Add("@OriginalLaborQuoteCode", originalLaborQuoteNo);
					param.Add("@Scope", null);
					param.Add("@EquipmentTools", null);
					param.Add("@VendorNumberLookupID", null);
					param.Add("@UnionVendorsLookupIDs", GetLaborQuoteUnionVendors(laborQuoteUnionVendors, unionVendorList));
					param.Add("@UnionVendorsAdd", laborQuoteUnionVendorsADD);
					param.Add("@Taxable", null);
					param.Add("@Notes", null);
					param.Add("@EstimatorID", null);
					param.Add("@EstimatorName", null);
					param.Add("@ComplexProject", null);
					param.Add("@Surcharge", null);
					param.Add("@NumberOfDays", null);
					param.Add("@QuoteOrOrderNumber", quoteOrOrderNumber);
					param.Add("@ProjectIs", laborQuoteProjectIs);
					param.Add("@TargetDate", laborQuoteTargetDate == DateTime.MinValue ? null : (DateTime?)laborQuoteTargetDate);
					param.Add("@UnionJobsEmailAddress", laborQuoteSupplyRequestorEmail);
					param.Add("@ScopeOfWork", laborQuoteScopeOfWork);
					param.Add("@LaborQuoteReceiveDeliverLookupID", laborQuoteDeliver);
					param.Add("@LaborQuoteReceiveDeliverDayTimeLookupID", laborQuoteDeliverTime);
					param.Add("@LaborQuoteInstallationLookupID", laborQuoteInstallation);
					param.Add("@LaborQuoteInstallationTimeLookupID", laborQuoteInstallationTime);
					param.Add("@ProductFromLookupIDs", GetLaborQuoteProductFroms(laborQuoteProductForm));
					param.Add("@LaborQuoteUnloadAtLookupID", laborQuoteUnloadAt);
					param.Add("@LaborQuoteSiteIsLookupID", laborQuoteSiteIs);
					param.Add("@WallsAreLookupIDs", GetLaborQuoteWalls(laborQuoteWallsAre));
					param.Add("@ProtectionLookupIDs", GetLaborQuoteProtections(laborQuoteProtection));
					param.Add("@FeetOfMasonite", laborQuoteFeetMasonite != null ? laborQuoteFeetMasonite.Replace(",",".") : null);
					param.Add("@ElevatorLookupIDs", GetLaborQuoteElevators(laborQuoteElevator));
					param.Add("@CarryUp", Convert.ToBoolean(laborQuoteCarryUp));
					param.Add("@NumberOfFlights", laborQuoteNoOfFlights);
					param.Add("@PowerLookupIDs", GetLaborQuotePowers(laborQuotePower));
					param.Add("@SecurityClearanceRequired", Convert.ToBoolean(laborQuoteSecurity));
					param.Add("@ElectricalPerimitRequired", Convert.ToBoolean(laborQuoteElectrical));
					param.Add("@ProductTypeLookupIDs", GetLaborQuoteProductTypes(laborQuoteProduct));
					param.Add("@ProductManufacturer", laborQuoteManufacturer);
					param.Add("@ProductSeries", laborQuoteSeries);
					param.Add("@ProductWorkstations", laborQuoteWorkstations);
					param.Add("@ProductPanelsOnly", laborQuotePanels);
					param.Add("@ProductPrivateOffices", laborQuotePrivOffices);
					param.Add("@ProductConferenceRooms", laborQuoteConfRooms);
					param.Add("@ProductExamRooms", laborQuoteExamRooms);
					param.Add("@ProductClassRooms", laborQuoteClassRooms);
					param.Add("@ProductOther", laborQuoteOtherAncillary);
					param.Add("@ProductIsLookupIDs", GetLaborQuoteProductIs(laborQuoteProductIs));
					param.Add("@ProductIsAdditionalInfo", laborQuoteAddInfo);
					param.Add("@InstallTotal", null);
					param.Add("@HealthAndSafetySurcharge", null);
					param.Add("@RegularHours", null);
					param.Add("@OTHours", null);
					param.Add("@DTHours", null);
					param.Add("@RegularHsRate", null);
					param.Add("@OTHsRate", null);
					param.Add("@DTHsRate", null);
					param.Add("@SurchargePct", null);
					param.Add("@TypeOfScreeningLookupIDs", GetLaborQuoteTypeOfScreenings(laborQuoteTypeOfScreening));
					param.Add("@ElevatorNeeded", Convert.ToBoolean(laborQuoteElevatorNeeded));
					param.Add("@ProductNeedToBeCleanedOrSanitized", Convert.ToBoolean(laborQuoteProductCleaned));
					param.Add("@RevisionReason", laborQuoteRevisionReason);
					param.Add("@TruckCapacity", null);
					param.Add("@YPRequired", laborQuoteYulioPresentation);
					param.Add("@OutOfStateProject", laborQuoteOutOfStateProject);
					param.Add("@QtyOfIncludedFloors", laborQuoteFloorsIncluded);
					param.Add("@LaborQuoteScenesPerFloorLookUpID", laborQuoteScenesPerFloor);
					param.Add("@CustomerName", null);
					param.Add("@RegularHsLaborTypeLookupID", null);
					param.Add("@OTHoursLaborTypeLookupID", null);
					param.Add("@DTHoursLaborTypeLookupID", null);
					param.Add("@IsEstimator", false);
					param.Add("@RequestorEmailCC", laborQuoteRequestorEmailCC1);
					param.Add("@RequestorEmailCC2", laborQuoteRequestorEmailCC2);
					param.Add("@IsScheduled", false);
					connection.Execute("dbo.[Update_LaborQuote_Header]", param, commandType: CommandType.StoredProcedure);

					return true;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public string CreateLaborQuoteHeaderOSQ(string status, string code, string[] laborQuoteTypes, string laborQuoteRequestorName, string laborQuoteRequestorEmail,
			string laborQuoteHedbergProjectID, string laborQuoteCustomer, string laborQuoteStreetAddress, string laborQuoteBRF,
			string laborQuoteCity, char laborQuoteOrOrder, string laborQuoteSalesRep, string laborQuoteCoordinator,
			string laborQuoteProjectManager, int laborQuoteDirectoryCreated, string laborQuoteNoDaysForInstall, string originalLaborQuoteNo,
			string quoteOrOrderNumber, char laborQuoteProjectIs, string[] laborQuoteUnionVendors, DateTime? laborQuoteTargetDate,
			string laborQuoteSupplyRequestorEmail, string laborQuoteScopeOfWork, int? laborQuoteDeliver, int? laborQuoteDeliverTime, int? laborQuoteInstallation,
			int? laborQuoteInstallationTime, string[] laborQuoteProductForm, int? laborQuoteUnloadAt, int? laborQuoteSiteIs, string[] laborQuoteWallsAre,
			string[] laborQuoteProtection, string laborQuoteFeetMasonite, string[] laborQuoteElevator, int? laborQuoteCarryUp, int? laborQuoteNoOfFlights,
			string[] laborQuotePower, int? laborQuoteSecurity, int? laborQuoteElectrical, string[] laborQuoteProduct, string laborQuoteManufacturer,
			string laborQuoteSeries, string laborQuoteWorkstations, string laborQuotePanels, string laborQuotePrivOffices, string laborQuoteConfRooms,
			string laborQuoteExamRooms, string laborQuoteClassRooms, string laborQuoteOtherAncillary, string[] laborQuoteProductIs, string laborQuoteAddInfo,
			string[] laborQuoteTypeOfScreening, int? laborQuoteElevatorNeeded, int? laborQuoteProductCleaned, string laborQuoteRevisionReason,
			int? laborQuoteElectricalRequired, string laborQuoteElectricalRequiredDetails, string laborQuoteRequestorEmailCC1, string laborQuoteRequestorEmailCC2, string projectIs_OutOfState_Emails, ILogger _logger)
		{
			try
			{
				string laborQuoteTypesDB = GetLaborQuoteTypes(laborQuoteTypes);
				int laborQuoteStatusDB = Convert.ToInt32(status);
				bool laborQuoteDirectoryCreatedDB = Convert.ToBoolean(laborQuoteDirectoryCreated);
				int laborQuoteNoDaysForInstallDB = Convert.ToInt32(laborQuoteNoDaysForInstall);
				string laborQuoteUnionVendorsDB = GetLaborQuoteUnionVendorsOSQ(laborQuoteUnionVendors);
				DateTime? laborQuoteTargetDateDB = laborQuoteTargetDate == DateTime.MinValue ? null : (DateTime?)laborQuoteTargetDate;
				string laborQuoteProductFormDB = GetLaborQuoteProductFroms(laborQuoteProductForm);
				string laborQuoteWallsAreDB = GetLaborQuoteWalls(laborQuoteWallsAre);
				string laborQuoteProtectionDB = GetLaborQuoteProtections(laborQuoteProtection);
				string laborQuoteElevatorDB = GetLaborQuoteElevators(laborQuoteElevator);
				bool laborQuoteCarryUpDB = Convert.ToBoolean(laborQuoteCarryUp);
				string laborQuotePowerDB = GetLaborQuotePowers(laborQuotePower);
				bool laborQuoteSecurityDB = Convert.ToBoolean(laborQuoteSecurity);
				bool laborQuoteElectricalDB = Convert.ToBoolean(laborQuoteElectrical);
				string laborQuoteProductDB = GetLaborQuoteProductTypes(laborQuoteProduct);
				string laborQuoteProductIdDB = GetLaborQuoteProductIs(laborQuoteProductIs);
				string laborQuoteTypeOfScreeningDB = GetLaborQuoteTypeOfScreenings(laborQuoteTypeOfScreening);
				bool laborQuoteElevatorNeededDB = Convert.ToBoolean(laborQuoteElevatorNeeded);
				bool laborQuoteProductCleanedDB = Convert.ToBoolean(laborQuoteProductCleaned);
				bool laborQuoteElectricalRequiredDB = Convert.ToBoolean(laborQuoteElectricalRequired);

				using (var connection = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
				{
					connection.Open();
					var param = new DynamicParameters();
					param.Add("@LaborQuoteCode", code);
					param.Add("@LaborQuoteTypeLookUpIDs", laborQuoteTypesDB);
					param.Add("@LaborQuoteStatusLookUpID", laborQuoteStatusDB);
					param.Add("@RequestorName", laborQuoteRequestorName);
					param.Add("@RequestorEmail", laborQuoteRequestorEmail);
					param.Add("@ProjectID", laborQuoteHedbergProjectID);
					param.Add("@CustomerNumber", laborQuoteCustomer);
					param.Add("@AddressLine1", laborQuoteStreetAddress);
					param.Add("@AddressLine2", laborQuoteBRF);
					param.Add("@AddressCity", laborQuoteCity);
					param.Add("@AddressState", null);
					param.Add("@QuoteOrOrder", laborQuoteOrOrder);
					param.Add("@SalespersonID", laborQuoteSalesRep);
					param.Add("@Coordinator", laborQuoteCoordinator);
					param.Add("@ProjectManager", laborQuoteProjectManager);
					param.Add("@DirectoryCreated", laborQuoteDirectoryCreatedDB);
					param.Add("@DaysForInstall", laborQuoteNoDaysForInstallDB);
					param.Add("@CreatedBy", "");
					param.Add("@ModifiedBy", null);
					param.Add("@OriginalLaborQuoteCode", originalLaborQuoteNo);
					param.Add("@Scope", null);
					param.Add("@EquipmentTools", null);
					param.Add("@VendorNumberLookupID", null);
					param.Add("@UnionVendorsLookupIDs", laborQuoteUnionVendorsDB);
					param.Add("@Taxable", null);
					param.Add("@Notes", null);
					param.Add("@EstimatorID", null);
					param.Add("@EstimatorName", null);
					param.Add("@ComplexProject", null);
					param.Add("@Surcharge", null);
					param.Add("@NumberOfDays", null);
					param.Add("@QuoteOrOrderNumber", quoteOrOrderNumber);
					param.Add("@ProjectIs", laborQuoteProjectIs);
					param.Add("@TargetDate", laborQuoteTargetDateDB);
					param.Add("@UnionJobsEmailAddress", laborQuoteSupplyRequestorEmail);
					param.Add("@ScopeOfWork", laborQuoteScopeOfWork);
					param.Add("@LaborQuoteReceiveDeliverLookupID", laborQuoteDeliver);
					param.Add("@LaborQuoteReceiveDeliverDayTimeLookupID", laborQuoteDeliverTime);
					param.Add("@LaborQuoteInstallationLookupID", laborQuoteInstallation);
					param.Add("@LaborQuoteInstallationTimeLookupID", laborQuoteInstallationTime);
					param.Add("@ProductFromLookupIDs", laborQuoteProductFormDB);
					param.Add("@LaborQuoteUnloadAtLookupID", laborQuoteUnloadAt);
					param.Add("@LaborQuoteSiteIsLookupID", laborQuoteSiteIs);
					param.Add("@WallsAreLookupIDs", laborQuoteWallsAreDB);
					param.Add("@ProtectionLookupIDs", laborQuoteProtectionDB);
					param.Add("@FeetOfMasonite", laborQuoteFeetMasonite);
					param.Add("@ElevatorLookupIDs", laborQuoteElevatorDB);
					param.Add("@CarryUp", laborQuoteCarryUpDB);
					param.Add("@NumberOfFlights", laborQuoteNoOfFlights);
					param.Add("@PowerLookupIDs", laborQuotePowerDB);
					param.Add("@SecurityClearanceRequired", laborQuoteSecurityDB);
					param.Add("@ElectricalPerimitRequired", laborQuoteElectricalDB);
					param.Add("@ProductTypeLookupIDs", laborQuoteProductDB);
					param.Add("@ProductManufacturer", laborQuoteManufacturer);
					param.Add("@ProductSeries", laborQuoteSeries);
					param.Add("@ProductWorkstations", laborQuoteWorkstations);
					param.Add("@ProductPanelsOnly", laborQuotePanels);
					param.Add("@ProductPrivateOffices", laborQuotePrivOffices);
					param.Add("@ProductConferenceRooms", laborQuoteConfRooms);
					param.Add("@ProductExamRooms", laborQuoteExamRooms);
					param.Add("@ProductClassRooms", laborQuoteClassRooms);
					param.Add("@ProductOther", laborQuoteOtherAncillary);
					param.Add("@ProductIsLookupIDs", laborQuoteProductIdDB);
					param.Add("@ProductIsAdditionalInfo", laborQuoteAddInfo);
					param.Add("@InstallTotal", null);
					param.Add("@HealthAndSafetySurcharge", null);
					param.Add("@RegularHours", null);
					param.Add("@OTHours", null);
					param.Add("@DTHours", null);
					param.Add("@RegularHsRate", null);
					param.Add("@OTHsRate", null);
					param.Add("@DTHsRate", null);
					param.Add("@SurchargePct", null);
					param.Add("@TypeOfScreeningLookupIDs", laborQuoteTypeOfScreeningDB);
					param.Add("@ElevatorNeeded", laborQuoteElevatorNeededDB);
					param.Add("@ProductNeedToBeCleanedOrSanitized", laborQuoteProductCleanedDB);
					param.Add("@RevisionReason", laborQuoteRevisionReason);
					param.Add("@TruckCapacity", null);
					param.Add("@CustomerName", null);
					param.Add("@ElectricalRequired", laborQuoteElectricalRequiredDB);
					param.Add("@ElectricalAdditionalDetails", laborQuoteElectricalRequiredDetails);
					param.Add("@RegularHsLaborTypeLookupID", null);
					param.Add("@OTHoursLaborTypeLookupID", null);
					param.Add("@DTHoursLaborTypeLookupID", null);
					param.Add("@IsEstimator", 0);
					param.Add("@RequestorEmailCC", laborQuoteRequestorEmailCC1);
					param.Add("@RequestorEmailCC2", laborQuoteRequestorEmailCC2);
					param.Add("@ProjectIs_OutOfState_Emails", projectIs_OutOfState_Emails);
					param.Add("@IsScheduled", false);

					_logger.LogWarning("Start Update_LaborQuote_Header -- LaborQuoteCode: {code} |" +
						" LaborQuoteTypeLookUpIDs: {laborQuoteTypesDB} |" +
						" LaborQuoteStatusLookUpID: {laborQuoteStatusDB} |" +
						" RequestorName: {laborQuoteRequestorName} |" +
						" RequestorEmail: {laborQuoteRequestorEmail} |" +
						" ProjectID: {laborQuoteHedbergProjectID} |" +
						" CustomerNumber: {laborQuoteCustomer} |" +
						" AddressLine1: {laborQuoteStreetAddress} |" +
						" AddressLine2: {laborQuoteBRF} |" +
						" AddressCity: {laborQuoteCity} |" +
						" QuoteOrOrder: {laborQuoteOrOrder} |" +
						" SalespersonID: {laborQuoteSalesRep} |" +
						" Coordinator: {laborQuoteCoordinator} |" +
						" ProjectManager: {laborQuoteProjectManager} |" +
						" DirectoryCreated: {laborQuoteDirectoryCreatedDB} |" +
						" DaysForInstall: {laborQuoteNoDaysForInstallDB} |" +
						" OriginalLaborQuoteCode: {originalLaborQuoteNo} |" +
						" UnionVendorsLookupIDs: {laborQuoteUnionVendorsDB} |" +
						" QuoteOrOrderNumber: {quoteOrOrderNumber} |" +
						" ProjectIs: {laborQuoteProjectIs} |" +
						" TargetDate: {laborQuoteTargetDateDB} |" +
						" UnionJobsEmailAddress: {laborQuoteSupplyRequestorEmail} |" +
						" ScopeOfWork: {laborQuoteScopeOfWork} |" +
						" LaborQuoteReceiveDeliverLookupID: {laborQuoteDeliver} |" +
						" LaborQuoteReceiveDeliverDayTimeLookupID: {laborQuoteDeliverTime} |" +
						" LaborQuoteInstallationLookupID: {laborQuoteInstallation} |" +
						" LaborQuoteInstallationTimeLookupID: {laborQuoteInstallationTime} |" +
						" ProductFromLookupIDs: {laborQuoteProductFormDB} |" +
						" LaborQuoteUnloadAtLookupID: {laborQuoteUnloadAt} |" +
						" LaborQuoteSiteIsLookupID: {laborQuoteSiteIs} |" +
						" WallsAreLookupIDs: {laborQuoteWallsAreDB} |" +
						" ProtectionLookupIDs: {laborQuoteProtectionDB} |" +
						" FeetOfMasonite: {laborQuoteFeetMasonite} |" +
						" ElevatorLookupIDs: {laborQuoteElevatorDB} |" +
						" CarryUp: {laborQuoteCarryUpDB} |" +
						" NumberOfFlights: {laborQuoteNoOfFlights} |" +
						" PowerLookupIDs: {laborQuotePowerDB} |" +
						" SecurityClearanceRequired: {laborQuoteSecurityDB} |" +
						" ElectricalPerimitRequired: {laborQuoteElectricalDB} |" +
						" ProductTypeLookupIDs: {laborQuoteProductDB} |" +
						" ProductManufacturer: {laborQuoteManufacturer} |" +
						" ProductSeries: {laborQuoteSeries} |" +
						" ProductWorkstations: {laborQuoteWorkstations} |" +
						" ProductPanelsOnly: {laborQuotePanels} |" +
						" ProductPrivateOffices: {laborQuotePrivOffices} |" +
						" ProductConferenceRooms: {laborQuoteConfRooms} |" +
						" ProductExamRooms: {laborQuoteExamRooms} |" +
						" ProductClassRooms: {laborQuoteClassRooms} |" +
						" ProductOther: {laborQuoteOtherAncillary} |" +
						" ProductIsLookupIDs: {laborQuoteProductIdDB} |" +
						" ProductIsAdditionalInfo: {laborQuoteAddInfo} |" +
						" TypeOfScreeningLookupIDs: {laborQuoteTypeOfScreeningDB} |" +
						" ElevatorNeeded: {laborQuoteElevatorNeededDB} |" +
						" ProductNeedToBeCleanedOrSanitized: {laborQuoteProductCleanedDB} |" +
						" RevisionReason: {laborQuoteRevisionReason} |" +
						" ElectricalRequired: {laborQuoteElectricalRequiredDB} |" +
						" ElectricalAdditionalDetails: {laborQuoteElectricalRequiredDetails} |" +
						" RequestorEmailCC: {laborQuoteRequestorEmailCC1} |" +
						" RequestorEmailCC2: {laborQuoteRequestorEmailCC2} |" +
						" ProjectIs_OutOfState_Emails: {projectIs_OutOfState_Emails} |",
						code,
						laborQuoteTypesDB,
						laborQuoteStatusDB,
						laborQuoteRequestorName,
						laborQuoteRequestorEmail,
						laborQuoteHedbergProjectID,
						laborQuoteCustomer,
						laborQuoteStreetAddress,
						laborQuoteBRF,
						laborQuoteCity,
						laborQuoteOrOrder,
						laborQuoteSalesRep,
						laborQuoteCoordinator,
						laborQuoteProjectManager,
						laborQuoteDirectoryCreatedDB,
						laborQuoteNoDaysForInstallDB,
						originalLaborQuoteNo,
						laborQuoteUnionVendorsDB,
						quoteOrOrderNumber,
						laborQuoteProjectIs,
						laborQuoteTargetDateDB,
						laborQuoteSupplyRequestorEmail,
						laborQuoteScopeOfWork,
						laborQuoteDeliver,
						laborQuoteDeliverTime,
						laborQuoteInstallation,
						laborQuoteInstallationTime,
						laborQuoteProductFormDB,
						laborQuoteUnloadAt,
						laborQuoteSiteIs,
						laborQuoteWallsAreDB,
						laborQuoteProtectionDB,
						laborQuoteFeetMasonite,
						laborQuoteElevatorDB,
						laborQuoteCarryUpDB,
						laborQuoteNoOfFlights,
						laborQuotePowerDB,
						laborQuoteSecurityDB,
						laborQuoteElectricalDB,
						laborQuoteProductDB,
						laborQuoteManufacturer,
						laborQuoteSeries,
						laborQuoteWorkstations,
						laborQuotePanels,
						laborQuotePrivOffices,
						laborQuoteConfRooms,
						laborQuoteExamRooms,
						laborQuoteClassRooms,
						laborQuoteOtherAncillary,
						laborQuoteProductIdDB,
						laborQuoteAddInfo,
						laborQuoteTypeOfScreeningDB,
						laborQuoteElevatorNeededDB,
						laborQuoteProductCleanedDB,
						laborQuoteRevisionReason,
						laborQuoteElectricalRequiredDB,
						laborQuoteElectricalRequiredDetails,
						laborQuoteRequestorEmailCC1,
						laborQuoteRequestorEmailCC2,
						projectIs_OutOfState_Emails
					);

					string done = connection.Query<string>("dbo.[Update_LaborQuote_Header_OSQ]", param, commandType: CommandType.StoredProcedure).FirstOrDefault();
					_logger.LogWarning("Finished Update_LaborQuote_Header {code}", code);
					return done;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public bool EstimateLaborQuoteHeader(string submitType, string code, string address, string city, string state, string zip, string scope, string tools, string vendorNo, string taxable, string notes, string estimatorName,
			string customerName, int? complexProject, string noOfDays, string installTotal, string hsSurcharge, string surchargePct, int? status, string complete, int? truckCapacity, string projectId, string quoteNo, string region, bool? isScheduled)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					SqlCommand cmd1 = new SqlCommand();

					if (region == "OWP")
                    {
						cmd1 = db.GetCommandSQL(@"[dbo].[Update_LaborQuote_Header]");
					}
					else
                    {
						cmd1 = db.GetCommandSQL(@"[dbo].[Update_LaborQuote_Header_OSQ]");
					}					

					cmd1.CommandType = CommandType.StoredProcedure;
					string param1 = "@LaborQuoteCode";
					string param3 = "@LaborQuoteStatusLookUpID";
					string param6 = "@ProjectID";
					string param8 = "@AddressLine1";
					string param10 = "@AddressCity";
					string param11 = "@AddressState";
					string param12 = "@AddressZIP";
					string param21 = "@Scope";
					string param22 = "@EquipmentTools";
					string param23 = "@VendorNumberLookupID";
					string param25 = "@Taxable";
					string param26 = "@Notes";
					string param28 = "@EstimatorName";
					string param29 = "@ComplexProject";
					string param31 = "@NumberOfDays";
					string param32 = "@QuoteOrOrderNumber";
					string param65 = "@InstallTotal";
					string param66 = "@HealthAndSafetySurcharge";
					//string param67 = "@RegularHours";
					//string param68 = "@OTHours";
					//string param69 = "@DTHours";
					//string param70 = "@RegularHsRate";
					//string param71 = "@OTHsRate";
					//string param72 = "@DTHsRate";
					string param73 = "@SurchargePct";
					string param78 = "@TruckCapacity";
					string param83 = "@CustomerName";
					//string param84 = "@RegularHsLaborTypeLookupID";
					//string param85 = "@OTHoursLaborTypeLookupID";
					//string param86 = "@DTHoursLaborTypeLookupID";
					string param84 = "@IsScheduled";

					cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), code);
					cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), submitType == "Estimate" ? Convert.ToInt32(complete) : status);
					cmd1.Parameters.AddWithValue("@" + param6.Replace("@", ""), projectId);
					cmd1.Parameters.AddWithValue("@" + param8.Replace("@", ""), address);
					cmd1.Parameters.AddWithValue("@" + param10.Replace("@", ""), city);
					cmd1.Parameters.AddWithValue("@" + param11.Replace("@", ""), state);
					cmd1.Parameters.AddWithValue("@" + param12.Replace("@", ""), zip);
					cmd1.Parameters.AddWithValue("@" + param21.Replace("@", ""), scope);
					cmd1.Parameters.AddWithValue("@" + param22.Replace("@", ""), tools);
					cmd1.Parameters.AddWithValue("@" + param23.Replace("@", ""), Convert.ToInt32(vendorNo));
					cmd1.Parameters.AddWithValue("@" + param25.Replace("@", ""), Convert.ToBoolean(Convert.ToInt32(taxable)));
					cmd1.Parameters.AddWithValue("@" + param26.Replace("@", ""), notes);
					cmd1.Parameters.AddWithValue("@" + param28.Replace("@", ""), estimatorName);
					cmd1.Parameters.AddWithValue("@" + param29.Replace("@", ""), Convert.ToBoolean(complexProject));
					cmd1.Parameters.AddWithValue("@" + param31.Replace("@", ""), noOfDays);
					cmd1.Parameters.AddWithValue("@" + param32.Replace("@", ""), quoteNo);
					cmd1.Parameters.AddWithValue("@" + param65.Replace("@", ""), complexProject == 1 ? null : installTotal.Replace("$", ""));
					cmd1.Parameters.AddWithValue("@" + param66.Replace("@", ""), hsSurcharge.Replace("$", ""));
					//cmd1.Parameters.AddWithValue("@" + param67.Replace("@", ""), regHs);
					//cmd1.Parameters.AddWithValue("@" + param68.Replace("@", ""), otHs);
					//cmd1.Parameters.AddWithValue("@" + param69.Replace("@", ""), dtHs);
					//cmd1.Parameters.AddWithValue("@" + param70.Replace("@", ""), Convert.ToDecimal(regHourlyRate.Replace("$", "").Replace(",", ".")));
					//cmd1.Parameters.AddWithValue("@" + param71.Replace("@", ""), Convert.ToDecimal(otHourlyRate.Replace("$", "").Replace(",",".")));
					//cmd1.Parameters.AddWithValue("@" + param72.Replace("@", ""), Convert.ToDecimal(dtHourlyRate.Replace("$", "").Replace(",", ".")));
					cmd1.Parameters.AddWithValue("@" + param73.Replace("@", ""), Convert.ToDecimal(surchargePct.Replace("%", "").Replace(",", ".")));
					cmd1.Parameters.AddWithValue("@" + param78.Replace("@", ""), truckCapacity);
					cmd1.Parameters.AddWithValue("@" + param83.Replace("@", ""), customerName);
					//cmd1.Parameters.AddWithValue("@" + param84.Replace("@", ""), hsRegType);
					//cmd1.Parameters.AddWithValue("@" + param85.Replace("@", ""), hsOtType);
					//cmd1.Parameters.AddWithValue("@" + param86.Replace("@", ""), hsDtType);
					cmd1.Parameters.AddWithValue("@" + param84.Replace("@", ""), isScheduled);

					if (region == "OWP")
					{
						string param87 = "@IsEstimator";
						cmd1.Parameters.AddWithValue("@" + param87.Replace("@", ""), true);
					}

					db.SqlCommandToRow(cmd1);
					_logger.LogError("LQ # {0} - Status {1} - Estimator {2} - Region {3}", code, submitType == "Estimate" ? Convert.ToInt32(complete) : status, estimatorName, region);
					return true;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public bool SaveRates(List<HoursAndRates> hoursAndRates, int laborQuoteHeaderID, string region)
        {
            try
            {
				using (var connection = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
				{
					var hrDT = new System.Data.DataTable();
					hrDT.Columns.Add("RateTypeLookupID", typeof(int));
					hrDT.Columns.Add("RateValue", typeof(float));
					hrDT.Columns.Add("Hours", typeof(float));

					if (hoursAndRates != null && hoursAndRates.Count() > 0)
					{
						List<LookupGeneralWithValue> HourTypes = LoadLookupGeneralWithValue("LaborType", region);
						foreach (var hr in hoursAndRates)
						{
							var row = hrDT.NewRow();
							row["RateTypeLookupID"] = Convert.ToInt32(HourTypes.Where(x => x.Value == hr.Type).Select(x => x.LookupGeneralID).FirstOrDefault());
							row["RateValue"] = hr.Rate;
							row["Hours"] = hr.Hours;
							hrDT.Rows.Add(row);
						}
					}

					connection.Open();

					var param = new DynamicParameters();
					param.Add("@Company_Code", region == "OWP" ? "W" : "S");
					param.Add("@LaborQuoteHeaderID", laborQuoteHeaderID);
					param.Add("@HoursAndRatesTable", hrDT.AsTableValuedParameter("[dbo].[HoursAndRates]"));

					connection.Query("dbo.[Upsert_LaborQuote_HoursAndRates]", param, commandType: CommandType.StoredProcedure).ToList();

					return true;
				}				
            }
            catch (Exception e)
            {
                throw e;
            }
        }

		public bool EstimateLaborQuoteFurnitureInstallation(int id, string description, string quantity, string hoursPerQty, string hours, DateTime dtNow, string region)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Insert_LaborQuote_FurnitureInstallation]");
					cmd1.CommandType = CommandType.StoredProcedure;

					string param1 = "@LaborQuoteHeaderID";
					string param2 = "@Description";
					string param3 = "@Quantity";
					string param4 = "@Hours";
					string param5 = "@HoursPerQuantity";
					string param6 = "@CreatedOn";
					string param7 = "@Region";

					cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), id);
					cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), description);
					cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), quantity);
					cmd1.Parameters.AddWithValue("@" + param4.Replace("@", ""), hours);
					cmd1.Parameters.AddWithValue("@" + param5.Replace("@", ""), hoursPerQty);
					cmd1.Parameters.AddWithValue("@" + param6.Replace("@", ""), dtNow);
					cmd1.Parameters.AddWithValue("@" + param7.Replace("@", ""), region);

					db.SqlCommandToRow(cmd1);

					return true;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public bool EstimateLaborQuoteAddOns(int id, string description, string quantity, string hours, DateTime dtNow, string region)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Insert_LaborQuote_Addons]");
					cmd1.CommandType = CommandType.StoredProcedure;

					string param1 = "@LaborQuoteHeaderID";
					string param2 = "@Description";
					string param3 = "@Quantity";
					string param4 = "@Hours";
					string param5 = "@CreatedOn";
					string param6 = "@Region";

					cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), id);
					cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), description);
					cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), quantity);
					cmd1.Parameters.AddWithValue("@" + param4.Replace("@", ""), hours);
					cmd1.Parameters.AddWithValue("@" + param5.Replace("@", ""), dtNow);
					cmd1.Parameters.AddWithValue("@" + param6.Replace("@", ""), region);

					db.SqlCommandToRow(cmd1);

					return true;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public bool EstimateLaborQuoteMiscInstallationCharges(int id, string description, string cost, string quantity, string rate, DateTime dtNow, string region)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Insert_LaborQuote_MiscInstallationCharges]");
					cmd1.CommandType = CommandType.StoredProcedure;

					string param1 = "@LaborQuoteHeaderID";
					string param2 = "@Description";
					string param3 = "@Cost";
					string param4 = "@Quantity";
					string param5 = "@Rate";
					string param6 = "@CreatedOn";
					string param7 = "@Region";

					cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), id);
					cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), description);
					cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), Convert.ToDouble(cost));
					cmd1.Parameters.AddWithValue("@" + param4.Replace("@", ""), Convert.ToInt32(quantity));
					cmd1.Parameters.AddWithValue("@" + param5.Replace("@", ""), Convert.ToDouble(rate));
					cmd1.Parameters.AddWithValue("@" + param6.Replace("@", ""), dtNow);
					cmd1.Parameters.AddWithValue("@" + param7.Replace("@", ""), region);

					db.SqlCommandToRow(cmd1);

					return true;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public bool EstimateLaborQuoteHours(int id, int lookupId, bool isPw, float estimatedHs, float hourlyRate, float laborTotal, string region)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Insert_LaborQuote_Hour]");
					cmd1.CommandType = CommandType.StoredProcedure;

					string param1 = "@LaborQuoteHeaderID";
					string param2 = "@LaborBreakdownLookupID";
					string param3 = "@IsPW";
					string param4 = "@EstimatedHours";
					string param5 = "@HourlyRate";
					string param6 = "@LaborTotal";
					string param7 = "@Region";

					cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), id);
					cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), lookupId);
					cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), isPw);
					cmd1.Parameters.AddWithValue("@" + param4.Replace("@", ""), estimatedHs);
					cmd1.Parameters.AddWithValue("@" + param5.Replace("@", ""), hourlyRate);
					cmd1.Parameters.AddWithValue("@" + param6.Replace("@", ""), laborTotal);
					cmd1.Parameters.AddWithValue("@" + param7.Replace("@", ""), region);

					db.SqlCommandToRow(cmd1);

					return true;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<LaborQuoteHeader> GetLaborQuoteHeaders(string dateFrom, string dateTo, string laborQuoteCode, string laborQuoteStatus,
			string requestorName, string customerId, string projectId, string origLaborQuoteCode, string estimatorName, string quoOrdNo, string region)
		{
			try
			{
				using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
				{
					var r = db.Query<LaborQuoteHeader>(@"dbo.Get_LaborQuote_Header",
								   param: new
								   {
									   DateFrom = dateFrom,
									   DateTo = dateTo,
									   LaborQuoteCode = laborQuoteCode,
									   LaborQuoteStatusLookUpID = laborQuoteStatus,
									   RequestorName = requestorName,
									   CustomerNo = customerId,
									   ProjectID = projectId,
									   OriginalLaborQuoteCode = origLaborQuoteCode,
									   EstimatorName = estimatorName,
									   QuoteOrOrderNumber = quoOrdNo,
									   Region = region
								   },
								   commandType: CommandType.StoredProcedure);

					return r;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public List<HourAndRate> GetHourAndRates(string region, int laborQuoteHeaderID)
		{
			try
			{
				using (var connection = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
				{
					connection.Open();

					var param = new DynamicParameters();
					param.Add("@Company_Code", region == "OWP" ? "W" : "S");
					param.Add("@LaborQuoteHeaderID", laborQuoteHeaderID);

					List<HourAndRate> hr = connection.Query<HourAndRate>("dbo.[Get_LaborQuote_HoursAndRates]", param, commandType: CommandType.StoredProcedure).ToList();

					return hr;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		

		public List<SelectValues> GetOriginalLaborQuotes(string region)
		{
			using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
			{
				db.Open();
				var r = db.Query<LaborQuoteHeader>("dbo.Get_LaborQuote_Header", new { Region = region }, commandType: CommandType.StoredProcedure);

				if (r.Count() > 0)
				{
					var output = new List<SelectValues>();

					foreach (var row in r)
					{
						output.Add(new SelectValues
						{
							ID = row.LaborQuoteCode,
							Label = row.LaborQuoteCode
						});
					}

					return output.OrderBy(x => x.Label).ToList();
				}
				return null;
			}
		}

		public string GetLaborQuoteCode(string region)
		{
			string output = "";
			using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
			{
				SqlCommand sqlCommand = db.GetCommandSQL(@"EXEC dbo.Get_LaborQuote_LaborQuoteCode " + region);
				DataTable table = db.SqlCommandToTable(sqlCommand);

				output = table.Rows[0]["LaborQuoteCode"].ToString();
			}

			return output;
		}

		public EstimatorSummary GetEstimatorSummary(string region)
		{
			try
			{
				using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
				{
					var r = db.Query<EstimatorSummary>(@"dbo.Get_LaborQuote_SummaryLines", new { Region = region }, commandType: CommandType.StoredProcedure).FirstOrDefault();

					return r;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<SelectValues> LoadEstimatorList(string region)
		{
			using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
			{
				db.Open();
				var r = db.Query<Estimator>("dbo.Get_LaborQuote_Estimators", new { Region = region }, commandType: CommandType.StoredProcedure);

				if (r.Count() > 0)
				{
					var output = new List<SelectValues>();

					foreach (var row in r)
					{
						output.Add(new SelectValues
						{
							ID = row.EstimatorName,
							Label = row.EstimatorName
						});
					}

					return output.OrderBy(x => x.Label).ToList();
				}
				return null;
			}
		}

		public IEnumerable<dynamic> GetCompletedQuotes(string reportType, List<string> estimators, string dateFrom, string dateTo, string region)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					var estimatorsDT = new System.Data.DataTable();
					estimatorsDT.Columns.Add("ID", typeof(string));

					if (estimators != null && estimators.Any())
					{
						foreach (var est in estimators)
						{
							var row = estimatorsDT.NewRow();
							row["ID"] = est;
							estimatorsDT.Rows.Add(row);
						}
					};

					SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_LaborQuote_EstimatorStatsReport]");
					cmd1.CommandType = CommandType.StoredProcedure;

					string param1 = "@ReportType";
					string param2 = "@DateFrom";
					string param3 = "@DateTo";
					string param4 = "@EstimatorName_List";
					string param5 = "@Region";

					cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), reportType);
					cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), dateFrom);
					cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), dateTo);
					cmd1.Parameters.AddWithValue("@" + param4.Replace("@", ""), estimatorsDT);
					cmd1.Parameters.AddWithValue("@" + param5.Replace("@", ""), region);
					cmd1.Parameters["@" + param4.Replace("@", "")].SqlDbType = SqlDbType.Structured;

					DataTable dt1 = db.SqlCommandToTable(cmd1);

					switch (reportType)
					{
						case "M":
							List<CompletedQuotesByMonth> output = new List<CompletedQuotesByMonth>();
							if (dt1 != null && dt1.Rows.Count > 0)
							{
								CompletedQuotesByMonth item = null;
								foreach (DataRow row in dt1.Rows)
								{
									item = new CompletedQuotesByMonth
									{
										Month = clsLibrary.dBReadString(row["Month"]),
										Total_Budget = clsLibrary.dBReadDouble(row["Total_Budget"]),
										Count = clsLibrary.dBReadInt(row["Count"]),
										RevisionCount = clsLibrary.dBReadInt(row["RevisionCount"])
									};
									output.Add(item);
								}
							}
							return output;

						case "SP":
							List<CompletedQuotesByServiceProvider> output2 = new List<CompletedQuotesByServiceProvider>();
							if (dt1 != null && dt1.Rows.Count > 0)
							{
								CompletedQuotesByServiceProvider item = null;
								foreach (DataRow row in dt1.Rows)
								{
									item = new CompletedQuotesByServiceProvider
									{
										ServiceProvider = clsLibrary.dBReadString(row["ServiceProvider"]),
										Total_Budget = clsLibrary.dBReadDouble(row["Total_Budget"]),
										Count = clsLibrary.dBReadInt(row["Count"]),
										RevisionCount = clsLibrary.dBReadInt(row["RevisionCount"])
									};
									output2.Add(item);
								}
							}
							return output2;

						case "C":
							List<CompletedQuotesByCustomer> output3 = new List<CompletedQuotesByCustomer>();
							if (dt1 != null && dt1.Rows.Count > 0)
							{
								CompletedQuotesByCustomer item = null;
								foreach (DataRow row in dt1.Rows)
								{
									item = new CompletedQuotesByCustomer
									{
										CustomerNumber = clsLibrary.dBReadString(row["CustomerNumber"]),
										CustomerName = clsLibrary.dBReadString(row["CustomerName"]),
										Total_Budget = clsLibrary.dBReadDouble(row["Total_Budget"]),
										Count = clsLibrary.dBReadInt(row["Count"]),
										RevisionCount = clsLibrary.dBReadInt(row["RevisionCount"])
									};
									output3.Add(item);
								}
							}
							return output3;

						case "R":
							List<CompletedQuotesByRequestor> output4 = new List<CompletedQuotesByRequestor>();
							if (dt1 != null && dt1.Rows.Count > 0)
							{
								CompletedQuotesByRequestor item = null;
								foreach (DataRow row in dt1.Rows)
								{
									item = new CompletedQuotesByRequestor
									{
										Requestor = clsLibrary.dBReadString(row["Requestor"]),
										Total_Budget = clsLibrary.dBReadDouble(row["Total_Budget"]),
										Count = clsLibrary.dBReadInt(row["Count"]),
										RevisionCount = clsLibrary.dBReadInt(row["RevisionCount"])
									};
									output4.Add(item);
								}
							}
							return output4;

						case "AT":
							List<CompletedQuotesByEstimator> output5 = new List<CompletedQuotesByEstimator>();
							if (dt1 != null && dt1.Rows.Count > 0)
							{
								CompletedQuotesByEstimator item = null;
								foreach (DataRow row in dt1.Rows)
								{
									item = new CompletedQuotesByEstimator
									{
										CompletedBy = clsLibrary.dBReadString(row["CompletedBy"]),
										Total_Budget = clsLibrary.dBReadDouble(row["Total_Budget"]),
										Count = clsLibrary.dBReadInt(row["Count"]),
										RevisionCount = clsLibrary.dBReadInt(row["RevisionCount"])
									};
									output5.Add(item);
								}
							}
							return output5;

						case "REV":
							List<RevisionReasons> output6 = new List<RevisionReasons>();
							if (dt1 != null && dt1.Rows.Count > 0)
							{
								RevisionReasons item = null;
								foreach (DataRow row in dt1.Rows)
								{
									item = new RevisionReasons
									{
										LaborQuoteCode = clsLibrary.dBReadString(row["LaborQuoteCode"]),
										RequestorName = clsLibrary.dBReadString(row["RequestorName"]),
										CustomerName = clsLibrary.dBReadString(row["CustomerName"]),
										RevisionReason = clsLibrary.dBReadString(row["RevisionReason"])
									};
									output6.Add(item);
								}
							}
							return output6;

						default:
							return null;
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public void CancelQuote(string quoteNo, string region, string canceled)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					SqlCommand cmd1 = new SqlCommand();
					if (region == "OWP")
                    {
						cmd1 = db.GetCommandSQL(@"[dbo].[Update_LaborQuote_Header]");
					} else
                    {
						cmd1 = db.GetCommandSQL(@"[dbo].[Update_LaborQuote_Header_OSQ]");
					}
					
					cmd1.CommandType = CommandType.StoredProcedure;

					string param1 = "@LaborQuoteCode";
					string param2 = "@LaborQuoteStatusLookUpID";

					cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), quoteNo);
					cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), Convert.ToInt32(canceled));

					db.SqlCommandToRow(cmd1);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public Address GetLaborQuoteAddress(int orderNo, char orderType, string region)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_LaborQuote_Address]");
					cmd1.CommandType = CommandType.StoredProcedure;

					string param1 = "@Order_No";
					string param2 = "@Order_Type";
					string param3 = "@Region";

					cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), orderNo);
					cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), orderType);
					cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), region);

					DataTable dt1 = db.SqlCommandToTable(cmd1);

					Address output = new Address();
					if (dt1 != null && dt1.Rows.Count > 0)
					{
						output.AddressLine1 = clsLibrary.dBReadString(dt1.Rows[0]["Address_Line_1"]);
						output.City = clsLibrary.dBReadString(dt1.Rows[0]["City"]);
					}

					return output;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public void SetEstimatorFromDashboard(string code, string estimatorName, string region)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					SqlCommand cmd1 = new SqlCommand();
					if (region == "OWP")
					{
						cmd1 = db.GetCommandSQL(@"[dbo].[Update_LaborQuote_Header]");
					} else
                    {
						cmd1 = db.GetCommandSQL(@"[dbo].[Update_LaborQuote_Header_OSQ]");
					}
					cmd1.CommandType = CommandType.StoredProcedure;

					string param1 = "@LaborQuoteCode";
					string param28 = "@EstimatorName";

					cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), code);
					cmd1.Parameters.AddWithValue("@" + param28.Replace("@", ""), estimatorName);


					db.SqlCommandToRow(cmd1);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		private string GetLaborQuoteTypes(string[] laborQuoteTypes)
		{
			string csvLaborQuoteTypes = "";
			if (laborQuoteTypes.Count() > 0)
			{
				foreach (var type in laborQuoteTypes)
				{
					csvLaborQuoteTypes += type + ",";
				}
			}

			return csvLaborQuoteTypes.TrimEnd(',');
		}

		private string GetLaborQuoteUnionVendors(string[] laborQuoteUnionVendors, List<LookupGeneralWithValue> unionVendorList)
		{
			string csvLaborQuoteUnionVendors = "";
			if (laborQuoteUnionVendors.Count() > 0)
			{
				foreach (var unionVendor in laborQuoteUnionVendors)
				{
					csvLaborQuoteUnionVendors += unionVendorList.Where(x => x.Value == unionVendor).Select(x => x.LookupGeneralID).FirstOrDefault() + ",";
				}
			}

			return csvLaborQuoteUnionVendors.TrimEnd(',');
		}

		private string GetLaborQuoteUnionVendorsOSQ(string[] laborQuoteUnionVendors)
		{
			string csvLaborQuoteUnionVendors = "";
			if (laborQuoteUnionVendors.Count() > 0)
			{
				foreach (var unionVendor in laborQuoteUnionVendors)
				{
					csvLaborQuoteUnionVendors += unionVendor + ",";
				}
			}

			return csvLaborQuoteUnionVendors.TrimEnd(',');
		}

		private string GetLaborQuoteProductFroms(string[] laborQuoteProductFroms)
		{
			string csvLaborQuoteProductFroms = "";
			if (laborQuoteProductFroms.Count() > 0)
			{
				foreach (var pf in laborQuoteProductFroms)
				{
					csvLaborQuoteProductFroms += pf + ",";
				}
			}

			return csvLaborQuoteProductFroms.TrimEnd(',');
		}

		private string GetLaborQuoteWalls(string[] laborQuoteWalls)
		{
			string csvLaborQuoteWalls = "";
			if (laborQuoteWalls.Count() > 0)
			{
				foreach (var wall in laborQuoteWalls)
				{
					csvLaborQuoteWalls += wall + ",";
				}
			}

			return csvLaborQuoteWalls.TrimEnd(',');
		}

		private string GetLaborQuoteProtections(string[] laborQuoteProtections)
		{
			string csvLaborQuoteProtections = "";
			if (laborQuoteProtections.Count() > 0)
			{
				foreach (var prot in laborQuoteProtections)
				{
					csvLaborQuoteProtections += prot + ",";
				}
			}

			return csvLaborQuoteProtections.TrimEnd(',');
		}

		private string GetLaborQuoteElevators(string[] laborQuoteElevators)
		{
			string csvLaborQuoteElevators = "";
			if (laborQuoteElevators.Count() > 0)
			{
				foreach (var elev in laborQuoteElevators)
				{
					csvLaborQuoteElevators += elev + ",";
				}
			}

			return csvLaborQuoteElevators.TrimEnd(',');
		}

		private string GetLaborQuotePowers(string[] laborQuotePowers)
		{
			string csvLaborQuotePowers = "";
			if (laborQuotePowers.Count() > 0)
			{
				foreach (var power in laborQuotePowers)
				{
					csvLaborQuotePowers += power + ",";
				}
			}

			return csvLaborQuotePowers.TrimEnd(',');
		}

		private string GetLaborQuoteProductTypes(string[] laborQuoteProductTypes)
		{
			string csvLaborQuoteProductTypes = "";
			if (laborQuoteProductTypes.Count() > 0)
			{
				foreach (var pt in laborQuoteProductTypes)
				{
					csvLaborQuoteProductTypes += pt + ",";
				}
			}

			return csvLaborQuoteProductTypes.TrimEnd(',');
		}

		private string GetLaborQuoteProductIs(string[] laborQuoteProductIs)
		{
			string csvLaborQuoteProductIs = "";
			if (laborQuoteProductIs.Count() > 0)
			{
				foreach (var pi in laborQuoteProductIs)
				{
					csvLaborQuoteProductIs += pi + ",";
				}
			}

			return csvLaborQuoteProductIs.TrimEnd(',');
		}

		private string GetLaborQuoteTypeOfScreenings(string[] laborQuoteTypeOfScreening)
		{
			string csvLaborQuoteTypeOfScreenings = "";
			if (laborQuoteTypeOfScreening.Count() > 0)
			{
				foreach (var type in laborQuoteTypeOfScreening)
				{
					csvLaborQuoteTypeOfScreenings += type + ",";
				}
			}

			return csvLaborQuoteTypeOfScreenings.TrimEnd(',');
		}

		public ProjectData GetProjectData(string projectId, string region)
		{
			try
			{
				using (var connection = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
				{
					connection.Open();

					var param = new DynamicParameters();
					param.Add("@Project_ID", projectId);
					param.Add("@Region", region);

					ProjectData pd = connection.QueryFirstOrDefault<ProjectData>("dbo.Get_LaborQuote_HedbergData", param, commandType: CommandType.StoredProcedure);

					return pd;
				}				
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public string GetVendorEmail(string vendor)
        {
			try
			{
				using (var connection = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
				{
					connection.Open();

					var param = new DynamicParameters();
					param.Add("@App", "QI");
					param.Add("@Vnd_No", vendor);
					param.Add("@DebugMode", false);

					LQVendorEmail email = connection.QueryFirstOrDefault<LQVendorEmail>("dbo.Get_Vendor_Email", param, commandType: CommandType.StoredProcedure);

					return email.Vnd_Email;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}

	class CustomersModel
	{
		public string customer_no { get; set; }
		public string name { get; set; }
	}

	class SalespersonModel
	{
		public string salesperson_id { get; set; }
		public string salesperson_info { get; set; }
	}
}
