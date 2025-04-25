// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon;
using Amazon.SQS;
using Microsoft.Extensions.Options;
using OrderManagementSystem.Configurations;

namespace OrderManagementSystem.Common.AwsServices.Factory;

public class SqsClientFactory : ISqsClientFactory
{
    public IAmazonSQS CreateClient(IOptions<AWSSettings> awsSettings)
    {
        var sqsConfig = new AmazonSQSConfig();

        // Either ServiceUrl (for localstack) or RegionEndpoint should be set in the config
        if (!string.IsNullOrWhiteSpace(awsSettings.Value.ServiceUrl))
        {
            sqsConfig.ServiceURL = awsSettings.Value.ServiceUrl;
        }
        else
        {
            sqsConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(awsSettings.Value.Region);
        }

        return new AmazonSQSClient(sqsConfig);
    }
}
