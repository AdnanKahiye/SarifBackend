using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests.Accounts
{
public class CreateExpenseDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public Guid CashAccountId { get; set; } // lacagta laga bixinaayo
}}
