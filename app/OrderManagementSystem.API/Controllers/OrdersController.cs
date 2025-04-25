// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Microsoft.AspNetCore.Mvc;
using OrderManagementSystem.API.CustomAttributes;
using OrderManagementSystem.Data.Models;
using OrderManagementSystem.Services;

namespace OrderManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiKeyAuth]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<Guid> PlaceOrder([FromBody] Order order)
    {
        if (order == null)
        {
            throw new ArgumentNullException(nameof(order));
        }

        return await _orderService.PlaceOrderAsync(order);
    }
}
