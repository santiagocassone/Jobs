using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Mail;

namespace OWPApplications.Utils
{
    public class EmailHelper
    {
        private readonly ILogger _logger;
        IConfiguration _configuration;

        public EmailHelper(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger(nameof(EmailHelper));
            _configuration = configuration;
        }

        public bool SendEmail(string smtpHost, string from, string fromName, string to, string CC1, string CC2, string Subject, string Body,
            IFormFile file, string UserName, string Password, bool UseSSL, bool cc)
        {
            try
            {
                // Command line argument must the the SMTP host.
                SmtpClient client = new SmtpClient(smtpHost);
                if (UserName != "")
                {
                    client.EnableSsl = UseSSL;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential(UserName, Password);
                }
                // Specify the message content.
                MailMessage message = new MailMessage(from, to.Replace(";", ","));
                message.Body = Body;
                if (cc)
                {
                    MailAddress sender = new MailAddress(from, fromName);
                    message.CC.Add(sender);
                }

                if (!string.IsNullOrEmpty(CC1))
                {
                    MailAddress copy1 = new MailAddress(CC1);
                    message.CC.Add(copy1);
                }
                if (!string.IsNullOrEmpty(CC2))
                {
                    MailAddress copy2 = new MailAddress(CC2);
                    message.CC.Add(copy2);
                }

                message.IsBodyHtml = true;
                message.Subject = Subject;

                if (file != null)
                {
                    if (file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            var fileBytes = ms.ToArray();
                            Attachment att = new Attachment(new MemoryStream(fileBytes), file.FileName);
                            message.Attachments.Add(att);
                        }
                    }
                }

                client.Send(message);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EMAIL_ERROR", from, to, Body, Subject, CC1, CC2);
                return false;
            }
        }

        public class EmailProperties
        {
            /// <summary>
            /// Sender email address (default to noreply@oneworkplace.com) 
            /// </summary>
            public string FromAddress { get; set; } = "noreply@oneworkplace.com";

            /// <summary>
            /// Email address that will be used when the user chooses to reply the email
            /// </summary>
            public string ReplyTo { get; set; }

            /// <summary>
            /// Name that will be displayed as sender
            /// </summary>
            public string FromName { get; set; }

            /// <summary>
            /// List of email recipients addresses
            /// </summary>
            public string[] To { get; set; } = new string[] { };
            public string[] CC { get; set; } = new string[] { };
            public string[] BCC { get; set; } = new string[] { };

            public string Body { get; set; }

            /// <summary>
            /// If set to tru will add the email sender to the CC list
            /// </summary>
            public bool CopyEmailSender { get; set; }

            public bool IsHTMLBody { get; set; } = true;

            public string Subject { get; set; }

            public IFormFile[] Attachments { get; set; }
        }

        public bool SendEmailV2(IConfiguration config, EmailProperties emailProps, string app)
        {
            MailMessage message = new MailMessage();
            clsLog log = new clsLog(_configuration);

            try
            {
                string environment = config.GetValue<string>("CurrentEnvironment");
                string envEmail = config.GetValue<string>("EnvironmentEmail");
                var smtpHost = config.GetValue<string>("SMTP_HOST");
                var UserName = config.GetValue<string>("EmailUsername");
                var Password = config.GetValue<string>("EmailPassword");
                var UseSSL = config.GetValue<bool>("USE_SSL");

                // Command line argument must the the SMTP host.
                using (var client = new SmtpClient(smtpHost))
                {
                    if (!string.IsNullOrEmpty(UserName))
                    {
                        client.EnableSsl = UseSSL;
                        client.UseDefaultCredentials = false;
                        client.Credentials = new System.Net.NetworkCredential(UserName, Password);
                    }

                    // Specify the message content.
                    message.From = new MailAddress(emailProps.FromAddress, emailProps.FromName);

                    foreach (var toAddr in emailProps.To)
                    {
                        if (environment == "D")
                        {
                            message.Body += "<p>To: " + toAddr + "</p>";
                        } 
                        else
                        {
                            message.To.Add(toAddr);
                        }
                        
                    }

                    if (environment == "D")
                    {
                        message.To.Add(envEmail);
                    }

                    // Setup email reply to address
                    var sender = new MailAddress(emailProps.ReplyTo ?? emailProps.FromAddress, emailProps.FromName);
                    message.ReplyToList.Add(sender);

                    if (emailProps.CopyEmailSender)
                    {
                        if (environment == "D")
                        {
                            message.Body += "<p>CC: " + sender + "</p>";
                        }
                        else
                        {
                            message.CC.Add(sender);
                        }
                    }

                    foreach (var copyAddr in emailProps.CC)
                    {
                        if (environment == "D")
                        {
                            message.Body += "<p>CC: " + copyAddr + "</p>";
                        }
                        else
                        {
                            message.CC.Add(copyAddr);
                        }
                    }

                    foreach (var bccAddr in emailProps.BCC)
                    {
                        if (environment == "D")
                        {
                            message.Body += "<p>BCC: " + bccAddr + "</p>";
                        }
                        else
                        {
                            message.Bcc.Add(bccAddr);
                        }
                    }

                    if (environment == "D")
                    {
                        message.Body += emailProps.Body;
                    }
                    else
                    {
                        message.Body = emailProps.Body;
                    }

                    message.IsBodyHtml = emailProps.IsHTMLBody;
                    message.Subject = emailProps.Subject;

                    // Attach the files 
                    if (emailProps.Attachments != null && emailProps.Attachments.Length > 0)
                    {
                        foreach (var fileToAttach in emailProps.Attachments)
                        {
                            using (var ms = new MemoryStream())
                            {
                                fileToAttach.CopyTo(ms);
                                message.Attachments.Add(new Attachment(new MemoryStream(ms.ToArray()), fileToAttach.FileName));
                            }
                        }
                    }

                    client.Send(message);
                }

                return true;
            }
            catch (Exception ex)
            {
                string logText = string.Format("\r\nFROM: {0}\r\nTO: {1}\r\nCC: {2}\r\nBCC: {3}\r\nSUBJECT: {4}\r\nREPLYTO: {5}", message.From, message.To, message.CC, message.Bcc, message.Subject, message.ReplyToList);
                log.WriteError(app + " EmailHelper.SendEmailWithReply()", logText, ex);
                throw ex;
            }
        }

