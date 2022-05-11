using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OWPApplications.Utils
{
    public class WrikeAPI
    {

        private string _wrikeBearerToken;


        public WrikeAPI()
            : this("eyJ0dCI6InAiLCJhbGciOiJIUzI1NiIsInR2IjoiMSJ9.eyJkIjoie1wiYVwiOjYwNTE2LFwiaVwiOjY2NjI0ODEsXCJjXCI6NDYxNDQ2OCxcInVcIjo2NDEyNDYyLFwiclwiOlwiVVNcIixcInNcIjpbXCJXXCIsXCJGXCIsXCJJXCIsXCJVXCIsXCJLXCIsXCJDXCIsXCJBXCIsXCJMXCJdLFwielwiOltdLFwidFwiOjB9IiwiaWF0IjoxNTcxNjk2NzU5fQ.5BDmWS_lq9pe56Iyv3MpUKl6smOqG_7ZGL_Ivnc1nh0")
        {

        }

        public WrikeAPI(string AccessToken)
        {
            _wrikeBearerToken = AccessToken;
        }


        public async Task<WrikeFolderTree[]> WrikeFoldersTree()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _wrikeBearerToken);

                var response = await client.GetStringAsync("https://www.wrike.com/api/v4/folders");

                var reply = JsonConvert.DeserializeObject<WrikeFolderTreeReply>(response);

                return reply.data;
            }
        }

        public async void CreateTask(WrikeTask Task)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _wrikeBearerToken);

                var response = await client.PostAsJsonAsync("", JsonConvert.SerializeObject(Task));

            }
        }

    }
}
