using CrudUsuarioApi.Data;
using CrudUsuarioApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CrudUsuarioApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        // Injetamos o IConfiguration para ler a chave secreta do appsettings.json
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Busca o usuário pelo e-mail no banco
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);

            // 2. Validação (Aqui estamos simulando a senha "123456")
            if (usuario == null || request.Senha != "123456")
            {
                return Unauthorized("Credenciais inválidas.");
            }

            // 3. Gerar o Token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            // Converte a chave secreta de string para bytes
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // Claims são as informações que vão dentro do Payload do Token
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Name, usuario.Nome)
                }),
                Expires = DateTime.UtcNow.AddHours(2), // O token expira em 2 horas
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // 4. Retorna o token
            return Ok(new { Token = tokenString });
        }
    }
}