using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models
{
    public class VendorEmail
    {
        public string VendorNo { get; set; }
        public string Name { get; set; }
        
        public string Phone { get; set; }
        public List<string> Addresses { get; set; }

    }

    public class VendorComparer : IEqualityComparer<VendorEmail>
    {
        public bool Equals(VendorEmail x, VendorEmail y)
        {
            return x.VendorNo.Equals(y.VendorNo);
        }

        public int GetHashCode(VendorEmail obj)
        {
            return obj.VendorNo.GetHashCode();
        }
    }
}
