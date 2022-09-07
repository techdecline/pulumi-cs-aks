using Pulumi;
using Pulumi.Tls;
using AzureNative = Pulumi.AzureNative;
using Pulumi.AzureNative.ContainerService.Inputs;
using System.Text;
using System;

public class AksCluster
{
    public AksCluster(Input<string> resourceGroupName, string clusterName, Input<string> applicationId, Input<string> secret)
    {
        ResourceGroupName = resourceGroupName;
        ApplicationId = applicationId;
        Secret = secret;
        ClusterName = clusterName;

        var sshKey = new PrivateKey("ssh-key", new PrivateKeyArgs
        {
            Algorithm = "RSA",
            RsaBits = 4096
        });

        var cluster = new AzureNative.ContainerService.ManagedCluster(ClusterName, new AzureNative.ContainerService.ManagedClusterArgs
        {
            ResourceGroupName = ResourceGroupName,
            AgentPoolProfiles =
                {
                    new ManagedClusterAgentPoolProfileArgs
                    {
                        Count = 1,
                        MaxPods = 110,
                        Mode = "System",
                        Name = "agentpool",
                        OsDiskSizeGB = 30,
                        OsType = "Linux",
                        Type = "VirtualMachineScaleSets",
                        VmSize = "Standard_DS2_v2",
                    }
                },
            DnsPrefix = "AzureNativeprovider",
            EnableRBAC = true,
            KubernetesVersion = "1.21.2",
            LinuxProfile = new ContainerServiceLinuxProfileArgs
            {
                AdminUsername = "testuser",
                Ssh = new ContainerServiceSshConfigurationArgs
                {
                    PublicKeys =
                        {
                            new ContainerServiceSshPublicKeyArgs
                            {
                                KeyData = sshKey.PublicKeyOpenssh,
                            }
                        }
                }
            },
            ServicePrincipalProfile = new ManagedClusterServicePrincipalProfileArgs
            {
                ClientId = ApplicationId,
                Secret = Secret
            }
        });

        var configMap = new 

        // Export the KubeConfig
        KubeConfig = GetKubeConfig(ResourceGroupName, cluster.Name);
    }

    public Input<string> ResourceGroupName { get; } = null!;
    public Input<string> ApplicationId { get; } = null!;
    public Input<string> Secret { get; } = null!;

    public string ClusterName { get; } = null!;

    [Output("kubeconfig")]
    public Output<string> KubeConfig { get; set; }

    private static Output<string> GetKubeConfig(Output<string> ResourceGroupName, Output<string> clusterName)
        => AzureNative.ContainerService.ListManagedClusterUserCredentials.Invoke(new AzureNative.ContainerService.ListManagedClusterUserCredentialsInvokeArgs
        {
            ResourceGroupName = ResourceGroupName,
            ResourceName = clusterName
        }).Apply(credentials =>
        {
            var encoded = credentials.Kubeconfigs[0].Value;
            var data = Convert.FromBase64String(encoded);
            return Encoding.UTF8.GetString(data);
        });
}