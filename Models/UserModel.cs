namespace BankingApp.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }
        public decimal Balance { get; set; } = 0;
        public string IBAN { get; set; } = "";
        public string Role { get; set; } = "Client";
        
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





}
