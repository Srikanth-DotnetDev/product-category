# Azure Deployment Script for Product Category App
# Prerequisites: Azure CLI installed and logged in (az login)

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$Location = "eastus",
    
    [Parameter(Mandatory=$true)]
    [string]$AppServiceName,
    
    [Parameter(Mandatory=$true)]
    [string]$KeyVaultName,
    
    [Parameter(Mandatory=$true)]
    [string]$SqlServerName,
    
    [Parameter(Mandatory=$true)]
    [string]$SqlAdminLogin,
    
    [Parameter(Mandatory=$true)]
    [SecureString]$SqlAdminPassword,
    
    [string]$AppServicePlanSku = "B1"
)

Write-Host "?? Starting Azure deployment..." -ForegroundColor Cyan

# Create Resource Group
Write-Host "?? Creating resource group: $ResourceGroupName" -ForegroundColor Yellow
az group create --name $ResourceGroupName --location $Location

# Deploy Bicep template
Write-Host "?? Deploying Azure resources using Bicep..." -ForegroundColor Yellow
$deployment = az deployment group create `
    --resource-group $ResourceGroupName `
    --template-file "./azure-resources.bicep" `
    --parameters `
        appServiceName=$AppServiceName `
        location=$Location `
        appServicePlanSku=$AppServicePlanSku `
        keyVaultName=$KeyVaultName `
        sqlServerName=$SqlServerName `
        sqlAdminLogin=$SqlAdminLogin `
        sqlAdminPassword=$SqlAdminPassword `
        appInsightsName="$AppServiceName-insights" `
    --query 'properties.outputs' `
    --output json | ConvertFrom-Json

Write-Host "? Deployment complete!" -ForegroundColor Green

# Display outputs
Write-Host "`n?? Deployment Information:" -ForegroundColor Cyan
Write-Host "App Service URL: $($deployment.appServiceUrl.value)" -ForegroundColor White
Write-Host "Key Vault URL: $($deployment.keyVaultUrl.value)" -ForegroundColor White
Write-Host "SQL Server: $($deployment.sqlServerFqdn.value)" -ForegroundColor White
Write-Host "Application Insights: $($deployment.appInsightsConnectionString.value)" -ForegroundColor White

# Store SQL connection string in Key Vault
Write-Host "`n?? Storing SQL connection string in Key Vault..." -ForegroundColor Yellow
$connectionString = "Server=tcp:$($deployment.sqlServerFqdn.value),1433;Initial Catalog=ProductDb;Persist Security Info=False;User ID=$SqlAdminLogin;Password=$SqlAdminPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
az keyvault secret set --vault-name $KeyVaultName --name "DatabaseConnectionString" --value $connectionString

Write-Host "`n? All resources deployed successfully!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Configure GitHub Actions secrets" -ForegroundColor White
Write-Host "2. Push code to trigger deployment" -ForegroundColor White
Write-Host "3. Visit: $($deployment.appServiceUrl.value)" -ForegroundColor White
