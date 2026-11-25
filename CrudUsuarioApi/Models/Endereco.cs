using System.ComponentModel.DataAnnotations;

namespace CrudUsuarioApi.Models
{
    public class Endereco
    {
        [Key] // Define que este é a Chave Primária (Primary Key)
        public int Id { get; set; }

        [Required] // Torna o campo obrigatório no banco
        [MaxLength(200)] // Limita o tamanho da string
        public string Logradouro { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Numero { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Cidade { get; set; }

        // Chave Estrangeira (Foreign Key) que liga ao Usuario
        public int UsuarioId { get; set; }

        // Propriedade de navegação: permite acessar o Usuario a partir do Endereco
        public Usuario? Usuario { get; set; }
    }
}