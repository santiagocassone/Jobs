using Microsoft.Extensions.Configuration;
using OWPApplications.Models.StandardPrice;
using OWPApplications.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Data.StandardPrice
{
    public class StandardPriceHandler
    {
        private IConfiguration _configuration;

        public StandardPriceHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public HeaderInfoStandardPrice LoadHeaderInfo(string order)
        {
            try
            {

                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("Hedberg")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(String.Format(StandardPriceQueries.HEADER, order));
                    SqlCommand cmd2 = db.GetCommandSQL(String.Format(StandardPriceQueries.DELIVERY_INSTRUCTIONS, order));
                    SqlCommand cmd3 = db.GetCommandSQL(String.Format(StandardPriceQueries.MFG_INFO, order));

                    DataTable dt1 = db.SqlCommandToTable(cmd1);
                    DataTable dt2 = db.SqlCommandToTable(cmd2);
                    DataTable dt3 = db.SqlCommandToTable(cmd3);

                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return null;
                    }
                    return new HeaderInfoStandardPrice(dt1.Rows[0], dt2.Rows.Count > 0 ? dt2.Rows[0] : null, dt3);


                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<LineInfoStandardPrice> GetLinesInfo(string orderno)
        {

            try
            {

                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(String.Format(StandardPriceQueries.LINES, orderno, 1));

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

        public TotalsStandardPrice GetTotals(string orderno, int isBO)
        {
            try
            {

                using (DataBaseSQL db = new DataBaseSQL(_configuration.GetConnectionString("MxBTempDB")))
                {
                    SqlCommand cmd1 = db.GetCommandSQL(String.Format(StandardPriceQueries.TOTALS, orderno, isBO));

                    DataTable dt1 = db.SqlCommandToTable(cmd1);

                    if (dt1 == null || dt1.Rows.Count == 0)
                    {
                        return null;
                    }
                    return FillTotals(dt1.Rows[0]);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IEnumerable<LineInfoStandardPrice> FillLineInfo(DataTable table)
        {
            List<LineInfoStandardPrice> output = new List<LineInfoStandardPrice>();

            foreach (DataRow row in table.Rows)
            {
                output.Add(new LineInfoStandardPrice
                {
                    LineNo = clsLibrary.dBReadInt(row["Line_no"]),
                    GeneralTagging = clsLibrary.dBReadString(row["General_Tagging"]),
                    LineNotes = clsLibrary.dBReadString(row["LineNotes"]),
                    VendorNo = clsLibrary.dBReadString(row["Vnd_No"]),
                    CatalogNo = clsLibrary.dBReadString(row["Cat_No"]),
                    ProductDesc = clsLibrary.dBReadString(row["Product_Description"]),
                    QtyOrdered = clsLibrary.dBReadString(row["Qty_Ordered"]),
                    UnitSell = clsLibrary.dBReadDouble(row["UnitSell"]).ToString("C"),
                    ExtendedSell = clsLibrary.dBReadDouble(row["ExtendedSell"]).ToString("C"),
                    UnitCost = clsLibrary.dBReadDouble(row["UnitCost"]).ToString("C"),
                    ExtendedCost = clsLibrary.dBReadDouble(row["ExtendedCost"]).ToString("C"),
                    UnitList = clsLibrary.dBReadDouble(row["UnitList"]).ToString("C"),
                    ExtendedList = clsLibrary.dBReadDouble(row["ExtendedList"]).ToString("C"),
                    CostDiscount = (clsLibrary.dBReadDouble(row["Cost_Discount"]) / 100).ToString("P"),
                    GPPct = clsLibrary.dBReadString(row["GPPct"]) + "%",
                    GPDollars = clsLibrary.dBReadDouble(row["GPDollars"]).ToString("C"),
                    AutoPriced = clsLibrary.dBReadString(row["auto_priced"]),
                    IsBo = clsLibrary.dBReadBoolean(row["IsBO"])
                });
            }

            return output;
        }

        private TotalsStandardPrice FillTotals(DataRow row)
        {
            return new TotalsStandardPrice
            {
                TotalSell = clsLibrary.dBReadDouble(row["TotalSell"]).ToString("C"),
                TotalCost = clsLibrary.dBReadDouble(row["TotalCost"]).ToString("C"),
                GPDollars = clsLibrary.dBReadDouble(row["GPDollars"]).ToString("C"),
                GPPct = (clsLibrary.dBReadDouble(row["GPPct"]) / 100).ToString("P"),
                GPColor = (clsLibrary.dBReadDouble(row["GPPct"]) < 14.995) ? "red" : "",
                Tax = clsLibrary.dBReadDouble(row["Tax"]).ToString("C"),
                Total_W_Tax = clsLibrary.dBReadDouble(row["Total_W_Tax"]).ToString("C"),

                PIDTotalSell = clsLibrary.dBReadDouble(row["PIDTotalSell"]).ToString("C"),
                PIDTotalCost = clsLibrary.dBReadDouble(row["PIDTotalCost"]).ToString("C"),
                PIDGPDollars = clsLibrary.dBReadDouble(row["PIDGPDollars"]).ToString("C"),
                PIDGPPct = (clsLibrary.dBReadDouble(row["PIDGPPct"]) / 100).ToString("P"),
                PIDGPColor = (clsLibrary.dBReadDouble(row["PIDGPPct"]) < 14.995) ? "red" : ""
            };
        }
    }
}
