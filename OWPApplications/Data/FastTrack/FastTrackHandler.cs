using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OWPApplications.Models.FastTrack;
using OWPApplications.Utils;

namespace OWPApplications.Data.FastTrack
{
    public class FastTrackHandler
    {
        private IConfiguration _configuration;

        public FastTrackHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<InfoFastTrack> GetLinesInfo(string from, string to, List<string> warehouse)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    var dtWarehouse = new DataTable();
                    dtWarehouse.Columns.Add("ID", typeof(string));
                    if (warehouse != null && warehouse.Any())
                    {
                        foreach (var id in warehouse)
                        {
                            var row = dtWarehouse.NewRow();
                            row["ID"] = id;
                            dtWarehouse.Rows.Add(row);
                        }
                    };

                    SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_FastTrackReport_Lines]");
                    cmd1.CommandType = CommandType.StoredProcedure;

                    string param1 = "@Date1";
                    string param2 = "@Date2";
                    string param3 = "@QueryType";
                    string param4 = "@WarehouseList";

                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), from);
                    cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), to);
                    cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), "L");
                    cmd1.Parameters.AddWithValue("@" + param4.Replace("@", ""), dtWarehouse);
                    cmd1.Parameters["@" + param4.Replace("@", "")].SqlDbType = SqlDbType.Structured;

                    DataTable dt1 = db.SqlCommandToTable(cmd1);
                    if (dt1 == null || dt1.Rows.Count == 0) return null;

                    return FillLineInfo(dt1);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IEnumerable<InfoFastTrack> FillLineInfo(DataTable table)
        {
            List<InfoFastTrack> output = new List<InfoFastTrack>();
            string currentPoNumber = "";
            InfoFastTrack summary = null;
            foreach (DataRow row in table.Rows)
            {
                currentPoNumber = clsLibrary.dBReadString(row["PONumber"]);
                if (summary == null || currentPoNumber != summary.PO)
                {
                    summary = new InfoFastTrack
                    {
                        PO = currentPoNumber,
                        OrderNo = clsLibrary.dBReadString(row["OrderNo"]),
                        PoSuffix = clsLibrary.dBReadInt(row["PO_Suffix"]),
                        Vendor = clsLibrary.dBReadString(row["Vendor_Name"]),
                        ACK = clsLibrary.dBReadString(row["ACKNumber"]).Trim(),
                        ReceivedStatusColor = clsLibrary.dBReadString(row["SummaryReceivedStatusColor"]),
                        ReceivedStatus = GetReceivedStatus(clsLibrary.dBReadString(row["SummaryReceivedStatusColor"])),
                        EstimatedArrival = clsLibrary.dbReadDateAsStringFormat(row["EstimatedArrivalDate"], "MM/dd/yyyy"),
                        NextSchedule = clsLibrary.dBReadString(row["ScheduledDate"]),
                        OrderStatus = clsLibrary.dBReadString(row["Order_Status"]),
                        MultiSchedule = clsLibrary.dBReadString(row["MultiScheduledDate"]),
                        Comment = clsLibrary.dBReadString(row["Comment"]),
                        LoadComment = clsLibrary.dBReadString(row["LoadComment"]),
                        LoadNumbers = clsLibrary.dBReadString(row["LoadNumbers"]),
                        ScheduleDateBackgroundColor = clsLibrary.dBReadString(row["ScheduleDateBackgroundColor"]),
                        SoftScheduleTestColor = clsLibrary.dBReadString(row["SoftScheduleTestColor"]),
                        Carrier = clsLibrary.dBReadString(row["Carrier"]),
                        Tracking = clsLibrary.dBReadString(row["Tracking"]),
                        LinesInfo = new List<LineInfoFastTrack>()
                    };
                    output.Add(summary);
                }
                summary.LinesInfo.Add(new LineInfoFastTrack
                    {   SalesCode = clsLibrary.dBReadString(row["Sales_Code"]),
                        LineNo = clsLibrary.dBReadInt(row["Line_No"]),
                        Vendor = clsLibrary.dBReadString(row["Vnd_No"]),
                        CatalogNo = clsLibrary.dBReadString(row["Cat_No"]),
                        Description = clsLibrary.dBReadString(row["Description2L"]),
                        ReceivedStatusColor = clsLibrary.dBReadString(row["ReceivedStatusColor"]),
                        ReceivedStatus = GetReceivedStatus(clsLibrary.dBReadString(row["ReceivedStatusColor"])),
                        QtyOrdered = clsLibrary.dBReadString(row["QtyOrdered"]),
                        QtyReceived = clsLibrary.dBReadString(row["QtyReceived"])
                });
                
            }

            return output;
        }

        public List<GraphicFastTrack> GetGraphicFastTrack(List<string> warehouse)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    var dtWarehouse = new DataTable();
                    dtWarehouse.Columns.Add("ID", typeof(string));
                    if (warehouse != null && warehouse.Any())
                    {
                        foreach (var id in warehouse)
                        {
                            var row = dtWarehouse.NewRow();
                            row["ID"] = id;
                            dtWarehouse.Rows.Add(row);
                        }
                    };

                    SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_FastTrackReport_Graphic]");
                    cmd1.CommandType = CommandType.StoredProcedure;

                    string param1 = "@WarehouseList";

                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), dtWarehouse);
                    cmd1.Parameters["@" + param1.Replace("@", "")].SqlDbType = SqlDbType.Structured;

                    DataTable dt1 = db.SqlCommandToTable(cmd1);
                    if (dt1 == null || dt1.Rows.Count == 0) return null;

                    List<GraphicFastTrack> output = new List<GraphicFastTrack>();
                    GraphicFastTrack graphicFastTrack;

                    foreach (DataRow row in dt1.Rows)
                    {
                        graphicFastTrack = new GraphicFastTrack()
                        {
                            Week = clsLibrary.dBReadInt(row["Week"]),
                            DateFrom = clsLibrary.dBReadDate(row["DateFrom"]),
                            DateTo = clsLibrary.dBReadDate(row["DateTo"]),
                            Quantity = clsLibrary.dBReadDouble(row["Qty"])
                        };
                        output.Add(graphicFastTrack);
                    }

                    return output;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<WeekInfoFastTrack> GetWeekInfo(string from, string to, List<string> warehouse)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    var dtWarehouse = new DataTable();
                    dtWarehouse.Columns.Add("ID", typeof(string));
                    if (warehouse != null && warehouse.Any())
                    {
                        foreach (var id in warehouse)
                        {
                            var row = dtWarehouse.NewRow();
                            row["ID"] = id;
                            dtWarehouse.Rows.Add(row);
                        }
                    };

                    SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_FastTrackReport_Lines]");
                    cmd1.CommandType = CommandType.StoredProcedure;

                    string param1 = "@Date1";
                    string param2 = "@Date2";
                    string param3 = "@QueryType";
                    string param4 = "@WarehouseList";

                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), from);
                    cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), to);
                    cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), "B");
                    cmd1.Parameters.AddWithValue("@" + param4.Replace("@", ""), dtWarehouse);
                    cmd1.Parameters["@" + param4.Replace("@", "")].SqlDbType = SqlDbType.Structured;

                    DataTable dt1 = db.SqlCommandToTable(cmd1);
                    if (dt1 == null || dt1.Rows.Count == 0) return null;

                    return FillWeekInfo(dt1);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<WeekInfoFastTrack> FillWeekInfo(DataTable table)
        {
            List<WeekInfoFastTrack> output = new List<WeekInfoFastTrack>();
            WeekInfoFastTrack weekInfo;
            foreach (DataRow row in table.Rows)
            {
                    weekInfo = new WeekInfoFastTrack
                    {
                        ExpectedReceiptDate = clsLibrary.dBReadDate(row["ExpectedReceiptDate"]),
                        OrderNo = clsLibrary.dBReadString(row["OrderNo"]),
                        POSuffix = clsLibrary.dBReadInt(row["POSuffix"]),
                        VndNo = clsLibrary.dBReadString(row["VndNo"]),
                        VndName = clsLibrary.dBReadString(row["VndName"]),
                        SalespersonName = clsLibrary.dBReadString(row["SalespersonName"]),
                        CustomerName = clsLibrary.dBReadString(row["CustomerName"]),
                        LineNo = clsLibrary.dBReadInt(row["LineNo"]),
                        CatalogNo = clsLibrary.dBReadString(row["CatalogNo"]),
                        Description = clsLibrary.dBReadString(row["Description"]),
                        QtyOrdered = clsLibrary.dBReadDouble(row["QtyOrdered"]),
                        QtyReceived = clsLibrary.dBReadDouble(row["QtyReceived"]),
                        ExpectedQty = clsLibrary.dBReadDouble(row["ExpectedQty"]),
                        ACKNo = clsLibrary.dBReadString(row["ACKNo"]),
                        SchDate = clsLibrary.dBReadDate(row["SchDate"]),
                        SchDateColor = clsLibrary.dBReadString(row["SchDateColor"])
                    };

                    output.Add(weekInfo);
            }

            return output;
        }

        private string GetReceivedStatus(string color)
        {
            switch (color)
            {
                case "White": return "N";
                case "Green": return "C";
                case "Yellow": return "P";
                default: return "";
            }
        }

        public bool UpdateValues(string orderNo, int po, string comment, string source, string field)
        {
            try
            {

                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(String.Format(FastTrackQueries.UPSERT_VALUES, comment, orderNo, po, source, field));

                    db.SqlCommandToRow(cmd1);

                    return true;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
