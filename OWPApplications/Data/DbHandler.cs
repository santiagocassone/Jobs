using Microsoft.Extensions.Configuration;
using OWPApplications.Data.FastTrack;
using OWPApplications.Data.InternalStatusReport;
using OWPApplications.Data.POPDashboard;
using OWPApplications.Data.PostOrderPlacement;
using OWPApplications.Data.QIAuditTool;
using OWPApplications.Data.QuoteInquiry;
using OWPApplications.Data.StandardPrice;
using OWPApplications.Data.WarehouseDT;
using OWPApplications.Data.VendorDepositRequest;
using OWPApplications.Models;
using OWPApplications.Models.PostOrderPlacement;
using OWPApplications.Models.QuoteInquiry;
using OWPApplications.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using OWPApplications.Data.DefaultPricingReport;
using OWPApplications.Data.ISRManagerView;
using OWPApplications.Data.StockInventory;
using OWPApplications.Data.LaborQuoteAutomation;
using OWPApplications.Data.SubcontractorReceiving;
using OWPApplications.Data.JobCosting;
using Microsoft.Extensions.Logging;
using Dapper;

namespace OWPApplications.Data
{
    public class DbHandler
    {
        IConfiguration _configuration;
        readonly public QuoteInquiryHandler QuoteInquiryHandler;
        readonly public PostOrderPlacementHandler PostOrderPlacementHandler;
        readonly public FastTrackHandler FastTrackHandler;
        readonly public StandardPriceHandler StandardPriceHandler;
        readonly public InternalStatusReportHandler InternalStatusReportHandler;
        readonly public POPDashboardHandler POPDashboardHandler;
        readonly public VendorDepositRequestHandler VendorDepositRequestHandler;
        readonly public StockInventoryHandler StockInventoryHandler;
        readonly public LaborQuoteAutomationHandler LaborQuoteAutomationHandler;
        readonly public WarehouseDTHandler WarehouseDTHandler;
        readonly public QIAuditToolHandler QIAuditToolHandler;
        readonly public ISRManagerViewHandler ISRManagerViewHandler;
        readonly public SubcontractorReceivingHandler SubcontractorReceivingHandler;
        readonly public DefaultPricingReportHandler DefaultPricingReportHandler;
        readonly public JobCostingHandler JobCostingHandler;

        public DbHandler(IConfiguration configuration, ILoggerFactory _logger)
        {
            _configuration = configuration;
            QuoteInquiryHandler = new QuoteInquiryHandler(_configuration);
            PostOrderPlacementHandler = new PostOrderPlacementHandler(_configuration);
            FastTrackHandler = new FastTrackHandler(_configuration);
            StandardPriceHandler = new StandardPriceHandler(_configuration);
            InternalStatusReportHandler = new InternalStatusReportHandler(_configuration, _logger);
            WarehouseDTHandler = new WarehouseDTHandler(_configuration);
            POPDashboardHandler = new POPDashboardHandler(_configuration);
            LaborQuoteAutomationHandler = new LaborQuoteAutomationHandler(_configuration, _logger);
            VendorDepositRequestHandler = new VendorDepositRequestHandler(_configuration);
            QIAuditToolHandler = new QIAuditToolHandler(_configuration);
            DefaultPricingReportHandler = new DefaultPricingReportHandler(_configuration);
            ISRManagerViewHandler = new ISRManagerViewHandler(_configuration);
            SubcontractorReceivingHandler = new SubcontractorReceivingHandler(_configuration);
            StockInventoryHandler = new StockInventoryHandler(_configuration);
            JobCostingHandler = new JobCostingHandler(_configuration);
        }

        public List<VendorEmail> GetVendorEmails(string app, string region, bool isLiveISR)
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {                   
                    SqlCommand cmd1 = db.GetCommandSQL(VendorQuery(app, region));

                    DataTable dt1 = db.SqlCommandToTable(cmd1);

                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return null;
                    }

