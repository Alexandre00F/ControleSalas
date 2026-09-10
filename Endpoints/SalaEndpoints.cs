public static class SalaEndpoints
{
    public static void MapSalaEndpoints(this WebApplication app)
    {
        // GET /salas
        app.MapGet("/salas", (AppDbContext db) =>
        {
            return db.Salas.ToList();
        });


        // GET /salas/{id}
        app.MapGet("/salas/{id}", (int id, AppDbContext db) =>
        {
            var sala = db.Salas.Find(id);

            if (sala == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(sala);
        });


        // POST /salas
        app.MapPost("/salas", (CriarSalaDto salaDto, AppDbContext db) =>
        {
            var erros = ValidationHelper.ValidarModelo(salaDto);

            if (erros.Count > 0)
            {
                return Results.BadRequest(erros);
            }

            var sala = new Sala
            {
                Nome = salaDto.Nome,
                Capacidade = salaDto.Capacidade
            };

            db.Salas.Add(sala);

            db.SaveChanges();

            return Results.Created(
                $"/salas/{sala.Id}",
                sala
            );
        });


        // PUT /salas/{id}
        app.MapPut("/salas/{id}", (
            int id,
            AtualizarSalaDto salaDto,
            AppDbContext db) =>
        {
            var sala = db.Salas.Find(id);

            if (sala == null)
            {
                return Results.NotFound();
            }

            var erros = ValidationHelper.ValidarModelo(salaDto);

            if (erros.Count > 0)
            {
                return Results.BadRequest(erros);
            }

            sala.Nome = salaDto.Nome;
            sala.Capacidade = salaDto.Capacidade;

            db.SaveChanges();

            return Results.Ok(sala);
        });


        // DELETE /salas/{id}
        app.MapDelete("/salas/{id}", (int id, AppDbContext db) =>
        {
            var sala = db.Salas.Find(id);

            if (sala == null)
            {
                return Results.NotFound();
            }

            var possuiReservas = db.Reservas.Any(r => r.SalaId == id);

            if (possuiReservas)
            {
                return Results.Conflict(new
                {
                    erro = "Não é possível excluir a sala porque existem reservas vinculadas a ela."
                });
            }

            db.Salas.Remove(sala);

            db.SaveChanges();

            return Results.NoContent();
        });
    }
}