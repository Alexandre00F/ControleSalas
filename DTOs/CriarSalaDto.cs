using System.ComponentModel.DataAnnotations;

public class CriarSalaDto
{
    [Required]
    [MinLength(3)]
    public string Nome { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int Capacidade { get; set; }
}