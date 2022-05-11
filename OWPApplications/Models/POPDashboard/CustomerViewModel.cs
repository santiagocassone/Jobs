using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.POPDashboard
{
    public class CustomerViewModel
    {
        public string Quote_No { get; set; }
        public string Order_No { get; set; }
        public string Customer_PO { get; set; }
        public string Order_Title { get; set; }
        public string Estimated_Arr_Date { get; set; }
        public string Install_Delivery_Date { get; set; }
        public string Note1 { get; set; }
        public string Note2 { get; set; }
        public string Note3 { get; set; }
    }
}
