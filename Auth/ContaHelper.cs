using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

public static class ContaHelper
{
    public const string Admin = "Admin";
    public const string Usuario = "Usuario";
    public static int Id(ClaimsPrincipal principal) =>
        int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public static string NormalizarEmail(string email) => email.Trim().ToUpperInvariant();

    // Trocar a senha invalida cookies antigos na próxima requisição.
    public static string Credencial(Usuario usuario) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(usuario.SenhaHash)));

    public static Task Entrar(HttpContext contexto, Usuario usuario)
    {
        var identidade = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Role, usuario.Perfil),
            new Claim("credencial", Credencial(usuario))
        }, CookieAuthenticationDefaults.AuthenticationScheme);
        return contexto.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidade));
    }
}
