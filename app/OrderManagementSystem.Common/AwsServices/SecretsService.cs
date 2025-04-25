using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Options;
using OrderManagementSystem.Configurations;
using OrderManagementSystem.Common.AwsServices.Factory;
using OrderManagementSystem.Common.Interfaces;

namespace OrderManagementSystem.Common.AwsServices;

public class SecretsService : ISecretsService
{
    private readonly IAmazonSecretsManager _secretsMgrClient;

    public SecretsService(ISecretsManagerClientFactory clientFactory, IOptions<AWSSettings> awsSettings)
    {
        _secretsMgrClient = clientFactory.CreateClient(awsSettings);
    }

    public async Task<string> GetSecret(string secretId)
    {
        try
        {
            var request = new GetSecretValueRequest
            {
                SecretId = secretId
            };

            var response = await _secretsMgrClient.GetSecretValueAsync(request);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Secrets retrieval failed with statusCode: {response.HttpStatusCode} " +
                    $"for secretId: {request.SecretId}");
            }

            return response.SecretString;
        }
        catch (Exception ex)
        {
            throw new Exception("Error retrieving secrets", ex);
        }
    }
}
