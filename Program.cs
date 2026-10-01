using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var criarAdmin = args.Contains("--criar-admin");
var builder = WebApplication.CreateBuilder(args.Where(a => a != "--criar-admin").ToArray());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("csrf", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey, In = ParameterLocation.Header, Name = "X-CSRF-TOKEN",
        Description = "Faça login pela interface. Copie o token de /autenticacao/csrf e informe aqui."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference
            { Type = ReferenceType.SecurityScheme, Id = "csrf" } }] = Array.Empty<string>()
    });
});
builder.Services.AddRazorPages();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("contas", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 30, Window = TimeSpan.FromMinutes(1), QueueLimit = 0,
            AutoReplenishment = true
        }));
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=controleSalas.db"));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/SemPermissao";
        options.Cookie.Name = "ControleSalas.Sessao";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;
        options.Events.OnRedirectToLogin = context =>
        {
            if (EhApi(context.Request.Path)) context.Response.StatusCode = 401;
            else context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (EhApi(context.Request.Path)) context.Response.StatusCode = 403;
            else context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnValidatePrincipal = async context =>
        {
            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var id = ContaHelper.Id(context.Principal!);
            var usuario = await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null || string.IsNullOrEmpty(usuario.SenhaHash) ||
                !context.Principal!.IsInRole(usuario.Perfil) ||
                context.Principal.FindFirst("credencial")?.Value != ContaHelper.Credencial(usuario))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync();
            }
        };
    });
builder.Services.AddAuthorization(options =>
    options.AddPolicy(ContaHelper.Admin, policy => policy.RequireRole(ContaHelper.Admin)));

var app = builder.Build();

// Comando local explícito: cria um admin com uma senha escolhida no terminal.
// O cadastro público nunca atribui esse perfil.
if (criarAdmin)
{
    await AdminSetup.Executar(app.Services);
    return;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapGet("/autenticacao/csrf", (HttpContext context, IAntiforgery antiforgery) =>
{
    context.Response.Headers.CacheControl = "no-store";
    return Results.Ok(new { token = antiforgery.GetAndStoreTokens(context).RequestToken });
}).RequireAuthorization();
app.MapSalaEndpoints();
app.MapUsuarioEndpoints();
app.MapReservaEndpoints();
app.Run();

static bool EhApi(PathString path) =>
    path.StartsWithSegments("/salas") || path.StartsWithSegments("/usuarios") ||
    path.StartsWithSegments("/reservas") || path.StartsWithSegments("/autenticacao");
