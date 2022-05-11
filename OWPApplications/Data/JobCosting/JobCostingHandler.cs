using Dapper;
using Microsoft.Extensions.Configuration;
using OWPApplications.Models;
using OWPApplications.Models.Common;
using OWPApplications.Models.JobCosting;
using OWPApplications.Models.JobCostingLeadership;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Data.JobCosting
{
    public class JobCostingHandler
    {
        private IConfiguration _configuration;

        public JobCostingHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<SelectValues> LoadCustomers()
        {
            using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                db.Open();
                var r = db.Query<Customer>("dbo.[Get_Customer_List]", new { salespersonid = "_ALL" }, commandType: System.Data.CommandType.StoredProcedure);

                if (r.Count() > 0)
                {
                    var output = new List<SelectValues>();

                    foreach (var row in r)
                    {
                        output.Add(new SelectValues
                        {
                            ID = row.customer_no,
                            Label = row.name
                        });
                    }

                    return output;
                }
                return null;
            }
        }

        public List<SelectValues> LoadLeads()
        {
            using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                db.Open();
                var r = db.Query("dbo.[Get_JobCosting_Filters]", new { FilterName = "Lead" }, commandType: System.Data.CommandType.StoredProcedure);

                if (r.Count() > 0)
                {
                    var output = new List<SelectValues>();

                    foreach (var row in r)
                    {
                        output.Add(new SelectValues
                        {
                            ID = row.Available_Values,
                            Label = row.Available_Values
                        });
                    }

                    return output;
                }
                return null;
            }
        }

        public List<SelectValues> LoadWarehouses()
        {
            using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                db.Open();

                var r = db.Query("dbo.[Get_JobCosting_Filters]", new { FilterName = "Warehouse", Company_Code = "W" }, commandType: System.Data.CommandType.StoredProcedure);

                if (r.Count() > 0)
                {
                    var output = new List<SelectValues>();

                    foreach (var row in r)
                    {
                        output.Add(new SelectValues
                        {
                            ID = row.Available_Values,
                            Label = row.Available_Values
                        });
                    }

                    return output;
                }
                return null;
            }
        }

        public List<Report> GetReport(string fromDate, string toDate, string projectid, string orderno, string[] customerid, string warehoseid, string leadid)
        {
            using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                db.Open();

                var custIdsDT = new System.Data.DataTable();
                custIdsDT.Columns.Add("ID", typeof(string));

                if (customerid != null && customerid.Count() > 0)
                {
                    foreach (var id in customerid)
                    {
                        var row = custIdsDT.NewRow();
                        row["ID"] = id;
                        custIdsDT.Rows.Add(row);
                    }
                }

                var r = db.Query<Report>(
                    "dbo.[Get_JobCosting_Main_New]",
                    param: new
                    {
                        Customer_No_Table = custIdsDT.AsTableValuedParameter("VARCHARIDTABLETYPE"),
                        DateFrom = fromDate,
                        DateTo = toDate,
                        Project_ID = projectid,
                        Order_No = orderno,
                        Warehouse_No = warehoseid,
                        Lead = leadid
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();

                if (r.Count() > 0)
                {
                    return r;
                }
                return null;
            }
        }

        public List<Summary> GetSummary(string fromDate, string toDate, string projectid, string region, string[] customerid, string location)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
                {
                    connection.Open();

                    var custIdsDT = new System.Data.DataTable();
                    custIdsDT.Columns.Add("ID", typeof(string));

                    if (customerid != null && customerid.Count() > 0)
                    {
                        foreach (var id in customerid)
                        {
                            var row = custIdsDT.NewRow();
                            row["ID"] = id;
                            custIdsDT.Rows.Add(row);
                        }
                    }

                    var param = new DynamicParameters();
                    param.Add("@Customer_No_Table", custIdsDT.AsTableValuedParameter("[dbo].[VARCHARIDTABLETYPE]"));
                    param.Add("@DateFrom", fromDate);
                    param.Add("@DateTo", toDate);
                    param.Add("@Project_ID", projectid);
                    param.Add("@Region", region);
                    param.Add("@Location_ID", location);

                    List<Summary> output = connection.Query<Summary>("dbo.[Get_JobCostingLeadership_Main]", param, commandType: CommandType.StoredProcedure).ToList();

                    return output;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Detail> GetLeadershipProjectDetails(string projectid, string region, string fromDate, string toDate)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
                {
                    connection.Open();

                    var param = new DynamicParameters();
                    param.Add("@DateFrom", fromDate);
                    param.Add("@DateTo", toDate);
                    param.Add("@Project_ID", projectid);
                    param.Add("@Region", region);

                    List<Detail> output = connection.Query<Detail>("dbo.[Get_JobCostingLeadership_DetailScreen]", param, commandType: CommandType.StoredProcedure).ToList();

                    return output;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<ProjectDetail> GetProjectDetails(string projectid)
        {
            using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                db.Open();

                var r = db.Query<ProjectDetail>(
                    "dbo.[Get_JobCosting_ProjectDetail_New]",
                    param: new
                    {
                        Project_ID = projectid
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();

                if (r.Count() > 0)
                {
                    return r;
                }

                return null;
            }
        }

        public List<OrderDetail> GetOrderDetails(string orderindex, DateTime orderDateFrom, DateTime orderDateTo)
        {
            using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                db.Open();

                if (orderDateFrom == DateTime.Parse("1/1/0001 12:00:00 AM"))
                {
                    orderDateFrom = DateTime.Parse("1/1/1753 12:00:00 AM");
                }

                if (orderDateTo == DateTime.Parse("1/1/0001 12:00:00 AM"))
                {
                    orderDateTo = DateTime.Parse("12/31/9999 12:00:00 AM");
                }

                var r = db.Query<OrderDetail>(
                    "dbo.[Get_JobCosting_OrderDetail_New]",
                    param: new
                    {
                        DebugMode = 0,
                        Order_Index = orderindex,
                        DateFrom = orderDateFrom,
                        DateTo = orderDateTo
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();

                if (r.Count() > 0)
                {
                    return r;
                }

                return null;
            }
        }
        public List<JCLocations> GetLocations()
        {
            try
            {
                using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
                {
                    db.Open();
                    return db.Query<JCLocations>("dbo.[Get_JobCostingLeadership_Locations]", commandType: System.Data.CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
