// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrderManagementSystem.Configurations;
using OrderManagementSystem.Common.AwsServices.Factory;
using OrderManagementSystem.Common.Interfaces;

namespace OrderManagementSystem.Common.AwsServices;

public class SQSService : IQueueService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly string _queueUrl;

    public SQSService(ISqsClientFactory clientFactory, IOptions<AWSSettings> awsSettings)
    {
        _sqsClient = clientFactory.CreateClient(awsSettings);
        _queueUrl = awsSettings.Value.SQSQueueUrl;
    }

    public async Task<string?> PublishMessage<T>(T message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        try
        {
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = JsonConvert.SerializeObject(message)
            };

            var response = await _sqsClient.SendMessageAsync(sendMessageRequest);

            return response?.MessageId;
        }
        catch (AmazonSQSException sqsException)
        {
            Console.WriteLine($"SQS Error: {sqsException.Message}");
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

    public async Task<List<T>?> ReceiveMessages<T>(int count)
    {
        try
        {
            var response = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = count
            });

            return response?.Messages?.Select(x => JsonConvert.DeserializeObject<T>(x.Body))?.ToList();
        }
        catch (AmazonSQSException sqsException)
        {
            Console.WriteLine($"SQS Error: {sqsException.Message}");
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
