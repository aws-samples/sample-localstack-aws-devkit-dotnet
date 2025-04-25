// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OrderManagementSystem.Data.Models;
using OrderManagementSystem.Data.Repository;
using OrderManagementSystem.Common.Interfaces;

namespace OrderManagementSystem.Services.Tests;

[TestFixture]
public class OrderServiceTests
{
    private Mock<IOrderRepository> _mockOrderRepository;
    private Mock<INotificationService> _mockNotificationService;
    private Mock<IFileStorageService> _mockFileStorageService;
    private Mock<INoSqlDbService> _mockNoSqlDbService;
    private Mock<ILogger<OrderService>> _mockLogger;

    private OrderService _orderService;

    [SetUp]
    public void Setup()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockFileStorageService = new Mock<IFileStorageService>();
        _mockNoSqlDbService = new Mock<INoSqlDbService>();
        _mockLogger = new Mock<ILogger<OrderService>>();

        _orderService = new OrderService(
            _mockOrderRepository.Object,
            _mockNotificationService.Object,
            _mockFileStorageService.Object,
            _mockNoSqlDbService.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task PlaceOrderAsync_When_SuccessfulOrder_ReturnsOrderId()
    {
        // Arrange
        var order = new Order { CustomerId = Guid.NewGuid() };
        var expectedOrderId = Guid.NewGuid();
        var expectedMessage = JsonSerializer.Serialize(order);
        var expectedKey = $"orders/{expectedOrderId}.json";

        _mockOrderRepository
            .Setup(r => r.AddOrderAsync(order))
            .ReturnsAsync(expectedOrderId);

        // Act
        var result = await _orderService.PlaceOrderAsync(order);

        // Assert
        Assert.That(result, Is.EqualTo(expectedOrderId));

        _mockOrderRepository.Verify(r => r.AddOrderAsync(order), Times.Once);
        _mockNotificationService.Verify(n => n.PublishMessage(It.IsAny<string>()), Times.Once);
        _mockFileStorageService.Verify(f => f.SaveFile(expectedKey, It.IsAny<string>()), Times.Once);
        _mockNoSqlDbService.Verify(d => d.AddItem(expectedOrderId.ToString(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void PlaceOrderAsync_When_RepositoryThrowsException_LogsAndRethrowsException()
    {
        // Arrange
        var order = new Order { CustomerId = Guid.NewGuid() };
        var expectedException = new Exception("Database error");

        _mockOrderRepository
            .Setup(r => r.AddOrderAsync(order))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(async () =>
            await _orderService.PlaceOrderAsync(order));

        Assert.That(exception, Is.SameAs(expectedException));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Test]
    public void PlaceOrderAsync_When_NotificationServiceThrowsException_LogsAndRethrowsException()
    {
        // Arrange
        var order = new Order { CustomerId = Guid.NewGuid() };
        var expectedException = new Exception("Notification service error");

        _mockOrderRepository
            .Setup(r => r.AddOrderAsync(order))
            .ReturnsAsync(Guid.NewGuid());

        _mockNotificationService
            .Setup(n => n.PublishMessage(It.IsAny<string>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(async () =>
            await _orderService.PlaceOrderAsync(order));

        Assert.That(exception, Is.SameAs(expectedException));
    }

    [Test]
    public void PlaceOrderAsync_When_FileStorageThrowsException_LogsAndRethrowsException()
    {
        // Arrange
        var order = new Order { CustomerId = Guid.NewGuid() };
        var expectedException = new Exception("File storage error");

        _mockOrderRepository
            .Setup(r => r.AddOrderAsync(order))
            .ReturnsAsync(Guid.NewGuid());

        _mockNotificationService
            .Setup(n => n.PublishMessage(It.IsAny<string>()))
            .ReturnsAsync("message_1");

        _mockFileStorageService
            .Setup(f => f.SaveFile(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(async () =>
            await _orderService.PlaceOrderAsync(order));

        Assert.That(exception, Is.SameAs(expectedException));
    }

    [Test]
    public void PlaceOrderAsync_When_NoSqlDbThrowsException_LogsAndRethrowsException()
    {
        // Arrange
        var order = new Order { CustomerId = Guid.NewGuid() };
        var expectedException = new Exception("NoSQL DB error");

        _mockOrderRepository
            .Setup(r => r.AddOrderAsync(order))
            .ReturnsAsync(Guid.NewGuid());

        _mockNotificationService
            .Setup(n => n.PublishMessage(It.IsAny<string>()))
            .ReturnsAsync("message_1");

        _mockFileStorageService
            .Setup(f => f.SaveFile(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockNoSqlDbService
            .Setup(d => d.AddItem(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(async () =>
            await _orderService.PlaceOrderAsync(order));

        Assert.That(exception, Is.SameAs(expectedException));
    }
}
