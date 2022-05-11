using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.StandardPrice
{
    public class StandardPriceViewModel
    {
        public bool ShowResults { get; set; }

        public string OrderNo { get; set; }

        public HeaderInfoStandardPrice HeaderInfo { get; set; }

        public IEnumerable<LineInfoStandardPrice> LinesInfos { get; set; }

        public TotalsStandardPrice Totals { get; set; }
    }

    public class HeaderInfoStandardPrice
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

        public HeaderInfoStandardPrice(DataRow row, DataRow instr, DataTable MFG)
        {
            ProjectId = clsLibrary.dBReadString(row["Project_Id"]);
            OrderTitle = clsLibrary.dBReadString(row["Title"]);
            CustomerNo = clsLibrary.dBReadString(row["Customer_No"]);
            SalesIDs = $"{clsLibrary.dBReadString(row["Salesperson_Id_1"])} ({clsLibrary.dBReadString(row["Salesperson_1_Pct"])}%)";
            SalesIDs += (clsLibrary.dBReadString(row["Salesperson_Id_2"]) == "") ? "" : $",{clsLibrary.dBReadString(row["Salesperson_Id_2"])} ({100 - clsLibrary.dBReadInt(row["Salesperson_1_Pct"])}%)";
            CustomerPONo = clsLibrary.dBReadString(row["Customer_PO_Number"]);

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

    public class LineInfoStandardPrice
    {
        public int LineNo { get; set; }
        public string GeneralTagging { get; set; }
        public string LineNotes { get; set; }
        public string VendorNo { get; set; }
        public string CatalogNo { get; set; }
        public string ProductDesc { get; set; }
        public string QtyOrdered { get; set; }
        public string UnitSell { get; set; }
        public string ExtendedSell { get; set; }
        public string UnitCost { get; set; }
        public string ExtendedCost { get; set; }
        public string UnitList { get; set; }
        public string ExtendedList { get; set; }
        public string CostDiscount { get; set; }
        public string GPPct { get; set; }
        public string GPDollars { get; set; }
        public string AutoPriced { get; set; }
        public bool IsBo { get; set; }
    }

    public class TotalsStandardPrice
    {
        public string TotalSell { get; set; }
        public string TotalCost { get; set; }
        public string GPDollars { get; set; }
        public string GPPct { get; set; }
        public string GPColor { get; set; }
        public string Tax { get; set; }
        public string Total_W_Tax { get; set; }

        public string PIDTotalSell { get; set; }
        public string PIDTotalCost { get; set; }
        public string PIDGPDollars { get; set; }
        public string PIDGPPct { get; set; }
        public string PIDGPColor { get; set; }
    }
}
