using biblioteca.Models;
using biblioteca.Models.Dtos.Material;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class MaterialController : ControllerBase
{
    private readonly BibliotecaDbContext _context;

    public MaterialController(BibliotecaDbContext context)
    {
        _context = context;
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<MaterialDto>>> GetMateriales()
    {
        var materiales = await _context.Materiales
            .Include(m => m.TipoMaterial)
            .Select(m => new MaterialDto
            {
                Id = m.Id,
                Titulo = m.Titulo,
                TipoId = m.TipoId,
                TipoNombre = m.TipoMaterial.Tipo,
                FechaRegistro = m.FechaRegistro,
                CantidadRegistrada = m.CantidadRegistrada,
                CantidadActual = m.CantidadActual
            })
            .ToListAsync();

        return Ok(materiales);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<MaterialDto>> GetMaterial(int id)
    {
        var material = await _context.Materiales
            .Include(m => m.TipoMaterial)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (material == null)
        {
            return NotFound();
        }

        var materialDto = new MaterialDto
        {
            Id = material.Id,
            Titulo = material.Titulo,
            TipoId = material.TipoId,
            TipoNombre = material.TipoMaterial.Tipo,
            FechaRegistro = material.FechaRegistro,
            CantidadRegistrada = material.CantidadRegistrada,
            CantidadActual = material.CantidadActual
        };

        return Ok(materialDto);
    }


    [HttpPost]
    public async Task<ActionResult<MaterialDto>> CreateMaterial(CreateMaterialDto materialDto)
    {
        // Valida si el tipo de material existe
        var tipoExists = await _context.TipoMateriales.AnyAsync(t => t.Id == materialDto.TipoId);
        if (!tipoExists)
        {
            return BadRequest("El tipo de material no existe.");
        }

        var material = new Material
        {
            Titulo = materialDto.Titulo,
            TipoId = materialDto.TipoId,
            FechaRegistro = DateTime.Now,
            CantidadRegistrada = materialDto.CantidadRegistrada,
            CantidadActual = materialDto.CantidadRegistrada
        };

        _context.Materiales.Add(material);
        await _context.SaveChangesAsync();

        await _context.Entry(material).Reference(m => m.TipoMaterial).LoadAsync();

        var resultado = new MaterialDto
        {
            Id = material.Id,
            Titulo = material.Titulo,
            TipoId = material.TipoId,
            TipoNombre = material.TipoMaterial.Tipo,
            FechaRegistro = material.FechaRegistro,
            CantidadRegistrada = material.CantidadRegistrada,
            CantidadActual = material.CantidadActual
        };

        return CreatedAtAction(nameof(GetMaterial), new { id = resultado.Id }, resultado);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMaterial(int id, UpdateMaterialDto materialDto)
    {
        var material = await _context.Materiales.FindAsync(id);

        if (material == null)
        {
            return NotFound();
        }

        // Valida si el tipo de material existe
        var tipoExists = await _context.TipoMateriales.AnyAsync(t => t.Id == materialDto.TipoId);
        if (!tipoExists)
        {
            return BadRequest("El tipo de material especificado no existe.");
        }

        // Calcular la diferencia entre la cantidad nueva y la antigua
        var difference = materialDto.CantidadRegistrada - material.CantidadRegistrada;

        material.Titulo = materialDto.Titulo;
        material.TipoId = materialDto.TipoId;
        material.CantidadRegistrada = materialDto.CantidadRegistrada;

        // Actualice la cantidad actual según corresponda
        material.CantidadActual += difference;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MaterialExists(id))
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

    [HttpPatch("{id}/enter-stock")]
    public async Task<IActionResult> EnterStock(int id, int cantidadIncremento)
    {
        
        // Validar que la cantidad sea positiva
        if (cantidadIncremento <= 0)
        {
            return BadRequest("La cantidad a incrementar debe ser mayor que cero.");
        }

        var material = await _context.Materiales.FindAsync(id);
        if (material == null)
        {
            return NotFound($"No se encontró el material con ID {id}.");
        }

        // Incrementar tanto la cantidad registrada como la cantidad actual
        material.CantidadRegistrada += cantidadIncremento;
        material.CantidadActual += cantidadIncremento;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MaterialExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        // Retornar las cantidades actualizadas
        return Ok(new
        {
            Id = material.Id,
            Titulo = material.Titulo,
            CantidadRegistrada = material.CantidadRegistrada,
            CantidadActual = material.CantidadActual,
            CantidadIncrementada = cantidadIncremento
        });
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMaterial(int id)
    {
        var material = await _context.Materiales.FindAsync(id);
        if (material == null)
        {
            return NotFound();
        }

        // Check if there are loans associated with this material
        var prestamosConMaterial = await _context.Prestamos.AnyAsync(p => p.MaterialId == id && !p.Devuelto);
        if (prestamosConMaterial)
        {
            return BadRequest("No se puede eliminar este material porque tiene préstamos activos asociados.");
        }

        _context.Materiales.Remove(material);
        await _context.SaveChangesAsync();

        return NoContent();
    }


    private bool MaterialExists(int id)
    {
        return _context.Materiales.Any(e => e.Id == id);
    }

}