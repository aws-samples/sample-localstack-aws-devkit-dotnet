// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Options;
using OrderManagementSystem.Configurations;
using OrderManagementSystem.Common.AwsServices.Factory;
using OrderManagementSystem.Common.Interfaces;

namespace OrderManagementSystem.Common.AwsServices;

public class S3Service : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3Service(IS3ClientFactory s3ClientFactory, IOptions<AWSSettings> awsSettings)
    {
        _s3Client = s3ClientFactory.CreateClient(awsSettings);
        _bucketName = awsSettings.Value.S3BucketName;
    }

    public async Task SaveFile(string filePathAndName, string fileContent)
    {
        if (string.IsNullOrWhiteSpace(filePathAndName))
        {
            throw new ArgumentNullException(nameof(filePathAndName));
        }

        if (string.IsNullOrWhiteSpace(fileContent))
        {
            throw new ArgumentNullException(nameof(fileContent));
        }

        try
        {
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent));
            await _s3Client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
            {
                BucketName = _bucketName,
                Key = filePathAndName.Replace("\\", "/"),
                InputStream = stream
            });
        }
        catch (AmazonS3Exception s3Exception)
        {
            Console.WriteLine($"S3 Error: {s3Exception.Message}");
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
