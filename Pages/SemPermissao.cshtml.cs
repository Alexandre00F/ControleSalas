using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize]
public class SemPermissaoModel : PageModel
{
    public void OnGet() => Response.StatusCode = 403;
}
