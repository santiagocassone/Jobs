using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OWPApplications.Models.VendorDepositRequest;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;

namespace OWPApplications.Data.VendorDepositRequest
{
    public class VendorDepositRequestHandler
    {
        private IConfiguration _configuration;

        public VendorDepositRequestHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<VDRCost> RequestCost(int order, int suffix)
        {
            using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                db.Open();

                var r = db.Query<VDRCost>("dbo.Get_VendorDepositRequestCost",
                               param: new { Order_No = order, PO_Suffix = suffix },
                               commandType: System.Data.CommandType.StoredProcedure);

                return r;
            }
        }

        public IEnumerable<string> RequestSuffix(int order)
        {
            using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                db.Open();

                var r = db.Query<string>("dbo.Get_Order_Suffix_List",
                               param: new { Order_No = order },
                               commandType: System.Data.CommandType.StoredProcedure);

                return r;
            }
        }
    }
}
