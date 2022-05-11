using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.DefaultPricingReport
{
    public class DefaultPricingReportViewModel
    {
        public bool ShowResults { get; set; }
        public bool ShowMainView { get; set; }
        public List<SelectValues> Locations { get; internal set; }
        public List<SelectValues> TimePeriods { get; internal set; }
        public string Location { get; internal set; }
        public string TimePeriod { get; internal set; }
        public string DateFrom { get; internal set; }
        public string DateTo { get; internal set; }
        public string VendorNo { get; internal set; }
        public string SalespersonID { get; internal set; }
        public string CustomerNo { get; internal set; }
        public string ExcludeVendors { get; internal set; }
        public string DetailsView { get; internal set; }
        public IEnumerable<SalespersonFlowModel> SalespersonFlow { get; internal set; }
        public IEnumerable<VendorFlowModel> VendorFlow { get; internal set; }
        public IEnumerable<CustomerFlowModel> CustomerFlow { get; internal set; }
        public IEnumerable<OrderFlowModel> OrderFlow { get; internal set; }
        public SummaryModel Summary { get; internal set; }
        public SummaryComparisonModel SummaryComparison { get; internal set; }

        public class SalespersonFlowModel
        {
            public string Salesperson { get; set; }
            public string SalesPersonID { get; set; }
            public string TotalCost { get; set; }
            public string TotalSell { get; set; }
            public string TotalQTY { get; set; }
            public string GP { get; set; }
            public string AverageGPPct { get; set; }
            public string NumOfVendors { get; set; }
            public double DifferenceDollars { get; set; }
            public string DifferencePct { get; set; }
            public string BackgroundColor { get; set; }
            public string DefaultVendorGPPct { get; set; }
            public string VendorID { get; set; }
        }

        public class FormattedSalespersonFlowModel
        {
            private string totalSell;
            private string totalCost;
            private string gP;
            private string differenceDollars;
            private string averageGPPct;
            private string totalQTY;

            public string Salesperson { get; set; }
            public string SalesPersonID { get; set; }
            public string TotalSell { get { return totalSell; } set { totalSell = Convert.ToDouble(value).ToString("C"); } }
            public string TotalCost { get { return totalCost; } set { totalCost = Convert.ToDouble(value).ToString("C"); } }
            public string GP { get { return gP; } set { gP = Convert.ToDouble(value).ToString("C"); } }
            public string AverageGPPct { get { return averageGPPct; } set { averageGPPct = (Convert.ToDouble(value) / 100).ToString("P"); } }
            public string TotalQTY { get { return totalQTY; } set { totalQTY = Convert.ToDouble(value).ToString("N0"); } }
            public string DifferenceDollars { get { return differenceDollars; } set { differenceDollars = Convert.ToDouble(value).ToString("C"); } }
        }

        public class VendorFlowModel
        {
            public string Vendor { get; set; }
            public string VendorID { get; set; }
            public string TotalCost { get; set; }
            public string TotalSell { get; set; }
            public string TotalQTY { get; set; }
            public string GP { get; set; }
            public string AverageGPPct { get; set; }
            public string DefaultVendorGPPct { get; set; }
            public double DifferenceDollars { get; set; }
            public string DifferencePct { get; set; }
            public string NumOfSP { get; set; }
            public string BackgroundColor { get; set; }
        }

        public class FormattedVendorFlowModel
        {
            private string totalSell;
            private string totalCost;
            private string averageGPPct;
            private string defaultVendorGPPct;
            private string differenceDollars;
            private string differencePct;
            private string totalQTY;

            public string Vendor { get; set; }
            public string VendorID { get; set; }
            public string TotalSell { get { return totalSell; } set { totalSell = Convert.ToDouble(value).ToString("C"); } }
            public string TotalCost { get { return totalCost; } set { totalCost = Convert.ToDouble(value).ToString("C"); } }
            public string AverageGPPct { get { return averageGPPct; } set { averageGPPct = (Convert.ToDouble(value) / 100).ToString("P"); } }
            public string DefaultVendorGPPct { get { return defaultVendorGPPct; } set { defaultVendorGPPct = (Convert.ToDouble(value) / 100).ToString("P"); } }
            public string TotalQTY { get { return totalQTY; } set { totalQTY = Convert.ToDouble(value).ToString("N0"); } }
            public string DifferenceDollars { get { return differenceDollars; } set { differenceDollars = Convert.ToDouble(value).ToString("C"); } }
            public string DifferencePct { get { return differencePct; } set { differencePct = (Convert.ToDouble(value) / 100).ToString("P"); } }
        }

        public class CustomerFlowModel
        {
            public string Customer_Name { get; set; }
            public string Customer_no { get; set; }
            public string TotalCost { get; set; }
            public string TotalSell { get; set; }
            public string TotalQTY { get; set; }
            public string GP { get; set; }
            public string AverageGPPct { get; set; }
            public string DefaultVendorGPPct { get; set; }
            public double DifferenceDollars { get; set; }
            public string DifferencePct { get; set; }
            public string BackgroundColor { get; set; }
        }

        public class FormattedCustomerFlowModel
        {
            private string totalSell;
            private string totalCost;
            private string gP;
            private string differenceDollars;
            private string averageGPPct;
            private string defaultVendorGPPct;
            private string totalQTY;

            public string Customer_Name { get; set; }
            public string Customer_no { get; set; }
            public string TotalSell { get { return totalSell; } set { totalSell = Convert.ToDouble(value).ToString("C"); } }
            public string TotalCost { get { return totalCost; } set { totalCost = Convert.ToDouble(value).ToString("C"); } }
            public string GP { get { return gP; } set { gP = Convert.ToDouble(value).ToString("C"); } }
            public string AverageGPPct { get { return averageGPPct; } set { averageGPPct = (Convert.ToDouble(value) / 100).ToString("P"); } }
            public string DefaultVendorGPPct { get { return defaultVendorGPPct; } set { defaultVendorGPPct = (Convert.ToDouble(value) / 100).ToString("P"); } }
            public string TotalQTY { get { return totalQTY; } set { totalQTY = Convert.ToDouble(value).ToString("N0"); } }
            public string DifferenceDollars { get { return differenceDollars; } set { differenceDollars = Convert.ToDouble(value).ToString("C"); } }
        }

        public class OrderFlowModel
        {
            public string Order_No { get; set; }
            public string TotalCost { get; set; }
            public string TotalSell { get; set; }
            public string TotalQTY { get; set; }
            public string GP { get; set; }
            public string AverageGPPct { get; set; }
            public string DefaultVendorGPPct { get; set; }
            public double DifferenceDollars { get; set; }
            public string DifferencePct { get; set; }
            public string BackgroundColor { get; set; }
        }

        public class FormattedOrderFlowModel
        {
            private string totalSell;
            private string totalCost;
            private string gP;
            private string differenceDollars;
            private string averageGPPct;
            private string defaultVendorGPPct;
            private string differencePct;
            private string totalQTY;

            public string Order_No { get; set; }
            public string TotalSell { get { return totalSell; } set { totalSell = Convert.ToDouble(value).ToString("C"); } }
            public string TotalCost { get { return totalCost; } set { totalCost = Convert.ToDouble(value).ToString("C"); } }
            public string GP { get { return gP; } set { gP = Convert.ToDouble(value).ToString("C"); } }
            public string AverageGPPct { get { return averageGPPct; } set { averageGPPct = (Convert.ToDouble(value) / 100).ToString("P"); } }
            public string DefaultVendorGPPct { get { return defaultVendorGPPct; } set { defaultVendorGPPct = (Convert.ToDouble(value) / 100).ToString("P"); } }
            public string TotalQTY { get { return totalQTY; } set { totalQTY = Convert.ToDouble(value).ToString("N0"); } }
            public string DifferenceDollars { get { return differenceDollars; } set { differenceDollars = Convert.ToDouble(value).ToString("C"); } }
            public string DifferencePct { get { return differencePct; } set { differencePct = (Convert.ToDouble(value) / 100).ToString("P"); } }
        }

        public class SummaryModel
        {
            public double TotalSell { get; set; }
            public double TotalCost { get; set; }
            public double TotalGPDollars { get; set; }
            public double DifferenceGPDollars { get; set; }
        }

        public class SummaryComparisonModel
        {
            public SummaryModel SalespersonSummary { get; set; }
            public SummaryModel VendorSummary { get; set; }
        }
    }
}
