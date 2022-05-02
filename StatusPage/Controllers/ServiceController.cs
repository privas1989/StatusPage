using System;
using System.Collections.Generic;
using StatusPage.Models;
using StatusPage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
namespace StatusPage.Controllers
{
    public class ServiceController : Controller
    {
        private readonly IConfiguration _config;

        public ServiceController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult O365(string id)
        {
            StatusCollectionModel bucket = new StatusCollectionModel();
            bucket.CollectionName = "<a href=\"/\">All Services</a><span style=\"color: #cb132a;\"> / </span>" + id + " Details";
            List<StatusModel> statusList = new List<StatusModel>();

            Office365Class o365 = new Office365Class(
                _config.GetSection("Office365").GetValue("tenant", ""),
                _config.GetSection("Office365").GetValue("clientID", ""),
                _config.GetSection("Office365").GetValue("clientSecret", ""));

            try
            {
                JObject services = JObject.Parse(o365.json);
                foreach (var service in services["value"])
                {
                    if (service["WorkloadDisplayName"].ToString().Equals(id))
                    {
                        JArray features = JArray.Parse(service["FeatureStatus"].ToString());
                        foreach (var feature in features)
                        {
                            StatusModel feat = new StatusModel();
                            feat.Service = feature["FeatureDisplayName"].ToString();
                            feat.StatusDescription = feature["FeatureServiceStatusDisplayName"].ToString();
                            feat.UpdateDT = DateTime.Now.ToString();
                            statusList.Add(feat);
                        }
                    }
                }
            }
            catch { }

            bucket.StatusList = statusList;

            return View("Index", bucket);
        }        
    }
}
