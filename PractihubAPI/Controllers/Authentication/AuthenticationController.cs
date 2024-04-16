using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PractihubAPI.Models.Opinion;
using PractihubAPI.Persistance.Authenticate;
using System.Data;
using System.Threading.Tasks;
using System;
using PractihubAPI.Models.Authenticate;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Google.Apis.Auth;
using System.Collections.Generic;

namespace PractihubAPI.Controllers.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserHandler handler;

        public AuthenticationController(IUserHandler handler, IConfiguration configuration)
        {
            this.handler = handler;
        }

        //Autenticación con email/contraseña
        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateUser([FromBody] User userObj)
        {
                if (userObj == null)
                {
                    throw new ArgumentNullException("An error occurred: Bad Argument(s)");
                }

                DynamicParameters parameters = new();

                parameters.Add("Email", userObj.Email);

                User user = new User();

                user = await handler.FindUser(parameters);
                if (user == null)
                {
                    return NotFound(new { Message = "User Not Found!" });
                }

                if (user.Email == null && user.Password == null)
                    return NotFound(new { Message = "User Not Found!" });

                if (!PasswordHasher.VerifyPassword(userObj.Password, user.Password))
                    {
                        return BadRequest(new { Message = "Password incorrect" });
                    }

                 user.Token = CreateJwt(user);

                return Ok(new
                {
                    Token = user.Token,
                    Email = userObj.Email,
                    Message = "Login Success!"
                });
        }

        //Autenticación con google
        [HttpPost("authenticate-google")]
        public async Task<IActionResult> AuthenticateGoogleUser([FromBody] string credential)
        {
            GoogleSettings googleSettings = new GoogleSettings();
            googleSettings.GoogleClientId = "503475727072-4f8mubsi8ki00lg8g4k7qk4jbupdpk6o.apps.googleusercontent.com";
            googleSettings.Secret = "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";

            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { googleSettings.GoogleClientId }
            };
            
            var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);

            DynamicParameters parameters = new();

            Console.WriteLine("Este es el email", payload.Email);

            parameters.Add("Email", payload.Email);

            var user = await handler.FindUser(parameters);

            if (user == null)
            {
                return NotFound(new { Message = "User Not Found!" });
            }

            if (user.Email == null && user.Password == null)
                return NotFound(new { Message = "User Not Found!" });

            user.Token = CreateJwt(user);

            return Ok(new
            {
                Token = user.Token,
                Email = payload.Email,
                Message = "Login Success!"
            });
        }

        //Registro de usuarios
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userObj)
        {

            if (userObj == null)
            {
                throw new ArgumentNullException("An error occurred: Bad Argument(s)");
            }

            DynamicParameters parameters = new();

            parameters.Add("FirstName", userObj.FirstName);
            parameters.Add("LastName", userObj.LastName);

            userObj.Password = PasswordHasher.HashPassword(userObj.Password);
            parameters.Add("Password", userObj.Password);

            userObj.Token= "";

            userObj.Role = "User";
            parameters.Add("Role", userObj.Role);

            parameters.Add("Email", userObj.Email);

            parameters.Add("MessageResponse", dbType: DbType.String, direction:ParameterDirection.Output, size:100);


            return Ok(await handler.RegisterUser(parameters));

        }

        //Registro de usuario con google
        [HttpPost("register-google")]
        public async Task<IActionResult> RegisterGoogleUser([FromBody] string credential)
        {
            GoogleSettings googleSettings = new GoogleSettings();
            googleSettings.GoogleClientId = "503475727072-4f8mubsi8ki00lg8g4k7qk4jbupdpk6o.apps.googleusercontent.com";
            googleSettings.Secret = "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";

            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { googleSettings.GoogleClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);

            var existingUser = await handler.FindUser(new DynamicParameters(new { Email = payload.Email }));
            if (existingUser != null)
            {
                return BadRequest(new { Message = "User with this email already exists." });
            }

            var newUser = new User
            {
                Email = payload.Email,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                Password = "",
                Role = "User",
                Token = "" 
            };

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("FirstName", newUser.FirstName);
            parameters.Add("LastName", newUser.LastName);
            parameters.Add("Password", newUser.Password);
            parameters.Add("Role", newUser.Role);
            parameters.Add("Email", newUser.Email);
            parameters.Add("MessageResponse", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

            var registrationResult = await handler.RegisterUser(parameters);
            if (registrationResult is null)
            {
                return BadRequest(new { Message = "Registration failed" });
            }

            return Ok (registrationResult);
        }


        //Creamos token
        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryveryveryveryveryveryverysecret.....");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = credentials
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }
    }
}
