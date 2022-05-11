using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Data
{
    public class DefaultPricingReportQueries
    {
        public static string LOCATIONS = @"EXEC [dbo].[Get_Location]";

        public static string TIMEPERIODS = @"EXEC [dbo].[Get_DefaultPricing_TimePeriod]";

        public static string DEFAULTPRICINGDATA = @"EXEC [dbo].[Get_DefaultPricing_SalespersonFlow] '{0}', '{1}', '{2}'";
    }
}
