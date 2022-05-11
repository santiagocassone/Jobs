using Dapper;
using Microsoft.Extensions.Configuration;
using OWPApplications.Models;
using OWPApplications.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using static OWPApplications.Models.DefaultPricingReport.DefaultPricingReportViewModel;

namespace OWPApplications.Data.DefaultPricingReport
{
    public class DefaultPricingReportHandler
    {
        private IConfiguration _configuration;

        public DefaultPricingReportHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<SelectValues> LoadLocations()
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(DefaultPricingReportQueries.LOCATIONS);
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
                            Label = clsLibrary.dBReadString(row["Location_name"]).Trim()
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

        public List<SelectValues> LoadTimePeriods()
        {
            try
            {
                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(DefaultPricingReportQueries.TIMEPERIODS);
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
                            ID = clsLibrary.dBReadString(row["TPValue"]),
                            Label = clsLibrary.dBReadString(row["TPDescription"])
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

        public IEnumerable<SalespersonFlowModel> GetSalespersonFlow(string location, string dateFrom, string dateTo, bool excludeVendors)
        {
            return Request_SalespersonFlow<SalespersonFlowModel>("dbo.Get_DefaultPricing_SalespersonFlow", location, dateFrom, dateTo, null, null, null, excludeVendors);
        }

        public IEnumerable<VendorFlowModel> GetVendorsFlow(string location, string dateFrom, string dateTo, bool excludeVendors)
        {
            return Request_VendorFlow<VendorFlowModel>("dbo.Get_DefaultPricing_VendorFlow", location, dateFrom, dateTo, null, null, null, excludeVendors);
        }

        public IEnumerable<VendorFlowModel> GetVendorsFilterBySalesperson(string location, string dateFrom, string dateTo, string salespersonID, bool excludeVendors)
        {
            return Request_SalespersonFlow<VendorFlowModel>("dbo.Get_DefaultPricing_SalespersonFlow", location, dateFrom, dateTo, salespersonID, null, null, excludeVendors);
        }

            public IEnumerable<SalespersonFlowModel> GetSalespersonsFilterByVendor(string location, string dateFrom, string dateTo, string vendorNo, bool excludeVendors)
        {
            return Request_VendorFlow<SalespersonFlowModel>("dbo.Get_DefaultPricing_VendorFlow", location, dateFrom, dateTo, null, vendorNo, null, excludeVendors);
        }

        public IEnumerable<CustomerFlowModel> GetCustomerFilterBySalesperson(string location, string dateFrom, string dateTo, string vendorNo, string salespersonID, bool excludeVendors)
        {
            return Request_VendorFlow<CustomerFlowModel>("dbo.Get_DefaultPricing_VendorFlow", location, dateFrom, dateTo, salespersonID, vendorNo, null, excludeVendors);
        }

        public IEnumerable<CustomerFlowModel> GetCustomerFilterByVendor(string location, string dateFrom, string dateTo, string vendorNo, string salespersonID, bool excludeVendors)
        {
            return Request_VendorFlow<CustomerFlowModel>("dbo.Get_DefaultPricing_SalespersonFlow", location, dateFrom, dateTo, salespersonID, vendorNo, null, excludeVendors);
        }

        public IEnumerable<OrderFlowModel> GetOrdersFilterByCustomer(string location, string dateFrom, string dateTo, string customerNo, string vendorNo, string salespersonID, bool excludeVendors)
        {
            return Request_VendorFlow<OrderFlowModel>("dbo.Get_DefaultPricing_VendorFlow", location, dateFrom, dateTo, salespersonID, vendorNo, customerNo, excludeVendors);
        }

        public IEnumerable<OrderFlowModel> GetOrdersFilterByCustomer_(string location, string dateFrom, string dateTo, string customerNo, string vendorNo, string salespersonID, bool excludeVendors)
        {
            return Request_VendorFlow<OrderFlowModel>("dbo.Get_DefaultPricing_SalespersonFlow", location, dateFrom, dateTo, salespersonID, vendorNo, customerNo, excludeVendors);
        }

        private IEnumerable<T> Request_SalespersonFlow<T>(string storedProc, string location, string dateFrom, string dateTo, string salespersonID, string VendorNo, string CustomerNo, bool excludeVendors) where T : class
        {
            using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                db.Open();

                var r = db.Query<T>(storedProc,
                            param: new
                            {
                                TimePeriod = "",
                                Location = location,
                                SalespersonID = salespersonID,
                                ExcludeSTC = excludeVendors ? "1" : "0",
                                DateFrom = dateFrom,
                                DateTo = dateTo,
                                Vnd_No = VendorNo,
                                Customer_No = CustomerNo
                            },
                            commandType: System.Data.CommandType.StoredProcedure);

                return r;
            }
        }

        private IEnumerable<T> Request_VendorFlow<T>(string storedProc, string location, string dateFrom, string dateTo, string salespersonID, string vendorNo, string customerNo, bool excludeVendors) where T : class
        {
            using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                db.Open();

                var r = db.Query<T>(storedProc,
                           param: new
                           {
                               TimePeriod = "",
                               Location = location,
                               Vnd_No = vendorNo,
                               SalespersonID = salespersonID,
                               Customer_No = customerNo,
                               DateFrom = dateFrom,
                               DateTo = dateTo,
                               ExcludeSTC = excludeVendors ? "1" : "0"
                           },
                           commandType: System.Data.CommandType.StoredProcedure);

                return r;
            }
        }

