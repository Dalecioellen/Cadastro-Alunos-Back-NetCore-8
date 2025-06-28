using AlunosApi.Services;
using AlunosApi.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AlunosApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthenticateService _authenticateService;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(IConfiguration configuration, IAuthenticateService authenticateService, UserManager<IdentityUser> userManager)
        {
            _configuration = configuration;
            _authenticateService = authenticateService;
            _userManager = userManager;
        }

        [HttpPost("CreateUser")]
        public async Task<ActionResult<UserToken>> CreateUser([FromBody] RegisterModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmePassword", "As senhas não conferem");
                return BadRequest(ModelState);
            }

            var result = await _authenticateService.RegisterUser(model.Email, model.Password);

            if (result)
                return Ok($"Usuário {model.Email} criado com sucesso");
            else
                ModelState.AddModelError("CreateUser", "Registro inválido");
            return BadRequest(ModelState);
        }

        [HttpPost("LoginUser")]
        public async Task<ActionResult<UserToken>> LoginUser([FromBody] LoginModel userInfoModel)
        {
            var result = await _authenticateService.Authenticate(userInfoModel.Email, userInfoModel.Password);

            if (result)
                return GenerateToken(userInfoModel);
            else
            {
                ModelState.AddModelError("LoginUser", "Login inválido");
                return BadRequest(ModelState);
            }
        }

        private ActionResult<UserToken> GenerateToken(LoginModel userInfoModel)
        {
            // Define declarações do usuário
            var claims = new[]
            {
                new Claim("email", userInfoModel.Email),
                new Claim("meu token", "token"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Gera uma chave com base em um algoritmo simétrico
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:key"]));

            // Gera a assinatura digital do token usando o algoritmo HMAC e a chave privada
            var credenciais = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Tempo de expiração do token
            var expiration = DateTime.UtcNow.AddMinutes(20);

            // Classe que representa um token JWT e gera o token
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credenciais);

            var userToken = new UserToken()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token), 
                Expiration = expiration,
            };

            return Ok(userToken); // Retorna o token gerado
        }
    }
    
}
