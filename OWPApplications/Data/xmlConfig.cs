using OWPApplications.Models;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Linq;

namespace OWPApplications.Data
{
    public class xmlConfig
    {
        static XElement _xml;

        private static XElement xml
        {
            get
            {
                if (_xml == null) _xml = XElement.Load(@"wwwroot\xml\config.xml");
                return _xml;
            }
        }
        public static string PostOrderPlacementFrom
        {
            get
            {

                string strTemp = "";
                try
                {
                    strTemp = xml.Descendants("PostOrderPlacementFrom").Select(x => x.Value).FirstOrDefault();
                }
                catch { }
                return strTemp;
            }
        }
        public static string PostOrderPlacementFromName
        {
            get
            {

                string strTemp = "";
                try
                {
                    strTemp = xml.Descendants("PostOrderPlacementFromName").Select(x => x.Value).FirstOrDefault();
                }
                catch { }
                return strTemp;
            }
        }
        public static string getSubject(string EmailType, string app)
        {
            string strTemp = "";
            try
            {
                strTemp = xml.Descendants("EmailTemplate").Where(x => x.Attribute("app")?.Value == app && x.Attribute("type")?.Value == EmailType).Descendants("subject").FirstOrDefault().Value;
            }
            catch { }
            return strTemp;
        }
        public static string getBody(string EmailType, string app)
        {
            string strTemp = "";
            try
            {
                strTemp = xml.Descendants("EmailTemplate").Where(x => x.Attribute("app")?.Value == app && x.Attribute("type")?.Value == EmailType).Descendants("body").FirstOrDefault().Value;
            }
            catch { }
            return strTemp;

        }
        public static string getEmailFrom(string EmailType, string app)
        {
            string strTemp = "";
            try
            {
                strTemp = xml.Descendants("EmailTemplate").Where(x => x.Attribute("app")?.Value == app && x.Attribute("type")?.Value == EmailType).Descendants("from").FirstOrDefault().Value;
            }
            catch { }
            return strTemp;

        }
        public static string getEmailCc(string EmailType, string app)
        {
            string strTemp = "";
            try
            {
                strTemp = xml.Descendants("EmailTemplate").Where(x => x.Attribute("app")?.Value == app && x.Attribute("type")?.Value == EmailType).Descendants("cc").FirstOrDefault().Value;
            }
            catch { }
            return strTemp;

        }
        public static string getPageTemplate(string app, string type)
        {
            string strTemp = "";
            try
            {
                strTemp = xml.Descendants("PagesTemplate").Where(x => x.Attribute("app")?.Value == app && x.Attribute("type")?.Value == type).FirstOrDefault().Descendants().First().ToString();
                //strTemp = xml.Descendants("PagesTemplate").Where(x => x.Attribute("app")?.Value == app && x.Attribute("type")?.Value == type).FirstOrDefault().Value;
            }
            catch { }
            return strTemp;

        }

        private static string _ExcludedVendors = "";
        public static string GetExcludedVendors()
        {
            if (_ExcludedVendors == "")
                _ExcludedVendors = xml.Descendants("ExcludedVendorsOnPOP").Select(x => x.Value).FirstOrDefault();
            return _ExcludedVendors;
        }
        private static string _Dealer = "";
        public static string Dealer
        {
            get
            {
                if (_Dealer == "")
                    _Dealer = xml.Descendants("Dealer").Select(x => x.Value).FirstOrDefault();
                return _Dealer;
            }
        }
        private static List<EmailTypePOP> _emailTypes = null;
        public static List<EmailTypePOP> GetEmailTypes()
        {
            if (_emailTypes == null)
            {
                _emailTypes = new List<EmailTypePOP>();
                var emailTypes = xml.Descendants("EmailTemplate").Select(x => (string)x.Attribute("type") + "_" + (string)x.Attribute("title") + "_" + (string)x.Attribute("displayWeekDayField"));

                foreach (var et in emailTypes)
                {
                    string[] parts = et.Split('_');
                    var type = parts[0];
                    var title = parts[1];
                    var displayWeekDayField = parts[2];

                    _emailTypes.Add(new EmailTypePOP
                    {
                        Type = type,
                        Title = title,
                        displayWeekDayField = displayWeekDayField
                    });
                }
            }
            return _emailTypes;
        }

        public static List<EmailVendor> GetEmailVendor(string app)
        {
            var ret = new List<EmailVendor>();
            var emailVendor = xml.Descendants("VendorEmail").Select(x =>
                                new EmailVendor
                                {
                                    App = (string)x.Attribute("app"),
                                    Prefix = (string)x.Attribute("prefix"),
                                    Field = (string)x.Attribute("field")
                                });

            foreach (var et in emailVendor)
            {
                if ((app == et.App) && (et.Field != ""))
                {
                    ret.Add(new EmailVendor
                    {
                        Prefix = et.Prefix,
                        Field = et.Field
                    });
                }
            }

            return ret;
        }
        public static List<EmailField> GetEmailFields()
        {
            var ret = new List<EmailField>();
            foreach (var eTmp in xml.Descendants("EmailTemplate"))
            {
                foreach (var oEle in eTmp.Descendants("field"))
                {
                    ret.Add(new EmailField
                    {
                        EmailType = (string)eTmp.Attribute("type"),
                        Name = (string)oEle.Attribute("name"),
                        Type = (string)oEle.Attribute("type"),
                        Label = (string)oEle.Attribute("label")
                    });
                }
            }

            return ret;
        }

        private static string _CompanyCode = "";
        public static string getCompanyCode()
        {
            if (_CompanyCode == "")
            {
                string strTemp = "";
                try
                {
                    strTemp = xml.Descendants("CompanyCode").Select(x => x.Value).FirstOrDefault();
                }
                catch { }
                _CompanyCode = strTemp;
            }
            return _CompanyCode;
        }

        private static string _LaborQuoteEstimatorEmail = "";
        public static string getLaborQuoteEstimatorEmail()
        {
            if (_LaborQuoteEstimatorEmail == "")
            {
                string strTemp = "";
                try
                {
                    strTemp = xml.Descendants("LaborQuoteEstimatorEmail").Select(x => x.Value).FirstOrDefault();
                }
                catch { }
                _LaborQuoteEstimatorEmail = strTemp;
            }
            return _LaborQuoteEstimatorEmail;
        }

        public static string QuoteInquiryFrom
        {
            get
            {

                string strTemp = "";
                try
                {
                    strTemp = xml.Descendants("QuoteInquiryFrom").Select(x => x.Value).FirstOrDefault();
                }
                catch { }
                return strTemp;
            }
        }

    }
}
