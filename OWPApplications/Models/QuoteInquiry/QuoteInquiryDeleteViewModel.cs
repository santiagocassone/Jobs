using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.QuoteInquiry
{
    public class QuoteInquiryDeleteViewModel
    {
        public string QuoteNoToDelete { get; set; } 
        public string OrderCustomerName { get; set; }
        public string OrderSalespersonID { get; set; }
        public string DeletionRequestorEmail { get; set; }
        public string DeletionRequestorName { get; set; }
        public string OtherNotes { get; set; }
        public IFormFile BOMSIFFile { get; set; }
    } 
}
