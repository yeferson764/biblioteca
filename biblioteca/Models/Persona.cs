using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Index(nameof(Cedula), IsUnique = true)]
public class Persona
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; }

    [Required]
    public string Cedula { get; set; }

    public int RolId { get; set; }
    [ForeignKey("RolId")]
    public Rol Rol { get; set; }
}