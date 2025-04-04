using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Material
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Titulo { get; set; }

    [Required]
    public int TipoId { get; set; }
    [ForeignKey("TipoId")]
    public TipoMaterial TipoMaterial { get; set; }

    [Required]
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    [Required]
    public int CantidadRegistrada { get; set; }

    [Required]
    public int CantidadActual { get; set; }
}