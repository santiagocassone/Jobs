using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.SubcontractorReceiving
{
    public class SubcontractorReceivingViewModel
    {
        public IEnumerable<SummaryInfo> Summary { get; set; }
        public IEnumerable<LineInfo> Lines { get; set; }
        public IEnumerable<SelectValues> VendorList { get; set; }
        public string VendorNo { get; set; }
        public string POReference { get; set; }
        public bool ShowSummary { get; set; }
        public bool ShowDetails { get; set; }
        public bool ShowOnlyEmailForm { get; set; }
        public IEnumerable<LookupGeneral> EmailTypeList { get; set; }
		public IEnumerable<LookupGeneral> IssueTypeList { get; set; }
		public IEnumerable<LookupGeneral> IssueDetailList { get; set; }
		public string Success { get; set; }
		public string POSuffix { get; set; }
		public string VendorName { get; set; }
		public List<SummaryInfo> POsReceivedComplete { get; set; }
		public string DateFrom { get; set; }
		public string DateTo { get; set; }
        public int EmailType { get; set; }
        public string EmailTypeText { get; set; }
        public int? OrderNo { get; set; }
    }

    public class SummaryInfo
    {
        public string POSuffix { get; set; }
        public string POReference { get; set; }
        public string Salesperson { get; set; }
        public string InterimVndNo { get; set; }
        public string VendorName { get; set; }
        public string DateEntered { get; set; }
        public string AckNo { get; set; }
        public string ShipDate { get; set; }
        public string ExpectedReceiptDate { get; set; }
        public string ReceivedDate { get; set; }
        public string ScheduledReceiptDate { get; set; }
        public string Carrier { get; set; }
        public string TrackingNo { get; set; }
        public string TotallyReceived { get; set; }
		public string CatalogVendor { get; set; }
        public string CatalogVendorNo { get; set; }
        public string OrderLastRequest { get; set; }
        public string OrderNo { get; set; }
        public int TotalQtyOrdered { get; set; }
        public string OrderTitle { get; set; }
    }

    public class LineInfo
    {
        public string POSuffix { get; set; }
        public string POReference { get; set; }
        public string CatNo { get; set; }
        public string Description { get; set; }
        public string LineNo { get; set; }
        public string LineIndex { get; set; }
        public string Salesperson { get; set; }
        public string InterimVndNo { get; set; }
        public string VendorName { get; set; }
        public string DateEntered { get; set; }
        public string AckNo { get; set; }
        public string ShipDate { get; set; }
        public string ExpectedReceiptDate { get; set; }
        public string ReceivedDate { get; set; }
        public string ScheduledReceiptDate { get; set; }
        public string Carrier { get; set; }
        public string TrackingNo { get; set; }
        public int QtyOrdered { get; set; }
        public string QtyReceived { get; set; }
        public decimal Received { get; set; }
        public string ProcessingCode { get; set; }
        public string ReceiveGoods { get; set; }
        public string CustomerName { get; set; }
		public string ProductVendor { get; set; }
		public string LineLastRequest { get; set; }
		public int LineQtyReceived { get; set; }
		public bool LQRWasModified { get; set; }
		public string OrderIndex { get; set; }
		public string CompanyCode { get; set; }
		public string OrderNo { get; set; }
        public string CatalogVendorNo { get; set; }
    }

    public class SCREmailData
    {
        public string From { get; set; }
        public string To { get; set; }
        public string CC1 { get; set; }
        public string CC2 { get; set; }
        public string FromName { get; set; }
        public string Notes { get; set; }
        public string EmailType { get; set; }
        public string SubcontractorName { get; set; }
        public string POReference { get; set; }
		public string POSuffix { get; set; }
		public string ProductVendor { get; set; }
        public string Date { get; set; }
        public string PurchaseOrderNo { get; set; }
        public IEnumerable<SCREmailLineData> LinesData { get; set; }
		public string LineNo { get; set; }
		public string IssueType { get; set; }
		public string IssueDetail { get; set; }
		public string Quantity { get; set; }
		public string Description { get; set; }
        public string VendorName { get; set; }
        public string PORef { get; set; }
    }

    public class SCREmailLineData
    {
        public string LineNo { get; set; }
        public string Qty { get; set; }
		public string VendorName { get; set; }
		public string POSuffix { get; set; }
		public string ProductVendor { get; set; }
		public string LineQtyReceived { get; set; }
		public bool LQRWasModified { get; set; }
		public string OrderIndex { get; set; }
		public string OrderNo { get; set; }
	}

	public class UpdateQtyReceivedModel
	{
		public char CompanyCode { get; set; }
		public int POSuffix { get; set; }
		public int OrderIndex { get; set; }
		public int LineIndex { get; set; }
		public int NewQLRValue { get; set; }
	}

    enum EmailType
    {
        ReceiptOfProduct = 155,
        RequestShipDate = 156,
        RequestTrackingInformation = 157,
        ReportIssue = 158
    }
}