using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.WarehouseDT
{
    public class WarehouseDTViewModel
    {
        public string Date { get; set; }
        public string ScheduleType { get; set; }
        public IEnumerable<InfoWarehouseDT> SummaryInfo { get; set; }
        public bool ShowResults { get; set; }
        public List<string> Warehouse { get; set; }
        public string LinesInfoJson()
        {
            return JsonConvert.SerializeObject(this.SummaryInfo);
        }
        public List<StagingName> StagingNames { get; set; }
        public List<ScheduleType> ScheduleTypes { get; set; }
    }


    public class InfoWarehouseDT
    {
        public string OrderNo { get; set; }
        public string CustomerName { get; set; }
        public string Location { get; set; }
        public string LocationColor { get; set; }
        public string OrderStatus { get; set; }
        public string ScheduleType { get; set; }
        public string Comment { get; set; }
        public string Delivered { get; set; }
        public bool Staged { get; set; }
        public bool Loaded { get; set; }
        public string LoadedValue { get; set; }
        public bool IsDtShipped { get; set; }
        public string ProjectId { get; set; }
        public string HardOrSoft { get; set; }
        public List<LineInfoWarehouseDT> LinesInfo { get; set; }
        public List<int> InfoStagingNames { get; set; }
        public string Truck { get; set; }
        public string StopNo { get; set; }
        public string TypeOfRequest { get; set; }
        public string DeliveryTicket { get; set; }
        public string OrderNoColor { get; set; }
    }

    public class LineInfoWarehouseDT
    {
        public int LineNo { get; set; }
        public string QtyReceivedColor { get; set; }
        public string QtyReceived { get; set; }
        public string QtyOrdered { get; set; }
        public string QtyScheduled { get; set; }
        public string Vendor { get; set; }
        public string CatalogNo { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string LocationColor { get; set; }
        public bool Staged { get; set; }
        public bool Loaded { get; set; }
        public string OrderNo { get; set; }
        public bool IsLoadedLine { get; set; }
        public string QtyCartons { get; set; }
    }

    public class CSVWarehouse
    {
        public string Date { get; set; }
        public string OrderNo { get; set; }
        public string Warehouse { get; set; }
    }

    public class StagingName
    {
        public int StagingNameID { get; set; }
        public string StgName { get; set; }
    }

    public class ScheduleType
    {
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
