namespace biblioteca.Models.Dtos.Persona
{
    public class PersonaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Cedula { get; set; }
        public int RolId { get; set; }
        public string RolNombre { get; set; }
        public int CapacidadPrestamo { get; set; }
    }
}
