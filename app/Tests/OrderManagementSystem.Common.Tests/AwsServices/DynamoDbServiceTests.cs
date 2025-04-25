// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework.Internal;
using OrderManagementSystem.Configurations;
using OrderManagementSystem.Common.AwsServices.Factory;

namespace OrderManagementSystem.Common.AwsServices.Tests;

[TestFixture]
public class DynamoDbServiceTests
{
    private Mock<IAmazonDynamoDB> _mockDynamoDb;
    private Mock<IDynamoDbClientFactory> _mockClientFactory;
    private Mock<IOptions<AWSSettings>> _mockAwsSettings;
    private DynamoDbService _dynamoDbService;

    private const string TEST_TABLE_NAME = "TestTable";
    private const string TEST_SERVICE_URL = "http://localhost:8000";

    [SetUp]
    public void Setup()
    {
        _mockClientFactory = new Mock<IDynamoDbClientFactory>();
        _mockDynamoDb = new Mock<IAmazonDynamoDB>();
        _mockAwsSettings = new Mock<IOptions<AWSSettings>>();

        // Setup factory
        _mockClientFactory
            .Setup(x => x.CreateClient(It.IsAny<IOptions<AWSSettings>>()))
            .Returns(_mockDynamoDb.Object);

        // Setup AWS Settings
        _mockAwsSettings.Setup(x => x.Value).Returns(new AWSSettings
        {
            ServiceUrl = TEST_SERVICE_URL,
            DynamoDBTableName = TEST_TABLE_NAME
        });

        _dynamoDbService = new DynamoDbService(_mockClientFactory.Object, _mockAwsSettings.Object);
    }

    [Test]
    public async Task AddItem_When_ValidInput_SuccessfullyAddsItem()
    {
        // Arrange
        var testId = "test-id";
        var testMessage = "test-message";

        _mockDynamoDb
            .Setup(x => x.PutItemAsync(
                It.IsAny<PutItemRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutItemResponse());

        // Act
        await _dynamoDbService.AddItem(testId, testMessage);

        // Assert
        _mockDynamoDb.Verify(x => x.PutItemAsync(
            It.Is<PutItemRequest>(req =>
                req.TableName == TEST_TABLE_NAME &&
                req.Item["OrderId"].S == testId &&
                req.Item["MessageBody"].S == testMessage),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void AddItem_When_Id_Null_ThrowsException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _dynamoDbService.AddItem(null, "message"));
    }

    [Test]
    public void AddItem_When_MessageBody_Null_ThrowsException()
    {
        // Act & Assert
       Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _dynamoDbService.AddItem("id", null));
    }

    [Test]
    public void AddItem_When_DynamoDBException_ThrowsException()
    {
        // Arrange
        var testId = "test-id";
        var testMessage = "test-message";
        var expectedMessage = "DynamoDB Error";

        _mockDynamoDb
            .Setup(x => x.PutItemAsync(
                It.IsAny<PutItemRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonDynamoDBException(expectedMessage));

        // Act & Assert
        var exception = Assert.ThrowsAsync<AmazonDynamoDBException>(async () =>
            await _dynamoDbService.AddItem(testId, testMessage));

        Assert.That(exception.Message, Is.EqualTo(expectedMessage));
    }

    [Test]
    public void AddItem_When_AmazonServiceException_ThrowsException()
    {
        // Arrange
        var testId = "test-id";
        var testMessage = "test-message";
        var expectedMessage = "AWS Service Error";

        _mockDynamoDb
            .Setup(x => x.PutItemAsync(
                It.IsAny<PutItemRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonServiceException(expectedMessage));

        // Act & Assert
        var exception = Assert.ThrowsAsync<AmazonServiceException>(async () =>
            await _dynamoDbService.AddItem(testId, testMessage));

        Assert.That(exception.Message, Is.EqualTo(expectedMessage));
    }

    [Test]
    public void AddItem_When_GeneralException_ThrowsException()
    {
        // Arrange
        var testId = "test-id";
        var testMessage = "test-message";
        var expectedMessage = "Unexpected error";

        _mockDynamoDb
            .Setup(x => x.PutItemAsync(
                It.IsAny<PutItemRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(expectedMessage));

        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(async () =>
            await _dynamoDbService.AddItem(testId, testMessage));

        Assert.That(exception.Message, Is.EqualTo(expectedMessage));
    }
}
