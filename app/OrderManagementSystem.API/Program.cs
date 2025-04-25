// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Amazon.DynamoDBv2;
using Amazon.S3;
using Amazon.SecretsManager;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Microsoft.OpenApi.Models;
using OrderManagementSystem.Configurations;
using OrderManagementSystem.Data;
using OrderManagementSystem.Data.Repository;
using OrderManagementSystem.Services;
using OrderManagementSystem.Common.AwsServices;
using OrderManagementSystem.Common.AwsServices.Factory;
using OrderManagementSystem.Common.Interfaces;

namespace OrderManagementSystem.API;

public class Program
{
    public static void Main(string[] args)
    {

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        Console.WriteLine("Environment: " + environment);

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddConfiguration(config);

        // Bind configuration settings
        builder.Services.Configure<AWSSettings>(config.GetSection("AWS"));

        // Register AWS SDK services
        builder.Services.AddAWSService<IAmazonSimpleNotificationService>();
        builder.Services.AddAWSService<IAmazonS3>();
        builder.Services.AddAWSService<IAmazonSQS>();
        builder.Services.AddAWSService<IAmazonDynamoDB>();
        builder.Services.AddAWSService<IAmazonSecretsManager>();

        // Register application services
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<INotificationService, SNSService>();
        builder.Services.AddScoped<IFileStorageService, S3Service>();
        builder.Services.AddScoped<IQueueService, SQSService>();
        builder.Services.AddScoped<INoSqlDbService, DynamoDbService>();
        builder.Services.AddScoped<ISecretsService, SecretsService>();
        builder.Services.AddScoped<IDynamoDbClientFactory, DynamoDbClientFactory>();
        builder.Services.AddScoped<IS3ClientFactory, S3ClientFactory>();
        builder.Services.AddScoped<ISnsClientFactory, SnsClientFactory>();
        builder.Services.AddScoped<ISqsClientFactory, SqsClientFactory>();
        builder.Services.AddScoped<ISecretsManagerClientFactory, SecretsManagerClientFactory>();

        // Register DB Context
        builder.Services.AddScoped<IPostgreSqlDbContext, PostgreSqlDbContext>();

        // Register controllers
        builder.Services.AddControllers();

        // Add Swagger documentation
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Order Management API",
                Version = "v1",
                Description = "API for managing orders with AWS integration."
            });
        });

        // Add healthcheck endpoint
        builder.Services.AddHealthChecks();

        // Add application layers
        builder.Services.AddScoped<OrderService>();
        builder.Services.AddScoped<SNSService>();
        builder.Services.AddScoped<S3Service>();
        builder.Services.AddScoped<SQSService>();
        builder.Services.AddScoped<DynamoDbService>();
        builder.Services.AddScoped<SecretsService>();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Management API v1");
            options.RoutePrefix = "swagger";
        });

        app.MapHealthChecks("/health");

        app.MapControllers();

        app.Run();
    }
}
