using Microsoft.AspNetCore.Mvc;
using ExpenseManagement.Models;
using ExpenseManagement.Services;

namespace ExpenseManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExpensesController : ControllerBase
{
    private readonly ExpenseService _expenseService;
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(ExpenseService expenseService, ILogger<ExpensesController> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    /// <summary>
    /// Get all expenses
    /// </summary>
    /// <param name="filter">Optional filter string to search expenses</param>
    /// <returns>List of expenses</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<Expense>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Expense>>> GetExpenses([FromQuery] string? filter = null)
    {
        try
        {
            var expenses = await _expenseService.GetExpensesAsync(filter);
            return Ok(expenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expenses");
            return StatusCode(500, "An error occurred while retrieving expenses");
        }
    }

    /// <summary>
    /// Get pending expenses for approval
    /// </summary>
    /// <returns>List of pending expenses</returns>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(List<Expense>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Expense>>> GetPendingExpenses()
    {
        try
        {
            var expenses = await _expenseService.GetPendingExpensesAsync();
            return Ok(expenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending expenses");
            return StatusCode(500, "An error occurred while retrieving pending expenses");
        }
    }

    /// <summary>
    /// Create a new expense
    /// </summary>
    /// <param name="request">Expense creation request</param>
    /// <returns>Created expense ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateExpenseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateExpenseResponse>> CreateExpense([FromBody] CreateExpenseRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var expense = new Expense
            {
                CategoryId = request.CategoryId,
                AmountMinor = request.AmountMinor,
                Currency = "GBP",
                ExpenseDate = request.ExpenseDate,
                Description = request.Description
            };

            var expenseId = await _expenseService.CreateExpenseAsync(expense);
            var response = new CreateExpenseResponse { ExpenseId = expenseId };
            
            return CreatedAtAction(nameof(GetExpenses), new { id = expenseId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            return StatusCode(500, "An error occurred while creating the expense");
        }
    }

    /// <summary>
    /// Approve an expense
    /// </summary>
    /// <param name="id">Expense ID</param>
    /// <returns>Success status</returns>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ApproveExpense(int id)
    {
        try
        {
            var success = await _expenseService.ApproveExpenseAsync(id);
            if (success)
            {
                return Ok(new { message = "Expense approved successfully" });
            }
            return NotFound(new { message = "Expense not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving expense");
            return StatusCode(500, "An error occurred while approving the expense");
        }
    }

    /// <summary>
    /// Get expense categories
    /// </summary>
    /// <returns>List of categories</returns>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<ExpenseCategory>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ExpenseCategory>>> GetCategories()
    {
        try
        {
            var categories = await _expenseService.GetCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }
}

public class CreateExpenseRequest
{
    public int CategoryId { get; set; }
    public int AmountMinor { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Description { get; set; }
}

public class CreateExpenseResponse
{
    public int ExpenseId { get; set; }
}
