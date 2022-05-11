using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.PostOrderPlacement
{
    public class PostOrderPlacementViewModel
    {
        public bool ShowResults { get; set; }

        public string OrderNo { get; set; }

        public HeaderInfoPostOrder HeaderInfo { get; set; }

        public IEnumerable<string> PO_Suffixs { get; set; }
        
        public IEnumerable<SummaryPostOrder> SummaryInfo { get; set; }

        public IEnumerable<LineDetailPostOrder> LineDetail { get; set; }

        public string SummaryInfoJson()
        {
            return JsonConvert.SerializeObject(this.SummaryInfo);
        }
        public string LineDetailJson()
        {
            return JsonConvert.SerializeObject(this.LineDetail);
        }
        public IEnumerable<EmailTypePOP> EmailTypes { get; set; }
        public IEnumerable<EmailField> EmailFields { get; set; }
        public string HeaderHtml { get; set; }
        public string SummaryHeaderHtml { get; set; }
    }

    public class HeaderInfoPostOrder
    {
        public string ProjectId { get; set; }
        public string OrderTitle { get; set; }
        public string CustomerNo { get; set; }
        public string SalesIDs { get; set; }
        public string CustomerPONo { get; set; }
        public string ShipToAddress { get; set; }
        public string OrderStatus { get; set; }
        public string DeliveryInstructions { get; set; }
        public string MFG_PO_Info { get; set; }
        public string SalesPersonID { get; set; }
        public string FileName { get; set; }

        public string CustomerName { get; set; }
        public string Location_Code { get; set; }
        public string Location_Name { get; set; }

        public HeaderInfoPostOrder(DataRow row, DataRow instr, DataTable MFG)
        {
            CustomerName = clsLibrary.dBReadString(row["Customer_Name"]);
            FileName = clsLibrary.dBReadString(row["File_Name"]);
            ProjectId = clsLibrary.dBReadString(row["Project_Id"]);
            OrderTitle = clsLibrary.dBReadString(row["Title"]);
            CustomerNo = clsLibrary.dBReadString(row["Customer_No"]);
            SalesIDs = $"{clsLibrary.dBReadString(row["Salesperson_Id_1"])} ({clsLibrary.dBReadString(row["Salesperson_1_Pct"])}%)";
            SalesIDs += (clsLibrary.dBReadString(row["Salesperson_Id_2"]) == "") ? "" : $",{clsLibrary.dBReadString(row["Salesperson_Id_2"])} ({100 - clsLibrary.dBReadInt(row["Salesperson_1_Pct"])}%)";
            CustomerPONo = clsLibrary.dBReadString(row["Customer_PO_Number"]);
            SalesPersonID = clsLibrary.dBReadString(row["Salesperson_Id_1"]);
            Location_Code = clsLibrary.dBReadString(row["Location_Code"]).Trim();

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


        }
    }

    public class SummaryPostOrder
    {
        public string OrderNo { get; set; }
        public string VendorNo { get; set; }
        public string PoSuffix { get; set; }        
        public string OrderedDate { get; set; }
        public string AckNo { get; set; }
        public string AckDate { get; set; }
        public string ShipDate { get; set; }
        public string EstimatedArrival { get; set; }
        public string EstimatedArrivalColor { get; set; }
        public string RequestedArrival { get; set; }
        public string LastReceivedDate { get; set; }
        public string ScheduledArrivalDate { get; set; }
        public bool PONotReceived { get; set; }
        public bool ACKNotReceived { get; set; }
        public string Carrier { get; set; }
        public string Tracking { get; set; }
        public string TrackingLink { get; set; }
        public string Comments { get; set; }
        public bool IsSteelcaseVendor { get; set; }
        public bool IsAvailableVendor { get; set; }
        public string VendorName { get; set; }
        public string LatestSoftSchDt { get; set; }

        public List<LineInfoPostOrder> Lines { get; set; }
    }

    public class LineDetailPostOrder
    {
        public string POReference { get; set; }
        public int LineNo { get; set; }
        public string ProcessingCode { get; set; }
        public string Order_POSuffix { get; set; }
        public string VendorID { get; set; }
        public string VendorName { get; set; }
        public string CatalogNo { get; set; }
        public string Description { get; set; }
        public string QtyOrdered { get; set; }
        public string GeneralTagging { get; set; }
        public DateTime OrderedDate { get; set; }
        public DateTime RequestedArrivalDate { get; set; }
        public string AckNo { get; set; }
        public DateTime ShipDate { get; set; }
        public DateTime EstimatedArrivalDate { get; set; }
        public string QtyReceived { get; set; }
        public DateTime LatestReceivedDate { get; set; }
        public string Carrier { get; set; }
        public string TrackingNo { get; set; }
        public string FreeformNotes { get; set; }
        public string OrderNo { get; set; }
        public string PoSuffix { get; set; }
        public string Salesperson { get; set; }
        public string QtyDelivered { get; internal set; }
        public string QtyTicketed { get; internal set; }
        public DateTime LastSchedDate { get; set; }
        public string QtyCostVerified { get; set; }
        public string LastScheduleDateColor { get; set; }
    }

    public class LineInfoPostOrder
    {
        public int LineNo { get; set; }
        public string ProcessCode { get; set; }
        public string LineSalesCode { get; set; }
        public string Werehouse_Network { get; set; }
        public string Catalog { get; set; }
        public string Description { get; set; }
        public string OrderedDate { get; set; }
        public string ReceivedDate { get; set; }
        public string Comments { get; set; }
        public string RequestedArrival { get; set; }
        public string Ordered { get; set; }
        public string Received { get; set; }
        public string EstimatedArrivalDate { get; set; }
        public string ShipDate { get; set; }
        public string GeneralTagging { get; set; }
        public string ScheduledArrivalDate { get; set; }
    }
}
