using biblioteca.Models;
using biblioteca.Models.Dtos.Material;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace biblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipoMaterialController : Controller
    {

        private readonly BibliotecaDbContext _context;

        public TipoMaterialController(BibliotecaDbContext context)
        {
            _context = context;
        }


        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<TipoMaterialDto>>> GetTiposMaterial()
        {
            var tipos = await _context.TipoMateriales
                .Select(t => new TipoMaterialDto
                {
                    Id = t.Id,
                    Tipo = t.Tipo
                })
                .ToListAsync();

            return Ok(tipos);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<TipoMaterialDto>> GetTipoMaterial(int id)
        {
            var tipoMaterial = await _context.TipoMateriales.FindAsync(id);

            if (tipoMaterial == null)
            {
                return NotFound();
            }

            var tipoDto = new TipoMaterialDto
            {
                Id = tipoMaterial.Id,
                Tipo = tipoMaterial.Tipo
            };

            return Ok(tipoDto);
        }


        [HttpPost("")]
        public async Task<ActionResult<TipoMaterialDto>> CreateTipoMaterial(CreateTipoMaterialDto tipoDto)
        {
            var tipoMaterial = new TipoMaterial
            {
                Tipo = tipoDto.Tipo
            };

            _context.TipoMateriales.Add(tipoMaterial);
            await _context.SaveChangesAsync();

            var resultado = new TipoMaterialDto
            {
                Id = tipoMaterial.Id,
                Tipo = tipoMaterial.Tipo
            };

            return CreatedAtAction(nameof(GetTipoMaterial), new { id = resultado.Id }, resultado);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTipoMaterial(int id, CreateTipoMaterialDto tipoDto)
        {
            var tipoMaterial = await _context.TipoMateriales.FindAsync(id);

            if (tipoMaterial == null)
            {
                return NotFound();
            }
            tipoMaterial.Tipo = tipoDto.Tipo;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TipoMaterialExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTipoMaterial(int id)
        {
            var tipoMaterial = await _context.TipoMateriales.FindAsync(id);
            if (tipoMaterial == null)
            {
                return NotFound();
            }

            // Check if there are materials using this type
            var materialesConTipo = await _context.Materiales.AnyAsync(m => m.TipoId == id);
            if (materialesConTipo)
            {
                return BadRequest("No se puede eliminar este tipo de material porque hay materiales asociados a él.");
            }

            _context.TipoMateriales.Remove(tipoMaterial);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TipoMaterialExists(int id)
        {
            return _context.TipoMateriales.Any(e => e.Id == id);
        }
    }
}
