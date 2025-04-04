using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Prestamo
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PersonaId { get; set; }
    [ForeignKey("PersonaId")]
    public Persona Persona { get; set; }

    [Required]
    public int MaterialId { get; set; }
    [ForeignKey("MaterialId")]
    public Material Material { get; set; }

    [Required]
    public DateTime FechaPrestamo { get; set; } = DateTime.Now;

    public DateTime? FechaDevolucion { get; set; }

    public bool Devuelto { get; set; }
}
