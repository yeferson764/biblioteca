using biblioteca.Models;
using biblioteca.Models.Dtos.Persona;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace biblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonaController : ControllerBase
    {
        private readonly BibliotecaDbContext _context;

        public PersonaController(BibliotecaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonaDto>>> GetPersonas()
        {
            var personas = await _context.Personas
                .Include(p => p.Rol)
                .Select(p => new PersonaDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Cedula = p.Cedula,
                    RolId = p.RolId,
                    RolNombre = p.Rol.RolName,
                    CapacidadPrestamo = p.Rol.CapacidadPrestamo
                })
                .ToListAsync();

            return Ok(personas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PersonaDto>> GetPersona(int id)
        {
            var persona = await _context.Personas
                .Include(p => p.Rol)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (persona == null)
            {
                return NotFound();
            }

            var personaDto = new PersonaDto
            {
                Id = persona.Id,
                Nombre = persona.Nombre,
                Cedula = persona.Cedula,
                RolId = persona.RolId,
                RolNombre = persona.Rol.RolName,
                CapacidadPrestamo = persona.Rol.CapacidadPrestamo
            };

            return Ok(personaDto);
        }

        [HttpPost]
        public async Task<ActionResult<PersonaDto>> CreatePersona(CreatePersonaDto personaDto)
        {
            // Verificar que la cédula no exista
            var existingPersona = await _context.Personas.FirstOrDefaultAsync(p => p.Cedula == personaDto.Cedula);
            if (existingPersona != null)
            {
                return BadRequest("Ya existe una persona registrada con esta cédula.");
            }

            // Verificar que el rol exista
            var rolExists = await _context.Roles.AnyAsync(r => r.Id == personaDto.RolId);
            if (!rolExists)
            {
                return BadRequest("El rol especificado no existe.");
            }

            var persona = new Persona
            {
                Nombre = personaDto.Nombre,
                Cedula = personaDto.Cedula,
                RolId = personaDto.RolId
            };

            _context.Personas.Add(persona);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest("No se pudo crear la persona. Verifique que la cédula no esté registrada.");
            }

            await _context.Entry(persona).Reference(p => p.Rol).LoadAsync();

            var resultado = new PersonaDto
            {
                Id = persona.Id,
                Nombre = persona.Nombre,
                Cedula = persona.Cedula,
                RolId = persona.RolId,
                RolNombre = persona.Rol.RolName,
                CapacidadPrestamo = persona.Rol.CapacidadPrestamo
            };

            return CreatedAtAction(nameof(GetPersona), new { id = resultado.Id }, resultado);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePersona(int id, UpdatePersonaDto personaDto)
        {
            var persona = await _context.Personas.FindAsync(id);

            if (persona == null)
            {
                return NotFound();
            }

            // Verificar que la cédula nueva no exista en otra persona
            if (persona.Cedula != personaDto.Cedula)
            {
                var existingPersona = await _context.Personas.FirstOrDefaultAsync(p => p.Cedula == personaDto.Cedula && p.Id != id);
                if (existingPersona != null)
                {
                    return BadRequest("Ya existe otra persona registrada con esta cédula.");
                }
            }

            // Verificar que el rol exista
            var rolExists = await _context.Roles.AnyAsync(r => r.Id == personaDto.RolId);
            if (!rolExists)
            {
                return BadRequest("El rol especificado no existe.");
            }

            persona.Nombre = personaDto.Nombre;
            persona.Cedula = personaDto.Cedula;
            persona.RolId = personaDto.RolId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException)
            {
                return BadRequest("No se pudo actualizar la persona. Verifique que la cédula no esté registrada por otra persona.");
            }

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePersona(int id)
        {
            var persona = await _context.Personas.FindAsync(id);
            if (persona == null)
            {
                return NotFound();
            }

            // Verificar si la persona tiene préstamos activos
            var tienePrestamosPendientes = await _context.Prestamos
                .AnyAsync(p => p.PersonaId == id && !p.Devuelto);

            if (tienePrestamosPendientes)
            {
                return BadRequest("No se puede eliminar esta persona porque tiene préstamos pendientes de devolución.");
            }

            _context.Personas.Remove(persona);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("disponibilidad/{id}")]
        public async Task<ActionResult<object>> GetDisponibilidadPrestamos(int id)
        {
            // Verificar que la persona existe
            var persona = await _context.Personas
                .Include(p => p.Rol)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (persona == null)
            {
                return NotFound("La persona no existe.");
            }

            // Contar préstamos activos
            var prestamosActivos = await _context.Prestamos
                .CountAsync(p => p.PersonaId == id && !p.Devuelto);

            var capacidadPrestamo = persona.Rol.CapacidadPrestamo;
            var disponibles = capacidadPrestamo - prestamosActivos;

            return Ok(new
            {
                PersonaId = persona.Id,
                Nombre = persona.Nombre,
                Cedula = persona.Cedula,
                Rol = persona.Rol.RolName,
                CapacidadTotal = capacidadPrestamo,
                PrestamosActivos = prestamosActivos,
                DisponiblesPrestamo = disponibles
            });
        }

        private bool PersonaExists(int id)
        {
            return _context.Personas.Any(e => e.Id == id);
        }
    }
}