                    return FillVendors(app, dt1, region, isLiveISR);
                }

            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<VendorEmail> FillVendors(DataTable table)
        {
            List<VendorEmail> vendors = new List<VendorEmail>();
            string currentVendor = "";
            VendorEmail vendor = null;
            foreach (DataRow row in table.Rows)
            {
                currentVendor = clsLibrary.dBReadString(row["Vnd_No"]).Trim();
                if (vendor == null || currentVendor != vendor.VendorNo)
                {
                    vendor = new VendorEmail
                    {
                        VendorNo = clsLibrary.dBReadString(row["Vnd_No"]).Trim(),
                        Name = clsLibrary.dBReadString(row["Vendor_Name"]),
                        Addresses = new List<string>()
                    };

                    vendors.Add(vendor);
                }
                // Check if Vnd_Email -> Split ';' -> phone one.
                vendor.Addresses.Add(clsLibrary.dBReadString(row["Vnd_Email"]));               

            }
            return vendors;
        }

        private List<VendorEmail> FillVendorsV2(DataTable table, string region, bool isLiveISR)
        {
            List<VendorEmail> vendors = new List<VendorEmail>();
            string currentVendor = "";
            VendorEmail vendor = null;
            foreach (DataRow row in table.Rows)
            {
                currentVendor = clsLibrary.dBReadString(row["Vnd_No"]).Trim();
                if (vendor == null || currentVendor != vendor.VendorNo)
                {
                    vendor = new VendorEmail
                    {
                        VendorNo = clsLibrary.dBReadString(row["Vnd_No"]).Trim(),
                        Name = clsLibrary.dBReadString(row["Vendor_Name"]),
                        Phone = clsLibrary.dBReadString(row["Vnd_Phone"]),
                        Addresses = new List<string>()
                    };
                    
                    vendors.Add(vendor);
                }
                vendor.Addresses.AddRange(GetMultipleAddress(row, region, isLiveISR));

            }
            return vendors;
        }

        private List<string> GetMultipleAddress(DataRow row, string region, bool isLiveISR)
        {
            var list = new List<string>();
            var primaryEmails = clsLibrary.dBReadString(row["Vnd_Email"]).Split(';');
            string[] aditionalEmails = new string[] {};
            string[] liveIsrEmails = new string[] { };
            if (isLiveISR && clsLibrary.dBReadString(row["Vnd_LiveISR_Email"]) != "")
            {
                liveIsrEmails = clsLibrary.dBReadString(row["Vnd_LiveISR_Email"]).Split(';');
                if (liveIsrEmails.Length > 0 && !string.IsNullOrEmpty(liveIsrEmails[0])) list.AddRange(liveIsrEmails);
            } else
            {
                if (region == "OWP")
                {
                    aditionalEmails = clsLibrary.dBReadString(row["Vnd_Add_Email"]).Split(';');
                }
                else
                {
                    aditionalEmails = clsLibrary.dBReadString(row["Vnd_Amz_Email"]).Split(';');
                }

                if (primaryEmails.Length > 0 && !string.IsNullOrEmpty(primaryEmails[0])) list.AddRange(primaryEmails);
                if (aditionalEmails.Length > 0 && !string.IsNullOrEmpty(aditionalEmails[0])) list.AddRange(aditionalEmails);
            }           

            return list;
        }

        private List<VendorEmail> FillVendors(string app, DataTable table, string region, bool isLiveISR)
        {
            switch (app)
            {
                case "POP":
                    return FillVendorsV2(table, region, isLiveISR);
                case "QI":
                default:
                    return FillVendors(table);
            }
        }


