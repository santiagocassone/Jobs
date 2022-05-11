using Dapper;
using Microsoft.Extensions.Configuration;
using OWPApplications.Models;
using OWPApplications.Models.POPDashboard;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Data.POPDashboard
{
    public class SalespersonModel
    {
        public string salesperson_id { get; set; }
        public string salesperson_info { get; set; }
    }

    public class CustomersModel
    {
        public string customer_no { get; set; }
        public string name { get; set; }
    }

    public class POPDashboardHandler
    {
        private IConfiguration _configuration;

        public POPDashboardHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<POPDashboardSummary> Get_PostOrderDashboard_Summary(string SalesPersonID, IEnumerable<string> CustomerIDs, string CustomerNo, string ProjectID, string Region)
        {
            return Request_Data<POPDashboardSummary>("dbo.Get_PostOrderDashboard_Summary", SalesPersonID, CustomerIDs, CustomerNo, ProjectID, Region);
        }

        public IEnumerable<BudgetActualModel> Get_PostOrderDashboard_BudgetVsActual(string SalesPersonID, IEnumerable<string> CustomerIDs, string CustomerNo, string ProjectID, string Region)
        {
            return Request_Data<BudgetActualModel>("dbo.Get_PostOrderDashboard_BudgetVsActual", SalesPersonID, CustomerIDs, CustomerNo, ProjectID, Region);
        }

        public IEnumerable<FutureCRDModel> Get_PostOrderDashboard_FutureCRDs(string SalesPersonID, IEnumerable<string> CustomerIDs, string CustomerNo, string ProjectID, string Region)
        {
            return Request_Data<FutureCRDModel>("dbo.Get_PostOrderDashboard_FutureCRDs", SalesPersonID, CustomerIDs, CustomerNo, ProjectID, Region);
        }

        public IEnumerable<PastCRDModel> Get_PostOrderDashboard_PastCRDs(string SalesPersonID, IEnumerable<string> CustomerIDs, string CustomerNo, string ProjectID, string Region)
        {
            return Request_Data<PastCRDModel>("dbo.Get_PostOrderDashboard_PastCRDs", SalesPersonID, CustomerIDs, CustomerNo, ProjectID, Region);
        }

        public IEnumerable<OpenQuotes> Get_PostOrderDashboard_OpenQuotes(string SalesPersonID, IEnumerable<string> CustomerIDs, string CustomerNo, string ProjectID, string Region)
        {
            return Request_Data<OpenQuotes>("dbo.Get_PostOrderDashboard_Quotes", SalesPersonID, CustomerIDs, CustomerNo, ProjectID, Region).OrderByDescending(x => x.QuoteNo);
        }

        public IEnumerable<CustomerViewModel> Get_PostOrderDashboard_CustomerView(string SalesPersonID, IEnumerable<string> CustomerIDs, string CustomerNo, string ProjectID, string Region)
        {
            return Request_Data<CustomerViewModel>("dbo.Get_PostOrderDashboard_CustomerView", SalesPersonID, CustomerIDs, CustomerNo, ProjectID, Region);
        }

        private IEnumerable<T> Request_Data<T>(string StoredProc, string SalesPersonID, IEnumerable<string> CustomerIDs, string CustomerNo, string ProjectID, string Region) where T : class
        {
            using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                db.Open();

                // Prepate a Datatable for TVP
                var custIdsDT = new System.Data.DataTable();
                custIdsDT.Columns.Add("ID", typeof(string));

                if (CustomerIDs != null && CustomerIDs.Count() > 0)
                {
                    foreach (var id in CustomerIDs)
                    {
                        var row = custIdsDT.NewRow();
                        row["ID"] = id;
                        custIdsDT.Rows.Add(row);
                    }
                }

                var r = db.Query<T>(StoredProc,
                               param: new
                               {
                                   CUSTOMERNOTABLE = custIdsDT.AsTableValuedParameter("VARCHARIDTABLETYPE"),
                                   SALESPERSONID = SalesPersonID,
                                   CUSTOMERNO = CustomerNo,
                                   PROJECTID = ProjectID,
                                   Region = Region
                               },
                               commandType: System.Data.CommandType.StoredProcedure);

                return r;
            }
        }



        public List<SelectValues> LoadCustomerList(string salesperson, string region)
        {
            using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                db.Open();
                var param = new DynamicParameters();
                param.Add("@Region", region);
                param.Add("@salespersonid", salesperson ?? "ALL");
                var r = db.Query<CustomersModel>("dbo.[Get_Customer_List]", param, commandType: System.Data.CommandType.StoredProcedure);

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



        public List<SelectValues> LoadSalesPerson(string region)
        {
            try
            {
                using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
                {
                    db.Open();
                    var param = new DynamicParameters();
                    param.Add("@Region", region);
                    var r = db.Query<SalespersonModel>("dbo.[Get_InternalStatusReport_Salesperson]", param, commandType: System.Data.CommandType.StoredProcedure);

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

                        return output;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


    }
}
