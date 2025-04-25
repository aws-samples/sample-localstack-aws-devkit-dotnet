// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Options;
using OrderManagementSystem.Configurations;

namespace OrderManagementSystem.Common.AwsServices.Factory;

public interface ISnsClientFactory
{
    IAmazonSimpleNotificationService CreateClient(IOptions<AWSSettings> awsSettings);
}
