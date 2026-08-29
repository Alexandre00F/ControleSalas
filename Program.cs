var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/teste", () =>
{
    return new{ Nome = "Controle de salas",
    Tecnologias = new string[] { "C#", ".NET 8", "Entity Framework Core", "SQLite" },
    Versão = "8.0.0",
    };
});

app.MapGet("/salas", () =>
{
    return new Sala []{
        new Sala { Id = 1, Nome = "Sala de Reunião", Capacidade = 10} ,
        new Sala{Id = 2, Nome = "Sala de Treinamento", Capacidade = 20} ,
        new Sala{Id = 3, Nome = "Sala de Conferência", Capacidade = 30 }      

    };
    
});

app.Run();
