// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OrderManagementSystem.Configurations;
using OrderManagementSystem.Common.AwsServices.Factory;

namespace OrderManagementSystem.Common.AwsServices.Tests;

[TestFixture]
public class SNSServiceTests
{
    private Mock<IAmazonSimpleNotificationService> _mockSnsClient;
    private Mock<ISnsClientFactory> _mockClientFactory;
    private Mock<IOptions<AWSSettings>> _mockAwsSettings;
    private SNSService _snsService;
    private const string TEST_TOPIC_ARN = "arn:aws:sns:us-east-1:123456789012:test-topic";
    private const string TEST_SERVICE_URL = "http://localhost:4566";

    [SetUp]
    public void Setup()
    {
        _mockClientFactory = new Mock<ISnsClientFactory>();
        _mockSnsClient = new Mock<IAmazonSimpleNotificationService>();
        _mockAwsSettings = new Mock<IOptions<AWSSettings>>();

        // Setup factory
        _mockClientFactory
            .Setup(x => x.CreateClient(It.IsAny<IOptions<AWSSettings>>()))
            .Returns(_mockSnsClient.Object);

        // Setup AWS Settings
        _mockAwsSettings.Setup(x => x.Value).Returns(new AWSSettings
        {
            ServiceUrl = TEST_SERVICE_URL,
            SNSTopicArn = TEST_TOPIC_ARN
        });

        _snsService = new SNSService(_mockClientFactory.Object, _mockAwsSettings.Object);
    }

    [Test]
    public async Task PublishMessage_When_ValidMessage_ReturnsMessageId()
    {
        // Arrange
        var testMessage = "test message";
        var expectedMessageId = "test-message-id";

        _mockSnsClient
            .Setup(x => x.PublishAsync(
                It.IsAny<PublishRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PublishResponse { MessageId = expectedMessageId });

        // Act
        var result = await _snsService.PublishMessage(testMessage);

        // Assert
        Assert.That(result, Is.EqualTo(expectedMessageId));

        _mockSnsClient.Verify(x => x.PublishAsync(
            It.Is<PublishRequest>(req =>
                req.TopicArn == TEST_TOPIC_ARN &&
                req.Message == testMessage),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void PublishMessage_When_Message_Null_ThrowsException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _snsService.PublishMessage(null));
    }

    [Test]
    public void PublishMessage_When_SNSException_ThrowsException()
    {
        // Arrange
        var testMessage = "test message";
        var expectedMessage = "SNS Error";

        _mockSnsClient
            .Setup(x => x.PublishAsync(
                It.IsAny<PublishRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonSimpleNotificationServiceException(expectedMessage));

        // Act & Assert
        var exception = Assert.ThrowsAsync<AmazonSimpleNotificationServiceException>(async () =>
            await _snsService.PublishMessage(testMessage));

        Assert.That(exception.Message, Is.EqualTo(expectedMessage));
    }

    [Test]
    public void PublishMessage_When_AmazonServiceException_ThrowsException()
    {
        // Arrange
        var testMessage = "test message";
        var expectedMessage = "AWS Service Error";

        _mockSnsClient
            .Setup(x => x.PublishAsync(
                It.IsAny<PublishRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonServiceException(expectedMessage));

        // Act & Assert
        var exception = Assert.ThrowsAsync<AmazonServiceException>(async () =>
            await _snsService.PublishMessage(testMessage));

        Assert.That(exception.Message, Is.EqualTo(expectedMessage));
    }

    [Test]
    public void PublishMessage_When_GeneralException_ThrowsException()
    {
        // Arrange
        var testMessage = "test message";
        var expectedMessage = "Unexpected error";

        _mockSnsClient
            .Setup(x => x.PublishAsync(
                It.IsAny<PublishRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(expectedMessage));

        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(async () =>
            await _snsService.PublishMessage(testMessage));

        Assert.That(exception.Message, Is.EqualTo(expectedMessage));
    }

    [Test]
    public void Constructor_When_ValidSettings_CreatesInstance()
    {
        // Arrange & Act
        var service = new SNSService(_mockClientFactory.Object, _mockAwsSettings.Object);

        // Assert
        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public void PublishMessage_When_NullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        string testMessage = null;

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _snsService.PublishMessage(testMessage));
    }

    [Test]
    public void PublishMessage_When_EmptyMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var testMessage = string.Empty;

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _snsService.PublishMessage(testMessage));
    }

    [Test]
    public async Task PublishMessage_When_LargeMessage_SuccessfullyPublishes()
    {
        // Arrange
        var testMessage = new string('x', 256 * 1024); // 256KB message
        var expectedMessageId = "large-message-id";

        _mockSnsClient
            .Setup(x => x.PublishAsync(
                It.IsAny<PublishRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PublishResponse { MessageId = expectedMessageId });

        // Act
        var result = await _snsService.PublishMessage(testMessage);

        // Assert
        Assert.That(result, Is.EqualTo(expectedMessageId));
    }
}
