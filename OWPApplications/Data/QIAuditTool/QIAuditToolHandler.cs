using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Data.QIAuditTool
{
    public class QIAuditToolHandler
    {
        private IConfiguration _configuration;

        public QIAuditToolHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SaveComment(int Order_No, int Line_No, string KeyString, string Value)
        {
            using (var db = new SqlConnection(_configuration.GetConnectionString("MxBTempDB")))
            {
                await db.OpenAsync();
                string sql = @"
                    insert into dbo.[Order_Values]
                    (FieldID, Company_Code, Order_Type, Line_Type, Order_No, Line_No, KeyString, [Value], CreatedOn, CreatedBy)
                    values
                    ('Comments', 'W', 'Q', 'L', @Order_No, @Line_No, @KeyString, @Value, getdate(), 'QIAUDITTOOL')
                ";
                int rCount = await db.ExecuteAsync(sql, new { Order_No, Line_No, KeyString, Value });
                
                return rCount == 1;
            }
        }
    }
}
