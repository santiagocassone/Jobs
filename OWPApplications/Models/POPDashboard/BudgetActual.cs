using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.POPDashboard
{
    /*
    public class BudgetActual
    {
        public IEnumerable<BudgetActualLines> Detail { get; set; }
    }

    public class BudgetActualLines
    {
        public string ProjectID { get; set; }
        public string DesignBudget { get; set; }
        public double DesignCost { get; set; }
        public double DesignSell { get; set; }
        public string PMBudget { get; set; }
        public double PMCost { get; set; }
        public double PMSell { get; set; }
    }
    */

    public class BudgetActualModel
    {
        public string Project_ID { get; set; }
        public string DesignBudget { get; set; }
        public double? DesignCostHs { get; set; }
        public double? DesignSellHs { get; set; }
        public string PMBudget { get; set; }
        public double? PMCostHs { get; set; }
        public double? PMSellHs { get; set; }
        public double? DesignCost { get; set; }
        public double? DesignSell { get; set; }
        public double? PMCost { get; set; }
        public double? PMSell { get; set; }
        public string Project_Title { get; set; }
        public string CustomerName { get; set; }

    }
}
