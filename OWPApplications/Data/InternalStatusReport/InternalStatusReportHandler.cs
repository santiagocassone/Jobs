using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OWPApplications.Models;
using OWPApplications.Models.InternalStatusReport;
using OWPApplications.Utils;

namespace OWPApplications.Data.InternalStatusReport
{
    public class CustomersModel
    {
        public string customer_no { get; set; }
        public string name { get; set; }
    }

    public class SalespersonsModel
    {
        public string salesperson_id { get; set; }
        public string salesperson_info { get; set; }
    }

    public class InternalStatusReportHandler
    {
        private IConfiguration _configuration;
        private ILogger _logger;

        public InternalStatusReportHandler(IConfiguration configuration, ILoggerFactory logFactory)
        {
            _configuration = configuration;
            _logger = logFactory.CreateLogger(nameof(InternalStatusReportHandler));
        }

        public HeaderInfoInternalStatus LoadHeaderInfo(string order, string region)
        {
            try
            {
                DataTable dt1 = new DataTable();
                DataTable dt2 = new DataTable();
                DataTable dt3 = new DataTable();
                DataTable dt4 = new DataTable();
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    string param1 = "@Order_No";
                    string param2 = "@Region";

                    SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_InternalStatusReport_Header]");

                    cmd1.CommandType = CommandType.StoredProcedure;
                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), order);
                    cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), region);
                    dt1 = db.SqlCommandToTable(cmd1);

                    SqlCommand cmd2 = db.GetCommandSQL(@"[dbo].[Get_InternalStatusReport_Instructions]");

                    cmd2.CommandType = CommandType.StoredProcedure;
                    cmd2.Parameters.AddWithValue("@" + param1.Replace("@", ""), order);
                    cmd2.Parameters.AddWithValue("@" + param2.Replace("@", ""), region);
                    dt2 = db.SqlCommandToTable(cmd2);

                    SqlCommand cmd3 = db.GetCommandSQL(@"[dbo].[Get_InternalStatusReport_MFG]");

                    cmd3.CommandType = CommandType.StoredProcedure;
                    cmd3.Parameters.AddWithValue("@" + param1.Replace("@", ""), order);
                    cmd3.Parameters.AddWithValue("@" + param2.Replace("@", ""), region);
                    dt3 = db.SqlCommandToTable(cmd3);

                    SqlCommand cmd4 = db.GetCommandSQL(@"[dbo].[Get_InternalStatusReport_LinesLocations]");
                    param1 = "@OrderNo";
                    cmd4.CommandType = CommandType.StoredProcedure;
                    cmd4.Parameters.AddWithValue("@" + param1.Replace("@", ""), order);
                    cmd3.Parameters.AddWithValue("@" + param2.Replace("@", ""), region);
                    dt4 = db.SqlCommandToTable(cmd4);
                }

                if (dt1 == null || dt1.Rows.Count == 0)
                {
                    return null;
                }
                return new HeaderInfoInternalStatus(dt1.Rows[0], dt2.Rows.Count > 0 ? dt2.Rows[0] : null, dt3, dt4);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<SummaryInfoInternalStatus> GetLinesInfo(string orderno, List<string> salesperson, string cutoffdate, IEnumerable<string> selectedcustomers, string salespersontype, string regionISR, string projfolder)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    var custIdsDT = new System.Data.DataTable();
                    custIdsDT.Columns.Add("ID", typeof(string));

                    if (selectedcustomers != null && selectedcustomers.Any())
                    {
                        foreach (var id in selectedcustomers)
                        {
                            var row = custIdsDT.NewRow();
                            row["ID"] = id;
                            custIdsDT.Rows.Add(row);
                        }
                    };

                    var salespDT = new System.Data.DataTable();
                    salespDT.Columns.Add("ID", typeof(string));

                    if (salesperson != null && salesperson.Any())
                    {
                        foreach (var id in salesperson)
                        {
                            var row = salespDT.NewRow();
                            row["ID"] = id;
                            salespDT.Rows.Add(row);
                        }
                    };

                    SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_InternalStatusReport_OrderLines]");

                    cmd1.CommandType = CommandType.StoredProcedure;

                    string param1 = "@SALESPERSON_ID";
                    string param2 = "@CutoffDate";
                    string param3 = "@Order_No";
                    string param4 = "@CUSTOMERNOTABLE";
                    string param5 = "@SLSDIRECTOR_ID";
                    string param6 = "@SUPPMNGR_ID";
                    string param7 = "@SalespersonList";
                    string param8 = "@Accountability";
                    string param9 = "@Salesperson_Type";
                    string param10 = "@Region";
                    string param11 = "@Project_ID";

                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), "");
                    cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), cutoffdate);
                    cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), orderno);
                    cmd1.Parameters.AddWithValue("@" + param4.Replace("@", ""), custIdsDT);
                    cmd1.Parameters.AddWithValue("@" + param5.Replace("@", ""), "");
                    cmd1.Parameters.AddWithValue("@" + param6.Replace("@", ""), "");
                    cmd1.Parameters.AddWithValue("@" + param7.Replace("@", ""), salespDT);
                    cmd1.Parameters.AddWithValue("@" + param8.Replace("@", ""), "All");
                    cmd1.Parameters.AddWithValue("@" + param9.Replace("@", ""), Convert.ToInt32(salespersontype));
                    cmd1.Parameters.AddWithValue("@" + param10.Replace("@", ""), regionISR);
                    cmd1.Parameters.AddWithValue("@" + param11.Replace("@", ""), projfolder);
                    cmd1.Parameters["@" + param7.Replace("@", "")].SqlDbType = SqlDbType.Structured;
                    cmd1.Parameters["@" + param4.Replace("@", "")].SqlDbType = SqlDbType.Structured;

                    DataTable dt1 = db.SqlCommandToTable(cmd1);
                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return new List<SummaryInfoInternalStatus>();
                    }

                    return FillLineInfo(dt1);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<SummaryInfoInternalStatus> FillLineInfo(DataTable table)
        {
            List<SummaryInfoInternalStatus> output = new List<SummaryInfoInternalStatus>();
            string currentOrderNo = "";
            SummaryInfoInternalStatus summary = null;
            bool addVendor = true;
            string tempVend;
            string tempOrder;
            string tempSuffix;
            string tempInvVnd;
            string estArrDate;
            string[] exclVend = { "AME01", "BRA00", "ONE20", "ONE22", "ONE23", "ONE24", "ONE26", "ONE27", "ONE28", "ONE2W", "STE01" };
            foreach (DataRow row in table.Rows)
            {
                currentOrderNo = clsLibrary.dBReadString(row["order_no"]);
                if (summary == null || currentOrderNo != summary.OrderNo)
                {
                    summary = new SummaryInfoInternalStatus
                    {
                        OrderNo = clsLibrary.dBReadString(row["order_no"]),
                        ProjectID = clsLibrary.dBReadString(row["project_id"]),
                        OrderDate = clsLibrary.dbReadDateAsStringFormat(row["order_date"], "MM/dd/yyyy"),
                        CustomerName = clsLibrary.dBReadString(row["Customer_Name"]),
                        OrderTitle = clsLibrary.dBReadString(row["Order_Title"]).Trim(),
                        EstimatedArrivalDate = clsLibrary.dbReadDateAsStringFormat(row["maxEstimatedArrivalDate_summary"], "MM/dd/yyyy"),
                        CustomerRequestDate = clsLibrary.dbReadDateAsStringFormat(row["cust_request_date"], "MM/dd/yyyy"),
                        QtyOpen = clsLibrary.dBReadInt(row["countlines"]),
                        Comment = clsLibrary.dBReadString(row["comment"]),
                        Lines = new List<LineInfoInternalStatus>(),
                        ProcessCodes = new List<ProcessCode>(),
                        PercentageSellAvailablePartialInvoicing = clsLibrary.dBReadDouble(row["EligiblePct"]) / 100,
                        SalespersonName = clsLibrary.dBReadString(row["Salesperson_Name"]),
                        SalespersonID = clsLibrary.dBReadString(row["Salesperson_Id"]),
                        RequestedCRD = clsLibrary.dBReadString(row["RequestedCRD"]),
                        Accountability = clsLibrary.dBReadString(row["Accountability"]),
                        CompanyCode = clsLibrary.dBReadString(row["company_code"]),
                        OWP_PO = clsLibrary.dBReadString(row["owp_po"]),
                        Vendors = new List<Vendor>()
                    };

                    using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
                    {
                        db.Open();
                        summary.EmailLogs = db.Query<EmailLog>("dbo.[Get_ActivityLog]", new { Order = currentOrderNo, Company_Code = summary.CompanyCode }, commandType: CommandType.StoredProcedure).ToList();
                        summary.EstArrDates = db.Query<EstArrDate>("dbo.[Get_SchedArrivalDates]", new { Order_No = currentOrderNo, Company_Code = summary.CompanyCode }, commandType: CommandType.StoredProcedure).ToList();
                    }
                    output.Add(summary);
                }
                estArrDate = clsLibrary.dbReadDateAsStringFormat(row["maxestimatedarrivaldate"], "MM/dd/yyyy");
                tempVend = clsLibrary.dBReadString(row["vnd_no"]).Trim();
                tempOrder = clsLibrary.dBReadString(row["order_no"]).Trim();
                tempSuffix = clsLibrary.dBReadString(row["owp_po"]).Trim();
                tempInvVnd = clsLibrary.dBReadString(row["qty_cost_verified"]);
                addVendor = true;
                foreach (var item in summary.Vendors)
                {
                    if ((item.VendorNo == tempVend && item.Order == tempOrder)) 
                    {
                        addVendor = false;                        
                    }
                }

                if (addVendor && tempInvVnd == "0.00" && Array.IndexOf(exclVend, tempVend) == -1 && (estArrDate == "" || DateTime.Parse(estArrDate) <= DateTime.Now))
                {
                    summary.Vendors.Add(new Vendor { VendorNo = tempVend, VendorEmail = clsLibrary.dBReadString(row["VendorEmail"]).Split(","), Order = tempOrder, Suffix = tempSuffix });
                }

                summary.Vendors = summary.Vendors.OrderBy(x => x.VendorNo).ToList();

                summary.TotalOpenCost += clsLibrary.dBReadDouble(row["open_cost"]);
                summary.TotalOpenSell += clsLibrary.dBReadDouble(row["open_sell"]);
                summary.SellEligibleforPartialInvoicing += clsLibrary.dBReadDouble(row["SellEligibleforPartialInvoicing"]);
                summary.Lines.Add(new LineInfoInternalStatus
                {
                    OrderNo = clsLibrary.dBReadString(row["order_no"]),
                    LineNo = clsLibrary.dBReadInt(row["line_no"]),
                    VendorNo = clsLibrary.dBReadString(row["vnd_no"]),
                    VendorEmail = clsLibrary.dBReadString(row["VendorEmail"]).Split(","),
                    Ordered = clsLibrary.dBReadString(row["qty_ordered"]),
                    Received = clsLibrary.dBReadString(row["qty_received"]),
                    ReceivedColor = clsLibrary.dBReadString(row["qty_received_color"]),
                    Ticketed = clsLibrary.dBReadString(row["qty_Ticketed"]),
                    TicketedColor = clsLibrary.dBReadString(row["qty_ticketed_color"]),
                    Delivered = clsLibrary.dBReadString(row["qty_delivered"]),
                    DeliveredColor = clsLibrary.dBReadString(row["qty_delivered_color"]),
                    Invoiced = clsLibrary.dBReadString(row["qty_invoiced"]),
                    InvoicedColor = clsLibrary.dBReadString(row["colorForPartialInvoicing"]),
                    VendorInv = clsLibrary.dBReadString(row["qty_cost_verified"]),
                    VendorInvColor = clsLibrary.dBReadString(row["qty_cost_verified_color"]),
                    OpenSell = clsLibrary.dBReadDouble(row["open_sell"]).ToString("C"),
                    OpenSellWithoutFormat = clsLibrary.dBReadDouble(row["open_sell"]),
                    OpenSellColor = clsLibrary.dBReadString(row["open_sell_color"]),
                    OpenCost = clsLibrary.dBReadDouble(row["open_cost"]).ToString("C"),
                    OpenCostColor = clsLibrary.dBReadString(row["open_cost_color"]),
                    ACK = clsLibrary.dBReadString(row["ack_number"]),
                    OWP_PO = clsLibrary.dBReadString(row["owp_po"]),
                    ScheduleDate = clsLibrary.dbReadDateAsStringFormat(row["scheduleddate"], "MM/dd/yyyy"),
                    ProcessingCode = clsLibrary.dBReadString(row["processing_code"]),
                    SalesCode = clsLibrary.dBReadString(row["Sales_Code"]),
                    EstimatedArrivalDate = clsLibrary.dbReadDateAsStringFormat(row["maxestimatedarrivaldate"], "MM/dd/yyyy"),
                    CustomerRequestDate = clsLibrary.dbReadDateAsStringFormat(row["cust_request_date"], "MM/dd/yyyy"),
                    Catalog = clsLibrary.dBReadString(row["CatalogNo"]),
                    Description = clsLibrary.dBReadString(row["Description"]),
                    ShowLink = clsLibrary.dBReadBoolean(row["ShowLink"]),
                    SellEligibleforPartialInvoicing = clsLibrary.dBReadDouble(row["SellEligibleforPartialInvoicing"]).ToString("C"),
                    CostVerifiedReadyFlag = clsLibrary.dBReadInt(row["CostVerifiedReadyFlag"]),
                    TaxAmount = clsLibrary.dBReadDouble(row["TaxAmount"]),
                    TaxPercentage = clsLibrary.dBReadDouble(row["taxpct"]),
					ScheduledDateColor = clsLibrary.dBReadString(row["scheduleddate_color"]),
                    WarehouseNo = clsLibrary.dBReadString(row["warehouse_no"]).Trim(),
                    HasHardSched = clsLibrary.dBReadString(row["hashardsched"]),
                    Notes = clsLibrary.dBReadString(row["Notes"]),
                    Vendor_Name = clsLibrary.dBReadString(row["Vendor_Name"]),
                    Cost_Verified_Date = clsLibrary.dbReadDateAsStringFormat(row["Cost_Verified_Date"], "MM/dd/yyyy")
                });
            }

            foreach (DataRow row in table.DataSet.Tables[1].Rows)
            {
                summary.ProcessCodes.Add(new ProcessCode
                {
                    ProcessCodeGroup = clsLibrary.dBReadString(row["ProcessCodeName"]),
                    PctEligibleForPartialInvoice = clsLibrary.dBReadDouble(row["PCPercentage"]) / 100
                });
            }           

            return output;
        }

        public List<SelectValues> LoadSalesPerson(string regionISR)
        {
            try
            {

                using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
                {
                    db.Open();
                    var r = db.Query<SalespersonsModel>("dbo.[Get_InternalStatusReport_Salesperson]", new { Region = regionISR }, commandType: CommandType.StoredProcedure);

                    if (r.Count() > 0)
                    {
                        var output = new List<SelectValues>();

                        foreach (var row in r)
                        {
                            output.Add(new SelectValues
                            {
                                ID = row.salesperson_id,
                                Label = row.salesperson_info
                            });
                        }

                        return output.OrderBy(x => x.Label).ToList();
                    }
                    return null;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public List<VendorMiscCharge> GetVendorMiscCharges(string orderno, string salesperson, string cutoffdate, IEnumerable<string> selectedcustomers, string regionISR, string projfolder)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    var custIdsDT = new System.Data.DataTable();
                    custIdsDT.Columns.Add("ID", typeof(string));

                    if (selectedcustomers != null && selectedcustomers.Any())
                    {
                        foreach (var id in selectedcustomers)
                        {
                            var row = custIdsDT.NewRow();
                            row["ID"] = id;
                            custIdsDT.Rows.Add(row);
                        }
                    }

                    SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_InternalStatusReport_OrderMiscCharges]");//TODO: temp sp
                    cmd1.CommandType = CommandType.StoredProcedure;

                    string param1 = "@SALESPERSON_ID";
                    string param2 = "@CutoffDate";
                    string param3 = "@Order_No";
                    string param4 = "@CUSTOMERNOTABLE";
                    string param5 = "@Region";
                    string param6 = "@Project_ID";

                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), salesperson);
                    cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), cutoffdate);
                    cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), orderno);
                    cmd1.Parameters.AddWithValue("@" + param4.Replace("@", ""), custIdsDT);
                    cmd1.Parameters.AddWithValue("@" + param5.Replace("@", ""), regionISR);
                    cmd1.Parameters.AddWithValue("@" + param6.Replace("@", ""), projfolder);
                    cmd1.Parameters["@" + param4.Replace("@", "")].SqlDbType = SqlDbType.Structured;

                    DataTable dt1 = db.SqlCommandToTable(cmd1);
                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return new List<VendorMiscCharge>();
                    }

                    return FillVendorMiscCharges(dt1);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<VendorMiscCharge> FillVendorMiscCharges(DataTable table)
        {
            List<VendorMiscCharge> output = new List<VendorMiscCharge>();
            foreach (DataRow row in table.Rows)
            {
                output.Add(new VendorMiscCharge
                {
                    VendorNo = clsLibrary.dBReadString(row["VendorNo"]),
                    BOPASell = clsLibrary.dBReadDouble(row["BOPASell"]),
                    BOPACost = clsLibrary.dBReadDouble(row["BOPACost"]),
                    MiscSell = clsLibrary.dBReadDouble(row["MiscSell"]),
                    MiscCost = clsLibrary.dBReadDouble(row["MiscCost"]),
                    GrossProfit = clsLibrary.dBReadDouble(row["GrossProfit"])
                });
            }

            return output;
        }        

        public List<SelectValues> LoadCustomerList(List<string> salesperson, string regionISR)
        {
            using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                db.Open();

                var custIdsDT = new System.Data.DataTable();
                custIdsDT.Columns.Add("ID", typeof(string));

                if (salesperson != null && salesperson.Any())
                {
                    foreach (var id in salesperson)
                    {
                        var row = custIdsDT.NewRow();
                        row["ID"] = id;
                        custIdsDT.Rows.Add(row);
                    }
                }

                var r = db.Query<CustomersModel>("dbo.[Get_Customer_List_ForTesting]", new { salespersonid_list = custIdsDT, Region = regionISR }, commandType: CommandType.StoredProcedure);

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

        public List<SelectValues> LoadMultiCustomerList(IEnumerable<string> salespersons, string regionISR)
        {
            using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                db.Open();

                var salesperson = new System.Data.DataTable();
                salesperson.Columns.Add("ID", typeof(string));

                if (salespersons != null && salespersons.Count() > 0)
                {
                    foreach (var id in salespersons)
                    {
                        var row = salesperson.NewRow();
                        row["ID"] = id;
                        salesperson.Rows.Add(row);
                    }
                }

                var r = db.Query<CustomersModel>("dbo.[Get_Customer_List_ForTesting]", 
                    param: new
                    {                    
                        SALESPERSONID_LIST = salesperson.AsTableValuedParameter(@"dbo.VARCHARIDTABLETYPE"),
                        Region = regionISR
                    }, 
                    commandType: CommandType.StoredProcedure);

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

		public void InsertWrikeTask(string companyCode, string orderNo, double invoicingAmount, string yourEmail, string subject, string description, string createdBy)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					string query = @"EXEC dbo.Insert_WrikeTask_ISR '{0}','{1}','{2}','{3}','{4}','{5}','{6}'";
					SqlCommand sqlCommand = db.GetCommandSQL(String.Format(query,
						companyCode,
						Convert.ToInt32(orderNo.Trim()),
						invoicingAmount,
						yourEmail,
						subject,
						description,
						createdBy
						));

					db.SqlCommandToRow(sqlCommand);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public List<SelectValues> LoadCustomerListAll()
		{
			using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
			{
				db.Open();

				var r = db.Query<CustomersModel>("dbo.[Get_Customer_List]", new { SALESPERSONID = "_ALL" }, commandType: CommandType.StoredProcedure);

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

		public List<ClosedOrders> GetClosedOrderSummary(string dateFrom, string dateTo, string orderNo, string projectId, List<string> customers, List<string> salesperson, string region)
		{
			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					var custIdsDT = new System.Data.DataTable();
					custIdsDT.Columns.Add("ID", typeof(string));

					if (customers != null && customers.Any())
					{
						foreach (var id in customers)
						{
							var row = custIdsDT.NewRow();
							row["ID"] = id;
							custIdsDT.Rows.Add(row);
						}
					};

					var salespDT = new System.Data.DataTable();
					salespDT.Columns.Add("ID", typeof(string));

					if (salesperson != null && salesperson.Any())
					{
						foreach (var id in salesperson)
						{
							var row = salespDT.NewRow();
							row["ID"] = id;
							salespDT.Rows.Add(row);
						}
					};

					SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_InternalStatusReport_ClosedOrders]");
					cmd1.CommandType = CommandType.StoredProcedure;

					string param1 = "@SalespersonList";
					string param2 = "@CustomerList";
					string param3 = "@DateFrom";
					string param4 = "@DateTo";
					string param5 = "@Order_No";
					string param6 = "@Project_Id";
                    string param7 = "@Region";

                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), salespDT);
					cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), custIdsDT);
					cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), dateFrom);
					cmd1.Parameters.AddWithValue("@" + param4.Replace("@", ""), dateTo);
					cmd1.Parameters.AddWithValue("@" + param5.Replace("@", ""), orderNo);
					cmd1.Parameters.AddWithValue("@" + param6.Replace("@", ""), projectId);
                    cmd1.Parameters.AddWithValue("@" + param7.Replace("@", ""), region);
                    cmd1.Parameters["@" + param1.Replace("@", "")].SqlDbType = SqlDbType.Structured;
					cmd1.Parameters["@" + param2.Replace("@", "")].SqlDbType = SqlDbType.Structured;

					DataTable dt1 = db.SqlCommandToTable(cmd1);
					if (dt1 == null || dt1.Rows.Count == 0)
					{
						return new List<ClosedOrders>();
					}

					return FillClosedOrderSummary(dt1);
				}

			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

        public void InsertAccountabilityIntoActivityLog(int orderNo, string accountability, string companyCode)
		{
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    string query = @"EXEC dbo.Insert_ActivityLog_ISR_Accountability '{0}','{1}','{2}'";
                    SqlCommand sqlCommand = db.GetCommandSQL(String.Format(query, orderNo, accountability, companyCode));

                    db.SqlCommandToRow(sqlCommand);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public EmailAddressesList GetEmailsByEmailType(string emailType)
		{
            EmailAddressesList output = null;
            using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
            {
                SqlCommand sqlCommand = db.GetCommandSQL(@"dbo.Get_InternalStatusReport_Email");
                sqlCommand.CommandType = CommandType.StoredProcedure;
                string param1 = "@Email_Type";
                sqlCommand.Parameters.AddWithValue("@" + param1.Replace("@", ""), emailType);

                DataTable table = db.SqlCommandToTable(sqlCommand);

                output = new EmailAddressesList()
                {
                    Standard = table.Rows[0]["Email"].ToString(),
                    WrikeCC = table.Rows[0]["Email_CCW"].ToString(),
                    BCC = table.Rows[0]["Email_BBC"].ToString()
                };
            }

            return output;
        }

        public List<EmailAddressesList> GetEmailsByEmailTypeList(string emailType)
        {
            List<EmailAddressesList> output = new List<EmailAddressesList>();

            using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
            {
                SqlCommand sqlCommand = db.GetCommandSQL(@"dbo.Get_InternalStatusReport_Email");
                sqlCommand.CommandType = CommandType.StoredProcedure;
                string param1 = "@Email_Type";
                sqlCommand.Parameters.AddWithValue("@" + param1.Replace("@", ""), emailType);

                DataTable table = db.SqlCommandToTable(sqlCommand);
                
                
                for (var i=0; i<table.Rows.Count;i++)
                {
                    EmailAddressesList email = new EmailAddressesList()
                    {
                        Standard = table.Rows[i]["Email"].ToString(),
                        WrikeCC = table.Rows[i]["Email_CCW"].ToString(),
                        BCC = table.Rows[i]["Email_BBC"].ToString()
                    };

                    output.Add(email);
                }
            }

            return output;
        }

        private List<ClosedOrders> FillClosedOrderSummary(DataTable table)
		{
			List<ClosedOrders> output = new List<ClosedOrders>();
			ClosedOrders summary = null;
			string currentOrderNo = "";

			foreach (DataRow row in table.Rows)
			{
				currentOrderNo = clsLibrary.dBReadString(row["order_no"]);
				summary = new ClosedOrders
				{
					OrderIndex = clsLibrary.dBReadString(row["order_index"]),
					OrderNo = clsLibrary.dBReadString(row["order_no"]),
					ProjectID = clsLibrary.dBReadString(row["project_id"]),
					OrderTitle = clsLibrary.dBReadString(row["order_title"]),
					Customer = clsLibrary.dBReadString(row["customer"]).Trim(),
					DateEntered = clsLibrary.dbReadDateAsStringFormat(row["date_entered"], "MM/dd/yyyy"),
					Comments = clsLibrary.dBReadString(row["comments"]),
					TotalSell = clsLibrary.dBReadDouble(row["total_sell"]),
					TotalCost = clsLibrary.dBReadDouble(row["total_cost"]),
					MaxEstimatedArrivalDate = clsLibrary.dbReadDateAsStringFormat(row["MaxEstimatedArrivalDate"], "MM/dd/yyyy"),
					CustRequestDate = clsLibrary.dbReadDateAsStringFormat(row["Cust_Request_Date"], "MM/dd/yyyy")
				};

				using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
				{
					db.Open();
					summary.EmailLogs = db.Query<EmailLog>("dbo.[Get_ActivityLog]", new { Order = currentOrderNo }, commandType: CommandType.StoredProcedure).ToList();
				}

				output.Add(summary);
			}

			//summary.TotalOpenCost += clsLibrary.dBReadDouble(row["open_cost"]);
			//summary.TotalOpenSell += clsLibrary.dBReadDouble(row["open_sell"]);
			//summary.SellEligibleforPartialInvoicing += clsLibrary.dBReadDouble(row["SellEligibleforPartialInvoicing"]);

			return output;
		}

		private IEnumerable<T> Request_Data<T>(string StoredProc, string salesperson, string cutoffdate, string ordernoISR, IEnumerable<string> isrSelectedCustomers) where T : class
        {
            using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                db.Open();

                var custIdsDT = new System.Data.DataTable();
                custIdsDT.Columns.Add("ID", typeof(string));

                if (isrSelectedCustomers != null && isrSelectedCustomers.Count() > 0)
                {
                    foreach (var id in isrSelectedCustomers)
                    {
                        var row = custIdsDT.NewRow();
                        row["ID"] = id;
                        custIdsDT.Rows.Add(row);
                    }
                }

                var r = db.Query<T>(StoredProc,
                               param: new
                               {
                                   SALESPERSON_ID = salesperson,
                                   CutoffDate = cutoffdate,
                                   Order_No = ordernoISR,
                                   CUSTOMERNOTABLE = custIdsDT.AsTableValuedParameter(@"dbo.VARCHARIDTABLETYPE")
                               },
                               commandType: System.Data.CommandType.StoredProcedure);

                return r;
            }
        }

        public List<EmailISR> GetISREmailAllTypes()
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
                {
                    connection.Open();
                    var result = connection.Query<EmailISR>("dbo.[Get_InternalStatusReport_Email]", commandType: CommandType.StoredProcedure).ToList();
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("dbo.[Get_InternalStatusReport_EmailTypes]");
                throw ex;
            }
        }
    }
}
