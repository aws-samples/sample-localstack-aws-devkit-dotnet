// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.Options;
using OrderManagementSystem.Configurations;

namespace OrderManagementSystem.Common.AwsServices.Factory;

public class DynamoDbClientFactory : IDynamoDbClientFactory
{
    public IAmazonDynamoDB CreateClient(IOptions<AWSSettings> awsSettings)
    {
        var dynamoDbConfig = new AmazonDynamoDBConfig();

        // Either ServiceUrl (for localstack) or RegionEndpoint should be set in the config
        if (!string.IsNullOrWhiteSpace(awsSettings.Value.ServiceUrl))
        {
            dynamoDbConfig.ServiceURL = awsSettings.Value.ServiceUrl;
            dynamoDbConfig.UseHttp = true;
        }
        else
        {
            dynamoDbConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(awsSettings.Value.Region);
        }

        return new AmazonDynamoDBClient(dynamoDbConfig);
    }
}