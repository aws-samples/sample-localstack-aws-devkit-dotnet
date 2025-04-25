using Amazon;
using Amazon.SecretsManager;
using Microsoft.Extensions.Options;
using OrderManagementSystem.Configurations;

namespace OrderManagementSystem.Common.AwsServices.Factory;

public class SecretsManagerClientFactory : ISecretsManagerClientFactory
{
    public IAmazonSecretsManager CreateClient(IOptions<AWSSettings> awsSettings)
    {
        var config = new AmazonSecretsManagerConfig();

        // Either ServiceUrl (for localstack) or RegionEndpoint should be set in the config
        if (!string.IsNullOrWhiteSpace(awsSettings.Value.ServiceUrl))
        {
            config.ServiceURL = awsSettings.Value.ServiceUrl;
        }
        else
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(awsSettings.Value.Region);
        }

        return new AmazonSecretsManagerClient(config);
    }
}
