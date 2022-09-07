using System.Threading.Tasks;
using Pulumi;
using AzureNative = Pulumi.AzureNative;
// using Azure = Pulumi.Azure;
using Tls = Pulumi.Tls;
using Random = Pulumi.Random;



class MyStack : Stack
{
    public MyStack()
    {
        var config = new Pulumi.Config();
        var location = config.Get("azure-native:location") ?? "westeurope";
        var commonArgs = new LandingZoneArgs("Core", location, "shd");
        var resourceGroupName = $"rg-{commonArgs.Application}-{commonArgs.LocationShort}-{commonArgs.EnvironmentShort}";
        var vnetName = $"vnet-{commonArgs.Application}-{commonArgs.LocationShort}-{commonArgs.EnvironmentShort}";
        var clusterName = $"aks-{commonArgs.Application}-{commonArgs.LocationShort}-{commonArgs.EnvironmentShort}";

        // Create an Azure Resource Group
        var resourceGroup1 = new AzureNative.Resources.ResourceGroup(resourceGroupName);

        var vnet1 = new AzureNative.Network.VirtualNetwork(vnetName, new AzureNative.Network.VirtualNetworkArgs
        {
            ResourceGroupName = resourceGroup1.Name,
            AddressSpace = new AzureNative.Network.Inputs.AddressSpaceArgs
            {
                AddressPrefixes =
                {
                    "10.0.0.0/16",
                }
            },
        });

        var adSp = new AzureServicePrincipal("aks");
        ApplicationId = adSp.ApplicationId;

        var aksCluster = new AksCluster(resourceGroup1.Name, clusterName, ApplicationId, adSp.Secret);
        KubeConfig = aksCluster.KubeConfig;
    }

    public Output<string> ApplicationId { get; }
    public Output<string> KubeConfig { get; }
}