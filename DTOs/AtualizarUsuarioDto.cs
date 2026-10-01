using System.ComponentModel.DataAnnotations;

public class AtualizarUsuarioDto
{
    [Required]
    [MinLength(3)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    // Vazio mantém a senha atual. Apenas o administrador usa esta atualização.
    [MinLength(8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
    [MaxLength(128)]
    public string? Senha { get; set; }
}
