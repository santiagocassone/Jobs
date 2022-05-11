using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.JobCosting
{
    public class CustomValue
    {
        public string Company_Code { get; set; }
        public int? Order_No { get; set; }
        public string Order_Type { get; set; }
        public int PO { get; set; }
        public int? Line_No { get; set; }
        public string Line_Type { get; set; }
        public string KeyString { get; set; }
        public string AppName { get; set; }
    }

    public class ViewCustomValues
    {
        public string Value { get; set; }
        public string FieldID { get; set; }
        public string Project_ID { get; set; }

    }
}
