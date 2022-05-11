using Dapper;
using Microsoft.Extensions.Configuration;
using OWPApplications.Models;
using OWPApplications.Models.WarehouseDT;
using OWPApplications.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Data.WarehouseDT
{
    public class WarehouseDTHandler
    {
        private IConfiguration _configuration;

        public WarehouseDTHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<InfoWarehouseDT> GetLinesInfo(string date, List<string> warehouse)
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

					SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_WarehouseDTReport_OrderLines]");
					cmd1.CommandType = CommandType.StoredProcedure;

					string param1 = "@From";
					string param2 = "@To";
					string param3 = "@WarehouseList";

					cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), date);
					cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), date);
					cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), dtWarehouse);
					cmd1.Parameters["@" + param3.Replace("@", "")].SqlDbType = SqlDbType.Structured;



						DataTable dt1 = db.SqlCommandToTable(cmd1);

                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return null;
                    }
                    return FillLineInfo(dt1);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IEnumerable<InfoWarehouseDT> FillLineInfo(DataTable table)
        {
            List<InfoWarehouseDT> output = new List<InfoWarehouseDT>();
            string currentOrderNo = "";
            string stagingNames;
            string[] splitedStagingNames;
            string loc = "white";
            InfoWarehouseDT summary = null;
            foreach (DataRow row in table.Rows)
            {
                currentOrderNo = clsLibrary.dBReadString(row["Order_No"]).Trim();
                if (summary == null || currentOrderNo != summary.OrderNo)
                {
                    var linesCount = table.Select("Order_No = " + currentOrderNo).Count();
                    var linesDeliveredCount = table.Select("Order_No = " + currentOrderNo + " AND Delivered = 'YES'").Count();
                    loc = "white";
                    if (linesCount == linesDeliveredCount)
                    {
                        loc = "mediumspringgreen";
                    }
                    summary = new InfoWarehouseDT
                    {
                        OrderNo = currentOrderNo,
                        OrderStatus = clsLibrary.dBReadString(row["Order_Status"]),
                        CustomerName = clsLibrary.dBReadString(row["Customer_Name"]),
                        Location = linesCount == linesDeliveredCount ? "DT SHIPPED" : clsLibrary.dBReadString(row["Whs_Location"]),
                        LocationColor = clsLibrary.dBReadString(row["Whs_Location_Summary_Color"]),
                        ScheduleType = clsLibrary.dBReadString(row["Schedule_Type_Description"]),
                        Comment = clsLibrary.dBReadString(row["comment"]),
                        LinesInfo = new List<LineInfoWarehouseDT>(),
                        ProjectId = clsLibrary.dBReadString(row["project_id"]),
                        HardOrSoft = clsLibrary.dBReadString(row["HardOrSoft"]),
                        Truck = clsLibrary.dBReadString(row["Truck"]),
                        StopNo = clsLibrary.dBReadString(row["StopNo"]),
                        TypeOfRequest = clsLibrary.dBReadString(row["TypeOfRequest"]),
                        DeliveryTicket = clsLibrary.dBReadString(row["Delivery_Ticket"]),
                        OrderNoColor = "white"
                    };
                    output.Add(summary);

                    stagingNames = clsLibrary.dBReadString(row["StagingNameIDs"]);
                    summary.InfoStagingNames = new List<int>();
                    if (stagingNames != "")
                    {
                        splitedStagingNames = stagingNames.Split("|");
                        foreach(var name in splitedStagingNames)
                        {
                            summary.InfoStagingNames.Add(int.Parse(name));
                        }
                    }                    
                }                
                
                summary.LinesInfo.Add(new LineInfoWarehouseDT
                {
                    LineNo = clsLibrary.dBReadInt(row["Line_No"]),
                    Vendor = clsLibrary.dBReadString(row["Vnd_No"]),
                    CatalogNo = clsLibrary.dBReadString(row["Catalog_No"]),
                    Description = clsLibrary.dBReadString(row["Description"]),
                    QtyOrdered = clsLibrary.dBReadString(row["Qty_Ordered"]),
                    QtyReceived = clsLibrary.dBReadString(row["Qty_Received"]),
                    QtyScheduled = clsLibrary.dBReadString(row["Qty_Scheduled"]),
                    QtyReceivedColor = clsLibrary.dBReadString(row["ReceivedColor"]),
                    Location = clsLibrary.dBReadString(row["Delivered"]) == "YES" ? "DT SHIPPED" : clsLibrary.dBReadString(row["Whs_Location"]),
                    LocationColor = clsLibrary.dBReadString(row["Whs_Location_Color"]),
                    Staged = clsLibrary.dBReadInt(row["SL_Flag"]) == 1,
                    Loaded = clsLibrary.dBReadString(row["Staged_Or_Loaded"]) == "L",
                    OrderNo = currentOrderNo,
                    IsLoadedLine = clsLibrary.dBReadBoolean(row["Loaded"]),
                    QtyCartons = clsLibrary.dBReadString(row["QtyCartons"])
                });

                string tempLoc = clsLibrary.dBReadString(row["Whs_Location"]);

                if (loc == "white")
                {
                    if (tempLoc.StartsWith("Z"))
                    {
                        loc = "purple";
                    } else if (tempLoc.StartsWith("IP"))
                    {
                        loc = "yellow";
                    }
                } else if (loc == "purple")
                {
                    if (tempLoc.StartsWith("IP") || tempLoc == "")
                    {
                        loc = "red";
                    }
                } else if (loc == "yellow")
                {
                    if (tempLoc.StartsWith("Z") || tempLoc == "")
                    {
                        loc = "red";
                    }
                }

                foreach (var item in output)
                {
                    if (item.OrderNo == currentOrderNo)
                    {
                        item.OrderNoColor = loc;
                    }
                }

            }

            foreach (var i in output)
            {
                i.LinesInfo = i.LinesInfo.OrderBy(x => x.LineNo).ToList();
            }

            return output;
        }

        public List<StagingName> GetStagingNames()
        {
            try
            {

                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(String.Format(WarehouseDTQueries.STAGING_NAMES));

                    DataTable dt1 = db.SqlCommandToTable(cmd1);

                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return null;
                    }
                    return FillStagingNames(dt1);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<SelectValues> GetScheduleTypes()
        {
            try
            {

                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(String.Format(WarehouseDTQueries.SCHEDULE_TYPES));

                    DataTable dt1 = db.SqlCommandToTable(cmd1);

                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return null;
                    }
                    List<SelectValues> output = new List<SelectValues>();
                    output.Add(new SelectValues
                    {
                        ID = "_ALL",
                        Label = "ALL"
                    });
                    foreach (DataRow row in dt1.Rows)
                    {
                        output.Add(new SelectValues
                        {
                            ID = clsLibrary.dBReadString(row["Schedule_Type"]),
                            Label = clsLibrary.dBReadString(row["Description"])
                        });
                    }
                    return output;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<StagingName> FillStagingNames(DataTable table)
        {
            List<StagingName> output = new List<StagingName>();
            foreach (DataRow row in table.Rows)
            {                
                output.Add(new StagingName
                {
                    StagingNameID = clsLibrary.dBReadInt(row["StagingNameID"]),
                    StgName = clsLibrary.dBReadString(row["StagingName"])
                });
            }
            return output;
        }

        public void UpdateStagingNames(IEnumerable<int> names, string orderno)
        {
            try
            {
                using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
                {
                    db.Open();
                    var custIdsDT = new System.Data.DataTable();
                    custIdsDT.Columns.Add("ID", typeof(int));

                    if (names != null && names.Count() > 0)
                    {
                        foreach (var id in names)
                        {
                            var row = custIdsDT.NewRow();
                            row["ID"] = id;
                            custIdsDT.Rows.Add(row);
                        }
                    }

                    db.Query("dbo.Update_WarehouseDTReport_Order_StagingName",
                               param: new
                               {
                                   names = custIdsDT.AsTableValuedParameter("BIGINTIDTABLETYPE"),
                                   orderno = orderno,
                                   companycode = "W",
                                   ordertype = "O",
                                   modifiedon = DateTime.Now.ToString(),
                                   modifiedby = "SYSTEM"
                               },
                               commandType: System.Data.CommandType.StoredProcedure);                  
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool UpdateComment(string orderNo, string comment)
        {
            try
            {

                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(String.Format(WarehouseDTQueries.UPSERT_COMMENT, comment, orderNo));

                    db.SqlCommandToRow(cmd1);

                    return true;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateLoadedLine(string orderNo, string lineNo, bool isLoaded)
        {
            try
            {
                using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
                {
                    db.Open();

                    db.Query("dbo.Update_WarehouseDTReport_LoadedLines",
                               param: new
                               {
                                   Company_Code = "W",
                                   Order_No = orderNo,
                                   Line_No = lineNo,
                                   Order_Type = "O",
                                   Loaded = isLoaded
                               },
                               commandType: System.Data.CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }        
}
