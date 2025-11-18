using Microsoft.Data.SqlClient;
using Azure.Identity;
using Azure.Core;
using ExpenseManagement.Models;

namespace ExpenseManagement.Services;

public class ExpenseService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExpenseService> _logger;
    private bool _useDummyData = false;

    public ExpenseService(IConfiguration configuration, ILogger<ExpenseService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private SqlConnection? GetConnection()
    {
        var sqlServer = _configuration["SQL_SERVER"] ?? "sql-expense-mgmt-xyz.database.windows.net";
        var sqlDatabase = _configuration["SQL_DATABASE"] ?? "ExpenseManagementDB";
        var managedIdentityClientId = _configuration["MANAGED_IDENTITY_CLIENT_ID"];

        string connectionString;
        
        if (!string.IsNullOrEmpty(managedIdentityClientId))
        {
            // Use Managed Identity (Azure deployment)
            connectionString = $"Server=tcp:{sqlServer},1433;" +
                             $"Database={sqlDatabase};" +
                             "Authentication=Active Directory Managed Identity;" +
                             $"User Id={managedIdentityClientId};";
            return new SqlConnection(connectionString);
        }
        else
        {
            // Fallback for local development - use dummy data
            _logger.LogWarning("Managed Identity not configured. Using dummy data.");
            _useDummyData = true;
            return null;
        }
    }

    public async Task<List<Expense>> GetExpensesAsync(string? filter = null)
    {
        if (_useDummyData)
        {
            return GetDummyExpenses(filter);
        }

        try
        {
            using var connection = GetConnection();
            if (connection == null)
            {
                return GetDummyExpenses(filter);
            }

            await connection.OpenAsync();

            var query = @"
                SELECT e.ExpenseId, e.UserId, e.CategoryId, e.StatusId, e.AmountMinor, 
                       e.Currency, e.ExpenseDate, e.Description, e.SubmittedAt, e.CreatedAt,
                       u.UserName, c.CategoryName, s.StatusName
                FROM dbo.Expenses e
                JOIN dbo.Users u ON e.UserId = u.UserId
                JOIN dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
                JOIN dbo.ExpenseStatus s ON e.StatusId = s.StatusId
                ORDER BY e.ExpenseDate DESC";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var expenses = new List<Expense>();
            while (await reader.ReadAsync())
            {
                expenses.Add(new Expense
                {
                    ExpenseId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    CategoryId = reader.GetInt32(2),
                    StatusId = reader.GetInt32(3),
                    AmountMinor = reader.GetInt32(4),
                    Currency = reader.GetString(5),
                    ExpenseDate = reader.GetDateTime(6),
                    Description = reader.IsDBNull(7) ? null : reader.GetString(7),
                    SubmittedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                    CreatedAt = reader.GetDateTime(9),
                    UserName = reader.GetString(10),
                    CategoryName = reader.GetString(11),
                    StatusName = reader.GetString(12)
                });
            }

            return expenses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to database. Using dummy data.");
            _useDummyData = true;
            return GetDummyExpenses(filter);
        }
    }

    public async Task<List<Expense>> GetPendingExpensesAsync()
    {
        if (_useDummyData)
        {
            return GetDummyExpenses().Where(e => e.StatusName == "Submitted").ToList();
        }

        try
        {
            using var connection = GetConnection();
            if (connection == null)
            {
                return GetDummyExpenses().Where(e => e.StatusName == "Submitted").ToList();
            }

            await connection.OpenAsync();

            var query = @"
                SELECT e.ExpenseId, e.UserId, e.CategoryId, e.StatusId, e.AmountMinor, 
                       e.Currency, e.ExpenseDate, e.Description, e.SubmittedAt, e.CreatedAt,
                       u.UserName, c.CategoryName, s.StatusName
                FROM dbo.Expenses e
                JOIN dbo.Users u ON e.UserId = u.UserId
                JOIN dbo.ExpenseCategories c ON e.CategoryId = c.CategoryId
                JOIN dbo.ExpenseStatus s ON e.StatusId = s.StatusId
                WHERE s.StatusName = 'Submitted'
                ORDER BY e.SubmittedAt ASC";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var expenses = new List<Expense>();
            while (await reader.ReadAsync())
            {
                expenses.Add(new Expense
                {
                    ExpenseId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    CategoryId = reader.GetInt32(2),
                    StatusId = reader.GetInt32(3),
                    AmountMinor = reader.GetInt32(4),
                    Currency = reader.GetString(5),
                    ExpenseDate = reader.GetDateTime(6),
                    Description = reader.IsDBNull(7) ? null : reader.GetString(7),
                    SubmittedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                    CreatedAt = reader.GetDateTime(9),
                    UserName = reader.GetString(10),
                    CategoryName = reader.GetString(11),
                    StatusName = reader.GetString(12)
                });
            }

            return expenses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to database. Using dummy data.");
            _useDummyData = true;
            return GetDummyExpenses().Where(e => e.StatusName == "Submitted").ToList();
        }
    }

    public async Task<List<ExpenseCategory>> GetCategoriesAsync()
    {
        if (_useDummyData)
        {
            return GetDummyCategories();
        }

        try
        {
            using var connection = GetConnection();
            if (connection == null)
            {
                return GetDummyCategories();
            }

            await connection.OpenAsync();

            var query = "SELECT CategoryId, CategoryName, IsActive FROM dbo.ExpenseCategories WHERE IsActive = 1";
            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var categories = new List<ExpenseCategory>();
            while (await reader.ReadAsync())
            {
                categories.Add(new ExpenseCategory
                {
                    CategoryId = reader.GetInt32(0),
                    CategoryName = reader.GetString(1),
                    IsActive = reader.GetBoolean(2)
                });
            }

            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to database. Using dummy data.");
            _useDummyData = true;
            return GetDummyCategories();
        }
    }

    public async Task<int> CreateExpenseAsync(Expense expense)
    {
        if (_useDummyData)
        {
            _logger.LogInformation("Dummy data mode: Expense would be created");
            return new Random().Next(1000, 9999);
        }

        try
        {
            using var connection = GetConnection();
            if (connection == null)
            {
                _logger.LogInformation("Dummy data mode: Expense would be created");
                return new Random().Next(1000, 9999);
            }

            await connection.OpenAsync();

            var query = @"
                INSERT INTO dbo.Expenses (UserId, CategoryId, StatusId, AmountMinor, Currency, ExpenseDate, Description, SubmittedAt, CreatedAt)
                VALUES (@UserId, @CategoryId, @StatusId, @AmountMinor, @Currency, @ExpenseDate, @Description, @SubmittedAt, SYSUTCDATETIME());
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", 1); // Default user Alice
            command.Parameters.AddWithValue("@CategoryId", expense.CategoryId);
            command.Parameters.AddWithValue("@StatusId", 2); // Submitted status
            command.Parameters.AddWithValue("@AmountMinor", expense.AmountMinor);
            command.Parameters.AddWithValue("@Currency", expense.Currency);
            command.Parameters.AddWithValue("@ExpenseDate", expense.ExpenseDate);
            command.Parameters.AddWithValue("@Description", (object?)expense.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@SubmittedAt", DateTime.UtcNow);

            var result = await command.ExecuteScalarAsync();
            var expenseId = result != null ? (int)result : 0;
            return expenseId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense. Using dummy data.");
            _useDummyData = true;
            return new Random().Next(1000, 9999);
        }
    }

    public async Task<bool> ApproveExpenseAsync(int expenseId)
    {
        if (_useDummyData)
        {
            _logger.LogInformation("Dummy data mode: Expense {ExpenseId} would be approved", expenseId);
            return true;
        }

        try
        {
            using var connection = GetConnection();
            if (connection == null)
            {
                _logger.LogInformation("Dummy data mode: Expense {ExpenseId} would be approved", expenseId);
                return true;
            }

            await connection.OpenAsync();

            var query = @"
                UPDATE dbo.Expenses
                SET StatusId = (SELECT StatusId FROM dbo.ExpenseStatus WHERE StatusName = 'Approved'),
                    ReviewedBy = @ReviewedBy,
                    ReviewedAt = SYSUTCDATETIME()
                WHERE ExpenseId = @ExpenseId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ExpenseId", expenseId);
            command.Parameters.AddWithValue("@ReviewedBy", 2); // Default manager Bob

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving expense. Using dummy data.");
            _useDummyData = true;
            return true;
        }
    }

    private List<Expense> GetDummyExpenses(string? filter = null)
    {
        var expenses = new List<Expense>
        {
            new Expense
            {
                ExpenseId = 1,
                UserId = 1,
                CategoryId = 1,
                StatusId = 2,
                AmountMinor = 12000,
                Currency = "GBP",
                ExpenseDate = new DateTime(2024, 1, 15),
                Description = "Taxi from airport to client site",
                UserName = "Alice Example",
                CategoryName = "Travel",
                StatusName = "Submitted",
                SubmittedAt = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Expense
            {
                ExpenseId = 2,
                UserId = 1,
                CategoryId = 2,
                StatusId = 2,
                AmountMinor = 6900,
                Currency = "GBP",
                ExpenseDate = new DateTime(2023, 1, 10),
                Description = "Client lunch meeting",
                UserName = "Alice Example",
                CategoryName = "Meals",
                StatusName = "Submitted",
                SubmittedAt = DateTime.UtcNow.AddDays(-10),
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Expense
            {
                ExpenseId = 3,
                UserId = 1,
                CategoryId = 3,
                StatusId = 3,
                AmountMinor = 9950,
                Currency = "GBP",
                ExpenseDate = new DateTime(2023, 12, 4),
                Description = "Office stationery",
                UserName = "Alice Example",
                CategoryName = "Supplies",
                StatusName = "Approved",
                SubmittedAt = DateTime.UtcNow.AddDays(-20),
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Expense
            {
                ExpenseId = 4,
                UserId = 1,
                CategoryId = 1,
                StatusId = 2,
                AmountMinor = 1920,
                Currency = "GBP",
                ExpenseDate = new DateTime(2023, 9, 18),
                Description = "Train ticket",
                UserName = "Alice Example",
                CategoryName = "Transport",
                StatusName = "Submitted",
                SubmittedAt = DateTime.UtcNow.AddDays(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        if (!string.IsNullOrWhiteSpace(filter))
        {
            expenses = expenses.Where(e => 
                e.Description?.Contains(filter, StringComparison.OrdinalIgnoreCase) == true ||
                e.CategoryName?.Contains(filter, StringComparison.OrdinalIgnoreCase) == true ||
                e.StatusName?.Contains(filter, StringComparison.OrdinalIgnoreCase) == true
            ).ToList();
        }

        return expenses;
    }

    private List<ExpenseCategory> GetDummyCategories()
    {
        return new List<ExpenseCategory>
        {
            new ExpenseCategory { CategoryId = 1, CategoryName = "Travel", IsActive = true },
            new ExpenseCategory { CategoryId = 2, CategoryName = "Meals", IsActive = true },
            new ExpenseCategory { CategoryId = 3, CategoryName = "Supplies", IsActive = true },
            new ExpenseCategory { CategoryId = 4, CategoryName = "Accommodation", IsActive = true },
            new ExpenseCategory { CategoryId = 5, CategoryName = "Other", IsActive = true }
        };
    }
}
