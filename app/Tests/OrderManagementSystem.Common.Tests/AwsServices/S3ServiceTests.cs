// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Moq;
using OrderManagementSystem.Configurations;
using OrderManagementSystem.Common.AwsServices.Factory;

namespace OrderManagementSystem.Common.AwsServices.Tests;

[TestFixture]
public class S3ServiceTests
{
    private Mock<IAmazonS3> _mockS3Client;
    private Mock<IS3ClientFactory> _mockClientFactory;
    private Mock<IOptions<AWSSettings>> _mockAwsSettings;
    private S3Service _s3Service;

    private const string TEST_BUCKET_NAME = "test-bucket";
    private const string TEST_SERVICE_URL = "http://localhost:4566";

    [SetUp]
    public void Setup()
    {
        _mockClientFactory = new Mock<IS3ClientFactory>();
        _mockS3Client = new Mock<IAmazonS3>();
        _mockAwsSettings = new Mock<IOptions<AWSSettings>>();

        // Setup factory
        _mockClientFactory
            .Setup(x => x.CreateClient(It.IsAny<IOptions<AWSSettings>>()))
            .Returns(_mockS3Client.Object);

        // Setup AWS Settings
        _mockAwsSettings
            .Setup(x => x.Value)
            .Returns(new AWSSettings
            {
                ServiceUrl = TEST_SERVICE_URL,
                S3BucketName = TEST_BUCKET_NAME
            });

        _s3Service = new S3Service(_mockClientFactory.Object, _mockAwsSettings.Object);
    }

    [Test]
    public async Task SaveFile_ValidInput_SuccessfullySavesFile()
    {
        // Arrange
        var testFilePath = "test/path/file.json";
        var testContent = "test content";

        _mockS3Client
            .Setup(x => x.PutObjectAsync(
                It.IsAny<PutObjectRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse());

        // Act
        await _s3Service.SaveFile(testFilePath, testContent);

        // Assert
        _mockS3Client.Verify(x => x.PutObjectAsync(
            It.Is<PutObjectRequest>(req =>
                req.BucketName == TEST_BUCKET_NAME &&
                req.Key == testFilePath),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void SaveFile_When_FilePathAndName_Null_ThrowsException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _s3Service.SaveFile(null, "fileContent"));
    }

    [Test]
    public void SaveFile_When_FileContent_Null_ThrowsException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
             await _s3Service.SaveFile("filePathAndName", null));
    }

    [Test]
    public void SaveFile_When_S3Exception_ThrowsException()
    {
        // Arrange
        var testFilePath = "test/path/file.json";
        var testContent = "test content";
        var expectedMessage = "S3 Error";

        _mockS3Client
            .Setup(x => x.PutObjectAsync(
                It.IsAny<PutObjectRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception(expectedMessage));

        // Act & Assert
        var exception = Assert.ThrowsAsync<AmazonS3Exception>(async () =>
            await _s3Service.SaveFile(testFilePath, testContent));

        Assert.That(exception.Message, Is.EqualTo(expectedMessage));
    }

    [Test]
    public void SaveFile_When_AmazonServiceException_ThrowsException()
    {
        // Arrange
        var testFilePath = "test/path/file.json";
        var testContent = "test content";
        var expectedMessage = "AWS Service Error";

        _mockS3Client
            .Setup(x => x.PutObjectAsync(
                It.IsAny<PutObjectRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonServiceException(expectedMessage));

        // Act & Assert
        var exception = Assert.ThrowsAsync<AmazonServiceException>(async () =>
            await _s3Service.SaveFile(testFilePath, testContent));

        Assert.That(exception.Message, Is.EqualTo(expectedMessage));
    }

    [Test]
    public void SaveFile_When_GeneralException_ThrowsException()
    {
        // Arrange
        var testFilePath = "test/path/file.json";
        var testContent = "test content";
        var expectedMessage = "Unexpected error";

        _mockS3Client
            .Setup(x => x.PutObjectAsync(
                It.IsAny<PutObjectRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(expectedMessage));

        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(async () =>
            await _s3Service.SaveFile(testFilePath, testContent));

        Assert.That(exception.Message, Is.EqualTo(expectedMessage));
    }

    [Test]
    public async Task SaveFile_When_FilePathWithBackslashes_ConvertsToForwardSlashes()
    {
        // Arrange
        var testFilePath = "test\\path\\file.json";
        var expectedPath = "test/path/file.json";
        var testContent = "test content";

        _mockS3Client
            .Setup(x => x.PutObjectAsync(
                It.IsAny<PutObjectRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse());

        // Act
        await _s3Service.SaveFile(testFilePath, testContent);

        // Assert
        _mockS3Client.Verify(x => x.PutObjectAsync(
            It.Is<PutObjectRequest>(req => req.Key == expectedPath),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void Constructor_When_ValidSettings_CreatesInstance()
    {
        // Arrange & Act
        var service = new S3Service(_mockClientFactory.Object, _mockAwsSettings.Object);

        // Assert
        Assert.That(service, Is.Not.Null);
    }
}