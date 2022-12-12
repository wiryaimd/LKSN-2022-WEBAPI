using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PC_Bali.Dtos;
using PC_Bali.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PC_Bali.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost]
        public async Task<ActionResult<AuthResDto>> Auth(AuthReqDto authDto) {
            Customer? customer = _context.Customers.Where(delegate (Customer c)
            {
                return c.Email.Equals(authDto.Email) && c.Password.Equals(authDto.Password);
            }).FirstOrDefault();

            if (customer == null) {
                return NotFound();
            }

            DateTime expired = DateTime.Now.AddMinutes(10);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            SecurityToken token = handler.CreateToken(new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, customer.Email),
                    new Claim(ClaimTypes.Role, customer.Role.ToString())
                }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("JwtSettings:SecretKey").Value)), SecurityAlgorithms.HmacSha256Signature),
                Expires = expired
            });

            AuthResDto res = new AuthResDto()
            {
                Token = handler.WriteToken(token),
                ExpiresAt = expired
            };

            return Ok(res);
        }
    }
}
