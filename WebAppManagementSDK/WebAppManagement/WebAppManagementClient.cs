using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.Azure.Common.Authentication.Models;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.Azure.Management.WebSites;
using Microsoft.Azure.Management.WebSites.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure;

namespace AzureWebAppManagementSDKDemo.WebAppManagement
{
    internal class WebAppManagementClient
    {
        private readonly AuthenticationContext _authContext;
        private readonly ClientCredential _credential;
        private readonly string _subscriptionId;
        private readonly AzureEnvironment _azureEnvironment;
        private readonly WebSiteManagementClient _webAppManagementClient;
        private readonly ResourceManagementClient _resourceManagementClient;

        internal WebAppManagementClient()
        {
            var tenantId = ConfigurationManager.AppSettings["AAD.TenantId"];
            _subscriptionId = ConfigurationManager.AppSettings["AzureSubscriptionId"];
            var clientId = ConfigurationManager.AppSettings["AAD.ClientId"];
            var clientSecret = ConfigurationManager.AppSettings["AAD.ClientSecret"];

            // Set Environment - Choose between Azure public cloud, china cloud and US govt. cloud
            _azureEnvironment = AzureEnvironment.PublicEnvironments[EnvironmentName.AzureCloud];
            var authority = $"{_azureEnvironment.Endpoints[AzureEnvironment.Endpoint.ActiveDirectory]}{tenantId}";
            _authContext = new AuthenticationContext(authority);
            _credential = new ClientCredential(clientId, clientSecret);

            var tokenCloudCredential = GetTokenCloudCredential();
            var tokenCredential = new TokenCredentials(tokenCloudCredential.Token);
            _webAppManagementClient = new WebSiteManagementClient(_azureEnvironment.GetEndpointAsUri(AzureEnvironment.Endpoint.ResourceManager), tokenCredential)
            {
                SubscriptionId = _subscriptionId
            };
            _resourceManagementClient = new ResourceManagementClient(_azureEnvironment.GetEndpointAsUri(AzureEnvironment.Endpoint.ResourceManager), tokenCredential)
            {
                SubscriptionId = _subscriptionId
            };
        }

        internal async Task<SiteCollection> ListAllWebAppsAsync()
        {
            try
            {
                var webAppsResult = await _webAppManagementClient.GlobalModel.GetAllSitesAsync().ConfigureAwait(false);
                return webAppsResult;
            }
            catch (CloudException cex)
            {
                var error = $"{cex.GetType().FullName}: {cex.Body.Code} : {cex.Body.Message}";
                throw new CloudException(error);
            }
        }

        internal async Task<SiteCollection> ListAllWebAppsAsync(string resourceGroupName)
        {
            try
            {
                var webAppsResult =
                    await _webAppManagementClient.Sites.GetSitesAsync(resourceGroupName, null).ConfigureAwait(false);

                return webAppsResult;
            }
            catch (CloudException cex)
            {
                var error = $"{cex.GetType().FullName}: {cex.Body.Code} : {cex.Body.Message}";
                throw new CloudException(error);
            }
        }

        internal async Task<Site> ListWebAppAsync(string resourceGroupName, string webAppName)
        {
            try
            {
                var webAppResult =
                    await
                        _webAppManagementClient.Sites.GetSiteAsync(resourceGroupName, webAppName).ConfigureAwait(false);
                return webAppResult;
            }
            catch (CloudException cex)
            {
                var error = $"{cex.GetType().FullName}: {cex.Body.Code} : {cex.Body.Message}";
                throw new CloudException(error);
            }
        }

        internal async Task<SiteCollection> GetWebAppSlotsAsync(string resourceGroupName, string webAppName)
        {
            try
            {
                var webAppSlotResult =
                    await
                        _webAppManagementClient.Sites.GetSiteSlotsAsync(resourceGroupName, webAppName).ConfigureAwait(false);
                return webAppSlotResult;
            }
            catch (CloudException cex)
            {
                var error = $"{cex.GetType().FullName}: {cex.Body.Code} : {cex.Body.Message}";
                throw new CloudException(error);
            }
        }

        internal async Task<SiteConfig> GetWebAppConfigAsync(string resourceGroupName, string webAppName)
        {
            try
            {
                var webAppConfigResult =
                    await
                        _webAppManagementClient.Sites.GetSiteConfigAsync(resourceGroupName, webAppName).ConfigureAwait(false);
                return webAppConfigResult;
            }
            catch (CloudException cex)
            {
                var error = $"{cex.GetType().FullName}: {cex.Body.Code} : {cex.Body.Message}";
                throw new CloudException(error);
            }
        }

        internal async Task<StringDictionary> GetWebAppSettingsAsync(string resourceGroupName, string webAppName)
        {
            try
            {
                var appSettingsResult =
                    await
                        _webAppManagementClient.Sites.ListSiteAppSettingsAsync(resourceGroupName, webAppName)
                            .ConfigureAwait(false);
                return appSettingsResult;
            }
            catch (CloudException cex)
            {
                var error = $"{cex.GetType().FullName}: {cex.Body.Code} : {cex.Body.Message}";
                throw new CloudException(error);
            }
        }

