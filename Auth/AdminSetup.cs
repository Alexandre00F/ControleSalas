using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class AdminSetup
{
    public static async Task Executar(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        Console.Write("Nome do administrador: ");
        var nome = (Console.ReadLine() ?? "").Trim();
        Console.Write("E-mail: ");
        var email = (Console.ReadLine() ?? "").Trim();
        Console.Write("Senha (mínimo 8 caracteres): ");
        var senha = LerSenha();
        var dto = new CriarUsuarioDto { Nome = nome, Email = email, Senha = senha };
        var erros = ValidationHelper.ValidarModelo(dto);
        if (erros.Count > 0)
        {
            foreach (var erro in erros.Values.SelectMany(e => e)) Console.WriteLine(erro);
            Environment.ExitCode = 1;
            return;
        }
        var normalizado = ContaHelper.NormalizarEmail(email);
        if (db.Usuarios.Any(u => u.EmailNormalizado == normalizado || u.Email.Trim().ToUpper() == normalizado))
        {
            Console.WriteLine("Já existe uma conta com este e-mail. Use outro e-mail para o administrador.");
            Environment.ExitCode = 1;
            return;
        }
        var admin = new Usuario
        {
            Nome = nome, Email = email, EmailNormalizado = normalizado, Perfil = ContaHelper.Admin
        };
        admin.SenhaHash = new PasswordHasher<Usuario>().HashPassword(admin, senha);
        db.Usuarios.Add(admin);
        await db.SaveChangesAsync();
        Console.WriteLine("Administrador criado. Execute dotnet run --launch-profile http para abrir o sistema.");
    }

    private static string LerSenha()
    {
        if (Console.IsInputRedirected)
            throw new InvalidOperationException("Execute a criação do admin em um terminal interativo.");
        var senha = new System.Text.StringBuilder();
        while (true)
        {
            var tecla = Console.ReadKey(intercept: true);
            if (tecla.Key == ConsoleKey.Enter) break;
            if (tecla.Key == ConsoleKey.Backspace && senha.Length > 0) senha.Length--;
            else if (!char.IsControl(tecla.KeyChar) && senha.Length < 128) senha.Append(tecla.KeyChar);
        }
        Console.WriteLine();
        return senha.ToString();
    }
}
