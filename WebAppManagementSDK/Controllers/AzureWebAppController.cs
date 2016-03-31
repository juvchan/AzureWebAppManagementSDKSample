using System.Threading.Tasks;
using System.Web.Http;
using AzureWebAppManagementSDKDemo.WebAppManagement;
using Microsoft.Azure.Management.WebSites.Models;

namespace AzureWebAppManagementSDKDemo.Controllers
{
    [RoutePrefix("api/webapp")]
    public class AzureWebAppController : ApiController
    {
        private readonly WebAppManagementClient _webAppClient;

        public AzureWebAppController()
        {
            _webAppClient = new WebAppManagementClient();
        }

        /// <summary>
        /// List All Azure Web Apps from all Resource Groups under the current Azure subscription  
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("all")]
        public async Task<SiteCollection> GetWebAppsAsync()
        {
            return await _webAppClient.ListAllWebAppsAsync();
        }

        /// <summary>
        /// List All Azure Web Apps under a specific Resource Group   
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("all/resourcegroup/{resourceGroupName}")]
        public async Task<SiteCollection> GetWebAppsAsync(string resourceGroupName)
        {
            return await _webAppClient.ListAllWebAppsAsync(resourceGroupName);
        }

        /// <summary>
        /// List a specific web app under a specific resource group   
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{resourceGroupName}/{webAppName}")]
        public async Task<Site> GetWebAppAsync([FromUri] string resourceGroupName, [FromUri] string webAppName)
        {
            return await _webAppClient.ListWebAppAsync(resourceGroupName, webAppName);
        }

        /// <summary>
        /// List a specific web app under a specific resource group   
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("usagequota/{resourceGroupName}/{webAppName}")]
        public async Task<CsmUsageQuotaCollection> GetWebAppUsageQuotaAsync([FromUri] string resourceGroupName, [FromUri] string webAppName)
        {
            return await _webAppClient.GetWebAppUsageQuotaAsync(resourceGroupName, webAppName);
        }

        /// <summary>
        /// List all Configs under a specific web app under a specific resource group   
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("webAppConfig/{resourceGroupName}/{webAppName}")]
        public async Task<SiteConfig> GetWebAppConfigsAsync([FromUri] string resourceGroupName, [FromUri] string webAppName)
        {
            return await _webAppClient.GetWebAppConfigAsync(resourceGroupName, webAppName);
        }

        /// <summary>
        /// List all App Settings under a specific web app under a specific resource group   
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("appSettings/{resourceGroupName}/{webAppName}")]
        public async Task<StringDictionary> GetWebAppSettingsAsync([FromUri] string resourceGroupName, [FromUri] string webAppName)
        {
            return await _webAppClient.GetWebAppSettingsAsync(resourceGroupName, webAppName);
        }

        /// <summary>
        /// List all Slots under a specific web app under a specific resource group   
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("slots/{resourceGroupName}/{webAppName}")]
        public async Task<SiteCollection> GetWebAppSlotsAsync([FromUri] string resourceGroupName, [FromUri] string webAppName)
        {
            return await _webAppClient.GetWebAppSlotsAsync(resourceGroupName, webAppName);
        }

        /// <summary>
        /// List a specific App Service Plan (server Farm) under a specific resource group   
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("appServicePlan/{resourceGroupName}/{appServicePlanName}")]
        public async Task<ServerFarmWithRichSku> GetAppServicePlanAsync([FromUri] string resourceGroupName, [FromUri] string appServicePlanName)
        {
            return await _webAppClient.GetAppServicePlanAsync(resourceGroupName, appServicePlanName);
        }

        /// <summary>
        /// Get Publish Profile Xml for a web app under a specific resource group
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("publishProfile/{resourceGroupName}/{webAppName}")]
        public async Task<string> GetWebAppPublishProfileXmlAsync([FromUri] string resourceGroupName, [FromUri] string webAppName)
        {
            return await _webAppClient.GetPublishProfileXml(resourceGroupName, webAppName);
        }

        /// <summary>
        /// Create or Update a Web App for a new or existing resource group
        /// Using an existing App Service Plan (Free Tier)
        /// Add 2 new app settings: MyFirstKey, MySecondKey  
        /// Turn off PHP, which is enabled by default 
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("{resourceGroupName}/{resourceGroupLocation}/{webAppName}")]
        public async Task<Site> CreateOrUpdateWebAppAsync([FromUri] string resourceGroupName, [FromUri] string resourceGroupLocation, [FromUri] string webAppName)
        {
            return await _webAppClient.CreateOrUpdateWebAppAsync(resourceGroupName, resourceGroupLocation, webAppName);
        }

        /// <summary>
        /// Delete a Web App under a specific resource group
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("{resourceGroupName}/{webAppName}")]
        public async Task DeleteWebAppAsync([FromUri] string resourceGroupName, [FromUri] string webAppName)
        {
            await _webAppClient.DeleteWebAppAsync(resourceGroupName, webAppName);
        }
    }
}
