using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StatusPage.Models;

namespace StatusPage.Services
{
    public class StatusDotJsonClass
    {
        private string json_url;
        private string status_url;
        private string json;
        private string display_name;

        public StatusDotJsonClass(string display_name, string json_url, string status_url)
        {
            this.display_name = display_name;
            this.json_url = json_url;
            this.status_url = status_url;
            this.json = GetJSON(json_url).Result;
        }

        public StatusModel GetStatus()
        {
            StatusModel status_dot_json = new StatusModel();
            status_dot_json.Service = this.display_name;
            status_dot_json.StatusURL = this.status_url;
            status_dot_json.UpdateDT = DateTime.Now.ToString();

            if (this.json != null)
            {
                try
                {
                    var JSON = JObject.Parse(this.json);
                    status_dot_json.Service = display_name;
                    status_dot_json.StatusDescription = JSON["status"]["description"].ToString();
                    status_dot_json.UpdateDT = JSON["page"]["updated_at"].ToString();

                    if (JSON["status"]["indicator"].ToString().Equals("none"))
                    {
                        status_dot_json.Status = 0;
                    }
                    else if (JSON["status"]["indicator"].ToString().Equals("minor"))
                    {
                        status_dot_json.Status = 1;
                    }
                    else
                    {
                        status_dot_json.Status = 2;
                    }
                }
                catch
                {
                    status_dot_json.Service = display_name;
                    status_dot_json.StatusDescription = "Unable to reach " + display_name + " API.";
                    status_dot_json.UpdateDT = DateTime.Now.ToString();
                }
            }

            return status_dot_json;
        }
        private async Task<string> GetJSON(string uri)
        {
            string json = null;

            using (var httpClient = new HttpClient())
            {
                var result = await httpClient.GetAsync(uri);

                if (result.Content != null)
                {
                    json = await result.Content.ReadAsStringAsync();
                }
            }

            return json;
        }
    }
}
