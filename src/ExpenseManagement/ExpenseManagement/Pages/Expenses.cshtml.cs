using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExpenseManagement.Models;
using ExpenseManagement.Services;

namespace ExpenseManagement.Pages;

public class ExpensesModel : PageModel
{
    private readonly ExpenseService _expenseService;
    private readonly ILogger<ExpensesModel> _logger;

    public List<Expense> Expenses { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public string? Filter { get; set; }

    public ExpensesModel(ExpenseService expenseService, ILogger<ExpensesModel> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            Expenses = await _expenseService.GetExpensesAsync(Filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading expenses");
            Expenses = new List<Expense>();
        }
    }
}
