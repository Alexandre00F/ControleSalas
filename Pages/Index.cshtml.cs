using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext db;

    public IndexModel(AppDbContext db)
    {
        this.db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string? Secao { get; set; }

    [BindProperty]
    public int Id { get; set; }

    // Cada handler recebe apenas os dados do seu próprio formulário.
    public CriarSalaDto Sala { get; set; } = new();

    public CriarUsuarioDto Usuario { get; set; } = new();

    public CriarReservaDto Reserva { get; set; } = new();

    [TempData]
    public string? Mensagem { get; set; }

    [TempData]
    public string? Erro { get; set; }

    public List<Sala> Salas { get; private set; } = new();

    public List<Usuario> Usuarios { get; private set; } = new();

    public List<Reserva> Reservas { get; private set; } = new();

    public DateTime DataConsulta { get; private set; } = DateTime.Today;

    private Dictionary<int, List<HorarioOcupado>> horariosPorSala = new();

    // Contém somente os dados necessários para consultar a ocupação.
    public sealed class HorarioOcupado
    {
        public int SalaId { get; set; }

        public DateTime Inicio { get; set; }

        public DateTime Fim { get; set; }
    }

    public IReadOnlyList<HorarioOcupado> HorariosDaSala(int salaId)
    {
        if (horariosPorSala.TryGetValue(salaId, out var horarios))
        {
            return horarios;
        }

        return Array.Empty<HorarioOcupado>();
    }

    public bool Editando { get; private set; }

    public bool ConfirmandoExclusao { get; private set; }

    public string NomeExclusao { get; private set; } = "";

    public bool EhAdmin => User.IsInRole(ContaHelper.Admin);

    public string Titulo => Secao switch
    {
        "usuarios" => "Clientes",
        "reservas" => "Reservas",
        _ => "Salas"
    };

    public IActionResult OnGet(
        int? editar,
        int? excluir,
        DateTime? dataConsulta)
    {
        if (!EhAdmin &&
            (Secao == "usuarios" || editar.HasValue || excluir.HasValue))
        {
            return Forbid();
        }

        Secao = Secao is "salas" or "usuarios" or "reservas"
            ? Secao
            : (EhAdmin ? "salas" : "reservas");

        DataConsulta = (dataConsulta ?? DateTime.Today).Date;

        CarregarListas();

        var id = editar ?? excluir;

        if (id.HasValue)
        {
            Id = id.Value;

            switch (Secao)
            {
                case "salas":
                    var sala = Salas.Find(s => s.Id == Id);

                    if (sala == null)
                    {
                        return NotFound();
                    }

                    Sala = new CriarSalaDto
                    {
                        Nome = sala.Nome,
                        Capacidade = sala.Capacidade
                    };

                    NomeExclusao = sala.Nome;
                    break;

                case "usuarios":
                    var usuario = Usuarios.Find(u => u.Id == Id);

                    if (usuario == null)
                    {
                        return NotFound();
                    }

                    Usuario = new CriarUsuarioDto
                    {
                        Nome = usuario.Nome,
                        Email = usuario.Email
                    };

                    NomeExclusao = usuario.Nome;
                    break;

                case "reservas":
                    var reserva = Reservas.Find(r => r.Id == Id);

                    if (reserva == null)
                    {
                        return NotFound();
                    }

                    Reserva = new CriarReservaDto
                    {
                        SalaId = reserva.SalaId,
                        UsuarioId = reserva.UsuarioId,
                        Inicio = reserva.Inicio,
                        Fim = reserva.Fim
                    };

                    NomeExclusao =
                        $"Reserva #{reserva.Id} — {reserva.Sala.Nome}";
                    break;
            }

            Editando = editar.HasValue;
            ConfirmandoExclusao = !Editando && excluir.HasValue;
        }

        return Page();
    }

    public IActionResult OnPostSalvarSala(
        [FromForm(Name = "Sala")] CriarSalaDto dados)
    {
        if (!EhAdmin)
        {
            return Forbid();
        }

        Secao = "salas";
        Sala = dados;

        if (!FormularioValido())
        {
            return Reexibir();
        }

        var resultado = Id > 0
            ? SalaEndpoints.Atualizar(
                Id,
                new AtualizarSalaDto
                {
                    Nome = Sala.Nome,
                    Capacidade = Sala.Capacidade
                },
                db)
            : SalaEndpoints.Criar(Sala, db);

        return Processar(
            resultado,
            Id > 0 ? "Sala atualizada." : "Sala cadastrada.");
    }

    public IActionResult OnPostSalvarUsuario(
        [FromForm(Name = "Usuario")] AtualizarUsuarioDto dados)
    {
        if (!EhAdmin)
        {
            return Forbid();
        }

        Secao = "usuarios";

        Usuario = new CriarUsuarioDto
        {
            Nome = dados.Nome,
            Email = dados.Email,
            Senha = dados.Senha ?? ""
        };

        if (!FormularioValido())
        {
            return Reexibir();
        }

        var resultado = Id > 0
            ? UsuarioEndpoints.Atualizar(Id, dados, db)
            : UsuarioEndpoints.Criar(Usuario, db);

        return Processar(
            resultado,
            Id > 0 ? "Cliente atualizado." : "Cliente cadastrado.");
    }

    public IActionResult OnPostSalvarReserva(
        [FromForm(Name = "Reserva")] CriarReservaDto dados)
    {
        Secao = "reservas";
        Reserva = dados;

        if (!EhAdmin)
        {
            if (Id != 0)
            {
                return Forbid();
            }

            Reserva.UsuarioId = ContaHelper.Id(User);
            ModelState.Remove("Reserva.UsuarioId");
        }

        if (Reserva.SalaId <= 0 || Reserva.UsuarioId <= 0)
        {
            ModelState.AddModelError(
                "",
                "Selecione uma sala e um usuário.");
        }

        if (Reserva.Inicio == default || Reserva.Fim == default)
        {
            ModelState.AddModelError(
                "",
                "Preencha a data e o horário de início e de fim.");
        }

        if (!FormularioValido())
        {
            return Reexibir();
        }

        var resultado = Id > 0
            ? ReservaEndpoints.Atualizar(
                Id,
                new AtualizarReservaDto
                {
                    SalaId = Reserva.SalaId,
                    UsuarioId = Reserva.UsuarioId,
                    Inicio = Reserva.Inicio,
                    Fim = Reserva.Fim
                },
                db)
            : ReservaEndpoints.Criar(Reserva, db);

        return Processar(
            resultado,
            Id > 0 ? "Reserva atualizada." : "Reserva cadastrada.");
    }

    public IActionResult OnPostExcluir(
        [FromForm] int id,
        [FromForm] string? secao)
    {
        if (!EhAdmin)
        {
            return Forbid();
        }

        var idInvalido =
            ModelState.TryGetValue(nameof(Id), out var estadoId)
            && estadoId.Errors.Count > 0;

        var secaoInvalida =
            ModelState.TryGetValue(nameof(Secao), out var estadoSecao)
            && estadoSecao.Errors.Count > 0;

        if (id <= 0 ||
            idInvalido ||
            secaoInvalida ||
            secao is not ("salas" or "usuarios" or "reservas"))
        {
            Erro =
                "Não foi possível identificar o registro. " +
                "Abra a lista e tente excluir novamente.";

            return RedirectToPage("/Index");
        }

        Id = id;
        Secao = secao;

        ModelState.Clear();

        IResult resultado = Secao switch
        {
            "salas" => SalaEndpoints.Excluir(Id, db),
            "usuarios" => UsuarioEndpoints.Excluir(Id, db),
            _ => ReservaEndpoints.Excluir(Id, db)
        };

        return Processar(
            resultado,
            "Registro excluído.",
            excluindo: true);
    }

    private bool FormularioValido()
    {
        if (Id < 0)
        {
            ModelState.AddModelError(
                "",
                "Identificador inválido.");
        }

        return ModelState.IsValid;
    }

    private void CarregarListas()
    {
        var usuarioId = ContaHelper.Id(User);
        var ehAdmin = EhAdmin;

        Salas = db.Salas
            .AsNoTracking()
            .OrderBy(s => s.Nome)
            .ToList();

        Usuarios = db.Usuarios
            .AsNoTracking()
            .Where(u => ehAdmin || u.Id == usuarioId)
            .OrderBy(u => u.Nome)
            .ToList();

        // O usuário comum vê apenas suas próprias reservas nesta lista.
        Reservas = db.Reservas
            .AsNoTracking()
            .Include(r => r.Sala)
            .Include(r => r.Usuario)
            .Where(r => ehAdmin || r.UsuarioId == usuarioId)
            .OrderBy(r => r.Inicio)
            .ToList();

        if (Secao == "salas")
        {
            var inicioDia = DataConsulta.Date;

            var fimDia = inicioDia == DateTime.MaxValue.Date
                ? DateTime.MaxValue
                : inicioDia.AddDays(1);

            // A ocupação considera reservas de todos os usuários.
            // Inclui também reservas que atravessam a meia-noite.
            var horarios = db.Reservas
                .AsNoTracking()
                .Where(r => r.Inicio < fimDia && r.Fim > inicioDia)
                .OrderBy(r => r.Inicio)
                .ThenBy(r => r.Fim)
                .Select(r => new HorarioOcupado
                {
                    SalaId = r.SalaId,
                    Inicio = r.Inicio,
                    Fim = r.Fim
                })
                .ToList();

            horariosPorSala = horarios
                .GroupBy(r => r.SalaId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }

    private IActionResult Reexibir()
    {
        Editando = Id > 0;

        CarregarListas();

        return Page();
    }

    private IActionResult Processar(
        IResult resultado,
        string sucesso,
        bool excluindo = false)
    {
        var status =
            (resultado as IStatusCodeHttpResult)?.StatusCode ?? 200;

        if (status is >= 200 and < 300)
        {
            Mensagem = sucesso;

            if (excluindo)
            {
                return RedirectToPage("/Index");
            }

            return RedirectToPage(
                "/Index",
                new { secao = Secao });
        }

        if (status == 404)
        {
            ModelState.AddModelError(
                "",
                "Este registro não existe mais. Volte para a lista.");
        }
        else if (
            (resultado as IValueHttpResult)?.Value
            is Dictionary<string, string[]> erros)
        {
            foreach (var erro in erros)
            {
                foreach (var mensagem in erro.Value)
                {
                    ModelState.AddModelError("", mensagem);
                }
            }
        }
        else if (
            (resultado as IValueHttpResult)?.Value is object valor)
        {
            var json = JsonSerializer.SerializeToElement(valor);

            var mensagem = json.TryGetProperty("erro", out var erro)
                ? erro.GetString()
                : "Não foi possível concluir esta ação.";

            ModelState.AddModelError(
                "",
                mensagem ?? "Não foi possível concluir esta ação.");
        }
        else
        {
            ModelState.AddModelError(
                "",
                "Não foi possível concluir esta ação.");
        }

        if (excluindo)
        {
            Id = 0;
            ModelState.Remove(nameof(Id));
        }

        return Reexibir();
    }
}