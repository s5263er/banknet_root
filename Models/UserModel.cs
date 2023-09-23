using System.ComponentModel.DataAnnotations.Schema;

namespace BankingApp.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }
        public float Balance { get; set; } = 0;
        public string IBAN { get; set; } = "";
        public string Role { get; set; } = "Client";
        
    }
    
    public class UserStock
    {
        public int Id { get; set; }  
        public int UserId { get; set; }  // Foreign key referencing the User who owns the stock
        public string StockSymbol { get; set; }  
        public int Quantity { get; set; } 
        public DateTime PurchaseDate { get; set; }  


        public float PurchasePrice { get; set; } 

        public string BuySell {get; set;}
    }
    public class BuyStockRequest
    {
        public string Symbol { get; set; }
        public int Quantity { get; set; }
        public float PurchasePrice { get; set; }
    }
    public class SellStockRequest
    {
        public string Symbol { get; set; }
        public int Quantity { get; set; }
        public float SellPrice { get; set; }
    }
        public class StockProfitLoss
    {
        public string StockSymbol { get; set; }
        public float ProfitLoss { get; set; }

        public int CurrentQuantity {get; set;}

        public float TotalInvestment {get; set;}
    }




    public class JwtConfig
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SecretKey { get; set; }
    }
    public class YahooFinanceResponse
        {
            public Chart Chart { get; set; }
        }

        public class Chart
        {
            public List<Result> Result { get; set; }
        }

        public class Result
        {
            public List<long> Timestamp { get; set; }
            public Indicators Indicators { get; set; }
        }

        public class Indicators
        {
            public List<Quote> Quote { get; set; }
        }

        public class Quote
        {
            public List<double?> Close { get; set; }
        }

        public class Transaction
        {
            public int Id { get; set; }
            
            public int UserId { get; set; }
            
            public float Amount { get; set; }
            
            public string Type { get; set; }
            
            public DateTime Time { get; set; }
            
        }
        public class DepositRequest
        {
            public float Amount { get; set; }
        }
        public class WithdrawRequest
        {
            public float Amount { get; set; }
        }
        public class TransferRequest
        {
            public string IBAN {get; set;}
            public string Name {get; set;}
            public float Amount {get; set;}
        }

        public class Transfer
        {
            public int Id { get; set; }
            public int FromUserId { get; set; }
            public int ToUserId { get; set; }
            public float Amount { get; set; }
            public DateTime Date {get; set;}
        }
        public class Loan
        {
            public int LoanId { get; set; }
            public int UserId { get; set; }
            public float Amount { get; set; }
            public float InterestRate { get; set; }
            public int TermMonths { get; set; }
            public DateTime ApplicationDate { get; set; }
            public int Approved { get; set; }
            public DateTime ApprovalDate { get; set; }
        }

        public class LoanRequest
        {
            public float Amount {get; set;}
            public float InterestRate {get; set;}
            public int TermMonths {get; set;}


        }



}
