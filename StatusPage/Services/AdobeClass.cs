using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using StatusPage.Models;
using Newtonsoft.Json.Linq;

namespace StatusPage.Services
{
    public class AdobeClass
    {
        private string adobe_json_url;
        private string adobe_status_url;
        private string adobe_json;
        private string service_name;

        public AdobeClass(string service_name, string adobe_json_url, string adobe_status_url)
        {
            this.adobe_json_url = adobe_json_url;
            this.adobe_status_url = adobe_status_url;
            this.service_name = service_name;
            this.adobe_json = GetAdobeJSON(adobe_json_url).Result;
        }

        public StatusModel GetStatus()
        {
            StatusModel adobe_status = new StatusModel();
            adobe_status.Service = this.service_name;
            adobe_status.StatusURL = this.adobe_status_url;
            adobe_status.UpdateDT = DateTime.Now.ToString();

            if (this.adobe_json != null)
            {
                try
                {
                    JObject adobe_jo = JObject.Parse(this.adobe_json);
                    JArray adobe_products = (JArray)adobe_jo["products"];

                    List<int> event_ids = new List<int>();

                    foreach (JObject product in adobe_products)
                    {
                        if (product["ongoing"] != null)
                        {
                            JArray ongoing_outages = (JArray)product["ongoing"];
                            foreach (JObject outage in ongoing_outages)
                            {
                                if (outage["eventStatus"].ToString().Equals("3") && outage["eventType"].ToString().Equals("1") && outage["eventState"].ToString().Equals("1"))
                                {
                                    event_ids.Add(3);
                                }
                                else if (outage["eventStatus"].ToString().Equals("4") && outage["eventType"].ToString().Equals("1") && outage["eventState"].ToString().Equals("4"))
                                {
                                    event_ids.Add(9);
                                }
                                else if (outage["eventStatus"].ToString().Equals("3") && outage["eventType"].ToString().Equals("4") && outage["eventState"].ToString().Equals("1"))
                                {
                                    event_ids.Add(7);
                                }
                                else if (outage["eventStatus"].ToString().Equals("8") && outage["eventType"].ToString().Equals("5") && outage["eventState"].ToString().Equals("7"))
                                {
                                    event_ids.Add(8);
                                }
                                else if (outage["eventStatus"].ToString().Equals("2") && outage["eventType"].ToString().Equals("4") && outage["eventState"].ToString().Equals("2"))
                                {
                                    event_ids.Add(10);
                                }
                                else if (outage["eventStatus"].ToString().Equals("5") && outage["eventType"].ToString().Equals("1") && outage["eventState"].ToString().Equals("5"))
                                {
                                    event_ids.Add(5);
                                }
                                else if (outage["eventStatus"].ToString().Equals("1") && outage["eventType"].ToString().Equals("5") && outage["eventState"].ToString().Equals("1"))
                                {
                                    event_ids.Add(1);
                                }
                                else if (outage["eventStatus"].ToString().Equals("6") && outage["eventType"].ToString().Equals("4") && outage["eventState"].ToString().Equals("4"))
                                {
                                    event_ids.Add(6);
                                }
                            }
                        }
                        else
                        {
                            adobe_status.Status = 0;
                            adobe_status.StatusDescription = "All Systems Operational";
                        }
                    }

                    event_ids = event_ids.Distinct().ToList();

                    // 1  = discovery
                    // 2  = medium
                    // 3  = major
                    // 4  = resolved
                    // 5  = incident canceled
                    // 6  = maintenance complete
                    // 7  = maintenance started
                    // 8  = incident dismissed
                    // 9  = major incident resolved
                    // 10 = minor incident resolved
                    if (event_ids.Contains(3))
                    {
                        adobe_status.Status = 2;
                        adobe_status.StatusDescription = "Service Outage";
                    }
                    else if (event_ids.Contains(2))
                    {
                        adobe_status.Status = 1;
                        adobe_status.StatusDescription = "Partially Degraded Service";
                    }
                    else if (event_ids.Contains(1))
                    {
                        adobe_status.Status = 3;
                        adobe_status.StatusDescription = "Discovered Incident";
                    }
                    else if (event_ids.Contains(4))
                    {
                        adobe_status.Status = 3;
                        adobe_status.StatusDescription = "Incident Resolved";
                    }
                    else if (event_ids.Contains(9))
                    {
                        adobe_status.Status = 3;
                        adobe_status.StatusDescription = "Major Incident Resolved";
                    }
                    else if (event_ids.Contains(10))
                    {
                        adobe_status.Status = 3;
                        adobe_status.StatusDescription = "Minor Incident Resolved";
                    }
                    else if (event_ids.Contains(5))
                    {
                        adobe_status.Status = 3;
                        adobe_status.StatusDescription = "Incident Identified and Cancelled";
                    }
                    else if (event_ids.Contains(6))
                    {
                        adobe_status.Status = 3;
                        adobe_status.StatusDescription = "Maintenance Complete";
                    }
                    else if (event_ids.Contains(7))
                    {
                        adobe_status.Status = 3;
                        adobe_status.StatusDescription = "Maintenance in Progress";
                    }
                    else if (event_ids.Contains(8))
                    {
                        adobe_status.Status = 3;
                        adobe_status.StatusDescription = "Incident Identified and Dismissed";
                    }
                }
                catch (Exception) { }
            }

            return adobe_status;
        }

        private async Task<string> GetAdobeJSON(string uri)
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
