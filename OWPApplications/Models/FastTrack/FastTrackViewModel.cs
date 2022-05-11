using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.FastTrack
{
    public class FastTrackViewModel
    {
        public string From { get; set; }
        public string To { get; set; }
        public List<string> Warehouse { get; set; }
        public IEnumerable<InfoFastTrack> SummaryInfo { get; set; }
        public List<WeekInfoFastTrack> WeekInfo { get; set; }
        public List<GraphicFastTrack> Graphics { get; set; }
        public bool ShowFastTrackResults { get; set; }
        public bool ShowExpWarehouseRecResults { get; set; }
        public string LinesInfoJson()
        {
            return JsonConvert.SerializeObject(this.SummaryInfo);            
        }
    }

    public class InfoFastTrack
    {
        public string PO { get; set; }
        public string Vendor { get; set; }
        public string ACK { get; set; }
        public string ReceivedStatus { get; set; }
        public string LoadComment { get; set; }
        public string LoadNumbers { get; set; }
        public string EstimatedArrival { get; set; }
        public string NextSchedule { get; set; }
        public string OrderStatus { get; set; }
        public string MultiSchedule { get; set; }
        public string ReceivedStatusColor { get; set; }
        public List<LineInfoFastTrack> LinesInfo { get; set; }
        public string OrderNo { get; set; }
        public int PoSuffix { get; set; }
        public string Comment { get; set; }
        public string ScheduleDateBackgroundColor { get; set; }
        public string Tracking { get; set; }
        public string Carrier { get; set; }
        public string SoftScheduleTestColor { get; set; }
    }

    public class LineInfoFastTrack
    {        
        public string SalesCode { get; set; }
        public int LineNo { get; set; }
        public string Vendor { get; set; }
        public string CatalogNo { get; set; }
        public string Description { get; set; }
        public string ReceivedStatusColor { get; set; }
        public string ReceivedStatus { get; set; }
        public string QtyReceived { get; set; }
        public string QtyOrdered { get; set; }
    }

    public class GraphicFastTrack
	{
        public int Week { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public double Quantity { get; set; }
    }

    public class WeekInfoFastTrack
	{
        public DateTime ExpectedReceiptDate { get; set; }
        public string OrderNo { get; set; }
        public int POSuffix { get; set; }
        public string VndNo { get; set; }
        public string VndName { get; set; }
        public string SalespersonName { get; set; }
        public string CustomerName { get; set; }
        public int LineNo { get; set; }
        public string CatalogNo { get; set; }
        public string Description { get; set; }
        public double QtyOrdered { get; set; }
        public double QtyReceived { get; set; }
        public double ExpectedQty { get; set; }
        public string ACKNo { get; set; }
        public DateTime SchDate { get; set; }
        public string SchDateColor { get; set; }
    }
}
