﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.LaborQuoteAutomationCommons
{
    public class LaborQuoteHeader
	{
		public int LaborQuoteHeaderID { get; set; }
		public string LaborQuoteCode { get; set; }
		public string Status { get; set; }
		public string RequestorName { get; set; }
		public string CustomerName { get; set; }
		public DateTime CreatedOn { get; set; }
		public DateTime? ModifiedOn { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressCity { get; set; }
		public string CustomerNumber { get; set; }
		public string AddressLine2 { get; set; }
		public string LaborQuoteStatusLookUpID { get; set; }
		public string RequestorEmail { get; set; }
		public string ProjectID { get; set; }
		public string AddressState { get; set; }
		public string AddressZIP { get; set; }
		public char QuoteOrOrder { get; set; }
		public string SalespersonID { get; set; }
		public string SalespersonName { get; set; }
		public string Coordinator { get; set; }
		public string ProjectManager { get; set; }
		public bool DirectoryCreated { get; set; }
		public string DaysForInstall { get; set; }
		public string CreatedBy { get; set; }
		public string ModifiedBy { get; set; }
		public string LaborQuoteTypeLookUpIDs { get; set; }
		public string[] LaborQuoteTypes { get; set; }
		public string QuoteOrOrderNumber { get; set; }
		public DateTime? TargetDate { get; set; }
		public string LaborQuoteUnionVendorIDs { get; set; }
		public string[] LaborQuoteUnionVendors { get; set; }
		public string LaborQuoteUnionVendorsADD { get; set; }
		public string UnionJobsEmailAddress { get; set; }
		public string ScopeOfWork { get; set; }
		public string ProjectIs { get; set; }
		public string OriginalLaborQuoteCode { get; set; }
		public string LaborQuoteReceiveDeliverLookupID { get; set; }
		public string LaborQuoteReceiveDeliverDayTimeLookupID { get; set; }
		public string LaborQuoteInstallationLookupID { get; set; }
		public string LaborQuoteInstallationTimeLookupID { get; set; }
		public string LaborQuoteProductFromLookUpIDs { get; set; }
		public string[] LaborQuoteProductFroms { get; set; }
		public string LaborQuoteUnloadAtLookupID { get; set; }
		public string LaborQuoteSiteIsLookupID { get; set; }
		public string LaborQuoteWallsAreLookUpIDs { get; set; }
		public string[] LaborQuoteWallsAres { get; set; }
		public string LaborQuoteProtectionLookUpIDs { get; set; }
		public string[] LaborQuoteProtections { get; set; }
		public string FeetOfMasonite { get; set; }
		public string LaborQuoteElevatorLookUpIDs { get; set; }
		public string[] LaborQuoteElevators { get; set; }
		public bool? CarryUp { get; set; }
		public string NumberOfFlights { get; set; }
		public string LaborQuotePowerLookUpIDs { get; set; }
		public string[] LaborQuotePowers { get; set; }
		public bool? SecurityClearanceRequired { get; set; }
		public bool? ElectricalPerimitRequired { get; set; }
		public string LaborQuoteProductTypeLookUpIDs { get; set; }
		public string LaborQuoteProductType { get; set; }
		public string[] LaborQuoteProductTypes { get; set; }
		public string ProductManufacturer { get; set; }
		public string ProductSeries { get; set; }
		public string ProductWorkstations { get; set; }
		public string ProductPanelsOnly { get; set; }
		public string ProductPrivateOffices { get; set; }
		public string ProductConferenceRooms { get; set; }
		public string ProductExamRooms { get; set; }
		public string ProductClassRooms { get; set; }
		public string ProductOther { get; set; }
		public string LaborQuoteProductIsLookUpIDs { get; set; }
		public string[] LaborQuoteProductIs { get; set; }
		public string ProductIsAdditionalInfo { get; set; }
		public string EstimatorName { get; set; }
		public DateTime? DueDate { get; set; }
		public DateTime? ExpirationDate { get; set; }
		public int? NumberOfDays { get; set; }
		public string FullLaborQuoteCode { get; set; }
		public bool Taxable { get; set; }
		public double InstallTotal { get; set; }
		public double HealthAndSafetySurcharge { get; set; }
		public string EquipmentTools { get; set; }
		public string Scope { get; set; }
		public IEnumerable<FurnitureInstallation> FurnitureInstallationList { get; set; }
		public IEnumerable<Addons> AddonsList { get; set; }
		public IEnumerable<MiscCharges> MiscChargesList { get; set; }
		public IEnumerable<Addons> CustomAddonsList { get; set; }
		public IEnumerable<MiscCharges> CustomMiscChargesList { get; set; }
		public IEnumerable<FurnitureInstallation> CustomFurnitureInstallationList { get; set; }
		public List<AttachedFile> AttachedFiles { get; set; }
		public string VendorNumberLookupID { get; set; }
		public string Notes { get; set; }
		public bool ComplexProject { get; set; }
		public string RegularHours { get; set; }
		public string OTHours { get; set; }
		public string DTHours { get; set; }
		public string RegularHsRate { get; set; }
		public string OTHsRate { get; set; }
		public string DTHsRate { get; set; }
		public string SurchargePct { get; set; }
		public string LaborQuoteTypeOfScreeningLookUpIDs { get; set; }
		public string[] LaborQuoteTypeOfScreenings { get; set; }
		public bool? ElevatorNeeded { get; set; }
		public bool? ProductNeedToBeCleanedOrSanitized { get; set; }
		public string RevisionReason { get; set; }
		public int TruckCapacity { get; set; }
		public bool? YPRequired { get; set; }
		public bool? OutOfStateProject { get; set; }
		public string QtyOfIncludedFloors { get; set; }
		public int? LaborQuoteScenesPerFloorLookUpID { get; set; }
		public int? RegularHsLaborTypeLookupID { get; set; }
		public int? OTHoursLaborTypeLookupID { get; set; }
		public int? DTHoursLaborTypeLookupID { get; set; }
		public bool? ElectricalRequired { get; set; }
		public string ElectricalAdditionalDetails { get; set; }
		public string RequestorEmailCC { get; set; }
		public string RequestorEmailCC2 { get; set; }
		public float LaborTotal { get; set; }
		public List<HourAndRate> HourAndRates { get; set; }
		public string ProjectIs_OutOfState_Emails { get; set; }
		public string IsScheduled { get; set; }
	}
}
