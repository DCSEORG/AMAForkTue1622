using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExpenseManagement.Models;
using ExpenseManagement.Services;

namespace ExpenseManagement.Pages;

public class AddExpenseModel : PageModel
{
    private readonly ExpenseService _expenseService;
    private readonly ILogger<AddExpenseModel> _logger;

    [BindProperty]
    public decimal Amount { get; set; }

    [BindProperty]
    public DateTime ExpenseDate { get; set; } = DateTime.Now;

    [BindProperty]
    public int CategoryId { get; set; }

    [BindProperty]
    public string? Description { get; set; }

    public List<ExpenseCategory> Categories { get; set; } = new();
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public AddExpenseModel(ExpenseService expenseService, ILogger<AddExpenseModel> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        Categories = await _expenseService.GetCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Categories = await _expenseService.GetCategoriesAsync();

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please fill in all required fields.";
            return Page();
        }

        try
        {
            var expense = new Expense
            {
                CategoryId = CategoryId,
                AmountMinor = (int)(Amount * 100), // Convert pounds to pence
                Currency = "GBP",
                ExpenseDate = ExpenseDate,
                Description = Description
            };

            var expenseId = await _expenseService.CreateExpenseAsync(expense);
            
            SuccessMessage = $"Expense submitted successfully! Expense ID: {expenseId}";
            
            // Clear form
            Amount = 0;
            ExpenseDate = DateTime.Now;
            CategoryId = 0;
            Description = null;

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            ErrorMessage = "An error occurred while submitting the expense. Please try again.";
            return Page();
        }
    }
}
