
namespace StatusPage.Models
{
    public class StatusModel
    {
        // The service display name
        public string Service { get; set; }

        /* 
         * 1 = 
         * 2 = 
         * 3 = 
         * 4 = 
        */
        public int Status { get; set; }

        // A readable status description
        public string StatusDescription { get; set; }

        // Time since status was last updated
        public string UpdateDT { get; set; }

        // Redirect URL for more information
        public string StatusURL { get; set; }
    }
}
