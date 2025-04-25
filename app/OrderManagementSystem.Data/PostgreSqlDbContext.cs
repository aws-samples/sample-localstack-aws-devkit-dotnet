// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Microsoft.Extensions.Options;
using Npgsql;
using OrderManagementSystem.Common.Interfaces;
using OrderManagementSystem.Configurations;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.Json;

namespace OrderManagementSystem.Data;

public class PostgreSqlDbContext : IPostgreSqlDbContext, IDisposable
{
    private readonly string _connectionString;
    private readonly ISecretsService _secretsService;
    private readonly NpgsqlConnection _connection;

    public PostgreSqlDbContext(ISecretsService secretsService, IOptions<AWSSettings> awsSettings)
    {
        _secretsService = secretsService;
        var dbSecretId = awsSettings.Value.PostgreSqlDBSecretId;
        _connectionString = GetConnectionString(dbSecretId);
        _connection = new NpgsqlConnection(_connectionString);
    }

    public NpgsqlConnection GetConnection()
    {
        if (_connection.State == ConnectionState.Closed)
        {
            _connection.Open();
        }
        return _connection;
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    private string GetConnectionString(string dbCredentialsSecretId)
    {
        var dbCredentialsString = _secretsService.GetSecret(dbCredentialsSecretId).Result;
        var postgreSqlConnection = JsonSerializer.Deserialize<PostgreSQLConnection>(dbCredentialsString);
        if (postgreSqlConnection == null)
        {
            throw new ValidationException("Postgres connection configuration is invalid");
        }

        var (isValid, errors) = PostgreSQLConnectionValidator.Validate(postgreSqlConnection);
        if (!isValid)
        {
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }
            throw new ValidationException("Invalid PostgreSQL Connection configuration");
        }

        return $"Host={postgreSqlConnection.Host};" +
            $"Database={postgreSqlConnection.Database};" +
            $"Port={postgreSqlConnection.Port};" +
            $"Username={postgreSqlConnection.Username};" +
            $"Password={postgreSqlConnection.Password}";
    }
}
