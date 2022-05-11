using Microsoft.Extensions.Configuration;
using OWPApplications.Models;
using OWPApplications.Models.SubcontractorReceiving;
using OWPApplications.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Data.SubcontractorReceiving
{
    public class SubcontractorReceivingHandler
    {
        private IConfiguration _configuration;

        public SubcontractorReceivingHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public UserProfile ValidateAndGetVendorNo(string email, string pass)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(String.Format(@"EXEC dbo.Get_Subcontractor_VendorNo '{0}','{1}'", email, pass));
                    DataTable dt1 = db.SqlCommandToTable(cmd1);

                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return null;
                    }

                    UserProfile user = new UserProfile();
                    user.UserCode = clsLibrary.dBReadString(dt1.Rows[0]["VendorNo"]);
                    user.UserName = clsLibrary.dBReadString(dt1.Rows[0]["VendorName"]);

                    return user;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<LookupGeneral> LoadLookupGeneral(string type)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(@"EXEC dbo.Get_LookupGeneral " + type);
                    DataTable dt1 = db.SqlCommandToTable(cmd1);

                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return null;
                    }

                    List<LookupGeneral> output = new List<LookupGeneral>();

                    foreach (DataRow row in dt1.Rows)
                    {
                        output.Add(new LookupGeneral
                        {
                            Name = clsLibrary.dBReadString(row["Name"]),
                            Value = clsLibrary.dBReadString(row["TranslateValue"]),
                            LookupGeneralID = clsLibrary.dBReadString(row["LookupGeneralID"])
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

        public IEnumerable<SelectValues> GetVendorList()
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(@"EXEC dbo.VendorList_Select");
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

        public IEnumerable<SummaryInfo> GetSummaryInfo(string vendorNo, string dateFrom, string dateTo, int? orderNo)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_Subcontractor_Lines]");
                    cmd1.CommandType = CommandType.StoredProcedure;

                    string param1 = "@Vendor_No";
                    string param2 = "@PO_Reference";
                    string param3 = "@Detailed";
                    string param4 = "@DateFrom";
                    string param5 = "@DateTo";
                    string param6 = "@Order_No";

                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), vendorNo);
                    cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), "");
                    cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), false);
                    cmd1.Parameters.AddWithValue("@" + param4.Replace("@", ""), dateFrom);
                    cmd1.Parameters.AddWithValue("@" + param5.Replace("@", ""), dateTo);
                    cmd1.Parameters.AddWithValue("@" + param6.Replace("@", ""), orderNo);

                    DataTable dt1 = db.SqlCommandToTable(cmd1);

                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return null;
                    }

                    return FillSummaryInfo(dt1);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<LineInfo> GetLineInfo(string vendorNo, string dateFrom, string dateTo, string poRef)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_Subcontractor_Lines]");
                    cmd1.CommandType = CommandType.StoredProcedure;

                    string param1 = "@Vendor_No";
                    string param2 = "@PO_Reference";
                    string param3 = "@Detailed";
                    string param4 = "@DateFrom";
                    string param5 = "@DateTo";

                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), vendorNo);
                    cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), poRef);
                    cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), true);
                    cmd1.Parameters.AddWithValue("@" + param4.Replace("@", ""), dateFrom);
                    cmd1.Parameters.AddWithValue("@" + param5.Replace("@", ""), dateTo);

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
		public void UpdateQtyReceived(char companyCode, int poSuffix, int orderIndex, int lineIndex, int newValue)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					SqlCommand cmd1 = db.GetCommandSQL(String.Format(@"EXEC dbo.Upsert_Subcontractor_LinesQtyReceived '{0}','{1}','{2}','{3}','{4}'", companyCode, poSuffix, orderIndex, lineIndex, newValue));
					DataTable dt1 = db.SqlCommandToTable(cmd1);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

        private IEnumerable<SummaryInfo> FillSummaryInfo(DataTable table)
        {
            List<SummaryInfo> output = new List<SummaryInfo>();
            SummaryInfo summary;

            foreach (DataRow row in table.Rows)
            {
                summary = new SummaryInfo
                {
                    POSuffix = clsLibrary.dBReadString(row["PO_Suffix"]),
                    POReference = clsLibrary.dBReadString(row["PO_Reference"]),
                    Salesperson = clsLibrary.dBReadString(row["Salesperson"]),
                    InterimVndNo = clsLibrary.dBReadString(row["Interim_Vnd_No"]),
                    VendorName = clsLibrary.dBReadString(row["Vendor_Name"]),
                    DateEntered = clsLibrary.dBReadString(row["Date_Entered"]),
                    AckNo = clsLibrary.dBReadString(row["Ack #"]),
                    ShipDate = clsLibrary.dBReadString(row["Ship Date"]),
                    ExpectedReceiptDate = clsLibrary.dBReadString(row["Expected Receipt Date"]),
                    ReceivedDate = clsLibrary.dBReadString(row["Received Date"]),
                    ScheduledReceiptDate = clsLibrary.dBReadString(row["Scheduled Receipt Date"]),
                    Carrier = clsLibrary.dBReadString(row["Carrier"]),
                    TrackingNo = clsLibrary.dBReadString(row["Tracking #"]),
                    TotallyReceived = clsLibrary.dBReadString(row["Received"]),
					CatalogVendor = clsLibrary.dBReadString(row["Catalog_Vendor"]),
					OrderLastRequest = clsLibrary.dBReadString(row["Order_Last_Request"]),
                    CatalogVendorNo = clsLibrary.dBReadString(row["Catalog_Vendor_No"]),
                    OrderNo = clsLibrary.dBReadString(row["order_no"]),
                    TotalQtyOrdered = clsLibrary.dBReadInt(row["Total_Qty_Ordered"]),
                    OrderTitle = clsLibrary.dBReadString(row["title"])
                };

                output.Add(summary);
            }

            return output;
        }

        private IEnumerable<LineInfo> FillLineInfo(DataTable table)
        {
            List<LineInfo> output = new List<LineInfo>();
            LineInfo line;

            foreach (DataRow row in table.Rows)
            {
                line = new LineInfo
                {
                    POSuffix = clsLibrary.dBReadString(row["PO_Suffix"]),
                    POReference = clsLibrary.dBReadString(row["PO_Reference"]),
                    CatNo = clsLibrary.dBReadString(row["Cat_no"]),
                    Description = clsLibrary.dBReadString(row["Description"]),
                    LineNo = clsLibrary.dBReadString(row["Line_No"]),
                    LineIndex = clsLibrary.dBReadString(row["Line_Index"]),
                    Salesperson = clsLibrary.dBReadString(row["Salesperson"]),
                    InterimVndNo = clsLibrary.dBReadString(row["Interim_Vnd_No"]),
                    VendorName = clsLibrary.dBReadString(row["Vendor_Name"]),
                    DateEntered = clsLibrary.dBReadString(row["Date_Entered"]),
                    AckNo = clsLibrary.dBReadString(row["Ack #"]),
                    ShipDate = clsLibrary.dBReadString(row["Ship Date"]),
                    ExpectedReceiptDate = clsLibrary.dBReadString(row["Expected Receipt Date"]),
                    ReceivedDate = clsLibrary.dBReadString(row["Received Date"]),
                    ScheduledReceiptDate = clsLibrary.dBReadString(row["Scheduled Receipt Date"]),
                    Carrier = clsLibrary.dBReadString(row["Carrier"]),
                    TrackingNo = clsLibrary.dBReadString(row["Tracking #"]),
                    QtyOrdered = clsLibrary.dBReadInt(row["Qty_Ordered"]),
                    QtyReceived = clsLibrary.dBReadString(row["Qty_Received"]),
                    Received = clsLibrary.dBReadDecimal(row["Received"]),
                    ProcessingCode = clsLibrary.dBReadString(row["Processing_Code"]),
                    ReceiveGoods = clsLibrary.dBReadString(row["Receive_Goods"]),
                    CustomerName = clsLibrary.dBReadString(row["Customer_Name"]),
					ProductVendor = clsLibrary.dBReadString(row["Catalog_Vendor"]),
					LineLastRequest = clsLibrary.dBReadString(row["Last_Request_Date"]),
					LineQtyReceived = clsLibrary.dBReadInt(row["Line_Qty_Received"]),
					LQRWasModified = clsLibrary.dBReadBoolean(row["LQR_Was_Modified"]),
					OrderIndex = clsLibrary.dBReadString(row["order_index"]),
					CompanyCode = clsLibrary.dBReadString(row["company_code"]),
					OrderNo = clsLibrary.dBReadString(row["order_no"]),
                    CatalogVendorNo = clsLibrary.dBReadString(row["Catalog_Vendor_No"])
                };

                output.Add(line);
            }

            return output;
        }
    }
}
