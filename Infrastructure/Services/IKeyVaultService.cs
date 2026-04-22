namespace AprilPractice.Infrastructure.Services;

/// <summary>
/// Interface for Azure Key Vault operations.
/// </summary>
public interface IKeyVaultService
{
    /// <summary>
    /// Retrieves a secret value asynchronously from Azure Key Vault.
    /// </summary>
    Task<string> GetSecretAsync(string secretName);

    /// <summary>
    /// Retrieves a secret value synchronously from Azure Key Vault.
    /// </summary>
    string GetSecret(string secretName);

    /// <summary>
    /// Sets a secret in Azure Key Vault.
    /// </summary>
    Task SetSecretAsync(string secretName, string secretValue);

    /// <summary>
    /// Deletes a secret from Azure Key Vault.
    /// </summary>
    Task DeleteSecretAsync(string secretName);

    /// <summary>
    /// Checks if a secret exists in Azure Key Vault.
    /// </summary>
    Task<bool> SecretExistsAsync(string secretName);
}
