// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Options;
using OrderManagementSystem.Configurations;

namespace OrderManagementSystem.Common.AwsServices.Factory;

public class SnsClientFactory : ISnsClientFactory
{
    public IAmazonSimpleNotificationService CreateClient(IOptions<AWSSettings> awsSettings)
    {
        var snsConfig = new AmazonSimpleNotificationServiceConfig();

        // Either ServiceUrl (for localstack) or RegionEndpoint should be set in the config
        if (!string.IsNullOrWhiteSpace(awsSettings.Value.ServiceUrl))
        {
            snsConfig.ServiceURL = awsSettings.Value.ServiceUrl;
        }
        else
        {
            snsConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(awsSettings.Value.Region);
        }

        return new AmazonSimpleNotificationServiceClient(snsConfig);
    }
}
