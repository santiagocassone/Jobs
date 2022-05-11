using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.JobCosting
{
    public class JobCostingFilterData
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string ProjectId { get; set; }
        public string OrderNo { get; set; }
        public string[] CustomerId { get; set; }
        public string WarehouseId { get; set; }
        public string LeadId { get; set; }
    }
}
