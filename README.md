![Header image](https://github.com/DougChisholm/App-Mod-Assist/blob/main/repo-header.png)

# App-Mod-Assist - Expense Management System

A modernized cloud-native expense management application built on Azure, demonstrating how GitHub coding agent can transform legacy applications into modern, scalable solutions.

## 🚀 Quick Start

### Prerequisites
- Azure CLI installed and configured
- Azure subscription
- Existing Azure SQL Database (or use dummy data mode)

### Deploy to Azure

1. **Login to Azure:**
   ```bash
   az login
   az account set --subscription "<your-subscription-id>"
   ```

2. **Deploy the application:**
   ```bash
   ./deploy.sh
   ```

3. **Access your application:**
   - Navigate to: `https://<your-app-name>.azurewebsites.net/Index`
   - **Important:** Use `/Index` path, not just the root URL

For detailed deployment instructions, see [DEPLOYMENT.md](DEPLOYMENT.md)

## 📋 Features

- **Dashboard**: Overview of the expense management system
- **Add Expense**: Submit new expenses with category, amount, and description
- **View Expenses**: List and filter all expenses with status indicators
- **Approve Expenses**: Manager view for approving pending expenses
- **REST API**: Full RESTful API with Swagger documentation at `/swagger`

## 🏗️ Architecture

- **Frontend**: ASP.NET Core 8.0 Razor Pages with Bootstrap 5
- **Backend**: RESTful API with automatic Swagger/OpenAPI documentation
- **Database**: Azure SQL Database with Managed Identity authentication
- **Infrastructure**: Bicep templates for reproducible deployments
- **Hosting**: Azure App Service (Linux, .NET 8.0)

## 🔒 Security

- HTTPS enforced with minimum TLS 1.2
- Azure Managed Identity for database access (no connection strings!)
- FTP disabled
- Parameterized SQL queries prevent injection attacks
- Automatic fallback to dummy data if database unavailable

## 📸 Screenshots

See the [Modern-Screenshots](./Modern-Screenshots) folder for UI screenshots of the modernized application.

## 🛠️ Development

### Build
```bash
cd src/ExpenseManagement/ExpenseManagement
dotnet build
```

### Run Locally
```bash
dotnet run --urls "http://localhost:5000"
```

Then navigate to: http://localhost:5000/Index

## 📚 API Documentation

Once deployed, access the interactive API documentation at:
- `https://<your-app-url>/swagger`

Available endpoints:
- `GET /api/Expenses` - Get all expenses
- `GET /api/Expenses/pending` - Get pending expenses
- `POST /api/Expenses` - Create new expense
- `POST /api/Expenses/{id}/approve` - Approve expense
- `GET /api/Expenses/categories` - Get categories

## 📄 License

See [LICENSE](LICENSE) for details.

---

**Note**: This is a proof-of-concept application for demonstration purposes. For production use, review the PRODUCTION_CONSIDERATIONS documentation.
