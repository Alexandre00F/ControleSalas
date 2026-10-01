using System.ComponentModel.DataAnnotations;

public class CriarUsuarioDto
{
    [Required]
    [MinLength(3)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe uma senha.")]
    [MinLength(8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
    [MaxLength(128)]
    public string Senha { get; set; } = string.Empty;
}
