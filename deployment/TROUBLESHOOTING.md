# ?? Troubleshooting Guide

## Common Deployment Issues

### 1. GitHub Actions Fails to Deploy

#### Symptom
```
Error: No such host is known (app-name.scm.azurewebsites.net)
```

#### Solution
```powershell
# Verify app exists
az webapp show --name app-productcategory-001 --resource-group rg-productcategory-prod

# Re-download publish profile
az webapp deployment list-publishing-profiles `
    --name app-productcategory-001 `
    --resource-group rg-productcategory-prod `
    --xml

# Update GitHub secret with new profile
```

---

### 2. Application Won't Start (HTTP 500)

#### Symptom
Browser shows "HTTP Error 500.30 - ASP.NET Core app failed to start"

#### Diagnosis
```powershell
# View application logs
az webapp log tail `
    --name app-productcategory-001 `
    --resource-group rg-productcategory-prod

# Check application logs in portal
# Portal ? App Service ? Monitoring ? Log stream
```

#### Common Causes
1. **Missing Application Insights Connection String**
   ```powershell
   # Add to app settings
   az webapp config appsettings set `
       --name app-productcategory-001 `
       --resource-group rg-productcategory-prod `
       --settings APPLICATIONINSIGHTS_CONNECTION_STRING="your-connection-string"
   ```

2. **Incorrect .NET Version**
   ```powershell
   # Verify runtime
   az webapp config show --name app-productcategory-001 --resource-group rg-productcategory-prod
   ```

---

### 3. Key Vault Access Denied

#### Symptom
```
Azure.RequestFailedException: Access denied. Caller was not found on any access policy.
```

#### Diagnosis
```powershell
# Check Managed Identity is enabled
az webapp identity show `
    --name app-productcategory-001 `
    --resource-group rg-productcategory-prod

# Check role assignments
az role assignment list `
    --assignee <principal-id-from-above> `
    --scope /subscriptions/{subscription-id}/resourceGroups/rg-productcategory-prod/providers/Microsoft.KeyVault/vaults/kv-prodcat-001
```

#### Solution
```powershell
# Get app's Managed Identity principal ID
$principalId = az webapp identity show `
    --name app-productcategory-001 `
    --resource-group rg-productcategory-prod `
    --query principalId -o tsv

# Assign Key Vault Secrets User role
az role assignment create `
    --role "Key Vault Secrets User" `
    --assignee $principalId `
    --scope /subscriptions/{subscription-id}/resourceGroups/rg-productcategory-prod/providers/Microsoft.KeyVault/vaults/kv-prodcat-001
```

---

### 4. SQL Database Connection Fails

#### Symptom
```
Microsoft.Data.SqlClient.SqlException: Cannot open server 'sql-productcategory-001'
```

#### Diagnosis
```powershell
# Check firewall rules
az sql server firewall-rule list `
    --server sql-productcategory-001 `
    --resource-group rg-productcategory-prod

# Test connection from app
az webapp ssh --name app-productcategory-001 --resource-group rg-productcategory-prod
```

#### Solution
```powershell
# Add firewall rule for Azure services
az sql server firewall-rule create `
    --server sql-productcategory-001 `
    --resource-group rg-productcategory-prod `
    --name AllowAzureServices `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 0.0.0.0

# Add your IP for management
$myIp = (Invoke-WebRequest -Uri "https://api.ipify.org").Content
az sql server firewall-rule create `
    --server sql-productcategory-001 `
    --resource-group rg-productcategory-prod `
    --name MyIP `
    --start-ip-address $myIp `
    --end-ip-address $myIp
```

---

### 5. Resource Names Already Exist

#### Symptom
```
Code: ResourceNameAlreadyExists
Message: The specified name is already in use
```

#### Solution
Key Vault and SQL Server names must be globally unique.

```powershell
# Use naming convention with initials
$kvName = "kv-prodcat-sm-001"  # Add your initials
$sqlName = "sql-prodcat-sm-001"

# Or add random suffix
$suffix = Get-Random -Minimum 1000 -Maximum 9999
$kvName = "kv-prodcat-$suffix"
$sqlName = "sql-prodcat-$suffix"
```

---

### 6. Build Fails in GitHub Actions

#### Symptom
```
error NU1101: Unable to find package
error CS0246: The type or namespace name could not be found
```

#### Solution
```yaml
# Check .NET version in workflow
- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '10.0.x'  # Must match project

# Clear NuGet cache if needed
- name: Clear NuGet cache
  run: dotnet nuget locals all --clear
```

---

### 7. Slow Application Performance

#### Diagnosis
```powershell
# Check Application Insights
# Portal ? Application Insights ? Performance

# View slow requests
# Application Insights ? Investigate ? Performance ? Operations
```

#### Solutions

**Scale Up (Vertical)**
```powershell
# Upgrade to higher tier
az appservice plan update `
    --name app-productcategory-001-plan `
    --resource-group rg-productcategory-prod `
    --sku S1
```

**Scale Out (Horizontal)**
```powershell
# Add more instances
az appservice plan update `
    --name app-productcategory-001-plan `
    --resource-group rg-productcategory-prod `
    --number-of-workers 2
