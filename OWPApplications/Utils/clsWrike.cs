using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OWPApplications.Utils
{
    public class clsWrike
    {
        private string permanentToken = @"eyJ0dCI6InAiLCJhbGciOiJIUzI1NiIsInR2IjoiMSJ9.eyJkIjoie1wiYVwiOjYwNTE2LFwiaVwiOjY2NjI0ODEsXCJjXCI6NDYxNDQ2OCxcInVcIjo2NDEyNDYyLFwiclwiOlwiVVNcIixcInNcIjpbXCJXXCIsXCJGXCIsXCJJXCIsXCJVXCIsXCJLXCIsXCJDXCIsXCJBXCIsXCJMXCJdLFwielwiOltdLFwidFwiOjB9IiwiaWF0IjoxNTcxNjk2NzU5fQ.5BDmWS_lq9pe56Iyv3MpUKl6smOqG_7ZGL_Ivnc1nh0";

        //private string permanentToken = @"eyJ0dCI6InAiLCJhbGciOiJIUzI1NiIsInR2IjoiMSJ9.eyJkIjoie1wiYVwiOjYwNTE2LFwiaVwiOjcxODgwODUsXCJjXCI6NDYyMDk5NCxcInVcIjo2MjU1NTg4LFwiclwiOlwiVVNcIixcInNcIjpbXCJXXCIsXCJGXCIsXCJJXCIsXCJVXCIsXCJLXCIsXCJDXCIsXCJEXCIsXCJNXCIsXCJBXCIsXCJMXCIsXCJQXCJdLFwielwiOltdLFwidFwiOjB9IiwiaWF0IjoxNTk3NzAxNDA5fQ.eLNkcM0A650ulcmrdQ1RdP-Y1yWy6K2bsAEsa9LhbNw";
        private string urlBase = "https://www.wrike.com/api/v4";


        public class clsField
        {
            public string id { get; set; }
            public string value { get; set; }
            public clsField(string id, string value)
            {
                this.id = id;
                this.value = value;
            }
        }
        public class clsFields
        {
            List<clsField> _fields = null;
            public clsFields()
            {
                _fields = new List<clsField>();
            }
            public void add(clsField item)
            {
                _fields.Add(item);
            }
            public string getJsonCustomFields()
            {
                return JsonConvert.SerializeObject(_fields);
            }
        }

        public enum fieldType
        {
            Text,
            DropDown,
            Numeric,
            Currency,
            Percentage,
            Date,
            Duration,
            Checkbox,
            Contacts,
            Multiple
        }
        public class eleCustomField
        {
            public string id { get; set; }
            public string accountId { get; set; }
            public string title { get; set; }
            public string type { get; set; }
        }
        public class responseCustomFields
        {
            public string kind { get; set; }
            public List<eleCustomField> data { get; set; }
        }


        public class eleFolder
        {
            public string id { get; set; }
            public string title { get; set; }
            public string color { get; set; }
            public List<string> childIds { get; set; }
        }
        public class responseFolders
        {
            public string kind { get; set; }
            public List<eleFolder> data { get; set; }
        }
        public class eleProfile
        {
            public string accountId { get; set; }
            public string email { get; set; }
        }
        public class eleContact
        {
            public string id { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string title { get; set; }
            public List<eleProfile> profiles { get; set; }

            public eleContact()
            {
                profiles = new List<eleProfile>();
            }
            public bool containsEmail(string email)
            {
                if (profiles != null)
                {
                    string _email = email.ToUpper();
                    foreach (eleProfile item in profiles)
                    {
                        if (item.email != null)
                            if (item.email.ToUpper() == _email)
                                return true;
                    }
                }
                return false;
            }
        }
        public class responseContacts
        {
            public string kind { get; set; }
            public List<eleContact> data { get; set; }
        }
        public enum TaskStatus
        {
            Active, Completed, Deferred, Cancelled
        }
        public class eleDates
        {
            public DateTime start { get; set; }
            public DateTime due { get; set; }
            public eleDates()
            {
                this.start = new DateTime();
                this.due = new DateTime();
            }
        }
        public class eleTask
        {
            public string id { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public TaskStatus status { get; set; }
            public eleDates dates { get; set; }
            public clsFields customFields { get; set; }
            public eleTask()
            {
                customFields = new clsFields();
            }
            public bool hasDates
            {
                get
                {
                    if (dates != null)
                        if (dates.start != new DateTime() || dates.due != new DateTime())
                            return true;
                    return false;
                }
            }
            public string datesToString()
            {
                if (dates != null)
                {
                    if (dates.start != new DateTime() && dates.due != new DateTime())
                    {
                        return @"{""start"":""" + dates.start.ToString("yyyy-MM-dd") + @""",""due"":""" + dates.due.ToString("yyyy-MM-dd") + @"""}";
                    }
                    else
                    {
                        if (dates.start != null)
                        {
                            return @"{""start"":""" + dates.start.ToString("yyyy-MM-dd") + @"""}";
                        }
                        else
                        {
                            return @"{""due"":""" + dates.due.ToString("yyyy-MM-dd") + @"""}";
                        }
                    }
                }
                return "";
            }
            public string getJsonCustomFields()
            {
                string strRet = customFields.getJsonCustomFields();
                return strRet;
            }
        }

        List<clsWrike.eleContact> _Contacts;
        public clsWrike()
        {
            _Contacts = null;
        }
        private List<clsWrike.eleContact> Contacts
        {
            get
            {
                if (_Contacts == null)
                {
                    //try read from disk
                    readContactsFromDisk();
                    //get from API
                    if (_Contacts == null)
                        _Contacts = getContacts();
                }
                return _Contacts;
            }
        }
        private void readContactsFromDisk()
        {
            try
            {
                string filename = ""; //Path.Combine(global.RootFolder, "contacts.data");
                if (File.Exists(filename))
                {
                    string strContent = File.ReadAllText(filename);
                    _Contacts = JsonConvert.DeserializeObject<List<clsWrike.eleContact>>(strContent);
                }
            }
            catch { }
        }
        public eleContact getContactByEmail(string prefix)
        {
            if (Contacts != null)
                return Contacts.Find(x => x.containsEmail(prefix));
            return null;
        }


        public void addTask(string folderId, eleTask task)
        {
            //https://developers.wrike.com/api/v4/tasks/#create-task

            string WrikeRestURI = urlBase + "/folders/" + folderId + @"/tasks";
            string parameters = @"title=" + HttpUtility.UrlEncode(task.title) +
                @"&description=" + HttpUtility.UrlEncode(task.description) +
                clsLibrary.iif(task.hasDates, @"&dates=" + HttpUtility.UrlEncode(task.datesToString()), "") +
                @"&customFields=" + HttpUtility.UrlEncode(task.getJsonCustomFields()) +
                @"&status=" + task.status.ToString();

            string strResponse = WebRequest("POST", WrikeRestURI, parameters);
            JObject response = JsonConvert.DeserializeObject<JObject>(strResponse);

            JArray data = (JArray)response["data"];
            JObject newTask = (JObject)data[0];
            task.id = clsLibrary.dBReadString(newTask["id"]);
        }

        public List<eleCustomField> getCustomFields()
        {
            string WrikeRestURI = urlBase + "/customfields";

            string parameters = "";
            string response = WebRequest("GET", WrikeRestURI, parameters);
            responseCustomFields oLista = JsonConvert.DeserializeObject<responseCustomFields>(response);
            return oLista.data;
        }

        public List<eleFolder> getFolders()
        {
            string WrikeRestURI = urlBase + "/folders";

            string parameters = "";
            string response = WebRequest("GET", WrikeRestURI, parameters);
            responseFolders oLista = JsonConvert.DeserializeObject<responseFolders>(response);
            return oLista.data;
        }
        public List<eleContact> getContacts()
        {
            string WrikeRestURI = urlBase + "/contacts";

            string parameters = "";
            string response = WebRequest("GET", WrikeRestURI, parameters);
            //save data locally for cache purpose
            responseContacts oLista = JsonConvert.DeserializeObject<responseContacts>(response);
            string filename = "";  //Path.Combine(global.RootFolder, "contacts.data");
            File.WriteAllText(filename, JsonConvert.SerializeObject(oLista.data));
            return oLista.data;
        }


        public string WebRequest(string method, string url, string parameters)
        {
            try
            {
                byte[] postData = Encoding.UTF8.GetBytes(parameters);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Method = method;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + permanentToken);
                request.ContentLength = postData.Length;
                request.Timeout = 600000;
                if (postData.Length > 0)
                {
                    Stream postStream = request.GetRequestStream();
                    postStream.Write(postData, 0, postData.Length);
                    postStream.Close();
                }

                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);

                string strResponse = reader.ReadToEnd();
                return strResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
