using OWPApplications.Models.QuoteInquiry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models.QIAuditTool
{
    internal static class StrComparerExtension
    {
        /// <summary>
        /// Custom comparer that performs a Trim before comparing, this was made to avoid lots of spaces coming from the DB data
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static bool CustomEqual(this string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
            {
                return true;
            }

            if(string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            {
                return false;
            }

            return s1.Trim().Equals(s2.Trim());
        }
    }

    public class QIComparerElem
    {
        public int LineNumber { get; set; }
        public LineInfoQuoteInquiry DB { get; set; }
        public LineInfoQuoteInquiry File { get; set; }
        public string Comment { get; set; }


        public bool HasDifferences
        {
            get
            {
                return !(IsEqualCatalogNo && IsEqualDescription && IsEqualGeneralTagging && IsEqualQtyOrdered);
            }
        }

        public bool IsEqualCatalogNo
        {
            get
            {
                return DB != null && DB.CatalogNo.CustomEqual(File?.CatalogNo);
            }
        }

        public bool IsEqualDescription
        {
            get
            {
                return DB != null && DB.Description.CustomEqual(File?.Description);
            }
        }


        public bool IsEqualGeneralTagging
        {
            get
            {
                return DB != null && DB.GeneralTagging.CustomEqual(File?.GeneralTagging);
            }
        }


        public bool IsEqualQtyOrdered
        {
            get
            {
                return DB != null && DB.QtyOrdered == File?.QtyOrdered;
            }
        }


    }

    public class QIComparer : IEnumerable<QIComparerElem>
    {
        private Dictionary<int, QIComparerElem> _lineInfo { get; set; }

        public QIComparer()
        {
            _lineInfo = new Dictionary<int, QIComparerElem>();
        }

        public QIComparerElem GetLine(int line)
        {
            if (!_lineInfo.ContainsKey(line))
            {
                _lineInfo[line] = new QIComparerElem { LineNumber = line };
            }

            return _lineInfo[line];
        }

        public IEnumerator<QIComparerElem> GetEnumerator()
        {
            foreach (var key in _lineInfo.Keys.OrderBy(k => k))
            {
                yield return _lineInfo[key];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
