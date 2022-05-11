using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Data.WarehouseDT
{
    public class WarehouseDTQueries
    {
        public static string LINES = @"exec  dbo.[Get_WarehouseDTReport_OrderLines] '{0}','{0}'";

        public static string SCHEDULE_TYPES = @"EXEC dbo.[Get_ScheduleType_List]";

        public static string STAGING_NAMES = @"EXEC dbo.[Get_WarehouseDTReport_StagingNames]";

        public static string UPDATE_STAGING_NAMES = @"EXEC dbo.[Update_WarehouseDTReport_Order_StagingName] '{0}','{1}','{2}','{3}','{4}','{5}'";

        public static string UPSERT_COMMENT = @"
update dbo.Order_Values 
set Value = '{0}', ModifiedOn = getdate(), ModifiedBy = 'WDT_APP'
where Company_Code = 'W' and Order_Type = 'O' and FieldID = 'Comment'
and Order_NO = {1}
if @@ROWCOUNT = 0
insert into dbo.Order_Values(FieldID, Company_Code, Order_No, Order_Type, Value, CreatedOn, CreatedBy)
values('Comment', 'W', {1}, 'O','{0}', getdate(), 'WDT_APP')";

    }

}

