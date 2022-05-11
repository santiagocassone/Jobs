using OWPApplications.Models.QuoteInquiry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.QIAuditTool
{
    public class QuoteInquiryAuditViewModel
    {
        public bool ShowResults { get; set; }

        public string QuoteNo { get; set; }

        public HeaderInfoQouteInquiry HeaderInfo { get; set; }

        //public IEnumerable<LineInfoQuoteInquiry> linesInfos { get; set; }

        //public IEnumerable<MiscLinesQuoteInquiry> MiscLines { get; set; }

        public string TotalGP { get; set; }

        //public IEnumerable<VendorEmail> Vendors { get; set; }
        public QIComparer Lines { get; set; }
        public IEnumerable<QIComparerElem> MissingFromBOM { get; internal set; }
        public IEnumerable<QIComparerElem> MissingFromDB { get; internal set; }
        public IEnumerable<QIComparerElem> LinesWithDifferences { get; internal set; }
        public int BOMLineCount { get; internal set; }
        public int DBLineCount { get; internal set; }


    }
}
