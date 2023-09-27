using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BankingApp.Data;
using BankingApp.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Http.Features;

namespace BankingApp.Controllers
{
    [Route("api")]
    public class RegisterController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public RegisterController(AppDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        [HttpGet("balance")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Balance()
        {
            Console.WriteLine("Balance action accessed.");

            // Retrieve user's email 
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            Console.WriteLine("User email: " + userEmail);

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User email not found in the token.");
            }

            var user = _dbContext.Users.SingleOrDefault(u => u.Email == userEmail);
            Console.WriteLine("User = " + user);

            if (user == null)
            {
                return NotFound("User not found");
            }

            float userBalance = user.Balance;
            Console.WriteLine("Balance Successful");

            return Ok(new { balance = userBalance });
        }

        [HttpPost("deposit")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DepositAsync(DepositRequest request)
        {
            float amount; 
        using (StreamReader reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8))
        {
            string requestBody = await reader.ReadToEndAsync();
            Console.WriteLine("Request Content: " + requestBody);
            Console.WriteLine("RequestBody" + requestBody[0]);
            var depositRequest = JsonConvert.DeserializeObject<DepositRequest>(requestBody);
            amount = depositRequest.Amount;
            Console.WriteLine("AMOUNT: " + amount);
        }
            
            Console.WriteLine("Deposit action accessed.");

            Console.WriteLine("AMOUNT: " + amount);

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User not found in the token.");
            }

            var user = _dbContext.Users.SingleOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound("User not found");
            }
            var transaction = new Transaction
            {
                UserId = user.UserId,
                Amount = amount,
                Type = "Deposit",
                Time = DateTime.UtcNow
            };
            _dbContext.Transactions.Add(transaction);
            user.Balance += amount;
            _dbContext.SaveChanges();
            Console.WriteLine("Deposit Successful");
            return Ok();
        }
        [HttpPost("withdraw")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> WithdrawAsync(WithdrawRequest request)
        {
            float amount; 
        using (StreamReader reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8))
        {
            string requestBody = await reader.ReadToEndAsync();
            var withdrawRequest = JsonConvert.DeserializeObject<DepositRequest>(requestBody);
            amount = withdrawRequest.Amount;
        }
            

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User not found in the token.");
            }

            var user = _dbContext.Users.SingleOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound("User not found");
            }
            if(user.Balance < amount)
            {
                return BadRequest("Insufficient balance.");
            }
            var transaction = new Transaction
            {
                UserId = user.UserId,
                Amount = amount,
                Type = "Withdraw",
                Time = DateTime.UtcNow
            };
            _dbContext.Transactions.Add(transaction);
            user.Balance = user.Balance - amount;
            Console.WriteLine(user.Balance);
            Console.WriteLine(amount);
            _dbContext.SaveChanges();
            Console.WriteLine("Withdraw Successful");
            return Ok();
        }



        [HttpPost("register")]
        public IActionResult Register([FromBody] User model)
        {
            Console.WriteLine("Register Accessed");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newUser = new User
            {
                Email = model.Email,
                Password = model.Password,
                Name = model.Name,
            };

            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();

            return Ok("Registration successful!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            { 
                return BadRequest("Email and password are required.");
            }


            if (AuthenticateUser(model.Email, model.Password))
            {
                var token = GenerateJwtToken(model.Email);
                var user = _dbContext.Users.SingleOrDefault(u => u.Email == model.Email);
                Console.WriteLine("model email: "+ model.Email);
                Console.WriteLine("user email: "+ user.Email);


                var role = user.Role;
                Console.WriteLine("user role:" + role);
                return Ok(new { token, role });
            }

            return Unauthorized("Invalid credentials");
        }


        private bool AuthenticateUser(string email, string password)
        {
            var user = _dbContext.Users.SingleOrDefault(u => u.Email == email);
            bool isPasswordValid = false;

            if (user == null)
            {
                return false; 
            }
            
            if(user.Password == password)
            {
                isPasswordValid = true;
            }
            Console.WriteLine("Authenticate User Successful");
            return isPasswordValid;
        }



        private string GenerateJwtToken(string email)
        {   
            Console.WriteLine(email);
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }

            var jwtConfig = _configuration.GetSection("JwtConfig").Get<JwtConfig>();
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtConfig.Issuer,
                audience: jwtConfig.Audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1), 
                signingCredentials: credentials
            );
            Console.WriteLine("GenerateJwtToken Successful");

            return new JwtSecurityTokenHandler().WriteToken(token);
}
            [HttpGet("stock-data")]
        public async Task<IActionResult> GetStockData(string symbol)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync($"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?metrics=close?&interval=1d&range=5y");
                    Console.WriteLine(response);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Received Stock JSON data");

                        var stockData = JsonConvert.DeserializeObject<YahooFinanceResponse>(content);

                        var timestamps = stockData.Chart.Result[0].Timestamp;
                        var closePrices = stockData.Chart.Result[0].Indicators.Quote[0].Close;

                        var dataToSend = timestamps.Select((timestamp, index) => new
                        {
                            Timestamp = timestamp,
                            ClosePrice = closePrices[index]
                        }).ToList();

                        return Ok(dataToSend);
                    }
                    else
                    {
                        Console.WriteLine("Failed to fetch stock data. Status code: " + response.StatusCode);
                        Console.Out.Flush(); 
                        return BadRequest("Failed to fetch stock data");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.Out.Flush(); 
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        [HttpPost("buy-stock")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult BuyStock([FromBody] BuyStockRequest buyRequest)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine(UserId);
            Console.WriteLine(userEmail);



            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User email not found in the token.");
            }

            var user = _dbContext.Users.SingleOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound("User not found");
            }

            float totalCost = buyRequest.PurchasePrice;

            if (user.Balance < totalCost)
            {
                return BadRequest("Insufficient balance to make the purchase.");
            }

            user.Balance -= totalCost;

            var userStock = new UserStock
            {
                UserId = user.UserId,
                StockSymbol = buyRequest.Symbol,
                Quantity = buyRequest.Quantity,
                PurchasePrice = buyRequest.PurchasePrice,
                PurchaseDate = DateTime.UtcNow, 
                BuySell = "Buy"
            };

            _dbContext.UserStock.Add(userStock); 
            _dbContext.SaveChanges();

            return Ok("Stock purchase successful!");
        }

        [HttpPost("sell-stock")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult SellStock([FromBody] SellStockRequest sellRequest)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User email not found in the token.");
            }

            var user = _dbContext.Users.SingleOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var userStocks = _dbContext.UserStock
                .Where(us => us.UserId == user.UserId && us.StockSymbol == sellRequest.Symbol)
                .OrderBy(us => us.PurchaseDate)
                .ToList();

            if (userStocks == null)
            {
                return BadRequest("User does not own any stocks of this symbol.");
            }
            Console.WriteLine("Sell Request Received: " + JsonConvert.SerializeObject(sellRequest));

            Console.WriteLine("Sell Price Received" + sellRequest.SellPrice);
            Console.WriteLine("Quantity Received" + sellRequest.Quantity);
            Console.WriteLine("Symbol Received" + sellRequest.Symbol);

            int totalBoughtQuantity = userStocks
                .Where(us => us.BuySell == "Buy")
                .Sum(us => us.Quantity);

            int totalSoldQuantity = userStocks
                .Where(us => us.BuySell == "Sell")
                .Sum(us => us.Quantity);

            if (totalBoughtQuantity >= totalSoldQuantity + sellRequest.Quantity)
            {
                float totalAmountReceived = sellRequest.SellPrice; // Calculate total amount received

                // Create a record for the stock sale
                var stockSale = new UserStock
                {
                    UserId = user.UserId,
                    StockSymbol = sellRequest.Symbol,
                    Quantity = sellRequest.Quantity,
                    PurchaseDate = DateTime.UtcNow,
                    PurchasePrice = sellRequest.SellPrice,
                    BuySell = "Sell"
                };

                _dbContext.UserStock.Add(stockSale);

                // Update the user's balance with the received amount
                user.Balance += totalAmountReceived;

                _dbContext.SaveChanges();

                return Ok("Stock sell successful!");
            }
            else
            {
                // Insufficient quantity to sell
                return BadRequest("Insufficient quantity of stocks to sell.");
            }
        }
        [HttpPost("transfer")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Transfer(TransferRequest request)
        {
            float amount; 
            string toIBAN;
            string toname;
        using (StreamReader reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8))
        {
            string requestBody = await reader.ReadToEndAsync();
            var transferRequest = JsonConvert.DeserializeObject<TransferRequest>(requestBody);
            amount = transferRequest.Amount;
            toIBAN = transferRequest.IBAN;
            toname = transferRequest.Name;

            Console.WriteLine("name" + toname);
            Console.WriteLine("IBAN" + toIBAN);
            Console.WriteLine("Amount" + amount);
        }
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            Console.WriteLine(userEmail);

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User not found in the token.");
            }

            var sender_user = _dbContext.Users.SingleOrDefault(u => u.Email == userEmail);
            Console.WriteLine(sender_user.UserId);

            if (sender_user == null)
            {
                return NotFound("User not found");
            }

            if (sender_user.Balance < amount)
            {
                return BadRequest("Insufficient balance for the transfer.");
            }
            var receiver_user = await _dbContext.Users.SingleOrDefaultAsync(u => u.IBAN == toIBAN && u.Name == toname);

            if (receiver_user == null)
            {
                return NotFound("User not found with the given IBAN and Name combination");
            }

            sender_user.Balance -= amount;

            _dbContext.Users.Update(sender_user);

            var transfer = new Transfer
            {
                FromUserId = sender_user.UserId,
                ToUserId = receiver_user.UserId,
                Amount = amount,
                Date = DateTime.UtcNow
            };
            receiver_user.Balance += amount;
            _dbContext.Users.Update(receiver_user);

            await _dbContext.Transfer.AddAsync(transfer);
            await _dbContext.SaveChangesAsync();
            
            return Ok("Transfer successful.");
        }
        [HttpGet("transferhistory")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> TransferHistory()
        {
            // Retrieve the currently authenticated user's ID from the JWT token
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _dbContext.Users.SingleOrDefault(u => u.Email == userEmail);
            int userId;
            if (user == null)
            {
                return NotFound("User not found");
            }
            else
            {
                userId = user.UserId;
            }

            // Use the userId to query the database and retrieve the user's transfer history
            // Join the Transfer and Users tables to get user names
            var userTransfers = _dbContext.Transfer
                .Where(t => t.FromUserId == userId || t.ToUserId == userId)
                .Select(t => new
                {
                    t.Id,
                    FromUserName = GetUserDisplayName(_dbContext.Users.SingleOrDefault(u => u.UserId == t.FromUserId)),
                    ToUserName = GetUserDisplayName(_dbContext.Users.SingleOrDefault(u => u.UserId == t.ToUserId)),
                    t.Amount,
                    t.Date
                })
                .ToList();

            Console.WriteLine($"Total Transfers Retrieved: {userTransfers.Count}");

            // Filter out duplicates in memory
            var distinctUserTransfers = userTransfers
                .GroupBy(u => u.Id)
                .Select(group => group.First())
                .ToList();

            Console.WriteLine($"Distinct Transfers Count: {distinctUserTransfers.Count}");

            // Create a DTO to include user names
            foreach (var transfering in distinctUserTransfers)
            {
                // Generate a unique identifier based on the transfer data
                var uniqueIdentifier = $"{transfering.Id}_{transfering.FromUserName}_{transfering.ToUserName}_{transfering.Amount}_{transfering.Date}";

                Console.WriteLine($"Unique Identifier: {uniqueIdentifier}, Transfer ID: {transfering.Id}");
            }

            var transferDTOs = userTransfers.Select(transfer => new
            {
                Id = transfer.Id,
                FromUserName = transfer.FromUserName,
                ToUserName = transfer.ToUserName,
                Amount = transfer.Amount,
                Date = transfer.Date
            });

            return Ok(transferDTOs);
        }
        private static string GetUserDisplayName(User user)
        {
            return user != null ? user.Name : string.Empty;
        }

         [HttpGet("transactionhistory")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> TransactionHistory()
        {
            // Retrieve the currently authenticated user's ID from the JWT token
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _dbContext.Users.SingleOrDefault(u => u.Email == userEmail);
            int userId;
            if (user == null)
            {
                return NotFound("User not found");
            }
            else
            {
                userId = user.UserId;
            }

                var userTransaction = _dbContext.Transactions
                .Where(t => t.UserId == userId)
                .Select(t => new
                {
                    t.Id,
                    t.Type,
                    t.Amount,
                    t.Time
                })
                .ToList();

            var transactionDTO = userTransaction.Select(transaction => new
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Date = transaction.Time,
                Type = transaction.Type
            });

            return Ok(transactionDTO);
        }
        [HttpGet("calculate-profits-losses")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CalculateProfitsAndLossesAsync()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User email not found in the token.");
            }

            var user = _dbContext.Users.SingleOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var userStocks = _dbContext.UserStock
                .Where(us => us.UserId == user.UserId)
                .OrderBy(us => us.PurchaseDate)
                .ToList();

            var stockProfitsAndLosses = new Dictionary<string, StockProfitLoss>(); 

            foreach (var stock in userStocks)
            {
                if (!stockProfitsAndLosses.ContainsKey(stock.StockSymbol))
                {
                    stockProfitsAndLosses[stock.StockSymbol] = new StockProfitLoss
                    {
                        StockSymbol = stock.StockSymbol,
                        ProfitLoss = 0.0f, 
                        CurrentQuantity = 0, 
                        TotalInvestment = 0.0f 
                    };
                }

                var currentStock = stockProfitsAndLosses[stock.StockSymbol];

                if (stock.BuySell == "Buy")
                {
                    float purchaseCost = stock.PurchasePrice;
                    currentStock.ProfitLoss -= purchaseCost;
                    //currentStock.TotalInvestment += purchaseCost; // Track total investment
                    currentStock.CurrentQuantity += stock.Quantity;
                    
                }
                else if (stock.BuySell == "Sell")
                {
                    float saleValue = stock.PurchasePrice;
                    currentStock.ProfitLoss += saleValue;
                    currentStock.CurrentQuantity -= stock.Quantity;
                    //currentStock.TotalInvestment += saleValue;
                }
            }

            foreach (var stock in stockProfitsAndLosses.Values)
            {
                var closePriceTask = FetchLatestClosePriceAsync(stock.StockSymbol);
                
                var closePrice = await closePriceTask;        

                stock.ProfitLoss += (float)(closePrice ?? 0) * stock.CurrentQuantity;
            }

            return Ok(stockProfitsAndLosses.Values.ToList());
        }

        private async Task<double?> FetchLatestClosePriceAsync(string symbol)
        {

                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync($"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?metrics=close?&interval=1d&range=5y");
                    Console.WriteLine(response);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Received Stock JSON data");

                        var stockData = JsonConvert.DeserializeObject<YahooFinanceResponse>(content);

                        List<double?> current_price_list = stockData.Chart.Result[0].Indicators.Quote[0].Close;
                        double? current_price = current_price_list[current_price_list.Count - 1];


                        return current_price;
                    }
                    else
                    {
                        Console.WriteLine("Failed to fetch stock data. Status code: " + response.StatusCode);
                        Console.Out.Flush(); 
                    }
                }
            return 0.0f;
        }
        [HttpPost("apply-loan")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ApplyLoan([FromBody] LoanRequest loanApplication)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User email not found in the token.");
            }

            var user = _dbContext.Users.SingleOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound("User not found");
            }
            var loan = new Loan
            {
                UserId = user.UserId,
                Amount = loanApplication.Amount,
                InterestRate = loanApplication.InterestRate,
                TermMonths = loanApplication.TermMonths,
                ApplicationDate = DateTime.UtcNow,
                Approved = 1, 
                ApprovalDate = DateTime.MinValue 
            };

            _dbContext.Loans.Add(loan);
            _dbContext.SaveChanges();

            return Ok("Loan application submitted successfully!");
        }

        [HttpGet("user-loans")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetUserLoans()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User email not found in the token.");
            }

            var user = _dbContext.Users.SingleOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Retrieve user's loans
            var userLoans = _dbContext.Loans
                .Where(loan => loan.UserId == user.UserId)
                .Select(loan => new
                {
                    loan.LoanId,
                    loan.Amount,
                    loan.InterestRate,
                    loan.TermMonths,
                    loan.ApplicationDate,
                    loan.Approved,
                    loan.ApprovalDate
                })
                .ToList();
            foreach(var item in userLoans)
            {
                Console.WriteLine(item);
            }

            return Ok(userLoans);
        }
         [HttpGet("pending-loans")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetPendingLoans()
        {
            // Fetch and return a list of pending loan applications from your database
            Console.WriteLine("girdik");
            var pendingLoans = _dbContext.Loans.Where(loan => loan.Approved == 1).ToList();
            return Ok(pendingLoans);
        }

        // Implement this method to approve a loan application
        [HttpPost("approve-loan/{loanId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ApproveLoan(int loanId)
        {
            // Find the loan by loanId in your database and update its approval status
            var loan = _dbContext.Loans.SingleOrDefault(l => l.LoanId == loanId);

            if (loan == null)
            {
                return NotFound("Loan not found");
            }

            loan.Approved = 2; // You can use a different status code if needed
            loan.ApprovalDate = DateTime.UtcNow;

            _dbContext.SaveChanges();

            return Ok("Loan approved successfully!");
        }

        // Implement this method to reject a loan application
        [HttpPost("reject-loan/{loanId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult RejectLoan(int loanId)
        {
            // Find the loan by loanId in your database and update its approval status
            var loan = _dbContext.Loans.SingleOrDefault(l => l.LoanId == loanId);

            if (loan == null)
            {
                return NotFound("Loan not found");
            }

            loan.Approved = 3; // You can use a different status code if needed
            loan.ApprovalDate = DateTime.UtcNow;

            _dbContext.SaveChanges();

            return Ok("Loan rejected successfully!");
        }
    




    }
}
