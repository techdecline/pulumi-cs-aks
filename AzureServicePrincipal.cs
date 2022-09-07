using Pulumi;
using Pulumi.AzureAD;

public class AzureServicePrincipal
{
    public AzureServicePrincipal(string applicationName)
    {
        ApplicationName = applicationName;
        Pulumi.AzureAD.Application adApp = new Application(ApplicationName, new ApplicationArgs
        {
            DisplayName = ApplicationName
        });

        Pulumi.AzureAD.ServicePrincipal adSp = new ServicePrincipal($"{ApplicationName}Sp", new ServicePrincipalArgs
        {
            ApplicationId = adApp.ApplicationId
        });

        Pulumi.AzureAD.ApplicationPassword adSpSecret = new ApplicationPassword($"{ApplicationName}SpSecret", new ApplicationPasswordArgs
        {
            ApplicationObjectId = adApp.ObjectId
        });

        ApplicationId = adApp.ApplicationId;
        Secret = adSpSecret.Value;
    }
    public string ApplicationName { get; }

    public Output<string> ApplicationId { get; }
    public Output<string> Secret { get; }
}