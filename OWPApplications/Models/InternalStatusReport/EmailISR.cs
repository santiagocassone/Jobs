using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.InternalStatusReport
{
    public class EmailISR
    {
        public string Email_Type { get; set; }
        public string Email { get; set; }
        public string Email_CCW { get; set; }
        public string Email_BCC { get; set; }
        public string Vendor { get; set; }
        public string Process_Code { get; set; }
        public string Line_Sales_Cd { get; set; }
        public string Email_Type_Value { get; set; }
    }
}
