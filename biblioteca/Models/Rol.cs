using System.ComponentModel.DataAnnotations;

public class Rol
{
    [Key]
    public int Id { get; set; }

    public string RolName { get; set; }

    public int CapacidadPrestamo { get; set; }

}