using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models
{
    public class OrderValues
    {
        public string OrderNo { get; set; }
        public string LineNo { get; set; }
        public string FieldID { get; set; }
        public string Value { get; set; }
        public string PO { get; set; }
        public string Source { get; set; }
        public string Company { get; set; }
        public string KeyString { get; set; }
        public string OrderType { get; set; }
        public string Region { get; set; }
        public string LineType { get; set; }
        public string ProjectID { get; set; }
        public string UpsertLevel { get; set; }
    }
}
