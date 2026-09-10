using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Sala> Salas { get; set; }

    public DbSet<Usuario> Usuarios { get; set; }

    public DbSet<Reserva> Reservas { get; set; }
}