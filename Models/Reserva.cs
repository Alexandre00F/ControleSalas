public class Reserva
{
    public int Id { get; set; }

    public int SalaId { get; set; }
    public Sala Sala { get; set; } = null!;

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
}