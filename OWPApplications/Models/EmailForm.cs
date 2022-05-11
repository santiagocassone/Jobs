using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models
{
    public class EmailForm
    {
        public string Quote_OrderNo { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string CC1 { get; set; }
        public string CC2 { get; set; }
        public string HasToInvoice { get; set; }
        public string InvoiceType { get; set; }
		public string CheckedLines { get; set; }
        public string Name { get; set; }
    }

}
