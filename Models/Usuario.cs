using System.Text.Json.Serialization;

public class Usuario
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [JsonIgnore]
    public string? EmailNormalizado { get; set; }

    [JsonIgnore]
    public string SenhaHash { get; set; } = string.Empty;

    public string Perfil { get; set; } = "Usuario";
}
