using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.VendorDepositRequest
{
    public class VDREmailForm
    {
        public string To { get; set; }
        public string From { get; set; }
        public string CC1 { get; set; }
        public string CC2 { get; set; }
        public string YourName { get; set; }
        public string DueDate { get; set; }
        public string Order { get; set; }
        public string Vendor { get; set; }
        public string DepositTerms { get; set; }
        public string AmtDue { get; set; }
        public string Notes { get; set; }
        public string CustomDepositTerms { get; set; }
        public string POTotal { get; set; }
        public string PreviouslyPaid { get; set; }
        public string CostVerified { get; set; }
        public string PaymentType { get; set; }
        public string CurrencyType { get; set; }
        public string CurrencyCustom { get; set; }

    }
}
