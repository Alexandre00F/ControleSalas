using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.RateLimiting;

[AllowAnonymous]
[EnableRateLimiting("contas")]
public class CadastroModel : PageModel
{
    private readonly AppDbContext db;
    public CadastroModel(AppDbContext db) { this.db = db; }

    [BindProperty, ValidateNever]
    public CriarUsuarioDto Conta { get; set; } = new();

    [BindProperty, Required(ErrorMessage = "Confirme a senha.")]
    public string ConfirmarSenha { get; set; } = "";

    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true
        ? RedirectToPage("/Index") : Page();

    public IActionResult OnPost()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToPage("/Index");
        foreach (var erro in ValidationHelper.ValidarModelo(Conta).Values.SelectMany(e => e))
            ModelState.AddModelError("", erro);
        if (Conta.Senha != ConfirmarSenha) ModelState.AddModelError("", "As senhas precisam ser iguais.");
        if (!ModelState.IsValid) return Page();

        // O DTO não contém perfil. O handler sempre cria uma conta Usuario.
        var resultado = UsuarioEndpoints.Criar(Conta, db);
        if ((resultado as IStatusCodeHttpResult)?.StatusCode != 201)
        {
            ModelState.AddModelError("", "Não foi possível cadastrar. Confira se este e-mail já está em uso.");
            return Page();
        }
        TempData["Mensagem"] = "Conta criada! Entre com seu e-mail e sua senha.";
        return RedirectToPage("/Login");
    }
}
