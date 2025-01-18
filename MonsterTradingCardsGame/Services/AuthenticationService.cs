using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using MonsterTradingCardsGame.Common;
using MonsterTradingCardsGame.Enums;
using MonsterTradingCardsGame.Helper.HttpServer;
using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Repositories.Interfaces;

namespace MonsterTradingCardsGame.Services
{
    public class AuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly string _jwtSecret;
        private readonly int _jwtLifespan;

        public AuthenticationService(IUserRepository userRepository, string jwtSecret, int jwtLifespan)
        {
            _userRepository = userRepository;
            _jwtSecret = jwtSecret;
            _jwtLifespan = jwtLifespan;
        }

        public async Task<OperationResult<bool>> Register(User user, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(user.UserCredentials.Username))
            {
                return new OperationResult<bool>(false, HttpStatusCode.BAD_REQUEST, "Username is not set.");
            }

            var existingUserResult = await _userRepository.GetByUsername(user.UserCredentials.Username);
            if (existingUserResult.Success && existingUserResult.Data.user != null)
            {
                return new OperationResult<bool>(false, HttpStatusCode.CONFLICT, "User already exists");
            }

            var createUserResult = await _userRepository.Create(user, hashedPassword);
            if (createUserResult.Success)
            {
                return new OperationResult<bool>(true, HttpStatusCode.CREATED, null, true);
            }

            return new OperationResult<bool>(false, createUserResult.Code, createUserResult.Message);
        }

        public async Task<OperationResult<string>> Login(string name, string password)
        {
            var userResult = await _userRepository.GetByUsername(name);

            if (!userResult.Success || userResult.Data.user == null || !BCrypt.Net.BCrypt.Verify(password, userResult.Data.hashedPassword))
            {
                return new OperationResult<string>(false, HttpStatusCode.UNAUTHORIZED, "Invalid username or password.");
            }

            var user = userResult.Data.user;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserCredentials.Username)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtLifespan),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new OperationResult<string>(true, HttpStatusCode.OK, null, tokenHandler.WriteToken(token));
        }

        public async Task<OperationResult<User?>> AuthorizeAdmin(string token)
        {
            var verifiedUserResult = await VerifyToken(token);
            if (verifiedUserResult.Success && verifiedUserResult.Data.verified &&
                verifiedUserResult.Data.user.UserCredentials.Role == RoleEnum.Admin)
            {
                return new OperationResult<User?>(true, HttpStatusCode.OK, null, verifiedUserResult.Data.user);
            }
            return new OperationResult<User?>(false, HttpStatusCode.UNAUTHORIZED, "Unauthorized access.");
        }

        public async Task<OperationResult<User?>> AuthorizePlayer(string token)
        {
            var verifiedUserResult = await VerifyToken(token);
            if (verifiedUserResult.Success && verifiedUserResult.Data.verified &&
                (verifiedUserResult.Data.user.UserCredentials.Role == RoleEnum.Player ||
                 verifiedUserResult.Data.user.UserCredentials.Role == RoleEnum.Admin))
            {
                return new OperationResult<User?>(true, HttpStatusCode.OK, null, verifiedUserResult.Data.user);
            }
            return new OperationResult<User?>(false, HttpStatusCode.UNAUTHORIZED, "Unauthorized access.");
        }

        public async Task<OperationResult<(bool verified, User? user)>> VerifyToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new OperationResult<(bool, User?)>(false, HttpStatusCode.BAD_REQUEST, "Token is not provided.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == "nameid").Value;

                var userResult = await _userRepository.GetById(Guid.Parse(userId));
                if (!userResult.Success || userResult.Data == null)
                {
                    return new OperationResult<(bool, User?)>(false, HttpStatusCode.NOT_FOUND, "User not found.");
                }

                return new OperationResult<(bool, User?)>(true, HttpStatusCode.OK, null, (true, userResult.Data));
            }
            catch (Exception ex)
            {
                return new OperationResult<(bool, User?)>(false, HttpStatusCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }
    }
}