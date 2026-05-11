namespace BankingPlatform.Pact.Consumer.Tests;

public static class ProviderStates
{
    public static class Accounts
    {
        public const string AccountExistsForBalance = "account 1 exists for balance";
        public const string AccountExistsForOverdraft = "account 1 exists for overdraft";
        public const string AccountNotFound = "account 999 does not exist";
    }

    public static class Users
    {
        public const string UserExists = "user exists";
    }
}

