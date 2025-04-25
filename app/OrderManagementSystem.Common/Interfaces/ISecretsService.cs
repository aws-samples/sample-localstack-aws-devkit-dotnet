using System;

namespace OrderManagementSystem.Common.Interfaces;

public interface ISecretsService
{
    Task<string> GetSecret(string secretId);
}
