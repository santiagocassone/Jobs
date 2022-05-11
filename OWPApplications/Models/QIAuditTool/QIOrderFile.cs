using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.QIAuditTool
{
    public class QIOrderFile
    {
        public int Quantity { get; set; }
        public int LineItemNumber { get; set; }
        public string LineItemIdentifier { get; set; }
        public decimal PublishedPrice { get; set; }
        public decimal PublishedPriceExt { get; set; }
        public decimal EndCustomerPrice { get; set; }
        public decimal EndCustomerPriceExt { get; set; }
        public decimal OrderDealerPrice { get; set; }
        public decimal OrderDealerPriceExt { get; set; }
        public string SpecItemNumber { get; set; }
        public string SpecItemDescription { get; set; }
        public string SpecItemCatalogCode { get; set; }
    }
}
