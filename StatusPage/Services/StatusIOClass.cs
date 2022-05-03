using System;
using System.Collections.Generic;
using System.Net.Http;
using StatusPage.Models;
using Newtonsoft.Json.Linq;

namespace StatusPage.Services
{
    public class StatusIOClass
    {
        public string json_url;
        public string status_url;
        public string service_name;
        private string json;

        public StatusIOClass(string service_name, string json_url, string status_url)
        {
            this.service_name = service_name;
            this.json_url = json_url;
            this.status_url = status_url;
        }

        private void GetJSON()
        {
            string json_retrieved = "";

            using (var httpClient = new HttpClient())
            {
                var result = httpClient.GetAsync(this.json_url);
                json_retrieved = result.Result.Content.ReadAsStringAsync().Result;
            }

            this.json = json_retrieved;
        }

        public StatusModel GetStatus()
        {
            StatusModel io_service = new StatusModel();
            io_service.Service = this.service_name;
            io_service.StatusURL = this.status_url;
            io_service.StatusDescription = "All Systems Operational";


            var status_dict = new Dictionary<int, string>();
            status_dict.Add(100, "All Systems Operational");
            status_dict.Add(300, "Service Degradation");
            status_dict.Add(400, "Partially Degraded Service");
            status_dict.Add(500, "Service Outage");
            status_dict.Add(600, "Security Event");

            var status_dict_color = new Dictionary<string, int>();
            status_dict_color.Add("100", 0);
            status_dict_color.Add("300", 1);
            status_dict_color.Add("400", 1);
            status_dict_color.Add("500", 2);
            status_dict_color.Add("600", 3);

            GetJSON();

            try
            {
                var json = JObject.Parse(this.json);
                io_service.StatusDescription = status_dict[Convert.ToInt32(json["result"]["status_overall"]["status_code"].ToString())];
                //io_service.StatusDescription = json["result"].ToString();

                DateTime convertedDate = DateTime.Parse(json["result"]["status_overall"]["updated"].ToString());
                DateTime dt = convertedDate.ToLocalTime();
                io_service.UpdateDT = dt.ToString();
                io_service.Status = status_dict_color[json["result"]["status_overall"]["status_code"].ToString()];

            }
            catch (Exception e)
            {
                io_service.StatusDescription = "Could not reach status.io.";
                io_service.UpdateDT = DateTime.Now.ToString();
            }

            return io_service;
        }
    }
}
