public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this WebApplication app)
    {
        // GET /usuarios
        app.MapGet("/usuarios", (AppDbContext db) =>
        {
            return db.Usuarios.ToList();
        });


        // GET /usuarios/{id}
        app.MapGet("/usuarios/{id}", (int id, AppDbContext db) =>
        {
            var usuario = db.Usuarios.Find(id);

            if (usuario == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(usuario);
        });


        // POST /usuarios
        app.MapPost("/usuarios", (
            CriarUsuarioDto usuarioDto,
            AppDbContext db) =>
        {
            var erros = ValidationHelper.ValidarModelo(usuarioDto);

            if (erros.Count > 0)
            {
                return Results.BadRequest(erros);
            }

            var usuario = new Usuario
            {
                Nome = usuarioDto.Nome,
                Email = usuarioDto.Email
            };

            db.Usuarios.Add(usuario);

            db.SaveChanges();

            return Results.Created(
                $"/usuarios/{usuario.Id}",
                usuario
            );
        });


        // PUT /usuarios/{id}
        app.MapPut("/usuarios/{id}", (
            int id,
            AtualizarUsuarioDto usuarioDto,
            AppDbContext db) =>
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

            usuario.Nome = usuarioDto.Nome;
            usuario.Email = usuarioDto.Email;

            db.SaveChanges();

            return Results.Ok(usuario);
        });


        // DELETE /usuarios/{id}
app.MapDelete("/usuarios/{id}", (
    int id,
    AppDbContext db) =>
{
    // Procura o usuário pelo ID.
    var usuario = db.Usuarios.Find(id);

    // Se o usuário não existir, retorna 404.
    if (usuario == null)
    {
        return Results.NotFound();
    }

    // Verifica se existem reservas vinculadas ao usuário.
    var possuiReservas = db.Reservas.Any(r => r.UsuarioId == id);

    // Não permite excluir um usuário que possui reservas.
    if (possuiReservas)
    {
        return Results.Conflict(new
        {
            erro = "Não é possível excluir o usuário porque existem reservas vinculadas a ele."
        });
    }

    // Remove o usuário.
    db.Usuarios.Remove(usuario);

    // Salva a alteração.
    db.SaveChanges();

    // Retorna 204.
    return Results.NoContent();
});
    }
}