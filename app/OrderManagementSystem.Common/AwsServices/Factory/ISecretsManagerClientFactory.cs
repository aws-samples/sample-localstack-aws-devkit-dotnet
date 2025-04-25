using Amazon.SecretsManager;
using Microsoft.Extensions.Options;
using OrderManagementSystem.Configurations;

namespace OrderManagementSystem.Common.AwsServices.Factory;

public interface ISecretsManagerClientFactory
{
    IAmazonSecretsManager CreateClient(IOptions<AWSSettings> awsSettings);
}
