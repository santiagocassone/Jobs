using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.InternalStatusReport
{
    public class InternalStatusReportViewModel
    {
        public bool ShowSummary { get; set; }
        public bool ShowDetails { get; set; }
        public bool ShowSalespersonDashboard { get; set; }
        public int ActiveIndex { get; set; }
        public string SalesPerson { get; set; }
        public string CutOffDate { get; set; }
        public string TotalOrdersOpen { get; set; }
        public string TotalOpenSell { get; set; }
        public string TotalOpenCost { get; set; }
        public string TotalSellEligibleforPartialInvoicing { get; set; }
        public string SellEligibleforPartialInvoicing { get; set; }
        public List<SummaryInfoInternalStatus> SummaryInfo { get; set; }
        public List<VendorMiscCharge> VendorMiscCharges { get; set; }
        public List<SelectValues> Customers { get; internal set; }
        public List<SelectValues> SalesPersons { get; internal set; }
        public IEnumerable<string> SelectedCustomers { get; internal set; }
        public string SelectedCustomersUri { get; internal set; }
        public List<SelectValues> SalesDirectors { get; internal set; }
        public List<SelectValues> SalesSupportManagers { get; internal set; }
        public string SalesDirector { get; set; }
        public string SalesSupportManager { get; set; }
        public List<Salesperson> SalespersonInfo { get; set; }
        public bool ComesFromMV { get; set; }
        public string SalespersonType { get; set; }
        public string SalespersonCode { get; set; }
        public string View { get; set; }
        public string OrderNo { get; set; }
        public List<string> ResultSalespersons { get; set; }
		public bool ShowViewClosedOrdersLink { get; set; }
		public string SummaryInfoJson(string orderNo)
		{
			return JsonConvert.SerializeObject(this.SummaryInfo.Where(x => x.OrderNo.Trim() == orderNo.Trim()).Select(x => x).FirstOrDefault());
		}
        public string SummaryInfoJson()
        {
            return JsonConvert.SerializeObject(this.SummaryInfo);
        }
        public List<Vendor> Vendors { get; set; }
    }

    public class SummaryInfoInternalStatus
    {
        public string SalespersonName { get; set; }
        public string SalespersonID { get; set; }
        public string ProjectID { get; set; }
        public string OrderNo { get; set; }
        public string OrderDate { get; set; }
        public string CustomerName { get; set; }
        public string OrderTitle { get; set; }
        public string EstimatedArrivalDate { get; set; }
        public string CustomerRequestDate { get; set; }
        public double TotalOpenSell { get; set; } = 0;
        public double TotalOpenCost { get; set; } = 0;
        public int QtyOpen { get; set; }
        public string Comment { get; set; }
        public double SellEligibleforPartialInvoicing { get; set; } = 0;
        public List<LineInfoInternalStatus> Lines { get; set; }
        public HeaderInfoInternalStatus HeaderInfo { get; set; }
        public List<ProcessCode> ProcessCodes { get; set; }
        public string ProcessCodeName { get; set; }
        public double PercentageSellAvailablePartialInvoicing { get; set; }
        public double PercentageSellPartialInvoicingByProcessCode { get; set; }
        public List<EmailLog> EmailLogs { get; set; }
        public List<EstArrDate> EstArrDates { get; set; }
        public string RequestedCRD { get; set; }
		public List<ScheduledDate> ScheduledDates { get; set; }
        public string Accountability { get; set; }
        public string CompanyCode { get; set; }
        public string OWP_PO { get; set; }
        public List<Vendor> Vendors { get; set; }

        public override bool Equals(object obj)
		{
			SummaryInfoInternalStatus other = obj as SummaryInfoInternalStatus;
			return this.OrderNo.Equals(other?.OrderNo);
		}
		public override int GetHashCode()
        {
            return HashCode.Combine(OrderNo);
        }

        public string Tooltip(List<EmailLog> EmailLogs)
        {
            string TipText = @"<div class='rTable'><div class='rTableHeading'><div class='rTableHead'>Date/Time</div><div class='rTableHead'>Your Email</div><div class='rTableHead'>Email Type</div><div class='rTableHead'>Vendor</div><div class='rTableHead'>Line</div></div>";
            foreach (var item in EmailLogs)
            {
                TipText += @"<div class='rTableRow'><div class='rTableCell'>" + item.CreatedDate.ToString() + "</div><div class='rTableCell'>" + item.YourEmail + "</div><div class='rTableCell'>" + item.CreatedBy + "</div><div class='rTableCell'>" + item.Vendor + "</div><div class='rTableCell'>" + item.Line + "</div></div>";
            }

            TipText += @"</div>";

            return TipText;

        }

        public string DateTooltip(List<EstArrDate> EstArrDates)
        {
            string TipText = @"<div class='rTable'><div class='rTableHeading'><div class='rTableHead'>Estimated Arrival Dates</div></div>";
            foreach (var item in EstArrDates)
            {
                TipText += @"<div class='rTableRow'><div class='rTableCell'>" + item.scheduled_arrival_date.ToString("MM/dd/yyyy") + "</div></div>";
            }

            TipText += @"</div>";

            return TipText;

        }
    }

    public class HeaderInfoInternalStatus
    {
        public string ProjectId { get; set; }
        public string OrderTitle { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNo { get; set; }
        public string SalesIDs { get; set; }
        public string CustomerPONo { get; set; }
        public string ShipToAddress { get; set; }
        public string OrderStatus { get; set; }
        public string DeliveryInstructions { get; set; }
        public string MFG_PO_Info { get; set; }
        public string SalesPersonID { get; set; }



        public Dictionary<int, string> Locations { get; internal set; }

        public HeaderInfoInternalStatus(DataRow row, DataRow instr, DataTable MFG, DataTable LocationTable)
        {
            ProjectId = clsLibrary.dBReadString(row["Project_Id"]);
            OrderTitle = clsLibrary.dBReadString(row["Title"]);
            CustomerNo = clsLibrary.dBReadString(row["Customer_No"]);
            CustomerName = clsLibrary.dBReadString(row["Customer_Name"]);
            SalesIDs = $"{clsLibrary.dBReadString(row["Salesperson_Id_1"])} ({clsLibrary.dBReadString(row["Salesperson_1_Pct"])}%)";
            SalesIDs += (clsLibrary.dBReadString(row["Salesperson_Id_2"]) == "") ? "" : $",{clsLibrary.dBReadString(row["Salesperson_Id_2"])} ({100 - clsLibrary.dBReadInt(row["Salesperson_1_Pct"])}%)";
            CustomerPONo = clsLibrary.dBReadString(row["Customer_PO_Number"]);
            SalesPersonID = clsLibrary.dBReadString(row["Salesperson_Id_1"]);

            ShipToAddress = clsLibrary.dBReadString(row["Address_Line_1"]);
            if (clsLibrary.dBReadString(row["Address_Line_2"]) != "")
            {
                ShipToAddress += "<br>" + clsLibrary.dBReadString(row["Address_Line_2"]);
            }
            ShipToAddress += "<br>" + clsLibrary.dBReadString(row["City"]);

            OrderStatus = clsLibrary.dBReadString(row["Order_Status"]);

            if (instr != null)
            {
                DeliveryInstructions = $"<pre>{clsLibrary.dBReadString(instr["Instructions"])}</pre>";
            }

            string template = @"<b>##name##</b><br/>
                    Dealer Contact: <i class='##clsDealer##'>##Dealer##</i><br/>
                    Vendor Contact: <i>##Vendor##</i><br/>
                    Carrier Contact: <i>##Carrier##</i><br/><br/>";
            MFG_PO_Info = "";
            foreach (DataRow r in MFG.Rows)
            {
                string copy = template;
                string vnd_no = clsLibrary.dBReadString(r["Vnd_No"]).Trim().ToLower();
                if (vnd_no == "ste01") copy = copy.Replace("##name##", "Steelcase (STE01)");
                if (vnd_no == "bra00") copy = copy.Replace("##name##", "Coalesse (BRA00)");
                string carrierContactPhone = "";
                string formatted = clsLibrary.dBReadString(r["Carr_Cont_Formatted_Phone_No"]).ToString();
                if (formatted != "") carrierContactPhone = formatted;
                string extension = clsLibrary.dBReadString(r["Carr_Cont_Extension"]);
                if (extension != "") carrierContactPhone += " x" + extension;
                string dealer = clsLibrary.dBReadString(r["Dlr_First_Name"]).Trim() + " " + clsLibrary.dBReadString(r["Dlr_Last_Name"]).Trim();
                string clsDealer = (dealer == "One Workplace") ? "text-red" : "";
                string vendor = clsLibrary.dBReadString(r["STC_First_Name"]).Trim() + " " + clsLibrary.dBReadString(r["STC_Last_Name"]).Trim();
                string carrier = clsLibrary.dBReadString(r["Carr_First_Name"]).Trim() + " " + clsLibrary.dBReadString(r["Carr_Last_Name"]).Trim()
                        + " | " + carrierContactPhone + clsLibrary.dBReadString(r["Carrier_Email"]).Trim();
                copy = copy.Replace("##clsDealer##", clsDealer);
                copy = copy.Replace("##Dealer##", dealer);
                copy = copy.Replace("##Vendor##", vendor);
                copy = copy.Replace("##Carrier##", carrier);

                MFG_PO_Info += copy;
            }

            Locations = new Dictionary<int, string>();
            foreach (DataRow r in LocationTable.Rows)
            {
                Locations.Add(clsLibrary.dBReadInt(r["Line_No"]),
                    clsLibrary.dBReadString(r["Whs_Locations"]));
            }


        }
    }

    public class LineInfoInternalStatus
    {
        public string OrderNo { get; set; }
        public int LineNo { get; set; }
        public string VendorNo { get; set; }
        public string[] VendorEmail { get; set; }
        public string Ordered { get; set; }
        public string Received { get; set; }
        public string ReceivedColor { get; set; }
        public string Ticketed { get; set; }
        public string Delivered { get; set; }
        public string Invoiced { get; set; }
        public string VendorInv { get; set; }
        public string OpenSell { get; set; }
        public double OpenSellWithoutFormat { get; set; }
        public string OpenCost { get; set; }
        public string TicketedColor { get; set; }
        public string DeliveredColor { get; set; }
        public string InvoicedColor { get; set; }
        public string OpenSellColor { get; set; }
        public string OpenCostColor { get; set; }
        public string VendorInvColor { get; set; }
        public string ACK { get; set; }
        public string OWP_PO { get; set; }
        public string ScheduleDate { get; set; }
        public string ProcessingCode { get; set; }
        public string SalesCode { get; set; }
        public string EstimatedArrivalDate { get; set; }
        public string CustomerRequestDate { get; set; }
        public string Catalog { get; set; }
        public string Description { get; set; }
        public string SellEligibleforPartialInvoicing { get; set; }
        public int CostVerifiedReadyFlag { get; set; }
        public double TaxAmount { get; set; }
        public bool ShowLink { get; set; }
        public double TaxPercentage { get; set; }
		public string ScheduledDateColor { get; set; }
        public string WarehouseNo { get; set; }
        public string HasHardSched { get; set; }
        public string Notes { get; set; }
        public string Vendor_Name { get; set; }
        public string Cost_Verified_Date { get; set; }

        public string CostDateTooltip(string costVerDate)
        {
            return @"<div class='rTable'>
                        <div class='rTableHeading'>
                            <div class='rTableHead'>Cost Verification Date</div>
                        </div>
                     <div class='rTableRow'>
                        <div class='rTableCell'>" + costVerDate + "</div>" +
                       "</div></div>";

        }

    }

    public class VendorMiscCharge
    {
        public string VendorNo { get; set; }
        public double BOPASell { get; set; }
        public double BOPACost { get; set; }
        public double MiscSell { get; set; }
        public double MiscCost { get; set; }
        public double GrossProfit { get; set; }
    }

    //public class MiscCharge
    //{
    //    public int MiscChargeNo { get; set; }
    //    public string VendorNo { get; set; }
    //    public string SalesCode { get; set; }
    //    public string ChargeCode { get; set; }
    //    public string Sell { get; set; }
    //    public string Cost { get; set; }
    //    public string RecCostPt { get; set; }
    //    public string TaxCode { get; set; }
    //}

    public class WhseLocation
    {
        public int LineNo;
        public string Location;
    }

    public class EmailType
    {
        public string Label { get; set; }

    }

    public class ProcessCode
    {
        public string ProcessCodeGroup { get; set; }
        public double PctEligibleForPartialInvoice { get; set; }
    }

    public class Salesperson
    {
        public string SalespersonID { get; set; }
        public string SalespersonName { get; set; }
        public double PartialInvoicingSellEligible { get; set; }
        public double TotalSell { get; set; }
        public double TotalCost { get; set; }
        public double GPDollars { get; set; }
        public double GPPct { get; set; }
    }

	public class ScheduledDate
	{
		public string Date { get; set; }
		public string Color { get; set; }
	}

    public class Vendor
    {
        public string VendorNo { get; set; }
        public string[] VendorEmail { get; set; }
        public string Order { get; set; }
        public string Suffix { get; set; }
    }
}