        public IEnumerable<FormattedSalespersonFlowModel> GetFormattedSalespersonFlow(string location, string dateFrom, string dateTo, bool excludeVendors)
        {
            return Request_SalespersonFlow<FormattedSalespersonFlowModel>("dbo.Get_DefaultPricing_SalespersonFlow", location, dateFrom, dateTo, null, null, null, excludeVendors);
        }

        public IEnumerable<FormattedVendorFlowModel> GetFormattedVendorsFlow(string location, string dateFrom, string dateTo, bool excludeVendors)
        {
            return Request_VendorFlow<FormattedVendorFlowModel>("dbo.Get_DefaultPricing_VendorFlow", location, dateFrom, dateTo, null, null, null, excludeVendors);
        }

        public IEnumerable<FormattedVendorFlowModel> GetFormattedVendorsFilterBySalesperson(string location, string dateFrom, string dateTo, string salespersonID, bool excludeVendors)
        {
            return Request_SalespersonFlow<FormattedVendorFlowModel>("dbo.Get_DefaultPricing_SalespersonFlow", location, dateFrom, dateTo, salespersonID, null, null, excludeVendors);
        }

        public IEnumerable<FormattedSalespersonFlowModel> GetFormattedSalespersonsFilterByVendor(string location, string dateFrom, string dateTo, string vendorNo, bool excludeVendors)
        {
            return Request_VendorFlow<FormattedSalespersonFlowModel>("dbo.Get_DefaultPricing_VendorFlow", location, dateFrom, dateTo, null, vendorNo, null, excludeVendors);
        }

        public IEnumerable<FormattedCustomerFlowModel> GetFormattedCustomerFilterBySalesperson(string location, string dateFrom, string dateTo, string vendorNo, string salespersonID, bool excludeVendors)
        {
            return Request_VendorFlow<FormattedCustomerFlowModel>("dbo.Get_DefaultPricing_VendorFlow", location, dateFrom, dateTo, salespersonID, vendorNo, null, excludeVendors);
        }

        public IEnumerable<FormattedCustomerFlowModel> GetFormattedCustomerFilterByVendor(string location, string dateFrom, string dateTo, string vendorNo, string salespersonID, bool excludeVendors)
        {
            return Request_VendorFlow<FormattedCustomerFlowModel>("dbo.Get_DefaultPricing_SalespersonFlow", location, dateFrom, dateTo, salespersonID, vendorNo, null, excludeVendors);
        }

        public IEnumerable<FormattedOrderFlowModel> GetFormattedOrdersFilterByCustomer(string location, string dateFrom, string dateTo, string customerNo, string vendorNo, string salespersonID, bool excludeVendors)
        {
            return Request_VendorFlow<FormattedOrderFlowModel>("dbo.Get_DefaultPricing_VendorFlow", location, dateFrom, dateTo, salespersonID, vendorNo, customerNo, excludeVendors);
        }

        public IEnumerable<FormattedOrderFlowModel> GetFormattedOrdersFilterByCustomer_(string location, string dateFrom, string dateTo, string customerNo, string vendorNo, string salespersonID, bool excludeVendors)
        {
            return Request_VendorFlow<FormattedOrderFlowModel>("dbo.Get_DefaultPricing_SalespersonFlow", location, dateFrom, dateTo, salespersonID, vendorNo, customerNo, excludeVendors);
        }
    }
}
