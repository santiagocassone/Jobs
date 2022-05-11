using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models
{
    public class CommentData
    {
        public string OrderNo { get; set; }
        public string LineNo { get; set; }
        public string Comment { get; set; }
        public int PO { get; set; }
        public string Source { get; set; }
        public string Company { get; set; }
        public string KeyString { get; set; }
    }
}
