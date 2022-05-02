using System;
using System.Net.Http;
using StatusPage.Models;
using Newtonsoft.Json.Linq;

namespace StatusPage.Services
{
    public class GoogleClass
    {
        public string json_url;
        public string status_url;
        private string json;

        public GoogleClass(string status_url, string json_url)
        {
            this.status_url = status_url;
            this.json_url = json_url;
        }

        private void GetGoogleJSONHistory()
        {
            string json_retrieved = "";

            using (var httpClient = new HttpClient())
            {
                var result = httpClient.GetAsync("https://www.google.com/appsstatus/dashboard/incidents.json");
                json_retrieved = result.Result.Content.ReadAsStringAsync().Result;
            }

            this.json = json_retrieved;
        }

        public StatusModel GetServiceStatus(string service_name)
        {
            StatusModel google_service = new StatusModel();
            google_service.Service = service_name;
            google_service.StatusURL = this.status_url;
            google_service.StatusDescription = "All Systems Operational";
            google_service.Status = 0;
            google_service.UpdateDT = DateTime.Now.ToString();
            GetGoogleJSONHistory();

            try
            {
                JArray google_json = JArray.Parse(this.json);

                foreach (JObject incident in google_json)
                {
                    if (DateTime.Now >= DateTime.Parse(incident["begin"].ToString()) && DateTime.Now < DateTime.Parse(incident["end"].ToString()) ||
                        DateTime.Now >= DateTime.Parse(incident["begin"].ToString()) && String.IsNullOrEmpty(incident["end"].ToString())
                        && incident["service_name"].ToString().Equals(service_name))
                    {
                        if (incident["severity"].ToString().Equals("low"))
                        {
                            google_service.StatusDescription = "Minimal Degraded Service";
                            google_service.Status = 1;
                            google_service.UpdateDT = DateTime.Parse(incident["modified"].ToString()).ToString();
                        }
                        else if (incident["severity"].ToString().Equals("medium"))
                        {
                            google_service.StatusDescription = "Partially Degraded Service";
                            google_service.Status = 1;
                            google_service.UpdateDT = DateTime.Parse(incident["modified"].ToString()).ToString();
                        }
                        else if (incident["severity"].ToString().Equals("high"))
                        {
                            google_service.StatusDescription = "Service degradation";
                            google_service.Status = 1;
                            google_service.UpdateDT = DateTime.Parse(incident["modified"].ToString()).ToString();
                        }
                    }
                }
            }
            catch
            {
                google_service.StatusDescription = "Could not reach Google API.";
                google_service.Status = -1;
            }
            

            return google_service;
        }
    }
}
