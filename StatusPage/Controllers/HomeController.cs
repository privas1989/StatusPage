using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StatusPage.Models;
using StatusPage.Services;
using Microsoft.Extensions.Configuration;
using StatusPage.StatusClass;

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

            if (_config.GetSection("StatusDotJSON").Exists())
            {
                var svc_list = _config.GetSection("StatusDotJSON").Get<StatusDotJsonConfClass[]>();

                foreach (var svc_json in svc_list)
                {
                    StatusDotJsonClass svc = new StatusDotJsonClass(
                        svc_json.Service[0],
                        svc_json.Service[1], 
                        svc_json.Service[2]);
                    
                    statusList.Add(svc.GetStatus());
                }
            }

            if (_config.GetSection("StatusIO").Exists())
            {
                var svc_list = _config.GetSection("StatusIO").Get<StatusDotJsonConfClass[]>();

                foreach (var svc_json in svc_list)
                {
                    StatusIOClass svc = new StatusIOClass(
                        svc_json.Service[0],
                        svc_json.Service[1],
                        svc_json.Service[2]);

                    statusList.Add(svc.GetStatus());
                }
            }

            bucket.StatusList = statusList;

            return View(bucket);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