        internal async Task<CsmUsageQuotaCollection> GetWebAppUsageQuotaAsync(string resourceGroupName, string webAppName)
        {
            try
            {
                var webAppUsageQuotaResult =
                    await
                        _webAppManagementClient.Sites.GetSiteUsagesAsync(resourceGroupName, webAppName).ConfigureAwait(false);
                return webAppUsageQuotaResult;
            }
            catch (CloudException cex)
            {
                var error = $"{cex.GetType().FullName}: {cex.Body.Code} : {cex.Body.Message}";
                throw new CloudException(error);
            }
        }

        internal async Task<ServerFarmWithRichSku> GetAppServicePlanAsync(string resourceGroupName, string appServicePlanName)
        {
            try
            {
                var appServicePlanResult =
                    await
                        _webAppManagementClient.ServerFarms.GetServerFarmAsync(resourceGroupName, appServicePlanName)
                            .ConfigureAwait(false);
                return appServicePlanResult;
            }
            catch (CloudException cex)
            {
                var error = $"{cex.GetType().FullName}: {cex.Body.Code} : {cex.Body.Message}";
                throw new CloudException(error);
            }
        }

        internal async Task<Site> CreateOrUpdateWebAppAsync(string resourceGroupName, string resourceGroupLocation, string webAppName)
        {
            try
            {
                var isRgExist = await IsResourceGroupExistsAsync(resourceGroupName).ConfigureAwait(false);

                if (!isRgExist.HasValue || isRgExist.Value == false)
                {
                    var rgParam = new ResourceGroup() { Name = resourceGroupName, Location = resourceGroupLocation };
                    await
                        _resourceManagementClient.ResourceGroups.CreateOrUpdateAsync(resourceGroupName, rgParam).ConfigureAwait(false);
                }

                // Existing Free Tier - App Service Plan
                var webAppLocation = resourceGroupLocation;
                var appServicePlanName = "AzureWebAppManagementSDKDemoPlan";

                // Create/Update the Web App
                var webAppProperties = new Site
                {
                    Location = webAppLocation,
                    ServerFarmId = appServicePlanName
                };

                var webApp = await _webAppManagementClient.Sites.CreateOrUpdateSiteAsync(resourceGroupName, webAppName, webAppProperties).ConfigureAwait(false);

                // Create/Update the Website configuration
                var webAppConfig = new SiteConfig
                {
                    Location = webAppLocation,
                    PhpVersion = ""
                };
                await _webAppManagementClient.Sites.CreateOrUpdateSiteConfigAsync(resourceGroupName, webAppName, webAppConfig);

                // Create/Update some App Settings
                var appSettings = new StringDictionary
                {
                    Location = webAppLocation,
                    Properties = new Dictionary<string, string>
                {
                    { "MyFirstKey", "My first value" },
                    { "MySecondKey", "My second value" }
                }
                };
                await _webAppManagementClient.Sites.UpdateSiteAppSettingsAsync(resourceGroupName, webAppName, appSettings);

                // Restart the web app
                await _webAppManagementClient.Sites.RestartSiteAsync(resourceGroupName, webAppName, true);

                return webApp;
            }
            catch (CloudException cex)
            {
                var error = $"{cex.GetType().FullName}: {cex.Body.Code} : {cex.Body.Message}";
                throw new CloudException(error);
            }
        }

        internal async Task DeleteWebAppAsync(string resourceGroupName, string webAppName)
        {
            try
            {
                await _webAppManagementClient.Sites.DeleteSiteAsync(resourceGroupName, webAppName).ConfigureAwait(false);
            }
            catch (CloudException cex)
            {
                var error = $"{cex.GetType().FullName}: {cex.Body.Code} : {cex.Body.Message}";
                throw new CloudException(error);
            }
        }

        internal async Task<string> GetPublishProfileXml(string resourceGroupName, string webAppName)
        {
            try
            {
                // Default option will return both MSDeploy and FTP credential
                var ppOptions = new CsmPublishingProfileOptions();
                using (var webAppPublishProfileMemoryStream = await _webAppManagementClient.Sites.ListSitePublishingProfileXmlAsync(resourceGroupName, webAppName, ppOptions).ConfigureAwait(false))
                {
                    var sr = new StreamReader(webAppPublishProfileMemoryStream);
                    var webAppPublishProfileXml = await sr.ReadToEndAsync().ConfigureAwait(false);
                    return webAppPublishProfileXml;
                }
                
            }
            catch (CloudException cex)
            {
                var error = $"{cex.GetType().FullName}: {cex.Body.Code} : {cex.Body.Message}";
                throw new CloudException(error);
            }
        }

        private async Task<bool?> IsResourceGroupExistsAsync(string rgName)
        {
            try
            {
                var isExistResult =
                    await _resourceManagementClient.ResourceGroups.CheckExistenceAsync(rgName).ConfigureAwait(false);
                return isExistResult;
            }
            catch (CloudException cex)
            {
                var error = $"{cex.GetType().FullName}: {cex.Body.Code} : {cex.Body.Message}";
                throw new CloudException(error);
            }
        }

        private TokenCloudCredentials GetTokenCloudCredential()
        {
            try
            {
                var authResult = _authContext.AcquireToken(_azureEnvironment.Endpoints[AzureEnvironment.Endpoint.ActiveDirectoryServiceEndpointResourceId], _credential);
                return new TokenCloudCredentials(_subscriptionId, authResult.AccessToken);
            }
            catch (AdalException adalex)
            {
                var error = $"{adalex.GetType().FullName}: {adalex.Message}";
                throw new AdalException(adalex.ErrorCode, error);
            }
        }
    }
}