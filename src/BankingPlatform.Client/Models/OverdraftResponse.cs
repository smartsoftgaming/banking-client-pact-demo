namespace BankingPlatform.Client.Models;

public class OverdraftResponse
{
    public int AccountId { get; set; }
    public decimal Overdraft { get; set; }
    public string Currency { get; set; } = default!;
}
