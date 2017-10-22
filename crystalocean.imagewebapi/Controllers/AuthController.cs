using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using CrystalOcean.Data.Models;
using CrystalOcean.Data.Repository;
using CrystalOcean.ImageWebApi.Configuration;
using CrystalOcean.ImageWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

// Refer to  https://logcorner.com/token-based-authentication-using-asp-net-web-api-core/

namespace CrystalOcean.ImageWebApi.Controllers
{
    [AllowAnonymous]
    [Route("/api/[controller]")]
    public class AuthController : Controller
    {
        private readonly JwtSecuritySettings jwtSettings;
        private readonly ILogger<AuthController> logger;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly UserTokenRepository tokenRepository;

        public AuthController(UserManager<User> userManager, 
                            SignInManager<User> signInManager, 
                            IOptions<JwtSecuritySettings> jwtSettings, 
                            UserTokenRepository tokenRepository, 
                            ILogger<AuthController> logger)
        {
            this.userManager = userManager;
            this.passwordHasher = userManager.PasswordHasher;
            this.jwtSettings = jwtSettings.Value;
            this.logger = logger;
            this.tokenRepository = tokenRepository;
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh(RefreshViewModel model)
        {
            if (ModelState.IsValid)
            {
                UserToken token = await tokenRepository.FindOneAsync(
                        model.UserId, model.RefreshToken);
                if (token == null)
                {
                    return Unauthorized();
                }

                // Old token has been expired
                token.Expired = true;
                tokenRepository.Update(token);

                // Issue a new token
                token = NewToken(token.UserId);
                tokenRepository.Add(token);
                tokenRepository.SaveChanges();

                return Ok(BuildJwtResponse(token.UserId, token.Id));
            }

            return BadRequest("Could not refresh token");
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null) 
                {
                    return Unauthorized();
                }
                /*var result = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (result.Succeeded)
                {

                }*/
                if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, 
                        model.Password) != PasswordVerificationResult.Success) 
                {
                    return Unauthorized();
                }

                // Issue a new refresh token
                var token = NewToken(user.Id);
                tokenRepository.Add(token);
                tokenRepository.SaveChanges();

                return Ok(BuildJwtResponse(user.Id, token.Id));
            }

            return BadRequest("Could not generate token");
        }

        private static UserToken NewToken(long userId)
        {
            // Issue a new refresh token
            var token = new UserToken();
            token.Id = Guid.NewGuid().ToString().Replace("-", "");
            token.UserId = userId;
            return token;
        }

        private object BuildJwtResponse(long userId, String tokenId)
        {
            var now = DateTime.UtcNow;  
            var expires = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiresIn);

            var claims = new [] 
            {
                new Claim(JwtRegisteredClaimNames.Jti, tokenId),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToUniversalTime().ToString(), ClaimValueTypes.Integer64),
            };
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                notBefore: now, 
                expires: expires,
                signingCredentials: credentials
            );
            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            return new {
                access_token = token,
                expires = jwtSecurityToken.ValidTo,
                refresh_token = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenId))
            };
        }
    }
}