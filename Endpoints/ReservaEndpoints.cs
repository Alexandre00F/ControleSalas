using Microsoft.EntityFrameworkCore;

public static class ReservaEndpoints
{
    public static void MapReservaEndpoints(this WebApplication app)
    {
        // POST /reservas
        app.MapPost("/reservas", (
            CriarReservaDto reservaDto,
            AppDbContext db) =>
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
        });


        // GET /reservas
        app.MapGet("/reservas", (AppDbContext db) =>
        {
            var reservas = db.Reservas
                .Include(r => r.Sala)
                .Include(r => r.Usuario)
                .ToList();

            return Results.Ok(reservas);
        });


        // GET /reservas/{id}
        app.MapGet("/reservas/{id}", (
            int id,
            AppDbContext db) =>
        {
            var reserva = db.Reservas
                .Include(r => r.Sala)
                .Include(r => r.Usuario)
                .FirstOrDefault(r => r.Id == id);

            if (reserva == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(reserva);
        });


        // PUT /reservas/{id}
        app.MapPut("/reservas/{id}", (
            int id,
            AtualizarReservaDto reservaDto,
            AppDbContext db) =>
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
        });


        // DELETE /reservas/{id}
        app.MapDelete("/reservas/{id}", (
            int id,
            AppDbContext db) =>
        {
            var reserva = db.Reservas.Find(id);

            if (reserva == null)
            {
                return Results.NotFound();
            }

            db.Reservas.Remove(reserva);

            db.SaveChanges();

            return Results.NoContent();
        });
    }
}