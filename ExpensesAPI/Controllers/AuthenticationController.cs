using ExpensesAPI.Data;
using ExpensesAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ExpensesAPI.Controllers
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("auth")]
    public class AuthenticationController : ApiController
    {
        [Route("login")]
        [HttpPost]
        public IHttpActionResult Login([FromBody]User user)
        {
            return null;
        }


        [Route("register")]
        [HttpPost]
        public IHttpActionResult Register([FromBody]User user)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var exists = context.Users.Any(n => n.UserName == user.UserName);
                    if (exists)
                        return BadRequest("User already exists");

                    context.Users.Add(user);
                    context.SaveChanges();

                    return Ok(CreateToken(user));
                    
                }
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        private JwtPackage CreateToken(User user)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            ClaimsIdentity claims = new ClaimsIdentity(new[]{
                new Claim(ClaimTypes.Email, user.UserName)
            });

            const string secretKey = "2EC65795-2E1C-4186-9E3C-D39A410C4383";
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(secretKey));
            SigningCredentials signingCredetials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            JwtSecurityToken token = tokenHandler.CreateJwtSecurityToken(subject: claims, signingCredentials: signingCredetials);
            string tokenString = tokenHandler.WriteToken(token);

            return new JwtPackage() { UserName = user.UserName, Token = tokenString };
        }
    }
}

public class JwtPackage
{
    public string Token { get; set; }
    public string UserName { get; set; }
}