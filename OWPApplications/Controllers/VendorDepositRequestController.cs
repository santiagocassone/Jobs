using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OWPApplications.Data;
using OWPApplications.Models.VendorDepositRequest;
using OWPApplications.Utils;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace OWPApplications.Controllers
{
	public class VendorDepositRequestController : Controller
    {
        DbHandler _db;
        IConfiguration _configuration;
        EmailHelper _emailHelper;
        ILogger _logger;

        public VendorDepositRequestController(DbHandler dbHandler, IConfiguration configuration, ILoggerFactory logFactory, EmailHelper emailHelper)
        {
            _db = dbHandler;
            _configuration = configuration;
            _logger = logFactory.CreateLogger(nameof(VendorDepositRequestController));
            _emailHelper = emailHelper;
        }

        public IActionResult Index()
        {
            var vm = new VDRViewModel();

            return View(vm);
        }

        [HttpPost, DisableRequestSizeLimit, RequestFormLimits(ValueCountLimit = 15000)]
        public IActionResult Index(int order, string poSuffix )
        {
            var vm = new VDRViewModel();

            if (poSuffix == null)
            {
                ViewData["Suffixs"] = _db.VendorDepositRequestHandler.RequestSuffix(order);
                ViewData["OrderNo"] = order;

                return View(vm);
            } else
            {                
                vm.VDRCost = _db.VendorDepositRequestHandler.RequestCost(order, int.Parse(poSuffix));

                return View(vm);
            }
            
        }

        [HttpPost]
        public IActionResult SendEmail_VDR([FromForm] string rawdata, IFormFile file)
        {
            clsLog log = new clsLog(_configuration);

            try
            {
                string mailBody;
                string subject;
                string depTerm;

                IEnumerable<VDREmailForm> emails = JsonConvert.DeserializeObject<IEnumerable<VDREmailForm>>(rawdata);

                foreach (var email in emails)
                {
                    if (email.DepositTerms == "Custom")
                    {
                        depTerm = String.Concat(email.CustomDepositTerms, "%");
                    }
                    else
                    {
                        depTerm = email.DepositTerms;
                    }

                    DateTime genDate = DateTime.Now;
                    string guid = GeneratePDF(email, genDate, depTerm);

                    mailBody = VDRViewModel.CreateEmailBody(email, genDate, depTerm);
                    subject = VDRViewModel.CreateSubjectEmail(email);

					// Create email body
					var result = _emailHelper.SendEmailWithReply(
						email.From, email.YourName,
                        @"ap@​oneworkplace.​com",
						email.CC1,
						email.CC2,
						"",
						subject, mailBody, file,
                        _configuration["VDRPath"] + "Vendor_Deposit_Request" + guid + ".pdf",
                        "VDR");

					_db.SaveActivity(new ActivityLog
					{
						YourEmail = email.From,
						ToEmail = "ap@​oneworkplace.​com",
						Body = mailBody,
						Subject = subject,
						Order = email.Order.Split('-')[0].Trim(),
						Vendor = email.Vendor,
						CreatedBy = "VDR",
                        CompanyCode = "W"
					});

					string orderNo = email.Order.Split('-')[0].Trim();
                    string vendorName = email.Vendor.Trim();
                    double depositAmount = Convert.ToDouble(email.AmtDue.Replace("$", "").Replace(".", ",").Trim());
                    DateTime dueDate = Convert.ToDateTime(email.DueDate, new CultureInfo("en-US"));
                    string notes = email.Notes.Trim();
                    SendVDRToWrike(orderNo, vendorName, depositAmount, dueDate, notes);
                }

                return Json("OK");
            }
            catch (Exception ex)
            {
                log.WriteError("SendEmail_VDR", ex.Message, ex);
                return Json(ex.Message);
            }
        }

        private string GeneratePDF(VDREmailForm email, DateTime genDate, string depTerm)
        {
            try
            {
                removeOldFiles();
                string guid = Guid.NewGuid().ToString();
                using (StreamWriter stream = new StreamWriter(_configuration["VDRPath"] + "VDR_Table" + guid + ".html", false, System.Text.Encoding.UTF8))
                {
                    stream.Write(@"<html>" + stream.NewLine + "<head>" + stream.NewLine);
                    stream.Write(@"
                <style>.table td {text-align: center;}</style>
                <link href='lib/bootstrap/dist/css/bootstrap.min.css' rel='stylesheet' />
                <script src='lib/bootstrap/dist/js/bootstrap.min.js'></script>" + stream.NewLine + "</head>" + stream.NewLine);
                    stream.Write(@"<body>" + stream.NewLine);
                    stream.Write(@"
                <table class='table table-striped'>
                    <thead>
                        <tr>
                            <th scope='col'>Requested by</th>
                            <th scope='col'>Date Requested</th>
                            <th scope='col'>Vendor</th>
                            <th scope='col'>Order</th>
                            <th scope='col'>Deposit %</th>
                            <th scope='col'>Amt Due</th>
                            <th scope='col'>Date Needed</th>
                            <th scope='col'>Payment Type</th>
                            <th scope='col'>Currency</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>" + email.YourName + "</td><td>" + genDate + "</td><td>" + email.Vendor + "</td><td>" + email.Order + "</td><td>" + depTerm + "</td><td>" + email.AmtDue + "</td><td>" + email.DueDate + "</td><td>" + email.PaymentType + "</td><td>" + (email.CurrencyType == "Other" ? email.CurrencyCustom : email.CurrencyType) + "</td></tr></tbody></table>"
                                    + stream.NewLine + "</body>" + stream.NewLine + "</html>");
                }

                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjA3NTg3QDMxMzcyZTM0MmUzMEhhRDNnL1Z6TTBtTVR4VW5CaExZSkd0RVNDeVA3M2ZoYVoxaUEyakVWaGc9");

                WebKitConverterSettings settings = new WebKitConverterSettings();
				settings.WebKitPath = "Libraries/QtBinariesDotNetCore";
				settings.Orientation = PdfPageOrientation.Landscape;
                HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();
                htmlConverter.ConverterSettings = settings;

                using (PdfDocument document = htmlConverter.Convert(_configuration["VDRPath"] + "VDR_Table" + guid + ".html"))
                {
                    using (FileStream fileStream = new FileStream(_configuration["VDRPath"] + "Vendor_Deposit_Request" + guid + ".pdf", FileMode.Create, FileAccess.ReadWrite, FileShare.Delete)) //, FileShare.ReadWrite, 4096, true
                    {
                        document.Save(fileStream);
                        document.Close(true);
                    }
                }

                

                return guid;
            }
            catch (Exception ex)
            {   
                throw ex;
            }
        }

        private void SendVDRToWrike(string orderNo, string vendor, double depositAmount, DateTime dueDate, string notes)
		{
            clsLog log = new clsLog(_configuration);

            clsWrike oWrike = new clsWrike();
            clsWrike.eleTask task = new clsWrike.eleTask();
            task.dates = new clsWrike.eleDates();

            string Vendor_Deposit_Requests_Folder_ID = _configuration.GetValue<string>("VDR_FolderID");
            string Order_Custom_Field_ID = _configuration.GetValue<string>("VDR_OrderCustomFieldID");
            string Vendor_Custom_Field_ID = _configuration.GetValue<string>("VDR_VendorCustomFieldID");
            string Deposit_Amount_Custom_Field_ID = _configuration.GetValue<string>("VDR_DepositAmountCustomFieldID");
            //string Due_Date_Custom_Field_ID = _configuration.GetValue<string>("VDR_DueDate");

            task.description = notes;
            task.title = string.Format("Order #: {0}", orderNo);
            task.dates.due = dueDate;
            task.dates.start = DateTime.Now;
            task.status = clsWrike.TaskStatus.Active;
            task.customFields.add(new clsWrike.clsField(Order_Custom_Field_ID, orderNo));
            task.customFields.add(new clsWrike.clsField(Vendor_Custom_Field_ID, vendor));
            task.customFields.add(new clsWrike.clsField(Deposit_Amount_Custom_Field_ID, depositAmount.ToString("0.00")));
            //task.customFields.add(new clsWrike.clsField(Due_Date_Custom_Field_ID, dueDate.ToString("yyyy-MM-dd")));

            try
            {
                oWrike.addTask(Vendor_Deposit_Requests_Folder_ID, task);

                if (task.id.Trim() != "")
                {
                    log.Write("Sent to Wrike [task.id]: " + task.id);
                }
                else
                {
                    log.Write("Error sending to Wrike: Can't get the task Id");
                }
            }
            catch (Exception ex)
            {
                log.Write("Error sending to Wrike: " + ex.Message);
            }
        }

        private void removeOldFiles()
        {
            try
            {
                string[] files = Directory.GetFiles("wwwroot/files");
                foreach (var item in files)
                {

                    DateTime creation = System.IO.File.GetLastWriteTime(item);
                    if (creation.AddDays(1) < DateTime.Now)
                    {
                        //remove file
                        System.IO.File.Delete(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}