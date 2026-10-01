# Controle de Salas — administrador e usuário

Projeto .NET 8 + SQLite + Razor Pages. O frontend usa HTML, CSS e formulários
processados no servidor, sem JavaScript próprio e sem pacotes novos.

## Permissões

| Ação | Administrador | Usuário |
| --- | --- | --- |
| Criar sua própria conta pelo cadastro público | Conta pública sempre recebe perfil Usuário | Sim |
| Consultar salas | Sim | Sim |
| Cadastrar, editar e excluir salas | Sim | Não |
| Cadastrar, editar e excluir clientes | Sim | Não |
| Reservar sala | Para um cliente selecionado | Em seu próprio nome |
| Consultar reservas | Todas | Somente as próprias |
| Editar e excluir reservas | Sim | Não |

Cliente é a conta da tabela `Usuarios`. O login define o que cada pessoa pode
acessar; não há seletor de perfil na tela. A reserva do usuário recebe o ID da
sessão, mesmo que alguém tente enviar outro ID pelo formulário ou pela API.

## Executar no Windows

Extraia o ZIP em uma **pasta nova** e abra o terminal na pasta que contém
`ControleSalas.csproj` (a pasta `ControleSalas` dentro do ZIP).

```powershell
dotnet restore
dotnet ef database update
dotnet run --no-launch-profile -- --criar-admin
```

O último comando pergunta o nome, o e-mail e a senha do administrador. A senha
não aparece enquanto você digita. A conta é criada e o comando termina. Não
há senha padrão. Não use esse mesmo e-mail no cadastro público.

Depois execute:

```powershell
dotnet run --launch-profile http
```

Abra **http://localhost:5209/**. Entre com a conta de administrador para cadastrar
as salas. Um cliente pode abrir **http://localhost:5209/Cadastro**, criar sua
conta e depois fazer login para reservar uma sala.

Se `dotnet ef` ainda não estiver instalado:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.0
```

Reabra o terminal e volte à pasta do `.csproj`. As três migrations já estão
incluídas; não crie outra para esta alteração.

## Telas

- **Entrar:** e-mail, senha e link para criar conta.
- **Criar conta:** nome, e-mail, senha e confirmação. Sempre cria perfil Usuário.
- **Administrador:** abas Salas, Clientes e Reservas, com cadastro, edição e
  confirmação de exclusão.
- **Usuário:** abas Salas disponíveis e Minhas reservas. Pode cadastrar uma
  reserva, sem selecionar outro cliente; não vê ações de edição e exclusão.
- **Sair:** encerra a sessão por formulário POST.

As regras de conflito de horário e de bloqueio de exclusão de salas/clientes
com reservas foram mantidas. O único administrador não pode ser excluído.

## Banco anterior

Faça uma cópia de segurança do seu banco antes de atualizar. Se quiser manter
os dados, pare a aplicação e copie `controleSalas.db` para a pasta nova **antes**
de rodar `dotnet ef database update`.

A migration `AddAutenticacao` adiciona perfil, hash de senha e e-mail normalizado;
não remove salas, clientes nem reservas. Os clientes antigos ficam com perfil
Usuário e sem senha, porque o projeto anterior não possuía login. O admin pode
editar cada cliente antigo e definir sua senha para habilitar o acesso.

O banco não vem no ZIP. O e-mail deve ser único para criar/habilitar uma conta.
Se houver clientes antigos com o mesmo e-mail, o admin precisa corrigir seus
cadastros. Novas contas têm índice único no e-mail normalizado.

## Como funciona no código

- `Auth/ContaHelper.cs`: identidade, ID da sessão e perfis.
- `Auth/AdminSetup.cs`: criação local explícita do administrador.
- `Auth/ProtecaoApi.cs`: valida token contra falsificação nas escritas da API.
- `Pages/Login*`, `Cadastro*` e `Sair*`: autenticação por cookie.
- `Pages/Index.cshtml`: interface conforme o perfil.
- `Pages/Index.cshtml.cs`: verifica permissões antes das ações, filtra reservas
  por dono e chama as regras do backend.
- `Endpoints/`: acesso autenticado às salas e reservas; ações administrativas
  protegidas pela política Admin.
- `Models/Usuario.cs`: senha armazenada como hash com PasswordHasher do ASP.NET
  Core. Hash e e-mail normalizado não são serializados nas respostas JSON.

A senha não aparece em formulários de edição. Trocar uma senha ou excluir a
conta invalida a sessão antiga na próxima requisição. Login e cadastro possuem
limite de 30 requisições por minuto por endereço IP. Horários continuam locais,
sem conversão de fuso, como no backend anterior.

## Swagger e API

Em Development, **http://localhost:5209/swagger** continua disponível.
Agora a API exige autenticação: sem sessão retorna 401; sem perfil Admin, as
ações administrativas retornam 403. O cadastro público está na página
`/Cadastro`; `POST /usuarios` fica exclusivo do admin.

Para testar escritas no Swagger:

1. Faça login na interface no mesmo navegador e na mesma origem (localhost).
2. Abra **http://localhost:5209/autenticacao/csrf** e copie o campo `token`.
3. No Swagger, clique em **Authorize**, cole esse token e confirme.
4. Execute as requisições. O Swagger enviará `X-CSRF-TOKEN` e usará seu cookie.

`POST /usuarios` agora exige também `senha` (mínimo 8 caracteres).
`PUT /usuarios/{id}` aceita `senha` opcional; omita ou envie null para manter a
atual. Clientes nunca recebem perfil Admin pelo DTO.

## Verificação

A revisão estática cobriu as restrições nas páginas e em todas as rotas da API,
o vínculo das reservas à sessão, ausência de JavaScript e campos sensíveis no
JSON. A estrutura da migration foi conferida com SQLite em memória, mantendo
registros anteriores e rejeitando e-mail normalizado duplicado.

Este ambiente não possui o SDK .NET. **A compilação e os testes HTTP do login e
das permissões não foram executados aqui.** Rode `dotnet build` e use os casos
em `VERIFICAR.md` na sua máquina para conferir o comportamento real.
