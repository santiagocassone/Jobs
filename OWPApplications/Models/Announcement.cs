using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Models
{
    public class Announcement
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Date { get; set; }


        public Announcement(DataRow row)
        {
            Title = clsLibrary.dBReadString(row["Title"]).Trim();
            Description = clsLibrary.dBReadString(row["Description"]);
            Date = clsLibrary.dbReadDateAsStringFormat(row["Date"], "MM/dd/yyyy");
        }

        public Announcement()
        {

        }
    }
}
