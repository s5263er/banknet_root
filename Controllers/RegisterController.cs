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



        // Registration Endpoint
        [HttpPost("register")]
        public IActionResult Register([FromBody] User model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Create and save the new user
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

        // Login Endpoint
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
                return Ok(new { token });
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
                PurchasePrice = (int)buyRequest.PurchasePrice,
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
            PurchasePrice = (int)sellRequest.SellPrice,
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










    }
}
