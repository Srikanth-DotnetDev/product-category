using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace AprilPractice.Infrastructure.Services;

/// <summary>
/// Service for retrieving secrets from Azure Key Vault.
/// </summary>
public class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient _secretClient;

    public KeyVaultService(string keyVaultUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyVaultUrl);

        // DefaultAzureCredential supports multiple auth methods:
        // - Managed Identity (in Azure)
        // - Visual Studio credentials (local dev)
        // - Azure CLI credentials (local dev)
        // - Environment variables
        _secretClient = new SecretClient(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential());
    }

    /// <summary>
    /// Retrieves a secret value from Azure Key Vault.
    /// </summary>
    public async Task<string> GetSecretAsync(string secretName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);

        KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
        return secret.Value;
    }

    /// <summary>
    /// Retrieves a secret value synchronously from Azure Key Vault.
    /// </summary>
    public string GetSecret(string secretName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);

        KeyVaultSecret secret = _secretClient.GetSecret(secretName);
        return secret.Value;
    }

    /// <summary>
    /// Sets a secret in Azure Key Vault.
    /// </summary>
    public async Task SetSecretAsync(string secretName, string secretValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);
        ArgumentException.ThrowIfNullOrWhiteSpace(secretValue);

        await _secretClient.SetSecretAsync(secretName, secretValue);
    }

    /// <summary>
    /// Deletes a secret from Azure Key Vault.
    /// </summary>
    public async Task DeleteSecretAsync(string secretName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);

        DeleteSecretOperation operation = await _secretClient.StartDeleteSecretAsync(secretName);
        await operation.WaitForCompletionAsync();
    }

    /// <summary>
    /// Checks if a secret exists in Azure Key Vault.
    /// </summary>
    public async Task<bool> SecretExistsAsync(string secretName)
    {
        try
        {
            await _secretClient.GetSecretAsync(secretName);
            return true;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }
}
