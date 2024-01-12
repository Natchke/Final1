using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ATM_Consol
{
    public class CardDetails
    {
        public string CardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string CVC { get; set; }
        public decimal Balance { get; set; }
    }

    public class Transaction
    {
        public string TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountUSD { get; set; }
        public decimal AmountEUR { get; set; }
    }

    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public CardDetails CardDetails { get; set; }
        public string PinCode { get; set; }
        public decimal Balance { get; set; }
        public decimal BalanceUSD { get; set; }
        public decimal BalanceEUR { get; set; }
        public List<Transaction> TransactionHistory { get; set; }
    }

    internal class Program
    {
        static string jsonFilePath = @"C:\Users\lenovo\source\repos\projectfinalVOL3\projectfinalVOL3\UserData.json";

        static void Main(string[] args)
        {
            User user = LoadUserData();
            if (user == null) return;

            while (true)
            {
                try
                {
                    Console.WriteLine("Please enter your card number:");
                    string enteredCardNumber = Console.ReadLine();

                    Console.WriteLine("Please enter your card CVC:");
                    string enteredCVC = Console.ReadLine();

                    Console.WriteLine("Please enter your card expiration date (MM/YY):");
                    string enteredExpirationDate = Console.ReadLine();

                    if (ValidateCardDetails(enteredCardNumber, enteredCVC, enteredExpirationDate, user.CardDetails))
                    {
                        Console.WriteLine("Please enter your PIN:");
                        string enteredPin = Console.ReadLine();
                        if (enteredPin == user.PinCode)
                        {
                            ShowMenu(user);
                        }
                        else
                        {
                            Console.WriteLine("Invalid PIN.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid card details.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                }
            }
        }

        private static User LoadUserData()
        {
            try
            {
                var jsonString = File.ReadAllText(jsonFilePath);
                return JsonConvert.DeserializeObject<User>(jsonString);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Error: User data file not found: " + jsonFilePath);
            }
            catch (JsonException)
            {
                Console.WriteLine("Error: User data is not in the correct format: " + jsonFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }

            return null;
        }

        public static bool ValidateCardDetails(string enteredCardNumber, string enteredCVC, string enteredExpirationDate, CardDetails cardDetails)
        {
            string cleanEnteredCardNumber = CleanNumericInput(enteredCardNumber);
            string cleanEnteredExpirationDate = CleanNumericInput(enteredExpirationDate);
            string cleanEnteredCVC = CleanNumericInput(enteredCVC);

            string cleanStoredCardNumber = CleanNumericInput(cardDetails.CardNumber);
            string cleanStoredExpirationDate = CleanNumericInput(cardDetails.ExpirationDate);
            string cleanStoredCVC = CleanNumericInput(cardDetails.CVC);

            return cleanEnteredCardNumber == cleanStoredCardNumber && cleanEnteredExpirationDate == cleanStoredExpirationDate && cleanEnteredCVC == cleanStoredCVC;
        }

        private static void ShowMenu(User user)
        {
            bool exitMenu = false;
            while (!exitMenu)
            {
                Console.WriteLine("\nSelect an option:");
                Console.WriteLine("1. Check Balance");
                Console.WriteLine("2. Display Withdrawals");
                Console.WriteLine("3. View Last 5 Transactions");
                Console.WriteLine("4. Display Deposits");
                Console.WriteLine("5. Change PIN");
                Console.WriteLine("6. Convert Currency");
                Console.WriteLine("7. Exit");

                switch (Console.ReadLine())
                {
                    case "1":
                        CheckBalance(user);
                        break;
                    case "2":
                        WithdrawAmount(user);
                        break;
                    case "3":
                        DisplayLastFiveTransactions(user);
                        break;
                    case "4":
                        DepositAmount(user);
                        break;
                    case "5":
                        ChangePIN(user);
                        break;
                    case "6":
                        ChangeCurrencyDisplay(user);
                        break;
                    case "7":
                        exitMenu = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option, please try again.");
                        break;
                }

                SaveUserData(jsonFilePath, user);
            }
        }

        private static decimal ValidateAmount(string action)
        {
            decimal amount;
            while (true)
            {
                Console.WriteLine($"Enter the amount to {action}:");
                if (Decimal.TryParse(Console.ReadLine(), out amount) && amount > 0)
                {
                    return amount;
                }
                else
                {
                    Console.WriteLine("Invalid amount. Please enter a positive number.");
                }
            }
        }

        private static void CheckBalance(User user)
        {
            Console.WriteLine($"Your current balance is:");
            Console.WriteLine($"Balance: {user.Balance}");
            Console.WriteLine($"BalanceUSD: {user.BalanceUSD}");
            Console.WriteLine($"BalanceEUR: {user.BalanceEUR}");
            AddTransaction(user, "Balance Inquiry", 0);
        }

        private static void WithdrawAmount(User user)
        {
            decimal amount = ValidateAmount("withdraw");
            if (amount > user.Balance)
            {
                Console.WriteLine("Insufficient balance.");
            }
            else
            {
                user.Balance -= amount;
                Console.WriteLine($"Withdrawal successful. Your new balance is: {user.Balance}");
                AddTransaction(user, "Withdrawal", amount);
            }
        }

        private static void DepositAmount(User user)
        {
            decimal amount = ValidateAmount("deposit");
            user.Balance += amount;
            Console.WriteLine($"Deposit successful. Your new balance is: {user.Balance}");
            AddTransaction(user, "Deposit", amount);
        }

        private static void ChangePIN(User user)
        {
            Console.WriteLine("Enter your current PIN:");
            string currentPin = Console.ReadLine();
            if (currentPin == user.PinCode)
            {
                Console.WriteLine("Enter your new PIN:");
                user.PinCode = Console.ReadLine();
                var newpin = user.PinCode.ToString();

                Console.WriteLine("PIN changed successfully.");

                AddTransaction(user, "Change PIN", 0);
            }
            else
            {
                Console.WriteLine("Incorrect current PIN.");
            }
        }

        private static void ChangeCurrencyDisplay(User user)
        {
            Console.WriteLine("Select currency to convert to:");
            Console.WriteLine("1. Convert to USD");
            Console.WriteLine("2. Convert to EUR");

            if (int.TryParse(Console.ReadLine(), out int currencyChoice) && (currencyChoice == 1 || currencyChoice == 2))
            {
                Console.WriteLine("Enter the amount to convert:");
                decimal amountToConvert;
                if (decimal.TryParse(Console.ReadLine(), out amountToConvert) && amountToConvert > 0 && amountToConvert <= user.Balance)
                {
                    decimal conversionRate = (currencyChoice == 1) ? 2.6m : 2.9m; // Example conversion rates
                    decimal convertedAmount = ConvertCurrency(amountToConvert, conversionRate);

                    if (currencyChoice == 1)
                    {
                        user.BalanceUSD += convertedAmount;
                        user.Balance -= amountToConvert;
                        Console.WriteLine($"Converted {amountToConvert} to USD. Your new USD balance is: {user.BalanceUSD}");
                    }
                    else if (currencyChoice == 2)
                    {
                        user.BalanceEUR += convertedAmount;
                        user.Balance -= amountToConvert;
                        Console.WriteLine($"Converted {amountToConvert} to EUR. Your new EUR balance is: {user.BalanceEUR}");
                    }

                    AddTransaction(user, $"Converted {amountToConvert} to {(currencyChoice == 1 ? "USD" : "EUR")}", convertedAmount);
                }
                else
                {
                    Console.WriteLine("Invalid amount. Please enter a positive number and ensure it does not exceed your current balance.");
                }
            }
            else
            {
                Console.WriteLine("Invalid currency choice. Please try again.");
            }
        }

        private static decimal ConvertCurrency(decimal amount, decimal conversionRate)
        {
            return amount / conversionRate;
        }

        private static void DisplayLastFiveTransactions(User user)
        {
            if (user.TransactionHistory == null || !user.TransactionHistory.Any())
            {
                Console.WriteLine("No transaction history available.");
                return;
            }

            try
            {
                var lastFiveTransactions = user.TransactionHistory
                    .Select(t => new
                    {
                        Transaction = t,
                        Date = DateTime.Parse(t.TransactionDate)
                    })
                    .OrderByDescending(t => t.Date)
                    .Take(5)
                    .Select(t => t.Transaction);

                Console.WriteLine("\nLast 5 Transactions:");
                foreach (var transaction in lastFiveTransactions)
                {
                    Console.WriteLine($"Date: {transaction.TransactionDate}, Type: {transaction.TransactionType}, Amount: {transaction.Amount}");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Error: Invalid date format in transaction history.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }

        private static void AddTransaction(User user, string transactionType, decimal amount)
        {
            Transaction newTransaction = new Transaction
            {
                TransactionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                TransactionType = transactionType,
                Amount = amount
            };

            user.TransactionHistory.Add(newTransaction);
        }

        public static string CleanNumericInput(string input)
        {
            return Regex.Replace(input, "[^0-9]", "");
        }

        private static void SaveUserData(string filePath, User user)
        {
            try
            {
                string updatedJson = JsonConvert.SerializeObject(user, Formatting.Indented);
                File.WriteAllText(filePath, updatedJson);
            }
            catch (IOException)
            {
                Console.WriteLine("Error: Failed to save user data.");
            }
            catch (JsonException)
            {
                Console.WriteLine("Error: Problem encountered during user data serialization.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred while saving data: {ex.Message}");
            }
        }
    }
}