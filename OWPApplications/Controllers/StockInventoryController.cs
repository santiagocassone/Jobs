using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OWPApplications.Data;
using OWPApplications.Models.StockInventory;

namespace OWPApplications.Controllers
{
    public class StockInventoryController : Controller
    {
        private readonly DbHandler _db;

        public StockInventoryController(DbHandler dbHandler)
        {
            _db = dbHandler;
        }

        public IActionResult Index(string itemNo, string vendor, string productId, string catalogNo, List<int> location, string region)
        {
            var vendors = _db.StockInventoryHandler.LoadVendors();
            var locations = _db.StockInventoryHandler.LoadLocations(region ?? "OWP");

            ViewData["Vendors"] = vendors;
            ViewData["ItemNo"] = itemNo;
            ViewData["Vendor"] = vendor;
            ViewData["ProductId"] = productId;
            ViewData["CatalogNo"] = catalogNo;
            ViewData["Locations"] = locations;
            ViewData["Location"] = location;
            ViewData["Region"] = region ?? "OWP"; 

            var vm = new StockInventoryViewModel
            {
                //
            };

            if (!string.IsNullOrEmpty(itemNo) || !string.IsNullOrEmpty(vendor) || !string.IsNullOrEmpty(productId) || !string.IsNullOrEmpty(catalogNo))
            {
                vm.ShowResults = true;
                vm.Inventory = _db.StockInventoryHandler.GetStockInventory(itemNo, vendor, productId, catalogNo, region, location).ToList();
            }

            vm.QuoteInfos = new List<QuoteInfoDetails>();
            if (vm.Inventory != null && vm.Inventory.Count() > 0)
            {
                foreach (var item in vm.Inventory)
                {
                    if (item.QuoteInfos != null && item.QuoteInfos.Count() > 0)
                    {
                        foreach (var itemQ in item.QuoteInfos)
                        {
                            vm.QuoteInfos.Add(new QuoteInfoDetails { Item_No = item.Item_No, Order_No = itemQ.Order_No, Qty_Ordered = itemQ.Qty_Ordered });
                        }
                    }
                    
                }
            }
            

            return View(vm);
        }

        [HttpPost]
        public ActionResult LoadLocations(string region)
        {
            var result = _db.StockInventoryHandler.LoadLocations(region);
            return Json(result);
        }
    }
}