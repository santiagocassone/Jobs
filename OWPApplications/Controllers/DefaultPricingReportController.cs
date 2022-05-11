using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OWPApplications.Data;
using OWPApplications.Models.DefaultPricingReport;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OWPApplications.Controllers
{
    public class DefaultPricingReportController : Controller
    {
        private readonly DbHandler _db;

        public DefaultPricingReportController(DbHandler dbHandler) 
        {
            _db = dbHandler;
        }

        public IActionResult Index(string detView, string location, string dateFrom, string dateTo, string salespersonID, string vendorNo, string customerNo, string excludeVendors)
        {
            bool excludeVnds = excludeVendors == "on" ? true : false;
            var locations = _db.DefaultPricingReportHandler.LoadLocations();

            DefaultPricingReportViewModel vm = new DefaultPricingReportViewModel
            {
                Location = location,
                DateFrom = dateFrom,
                DateTo = dateTo,
                VendorNo = vendorNo,
                SalespersonID = salespersonID,
                CustomerNo = customerNo,
                ExcludeVendors = excludeVendors,
                DetailsView = detView
            };

            ViewData["Locations"] = locations;
            ViewData["Location"] = location;
            ViewData["DateFrom"] = dateFrom;
            ViewData["DateTo"] = dateTo;
            ViewData["ExcludeVendors"] = excludeVnds;
            ViewData["DetView"] = detView;

            if (string.IsNullOrEmpty(location))
            {
                vm.ShowResults = false;
            }
            else
            {
                vm.ShowResults = true;

                if (!string.IsNullOrEmpty(detView))
                {
                    vm.ShowMainView = false;

                    switch (detView)
                    {
                        case "VND":
                            vm.VendorFlow = _db.DefaultPricingReportHandler.GetVendorsFilterBySalesperson(location, dateFrom, dateTo, salespersonID, excludeVnds).OrderByDescending(x => x.DifferenceDollars);
                            break;
                        case "SPN":
                            vm.SalespersonFlow = _db.DefaultPricingReportHandler.GetSalespersonsFilterByVendor(location, dateFrom, dateTo, vendorNo, excludeVnds).OrderByDescending(x => x.DifferenceDollars);
                            break;
                        case "CST":
                            vm.CustomerFlow = _db.DefaultPricingReportHandler.GetCustomerFilterBySalesperson(location, dateFrom, dateTo, vendorNo, salespersonID, excludeVnds).OrderByDescending(x => x.DifferenceDollars);
                            break;
                        case "CST_":
                            vm.CustomerFlow = _db.DefaultPricingReportHandler.GetCustomerFilterByVendor(location, dateFrom, dateTo, vendorNo, salespersonID, excludeVnds).OrderByDescending(x => x.DifferenceDollars);
                            break;
                        case "ORD":
                            vm.OrderFlow = _db.DefaultPricingReportHandler.GetOrdersFilterByCustomer(location, dateFrom, dateTo, customerNo, vendorNo, salespersonID, excludeVnds).OrderByDescending(x => x.DifferenceDollars);
                            break;
                        case "ORD_":
                            vm.OrderFlow = _db.DefaultPricingReportHandler.GetOrdersFilterByCustomer_(location, dateFrom, dateTo, customerNo, vendorNo, salespersonID, excludeVnds).OrderByDescending(x => x.DifferenceDollars);
                            break;
                    }
                }
                else
                {
                    vm.ShowMainView = true;
                    vm.SalespersonFlow = _db.DefaultPricingReportHandler.GetSalespersonFlow(location, dateFrom, dateTo, excludeVnds).OrderByDescending(x => x.DifferenceDollars);
                    vm.VendorFlow = _db.DefaultPricingReportHandler.GetVendorsFlow(location, dateFrom, dateTo, excludeVnds).OrderByDescending(x => x.DifferenceDollars);
                }

                SetSummary(vm, detView ?? "MAIN");
            }

            ViewData["CurrView"] = vm;

            return View(vm);
        }

        private void SetSummary(DefaultPricingReportViewModel model, string view)
        {
            double totalCost = 0;
            double totalSell = 0;
            double totalGPDollars = 0;
            double differenceGPDollars = 0;

            switch (view)
            {
                case "VND":
                    totalCost = model.VendorFlow.Select(x => Convert.ToDouble(x.TotalCost)).Sum();
                    totalSell = model.VendorFlow.Select(x => Convert.ToDouble(x.TotalSell)).Sum();
                    totalGPDollars = model.VendorFlow.Select(x => Convert.ToDouble(x.GP)).Sum();
                    differenceGPDollars = model.VendorFlow.Select(x => x.DifferenceDollars).Sum();
                    break;
                case "SPN":
                    totalCost = model.SalespersonFlow.Select(x => Convert.ToDouble(x.TotalCost)).Sum();
                    totalSell = model.SalespersonFlow.Select(x => Convert.ToDouble(x.TotalSell)).Sum();
                    totalGPDollars = model.SalespersonFlow.Select(x => Convert.ToDouble(x.GP)).Sum();
                    differenceGPDollars = model.SalespersonFlow.Select(x => x.DifferenceDollars).Sum();
                    break;
                case "CST":
                case "CST_":
                    totalCost = model.CustomerFlow.Select(x => Convert.ToDouble(x.TotalCost)).Sum();
                    totalSell = model.CustomerFlow.Select(x => Convert.ToDouble(x.TotalSell)).Sum();
                    totalGPDollars = model.CustomerFlow.Select(x => Convert.ToDouble(x.GP)).Sum();
                    differenceGPDollars = model.CustomerFlow.Select(x => x.DifferenceDollars).Sum();
                    break;
                case "ORD":
                case "ORD_":
                    totalCost = model.OrderFlow.Select(x => Convert.ToDouble(x.TotalCost)).Sum();
                    totalSell = model.OrderFlow.Select(x => Convert.ToDouble(x.TotalSell)).Sum();
                    totalGPDollars = model.OrderFlow.Select(x => Convert.ToDouble(x.GP)).Sum();
                    differenceGPDollars = model.OrderFlow.Select(x => x.DifferenceDollars).Sum();
                    break;
                case "MAIN":
                    model.SummaryComparison = new DefaultPricingReportViewModel.SummaryComparisonModel()
                    {
                        SalespersonSummary = new DefaultPricingReportViewModel.SummaryModel()
                        {
                            TotalCost = model.SalespersonFlow.Select(x => Convert.ToDouble(x.TotalCost)).Sum(),
                            TotalSell = model.SalespersonFlow.Select(x => Convert.ToDouble(x.TotalSell)).Sum(),
                            TotalGPDollars = model.SalespersonFlow.Select(x => Convert.ToDouble(x.GP)).Sum(),
                            DifferenceGPDollars = model.SalespersonFlow.Select(x => x.DifferenceDollars).Sum()
                        },
                        VendorSummary = new DefaultPricingReportViewModel.SummaryModel()
                        {
                            TotalCost = model.VendorFlow.Select(x => Convert.ToDouble(x.TotalCost)).Sum(),
                            TotalSell = model.VendorFlow.Select(x => Convert.ToDouble(x.TotalSell)).Sum(),
                            TotalGPDollars = model.VendorFlow.Select(x => Convert.ToDouble(x.GP)).Sum(),
                            DifferenceGPDollars = model.VendorFlow.Select(x => x.DifferenceDollars).Sum()
                        }
                    };
                    break;
            }

            model.Summary = new DefaultPricingReportViewModel.SummaryModel()
            {
                TotalCost = totalCost,
                TotalSell = totalSell,
                TotalGPDollars = totalGPDollars,
                DifferenceGPDollars = differenceGPDollars

            };
        }

        [HttpPost]
        public ActionResult ExportToExcel(string view, string location, string dateFrom, string dateTo, string salespersonID, string vendorNo, string customerNo, string excludeVendors)
        {
            DataSet ds = new DataSet();
            bool excludeVnds = excludeVendors == "on" ? true : false;

            switch (view)
            {
                case "MAIN":
                    ds.Tables.Add(ConvertToDataTable(_db.DefaultPricingReportHandler.GetFormattedSalespersonFlow(location, dateFrom, dateTo, excludeVnds).OrderByDescending(x => x.DifferenceDollars).ToList()));
                    ds.Tables.Add(ConvertToDataTable(_db.DefaultPricingReportHandler.GetFormattedVendorsFlow(location, dateFrom, dateTo, excludeVnds).OrderByDescending(x => x.DifferenceDollars).ToList()));
                    break;
                case "SEC_SPN":
                    ds.Tables.Add(ConvertToDataTable(_db.DefaultPricingReportHandler.GetFormattedSalespersonsFilterByVendor(location, dateFrom, dateTo, vendorNo, excludeVnds).OrderByDescending(x => x.DifferenceDollars).ToList()));
                    break;
                case "SEC_VND":
                    ds.Tables.Add(ConvertToDataTable(_db.DefaultPricingReportHandler.GetFormattedVendorsFilterBySalesperson(location, dateFrom, dateTo, salespersonID, excludeVnds).OrderByDescending(x => x.DifferenceDollars).ToList()));
                    break;
                case "SEC_CST":
                    ds.Tables.Add(ConvertToDataTable(_db.DefaultPricingReportHandler.GetFormattedCustomerFilterBySalesperson(location, dateFrom, dateTo, vendorNo, salespersonID, excludeVnds).OrderByDescending(x => x.DifferenceDollars).ToList()));
                    break;
                case "SEC_CST_":
                    ds.Tables.Add(ConvertToDataTable(_db.DefaultPricingReportHandler.GetFormattedCustomerFilterByVendor(location, dateFrom, dateTo, vendorNo, salespersonID, excludeVnds).OrderByDescending(x => x.DifferenceDollars).ToList()));
                    break;
                case "SEC_ORD":
                    ds.Tables.Add(ConvertToDataTable(_db.DefaultPricingReportHandler.GetFormattedOrdersFilterByCustomer(location, dateFrom, dateTo, customerNo, vendorNo, salespersonID, excludeVnds).OrderByDescending(x => x.DifferenceDollars).ToList()));
                    break;
                case "SEC_ORD_":
                    ds.Tables.Add(ConvertToDataTable(_db.DefaultPricingReportHandler.GetFormattedOrdersFilterByCustomer_(location, dateFrom, dateTo, customerNo, vendorNo, salespersonID, excludeVnds).OrderByDescending(x => x.DifferenceDollars).ToList()));
                    break;
            }

            using (ExcelPackage objExcelPackage = new ExcelPackage())
            {
                foreach (DataTable dtSrc in ds.Tables)
                {
                    ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets.Add(dtSrc.TableName);
                    objWorksheet.Cells["A1"].LoadFromDataTable(dtSrc, true);
                    objWorksheet.Cells.Style.Font.SetFromFont(new Font("Calibri", 10));
                    objWorksheet.Cells[2, 2, dtSrc.Rows.Count + 1, dtSrc.Columns.Count + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    objWorksheet.Cells.AutoFitColumns();

                    using (ExcelRange objRange = objWorksheet.Cells[1, 1, 1, dtSrc.Columns.Count])
                    {
                        objRange.Style.Font.Bold = true;
                        objRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        objRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        objRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        objRange.Style.Fill.BackgroundColor.SetColor(Color.LightSeaGreen);
                    }
                }

                byte[] fileBytesArray = objExcelPackage.GetAsByteArray();
                MemoryStream ms = new MemoryStream(fileBytesArray);
                ms.Position = 0;

                return File(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DefaultPricingReport.xlsx");
            }
        }

        private static DataTable ConvertToDataTable<T>(List<T> models)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in Props)
            {   
                dataTable.Columns.Add(prop.Name);
            } 
            foreach (T item in models)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {  
                    values[i] = Props[i].GetValue(item, null);
                }  
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }
    }
}