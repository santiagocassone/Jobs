using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OWPApplications.Models.LaborQuoteAutomationCommons;

namespace OWPApplications.Models.LaborQuoteAutomation
{
	public class LaborQuoteAutomationViewModel
	{
		public IEnumerable<LaborQuoteHeader> LaborQuotes { get; set; }
		public IEnumerable<SelectValues> LaborQuoteStatuses { get; set; }
		public IEnumerable<LookupGeneralWithValue> LaborQuoteTypes { get; set; }
		public IEnumerable<LookupGeneralWithValue> LaborQuoteUnionVendors { get; set; }
		public IEnumerable<LaborQuoteVendor> LaborQuoteVendors { get; set; }
		public IEnumerable<SelectValues> Customers { get; set; }
		public IEnumerable<SelectValues> Salespersons { get; set; }
		public bool IsRevision { get; set; }
		public bool ShowResults { get; set; }
		public string Success { get; set; }
		public bool IsReadOnly { get; set; }
		public bool IsReadOnlyForEstimator { get; set; }
		public bool FromDashboard { get; set; }
		public LaborQuoteHeader LaborQuote { get; set; }
		public string OriginalLaborQuoteNo { get; set; }
		public IEnumerable<LookupGeneralWithValue> ReceiveDeliverList { get; set; }
		public IEnumerable<LookupGeneralWithValue> ReceiveDeliverTimeList { get; set; }
		public IEnumerable<LookupGeneralWithValue> InstallationList { get; set; }
		public IEnumerable<LookupGeneralWithValue> InstallationTimeList { get; set; }
		public IEnumerable<LookupGeneralWithValue> ProductFromList { get; set; }
		public IEnumerable<LookupGeneralWithValue> UnloadAtList { get; set; }
		public IEnumerable<LookupGeneralWithValue> SiteIsList { get; set; }
		public IEnumerable<LookupGeneralWithValue> ElevatorList { get; set; }
		public IEnumerable<LookupGeneralWithValue> PowerList { get; set; }
		public IEnumerable<LookupGeneralWithValue> ProductTypeList { get; set; }
		public IEnumerable<LookupGeneralWithValue> ProductIsList { get; set; }
		public IEnumerable<LookupGeneralWithValue> WallsAreList { get; set; }
		public IEnumerable<LookupGeneralWithValue> ProtectionList { get; set; }
		public IEnumerable<LookupGeneralWithValue> TypeOfScreeningList { get; set; }
		public IEnumerable<LookupGeneralWithValue> HourTypes { get; set; }
		public IEnumerable<LookupGeneralWithValue> HourTypesValues { get; set; }
		public IEnumerable<LookupGeneralWithValue> Breakdown { get; set; }
		public string RevisionCode { get; set; }
		public bool ShowForm { get; set; }
		public bool NoLaborQuoteFound { get; set; }
		public string Saved { get; set; }
		public EstimatorSummary EstimatorSummary { get; set; }
		public IEnumerable<LaborQuoteHeader> EstimatorPendingLaborQuotes { get; set; }
		public IEnumerable<LaborQuoteHeader> EstimatorCompletedLaborQuotes { get; set; }
		public List<CompletedQuotesByMonth> CompletedQuotesByMonth { get; set; }
		public List<CompletedQuotesByServiceProvider> CompletedQuotesByServiceProviderOWP { get; set; }
		public List<CompletedQuotesByServiceProvider> CompletedQuotesByServiceProviderOSQ { get; set; }
		public List<CompletedQuotesByCustomer> CompletedQuotesByCustomer { get; set; }
		public List<CompletedQuotesByRequestor> CompletedQuotesByRequestor { get; set; }
		public List<CompletedQuotesByEstimator> CompletedQuotesByEstimator { get; set; }
		public List<RevisionReasons> RevisionReasons { get; set; }
		public IEnumerable<Estimator> EstimatorList { get; set; }
		public IEnumerable<SelectValues> EstimatedByList { get; set; }
		public string CurrentStatus { get; set; }
		public IEnumerable<LookupGeneral> ScenesPerFloor { get; set; }
	}
}