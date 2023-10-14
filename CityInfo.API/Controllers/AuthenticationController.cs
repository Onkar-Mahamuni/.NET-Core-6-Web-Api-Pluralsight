using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CityInfo.API.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private IConfiguration _configuration;

        public class AuthenticationRequestBody
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        private class CityInfoUser
        {
            public int UserId { get; set; }
            public string UserName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string City { get; set; }

            public CityInfoUser(int userId, string userName, string firstName, string lastName, string city)
            {
                UserId = userId;
                UserName = userName;
                FirstName = firstName;
                LastName = lastName;
                City = city;
            }
        }

        public AuthenticationController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
        }

        [HttpPost("authenticate")]
        public ActionResult<string> Authenticate(AuthenticationRequestBody authenticationRequestBody)
        {
            //Step 1: Validate the username and password
            var user = ValidateUserCredentials(authenticationRequestBody.UserName, authenticationRequestBody.Password);

            if(user == null)
            {
                return Unauthorized();
            }

            //Step 2: create a token
            var securityKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(_configuration["Authentication:SecretForKey"]));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // The claims that
            var claimsForToken = new List<Claim>();
            claimsForToken.Add(new Claim("sub", user.UserId.ToString())); //sub is standardized name
            claimsForToken.Add(new Claim("given_name", user.FirstName));
            claimsForToken.Add(new Claim("sub", user.LastName));
            claimsForToken.Add(new Claim("sub", user.City));

            //Making jwt token
            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claimsForToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                signingCredentials
                );

            var tokenToReturn = new JwtSecurityTokenHandler()
                .WriteToken(jwtSecurityToken);

            return Ok(tokenToReturn);

        }

        private CityInfoUser ValidateUserCredentials(string? userName, string? password)
        {
            // Using in memory validation as we are not generating any database for storing users in this course
            // For demo, we assume the credentials are valid 
            // return a new CityInfoUser
            return new CityInfoUser(1, userName ?? "", "Kevin", "Dockx", "Antwerp");
        }
    }
}
