using System.Security.Claims;

using Microsoft.EntityFrameworkCore;

public static class ReservaEndpoints
{
    public static void MapReservaEndpoints(this WebApplication app)
    {
        // POST /reservas
        app.MapPost("/reservas", (CriarReservaDto dto, ClaimsPrincipal principal, AppDbContext db) =>
        {
            if (!principal.IsInRole(ContaHelper.Admin)) dto.UsuarioId = ContaHelper.Id(principal);
            return Criar(dto, db);
        }).RequireAuthorization().AddEndpointFilter<ProtecaoApi>();


        // GET /reservas
        app.MapGet("/reservas", (ClaimsPrincipal principal, AppDbContext db) => Listar(db, principal)).RequireAuthorization();


        // GET /reservas/{id}
        app.MapGet("/reservas/{id}", (int id, ClaimsPrincipal principal, AppDbContext db) => Buscar(id, db, principal)).RequireAuthorization();


        // PUT /reservas/{id}
        app.MapPut("/reservas/{id}", Atualizar).RequireAuthorization(ContaHelper.Admin).AddEndpointFilter<ProtecaoApi>();


        // DELETE /reservas/{id}
        app.MapDelete("/reservas/{id}", Excluir).RequireAuthorization(ContaHelper.Admin).AddEndpointFilter<ProtecaoApi>();
    }


    // Regras compartilhadas pela API JSON e pelos formulários Razor.
    public static IResult Criar(CriarReservaDto reservaDto, AppDbContext db)
    {
        var erros = ValidationHelper.ValidarModelo(reservaDto);

        if (erros.Count > 0)
        {
            return Results.BadRequest(erros);
        }

        var sala = db.Salas.Find(reservaDto.SalaId);

        if (sala == null)
        {
            return Results.BadRequest(new
            {
                erro = "A sala informada não existe."
            });
        }

        var usuario = db.Usuarios.Find(reservaDto.UsuarioId);

        if (usuario == null)
        {
            return Results.BadRequest(new
            {
                erro = "O usuário informado não existe."
            });
        }

        if (reservaDto.Inicio >= reservaDto.Fim)
        {
            return Results.BadRequest(new
            {
                erro = "O horário de início deve ser anterior ao horário de fim."
            });
        }

        var conflito = db.Reservas.Any(r =>
            r.SalaId == reservaDto.SalaId &&
            reservaDto.Inicio < r.Fim &&
            reservaDto.Fim > r.Inicio
        );

        if (conflito)
        {
            return Results.Conflict(new
            {
                erro = "Já existe uma reserva para esta sala neste horário."
            });
        }

        var reserva = new Reserva
        {
            UsuarioId = reservaDto.UsuarioId,
            SalaId = reservaDto.SalaId,
            Inicio = reservaDto.Inicio,
            Fim = reservaDto.Fim
        };

        db.Reservas.Add(reserva);

        db.SaveChanges();

        return Results.Created(
            $"/reservas/{reserva.Id}",
            reserva
        );
    }

    public static IResult Listar(AppDbContext db, ClaimsPrincipal principal)
    {
        var consulta = db.Reservas.Include(r => r.Sala).Include(r => r.Usuario).AsQueryable();
        if (!principal.IsInRole(ContaHelper.Admin))
        {
            var usuarioId = ContaHelper.Id(principal);
            consulta = consulta.Where(r => r.UsuarioId == usuarioId);
        }
        var reservas = consulta.ToList();

        return Results.Ok(reservas);
    }

    public static IResult Buscar(int id, AppDbContext db, ClaimsPrincipal principal)
    {
        var reserva = db.Reservas
            .Include(r => r.Sala)
            .Include(r => r.Usuario)
            .FirstOrDefault(r => r.Id == id);

        if (reserva == null || (!principal.IsInRole(ContaHelper.Admin) &&
            reserva.UsuarioId != ContaHelper.Id(principal))) return Results.NotFound();
        return Results.Ok(reserva);
    }

    public static IResult Atualizar(int id, AtualizarReservaDto reservaDto, AppDbContext db)
    {
        var reserva = db.Reservas.Find(id);

        if (reserva == null)
        {
            return Results.NotFound();
        }

        var erros = ValidationHelper.ValidarModelo(reservaDto);

        if (erros.Count > 0)
        {
            return Results.BadRequest(erros);
        }

        var usuario = db.Usuarios.Find(reservaDto.UsuarioId);

        if (usuario == null)
        {
            return Results.BadRequest(new
            {
                erro = "O usuário informado não existe."
            });
        }

        var sala = db.Salas.Find(reservaDto.SalaId);

        if (sala == null)
        {
            return Results.BadRequest(new
            {
                erro = "A sala informada não existe."
            });
        }

        if (reservaDto.Inicio >= reservaDto.Fim)
        {
            return Results.BadRequest(new
            {
                erro = "O horário de início deve ser anterior ao horário de fim."
            });
        }

        var conflito = db.Reservas.Any(r =>
            r.Id != id &&
            r.SalaId == reservaDto.SalaId &&
            reservaDto.Inicio < r.Fim &&
            reservaDto.Fim > r.Inicio
        );

        if (conflito)
        {
            return Results.Conflict(new
            {
                erro = "Já existe uma reserva para esta sala neste horário."
            });
        }

        reserva.UsuarioId = reservaDto.UsuarioId;
        reserva.SalaId = reservaDto.SalaId;
        reserva.Inicio = reservaDto.Inicio;
        reserva.Fim = reservaDto.Fim;

        db.SaveChanges();

        return Results.Ok(reserva);
    }

    public static IResult Excluir(int id, AppDbContext db)
    {
        var reserva = db.Reservas.Find(id);

        if (reserva == null)
        {
            return Results.NotFound();
        }

        db.Reservas.Remove(reserva);

        db.SaveChanges();

        return Results.NoContent();
    }
}