        public bool SendEmailWithReply(string from, string fromName, string to, string cc1, string cc2, string bcc, string subject, string body, IFormFile file, string fileName, string app)
        {
            string environment = _configuration.GetValue<string>("CurrentEnvironment");
            string envEmail = _configuration.GetValue<string>("EnvironmentEmail");
            string smtpHost = _configuration.GetValue<string>("SMTP_HOST");
            string userName = _configuration.GetValue<string>("EmailUsername");
            string password = _configuration.GetValue<string>("EmailPassword");
            bool useSSL = _configuration.GetValue<bool>("USE_SSL");
            SmtpClient client = new SmtpClient(smtpHost);
            MailMessage message = new MailMessage();
            clsLog log = new clsLog(_configuration);

            try
            {
                if (!string.IsNullOrEmpty(userName))
                {
                    client.EnableSsl = useSSL;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential(userName, password);
                }
                //string domain = from.Split("@")[1];
                //if (domain == "oneworkplace.com" || domain == "open-sq.com")
                //{
                    message.From = new MailAddress(from, fromName);
    //            }
				//else
				//{
				//	message.From = new MailAddress("noreply@oneworkplace.com", fromName);
				//}
                if (environment == "D")
                {
                    message.To.Add(envEmail);
                    message.Body += "<p>To: " + to.Replace(";", ",") + "</p>";
                } else
                {
                    if (to != "")
                    {
                        message.To.Add(to.Replace(";", ","));
                    }
                    
                }

                message.Subject = subject;
                
                message.IsBodyHtml = true;
				message.ReplyToList.Add(new MailAddress(from, fromName));

				string[] arrCC = (cc1?.Replace(",", ";") + ";" + cc2).Split(';', StringSplitOptions.RemoveEmptyEntries);

                foreach (var address in arrCC)
                {
                    if (environment == "D")
                    {
                        message.Body += "<p>CC: " + address.Trim() + "</p>";
                    }
                    else
                    {
                        message.CC.Add(address.Trim());
                    }
                }

                if (!string.IsNullOrEmpty(bcc))
                {
                    if (environment == "D")
                    {
                        message.Body += "<p>BCC: " + bcc + "</p>";
                    }
                    else
                    {
                        message.Bcc.Add(bcc);
                    }
                }

                if (environment == "D")
                {
                    message.Body += body;
                }
                else
                {
                    message.Body = body;
                }
                

                if (!string.IsNullOrEmpty(fileName))

				{
                    message.Attachments.Add(new Attachment(fileName));
                }

                if (file?.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        Attachment att = new Attachment(new MemoryStream(fileBytes), file.FileName);
                        message.Attachments.Add(att);
                    }
                }

                client.Send(message);
                string logText = string.Format("\r\nFROM: {0}\r\nTO: {1}\r\nCC: {2}\r\nBCC: {3}\r\nSUBJECT: {4}\r\nREPLYTO: {5}\r\n", message.From, message.To, message.CC, message.Bcc, message.Subject, message.ReplyToList);

                log.Write(app + " EmailHelper.SendEmailWithReply()" + logText);
                client.Dispose();
                message.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                string logText = string.Format("\r\nFROM: {0}\r\nTO: {1}\r\nCC: {2}\r\nBCC: {3}\r\nSUBJECT: {4}\r\nREPLYTO: {5}\r\n", message.From, message.To, message.CC, message.Bcc, message.Subject, message.ReplyToList);

                log.WriteError(app + " EmailHelper.SendEmailWithReply()", logText, ex);
                throw ex;
            }
        }

        private void SetTestingEnvironment(MailMessage message)
        {
            if (message.ReplyToList[0].Address == _configuration["TesterEmail"])
            {
                message.To.Clear();
                message.CC.Clear();
                message.Bcc.Clear();
                message.To.Add(_configuration["TesterEmail"].ToString());
                message.Subject = "[TESTING] " + message.Subject + " - Env: " + _configuration["CurrentEnvironment"];
            }
        }
    }
}
