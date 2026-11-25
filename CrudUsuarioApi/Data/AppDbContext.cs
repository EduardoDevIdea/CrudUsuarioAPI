using CrudUsuarioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CrudUsuarioApi.Data
{
    // A classe herda de DbContext, que é o coração do EF Core
    public class AppDbContext : DbContext
    {
        // O construtor recebe as opções (como a string de conexão) via Injeção de Dependência
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Representa a tabela de Usuários no banco
        public DbSet<Usuario> Usuarios { get; set; }

        // Representa a tabela de Endereços no banco
        public DbSet<Endereco> Enderecos { get; set; }

        // Configurações adicionais do modelo (opcional, mas boa prática)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Exemplo: Garantir que o Email seja único
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}