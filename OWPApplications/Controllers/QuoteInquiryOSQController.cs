using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OWPApplications.Data;
using OWPApplications.Models;
using OWPApplications.Models.QuoteInquiry;
using OWPApplications.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Controllers
{
    public class QuoteInquiryOSQController : Controller
    {
        ILogger _logger;
        DbHandler _db;
        IConfiguration _configuration;
        EmailHelper _emailHelper;


        public QuoteInquiryOSQController(DbHandler dbHandler, IConfiguration configuration, ILoggerFactory logFactory, EmailHelper emailHelper)
        {
            _db = dbHandler;
            _configuration = configuration;
            _logger = logFactory.CreateLogger(nameof(HomeController));
            _emailHelper = emailHelper;

        }

        public IActionResult Index(int? orderno, List<int> locations)
        {
            QuoteInquiryOSQViewModel vm = new QuoteInquiryOSQViewModel();
            vm.ShowResults = false;

            var usedLocations = _db.QuoteInquiryHandler.GetUsedLocations("OSQ");

            ViewData["FormValue"] = orderno;
            ViewData["SelectedUsedLocations"] = locations.Select(l => l.ToString());
            ViewData["UsedLocations"] = usedLocations;
            ViewData["Is61"] = locations.Contains(61) ? "yes" : "no";

            if (!(orderno == null && locations.Count == 0))
            {
                vm.ShowResults = true;
                vm.QuoteNo = orderno;

                vm.HeaderInfo = _db.QuoteInquiryHandler.LoadHeaderInfo(orderno.ToString(), "OSQ");

                if (vm.HeaderInfo != null)
                {
                    vm.linesInfos = _db.QuoteInquiryHandler.GetLinesInfo(orderno.ToString(), locations, "OSQ");
                    vm.MiscLines = _db.QuoteInquiryHandler.GetMiscLines(orderno.ToString(), locations, "OSQ");

                    List<string> filterVendors = new List<string> { "SUD02", "OTM01", "1" };
                    List<string> stcVendors = vm.linesInfos?.Where(x => x.IsSteelcaseVendor).Select(x => x.VendorNo.Trim()).Distinct().ToList();
                    if (stcVendors?.Count() > 0) filterVendors.AddRange(stcVendors);
                    List<string> vendors = new List<string>();
                    if (vm.linesInfos != null)
                    {
                        vendors = vm.linesInfos.Where(x => !filterVendors.Contains(x.VendorNo.Trim())).Select(x => x.VendorNo.Trim()).Distinct().ToList();
                    }

                    List<VendorEmail> vendorEmails = new List<VendorEmail>();
                    foreach (var vnd in vendors)
                    {
                        vendorEmails.Add(_db.QuoteInquiryHandler.GetVendorByNo(vnd, "OSQ"));
                    }
                    vm.Vendors = vendorEmails.Where(x => x != null);

                    vm.TotalGP = _db.QuoteInquiryHandler.GetTotalGP(orderno.ToString(), 0, "OSQ");
                }
                else
                {
                    vm.ShowResults = false;
                }
            }
            return View(vm);
        }

        [HttpPost]
        public IActionResult SendEmail_QI([FromForm] string rawdata, List<IFormFile> file)
        {
            clsLog log = new clsLog(_configuration);

            try
            {
                IEnumerable<EmailFormQuoteInquiryOSQ> data = JsonConvert.DeserializeObject<IEnumerable<EmailFormQuoteInquiryOSQ>>(rawdata);
                foreach (var email in data)
                {
                    string emailTo = "";
                    string mailBody = "";
                    string subject = QuoteInquiryOSQViewModel.CreateSubjectEmail(email);

                    foreach (var to in email.To)
                    {
                        if (!to.Contains("(A)")) // Standard Email
                        {
                            mailBody = QuoteInquiryOSQViewModel.CreateStandardEmailBody(email);
                            emailTo = to.Trim();
                        }
                        else // Amazon Email
                        {
                            mailBody = QuoteInquiryOSQViewModel.CreateAmazonEmailBody(email);
                            emailTo = to.Replace("(A)", "").Trim();
                        }

                        if (file.Where(x => x.FileName.Split("##")[0] == email.VendorNo).Any())
                        {
                            _emailHelper.SendEmailWithReply(
                                        xmlConfig.QuoteInquiryFrom, "",
                                        emailTo,
                                        email.CC1,
                                        email.CC2,
                                        "",
                                        subject,
                                        mailBody,
                                        file.Where(x => x.FileName.Split("##")[0] == email.VendorNo).FirstOrDefault(),
                                        null,
                                        "QIOSQ");
                        }
                        else
                        {
                            _emailHelper.SendEmailWithReply(
                                        xmlConfig.QuoteInquiryFrom, "",
                                        emailTo,
                                        email.CC1,
                                        email.CC2,
                                        "",
                                        subject, mailBody, null, null, "QIOSQ");
                        }


                        _db.SaveActivity(new ActivityLog
                        {
                            YourEmail = xmlConfig.QuoteInquiryFrom,
                            ToEmail = emailTo,
                            Vendor = email.VendorName,
                            Subject = subject,
                            Body = mailBody,
                            CreatedBy = "QIOSQ",
                            Order = email.Quote_OrderNo,
                            CompanyCode = "S"
                        });
                    }
                }

                return Json(true);
            }
            catch (Exception ex)
            {
                log.WriteError("SendEmail_QI", ex.Message, ex);
                throw ex;
            }
        }
    }
}
