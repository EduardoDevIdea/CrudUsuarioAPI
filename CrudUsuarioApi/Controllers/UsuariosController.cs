using CrudUsuarioApi.Data;
using CrudUsuarioApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudUsuarioApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // A rota final será /api/usuarios
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        // 1. Injeção de Dependência do Banco de Dados
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            // Busca todos os usuários no banco e inclui os endereços relacionados
            // O 'Include' é a Propriedade de Navegação que vimos lá atrás!
            return await _context.Usuarios.Include(u => u.Enderecos).ToListAsync();
        }

        // GET: api/usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            // Busca o usuário pelo ID e traz seus endereços
            var usuario = await _context.Usuarios
                .Include(u => u.Enderecos)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound(); // Retorna HTTP 404 se não achar
            }

            return usuario; // Retorna HTTP 200 com o JSON do usuário
        }

        // POST: api/usuarios
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            // O [ApiController] já validou as regras ([Required], [EmailAddress])
            // Adiciona o novo usuário ao contexto do banco
            _context.Usuarios.Add(usuario);

            // Salva as alterações no MySQL (gera o INSERT)
            await _context.SaveChangesAsync();

            // Retorna HTTP 201 (Created) e o endereço para buscar o recurso criado
            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
        }

        // PUT: api/usuarios/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutUsuario([FromRoute] int id, [FromBody] Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest("O ID da URL não bate com o ID do corpo."); // HTTP 400
            }

            // Marca o objeto como modificado no Entity Framework
            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(); // Executa o UPDATE
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound(); // Se o usuário não existe mais
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // HTTP 204 (Sucesso, mas sem corpo de resposta)
        }

        // Método auxiliar para verificar se existe
        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }

        // DELETE: api/usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // Remove do contexto e salva (gera o DELETE no banco)
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}