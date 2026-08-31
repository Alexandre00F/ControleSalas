using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// CONFIGURAÇÃO DOS SERVIÇOS
// ========================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=controleSalas.db"));


var app = builder.Build();


// ========================================
// CONFIGURAÇÃO DO SWAGGER
// ========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// ========================================
// FUNÇÃO DE VALIDAÇÃO
// ========================================

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
            g => g.Select(r => r.ErrorMessage ?? "Valor inválido").ToArray()
        );
}


// ========================================
// GET /teste
// Teste da API
// ========================================

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


// ========================================
// GET /salas
// Lista todas as salas
// ========================================

app.MapGet("/salas", (AppDbContext db) =>
{
    return db.Salas.ToList();
});

// ========================================
// GET /salas/{id}
// Busca uma sala pelo ID
// ========================================

app.MapGet("/salas/{id}", (int id, AppDbContext db) =>
{
    var sala = db.Salas.Find(id);

    if (sala == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(sala);
});

// ========================================
// POST /salas
// Cria uma nova sala
// ========================================

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

    return Results.Created($"/salas/{sala.Id}", sala);
});

// ========================================
// PUT /salas/{id}
// Atualiza uma sala existente
// ========================================

app.MapPut("/salas/{id}", (int id, AtualizarSalaDto salaDto, AppDbContext db) =>
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

// ========================================
// DELETE /salas/{id}
// Exclui uma sala
// ========================================

app.MapDelete("/salas/{id}", (int id, AppDbContext db) =>
{
    var sala = db.Salas.Find(id);

    if (sala == null)
    {
        return Results.NotFound();
    }

    db.Salas.Remove(sala);
    db.SaveChanges();

    return Results.Ok();
});


app.Run();