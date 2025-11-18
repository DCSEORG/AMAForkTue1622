using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExpenseManagement.Models;
using ExpenseManagement.Services;

namespace ExpenseManagement.Pages;

public class ApproveExpensesModel : PageModel
{
    private readonly ExpenseService _expenseService;
    private readonly ILogger<ApproveExpensesModel> _logger;

    public List<Expense> PendingExpenses { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public string? Filter { get; set; }
    
    public string? SuccessMessage { get; set; }

    public ApproveExpensesModel(ExpenseService expenseService, ILogger<ApproveExpensesModel> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            PendingExpenses = await _expenseService.GetPendingExpensesAsync();
            
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                PendingExpenses = PendingExpenses.Where(e =>
                    e.Description?.Contains(Filter, StringComparison.OrdinalIgnoreCase) == true ||
                    e.CategoryName?.Contains(Filter, StringComparison.OrdinalIgnoreCase) == true
                ).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pending expenses");
            PendingExpenses = new List<Expense>();
        }
    }

    public async Task<IActionResult> OnPostAsync(int expenseId)
    {
        try
        {
            var success = await _expenseService.ApproveExpenseAsync(expenseId);
            
            if (success)
            {
                SuccessMessage = $"Expense #{expenseId} has been approved successfully!";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving expense");
        }

        // Reload pending expenses
        await OnGetAsync();
        return Page();
    }
}
