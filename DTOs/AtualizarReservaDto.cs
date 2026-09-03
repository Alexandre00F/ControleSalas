using System.ComponentModel.DataAnnotations;

public class AtualizarReservaDto
{
    [Required]
    public int SalaId { get; set; }

    [Required]
    public int UsuarioId { get; set; }

    [Required]
    public DateTime Inicio { get; set; }

    [Required]
    public DateTime Fim { get; set; }
}