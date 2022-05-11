using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.JobCosting
{
    public class JobCostingViewModel
    {
        public List<SelectValues> Customers { get; internal set; }
        public List<Report> Reports { get; set; }
        public List<ProjectDetail> ProjectDetails { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
    }

    public class Report
    {
        public string Customer { get; set; }
        public string Lead { get; set; }
        public int Project { get; set; }
        public string MinSchDate { get; set; }
        public string MaxSchDate { get; set; }
        public float QuoteBudget { get; set; }
        public float AdditionalCost { get; set; }
        public float VehicleQuoteBreakout { get; set; }
        public float HSQuoteBreakout { get; set; }
        public float ChangeOrder { get; set; }
        public float BudgetTotal { get; set; }
        public float RegHrsRate { get; set; }
        public float OTHrsRate { get; set; }
        public float DTHrsRate { get; set; }
        public float PWRegHrsRate { get; set; }
        public float PWOTHrsRate { get; set; }
        public float PWDTHrsRate { get; set; }
        public float RegHrs { get; set; }
        public float OTHrs { get; set; }
        public float DTHrs { get; set; }
        public float PWRegHrs { get; set; }
        public float PWOTHrs { get; set; }
        public float PWDTHrs { get; set; }
        public float LaborCost { get; set; }
        public float VehicleCost { get; set; }
        public float AdditionalExpenses { get; set; }
        public float TotalCost { get; set; }
        public float GPDollar { get; set; }
        public float GPPct { get; set; }
        public string Order_Nos { get; set; }
        public string Order_Idxs { get; set; }
        public int FullyDeliveredProject { get; set; }
        public int FullyInvoicedProject { get; set; }
        public string AdditionalOrders { get; set; }
        public string AdditionalLines { get; set; }
        public string Delivered { get; set; }
        public string Invoiced { get; set; }
        public string Notes { get; set; }
        public string BillingTeamInitials { get; set; }
        public string BillingStatus { get; set; }
        public string LaborQuoteNumbers { get; set; }
    }

    public class ProjectDetail
    {
        public string SchDate { get; set; }
        public string FSchDate { get; set; }
        public int Order_No { get; set; }
        public string Lines { get; set; }
        public float TotalCost { get; set; }
        public float MxTActualHours { get; set; }
        public float MxTCost { get; set; }
    }

    public class OrderDetail
    {
        public string SchDate { get; set; }
        public string FSchDate { get; set; }
        public int Order_No { get; set; }
        public string Lines { get; set; }
        public float TotalCost { get; set; }
        public string LDelivDate { get; set; }
        public string LInvDate { get; set; }
        public float MxTActualHours { get; set; }
        public float MxTCost { get; set; }


    }
}
