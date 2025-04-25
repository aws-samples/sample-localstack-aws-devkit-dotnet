// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon;
using Amazon.S3;
using Microsoft.Extensions.Options;
using OrderManagementSystem.Configurations;

namespace OrderManagementSystem.Common.AwsServices.Factory;

public class S3ClientFactory : IS3ClientFactory
{
    public IAmazonS3 CreateClient(IOptions<AWSSettings> awsSettings)
    {
        var s3Config = new AmazonS3Config();

        // Either ServiceUrl (for localstack) or RegionEndpoint should be set in the config
        if (!string.IsNullOrWhiteSpace(awsSettings.Value.ServiceUrl))
        {
            s3Config.ServiceURL = awsSettings.Value.ServiceUrl;
            s3Config.ForcePathStyle = true;
        }
        else
        {
            s3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(awsSettings.Value.Region);
        }

       return new AmazonS3Client(s3Config);
    }
}
