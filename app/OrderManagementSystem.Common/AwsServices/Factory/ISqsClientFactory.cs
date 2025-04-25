// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon.SQS;
using Microsoft.Extensions.Options;
using OrderManagementSystem.Configurations;

namespace OrderManagementSystem.Common.AwsServices.Factory;

public interface ISqsClientFactory
{
    IAmazonSQS CreateClient(IOptions<AWSSettings> awsSettings);
}
