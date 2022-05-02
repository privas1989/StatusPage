using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StatusPage.Models;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;

namespace StatusPage.Services
{
    public class Office365Class
    {
        private string tenant_id;
        private string client_id;
        private string client_secret;
        private static string token;
        public string json;

        public Office365Class(string tenant_id, string client_id, string client_secret)
        {
            this.tenant_id = tenant_id;
            this.client_id = client_id;
            this.client_secret = client_secret;

            Task<string> response = GetO365Token(this.tenant_id, this.client_id, this.client_secret);
            response.Wait();

            Task<string> jsonReturned = GetO365JSON(response.Result);
            jsonReturned.Wait();
            this.json = jsonReturned.Result;
            token = response.Result;
        }

        public StatusModel GetServiceStatus(string service_name)
        {
            var status_color_dict = new Dictionary<string, int>();
            // Normal service = 0
            status_color_dict.Add("Normal service", 0);
            status_color_dict.Add("serviceOperational", 0);
            // Degredation = 1
            status_color_dict.Add("Service Degradation", 1);
            status_color_dict.Add("serviceDegradation", 1);
            // Outage = 2
            // Informational = 3
            status_color_dict.Add("serviceRestored", 3);
            status_color_dict.Add("extendedRecovery", 3);
            status_color_dict.Add("restoringService", 3);
            status_color_dict.Add("falsePositive", 3);
            status_color_dict.Add("investigating", 3);
            //status_color_dict.Add("Post-incident report published", 3);

            StatusModel o365Service = new StatusModel();

            try
            {
                JObject services = JObject.Parse(this.json);
                foreach (var service in services["value"])
                {
                    if (service["service"].ToString().Equals(service_name))
                    {
                        string stat = service["status"].ToString();
                        stat = string.Concat(stat.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
                        o365Service.Service = service["service"].ToString();
                        o365Service.StatusDescription = char.ToUpper(stat[0]) + stat.Substring(1);
                        o365Service.UpdateDT = DateTime.Now.ToString();
                        o365Service.StatusURL = "/Service/O365/" + service["service"].ToString();

                        if (service["status"].ToString().Equals("serviceOperational"))
                        {
                            o365Service.StatusDescription = "All Systems Operational";
                        }

                        if (status_color_dict.ContainsKey(service["status"].ToString()))
                        {
                            o365Service.Status = status_color_dict[service["status"].ToString()];
                        }
                        else
                        {
                            o365Service.Status = 2;
                        }

                    }

                }
            }
            catch
            {
                o365Service.Status = 2;
                o365Service.Service = "Office 365";
                o365Service.StatusDescription = "Service Degradation - Could not reach O365 API.";
                o365Service.UpdateDT = DateTime.Now.ToString();
            }
            return o365Service;
        }

        private static async Task<string> GetO365Token(string tenant_id, string client_id, string client_secret)
        {
            string uri = "https://login.microsoftonline.com/e30f5bdb-7f18-435b-8436-9d84aa7b96dd/oauth2/v2.0/token";
            string response;
            string token = null;

            var parameters = new Dictionary<string, string> {
                { "tenant", tenant_id},
                { "client_id", client_id },
                { "scope", "https://graph.microsoft.com/.default" },
                { "client_secret", client_secret },
                { "grant_type", "client_credentials" }
            };

            var encodedContent = new FormUrlEncodedContent(parameters);
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                var result = await httpClient.PostAsync(uri, encodedContent);
                response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            try
            {
                token = JObject.Parse(response)["access_token"].ToString();
            }
            catch { }

            return token;
        }

        private static async Task<string> GetO365JSON(string token)
        {
            string status = null;
            string uri = "https://graph.microsoft.com/v1.0/admin/serviceAnnouncement/healthOverviews";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var result = await httpClient.GetAsync(uri);

                if (result.Content != null)
                {
                    status = await result.Content.ReadAsStringAsync();
                }
            }

            return status;
        }

        public async Task<string> GetO365ServiceStatus(string service)
        {
            string status = null;
            string uri = "https://graph.microsoft.com/v1.0/admin/serviceAnnouncement/healthOverviews/" + service + "?$expand=issues($select=classification,title,impactDescription,status,isResolved,service,startDateTime,endDateTime)";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var result = await httpClient.GetAsync(uri);

                if (result.Content != null)
                {
                    status = await result.Content.ReadAsStringAsync();
                    Console.WriteLine(status);
                }
            }

            return status;
        }
    }
}