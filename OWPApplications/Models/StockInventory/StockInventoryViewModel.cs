using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.StockInventory
{
    public class StockInventoryViewModel
    {
        public List<Inventory> Inventory { get; set; }
        public List<SelectValues> Vendors { get; set; }
        public List<QuoteInfoDetails> QuoteInfos { get; set; }
        public string ItemNo { get; set; }
        public string Vendor { get; set; }
        public string ProjectId { get; set; }
        public string CatalogNo { get; set; }
        public bool ShowResults { get; set; }
        public string QuoteInfoJson()
        {
            return JsonConvert.SerializeObject(this.QuoteInfos);
        }
    }

    public class Inventory
    {
        public string Item_No { get; set; }
        public string Vnd_no { get; set; }
        public string Vendor_Name { get; set; }
        public string Product_ID { get; set; }
        public string Catalog_No { get; set; }
        public string Item_Desc { get; set; }
        public double On_Hand { get; set; }
        public double Qty_commited { get; set; }
        public double Qty_On_Order { get; set; }
        public double Qty_On_Quote { get; set; }
        public double Item_Cost { get; set; }
        public string R_Customer_No { get; set; }
        public List<QuoteInfo> QuoteInfos { get; set; }
    }

    public class QuoteInfo
    {
        public string Title { get; set; }
        public string Order_No { get; set; }
        public int Qty_Ordered { get; set; }
    }

    public class QuoteInfoDetails
    {
        public string Item_No { get; set; }
        public string Order_No { get; set; }
        public int Qty_Ordered { get; set; }
    }
}