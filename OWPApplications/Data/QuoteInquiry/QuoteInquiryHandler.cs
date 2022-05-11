using Microsoft.Extensions.Configuration;
using OWPApplications.Models;
using OWPApplications.Models.QuoteInquiry;
using OWPApplications.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using static OWPApplications.Models.QuoteInquiry.QuoteInquiryViewModel;

namespace OWPApplications.Data.QuoteInquiry
{
    public class QuoteInquiryHandler
    {
        IConfiguration _configuration;

        public QuoteInquiryHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public HeaderInfoQouteInquiry LoadHeaderInfo(string quote, string region)
        {
            try
            {

                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL("[dbo].[Get_QuoteInquiry_HeaderInfo]");
                    cmd1.CommandType = CommandType.StoredProcedure;
                    string param2 = "@Order_No";
                    string param1 = "@Region";
                    cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), int.Parse(quote));
                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), region);
                    DataTable dt1 = db.SqlCommandToTable(cmd1);
                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return null;
                    }

                    SqlCommand cmd2 = db.GetCommandSQL("[dbo].[Get_QuoteInquiry_OrderAddress]");
                    cmd2.CommandType = CommandType.StoredProcedure;
                    string param3 = "@Record_Type";
                    cmd2.Parameters.AddWithValue("@" + param2.Replace("@", ""), int.Parse(quote));
                    cmd2.Parameters.AddWithValue("@" + param1.Replace("@", ""), region);
                    cmd2.Parameters.AddWithValue("@" + param3.Replace("@", ""), "D");
                    DataTable dt2 = db.SqlCommandToTable(cmd2);

                    SqlCommand cmd3 = db.GetCommandSQL("[dbo].[Get_QuoteInquiry_OrderAddress]");
                    cmd3.CommandType = CommandType.StoredProcedure;
                    cmd3.Parameters.AddWithValue("@" + param2.Replace("@", ""), int.Parse(quote));
                    cmd3.Parameters.AddWithValue("@" + param1.Replace("@", ""), region);
                    cmd3.Parameters.AddWithValue("@" + param3.Replace("@", ""), "M");
                    DataTable dt3 = db.SqlCommandToTable(cmd3);

                    SqlCommand cmd4 = db.GetCommandSQL("[dbo].[Get_QuoteInquiry_Instructions]");
                    cmd4.CommandType = CommandType.StoredProcedure;
                    string param4 = "@Instruction_Type";
                    cmd4.Parameters.AddWithValue("@" + param2.Replace("@", ""), int.Parse(quote));
                    cmd4.Parameters.AddWithValue("@" + param1.Replace("@", ""), region);
                    cmd4.Parameters.AddWithValue("@" + param4.Replace("@", ""), "D");
                    DataTable dt4 = db.SqlCommandToTable(cmd4);

                    SqlCommand cmd5 = db.GetCommandSQL("[dbo].[Get_QuoteInquiry_Instructions]");
                    cmd5.CommandType = CommandType.StoredProcedure;
                    cmd5.Parameters.AddWithValue("@" + param2.Replace("@", ""), int.Parse(quote));
                    cmd5.Parameters.AddWithValue("@" + param1.Replace("@", ""), region);
                    cmd5.Parameters.AddWithValue("@" + param4.Replace("@", ""), "I");
                    DataTable dt5 = db.SqlCommandToTable(cmd5);

                    SqlCommand cmd6 = db.GetCommandSQL("[dbo].[Get_QuoteInquiry_VendorInfo]");
                    cmd6.CommandType = CommandType.StoredProcedure;
                    cmd6.Parameters.AddWithValue("@" + param2.Replace("@", ""), int.Parse(quote));
                    cmd6.Parameters.AddWithValue("@" + param1.Replace("@", ""), region);
                    DataTable dt6 = db.SqlCommandToTable(cmd6);

                    return new HeaderInfoQouteInquiry(dt1.Rows[0], dt2.Rows[0],
                        (dt3.Rows.Count > 0) ? dt3.Rows[0] : null,
                        (dt4.Rows.Count > 0) ? dt4.Rows[0] : null,
                        (dt5.Rows.Count > 0) ? dt5.Rows[0] : null,
                        dt6);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<LineInfoQuoteInquiry> GetLinesInfo(string quote, List<int> locations, string region)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    var dtLocations = new DataTable();
                    dtLocations.Columns.Add("ID", typeof(string));
                    if (locations?.Count > 0)
                    {
                        foreach (var loc in locations)
                        {
                            var row = dtLocations.NewRow();
                            row["ID"] = loc;
                            dtLocations.Rows.Add(row);
                        }
                    };

                    SqlCommand cmd1 = db.GetCommandSQL("[dbo].[Get_QuoteInquiry_Lines]");
                    cmd1.CommandType = CommandType.StoredProcedure;

                    string param1 = "@OrderNo";
                    string param2 = "@LineType";
                    string param3 = "@Locations";
                    string param4 = "@Region";

                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), int.Parse(quote));
                    cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), 'L');
                    cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), dtLocations);
                    cmd1.Parameters.AddWithValue("@" + param4.Replace("@", ""), region);
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

        public IEnumerable<MiscLinesQuoteInquiry> GetMiscLines(string quote, List<int> locations, string region)
        {
            try
            {

                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    var dtLocations = new DataTable();
                    dtLocations.Columns.Add("ID", typeof(string));
                    if (locations?.Count > 0)
                    {
                        foreach (var loc in locations)
                        {
                            var row = dtLocations.NewRow();
                            row["ID"] = loc;
                            dtLocations.Rows.Add(row);
                        }
                    };

                    SqlCommand cmd1 = db.GetCommandSQL("[dbo].[Get_QuoteInquiry_Lines]");
                    cmd1.CommandType = CommandType.StoredProcedure;

                    string param1 = "@OrderNo";
                    string param2 = "@LineType";
                    string param3 = "@Locations";
                    string param4 = "@Region";

                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), int.Parse(quote));
                    cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), 'M');
                    cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), dtLocations);
                    cmd1.Parameters.AddWithValue("@" + param4.Replace("@", ""), region);
                    cmd1.Parameters["@" + param3.Replace("@", "")].SqlDbType = SqlDbType.Structured;

                    DataTable dt1 = db.SqlCommandToTable(cmd1);
                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return null;
                    }

                    return FillMiscLines(dt1);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetTotalGP(string quote, int includeBO, string region)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    
                    SqlCommand cmd1 = db.GetCommandSQL("[dbo].[Get_Quote_GP]");
                    cmd1.CommandType = CommandType.StoredProcedure;

                    string param1 = "@Order_No";
                    string param2 = "@IncludeBO";
                    string param3 = "@Region";

                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), int.Parse(quote));
                    cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), includeBO);
                    cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), region);

                    DataTable dt1 = db.SqlCommandToTable(cmd1);
                    if (dt1.Rows.Count > 0)
                    {
                        return clsLibrary.dBReadString(dt1.Rows[0]["QuoteGP"]) + '%';
                    }
                    else
                    {
                        return "";
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

		public void UpdateLinesComments(int quoteNo, int lineNo, string comment)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					string query = @"EXEC dbo.Upsert_QuoteInquiry_Lines_Comments '{0}','{1}','{2}','{3}','{4}'";
					SqlCommand sqlCommand = db.GetCommandSQL(String.Format(query, 'W', quoteNo, lineNo, 'Q', comment));

					db.SqlCommandToRow(sqlCommand);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

        private IEnumerable<LineInfoQuoteInquiry> FillLineInfo(DataTable table)
        {
            List<LineInfoQuoteInquiry> output = new List<LineInfoQuoteInquiry>();

            foreach (DataRow row in table.Rows)
            {
                double QtyOrdered = clsLibrary.dBReadDouble(row["Qty_Ordered"]);
                double OrderLineSell = QtyOrdered * clsLibrary.dBReadDouble(row["Unit_Sell"]);
                double ListPricingExt = QtyOrdered * clsLibrary.dBReadDouble(row["Unit_List"]);

                output.Add(new LineInfoQuoteInquiry
                {
                    LineNo = clsLibrary.dBReadInt(row["Line_No"]),
                    CatalogNo = clsLibrary.dBReadString(row["Cat_No"]),
                    Cost = clsLibrary.dBReadDouble(row["Extended_Cost"]).ToString("C"),
                    QtyOrdered = QtyOrdered.ToString(),
                    Description = clsLibrary.dBReadString(row["LineDescription"]),
                    VendorNo = clsLibrary.dBReadString(row["Vnd_No"]),
                    VendorName = clsLibrary.dBReadString(row["Vendor_Name"]).Trim(),
                    GP = (clsLibrary.dBReadDouble(row["GP_Percentage"]) / 100).ToString("P"),
                    HasColor = false,
                    IsBo = clsLibrary.dBReadBoolean(row["IsBo"]),
                    GeneralTagging = clsLibrary.dBReadString(row["GeneralTagging"]),
                    Sell = OrderLineSell.ToString("C"),
                    ListPricing = ListPricingExt.ToString("C"),
                    MergedComments = clsLibrary.dBReadString(row["MergedComments"]),
                    IsOM = clsLibrary.dBReadBoolean(row["IsOM"]),
                    FormattedPostalCode = clsLibrary.dBReadString(row["Formatted_Postal_Code"]),
					GPDlls = clsLibrary.dBReadDouble(row["GP_Cost"]).ToString("C"),
					List = clsLibrary.dBReadDouble(row["Extended_List"]).ToString("C"),
					LineSell = clsLibrary.dBReadDouble(row["Extended_Sell"]).ToString("C"),
					Comments = clsLibrary.dBReadString(row["Comments"])
				});
            }

            return output;
        }

        private IEnumerable<MiscLinesQuoteInquiry> FillMiscLines(DataTable table)
        {
            List<MiscLinesQuoteInquiry> output = new List<MiscLinesQuoteInquiry>();

            foreach (DataRow row in table.Rows)
            {
                output.Add(new MiscLinesQuoteInquiry
                {
                    MiscCharge = clsLibrary.dBReadInt(row["MiscCharge_No"]),
                    VendorNo = clsLibrary.dBReadString(row["Vnd_No"]),
                    SalesCode = clsLibrary.dBReadString(row["sales_code"]),
                    ChargeCode = clsLibrary.dBReadString(row["Charge_Code"]),
                    Sell = clsLibrary.dBReadDouble(row["Sell"]).ToString("C"),
                    Cost = clsLibrary.dBReadDouble(row["Cost"]).ToString("C"),
                    RaceivingCostPercent = clsLibrary.dBReadString(row["MiscChargesReceivingCostPercent"]),
                    TaxCode = clsLibrary.dBReadString(row["Tax_Code"])
                });
            }

            return output;
        }

        public List<SelectValues> GetUsedLocations(string region)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_Location]");
                    cmd1.CommandType = CommandType.StoredProcedure;
                    cmd1.Parameters.AddWithValue("@Region", region);
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
                            ID = clsLibrary.dBReadString(row["Location_Code"]).Trim(),
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

        public VendorEmail GetVendorByNo(string vendorNo, string region)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL("dbo.Get_Vendor_Email");
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.AddWithValue("@App", "QI");
                    cmd1.Parameters.AddWithValue("@Region", region);
                    cmd1.Parameters.AddWithValue("@Vnd_No", vendorNo.Trim());

                    DataTable dt1 = db.SqlCommandToTable(cmd1);
                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return null;
                    }

                    return FillVendorInfo(dt1);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private VendorEmail FillVendorInfo(DataTable table)
        {

            VendorEmail vendorEmails = new VendorEmail
            {
                VendorNo = clsLibrary.dBReadString(table.Rows[0]["Vnd_No"]),
                Name = clsLibrary.dBReadString(table.Rows[0]["vendor_name"]),
                Addresses = new List<string>()
            };

            var emailList = new List<string>();
            List<EmailVendor> emailFields = xmlConfig.GetEmailVendor("qi");
            foreach (EmailVendor oEmailVendor in emailFields)
            {
                try
                {
                    var email = clsLibrary.dBReadString(table.Rows[0][oEmailVendor.Field]).Trim();
                    if (email != "")
                    {
                        if (oEmailVendor.Prefix.Trim() != "")
                        {
                            email = oEmailVendor.Prefix.Trim() + " " + email.Trim();
                        }
                        emailList.Add(email);
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            vendorEmails.Addresses.AddRange(emailList);
            return vendorEmails;
        }
    }
}
