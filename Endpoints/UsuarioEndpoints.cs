using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this WebApplication app)
    {
        // GET /usuarios
        app.MapGet("/usuarios", Listar).RequireAuthorization(ContaHelper.Admin);


        // GET /usuarios/{id}
        app.MapGet("/usuarios/{id}", Buscar).RequireAuthorization(ContaHelper.Admin);


        // POST /usuarios
        app.MapPost("/usuarios", Criar).RequireAuthorization(ContaHelper.Admin).AddEndpointFilter<ProtecaoApi>();


        // PUT /usuarios/{id}
        app.MapPut("/usuarios/{id}", Atualizar).RequireAuthorization(ContaHelper.Admin).AddEndpointFilter<ProtecaoApi>();


        // DELETE /usuarios/{id}
        app.MapDelete("/usuarios/{id}", Excluir).RequireAuthorization(ContaHelper.Admin).AddEndpointFilter<ProtecaoApi>();
    }


    // Regras compartilhadas pela API JSON e pelos formulários Razor.
    public static IResult Listar(AppDbContext db)
    {
        return Results.Ok(db.Usuarios.ToList());
    }

    public static IResult Buscar(int id, AppDbContext db)
    {
        var usuario = db.Usuarios.Find(id);

        if (usuario == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(usuario);
    }

    public static IResult Criar(CriarUsuarioDto usuarioDto, AppDbContext db)
    {
        var erros = ValidationHelper.ValidarModelo(usuarioDto);

        if (erros.Count > 0)
        {
            return Results.BadRequest(erros);
        }

        var normalizado = ContaHelper.NormalizarEmail(usuarioDto.Email);
        if (db.Usuarios.Any(u => u.EmailNormalizado == normalizado || u.Email.Trim().ToUpper() == normalizado))
            return Results.Conflict(new { erro = "Já existe uma conta com este e-mail." });

        var usuario = new Usuario
        {
            Nome = usuarioDto.Nome,
            Email = usuarioDto.Email.Trim(),
            EmailNormalizado = normalizado,
            Perfil = ContaHelper.Usuario
        };

        usuario.SenhaHash = new PasswordHasher<Usuario>().HashPassword(usuario, usuarioDto.Senha);
        db.Usuarios.Add(usuario);

        try { db.SaveChanges(); }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.Sqlite.SqliteException { SqliteErrorCode: 19 })
        {
            db.ChangeTracker.Clear();
            return Results.Conflict(new { erro = "Não foi possível salvar. Confira se o e-mail já está cadastrado." });
        }

        return Results.Created(
            $"/usuarios/{usuario.Id}",
            usuario
        );
    }

    public static IResult Atualizar(int id, AtualizarUsuarioDto usuarioDto, AppDbContext db)
    {
        var usuario = db.Usuarios.Find(id);

        if (usuario == null)
        {
            return Results.NotFound();
        }

        var erros = ValidationHelper.ValidarModelo(usuarioDto);

        if (erros.Count > 0)
        {
            return Results.BadRequest(erros);
        }

        var normalizado = ContaHelper.NormalizarEmail(usuarioDto.Email);
        if (db.Usuarios.Any(u => u.Id != id &&
            (u.EmailNormalizado == normalizado || u.Email.Trim().ToUpper() == normalizado)))
            return Results.Conflict(new { erro = "Já existe uma conta com este e-mail." });

        usuario.EmailNormalizado = normalizado;
        if (!string.IsNullOrEmpty(usuarioDto.Senha))
            usuario.SenhaHash = new PasswordHasher<Usuario>().HashPassword(usuario, usuarioDto.Senha);
        usuario.Nome = usuarioDto.Nome;
        usuario.Email = usuarioDto.Email.Trim();

        try { db.SaveChanges(); }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.Sqlite.SqliteException { SqliteErrorCode: 19 })
        {
            db.ChangeTracker.Clear();
            return Results.Conflict(new { erro = "Não foi possível salvar. Confira se o e-mail já está cadastrado." });
        }

        return Results.Ok(usuario);
    }

    public static IResult Excluir(int id, AppDbContext db)
    {
        var usuario = db.Usuarios.Find(id);

        if (usuario == null)
        {
            return Results.NotFound();
        }

        if (usuario.Perfil == ContaHelper.Admin && db.Usuarios.Count(u => u.Perfil == ContaHelper.Admin) <= 1)
            return Results.Conflict(new { erro = "Não é possível excluir o único administrador." });

        var possuiReservas = db.Reservas.Any(r => r.UsuarioId == id);

        if (possuiReservas)
        {
            return Results.Conflict(new
            {
                erro = "Não é possível excluir o usuário porque existem reservas vinculadas a ele."
            });
        }

        db.Usuarios.Remove(usuario);
        db.SaveChanges();

        return Results.NoContent();
    }
}
