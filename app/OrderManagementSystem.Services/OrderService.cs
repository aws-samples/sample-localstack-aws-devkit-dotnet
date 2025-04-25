// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Microsoft.Extensions.Logging;
using OrderManagementSystem.Common.Interfaces;
using OrderManagementSystem.Data.Models;
using OrderManagementSystem.Data.Repository;
using System.Text.Json;

namespace OrderManagementSystem.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly INotificationService _notificationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly INoSqlDbService _noSqlDbService;
    private readonly ILogger<OrderService> _logger;

    public OrderService
    (
        IOrderRepository orderRepository,
        INotificationService notificationService,
        IFileStorageService fileStorageService,
        INoSqlDbService noSqlDbService,
        ILogger<OrderService> logger
    )
    {
        _orderRepository = orderRepository;
        _notificationService = notificationService;
        _fileStorageService = fileStorageService;
        _noSqlDbService = noSqlDbService;
        _logger = logger;
    }

    public async Task<Guid> PlaceOrderAsync(Order order)
    {
        try
        {
            _logger.LogInformation("Placing new order for CustomerId: {CustomerId}", order.CustomerId);

            // Save order to database via repository
            order.Id = await _orderRepository.AddOrderAsync(order);
            _logger.LogInformation("Order {OrderId} saved successfully in the database.", order.Id);

            // Serialize order
            var message = JsonSerializer.Serialize(order);

            // Publish order message to SNS
            await _notificationService.PublishMessage(message);
            _logger.LogInformation("Order {OrderId} published to SNS topic.", order.Id);

            // Save order details to S3
            var key = $"orders/{order.Id}.json";
            await _fileStorageService.SaveFile(key, message);
            _logger.LogInformation("Order {OrderId} saved in S3 with key: {key}", order.Id, key);

            // Log order details in DynamoDB
            await _noSqlDbService.AddItem(order.Id.ToString(), message);
            _logger.LogInformation("Order {OrderId} logged in DynamoDB.", order.Id);

            return order.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while placing the order.");
            throw;
        }
    }
}