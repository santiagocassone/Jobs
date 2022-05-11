using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.InternalStatusReport
{
    public class CRDData
    {
        public string ProjectID { get; set; }
        public DateTime? CRD { get; set; }
        public DateTime? ReqCRD { get; set; }
        public string Customer { get; set; }
    }
}
