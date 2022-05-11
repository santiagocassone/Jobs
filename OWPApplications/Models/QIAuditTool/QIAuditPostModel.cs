using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.QIAuditTool
{
    public class QIAuditPostModel
    {
        public string QuoteNo { get; set; }
        public IFormFile fileSalesXML { get; set; }
        public IFormFile fileOrderXML { get; set; }
    }
}
