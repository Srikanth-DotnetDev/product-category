# Azure App Service Deployment Guide

This guide covers deploying the Product Category Razor Pages application to Azure App Service with complete CI/CD.

## ?? Prerequisites

- Azure subscription (AZ-104/305 certified ?)
- Azure CLI installed: `az --version`
- .NET 10 SDK installed
- GitHub repository access

## ?? Architecture Overview

```
GitHub Repository
    ? (Push to master)
GitHub Actions CI/CD
    ?
Azure App Service (Production + Staging)
    ??? Managed Identity
    ??? Application Insights
    ??? Azure Key Vault
    ?
Azure SQL Database
```

## ?? Deployment Steps

### Step 1: Azure Login

```powershell
az login
az account set --subscription "Your-Subscription-Name"
```

### Step 2: Deploy Azure Resources

```powershell
cd deployment

# Run deployment script
.\deploy.ps1 `
    -ResourceGroupName "rg-productcategory-prod" `
    -Location "eastus" `
    -AppServiceName "app-productcategory-001" `
    -KeyVaultName "kv-prodcat-001" `
    -SqlServerName "sql-productcategory-001" `
    -SqlAdminLogin "sqladmin" `
    -SqlAdminPassword (ConvertTo-SecureString "YourStrongPassword123!" -AsPlainText -Force) `
    -AppServicePlanSku "B1"
```

**Resource Naming Convention (AZ-104 Best Practice):**
- Resource Group: `rg-{project}-{environment}`
- App Service: `app-{project}-{instance}`
- Key Vault: `kv-{project}-{instance}` (max 24 chars)
- SQL Server: `sql-{project}-{instance}`

### Step 3: Configure GitHub Secrets

1. **Get Publish Profile:**
```powershell
az webapp deployment list-publishing-profiles `
    --name app-productcategory-001 `
    --resource-group rg-productcategory-prod `
    --xml
```

2. **Add to GitHub Secrets:**
   - Go to: `https://github.com/Srikanth-DotnetDev/product-category/settings/secrets/actions`
   - Click "New repository secret"
   - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Value: Paste the XML from step 1

### Step 4: Update GitHub Actions Workflow

Edit `.github/workflows/azure-deploy.yml`:
```yaml
env:
  AZURE_WEBAPP_NAME: 'app-productcategory-001'  # Your actual app name
```

### Step 5: Update Configuration

Edit `Web/appsettings.Production.json`:
```json
{
  "AzureKeyVault": {
    "VaultUrl": "https://kv-prodcat-001.vault.azure.net/"
  }
}
```

### Step 6: Deploy

```bash
git add .
git commit -m "Configure Azure deployment"
git push origin master
```

GitHub Actions will automatically:
- ? Build the application
- ? Run tests
- ? Publish artifacts
- ? Deploy to Azure App Service

## ?? Azure Portal Configuration

### Enable Deployment Slots (AZ-104)

1. Navigate to App Service ? Deployment slots
2. Click "Add Slot"
3. Name: `staging`
4. Clone settings from: Production
5. Click "Add"

### Configure Custom Domain (AZ-104)

1. App Service ? Custom domains
2. Add custom domain: `www.yourapp.com`
3. Validate domain ownership
4. Add SSL certificate (Free App Service Managed Certificate)

### Configure Autoscaling (AZ-104)

1. App Service Plan ? Scale out
2. Custom autoscale:
   - Minimum instances: 1
   - Maximum instances: 3
   - Scale rule: CPU > 70% ? Add 1 instance
   - Scale rule: CPU < 30% ? Remove 1 instance

### Enable Application Insights (AZ-104)

Already configured via Bicep template!

**View metrics:**
```
Application Insights ? Live Metrics
Application Insights ? Performance
Application Insights ? Failures
```

## ?? Security Configuration (AZ-305)

### Managed Identity Setup

? Already configured in Bicep template

**Verify:**
```powershell
az webapp identity show `
    --name app-productcategory-001 `
    --resource-group rg-productcategory-prod
```

### Key Vault Access

