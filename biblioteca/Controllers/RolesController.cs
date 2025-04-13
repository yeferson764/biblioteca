using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using biblioteca.Models;
using System;
using System.Threading.Tasks;
using biblioteca.Models.Dtos;

namespace biblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly BibliotecaDbContext _context;

        public RolesController(BibliotecaDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rol>>> GetRoles()
        {
            return await _context.Roles.ToListAsync();
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Rol>> GetRol(int id)
        {
            var rol = await _context.Roles.FindAsync(id);

            if (rol == null)
            {
                return NotFound();
            }

            return rol;
        }


        [HttpPost]
        public async Task<ActionResult<Rol>> PostRol(CreateRolDto rolDto)
        {
            // Crear el objeto Rol a partir del DTO
            var rol = new Rol
            {
                RolName = rolDto.RolName,
                CapacidadPrestamo = rolDto.CapacidadPrestamo
            };

            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRol), new { id = rol.Id }, rol);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRol(int id, CreateRolDto rolDto)
        {
            // Buscar el rol existente
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                return NotFound($"No se encontró el rol con ID {id}");
            }

            // Actualizar solo los campos permitidos
            rol.RolName = rolDto.RolName;
            rol.CapacidadPrestamo = rolDto.CapacidadPrestamo;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(GetRol), new { id = rol.Id }, rol);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRol(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                return NotFound();
            }

            // Verificar si hay personas asociadas a este rol
            bool tienePersonasAsociadas = await _context.Personas
                .AnyAsync(p => p.RolId == id);

            if (tienePersonasAsociadas)
            {
                return BadRequest("No se puede eliminar el rol porque hay personas asociadas a él");
            }

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}