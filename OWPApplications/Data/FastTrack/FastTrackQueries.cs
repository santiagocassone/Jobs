using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Data.FastTrack
{
    public class FastTrackQueries
    {
        public static string LINES = @"exec  dbo.[Get_FastTrackReport_Lines] '{0}','{1}','L','{2}'";

        public static string WEEKINFO = @"exec  dbo.[Get_FastTrackReport_Lines] '{0}','{1}','B','{2}'";

        public static string GRAPHIC = @"exec  dbo.[Get_FastTrackReport_Graphic] '{0}'";

        public static string UPSERT_VALUES = @"
update dbo.Order_Values 
set Value = '{0}', ModifiedOn = getdate(), ModifiedBy = '{3}'
where Company_Code = 'W' and Order_Type = 'O' and FieldID = '{4}'
and Order_NO = {1} and PO = {2}
if @@ROWCOUNT = 0
insert into dbo.Order_Values(FieldID, Company_Code,Order_No, Order_Type, PO, Value, CreatedOn, CreatedBy)
values('{4}','W', {1}, 'O', {2}, '{0}', getdate(), '{3}')";
    }
}

