using System.ComponentModel.DataAnnotations;

public class TipoMaterial
{
    [Key]
    public int Id { get; set; }

    public string Tipo { get; set; }
}