```

**Enable Application Insights Profiler**
```powershell
# Portal ? Application Insights ? Profiler ? Enable
```

---

### 8. Staging Slot Swap Fails

#### Symptom
```
Error: Swap operation failed
```

#### Solution
```powershell
# Verify both slots are running
az webapp show --name app-productcategory-001 --resource-group rg-productcategory-prod --query state
az webapp show --name app-productcategory-001 --slot staging --resource-group rg-productcategory-prod --query state

# Warm up staging slot first
Invoke-WebRequest -Uri "https://app-productcategory-001-staging.azurewebsites.net"

# Perform swap with preview
az webapp deployment slot swap `
    --name app-productcategory-001 `
    --resource-group rg-productcategory-prod `
    --slot staging `
    --target-slot production
```

---

### 9. High Costs / Budget Exceeded

#### Diagnosis
```powershell
# Check current costs
az consumption usage list `
    --start-date 2024-01-01 `
    --end-date 2024-01-31

# Portal ? Cost Management ? Cost Analysis
```

#### Solutions

**Optimize App Service Plan**
```powershell
# Scale down during off-hours (use Azure Automation)
# Or use B1 tier instead of S1
az appservice plan update `
    --name app-productcategory-001-plan `
    --resource-group rg-productcategory-prod `
    --sku B1
```

**Optimize SQL Database**
```powershell
# Use Basic tier for dev/test
az sql db update `
    --name ProductDb `
    --server sql-productcategory-001 `
    --resource-group rg-productcategory-prod `
    --service-objective Basic
```

**Set Budget Alerts**
```powershell
# Portal ? Cost Management ? Budgets ? Create
```

---

### 10. Cannot Access Application Insights Data

#### Symptom
No telemetry data appearing in Application Insights

#### Solution
```powershell
# Verify connection string is set
az webapp config appsettings list `
    --name app-productcategory-001 `
    --resource-group rg-productcategory-prod `
    | grep APPLICATIONINSIGHTS

# Check instrumentation key in code
# Should be automatically configured in Program.cs

# Wait 2-3 minutes for data to appear
# If still no data, check application logs
```

---

## Diagnostic Commands Reference

### App Service
```powershell
# View all app settings
az webapp config appsettings list --name <app-name> --resource-group <rg-name>

# Restart app
az webapp restart --name <app-name> --resource-group <rg-name>

# Stop app
az webapp stop --name <app-name> --resource-group <rg-name>

# Start app
az webapp start --name <app-name> --resource-group <rg-name>

# View deployment history
az webapp deployment list-publishing-credentials --name <app-name> --resource-group <rg-name>

# SSH into app
az webapp ssh --name <app-name> --resource-group <rg-name>
```

### Key Vault
```powershell
# List secrets
az keyvault secret list --vault-name <kv-name>

# Get secret value
az keyvault secret show --vault-name <kv-name> --name <secret-name>

# Set secret
az keyvault secret set --vault-name <kv-name> --name <secret-name> --value <value>

# Check access policies
az keyvault show --name <kv-name> --query properties.accessPolicies
```

### SQL Database
```powershell
# List databases
az sql db list --server <sql-server> --resource-group <rg-name>

# Show database details
az sql db show --name <db-name> --server <sql-server> --resource-group <rg-name>

# Update database tier
az sql db update --name <db-name> --server <sql-server> --resource-group <rg-name> --service-objective Basic
```

### Application Insights
```powershell
# Query logs (KQL)
az monitor app-insights query `
    --app <app-insights-name> `
    --resource-group <rg-name> `
    --analytics-query "requests | take 10"
```

---

## Health Check Endpoints

Add to your app for monitoring:

**Create**: `Web/Pages/Health.cshtml.cs`
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

public class HealthModel : PageModel
{
    public IActionResult OnGet()
    {
        return new JsonResult(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}
```

**Test**:
```powershell
Invoke-WebRequest -Uri "https://app-productcategory-001.azurewebsites.net/health"
```

---

## Support Resources

- **Azure Status**: https://status.azure.com
- **Azure Support**: https://portal.azure.com ? Help + Support
- **App Service Diagnostics**: Portal ? App Service ? Diagnose and solve problems
- **Application Insights Live Metrics**: Real-time monitoring
- **Kudu Console**: https://{app-name}.scm.azurewebsites.net

---

## Emergency Procedures

### Rollback Deployment
```powershell
# Swap back to previous version
az webapp deployment slot swap `
    --name app-productcategory-001 `
    --resource-group rg-productcategory-prod `
    --slot production `
    --target-slot staging
```

### Stop All Traffic
```powershell
# Stop the app
az webapp stop --name app-productcategory-001 --resource-group rg-productcategory-prod
```

### Scale to Zero (Save Costs)
```powershell
# Scale down to 0 instances (not available on all tiers)
az appservice plan update `
    --name app-productcategory-001-plan `
    --resource-group rg-productcategory-prod `
    --number-of-workers 0
```

### Complete Teardown
```powershell
# Delete everything
az group delete --name rg-productcategory-prod --yes --no-wait
```
