using AprilPractice.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

/// <summary>
/// Example page demonstrating Azure Key Vault usage.
/// </summary>
public class SecretsExampleModel : PageModel
{
    private readonly IKeyVaultService? _keyVaultService;
    private readonly ILogger<SecretsExampleModel> _logger;

    public string? ConnectionString { get; private set; }
    public string? ApiKey { get; private set; }
    public string? ErrorMessage { get; private set; }

    public SecretsExampleModel(
        ILogger<SecretsExampleModel> logger,
        IKeyVaultService? keyVaultService = null)
    {
        _logger = logger;
        _keyVaultService = keyVaultService;
    }

    public async Task OnGetAsync()
    {
        if (_keyVaultService is null)
        {
            ErrorMessage = "Key Vault service is not configured. Please set AzureKeyVault:VaultUrl in appsettings.json";
            return;
        }

        try
        {
            // Example: Retrieve database connection string from Key Vault
            // Secret names in Key Vault should not contain special characters
            ConnectionString = await _keyVaultService.GetSecretAsync("DatabaseConnectionString");

            // Example: Retrieve API key from Key Vault
            ApiKey = await _keyVaultService.GetSecretAsync("ExternalApiKey");

            _logger.LogInformation("Successfully retrieved secrets from Key Vault");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secrets from Key Vault");
            ErrorMessage = $"Failed to retrieve secrets: {ex.Message}";
        }
    }
}
