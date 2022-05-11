using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.ISRManagerView
{
    public class ISRManagerViewViewModel
    {
        public List<SelectValues> SalesDirectors { get; set; }
        public List<SelectValues> SalesSupportManagers { get; set; }
        public string SalesDirector { get; set; }
        public string SalesSupportManager { get; set; }
        public string CutOffDate { get; set; }
        public List<Salesperson> SalespersonInfo { get; set; }
        public string SalespersonLink { get; set; }
		public bool ShowDisclaimer { get; set; }
	}

    public class Salesperson
    {
        public string SalespersonID { get; set; }
        public string SalespersonName { get; set; }
        public double PartialInvoicingSellEligible { get; set; }
        public double TotalSell { get; set; }
        public double TotalCost { get; set; }
        public double GPDollars { get; set; }
        public double GPPct { get; set; }
		public bool IsSplitted { get; set; }
        public double OvdQty { get; set; }
        public double OvdPct { get; set; }
    }
}
