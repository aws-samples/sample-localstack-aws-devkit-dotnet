// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

namespace OrderManagementSystem.Configurations;

public class AWSSettings
{
    public string Region { get; set; } = string.Empty;
    public string S3BucketName { get; set; } = string.Empty;
    public string SNSTopicArn { get; set; } = string.Empty;
    public string SQSQueueUrl { get; set; } = string.Empty;
    public string DynamoDBTableName { get; set; } = string.Empty;
    public string PostgreSqlDBSecretId { get; set; } = string.Empty;
    public string ApiKeySecretId { get; set; } = string.Empty;
    public string ServiceUrl { get; set; } = string.Empty;
}
