using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StatusPage.Models;
using StatusPage.Services;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

namespace StatusPage.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public IActionResult Index()
        {
            StatusCollectionModel bucket = new StatusCollectionModel();
            List<StatusModel> statusList = new List<StatusModel>();          

            statusList.Add(GetStatusAsync("https://status.instructure.com/", "https://status.instructure.com/api/v2/status.json", "Canvas").Result);
            statusList.Add(GetStatusAsync("https://status.duo.com/", "https://status.duo.com/api/v2/status.json", "Duo MFA").Result);

            statusList.Add(GetStatusAsync("https://status.zoom.us/", "https://status.zoom.us/api/v2/status.json", "Zoom").Result);
            statusList.Add(GetStatusAsync("https://status.slack.com/", "https://status.slack.com/api/v2.0.0/current", "Slack").Result);

            if (_config.GetSection("Office365").Exists())
            {
                Office365Class O365 = new Office365Class(
                _config.GetSection("Office365").GetValue("tenant", ""),
                _config.GetSection("Office365").GetValue("clientID", ""),
                _config.GetSection("Office365").GetValue("clientSecret", ""));

                var o365_apps = _config.GetSection("Office365:monitoredApplications").Get<string[]>();

                foreach (string O365_app in o365_apps)
                {
                    statusList.Add(O365.GetServiceStatus(O365_app));
                }
            }

            if (_config.GetSection("GoogleApps").Exists())
            {
                string status_url = _config.GetSection("GoogleApps:statusURL").Value;
                string json_url = _config.GetSection("GoogleApps:jsonURL").Value;

                GoogleClass google = new GoogleClass(status_url, json_url);

                var google_apps = _config.GetSection("GoogleApps:monitoredApplications").Get<string[]>();

                foreach (string google_app in google_apps)
                {
                    statusList.Add(google.GetServiceStatus(google_app));
                }
            }

            if (_config.GetSection("AdobeCC").Exists())
            {
                string status_url = _config.GetSection("AdobeCC:statusURL").Value;
                string json_url = _config.GetSection("AdobeCC:jsonURL").Value;
                string display_name = _config.GetSection("AdobeCC:displayName").Value;

                AdobeClass adobe = new AdobeClass(display_name, json_url, status_url);

                statusList.Add(adobe.GetStatus());
            }

            StatusIOClass echo_ci = new StatusIOClass("https://status.io/1.0/status/589a53b1243c30490e000feb", "https://omniupdate.status.io/", "EchoCI");
            statusList.Add(echo_ci.GetServiceStatus());

            bucket.StatusList = statusList;

            return View(bucket);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<StatusModel> GetStatusAsync(string info_url, 
            string status_json_url,
            string service_name)
        {
            StatusModel service = new StatusModel();
            service.StatusURL = info_url;
            service.Service = service_name;
            string result;

            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(status_json_url))
                {
                    using (var content = response.Content)
                    {
                        result = await content.ReadAsStringAsync();
                    }
                }
            }

            // Slack output https://api.slack.com/docs/slack-status
            if (status_json_url.Equals("https://status.slack.com/api/v2.0.0/current"))
            {
                try
                {
                    var JSON = JObject.Parse(result);
                    if (JSON["status"].ToString().Equals("ok"))
                    {
                        service.StatusDescription = "All Systems Operational";
                        service.UpdateDT = JSON["date_updated"].ToString();
                        service.Status = 0;
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
                                service.UpdateDT = incident["date_updated"].ToString();
                            }
                        }

                        if (level == 1)
                        {
                            service.StatusDescription = "Notice";
                            service.Status = -1;
                        }
                        else if (level == 2)
                        {
                            service.StatusDescription = "Partially Degraded Service";
                            service.Status = 1;
                        }
                        else if (level == 3)
                        {
                            service.StatusDescription = "Service Outage";
                            service.Status = 2;
                        }
                    }
                }
                catch
                {
                    service.StatusDescription = "Unable to reach " + service_name + " API.";
                    service.UpdateDT = DateTime.Now.ToString();
                }
            }
            // Generic status.json
            else
            {
                try
                {
                    var JSON = JObject.Parse(result);
                    service.Service = service_name;
                    service.StatusDescription = JSON["status"]["description"].ToString();
                    service.UpdateDT = JSON["page"]["updated_at"].ToString();

                    if (JSON["status"]["indicator"].ToString().Equals("none"))
                    {
                        service.Status = 0;
                    }
                    else if (JSON["status"]["indicator"].ToString().Equals("minor"))
                    {
                        service.Status = 1;
                    }
                    else
                    {
                        service.Status = 2;
                    }
                }
                catch
                {
                    service.Service = service_name;
                    service.StatusDescription = "Unable to reach " + service_name + " API.";
                    service.UpdateDT = DateTime.Now.ToString();
                }
            }

            return service;
        }
    }
}
