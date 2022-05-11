using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.JobCostingLeadership
{
    public class JobCostingLeadershipViewModel
    {
        public List<SelectValues> Customers { get; internal set; }
        public List<Summary> Summaries { get; set; }
        public List<Detail> Details { get; set; }
        public string Customer { get; set; }
        public string ProjectId { get; set; }
    }

    public class Summary
    {
        public string Company_Code { get; set; }
        public string Customer { get; set; }
        public int Project_ID { get; set; }
        public string MinSchDate { get; set; }
        public string MaxSchDate { get; set; }
        public string MaxSchDateColor { get; set; }
        public string FullLaborQuoteCode { get; set; }
        public string FullLaborQuoteCode_NotScheduled { get; set; }
        public float LaborQuoteCost { get; set; }
        public float ActualCost { get; set; }
        public float BudgetCost { get; set; }
        public float GPCost { get; set; }
        public float GPPct { get; set; }
        public string GPPctColor { get; set; }
        public string ProjectOpenLines { get; set; }
        public float LQ_GPCost { get; set; }
        public float LQ_GPPct { get; set; }
    }

    public class Detail
    {
        public string Company_Code { get; set; }
        public int Order_No { get; set; }
        public string Lines { get; set; }
        public string SchDate { get; set; }
        public string Lead { get; set; }
        public bool Delivered { get; set; }
        public bool Invoiced { get; set; }
        public float LaborLineTotal { get; set; }
        public float ActualCost { get; set; }
        public float BudgetCost { get; set; }
    }
}
