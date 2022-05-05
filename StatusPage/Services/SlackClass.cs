using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StatusPage.Models;

namespace StatusPage.Services
{
    public class SlackClass
    {
        private string json_url;
        private string status_url;
        private string json;
        private string display_name;

        public SlackClass(string display_name, string json_url, string status_url)
        {
            this.display_name = display_name;
            this.json_url = json_url;
            this.status_url = status_url;
            this.json = GetJSON(json_url).Result;
        }

        public StatusModel GetStatus()
        {
            StatusModel slack_status = new StatusModel();
            slack_status.Service = this.display_name;
            slack_status.StatusURL = this.status_url;
            slack_status.UpdateDT = DateTime.Now.ToString();

            if (this.json != null)
            {
                try
                {
                    var JSON = JObject.Parse(this.json);
                    if (JSON["status"].ToString().Equals("ok"))
                    {
                        slack_status.StatusDescription = "All Systems Operational";
                        slack_status.UpdateDT = JSON["date_updated"].ToString();
                        slack_status.Status = 0;
                    }
                    else
                    {
                        var incidents = JArray.Parse(JSON["active_incidents"].ToString());

                        // loop through the types and break at the worst type
                        int level = 0;
                        foreach (var incident in incidents)
                        {
                            int this_level = 0;

                            if (incident["type"].ToString().Equals("notice") && incident["status"].ToString().Equals("active"))
                            {
                                this_level = 1;
                            }
                            else if (incident["type"].ToString().Equals("incident") && incident["status"].ToString().Equals("active"))
                            {
                                this_level = 2;
                            }
                            else if (incident["type"].ToString().Equals("outage") && incident["status"].ToString().Equals("active"))
                            {
                                this_level = 3;
                            }


                            if (this_level > level)
                            {
                                level = this_level;
                                slack_status.UpdateDT = incident["date_updated"].ToString();
                            }
                        }

                        if (level == 1)
                        {
                            slack_status.StatusDescription = "Notice";
                            slack_status.Status = -1;
                        }
                        else if (level == 2)
                        {
                            slack_status.StatusDescription = "Partially Degraded Service";
                            slack_status.Status = 1;
                        }
                        else if (level == 3)
                        {
                            slack_status.StatusDescription = "Service Outage";
                            slack_status.Status = 2;
                        }
                    }
                }
                catch
                {
                    slack_status.StatusDescription = "Unable to reach " + this.display_name + " API.";
                    slack_status.UpdateDT = DateTime.Now.ToString();
                }
            }

            return slack_status;
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
