public static class SalaEndpoints
{
    public static void MapSalaEndpoints(this WebApplication app)
    {
        // GET /salas
        app.MapGet("/salas", Listar).RequireAuthorization();


        // GET /salas/{id}
        app.MapGet("/salas/{id}", Buscar).RequireAuthorization();


        // POST /salas
        app.MapPost("/salas", Criar).RequireAuthorization(ContaHelper.Admin).AddEndpointFilter<ProtecaoApi>();


        // PUT /salas/{id}
        app.MapPut("/salas/{id}", Atualizar).RequireAuthorization(ContaHelper.Admin).AddEndpointFilter<ProtecaoApi>();


        // DELETE /salas/{id}
        app.MapDelete("/salas/{id}", Excluir).RequireAuthorization(ContaHelper.Admin).AddEndpointFilter<ProtecaoApi>();
    }


    // Regras compartilhadas pela API JSON e pelos formulários Razor.
    public static IResult Listar(AppDbContext db)
    {
        return Results.Ok(db.Salas.ToList());
    }

    public static IResult Buscar(int id, AppDbContext db)
    {
        var sala = db.Salas.Find(id);

        if (sala == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(sala);
    }

    public static IResult Criar(CriarSalaDto salaDto, AppDbContext db)
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
    }

    public static IResult Atualizar(int id, AtualizarSalaDto salaDto, AppDbContext db)
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
    }

    public static IResult Excluir(int id, AppDbContext db)
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
    }
}
