// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Npgsql;

namespace OrderManagementSystem.Data;

public interface IPostgreSqlDbContext
{
    NpgsqlConnection GetConnection();
}
