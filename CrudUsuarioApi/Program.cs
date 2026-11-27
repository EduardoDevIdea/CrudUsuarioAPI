using CrudUsuarioApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi; // Necessário para OpenApiInfo e configurações de segurança
using System.Text;

namespace CrudUsuarioApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Diz ao serializador para ignorar os ciclos de navegação
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            // Lê a chave secreta do appsettings.json
            var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]!);

            // Configuração da Autenticação JWT
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            // Registra o AppDbContext e configura o uso do MySQL (Pomelo)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
                )
            );

            // --- CONFIGURAÇÃO DO SWAGGER (SWASHBUCKLE) ---
            // Adiciona serviços necessários para explorar os endpoints da API
            builder.Services.AddEndpointsApiExplorer();

            // Adiciona e configura o gerador do Swagger
            builder.Services.AddSwaggerGen(options =>
            {
                // Define as informações básicas do documento. Sem isso, o JSON não terá a versão.
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CrudUsuarioApi",
                    Version = "v1",
                    Description = "API de CRUD de Usuários com Autenticação JWT"
                });

                // 1. Define como o Swagger deve enviar o Token (no Header, tipo Bearer)
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira o token JWT desta forma: Bearer {seu_token}"
                });

                // 2. Aplica a segurança globalmente
                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                });
            });

            var app = builder.Build();

            // Configure o pipeline de requisições HTTP.
            if (app.Environment.IsDevelopment())
            {
                // Gera o JSON da documentação em /swagger/v1/swagger.json
                // Forçamos a versão 3.0 para garantir compatibilidade total com a interface visual
                app.UseSwagger(options =>
                {
                    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
                });

                // Serve a interface visual em /swagger
                app.UseSwaggerUI(options =>
                {
                    // Aponta explicitamente para o arquivo JSON gerado
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "CrudUsuarioApi v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}