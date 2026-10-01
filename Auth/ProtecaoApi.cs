using Microsoft.AspNetCore.Antiforgery;

// As páginas Razor validam seus formulários automaticamente.
// A API usa o mesmo cookie de login e exige token nas ações de escrita.
public class ProtecaoApi : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        try
        {
            var antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.BadRequest(new { erro = "Token de proteção inválido. Envie X-CSRF-TOKEN." });
        }
        return await next(context);
    }
}
