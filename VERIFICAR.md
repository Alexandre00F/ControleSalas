# Conferência de login e permissões

Execute `dotnet build`, aplique as migrations, crie o administrador com
`--criar-admin` e inicie o servidor. Use a janela normal para o admin e uma
janela privada para o usuário, mantendo as sessões separadas.

| Teste | Resultado esperado |
| --- | --- |
| Abrir `/` sem login | Redireciona para Entrar |
| Usar e-mail/senha inválidos | Mensagem de erro e nenhuma sessão |
| Cadastro com senhas diferentes ou senha curta | Cadastro bloqueado |
| Cadastrar duas contas com mesmo e-mail, variando maiúsculas | Segunda bloqueada |
| Cliente entrar | Apenas Salas disponíveis e Minhas reservas |
| Admin entrar | Salas, Clientes e Reservas com controles administrativos |
| Cliente abrir `/?secao=usuarios` ou `/?secao=salas&editar=1` | Acesso restrito |
| Cliente tentar salvar sala/cliente ou excluir via formulário POST | Acesso restrito; nenhum registro alterado |
| Cliente enviar `Id` diferente de zero ao salvar reserva | Acesso restrito; nenhuma reserva alterada |
| Cliente criar reserva | Dono é sua conta, sem escolher outro cliente |
| Cliente tentar enviar outro `UsuarioId` à API | Dono continua sendo sua conta |
| Cliente listar reservas ou acessar reserva de outro cliente | Lista só as próprias; outra reserva retorna 404 |
| Cliente usar POST/PUT/DELETE de salas ou clientes na API | 403 |
| Cliente usar PUT/DELETE de reservas na API | 403 |
| Chamar API sem cookie de login | 401 |
| Admin fazer escrita na API sem X-CSRF-TOKEN válido | 400; nada alterado |
| Enviar formulário de página sem token antifalsificação | 400; nada alterado |
| Criar reservas com horários sobrepostos na mesma sala | Segunda bloqueada |
| Admin editar/excluir sala, cliente ou reserva | Operação permitida conforme validações existentes |
| Excluir sala ou cliente com reserva vinculada | Bloqueado |
| Excluir o único administrador | Bloqueado |
| Alterar a senha de um cliente antigo e fazer login com ela | Acesso habilitado |
| Cliente com sessão ativa e admin trocar sua senha | Sessão antiga invalidada na próxima requisição |
| Clicar em Sair e voltar a `/` | Pede login |

Para os testes de escrita na API, obtenha o token em `/autenticacao/csrf` depois
do login e informe-o em Authorize no Swagger. Mantenha a mesma sessão e origem.
Os casos acima são um roteiro de verificação; não representam testes HTTP já
executados neste ambiente.
