using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OWPApplications.Data;
using OWPApplications.Models;
using OWPApplications.Models.SubcontractorReceiving;
using OWPApplications.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OWPApplications.Controllers
{
	public class SubcontractorReceivingController : Controller
    {
        DbHandler _db;
        EmailHelper _emailHelper;
        IConfiguration _configuration;

        public SubcontractorReceivingController(DbHandler dbHandler, EmailHelper emailHelper, IConfiguration configuration)
        {
            _db = dbHandler;
            _emailHelper = emailHelper;
            _configuration = configuration;
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserProfile user)
        {
            if (ModelState.IsValid)
            {
                var userProfile = _db.SubcontractorReceivingHandler.ValidateAndGetVendorNo(user.UserEmail, user.Password);

                if (!string.IsNullOrEmpty(userProfile.UserCode))
                {
                    HttpContext.Session.SetString("UserCode", userProfile.UserCode);
                    HttpContext.Session.SetString("UserName", userProfile.UserName);
                    HttpContext.Session.SetString("UserEmail", user.UserEmail);

                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Message = "Invalid email or password";
                    return View();
                }
            }
            return View(user);
        }

        public ActionResult Logout()
        {
            HttpContext.Session.Remove("UserCode");
			HttpContext.Session.Remove("UserEmail");
            HttpContext.Session.Remove("UserName");

            return RedirectToAction("Login");
        }

        public IActionResult Index(string dateFrom, string dateTo, int? orderNo, bool isDetail, string emailType, string poRef)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserCode")))
            {
                ViewData["UserCode"] = HttpContext.Session.GetString("UserCode");
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
				ViewData["UserEmail"] = HttpContext.Session.GetString("UserEmail");

				SubcontractorReceivingViewModel vm = new SubcontractorReceivingViewModel
                {
                    VendorNo = HttpContext.Session.GetString("UserCode"),
                    VendorName = HttpContext.Session.GetString("UserName"),
					DateFrom = dateFrom,
					DateTo = dateTo,
                    OrderNo = orderNo,
                    Success = ""
                };

                if (!isDetail)
                {
                    vm.ShowSummary = true;
                    vm.Summary = _db.SubcontractorReceivingHandler.GetSummaryInfo(vm.VendorNo, dateFrom, dateTo, orderNo);
                    vm.EmailTypeList = _db.SubcontractorReceivingHandler.LoadLookupGeneral("SubcontractorEmailType");
                    vm.POsReceivedComplete = vm.Summary?.Where(x => x.TotallyReceived == "X").Select(x => x).ToList();
				}
                else
                {
                    vm.ShowDetails = true;
                    vm.EmailType = Convert.ToInt32(GetEmailType(emailType, "value"));
                    vm.EmailTypeText = GetEmailType(emailType, "text");
                    vm.Lines = _db.SubcontractorReceivingHandler.GetLineInfo(vm.VendorNo, dateFrom, dateTo, poRef);
					vm.IssueTypeList = _db.SubcontractorReceivingHandler.LoadLookupGeneral("SubcontractorIssueType");
					vm.IssueDetailList = _db.SubcontractorReceivingHandler.LoadLookupGeneral("SubcontractorDamageDetail");
                    vm.POReference = poRef;
                }

                return View(vm);
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public IActionResult SendEmail_SCR([FromForm] string rawdata, IFormFile file)
        {
			SCREmailData data = JsonConvert.DeserializeObject<IEnumerable<SCREmailData>>(rawdata).FirstOrDefault();

			if (data.LinesData == null)
            {
                return Json(new { success = false, responseText = "There is no rows checked to send email." });
            }

            if (data.To == "")
            {
                return Json(new { success = false, responseText = "Please complete Your Email Address field." });
            }

            try
            {
                string bcc = "";

                switch (Enum.Parse(typeof(EmailType), data.EmailType))
                {
                    case EmailType.ReceiptOfProduct:
                        bcc = "wrike+into636988955@wrike.com";
                        break;

                    case EmailType.RequestTrackingInformation:
                        bcc = "wrike+into636989528@wrike.com";
                        break;

                    case EmailType.RequestShipDate:
                        bcc = "wrike+into636989528@wrike.com";
                        break;

                    case EmailType.ReportIssue:
                        bcc = "wrike+into636989601@wrike.com";
                        break;
                }

                _emailHelper.SendEmailWithReply(
                    @"wrikeintegrate@oneworkplace.com",
					data.FromName,
                    data.To,
					data.CC1,
					data.CC2,
                    bcc,
					GetSubject(data),
					GetBody(data),
					file,
                    null,
                    "SCR");

				foreach (var line in data.LinesData)
				{
					_db.SaveActivity(new ActivityLog
					{
						YourEmail = data.From,
						ToEmail = data.To,
						Body = GetBody(data),
						Subject = GetSubject(data),
						Order = line.OrderNo,
						Line = line.LineNo,
						Vendor = "",
						CreatedBy = "SCR",
                        CompanyCode = "W"
					});
				}

                return Json(new { success = true, responseText = "Your email has been sent successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message });
            }
        }

        public string GetSubject(SCREmailData data)
        {
            string ret = "";

			switch (Enum.Parse(typeof(EmailType), data.EmailType))
			{
				case EmailType.ReceiptOfProduct:
					ret = "Product Received – " + data.LinesData.ToList()[0].VendorName + ": " + data.POReference + " " + data.LinesData.ToList()[0].ProductVendor + " " + DateTime.Now.ToString("MM/dd/yyyy");
					break;

				case EmailType.RequestTrackingInformation:
					ret = "Tracking Information Requested – " + data.VendorName + ": " + data.PORef + " " + DateTime.Now.ToString("MM/dd/yyyy");
					break;

                case EmailType.RequestShipDate:
                    ret = "Ship Date Requested – " + data.VendorName + ": " + data.PORef + " " + DateTime.Now.ToString("MM/dd/yyyy");
                    break;

                case EmailType.ReportIssue:
					ret = "OI Reported " + data.LinesData.ToList()[0].VendorName + ": " + data.POReference + " " + data.LinesData.ToList()[0].ProductVendor + " " + DateTime.Now.ToString("MM/dd/yyyy");
					break;
			}

			return ret;
        }

        public string GetBody(SCREmailData data)
        {
            string ret = @"<html>
                        <style>
                        body {font:14px arial, sans-serif;color:#666;}
                        table {border-collapse:collapse;}
                        table, td, th {border: 1px solid #ccc; padding:10px;}
                        th {font-size:14px; background-color:#ff4d4d; border:1px solid #ff4d4d; color:#fff;}
                        p {margin: 0;padding: 0;}
                        </style>
                        <body>
                        <br/>            
                        <p>Dear One Workplace:</p>
                        <br/>";

            switch (Enum.Parse(typeof(EmailType), data.EmailType))
            {
                case EmailType.ReceiptOfProduct:

					ret += @"<p>The following product has been received at " + data.LinesData.ToList()[0].VendorName + @"'s warehouse:</p>
							<br/>
                            <p>Purchase Order #: " + data.POReference + @"</p>
                            <p>Product Vendor(s): " + data.LinesData.ToList()[0].ProductVendor + @"</p>
                            <p>Date: " + DateTime.Now.ToString("MM/dd/yyyy") + @"</p>";

					var linesData = data.LinesData.OrderBy(x => x.LineNo);
					foreach (var line in linesData)
                    {
                        ret += @"<p>Line # " + line.LineNo + ", Qty " + line.LineQtyReceived + "</p>";
                    }

                    ret += @"<br/>
                            <p>Please contact " + data.FromName + " at " + data.From + @" if you have any other questions.</p>
                            <br/>
                            <p>Thank you!</p>";

                    break;

                case EmailType.RequestTrackingInformation:

                    ret += @"<p>Please provide tracking information for purchase order " + data.PORef + @"</p>
                            <br/>
                            <p>Please contact " + data.FromName + " at " + data.From + @" with the requested information or if you have any other questions.</p>
                            <br/>
                            <p>Thank you!</p>";

                    break;

                case EmailType.ReportIssue:

					string issueType = _db.SubcontractorReceivingHandler.LoadLookupGeneral("SubcontractorIssueType").Where(x => x.LookupGeneralID.Trim() == data.IssueType.Trim()).Select(x => x.Value).FirstOrDefault();
					string issueDetail = _db.SubcontractorReceivingHandler.LoadLookupGeneral("SubcontractorDamageDetail").Where(x => x.LookupGeneralID.Trim() == data.IssueDetail.Trim()).Select(x => x.Value).FirstOrDefault();

					ret += @"<p>" + data.LinesData.ToList()[0].VendorName + @" has reported an issue with the following product:</p>
							<br/>
                            <p>Purchase Order: " + data.POReference + @"</p>";
							foreach (var line in data.LinesData)
							{
								ret += @"<p>Line #: " + line.LineNo + @"</p>
										<p>QTY: " + line.Qty + @"</p>";
							}
							ret += @" <p>Type of Issue: " + issueType + @"</p>
                            <p>Detail: " + issueDetail + @"</p>
                            <p>Description: " + data.Description + @"</p>
                            <br/>
                            <p>See the attachments for reference. Please contact " + data.FromName + " at " + data.From + @" if you have any other questions.</p>
                            <br/>
                            <p>Thank you!</p>";

					break;
                case EmailType.RequestShipDate:

                    ret += @"<p>Our system has indicated that there has been no ship date information for PO #" + data.PORef + @". Please reach out to the vendor to acquire an acknowledgment and/or load the information into Hedberg.</p>
                            <p>Feel free to contact me at " + data.From + @" with any other questions. Thank you!</p>
                            <br />
                            <p>" + data.FromName + @"</p>";
                    break;
            }

            return ret;
        }

		[HttpPost]
		public IActionResult UpdateQtyReceived(UpdateQtyReceivedModel data)
		{
			_db.SubcontractorReceivingHandler.UpdateQtyReceived(data.CompanyCode, data.POSuffix, data.OrderIndex, data.LineIndex, data.NewQLRValue);

			return Ok("Qty Received saved.");
		}

        private string GetEmailType(string emailType, string format)
		{
            if (format == "value")
            {
                return emailType.Split("|")[0];
            }
            else if (format == "text")
            {
                return emailType.Split("|")[1];
            }
            else
                return "";
		}
	}
}