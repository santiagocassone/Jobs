using Dapper;
using Microsoft.Extensions.Configuration;
using OWPApplications.Models;
using OWPApplications.Models.ISRManagerView;
using OWPApplications.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Data.ISRManagerView
{
    public class ISRManagerViewHandler
    {
        private IConfiguration _configuration;

        public ISRManagerViewHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<SelectValues> LoadSalesDirectors()
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_InternalStatusReport_SalesDirector]");
                    DataTable dt1 = db.SqlCommandToTable(cmd1);

                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return null;
                    }

                    List<SelectValues> output = new List<SelectValues>();

                    foreach (DataRow row in dt1.Rows)
                    {
                        output.Add(new SelectValues
                        {
                            ID = clsLibrary.dBReadString(row["salesdirector_id"]),
                            Label = clsLibrary.dBReadString(row["salesdirector_info"])
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

        public List<SelectValues> LoadSalesSupportManagers()
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_InternalStatusReport_SuppMngr]");
                    DataTable dt1 = db.SqlCommandToTable(cmd1);

                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return null;
                    }

                    List<SelectValues> output = new List<SelectValues>();

                    foreach (DataRow row in dt1.Rows)
                    {
                        output.Add(new SelectValues
                        {
                            ID = clsLibrary.dBReadString(row["suppmngr_id"]),
                            Label = clsLibrary.dBReadString(row["suppmngr_info"])
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

        private List<SelectValues> LoadCustomerList(string salesperson)
        {
            using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                db.Open();
                var r = db.Query<CustomersModel>("dbo.[Get_Customer_List]", new { salespersonid = salesperson }, commandType: CommandType.StoredProcedure);

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

                    return output.OrderBy(x => x.Label).ToList();
                }

                return null;
            }
        }

        public List<Salesperson> GetSalespersonInfo(string salesdirector, string salessupportmanager, string cutoffdate)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_InternalStatusReport_OrderLines]");
                    cmd1.CommandType = CommandType.StoredProcedure;

                    //var selectedcustomers = LoadCustomerList(salesdirector ?? salessupportmanager).Select(x => x.ID).ToArray();

                    //var custIdsDT = new DataTable();
                    //custIdsDT.Columns.Add("ID", typeof(string));

                    //if (selectedcustomers != null && selectedcustomers.Count() > 0)
                    //{
                    //    foreach (var id in selectedcustomers)
                    //    {
                    //        var row = custIdsDT.NewRow();
                    //        row["ID"] = id;
                    //        custIdsDT.Rows.Add(row);
                    //    }
                    //}

                    string param1 = "@SALESPERSON_ID";
                    string param2 = "@CutoffDate";
                    string param3 = "@Order_No";
                    string param4 = "@CUSTOMERNOTABLE";
                    string param5 = "@SLSDIRECTOR_ID";
                    string param6 = "@SUPPMNGR_ID";

                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), "");
                    cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), cutoffdate);
                    cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), "");
                    cmd1.Parameters.AddWithValue("@" + param4.Replace("@", ""), null);
                    cmd1.Parameters.AddWithValue("@" + param5.Replace("@", ""), salesdirector);
                    cmd1.Parameters.AddWithValue("@" + param6.Replace("@", ""), salessupportmanager);
                    cmd1.Parameters["@" + param4.Replace("@", "")].SqlDbType = SqlDbType.Structured;

                    DataTable dt1 = db.SqlCommandToTable(cmd1);
                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return new List<Salesperson>();
                    }

                    return FillSalespersonInfo(dt1);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<Salesperson> FillSalespersonInfo(DataTable table)
        {
            List<Salesperson> output = new List<Salesperson>();

            foreach (DataRow row in table.Rows)
            {
                output.Add(new Salesperson()
                {
                    SalespersonID = clsLibrary.dBReadString(row["salesperson_id"]),
                    SalespersonName = clsLibrary.dBReadString(row["salespeson_name"]),
                    PartialInvoicingSellEligible = clsLibrary.dBReadDouble(row["SellEligibleforPartialInvoicing"]),
                    TotalSell = clsLibrary.dBReadDouble(row["open_sell"]),
                    TotalCost = clsLibrary.dBReadDouble(row["open_cost"]),
                    GPDollars = clsLibrary.dBReadDouble(row["GP"]),
                    GPPct = clsLibrary.dBReadDouble(row["GPPCt"]) / 100,
					IsSplitted = clsLibrary.dBReadBoolean(row["IsSplitted"]),
                    OvdQty = clsLibrary.dBReadDouble(row["OverdueCRD_Qty"]),
                    OvdPct = clsLibrary.dBReadDouble(row["Overdue_CRD_Sell"])
                });
            }

            return output;
        }
    }

    public class CustomersModel
    {
        public string customer_no { get; set; }
        public string name { get; set; }
    }
}
