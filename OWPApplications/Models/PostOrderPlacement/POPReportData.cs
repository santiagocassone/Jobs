using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.PostOrderPlacement
{
    public class POPReportData
    {
        public List<POPPdfReport> Data { get; set; }
        public string Type { get; set; }
        public string Cols { get; set; }
        public bool ExcludeLines { get; set; }
        public string ProjectID { get; set; }
        public string CustomerName { get; set; }
        public string Location_Code { get; set; }
        public string SearchBy { get; set; }
    }
}
