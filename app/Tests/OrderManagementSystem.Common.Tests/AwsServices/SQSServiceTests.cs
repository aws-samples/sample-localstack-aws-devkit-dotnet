// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using OrderManagementSystem.Configurations;
using OrderManagementSystem.Common.AwsServices.Factory;

namespace OrderManagementSystem.Common.AwsServices.Tests;

[TestFixture]
public class SQSServiceTests
{
    private Mock<IAmazonSQS> _mockSqsClient;
    private Mock<ISqsClientFactory> _mockClientFactory;
    private Mock<IOptions<AWSSettings>> _mockAwsSettings;
    private SQSService _sqsService;
    private const string TEST_QUEUE_URL = "http://localhost:4566/queue/test-queue";
    private const string TEST_SERVICE_URL = "http://localhost:4566";

    [SetUp]
    public void Setup()
    {
        _mockClientFactory = new Mock<ISqsClientFactory>();
        _mockSqsClient = new Mock<IAmazonSQS>();
        _mockAwsSettings = new Mock<IOptions<AWSSettings>>();

        // Setup factory
        _mockClientFactory
            .Setup(x => x.CreateClient(It.IsAny<IOptions<AWSSettings>>()))
            .Returns(_mockSqsClient.Object);

        // Setup AWS Settings
        _mockAwsSettings.Setup(x => x.Value).Returns(new AWSSettings
        {
            ServiceUrl = TEST_SERVICE_URL,
            SQSQueueUrl = TEST_QUEUE_URL
        });

        _sqsService = new SQSService(_mockClientFactory.Object, _mockAwsSettings.Object);
    }

    public class TestMessage
    {
        public string Id { get; set; }
        public string Content { get; set; }
    }

    [Test]
    public async Task PublishMessage_When_ValidMessage_ReturnsMessageId()
    {
        // Arrange
        var testMessage = new TestMessage { Id = "1", Content = "test content" };
        var expectedMessageId = "test-message-id";

        _mockSqsClient
            .Setup(x => x.SendMessageAsync(
                It.IsAny<SendMessageRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SendMessageResponse { MessageId = expectedMessageId });

        // Act
        var result = await _sqsService.PublishMessage(testMessage);

        // Assert
        Assert.That(result, Is.EqualTo(expectedMessageId));

        _mockSqsClient.Verify(x => x.SendMessageAsync(
            It.Is<SendMessageRequest>(req =>
                req.QueueUrl == TEST_QUEUE_URL &&
                JsonConvert.DeserializeObject<TestMessage>(req.MessageBody).Id == testMessage.Id),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task ReceiveMessages_When_ValidCount_ReturnsMessages()
    {
        // Arrange
        var count = 2;
        var testMessages = new List<TestMessage>
        {
            new TestMessage { Id = "1", Content = "content 1" },
            new TestMessage { Id = "2", Content = "content 2" }
        };

        var messages = testMessages.Select(m => new Message
        {
            Body = JsonConvert.SerializeObject(m)
        }).ToList();

        _mockSqsClient
            .Setup(x => x.ReceiveMessageAsync(
                It.IsAny<ReceiveMessageRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReceiveMessageResponse { Messages = messages });

        // Act
        var result = await _sqsService.ReceiveMessages<TestMessage>(count);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Id, Is.EqualTo("1"));
        Assert.That(result[1].Id, Is.EqualTo("2"));
    }

    [Test]
    public void PublishMessage_When_Message_Null_ThrowsException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _sqsService.PublishMessage<string>(null));
    }

    [Test]
    public void PublishMessage_When_SQSException_ThrowsException()
    {
        // Arrange
        var testMessage = new TestMessage { Id = "1", Content = "test content" };
        var expectedMessage = "SQS Error";

        _mockSqsClient
            .Setup(x => x.SendMessageAsync(
                It.IsAny<SendMessageRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonSQSException(expectedMessage));

        // Act & Assert
        var exception = Assert.ThrowsAsync<AmazonSQSException>(async () =>
            await _sqsService.PublishMessage(testMessage));

        Assert.That(exception.Message, Is.EqualTo(expectedMessage));
    }

    [Test]
    public void ReceiveMessages_When_SQSException_ThrowsException()
    {
        // Arrange
        var expectedMessage = "SQS Error";

        _mockSqsClient
            .Setup(x => x.ReceiveMessageAsync(
                It.IsAny<ReceiveMessageRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonSQSException(expectedMessage));

        // Act & Assert
        var exception = Assert.ThrowsAsync<AmazonSQSException>(async () =>
            await _sqsService.ReceiveMessages<TestMessage>(1));

        Assert.That(exception.Message, Is.EqualTo(expectedMessage));
    }

    [Test]
    public async Task ReceiveMessages_When_NoMessages_ReturnsEmptyList()
    {
        // Arrange
        _mockSqsClient
            .Setup(x => x.ReceiveMessageAsync(
                It.IsAny<ReceiveMessageRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReceiveMessageResponse { Messages = new List<Message>() });

        // Act
        var result = await _sqsService.ReceiveMessages<TestMessage>(1);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void PublishMessage_When_NullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        TestMessage testMessage = null;

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _sqsService.PublishMessage(testMessage));
    }

    [Test]
    public void Constructor_When_ValidSettings_CreatesInstance()
    {
        // Arrange & Act
        var service = new SQSService(_mockClientFactory.Object, _mockAwsSettings.Object);

        // Assert
        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public async Task PublishMessage_When_LargeMessage_SuccessfullyPublishes()
    {
        // Arrange
        var testMessage = new TestMessage
        {
            Id = "1",
            Content = new string('x', 256 * 1024) // 256KB message
        };
        var expectedMessageId = "large-message-id";

        _mockSqsClient
            .Setup(x => x.SendMessageAsync(
                It.IsAny<SendMessageRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SendMessageResponse { MessageId = expectedMessageId });

        // Act
        var result = await _sqsService.PublishMessage(testMessage);

        // Assert
        Assert.That(result, Is.EqualTo(expectedMessageId));
    }
}
