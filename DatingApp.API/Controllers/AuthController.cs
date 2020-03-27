using System;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using DatingApp.API.Controllers.Data;
using DatingApp.API.DTO;
using DatingApp.API.Properties.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _config = config;
            _repo = repo;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            //validate request
            userForRegisterDto.username = userForRegisterDto.username.ToLower();
            if (await _repo.UserExists(userForRegisterDto.username))
            {
                return BadRequest("User already exist");
            }
            var userToCreate = new user
            {
                Username = userForRegisterDto.username
            };
            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.password);
            return StatusCode(201);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
            if (userFromRepo == null)
            {
                return Unauthorized();
            }
            var claim = new[]
            {
                 new Claim(ClaimTypes.NameIdentifier,userFromRepo.Id.ToString()),
                 new Claim(ClaimTypes.Name,userFromRepo.Username)

             };
             var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
             var cred=new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);
             var tokendescriptor=new SecurityTokenDescriptor
             {
                 Subject=new ClaimsIdentity(claim),
                 Expires=DateTime.Now.AddDays(1),
                 SigningCredentials=cred
             } ;
             var tokenHandler=new JwtSecurityTokenHandler();
             var token=tokenHandler.CreateToken(tokendescriptor);
             return Ok(new {
                 token=tokenHandler.WriteToken(token)
             });


        }
    }
}