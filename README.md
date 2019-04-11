# Azure Active Directory Discovery Lab DNS Utility Portal
## Sample/Prototype project using Azure DNS to enable custom routable domains for use in an Azure AD Hybrid Lab
## Quick Start

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FMicrosoft%2Fazuread-discovery-lab-dns-utility%2Fmaster%2Fazuredeploy.json" target="_blank"><img src="http://azuredeploy.net/deploybutton.png"/></a>


__Details__

Using https://github.com/Microsoft/aad-hybrid-lab, an instructor can lead a class/lab to facilitate discovery of Azure AD Hybrid Identity using Azure AD Connect Sync. However, a publicly-routable DNS name must be used in order for users to sync properly with on-prem users. This utility uses one or more DNS zones, hosted in Azure DNS, to be shared by creating sub-zones for each student based on class size.

* Leverages Azure CosmosDB. For development, a downloadable emulator is available: https://aka.ms/documentdb-emulator
* ARM template deploys the following:
  * Azure Web App
  * Azure CosmosDB
  * Azure Storage Account
* Requires the following (see step-by-step deployment instructions above for details):
  1. Azure AD application (Admin) with the following:
    * Azure AD Graph - delegated permissions
      * Sign in and read user profile
  2. Azure AD application (Students) with the following:
    * Multi-Tenant enabled
    * Azure AD Graph - delegated permissions
      * Sign in and read user profile
    * Microsoft Graph - app permissions
      * Read and write domains (Domain.ReadWrite.All)
      * Read and write directory data (Directory.ReadWrite.All)

__Operation__

* Lab instructors log in and enable the application to access their resource group containing one or more DNS zones by granting RBAC "DNS ZONE CONTRIBUTOR" right to the Admin app above. Additionally, a Resource Group Tag "RootLabDomain: true" should be added to each zone.
* Lab instructors then schedule a lab session, selecting their resource group and indicating the number of students
* A web job kicks off and creates sub-zones equal to the number of students in the lab. If multiple zones are labeled in the resource group, the zones are allocated equally among them.
* A report or CVS file is generated with the zone names, a single unique "lab code" and a "team code" for each sub-zone.
* On lab day:
  * students are handed their "credentials" (the lab code and one of the team codes). 
  * They will create the AD VM using the lab link above and using the assigned sub-zone as their AD domain name. 
  * The student will then create a new Azure AD tenant, and log into this portal as a student using their new tenant credentials. They will consent to the student app above, which will allow the instructor to check the student's tenant for status throughout the session.
  * Using their team code, they will link their AAD tenant to their team DNS assignment. They can then go to Azure and validate their assigned domain in their tenant. They'll retrieve the DNS TXT validation code and, using this portal, add the TXT record to their assigned domain. They can then return to Azure and complete domain validation.

# As-Is Code

This code is made available as a sample utility. It should be customized by your dev team or a partner, and should be reviewed before being deployed in a production scenario.


# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.