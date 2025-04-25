// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Options;
using OrderManagementSystem.Configurations;
using OrderManagementSystem.Common.AwsServices.Factory;
using OrderManagementSystem.Common.Interfaces;

namespace OrderManagementSystem.Common.AwsServices;

public class DynamoDbService : INoSqlDbService
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;

    public DynamoDbService(IDynamoDbClientFactory clientFactory, IOptions<AWSSettings> awsSettings)
    {
        _dynamoDb = clientFactory.CreateClient(awsSettings);
        _tableName = awsSettings.Value.DynamoDBTableName;
    }

    public async Task AddItem(string id, string messageBody)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        if (string.IsNullOrWhiteSpace(messageBody))
        {
            throw new ArgumentNullException(nameof(messageBody));
        }

        try
        {
            var item = new Dictionary<string, AttributeValue>
            {
                { "OrderId", new AttributeValue { S = id } },
                { "MessageBody", new AttributeValue { S = messageBody } }
            };

            await _dynamoDb.PutItemAsync(new PutItemRequest
            {
                TableName = _tableName,
                Item = item
            });
        }
        catch (AmazonDynamoDBException ddbException)
        {
            Console.WriteLine($"DynamoDB Error: {ddbException.Message}");
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
