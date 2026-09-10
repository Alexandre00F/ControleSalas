using Microsoft.EntityFrameworkCore;

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

// REGISTRO DOS ENDPOINTS

app.MapSalaEndpoints();

app.MapUsuarioEndpoints();

app.MapReservaEndpoints();

app.Run();