using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.PostOrderPlacement
{
    public class POPPdfReport
    {
        public string AckDate { get; set; }
        public string AckNo { get; set; }
        public bool ACKNotReceived { get; set; }
        public string Carrier { get; set; }
        public string Comments { get; set; }
        public string EstimatedArrival { get; set; }
        public string EstimatedArrivalColor { get; set; }
        public string LastReceivedDate { get; set; }
        public List<LineInfoPostOrder> Lines { get; set; }
        public DateTime? OrderedDate { get; set; }
        public string OrderNo { get; set; }
        public bool PONotReceived { get; set; }
        public string PoSuffix { get; set; }
        public string RequestedArrival { get; set; }
        public DateTime? ShipDate { get; set; }
        public string Tracking { get; set; }
        public string VendorNo { get; set; }
        public string ScheduledArrivalDate { get; set; }

        public string POReference { get; set; }
        public string LineNo { get; set; }
        public string ProcessingCode { get; set; }
        public string Order_POSuffix { get; set; }
        public string VendorID { get; set; }
        public string VendorName { get; set; }
        public string CatalogNo { get; set; }
        public string Description { get; set; }
        public string QtyOrdered { get; set; }
        public string GeneralTagging { get; set; }
        public DateTime RequestedArrivalDate { get; set; }
        public DateTime EstimatedArrivalDate { get; set; }
        public string QtyReceived { get; set; }
        public DateTime LatestReceivedDate { get; set; }
        public string TrackingNo { get; set; }
        public string FreeformNotes { get; set; }
        public string Salesperson { get; set; }
        public string LatestSoftSchDt { get; set; }
    }
}
