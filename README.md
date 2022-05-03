# Status Page
![Employee data](/Images/example.jpg?raw=true "Employee Data title")

A generic status page that pulls data from various sources used within an organization. This webapp uses .NET Core 3.1.

## This repo
This repo contains a .NET Core 3.1 project that creates a status page web application. 
It creates web calls that collects status information from Office 365, Adobe CC, Google GSuite, and other status pages and combines their output
into an easy to read site.

## Customization
This project outputs a table element with a service in each row based off of the services in the appsettings.json file. The overall look of the table can be modified under the Views/Home/index.cshtml filwe. The default project is running Bootstrap 5 as its CSS framework but can be changed under the Views/Shared/_Layout.cshtml file. 

## Configuration
All of the configurations can be done through the appsettings.json file. To remove a service, simply delete the subsection from the appsettings.json file.

### Office 365
- Create your Azure AD service account and apply the correct MS Graph permissions as required. Note your client ID and secret for the next step. Follow this link for more details: https://docs.microsoft.com/en-us/graph/api/resources/servicehealth?view=graph-rest-1.0
- Grab your tenant ID by using these directions: https://docs.microsoft.com/en-us/azure/active-directory/fundamentals/active-directory-how-to-find-tenant
- Insert your tenant ID, client ID, and secret password under the Office365 section of your appsettings.json file.

### Google Apps
This webapp comes with default applications that are monitored. 
If you want to add more, such as Google Classroom or Google Voice, add them to the "monitoredApplications" section under "GoogleApps" with the appsettings.json file.
To remove any applications, delete them from the same array. A list can be found here: https://www.google.com/appsstatus/dashboard/summary

### Adobe CC
I've programmed the Adobe CC section to display critical information first, followed by partial degradation and more informational status.
To change the way this works, you will need to modify the Services/AdobeClass.cs file and modify the logic as you want.

### StatusDotJson (status.json)
With my research, I noticed that many vendors follow a template when developing a status page. Most of which will output a status.json file.
If one of your services follows this template, you can add the service under the StatusDotJSON section within the appsettings.json file.
The array value under each service follows the following pattern: ["Display name", "URL for status.json", "The redirect URL when clicking the service"].
