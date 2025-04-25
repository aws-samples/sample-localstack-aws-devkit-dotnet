// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon.S3;
using Microsoft.Extensions.Options;
using OrderManagementSystem.Configurations;

namespace OrderManagementSystem.Common.AwsServices.Factory;

public interface IS3ClientFactory
{
    IAmazonS3 CreateClient(IOptions<AWSSettings> awsSettings);
}
