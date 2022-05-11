using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.VendorDepositRequest
{
    public class VDRCost
    {
        public int Order_No { get; set; }
        public int Order_Index { get; set; }
        public int PO_Suffix { get; set; }
        public string Vnd_No { get; set; }
        public string Vendor_Name { get; set; }
        public float? Total_Cost { get; set; }
        public float? Total_Cost_Including_PA { get; set; }
        public float? Previously_Paid { get; set; }
        public float? Cost_Verified { get; set; }
    }
}
