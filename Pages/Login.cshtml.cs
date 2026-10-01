using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

[AllowAnonymous]
[EnableRateLimiting("contas")]
public class LoginModel : PageModel
{
    private readonly AppDbContext db;
    public LoginModel(AppDbContext db) { this.db = db; }

    [BindProperty, Required, EmailAddress]
    public string Email { get; set; } = "";

    [BindProperty, Required, MaxLength(128)]
    public string Senha { get; set; } = "";

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true
        ? RedirectToPage("/Index") : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var normalizado = ContaHelper.NormalizarEmail(Email);
        var usuario = db.Usuarios.FirstOrDefault(u => u.EmailNormalizado == normalizado);
        var hasher = new PasswordHasher<Usuario>();
        var resultado = usuario == null || string.IsNullOrEmpty(usuario.SenhaHash)
            ? PasswordVerificationResult.Failed
            : hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, Senha);
        if (resultado == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "E-mail ou senha inválidos.");
            return Page();
        }
        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario!.SenhaHash = hasher.HashPassword(usuario, Senha);
            db.SaveChanges();
        }
        await ContaHelper.Entrar(HttpContext, usuario!);
        return !string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl)
            ? LocalRedirect(ReturnUrl) : RedirectToPage("/Index");
    }
}
