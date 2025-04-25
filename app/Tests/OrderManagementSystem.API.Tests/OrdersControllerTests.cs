// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Moq;
using OrderManagementSystem.API.Controllers;
using OrderManagementSystem.Data.Models;
using OrderManagementSystem.Services;

namespace OrderManagementSystem.API.Tests;

[TestFixture]
public class OrdersControllerTests
{
    private Mock<IOrderService> _mockOrderService;
    private OrdersController _controller;

    [SetUp]
    public void Setup()
    {
        _mockOrderService = new Mock<IOrderService>();
        _controller = new OrdersController(_mockOrderService.Object);
    }

    [Test]
    public async Task PlaceOrder_ValidOrder_ReturnsOrderId()
    {
        // Arrange
        var order = new Order
        {
            CustomerId = Guid.NewGuid(),
            // Add other required properties
        };
        var expectedOrderId = Guid.NewGuid();

        _mockOrderService
            .Setup(s => s.PlaceOrderAsync(It.IsAny<Order>()))
            .ReturnsAsync(expectedOrderId);

        // Act
        var result = await _controller.PlaceOrder(order);

        // Assert
        Assert.That(result, Is.EqualTo(expectedOrderId));
        _mockOrderService.Verify(s => s.PlaceOrderAsync(order), Times.Once);
    }

    [Test]
    public void PlaceOrder_NullOrder_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _controller.PlaceOrder(null));
    }

    [Test]
    public void PlaceOrder_ServiceThrowsException_ThrowsException()
    {
        // Arrange
        var order = new Order
        {
            CustomerId = Guid.NewGuid()
        };
        var expectedException = new Exception("Service error");

        _mockOrderService
            .Setup(s => s.PlaceOrderAsync(It.IsAny<Order>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(async () =>
            await _controller.PlaceOrder(order));

        Assert.That(exception.Message, Is.EqualTo(expectedException.Message));
    }

    [Test]
    public async Task PlaceOrder_ConcurrentRequests_HandlesCorrectly()
    {
        // Arrange
        var order1 = new Order { CustomerId = Guid.NewGuid() };
        var order2 = new Order { CustomerId = Guid.NewGuid() };
        var expectedId1 = Guid.NewGuid();
        var expectedId2 = Guid.NewGuid();

        _mockOrderService
            .Setup(s => s.PlaceOrderAsync(order1))
            .ReturnsAsync(expectedId1);
        _mockOrderService
            .Setup(s => s.PlaceOrderAsync(order2))
            .ReturnsAsync(expectedId2);

        // Act
        var task1 = _controller.PlaceOrder(order1);
        var task2 = _controller.PlaceOrder(order2);
        await Task.WhenAll(task1, task2);

        // Assert
        Assert.That(await task1, Is.EqualTo(expectedId1));
        Assert.That(await task2, Is.EqualTo(expectedId2));
    }

}