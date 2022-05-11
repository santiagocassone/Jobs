using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.LaborQuoteAutomationCommons
{
    public class EstimationData
	{
		public string Address { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Zip { get; set; }
		public string LaborQuoteNo { get; set; }
		public string Scope { get; set; }
		public string Equipments { get; set; }
		public string VendorNo { get; set; }
		public string Taxable { get; set; }
		public string Notes { get; set; }
		public string EstimatedBy { get; set; }
		public string CustomerName { get; set; }
		public int? ComplexProject { get; set; }
		public string NoOfDays { get; set; }
		public string InstallTotal { get; set; }
		public string HsSurcharge { get; set; }
		public string SurchargePct { get; set; }
		public float? RegHs { get; set; }
		public float? OtHs { get; set; }
		public float? DtHs { get; set; }
		public string RegHourlyRate { get; set; }
		public string OtHourlyRate { get; set; }
		public string DtHourlyRate { get; set; }
		public int? LaborQuoteStatus { get; set; }
		public string CurrLaborQuoteStatus { get; set; }
		public int? TruckCap { get; set; }
		public string ProjectId { get; set; }
		public string QuoteNo { get; set; }
		public string HsRegType { get; set; }
		public string HsOtType { get; set; }
		public string HsDtType { get; set; }
		public string SystemQty { get; set; }
		public string SystemHsXQty { get; set; }
		public string SystemHs { get; set; }
		public string DeskAndTablesQty { get; set; }
		public string DeskAndTablesHsXQty { get; set; }
		public string DeskAndTablesHs { get; set; }
		public string StorageAndMiscQty { get; set; }
		public string StorageAndMiscHsXQty { get; set; }
		public string StorageAndMiscHs { get; set; }
		public string SeatingQty { get; set; }
		public string SeatingHsXQty { get; set; }
		public string SeatingHs { get; set; }
		public string StairDistStagingQty { get; set; }
		public string StairDistStagingHs { get; set; }
		public string WallChannelDrywallQty { get; set; }
		public string WallChannelDrywallHs { get; set; }
		public string WallChannelConcreteQty { get; set; }
		public string WallChannelConcreteHs { get; set; }
		public string GrommetHoleCutsQty { get; set; }
		public string GrommetHoleCutsHs { get; set; }
		public string WorksurfaceCutsQty { get; set; }
		public string WorksurfaceCutsHs { get; set; }
		public string DeliveryTimeQty { get; set; }
		public string DeliveryTimeHs { get; set; }
		public string WarehouseQty { get; set; }
		public string WarehouseHs { get; set; }
		public string NewLabor1 { get; set; }
		public string NewLabor1Qty { get; set; }
		public string NewLabor1Hs { get; set; }
		public string NewLabor2 { get; set; }
		public string NewLabor2Qty { get; set; }
		public string NewLabor2Hs { get; set; }
		public string NewLabor3 { get; set; }
		public string NewLabor3Qty { get; set; }
		public string NewLabor3Hs { get; set; }
		public string NewLabor4 { get; set; }
		public string NewLabor4Qty { get; set; }
		public string NewLabor4Hs { get; set; }
		public string Truck { get; set; }
		public string TruckQty { get; set; }
		public string TruckRate { get; set; }
		public string MileageParking { get; set; }
		public string Supplies { get; set; }
		public string LoadCrew { get; set; }
		public string LoadCrewQty { get; set; }
		public string LoadCrewRate { get; set; }
		public string Disposal { get; set; }
		public string NewMisc1 { get; set; }
		public string NewMisc1Dollars { get; set; }
		public string NewMisc2 { get; set; }
		public string NewMisc2Dollars { get; set; }
		public string NewMisc3 { get; set; }
		public string NewMisc3Dollars { get; set; }
		public string NewMisc4 { get; set; }
		public string NewMisc4Dollars { get; set; }
		public string NewMisc5 { get; set; }
		public string NewMisc5Dollars { get; set; }
		public string NewMisc6 { get; set; }
		public string NewMisc6Dollars { get; set; }
		public float CommonTotalDlls { get; set; }
		public string RegularTotalHs { get; set; }
		public string RegularHourlyRate { get; set; }
		public string OvertimeTotalHs { get; set; }
		public string OvertimeHourlyRate { get; set; }
		public string DoubletimeTotalHs { get; set; }
		public string DoubletimeHourlyRate { get; set; }
		public string PwTotalHs { get; set; }
		public string PwHourlyRate { get; set; }
		public string PwOtTotalHs { get; set; }
		public string PwOtHourlyRate { get; set; }
		public string PwDtTotalHs { get; set; }
		public string PwDtHourlyRate { get; set; }
		public List<string> NewFurnItems { get; set; }
		public string Region { get; set; }
		public List<HoursAndRates> HoursAndRates { get; set; }
		public int LaborQuoteHeaderID { get; set; }
		public bool? IsScheduled { get; set; }
	}

	public class HoursAndRates
    {
		public string Type { get; set; }
		public float Hours { get; set; }
		public float Rate { get; set; }
    }
}
