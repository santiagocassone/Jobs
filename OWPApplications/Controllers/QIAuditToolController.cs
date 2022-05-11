using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OWPApplications.Data;
using OWPApplications.Models;
using OWPApplications.Models.QIAuditTool;
using OWPApplications.Models.QuoteInquiry;
using OWPApplications.Utils;

namespace OWPApplications.Controllers
{
    public class QIAuditToolController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly EmailHelper _emailHelper;
        private DbHandler _db;

        public QIAuditToolController(DbHandler dbHandler, IConfiguration configuration, EmailHelper EmailHelper)
        {
            _db = dbHandler;
            _configuration = configuration;
            _emailHelper = EmailHelper;
        }

        public IActionResult Index(string orderno)
        {
            var vm = new QuoteInquiryAuditViewModel();
            vm.ShowResults = false;
            vm.QuoteNo = orderno;

            return View(vm);
        }

        [HttpPost]
        public IActionResult Index(QIAuditPostModel postModel)
        {
            var salesXml = new StreamReader(postModel.fileSalesXML.OpenReadStream()).ReadToEnd();
            var sales = QIFilesReader.ReadQISalesXMLFile(salesXml);

            if (postModel.fileOrderXML != null)
            {
                var orderXml = new StreamReader(postModel.fileOrderXML.OpenReadStream()).ReadToEnd();
                var order = QIFilesReader.ReadQIOrderXMLFile(orderXml);
            }

            var vm = new QuoteInquiryAuditViewModel();
            vm.ShowResults = true;

            var linesInfo = _db.QuoteInquiryHandler.GetLinesInfo(postModel.QuoteNo, new List<int>(), "OWP");
            var comparer = new QIComparer();

            // Set comparer lines coming from the DB
            foreach (var line in linesInfo)
            {
                comparer.GetLine(line.LineNo).DB = line;
                // Set line previously saved comments
                comparer.GetLine(line.LineNo).Comment = line.MergedComments.Replace("\r\n", "<hr>");
            }

            // Set comparer lines coming from the File
            if (sales.DocumentList?.Length > 0)
            {
                foreach (var salesLine in sales.DocumentList[0].LineItemList)
                {
                    if (comparer.GetLine(salesLine.LineItemNumber).File == null)
                    {
                        comparer.GetLine(salesLine.LineItemNumber).File = new LineInfoQuoteInquiry();
                    }

                    var fileData = comparer.GetLine(salesLine.LineItemNumber).File;

                    fileData.QtyOrdered = salesLine.Quantity.Value.ToString();
                    fileData.CatalogNo = salesLine.Product.ProductCode;
                    fileData.Description = BuildDescriptionText(salesLine);

                    var generalTagging = salesLine.Product.VariantCode.Split(';').Where(c => c.StartsWith("IT:")).FirstOrDefault()?.Replace("IT:", "");
                    fileData.GeneralTagging = generalTagging;
                }
            }


            vm.Lines = comparer;

            vm.QuoteNo = postModel.QuoteNo;


            // Get Header Info
            vm.HeaderInfo = _db.QuoteInquiryHandler.LoadHeaderInfo(postModel.QuoteNo, "OWP");

            // Calcuate BOM and DB line count
            vm.BOMLineCount = comparer.Where(i => i.File != null).Count();
            vm.DBLineCount = comparer.Where(i => i.DB != null).Count();

            // Find out the lines that are in the DB but not in the BOM file
            vm.MissingFromBOM = comparer.Where(i => i.File == null).Select(i => i);
            vm.MissingFromDB = comparer.Where(i => i.DB == null).Select(i => i);

            // Find out lines with differences
            vm.LinesWithDifferences = comparer.Where(i => i.HasDifferences).Select(i => i);


            if (vm.HeaderInfo != null)
            {
                //Get Lines Info
                string[] filterVendors = new string[] { "STE01", "BRA00", "ONE20", "ONE22", "ONE23", "ONE24", "ONE26", "ONE27", "ONE28" };
                List<VendorEmail> emails = _db.GetVendorEmails("QI", "OWP", false);

                vm.TotalGP = _db.QuoteInquiryHandler.GetTotalGP(postModel.QuoteNo, 0, "OWP");
            }

            return View(vm);
        }


        private string BuildDescriptionText(LineItemType line)
        {
            var desc = new StringBuilder();

            desc.Append(line.Product.ProductDescription?.Length > 0 ? line.Product.ProductDescription[0].Value : string.Empty);

            foreach (var feature in line.Product.FeatureList)
            {
                var name = feature.ValueName?.Length > 0 ? feature.ValueName[0].Value : string.Empty;
                var value = feature.Value ?? string.Empty;

                desc.Append($" {name}: {value}");
            }

            return desc.ToString();
        }


        public string BuildEmailBody(string ComparisonTable)
        {
            string body = @"
            <html>
            <style>
            body {font:12px ""Segoe UI"",Roboto,""Helvetica Neue"",Arial,""Noto Sans"",sans-serif,""Apple Color Emoji"",""Segoe UI Emoji"",""Segoe UI Symbol"",""Noto Color Emoji"";color:#212529;}
            table {border-collapse:collapse;}
            table, td, th {border: 1px solid #ccc; padding:10px;}
            
            p {
             margin: 0;
             padding: 0;
            }
        .comparison-even-row { background-color: #f9f9f9; }
        .comparison-odd-row { background-color: #efefef; }
        .equal-comparison-color {background-color: #60c448;}
        .different-comparison-color {background-color: #ff959f;}
        .email-highlight  {background-color: yellow;}
        .table-bordered {  border: 1px solid #dee2e6;}

        .table-bordered th, .table-bordered td {  border: 1px solid #dee2e6;}
        .table-bordered thead th, .table-bordered thead td {  border-bottom-width: 2px; }
        .table-bordered th,  .table-bordered td {    border: 1px solid #dee2e6 !important;  }
            </style>
            <body>
            <h3>Quote Audit Tool</h3>
            <h4>Comparison Results</h4>
            <div>##_COMPARISONTABLE_##</div>
            </body>
            </html>
            ";

            body = body.Replace("##_COMPARISONTABLE_##", ComparisonTable);
            return body;
        }


        public class QIAuditCommentItem
        {
            public string value { get; set; }
            public string lineno { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(string FromAddress, string FromName, string To, string CC1, string CC2, string QuoteNo, string ComparisonTable, QIAuditCommentItem[] Comments)
        {
            // Save comments
            foreach (var comment in Comments)
            {
                var commentText = $"On: {DateTime.Now.ToString("g")} - By: {FromAddress}({FromName}) - To: {To} - Comment: {comment.value}";
                await _db.QIAuditToolHandler.SaveComment(int.Parse(QuoteNo), int.Parse(comment.lineno), "", commentText.Substring(0, Math.Min(commentText.Length, 2000)));
            }

            // Send Email
            if (ComparisonTable != null)
            {
                var result = _emailHelper.SendEmailWithReply(
                                 FromAddress,
                                 FromName,
                                 To,
                                 CC1,
                                 CC2,
                                 "",
                                 $"Quote Audit Tool - Comparison of Quote# {QuoteNo}",
                                BuildEmailBody(ComparisonTable),
                                 null,
                                null,
                                "QIAT");
            }
            return Json(true);
        }
    }
}