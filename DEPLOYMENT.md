# Deployment Instructions

## Quick Deployment

To deploy the Expense Management System to Azure, run the following command:

```bash
./deploy.sh
```

This will:
1. Create a resource group in UK South
2. Deploy the managed identity (mid-AppModAssist)
3. Deploy the App Service with the application
4. Output the application URL

## Accessing the Application

⚠️ **IMPORTANT**: After deployment, navigate to the application at:

```
https://<your-app-name>.azurewebsites.net/Index
```

**Note**: Navigate to `/Index` (not just the root URL) to access the expense management system.

## Prerequisites

Before running the deployment script:

1. **Azure CLI**: Make sure Azure CLI is installed and you're logged in:
   ```bash
   az login
   az account set --subscription "<your-subscription-id>"
   ```

2. **Set Subscription**: Ensure you're using the correct Azure subscription

## Post-Deployment Steps

After deploying, you need to grant the managed identity access to your Azure SQL database:

1. Get the Managed Identity Principal ID from the deployment output
2. Connect to your Azure SQL Server
3. Run these SQL commands:

```sql
CREATE USER [mid-AppModAssist] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [mid-AppModAssist];
ALTER ROLE db_datawriter ADD MEMBER [mid-AppModAssist];
```

## Features

- **Add Expense**: Submit new expenses for approval
- **View Expenses**: See all expenses with filtering
- **Approve Expenses**: Manager view for approving pending expenses
- **API Documentation**: Access Swagger UI at `/swagger`

## Application Architecture

- **Frontend**: ASP.NET Core 8.0 Razor Pages
- **Backend**: RESTful API with Swagger documentation
- **Database**: Azure SQL Database
- **Authentication**: Azure Managed Identity for secure database access
- **Hosting**: Azure App Service (Linux, .NET 8.0)

## Dummy Data Mode

If the application cannot connect to the database (e.g., managed identity not configured or database permissions not set), it will automatically fall back to using dummy data. This allows you to test the UI and functionality without a database connection.
