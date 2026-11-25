using System.ComponentModel.DataAnnotations;

namespace CrudUsuarioApi.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress] // Valida se o formato é um e-mail válido
        public string Email { get; set; } = string.Empty;

        // Propriedade de navegação: um usuário tem uma lista de endereços
        public ICollection<Endereco>? Enderecos { get; set; }
    }
}