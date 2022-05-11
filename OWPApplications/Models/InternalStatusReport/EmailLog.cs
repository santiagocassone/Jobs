using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.InternalStatusReport
{
    public class EmailLog
    {
        public DateTime? CreatedDate { get; set; }
        public string YourEmail { get; set; }
        public string CreatedBy { get; set; }
        public string Vendor { get; set; }
        public string Line { get; set; }

    }
}
