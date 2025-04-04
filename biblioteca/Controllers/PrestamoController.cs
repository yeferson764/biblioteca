using biblioteca.Models;
using biblioteca.Models.Dtos;
using biblioteca.Models.Dtos.Prestamo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace biblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrestamoController : ControllerBase
    {
        private readonly BibliotecaDbContext _context;

        public PrestamoController(BibliotecaDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrestamoDto>>> GetPrestamos()
        {
            var prestamos = await _context.Prestamos
                .Include(p => p.Persona)
                .Include(p => p.Material)
                .Select(p => new PrestamoDto
                {
                    Id = p.Id,
                    PersonaId = p.PersonaId,
                    PersonaNombre = p.Persona.Nombre,
                    PersonaCedula = p.Persona.Cedula,
                    MaterialId = p.MaterialId,
                    MaterialTitulo = p.Material.Titulo,
                    FechaPrestamo = p.FechaPrestamo,
                    FechaDevolucion = p.FechaDevolucion,
                    Devuelto = p.Devuelto
                })
                .ToListAsync();

            return Ok(prestamos);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<PrestamoDto>> GetPrestamo(int id)
        {
            var prestamo = await _context.Prestamos
                .Include(p => p.Persona)
                .Include(p => p.Material)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prestamo == null)
            {
                return NotFound();
            }

            var prestamoDto = new PrestamoDto
            {
                Id = prestamo.Id,
                PersonaId = prestamo.PersonaId,
                PersonaNombre = prestamo.Persona.Nombre,
                PersonaCedula = prestamo.Persona.Cedula,
                MaterialId = prestamo.MaterialId,
                MaterialTitulo = prestamo.Material.Titulo,
                FechaPrestamo = prestamo.FechaPrestamo,
                FechaDevolucion = prestamo.FechaDevolucion,
                Devuelto = prestamo.Devuelto
            };

            return Ok(prestamoDto);
        }


        [HttpPost]
        public async Task<ActionResult<PrestamoDto>> CreatePrestamo(CreatePrestamoDto prestamoDto)
        {
            // Verificar que la persona existe
            var persona = await _context.Personas
                .Include(p => p.Rol)
                .FirstOrDefaultAsync(p => p.Id == prestamoDto.PersonaId);

            if (persona == null)
            {
                return BadRequest("La persona especificada no existe.");
            }

            // Verificar que el material existe
            var material = await _context.Materiales
                .FirstOrDefaultAsync(m => m.Id == prestamoDto.MaterialId);

            if (material == null)
            {
                return BadRequest("El material especificado no existe.");
            }

            // Verificar que hay unidades disponibles del material
            if (material.CantidadActual <= 0)
            {
                return BadRequest("No hay unidades disponibles de este material.");
            }

            // Verificar la capacidad de préstamo de la persona según su rol
            var prestamosActivos = await _context.Prestamos
                .CountAsync(p => p.PersonaId == prestamoDto.PersonaId && !p.Devuelto);

            if (prestamosActivos >= persona.Rol.CapacidadPrestamo)
            {
                return BadRequest($"La persona ha alcanzado su límite de préstamos ({persona.Rol.CapacidadPrestamo}) según su rol.");
            }

            // Crear el préstamo
            var prestamo = new Prestamo
            {
                PersonaId = prestamoDto.PersonaId,
                MaterialId = prestamoDto.MaterialId,
                FechaPrestamo = DateTime.Now,
                Devuelto = false
            };

            _context.Prestamos.Add(prestamo);

            // Actualizar la cantidad actual del material
            material.CantidadActual--;

            await _context.SaveChangesAsync();

            // Cargar las relaciones para devolver el DTO completo
            await _context.Entry(prestamo).Reference(p => p.Persona).LoadAsync();
            await _context.Entry(prestamo).Reference(p => p.Material).LoadAsync();

            var resultado = new PrestamoDto
            {
                Id = prestamo.Id,
                PersonaId = prestamo.PersonaId,
                PersonaNombre = prestamo.Persona.Nombre,
                PersonaCedula = prestamo.Persona.Cedula,
                MaterialId = prestamo.MaterialId,
                MaterialTitulo = prestamo.Material.Titulo,
                FechaPrestamo = prestamo.FechaPrestamo,
                FechaDevolucion = prestamo.FechaDevolucion,
                Devuelto = prestamo.Devuelto
            };

            return CreatedAtAction(nameof(GetPrestamo), new { id = resultado.Id }, resultado);
        }




        [HttpPost("devolucion")]
        public async Task<ActionResult<PrestamoDto>> RegistrarDevolucion(DevolucionDto devolucionDto)
        {
            // Buscar el préstamo
            var prestamo = await _context.Prestamos
                .Include(p => p.Material)
                .FirstOrDefaultAsync(p => p.Id == devolucionDto.PrestamoId);

            if (prestamo == null)
            {
                return NotFound("El préstamo especificado no existe.");
            }

            // Verificar que el préstamo no haya sido devuelto
            if (prestamo.Devuelto)
            {
                return BadRequest("Este material ya ha sido devuelto.");
            }

            // Registrar la devolución
            prestamo.Devuelto = true;
            prestamo.FechaDevolucion = DateTime.Now;

            // Actualizar la cantidad actual del material
            var material = prestamo.Material;
            material.CantidadActual++;

            await _context.SaveChangesAsync();

            // Cargar relaciones para el DTO completo
            await _context.Entry(prestamo).Reference(p => p.Persona).LoadAsync();

            var resultado = new PrestamoDto
            {
                Id = prestamo.Id,
                PersonaId = prestamo.PersonaId,
                PersonaNombre = prestamo.Persona.Nombre,
                PersonaCedula = prestamo.Persona.Cedula,
                MaterialId = prestamo.MaterialId,
                MaterialTitulo = prestamo.Material.Titulo,
                FechaPrestamo = prestamo.FechaPrestamo,
                FechaDevolucion = prestamo.FechaDevolucion,
                Devuelto = prestamo.Devuelto
            };

            return Ok(resultado);
        }


        [HttpGet("historial")]
        public async Task<ActionResult<IEnumerable<PrestamoDto>>> GetHistorial()
        {
            var prestamos = await _context.Prestamos
                .Include(p => p.Persona)
                .Include(p => p.Material)
                .OrderByDescending(p => p.FechaPrestamo)
                .Select(p => new PrestamoDto
                {
                    Id = p.Id,
                    PersonaId = p.PersonaId,
                    PersonaNombre = p.Persona.Nombre,
                    PersonaCedula = p.Persona.Cedula,
                    MaterialId = p.MaterialId,
                    MaterialTitulo = p.Material.Titulo,
                    FechaPrestamo = p.FechaPrestamo,
                    FechaDevolucion = p.FechaDevolucion,
                    Devuelto = p.Devuelto
                })
                .ToListAsync();

            return Ok(prestamos);
        }


        [HttpGet("activos")]
        public async Task<ActionResult<IEnumerable<PrestamoDto>>> GetPrestamosActivos()
        {
            var prestamosActivos = await _context.Prestamos
                .Include(p => p.Persona)
                .Include(p => p.Material)
                .Where(p => !p.Devuelto)
                .OrderBy(p => p.FechaPrestamo)
                .Select(p => new PrestamoDto
                {
                    Id = p.Id,
                    PersonaId = p.PersonaId,
                    PersonaNombre = p.Persona.Nombre,
                    PersonaCedula = p.Persona.Cedula,
                    MaterialId = p.MaterialId,
                    MaterialTitulo = p.Material.Titulo,
                    FechaPrestamo = p.FechaPrestamo,
                    FechaDevolucion = p.FechaDevolucion,
                    Devuelto = p.Devuelto
                })
                .ToListAsync();

            return Ok(prestamosActivos);
        }


        [HttpGet("persona/{personaId}")]
        public async Task<ActionResult<IEnumerable<PrestamoDto>>> GetPrestamosPorPersona(int personaId)
        {
            // Verificar que la persona existe
            var persona = await _context.Personas.FindAsync(personaId);
            if (persona == null)
            {
                return NotFound("La persona no existe.");
            }

            // Obtener préstamos de la persona
            var prestamos = await _context.Prestamos
                .Include(p => p.Material)
                .Where(p => p.PersonaId == personaId)
                .OrderByDescending(p => p.FechaPrestamo)
                .Select(p => new PrestamoDto
                {
                    Id = p.Id,
                    PersonaId = p.PersonaId,
                    PersonaNombre = persona.Nombre,
                    PersonaCedula = persona.Cedula,
                    MaterialId = p.MaterialId,
                    MaterialTitulo = p.Material.Titulo,
                    FechaPrestamo = p.FechaPrestamo,
                    FechaDevolucion = p.FechaDevolucion,
                    Devuelto = p.Devuelto
                })
                .ToListAsync();

            return Ok(prestamos);
        }

    }
}