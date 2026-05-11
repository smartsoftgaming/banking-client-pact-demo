namespace BankingPlatform.Client.Models;

public class BalanceResponse
{
    public int AccountId { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = default!;
}