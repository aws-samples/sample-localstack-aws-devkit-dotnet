// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Options;
using OrderManagementSystem.Configurations;
using OrderManagementSystem.Common.AwsServices.Factory;
using OrderManagementSystem.Common.Interfaces;

namespace OrderManagementSystem.Common.AwsServices;

public class SNSService : INotificationService
{
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly string _topicArn;

    public SNSService(ISnsClientFactory clientFactory, IOptions<AWSSettings> awsSettings)
    {
        _snsClient = clientFactory.CreateClient(awsSettings);
        _topicArn = awsSettings.Value.SNSTopicArn;
    }

    public async Task<string?> PublishMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentNullException(nameof(message));
        }

        var publishRequest = new PublishRequest
        {
            TopicArn = _topicArn,
            Message = message
        };

        try
        {
            var response = await _snsClient.PublishAsync(publishRequest);

            Console.WriteLine($"Message published successfully. Message ID: {response.MessageId}");

            return response.MessageId;
        }
        catch (AmazonSimpleNotificationServiceException snsException)
        {
            Console.WriteLine($"SNS Error: {snsException.Message}");
            throw;
        }
        catch (AmazonServiceException serviceException)
        {
            Console.WriteLine($"AWS Service Error: {serviceException.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }
    }
}
