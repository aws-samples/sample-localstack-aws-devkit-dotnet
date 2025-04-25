// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System.ComponentModel.DataAnnotations;

namespace OrderManagementSystem.Configurations;

public class ApiKeySecret
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;
}