? Already configured with RBAC (Key Vault Secrets User role)

**Add additional secrets:**
```powershell
az keyvault secret set `
    --vault-name kv-prodcat-001 `
    --name "ExternalApiKey" `
    --value "your-api-key-here"
```

### Network Security

**Restrict access to specific IPs:**
```powershell
az webapp config access-restriction add `
    --name app-productcategory-001 `
    --resource-group rg-productcategory-prod `
    --rule-name "OfficeIP" `
    --priority 100 `
    --ip-address "1.2.3.4/32"
```

## ?? Monitoring & Diagnostics (AZ-104)

### View Logs

```powershell
# Stream logs
az webapp log tail `
    --name app-productcategory-001 `
    --resource-group rg-productcategory-prod

# Download logs
az webapp log download `
    --name app-productcategory-001 `
    --resource-group rg-productcategory-prod
```

### Application Insights Queries

**KQL Query Examples:**

```kql
// Request count by hour
requests
| summarize count() by bin(timestamp, 1h)

// Failed requests
requests
| where success == false
| project timestamp, name, resultCode, duration

// Top 5 slowest pages
requests
| summarize avg(duration) by name
| top 5 by avg_duration desc
```

## ?? Deployment Strategies (AZ-305)

### Blue-Green Deployment

1. Deploy to staging slot
2. Test: `https://app-productcategory-001-staging.azurewebsites.net`
3. Swap to production:
```powershell
az webapp deployment slot swap `
    --name app-productcategory-001 `
    --resource-group rg-productcategory-prod `
    --slot staging `
    --target-slot production
```

### Rollback

```powershell
az webapp deployment slot swap `
    --name app-productcategory-001 `
    --resource-group rg-productcategory-prod `
    --slot production `
    --target-slot staging
```

## ?? Cost Optimization (AZ-104)

**Current Costs (Estimated):**
- App Service Plan (B1): ~$13/month
- Azure SQL (Basic): ~$5/month
- Key Vault: ~$0.03/10k operations
- Application Insights: First 5GB free

**Optimization Tips:**
- Use Free tier for dev/test
- Enable autoscale to scale down during off-hours
- Use reserved capacity for predictable workloads (57% savings)

## ?? Testing Checklist

- [ ] Application loads successfully
- [ ] Managed Identity connects to Key Vault
- [ ] Database connection works
- [ ] Application Insights receives telemetry
- [ ] Staging slot accessible
- [ ] SSL certificate valid
- [ ] Custom domain resolves (if configured)

## ?? Useful Commands

```powershell
# Restart app
az webapp restart --name app-productcategory-001 --resource-group rg-productcategory-prod

# View app settings
az webapp config appsettings list --name app-productcategory-001 --resource-group rg-productcategory-prod

# Set app setting
az webapp config appsettings set --name app-productcategory-001 --resource-group rg-productcategory-prod --settings KEY=VALUE

# Scale up/down
az appservice plan update --name app-productcategory-001-plan --resource-group rg-productcategory-prod --sku S1
```

## ?? AZ-104 & AZ-305 Skills Demonstrated

? Resource provisioning & management  
? Managed Identity implementation  
? Key Vault integration with RBAC  
? Application Insights monitoring  
? Deployment slots & blue-green deployment  
? Autoscaling configuration  
? Network security (firewall rules)  
? Infrastructure as Code (Bicep)  
? CI/CD pipeline (GitHub Actions)  
? Cost management & optimization  

## ?? Troubleshooting

**App won't start:**
```powershell
az webapp log tail --name app-productcategory-001 --resource-group rg-productcategory-prod
```

**Key Vault access denied:**
- Verify Managed Identity is enabled
- Check RBAC role assignment
- Ensure correct Key Vault URL in config

**Database connection fails:**
- Check SQL firewall rules
- Verify connection string in Key Vault
- Test with SQL Server Management Studio

## ?? Support

- Azure Documentation: https://docs.microsoft.com/azure
- GitHub Issues: https://github.com/Srikanth-DotnetDev/product-category/issues
