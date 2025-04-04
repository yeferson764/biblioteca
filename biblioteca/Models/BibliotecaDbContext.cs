using Microsoft.EntityFrameworkCore;

namespace biblioteca.Models
{
    public class BibliotecaDbContext : DbContext
    {
        public BibliotecaDbContext(DbContextOptions<BibliotecaDbContext> options) : base(options)
        {
        }

        public DbSet<TipoMaterial> TipoMateriales { get; set; }
        public DbSet<Material> Materiales { get; set; }
        public DbSet<Prestamo> Prestamos { get; set; }
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Rol> Roles { get; set; }


    }
}
