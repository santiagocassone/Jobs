using Dapper;
using Microsoft.Extensions.Configuration;
using OWPApplications.Models;
using OWPApplications.Models.StockInventory;
using OWPApplications.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Data.StockInventory
{
    public class StockInventoryHandler
    {
        private IConfiguration _configuration;

        public StockInventoryHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<SelectValues> LoadVendors()
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(@"EXEC [dbo].[VendorList_Select] 1");
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
                            ID = clsLibrary.dBReadString(row["Vnd_No"]),
                            Label = clsLibrary.dBReadString(row["Vendor_Name"])
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

        public List<SelectValues> LoadLocations(string region)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    string param1 = "@Region";
                    SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_Location]");
                    cmd1.CommandType = CommandType.StoredProcedure;
                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), region);
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
                            ID = clsLibrary.dBReadString(row["Location_Code"]),
                            Label = clsLibrary.dBReadString(row["Location_Name"])
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

        public List<Inventory> GetStockInventory(string itemNo, string vendor, string productId, string catalogNo, string region, List<int> location)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                var dtLocations = new DataTable();
                dtLocations.Columns.Add("ID", typeof(string));
                if (location?.Count > 0)
                {
                    foreach (var loc in location)
                    {
                        var row = dtLocations.NewRow();
                        row["ID"] = loc;
                        dtLocations.Rows.Add(row);
                    }
                };
                connection.Open();
                var param = new DynamicParameters();
                param.Add("@Item_No", itemNo);
                param.Add("@Vnd_No", vendor);
                param.Add("@Product_ID", productId);
                param.Add("@Cat_No", catalogNo);
                param.Add("@Region", region);
                param.Add("@Locations", dtLocations.AsTableValuedParameter("[dbo].[BIGINTIDTABLETYPE]"));

                List<Inventory> output = connection.Query<Inventory>("[dbo].[Get_StockInventory_Lines]", param, commandType: CommandType.StoredProcedure).ToList();

                var paramQuote = new DynamicParameters();
                paramQuote.Add("@Region", region);
                paramQuote.Add("@Locations", dtLocations.AsTableValuedParameter("[dbo].[BIGINTIDTABLETYPE]"));

                if (output != null && output.Count() > 0)
                {
                    foreach (var item in output)
                    {
                        paramQuote.Add("@Item", item.Item_No);
                        item.QuoteInfos = connection.Query<QuoteInfo>("[dbo].[Get_StockInventory_QtyByQuotes]", paramQuote, commandType: CommandType.StoredProcedure).ToList();
                    }
                }               

                return output;
            }
        }
    }
}
