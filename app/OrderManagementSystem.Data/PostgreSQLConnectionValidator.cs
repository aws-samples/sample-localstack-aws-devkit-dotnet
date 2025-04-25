// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using OrderManagementSystem.Configurations;
using System.ComponentModel.DataAnnotations;

namespace OrderManagementSystem.Data;

public class PostgreSQLConnectionValidator
{
    public static (bool IsValid, IEnumerable<string?> Errors) Validate(PostgreSQLConnection connection)
    {
        var context = new ValidationContext(connection);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(connection, context, validationResults, true);

        var errors = validationResults.Select(r => r.ErrorMessage).ToList();

        var port = Convert.ToInt32(connection.Port);

        // Additional custom validations
        if (port <= 0 || port > 65535)
        {
            isValid = false;
            errors.Add("Port must be between 1 and 65535");
        }

        return (isValid, errors);
    }
}
