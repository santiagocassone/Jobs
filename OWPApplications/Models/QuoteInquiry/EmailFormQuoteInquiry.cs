using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.QuoteInquiry
{
    public class EmailFormQuoteInquiry 
    {
        public string Quote_OrderNo { get; set; }
        public string[] To { get; set; }
        public string From { get; set; }
        public string CC1 { get; set; }
        public string CC2 { get; set; }
        public string Notes { get; set; }
        public string QuoteHeader { get; set; }
        public string SpecifiedAndLocation { get; set; }
        public string ZipCode { get; set; }
        public string AnticipatedDate { get; set; }
        public bool RFP_BID { get; set; }
        public string VendorName { get; set; }
        public string CustomerName { get; set; }
        public IEnumerable<LineForEmail> Lines {get; set;}
        public bool ExcludeTitle { get; set; }
        public string YourName { get; set; }
        public string FormattedZipCode { get; set; }
        public bool ItemPricing { get; set; }
        public bool ItemFreight { get; set; }
        public bool ItemLeadTime { get; set; }
        public bool ItemComYardage { get; set; }
        public bool ItemComApproval { get; set; }
        public bool ItemConfirmComShip { get; set; }
        public bool ItemConfirmComAcrylic { get; set; }
        public bool ItemBudgetaryPricing { get; set; }
    }

    public class EmailFormQuoteInquiryOSQ
    {
        public string Quote_OrderNo { get; set; }
        public string[] To { get; set; }
        public string From { get; set; }
        public string CC1 { get; set; }
        public string CC2 { get; set; }
        public string Notes { get; set; }
        public string QuoteHeader { get; set; }
        public string ProjectNameAndLocation { get; set; }
        public string SpecifiedAndLocation { get; set; }
        public string ZipCode { get; set; }
        public string AnticipatedDate { get; set; }
        public string ReturnInfoBy { get; set; }
        public string Company { get; set; }
        public bool RFP_BID { get; set; }
        public string VendorName { get; set; }
        public string CustomerName { get; set; }
        public IEnumerable<LineForEmail> Lines { get; set; }
        public bool ExcludeCustomerName { get; set; }
        public bool ExcludeTitle { get; set; }
        public string YourName { get; set; }
        public string FormattedZipCode { get; set; }
        public bool InputListPrice { get; set; }
        public bool InputNetOrDiscountOffList { get; set; }
        public bool InputFreightEstimate { get; set; }
        public bool InputCOMApproval { get; set; }
        public bool InputQuoteAsCOM { get; set; }
        public bool InputQuoteAsGradedIn { get; set; }
        public bool InputCOMYardageRequirements { get; set; }
        public bool InputAnyAdditionalCharges { get; set; }
        public bool InputCurrentLeadTime { get; set; }
        public bool InputWarrantyInfo { get; set; }
        public bool InputUpcomingPriceChangesAnticipated { get; set; }
        public bool InputWhatIsMissingForACompleteSpec { get; set; }
        public bool InputFabricTestingRequired { get; set; }
        public bool InputShipToForCOM { get; set; }
        public bool InputConfirmComShip { get; set; }
        public bool InputEnvironmentalDataOrCertifications { get; set; }
        public bool InputCostsAndLeadTimeForAirFreight { get; set; }
        public bool InputDepositRequirements { get; set; }
        public bool InputPaymentTerms { get; set; }
        public string Title { get; set; }
        public string PhoneNo { get; set; }
        public string VendorNo { get; set; }
        public string Is61 { get; set; }
        public string ProjectID { get; set; }
    }


    public class LineForEmail
    {
        public string LineNo { get; set; }
        public string Catalog { get; set; }
        public string Description { get; set; }
        public string GeneralTagging { get; set; }

        public string QtyOrdered { get; set; }
    }
}
