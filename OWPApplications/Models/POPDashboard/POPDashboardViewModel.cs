using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.POPDashboard
{
    public class POPDashboardViewModel
    {
        public IEnumerable<BudgetActualModel> Budget { get; internal set; }
        public IEnumerable<PastCRDModel> PastCRDs { get; internal set; }
        public IEnumerable<FutureCRDModel> FutureCRDs { get; internal set; }
        public List<SelectValues> Customers { get; internal set; }
        public List<SelectValues> SalesPersons { get; internal set; }
        public POPDashboardSummary Summary { get; internal set; }
        public string Salesperson { get; internal set; }
        public string[] SelectedCustomers { get; internal set; }
        public string CustomerNo { get; internal set; }
        public string ProjectId { get; internal set; }
        public IEnumerable<OpenQuotes> OpenQuotes { get; internal set; }
        public IEnumerable<CustomerViewModel> CustomerView { get; internal set; }
    }

    public class CustomerViewPdfReport
    {
        public List<CustomerViewModel> Data { get; set; }
        public string[] Customers { get; set; }
    }
}
