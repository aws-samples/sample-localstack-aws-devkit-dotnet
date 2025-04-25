// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using OrderManagementSystem.Data.Models;

namespace OrderManagementSystem.Services;

public interface IOrderService
{
    Task<Guid> PlaceOrderAsync(Order order);
}