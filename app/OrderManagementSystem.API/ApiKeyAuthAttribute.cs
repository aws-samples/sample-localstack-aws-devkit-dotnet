// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using OrderManagementSystem.Common.Interfaces;
using OrderManagementSystem.Configurations;
using System.Text.Json;

namespace OrderManagementSystem.API.CustomAttributes;

[AttributeUsage(AttributeTargets.Class)]
public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
{
    private const string API_KEY = "ApiKey";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(API_KEY, out var apiKey))
        {
            context.Result = new ContentResult
            {
                StatusCode = 401,
                Content = "Api key is required"
            };
            return;
        }

        var secretsService = context.HttpContext.RequestServices.GetRequiredService<ISecretsService>();
        var awsSettings = context.HttpContext.RequestServices.GetRequiredService<IOptions<AWSSettings>>();
        var secretId = awsSettings.Value.ApiKeySecretId;

        var secretString = await secretsService.GetSecret(secretId);
        var apiKeySecret = JsonSerializer.Deserialize<ApiKeySecret>(secretString);

        if (apiKeySecret == null || string.IsNullOrEmpty(apiKeySecret.ApiKey))
        {
            context.Result = new ContentResult
            {
                StatusCode = 500,
                Content = "Failed to retrieve or deserialize API key from secret"
            };
            return;
        }

        if (!apiKeySecret.ApiKey.Equals(apiKey))
        {
            context.Result = new ContentResult
            {
                StatusCode = 401,
                Content = "Api key mismatch"
            };
            return;
        }

        await next();
    }
}