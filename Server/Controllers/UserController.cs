using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Server.Controllers
{
    [Route("users")]
    [ApiController]
   
    public class UserController : ControllerBase
    {
        private SqliteDbContext _db;
        private readonly IConfiguration _configuration;
        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
            _db = new SqliteDbContext();
        }

        [HttpPost("new")]
        public async Task NewUser(User user)
        {
            user.Role = Roles.User;
            await _db.AddUser(user);
        }

        [HttpPost("getuser")]
        public async Task<ActionResult<User>> GetUser(User user)
        {
            return await _db.GetUser(user);
        }

        [HttpPost("getuserbyjwt")]
        public async Task<ActionResult<User>> GetUserByJWT([FromBody] string jwtToken)
        {
            try
            {
                //getting the secret key
                string secretKey = _configuration["JWTSettings:SecretKey"];
                var key = Encoding.ASCII.GetBytes(secretKey);

                //preparing the validation parameters
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken securityToken;

                //validating the token
                var principle = tokenHandler.ValidateToken(jwtToken, tokenValidationParameters, out securityToken);
                var jwtSecurityToken = (JwtSecurityToken)securityToken;

                if (jwtSecurityToken != null
                    && jwtSecurityToken.ValidTo > DateTime.Now
                    && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    //returning the user if found
                    var userId = principle.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    return await GetUserByUserID(Convert.ToInt64(userId));
                }
            }
            catch (Exception ex)
            {
                //logging the error and returning null
                Console.WriteLine("Exception : " + ex.Message);
                return null;
            }
            //returning null if token is not validated
            return null;
        }

        [HttpPut]
        public async Task EditUser(User user)
        {
            await _db.EditUser(user);
        }

        protected async Task<User> GetUserByUserID(long userId)
        {
            return await _db.GetUserById(Convert.ToInt32(userId));
        }

        //Migrating to JWT Authorization...

        [HttpPost("authenticatejwt")]
        public async Task<AuthenticationUser> AuthenticateJWT(AuthenticationUser authUser)
        {
            string token = string.Empty;

            //checking if the user exists in the database
            User validUser = await _db.GetUser(new User { Email = authUser.Email, Password = authUser.Password });

            if (validUser != null)
            {
                //generating the token
                authUser.Token = GenerateJwtToken(validUser);
            }
            return authUser;
        }

        protected string GenerateJwtToken(User user)
        {
            //getting the secret key
            string secretKey = _configuration["JWTSettings:SecretKey"];
            var key = Encoding.ASCII.GetBytes(secretKey);

            //create claims
            var claimEmail = new Claim(ClaimTypes.Email, user.Email);
            var claimNameIdentifier = new Claim(ClaimTypes.NameIdentifier, user.Id.ToString());
            var claimRole = new Claim(ClaimTypes.Role, user.Role.ToString() == null ? "" : user.Role.ToString());

            //create claimsIdentity
            var claimsIdentity = new ClaimsIdentity(new[] { claimEmail, claimNameIdentifier, claimRole }, "serverAuth");

            // generate token that is valid for 7 days
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            //creating a token handler
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            //returning the token back
            return tokenHandler.WriteToken(token);
        }
    }
}