        private string VendorQuery(string app, string region)
        {
            if (region == "OSQ")
            {
                return @"SELECT [Vnd_No]
                                ,[Vendor_Name]
                                ,[Vnd_Email]
                                ,[Vnd_Amz_Email]
                                ,[Vnd_LiveISR_Email]
                                ,[Vnd_Phone]
                            FROM [dbo].[vw_VendorEmailPOP_OSQ]
                            order by 1,3";
            } else
            {
                switch (app)
                {
                    case "POP":
                        return @"SELECT [Vnd_No]
                                ,[Vendor_Name]
                                ,[Vnd_Email]
                                ,[Vnd_Add_Email]
                                ,[Vnd_LiveISR_Email]
                                ,[Vnd_Phone]
                            FROM [dbo].[vw_VendorEmailPOP]
                            order by 1,3";
                    case "QI":
                    default:
                        return @"SELECT [Vnd_No]
                                ,[Vendor_Name]
                                ,[Vnd_Email]
                            FROM [dbo].[vw_VendorEmailQI]
                            order by 1,3";
                }
            }
            
        }
      

        public void SaveActivity(ActivityLog activity)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
                {
                    connection.Open();

                    var param = new DynamicParameters();
                    param.Add("@YourEmail", clsLibrary.getStringLeft(activity.YourEmail, 150));
                    param.Add("@ToEmail", clsLibrary.getStringLeft(activity.ToEmail, 150));
                    param.Add("@Vendor", clsLibrary.getStringLeft(!string.IsNullOrEmpty(activity.Vendor) ? activity.Vendor : "", 200));
                    param.Add("@Subject", clsLibrary.getStringLeft(activity.Subject, 500));
                    param.Add("@Body", activity.Body);
                    param.Add("@CreatedBy", clsLibrary.getStringLeft(activity.CreatedBy, 200));
                    param.Add("@Order", clsLibrary.getStringLeft(activity.Order, 100));
                    param.Add("@Line", clsLibrary.getStringLeft(!string.IsNullOrEmpty(activity.Line) ? activity.Line : "", 100));
                    param.Add("@Company_Code", activity.CompanyCode);
                    param.Add("@YourName", activity.Name);

                    connection.Execute("dbo.[Insert_ActivityLog]", param, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                clsLog log = new clsLog(_configuration);
                log.WriteError(activity.CreatedBy, "SaveActivity()", ex);
            }
        }

        public List<Announcement> GetAnnouncements()
        {
            var outList = new List<Announcement>();
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL("SELECT * FROM [dbo].[Announcement] WHERE Active=1 ORDER BY Date DESC");

                    DataTable dt1 = db.SqlCommandToTable(cmd1);

                    if (dt1 != null)
                    {
                        foreach (DataRow row in dt1.Rows)
                        {
                            outList.Add(new Announcement(row));
                        }
                    }

                    return outList;
                }

            }
            catch (Exception)
            {
                return outList;
            }

        }
        public bool UpdateValues(OrderValues data, ILogger _logger)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
                {
                    connection.Open();
                    var param = new DynamicParameters();
                    param.Add("@FieldID", data.FieldID);
                    param.Add("@Company_Code", data.Company);
                    param.Add("@Project_ID", data.ProjectID);
                    param.Add("@Order_No", data.OrderNo);
                    param.Add("@Order_Type", data.OrderType);
                    param.Add("@PO", data.PO);
                    param.Add("@Line_No", data.LineNo);
                    param.Add("@Line_Type", data.LineType);
                    param.Add("@KeyString", data.KeyString);
                    param.Add("@Value", data.Value);
                    param.Add("@AppName", data.Source);
                    param.Add("@UpsertLevel", data.UpsertLevel);

                    List<LookupGeneralWithValue> output = connection.Query<LookupGeneralWithValue>("dbo.[Upsert_OrderValuesTable]", param, commandType: CommandType.StoredProcedure).ToList();

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Upsert_OrderValuesTable {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}", data.FieldID, data.Company, data.ProjectID, data.OrderNo, data.OrderType, data.PO, data.LineNo, data.LineType, data.KeyString, data.Value, data.Source, data.UpsertLevel);
                throw ex;
            }
        }
    }

}