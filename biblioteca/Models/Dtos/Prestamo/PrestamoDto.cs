namespace biblioteca.Models.Dtos.Prestamo
{
    public class PrestamoDto
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public string PersonaNombre { get; set; }
        public string PersonaCedula { get; set; }
        public int MaterialId { get; set; }
        public string MaterialTitulo { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public bool Devuelto { get; set; }
    }
}
