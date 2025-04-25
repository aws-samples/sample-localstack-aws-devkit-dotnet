// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using OrderManagementSystem.Data.Models;

namespace OrderManagementSystem.Data.Repository;

public interface IOrderRepository
{
    Task<Guid> AddOrderAsync(Order order);
}