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
using System.Security.Cryptography;
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
            if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Password)) 
                return BadRequest("Enter your username and password");

            try
            {
                using (var context = new AppDbContext())
                {
                    var exsists = context.Users.Any(n => n.UserName == user.UserName);
                    if (exsists && ComparePasswords(user))
                        return Ok(CreateToken(user));
                    else
                        return BadRequest("Invalid Username/Password");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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

                    user.Password = HashPassword(user.Password);
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

        private bool ComparePasswords(User user)
        {
            bool pwMatch = false;
            using (var context = new AppDbContext())
            {
                var exsists = context.Users.Any(n => n.UserName == user.UserName);
                if (exsists)
                {
                    string savedPasswordHash = context.Users.FirstOrDefault(u => u.UserName == user.UserName).Password;
                    byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
                    byte[] salt = new byte[16];
                    Array.Copy(hashBytes, 0, salt, 0, 16);
                    var pbkdf2 = new Rfc2898DeriveBytes(user.Password, salt, 100000);
                    byte[] hash = pbkdf2.GetBytes(20);
                    /* Compare the results */
                    for (int i = 0; i < 20; i++)
                        if (hashBytes[i + 16] != hash[i])
                            pwMatch = false;
                        else
                            pwMatch = true;
                }
            }
            return pwMatch;
        }

        private string HashPassword(string password)
        {
            byte[] salt; 
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            string savedPasswordHash = Convert.ToBase64String(hashBytes);
            return savedPasswordHash;
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