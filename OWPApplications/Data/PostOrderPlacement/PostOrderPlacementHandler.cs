using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OWPApplications.Models;
using OWPApplications.Models.PostOrderPlacement;
using OWPApplications.Utils;

namespace OWPApplications.Data.PostOrderPlacement
{
    public class PostOrderPlacementHandler
    {
        private IConfiguration _configuration;

        public PostOrderPlacementHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

		public HeaderInfoPostOrder LoadHeaderInfo(int? order, string projectid, string region)
		{
			try
			{

				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
                    SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_PostOrderPlacement_HeaderInfo]");
                    cmd1.CommandType = CommandType.StoredProcedure;

                    string param1 = "@Order_No";
                    string param2 = "@Project_ID";
                    string param3 = "@Region";

                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), order == 0 ? null : order);
                    cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), projectid);
                    cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), region);

                    DataTable dt1 = db.SqlCommandToTable(cmd1);

                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return null;
                    }

                    SqlCommand cmd2 = db.GetCommandSQL(@"[dbo].[Get_PostOrderPlacement_Instructions]");
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), order == 0 ? null : order);
                    cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), projectid);
                    cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), region);

                    DataTable dt2 = db.SqlCommandToTable(cmd2);

                    SqlCommand cmd3 = db.GetCommandSQL(@"[dbo].[Get_PostOrderPlacement_VendorInfo]");
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), order == 0 ? null : order);
                    cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), projectid);
                    cmd1.Parameters.AddWithValue("@" + param3.Replace("@", ""), region);

                    DataTable dt3 = db.SqlCommandToTable(cmd3);

					if (dt1 == null || dt1.Rows.Count == 0)
					{
						return null;
					}
					return new HeaderInfoPostOrder(dt1.Rows[0], dt2.Rows.Count > 0 ? dt2.Rows[0] : null, dt3);


				}

			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<SummaryPostOrder> GetSummaryInfo(int? orderno, string projectid)
		{

			try
			{
				using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
				{
					SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_PostOrderPlacement_Lines]");
					cmd1.CommandType = CommandType.StoredProcedure;

					string param1 = "@OrderNo";
					string param2 = "@Project_ID";

					cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), orderno);
					cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), projectid);

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

		private IEnumerable<SummaryPostOrder> FillSummaryInfo(DataTable table)
        {
            List<SummaryPostOrder> output = new List<SummaryPostOrder>();
            string currentVendor = "";
            string currentPo = "";
			string currentOrderNo = ""; 
            SummaryPostOrder summary = null;
            foreach (DataRow row in table.Rows)
            {
                currentVendor = clsLibrary.dBReadString(row["Vendor #"]);
                currentPo = clsLibrary.dBReadString(row["PO Suffix #"]);
				currentOrderNo = clsLibrary.dBReadString(row["order_no"]);
				if (summary == null || (currentVendor != summary.VendorNo || currentPo != summary.PoSuffix || currentOrderNo != summary.OrderNo))
                {
                    if (summary != null)
                    {
                        summary.LastReceivedDate = GetLastReceivedDate(summary);
                        summary.EstimatedArrivalColor = GetEstimatedArrivalColor(summary);
                    }
                    summary = new SummaryPostOrder
                    {
                        OrderNo = clsLibrary.dBReadString(row["order_no"]),
                        VendorNo = clsLibrary.dBReadString(row["Vendor #"]),
                        VendorName = clsLibrary.dBReadString(row["vendor_name"]),
                        PoSuffix = clsLibrary.dBReadString(row["PO Suffix #"]),
                        OrderedDate = clsLibrary.dbReadDateAsStringFormat(row["Ordered Date"], "MM/dd/yyyy"),
                        AckNo = clsLibrary.dBReadString(row["Ack #"]),
                        AckDate = clsLibrary.dbReadDateAsStringFormat(row["Ack Date"], "MM/dd/yyyy"),
                        ShipDate = clsLibrary.dbReadDateAsStringFormat(row["Ship Date"], "MM/dd/yyyy"),
                        EstimatedArrival = clsLibrary.dbReadDateAsStringFormat(row["Estimated Arrival"], "MM/dd/yyyy"),
                        RequestedArrival = clsLibrary.dbReadDateAsStringFormat(row["OrderRequestedArrivalDate"], "MM/dd/yyyy"),
                        PONotReceived = clsLibrary.dBReadBoolean(row["PONotReceivedLineColor"]),
                        ACKNotReceived = clsLibrary.dBReadBoolean(row["NoAckRedColor"]),
                        Comments = clsLibrary.dBReadString(row["Comment"]),
                        Carrier = clsLibrary.dBReadString(row["Carrier"]),
                        Tracking = clsLibrary.dBReadString(row["Tracking"]),
                        TrackingLink = clsLibrary.dBReadString(row["TrackingLink"]),
                        ScheduledArrivalDate = clsLibrary.dBReadString(row["Sched_Arrival_Dt"]),
                        Lines = new List<LineInfoPostOrder>()
                    };
                    output.Add(summary);
                }
                summary.Lines.Add(new LineInfoPostOrder
                {
                    LineNo = clsLibrary.dBReadInt(row["Line #"]),
                    ProcessCode = clsLibrary.dBReadString(row["Process Code"]),
                    LineSalesCode = clsLibrary.dBReadString(row["Line Sales Code"]),
                    Werehouse_Network = clsLibrary.dBReadString(row["Warehouse/Network Installer"]),
                    Catalog = clsLibrary.dBReadString(row["Catalog #"]),
                    OrderedDate = clsLibrary.dbReadDateAsStringFormat(row["Ordered Date"], "MM/dd/yyyy"),
                    Description = clsLibrary.dBReadString(row["description"]),
                    ReceivedDate = clsLibrary.dbReadDateAsStringFormat(row["Received Date"], "MM/dd/yyyy"),
                    RequestedArrival = clsLibrary.dbReadDateAsStringFormat(row["LineRequestedArrivalDate"], "MM/dd/yyyy"),
                    Ordered = clsLibrary.dBReadString(row["qty_ordered"]),
                    Received = clsLibrary.dBReadString(row["qty_received"]),
                    EstimatedArrivalDate = clsLibrary.dBReadString(row["Estimated Arrival"]),
                    ShipDate = clsLibrary.dBReadString(row["Ship Date"]),
                    GeneralTagging = clsLibrary.dBReadString(row["General Tagging"]),
                    ScheduledArrivalDate = clsLibrary.dBReadString(row["LatestSchDate_Line"])
                });
            }

            if (output.Count == 1)
            {
                if (summary != null)
                {
                    summary.LastReceivedDate = GetLastReceivedDate(summary);
                }
            }

            return output;


        }

        public IEnumerable<LineDetailPostOrder> GetLineDetailInfo(int? orderno, string projectid)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_PostOrderPlacement_Lines]");
                    cmd1.CommandType = CommandType.StoredProcedure;

                    string param1 = "@OrderNo";
                    string param2 = "@Project_ID";

                    cmd1.Parameters.AddWithValue("@" + param1.Replace("@", ""), orderno);
                    cmd1.Parameters.AddWithValue("@" + param2.Replace("@", ""), projectid);

                    DataTable dt1 = db.SqlCommandToTable(cmd1);
                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return null;
                    }

                    return FillLineDetailInfo(dt1);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IEnumerable<LineDetailPostOrder> FillLineDetailInfo(DataTable table)
        {
            List<LineDetailPostOrder> output = new List<LineDetailPostOrder>();

			foreach (DataRow row in table.Rows)
            {
                string orderNo = clsLibrary.dBReadString(row["order_no"]);
                string poSuffix = clsLibrary.dBReadString(row["PO Suffix #"]);

				LineDetailPostOrder lineDetail = new LineDetailPostOrder
				{
					POReference = string.Format("W{0}-{1}", orderNo, poSuffix),
					LineNo = clsLibrary.dBReadInt(row["Line #"]),
					ProcessingCode = clsLibrary.dBReadString(row["Process Code"]),
					Order_POSuffix = orderNo + "/" + poSuffix,
					VendorID = clsLibrary.dBReadString(row["Vendor #"]),
					VendorName = clsLibrary.dBReadString(row["vendor_name"]),
					CatalogNo = clsLibrary.dBReadString(row["Catalog #"]),
					Description = clsLibrary.dBReadString(row["description"]),
					QtyOrdered = clsLibrary.dBReadString(row["qty_ordered"]),
					GeneralTagging = clsLibrary.dBReadString(row["General Tagging"]),
					OrderedDate = clsLibrary.dBReadDate(row["Ordered Date"]),
					RequestedArrivalDate = clsLibrary.dBReadDate(row["OrderRequestedArrivalDate"]),
					AckNo = clsLibrary.dBReadString(row["Ack #"]),
					ShipDate = clsLibrary.dBReadDate(row["Ship Date"]),
					EstimatedArrivalDate = clsLibrary.dBReadDate(row["Estimated Arrival"]),
					QtyReceived = clsLibrary.dBReadString(row["qty_received"]),
					LatestReceivedDate = clsLibrary.dBReadDate(row["Received Date"]),
					Carrier = clsLibrary.dBReadString(row["Carrier_Detail"]),
					TrackingNo = clsLibrary.dBReadString(row["Tracking_Detail"]),
					FreeformNotes = clsLibrary.dBReadString(row["Notes_Detail"]),
                    OrderNo = orderNo,
                    PoSuffix = poSuffix,
                    Salesperson = clsLibrary.dBReadString(row["Salesperson"]),
                    QtyDelivered = clsLibrary.dBReadString(row["qty_delivered"]),
                    QtyTicketed = clsLibrary.dBReadString(row["qty_ticketed"]),
                    LastSchedDate = clsLibrary.dBReadDate(row["LatestSchDate_Line"]),
                    QtyCostVerified = clsLibrary.dBReadString(row["qty_cost_verified"]),
                    LastScheduleDateColor = clsLibrary.dBReadString(row["LatestSchDate_Line_Color"])
                };

				output.Add(lineDetail);
            }

            return output;
        }        

        private string GetLastReceivedDate(SummaryPostOrder sum)
        {
            string greater = "";
            foreach (var line in sum.Lines)
            {
                if (greater.CompareTo(line.ReceivedDate) < 0) greater = line.ReceivedDate;
            }

            return greater;
        }
        private string GetEstimatedArrivalColor(SummaryPostOrder sum)
        {
            foreach (var line in sum.Lines)
            {
                if (string.IsNullOrEmpty(line.RequestedArrival)) return "bg-red-ft";
            }

            return "";
        }

        public List<SelectValues> GetUsedLocations()
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(@"[dbo].[Get_UsedLocations]");
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

        public IEnumerable<SummaryPostOrder> GetLinesInfo(int? orderno, string projectid, List<int> locations, string region)
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

                    SqlCommand cmd1 = db.GetCommandSQL("dbo.Get_PostOrderPlacement_Lines");
                    cmd1.CommandType = CommandType.StoredProcedure;
                    if (orderno == null)
                    {
                        cmd1.Parameters.AddWithValue("@OrderNo", System.DBNull.Value);
                    }
                    else
                    {
                        cmd1.Parameters.AddWithValue("@OrderNo", orderno);
                    }
                    cmd1.Parameters.AddWithValue("@Project_ID", projectid);
                    cmd1.Parameters.AddWithValue("@Locations", dtLocations);
                    cmd1.Parameters["@Locations"].SqlDbType = SqlDbType.Structured;
                    cmd1.Parameters.AddWithValue("@Region", region);

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

        public VendorEmail GetVendorByNo(string vendorNo)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL("dbo.Get_Vendor_Email");
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.AddWithValue("@App", "POP");
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
                Addresses = new List<string>()
            };

            var emailList = new List<string>();
            List<EmailVendor> emailFields = xmlConfig.GetEmailVendor("pop");
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

        private IEnumerable<SummaryPostOrder> FillLineInfo(DataTable table)
        {
            List<SummaryPostOrder> output = new List<SummaryPostOrder>();
            SummaryPostOrder summary = null;

            foreach (DataRow row in table.Rows)
            {
                string currentVendor = clsLibrary.dBReadString(row["Vendor #"]);
                string currentPo = clsLibrary.dBReadString(row["PO Suffix #"]);
                string currentOrderNo = clsLibrary.dBReadString(row["order_no"]);
                if (summary == null || (currentVendor != summary.VendorNo || currentPo != summary.PoSuffix || currentOrderNo != summary.OrderNo))
                {
                    if (summary != null)
                    {
                        summary.LastReceivedDate = GetLastReceivedDate(summary);
                    }
                    summary = new SummaryPostOrder
                    {
                        OrderNo = clsLibrary.dBReadString(row["order_no"]),
                        VendorNo = clsLibrary.dBReadString(row["Vendor #"]),
                        PoSuffix = clsLibrary.dBReadString(row["PO Suffix #"]),
                        OrderedDate = clsLibrary.dbReadDateAsStringFormat(row["Ordered Date"], "MM/dd/yyyy"),
                        AckNo = clsLibrary.dBReadString(row["Ack #"]),
                        AckDate = clsLibrary.dbReadDateAsStringFormat(row["Ack Date"], "MM/dd/yyyy"),
                        ShipDate = clsLibrary.dbReadDateAsStringFormat(row["Ship Date"], "MM/dd/yyyy"),
                        EstimatedArrival = clsLibrary.dbReadDateAsStringFormat(row["Estimated Arrival"], "MM/dd/yyyy"),
                        RequestedArrival = clsLibrary.dbReadDateAsStringFormat(row["OrderRequestedArrivalDate"], "MM/dd/yyyy"),
                        ACKNotReceived = clsLibrary.dBReadBoolean(row["NoAckRedColor"]),
                        Comments = clsLibrary.dBReadString(row["Comment"]),
                        Carrier = clsLibrary.dBReadString(row["Carrier"]),
                        Tracking = clsLibrary.dBReadString(row["Tracking"]),
                        ScheduledArrivalDate = clsLibrary.dBReadString(row["Sched_Arrival_Dt"]),
                        IsAvailableVendor = CheckAvailableVendor(clsLibrary.dBReadString(row["Vendor #"])),
                        IsSteelcaseVendor = clsLibrary.dBReadBoolean(row["Steelcase_Vendor"]),
                        VendorName = clsLibrary.dBReadString(row["vendor_name"]),
                        LatestSoftSchDt = clsLibrary.dbReadDateAsStringFormat(row["LatestSoftSchDt"], "MM/dd/yyyy"),
                        Lines = new List<LineInfoPostOrder>()
                    };
                    output.Add(summary);
                }
                summary.Lines.Add(new LineInfoPostOrder
                {
                    LineNo = clsLibrary.dBReadInt(row["Line #"]),
                    ProcessCode = clsLibrary.dBReadString(row["Process Code"]),
                    LineSalesCode = clsLibrary.dBReadString(row["Line Sales Code"]),
                    Werehouse_Network = clsLibrary.dBReadString(row["Warehouse/Network Installer"]),
                    Catalog = clsLibrary.dBReadString(row["Catalog #"]),
                    Description = clsLibrary.dBReadString(row["description"]),
                    ReceivedDate = clsLibrary.dbReadDateAsStringFormat(row["Received Date"], "MM/dd/yyyy"),
                    RequestedArrival = clsLibrary.dbReadDateAsStringFormat(row["LineRequestedArrivalDate"], "MM/dd/yyyy"),
                    Ordered = clsLibrary.dBReadString(row["qty_ordered"]),
                    Received = clsLibrary.dBReadString(row["qty_received"]),
                    EstimatedArrivalDate = clsLibrary.dBReadString(row["Estimated Arrival"]),
                    ShipDate = clsLibrary.dBReadString(row["Ship Date"]),
                    GeneralTagging = clsLibrary.dBReadString(row["General Tagging"])
                });
            }

            if (output.Count == 1)
            {
                if (summary != null)
                {
                    summary.LastReceivedDate = GetLastReceivedDate(summary);
                }
            }
            //if (output != null)
            //{
            //    foreach (var item in output)
            //    {
            //        if (item.Lines != null)
            //        {
            //            item.Lines = item.Lines.OrderBy(x => x.LineNo).ToList();
            //        }
            //    }
            //}

            return output;
        }

        private bool CheckAvailableVendor(string vendorNo)
        {
            var excludedVendors = xmlConfig.GetExcludedVendors();
            return !excludedVendors.Contains(vendorNo.Trim());
        }
    }
}
