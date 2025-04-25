// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Npgsql;
using OrderManagementSystem.Data.Models;

namespace OrderManagementSystem.Data.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly IPostgreSqlDbContext _dbContext;

    public OrderRepository(IPostgreSqlDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> AddOrderAsync(Order order)
    {
        var orderId = Guid.NewGuid();

        using var conn = _dbContext.GetConnection();
        using var cmd = new NpgsqlCommand(@"
            INSERT INTO oms.orders (id, customer_id, order_date, total_amount, status, create_date, update_date) 
            VALUES (@id, @CustomerId, @OrderDate, @TotalAmount, @Status, @CreateDate, @UpdateDate);", conn);

        cmd.Parameters.AddWithValue("@id", orderId);
        cmd.Parameters.AddWithValue("@CustomerId", order.CustomerId);
        cmd.Parameters.AddWithValue("@OrderDate", order.OrderDate);
        cmd.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
        cmd.Parameters.AddWithValue("@Status", order.Status.ToString());
        cmd.Parameters.AddWithValue("@CreateDate", order.CreateDate);
        cmd.Parameters.AddWithValue("@UpdateDate", order.UpdateDate);

        await cmd.ExecuteScalarAsync();

        return orderId;
    }
}