using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWPApplications.Utils
{
    public class WrikeModels
    {
    }


    public class WrikeReply<T>
    {
        public string kind { get; set; }
        public string state { get; set; }
        public T[] data { get; set; }
    }

    public class WrikeTask
    {
        /// <summary>
        /// Title of task, required
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Description of task, will be left blank, if not set
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Parent folders for newly created task. Can not contain recycleBinId
        /// </summary>
        public string[] Parents { get; set; }

    }

    public class WrikeFolderTreeReply : WrikeReply<WrikeFolderTree>
    {

    }

    public class WrikeFolderTree
    {
        public string id { get; set; }
        public string title { get; set; }
        public string scope { get; set; }
        public string[] childIds { get; set; }
    }
}
