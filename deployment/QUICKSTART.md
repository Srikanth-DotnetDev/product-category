# ?? Quick Start - Azure Deployment

## Prerequisites Check
```powershell
# Check Azure CLI
az --version

# Check .NET
dotnet --version

# Login to Azure
az login
```

## 5-Minute Deployment

### 1. Set Variables (Update these!)
```powershell
$resourceGroup = "rg-productcategory-prod"
$location = "eastus"
$appName = "app-productcategory-001"
$kvName = "kv-prodcat-001"  # Must be globally unique, 3-24 chars
$sqlName = "sql-productcategory-001"  # Must be globally unique
$sqlAdmin = "sqladmin"
$sqlPassword = "YourStrongP@ssw0rd123!"  # Change this!
```

### 2. Deploy Resources
```powershell
cd deployment

.\deploy.ps1 `
    -ResourceGroupName $resourceGroup `
    -Location $location `
    -AppServiceName $appName `
    -KeyVaultName $kvName `
    -SqlServerName $sqlName `
    -SqlAdminLogin $sqlAdmin `
    -SqlAdminPassword (ConvertTo-SecureString $sqlPassword -AsPlainText -Force)
```

### 3. Get Publish Profile
```powershell
az webapp deployment list-publishing-profiles `
    --name $appName `
    --resource-group $resourceGroup `
    --xml | Out-File publish-profile.xml
```

### 4. Add GitHub Secret
1. Copy content of `publish-profile.xml`
2. Go to: https://github.com/Srikanth-DotnetDev/product-category/settings/secrets/actions
3. Click "New repository secret"
4. Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
5. Paste XML content
6. Click "Add secret"

### 5. Update Workflow File
Edit `.github/workflows/azure-deploy.yml`:
```yaml
env:
  AZURE_WEBAPP_NAME: 'app-productcategory-001'  # Your app name here
```

### 6. Update App Settings
Edit `Web/appsettings.Production.json`:
```json
{
  "AzureKeyVault": {
    "VaultUrl": "https://kv-prodcat-001.vault.azure.net/"
  }
}
```

### 7. Deploy!
```bash
git add .
git commit -m "Configure Azure deployment"
git push origin master
```

### 8. Verify
- Check GitHub Actions: https://github.com/Srikanth-DotnetDev/product-category/actions
- Visit your app: The URL will be shown in deployment output

## ? Success Checklist

- [ ] Azure resources created
- [ ] GitHub secret configured
- [ ] Workflow file updated
- [ ] GitHub Actions completed successfully
- [ ] App accessible in browser
- [ ] Application Insights receiving data
- [ ] Key Vault accessible via Managed Identity

## ?? Important URLs

After deployment:
- **App URL**: `https://{your-app-name}.azurewebsites.net`
- **Staging URL**: `https://{your-app-name}-staging.azurewebsites.net`
- **Azure Portal**: https://portal.azure.com
- **Application Insights**: Portal ? Your Resource Group ? Application Insights
- **GitHub Actions**: https://github.com/Srikanth-DotnetDev/product-category/actions

## ?? Common Issues

**Issue**: "The subscription is not registered to use namespace 'Microsoft.Web'"
```powershell
az provider register --namespace Microsoft.Web
az provider register --namespace Microsoft.Sql
az provider register --namespace Microsoft.KeyVault
```

**Issue**: Key Vault name already exists
- Key Vault names must be globally unique
- Try: `kv-prodcat-{your-initials}-001`

**Issue**: SQL Server name already exists
- SQL Server names must be globally unique
- Try: `sql-prodcat-{your-initials}-001`

**Issue**: GitHub Actions fails
- Check secrets are configured correctly
- Verify app name in workflow file
- Check build logs in GitHub Actions tab

## ?? View Your App

```powershell
# Open in browser
start "https://$appName.azurewebsites.net"

# View logs
az webapp log tail --name $appName --resource-group $resourceGroup
```

## ?? Cleanup (Delete Everything)

```powershell
az group delete --name $resourceGroup --yes --no-wait
```

## ?? Next Steps

1. **Configure Custom Domain** (AZ-104)
   - Portal ? App Service ? Custom domains

2. **Set Up Autoscaling** (AZ-104)
   - Portal ? App Service Plan ? Scale out

3. **Review Application Insights** (AZ-104)
   - Portal ? Application Insights ? Live Metrics

4. **Test Staging Slot** (AZ-104)
   - Visit staging URL
   - Perform slot swap

5. **Configure Monitoring Alerts** (AZ-104)
   - Portal ? Alerts ? New alert rule
