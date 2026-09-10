using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// CONFIGURAÇÃO DOS SERVIÇOS

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=controleSalas.db"));

var app = builder.Build();

// CONFIGURAÇÃO DO SWAGGER

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// FUNÇÃO DE VALIDAÇÃO

static Dictionary<string, string[]> ValidarModelo(object modelo)
{
    var contexto = new ValidationContext(modelo);

    var resultados = new List<ValidationResult>();

    Validator.TryValidateObject(
        modelo,
        contexto,
        resultados,
        validateAllProperties: true
    );

    return resultados
        .GroupBy(r => r.MemberNames.FirstOrDefault() ?? "")
        .ToDictionary(
            g => g.Key,
            g => g.Select(
                r => r.ErrorMessage ?? "Valor inválido"
            ).ToArray()
        );
}

// GET /teste

app.MapGet("/teste", () =>
{
    return new
    {
        Nome = "Controle de salas",

        Tecnologias = new[]
        {
            "C#",
            ".NET 8",
            "Entity Framework Core",
            "SQLite"
        },

        Versão = "8.0.0"
    };
});


// ==================================================
// SALAS
// ==================================================

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
    var erros = ValidarModelo(salaDto);

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

    var erros = ValidarModelo(salaDto);

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

// ==================================================
// USUÁRIOS
// ==================================================

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
    var erros = ValidarModelo(usuarioDto);

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

    var erros = ValidarModelo(usuarioDto);

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
    var usuario = db.Usuarios.Find(id);

    if (usuario == null)
    {
        return Results.NotFound();
    }

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
});


// ==================================================
// RESERVAS
// ==================================================

// POST /reservas

// POST /reservas
//
// Cria uma nova reserva no sistema.
// Cria uma nova reserva no sistema.
app.MapPost("/reservas", (
    CriarReservaDto reservaDto,
    AppDbContext db) =>
{
    // Valida os dados enviados pelo usuário.
    // Verifica regras como [Required], [EmailAddress], etc.
    var erros = ValidarModelo(reservaDto);

    // Se houver algum erro de validação,
    // retorna HTTP 400 (Bad Request).
    if (erros.Count > 0)
    {
        return Results.BadRequest(erros);
    }

    // Procura no banco uma sala com o ID informado.
    var sala = db.Salas.Find(reservaDto.SalaId);

    // Se a sala não existir, não podemos criar a reserva.
    if (sala == null)
    {
        return Results.BadRequest(new
        {
            erro = "A sala informada não existe."
        });
    }

    // Procura no banco um usuário com o ID informado.
    var usuario = db.Usuarios.Find(reservaDto.UsuarioId);

    // Se o usuário não existir, não podemos criar a reserva.
    if (usuario == null)
    {
        return Results.BadRequest(new
        {
            erro = "O usuário informado não existe."
        });
    }

    // Verifica se o horário de início é anterior ao horário de fim.
    if (reservaDto.Inicio >= reservaDto.Fim)
    {
        // Retorna HTTP 400 porque o período informado é inválido.
        return Results.BadRequest(new
        {
            erro = "O horário de início deve ser anterior ao horário de fim."
        });
    }

    // Verifica se já existe uma reserva para a mesma sala
    // em um horário que se sobrepõe ao horário informado.
    var conflito = db.Reservas.Any(r =>
        r.SalaId == reservaDto.SalaId &&
        reservaDto.Inicio < r.Fim &&
        reservaDto.Fim > r.Inicio
    );

    // Se existir conflito, não permite criar a reserva.
    if (conflito)
    {
        // Retorna HTTP 409 (Conflict),
        // pois a sala já está ocupada nesse horário.
        return Results.Conflict(new
        {
            erro = "Já existe uma reserva para esta sala neste horário."
        });
    }

    // Cria uma nova reserva usando os dados recebidos.
    var reserva = new Reserva
    {
        UsuarioId = reservaDto.UsuarioId,
        SalaId = reservaDto.SalaId,
        Inicio = reservaDto.Inicio,
        Fim = reservaDto.Fim
    };

    // Adiciona a reserva ao Entity Framework.
    // Neste momento ela ainda não foi salva no banco.
    db.Reservas.Add(reserva);

    // Salva a nova reserva no banco SQLite.
    db.SaveChanges();

    // Retorna HTTP 201 (Created)
    // junto com a reserva criada.
    return Results.Created(
        $"/reservas/{reserva.Id}",
        reserva
    );
});


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

app.MapPut("/reservas/{id}", (int id, AtualizarReservaDto reservaDto, AppDbContext db) =>
{
    var reserva = db.Reservas.Find(id);

    if (reserva == null)
    {
        return Results.NotFound();
    }

    var erros = ValidarModelo(reservaDto);

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


app.Run();