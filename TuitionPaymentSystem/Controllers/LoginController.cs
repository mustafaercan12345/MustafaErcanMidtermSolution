using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TuitionPaymentSystem.Models;

namespace TuitionPaymentSystem.Controllers
{
    [Route("api/v1/Login")]
    public class LoginController : ControllerBase
    {
        /// <summary>
        /// this is a data transfer object for requesting Login endpoint
        /// </summary>
        public class LoginReq
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        /// <summary>
        /// its for logging in. provide email and password with the correct json mapping.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="BadHttpRequestException"></exception>
        [HttpPost]
        public string Login([FromBody] LoginReq request)
        {
            var person = Person.people.FirstOrDefault(person => person.Email == request.Email && person.Password == request.Password);

            if( person == null)
            {
                throw new BadHttpRequestException("Eihter email or password is incorrect");
            }

            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Program.MyJwtKey));
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey,
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                    new Claim(ClaimTypes.NameIdentifier, person.Name)
                };

            var token = new JwtSecurityToken("mustafa ercan",
              "people",
              claims,
              expires: DateTime.Now.AddMinutes(50),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
