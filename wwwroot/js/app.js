// Guarda o ID da sala que está sendo editada.
// null significa que estamos cadastrando uma nova sala.
let salaEditandoId = null;


// Localiza o formulário.
const formulario = document.getElementById("form-sala");

// Localiza o botão do formulário.
const botaoSala = document.getElementById("botao-sala");


// ==================================================
// CARREGAR SALAS
// ==================================================

// Busca as salas na API e mostra os dados na tela.
async function carregarSalas() {

    // Faz uma requisição GET para o endpoint /salas.
    const resposta = await fetch("/salas");

    // Converte a resposta da API para JSON.
    const salas = await resposta.json();

    // Localiza a área onde as salas serão exibidas.
    const lista = document.getElementById("lista-salas");

    // Limpa o conteúdo atual.
    lista.innerHTML = "";

    // Percorre todas as salas recebidas.
    salas.forEach(sala => {

        // Cria um elemento para representar a sala.
        const card = document.createElement("div");

        // Adiciona a classe visual do card.
        card.classList.add("card-sala");

        // Preenche o card com os dados da sala.
        card.innerHTML = `
            <h3>${sala.nome}</h3>

            <p>Capacidade: ${sala.capacidade} lugares</p>

            <div class="acoes-sala">

                <button onclick="editarSala(${sala.id})">
                    Editar
                </button>

                <button onclick="excluirSala(${sala.id})">
                    Excluir
                </button>

            </div>
        `;

        // Adiciona o card na página.
        lista.appendChild(card);
    });
}


// ==================================================
// CADASTRAR OU EDITAR
// ==================================================

// Executa quando o formulário for enviado.
formulario.addEventListener("submit", async (evento) => {

    // Impede o navegador de recarregar a página.
    evento.preventDefault();

    // Pega os valores digitados.
    const nome = document.getElementById("nome").value;
    const capacidade = document.getElementById("capacidade").value;


    // ==============================================
    // CADASTRAR
    // ==============================================

    if (salaEditandoId === null) {

        // Envia os dados para a API.
        const resposta = await fetch("/salas", {

            method: "POST",

            headers: {
                "Content-Type": "application/json"
            },

            body: JSON.stringify({
                nome: nome,
                capacidade: Number(capacidade)
            })
        });


        // Verifica se a API criou a sala.
        if (resposta.ok) {

            // Limpa o formulário.
            formulario.reset();

            // Atualiza a lista.
            carregarSalas();

        } else {

            alert("Não foi possível cadastrar a sala.");
        }

        return;
    }


    // ==============================================
    // EDITAR
    // ==============================================

    const resposta = await fetch(`/salas/${salaEditandoId}`, {

        method: "PUT",

        headers: {
            "Content-Type": "application/json"
        },

        body: JSON.stringify({
            nome: nome,
            capacidade: Number(capacidade)
        })
    });


    // Verifica se a atualização funcionou.
    if (resposta.ok) {

        // Limpa o formulário.
        formulario.reset();

        // Sai do modo de edição.
        salaEditandoId = null;

        // Volta o botão para o modo de cadastro.
        botaoSala.textContent = "Cadastrar sala";

        // Atualiza a lista.
        carregarSalas();

    } else {

        alert("Não foi possível editar a sala.");
    }

});


// ==================================================
// EDITAR SALA
// ==================================================

// Coloca o formulário no modo de edição.
async function editarSala(id) {

    // Busca a sala específica na API.
    const resposta = await fetch(`/salas/${id}`);


    // Verifica se conseguiu encontrar a sala.
    if (!resposta.ok) {

        alert("Não foi possível carregar a sala.");

        return;
    }


    // Converte a resposta para JSON.
    const sala = await resposta.json();


    // Guarda o ID da sala que estamos editando.
    salaEditandoId = sala.id;


    // Coloca os dados da sala no formulário.
    document.getElementById("nome").value = sala.nome;

    document.getElementById("capacidade").value = sala.capacidade;


    // Altera o texto do botão.
    botaoSala.textContent = "Salvar alterações";


    // Leva o usuário até o formulário.
    formulario.scrollIntoView({
        behavior: "smooth"
    });
}


// ==================================================
// EXCLUIR SALA
// ==================================================

// Exclui uma sala.
async function excluirSala(id) {

    // Pede confirmação antes de excluir.
    const confirmar = confirm(
        "Tem certeza que deseja excluir esta sala?"
    );


    // Se o usuário cancelar, não faz nada.
    if (!confirmar) {
        return;
    }


    // Envia a requisição DELETE para a API.
    const resposta = await fetch(`/salas/${id}`, {

        method: "DELETE"

    });


    // Verifica se a exclusão funcionou.
    if (resposta.ok) {

        // Atualiza a lista.
        carregarSalas();

    }

    // A API retorna 409 quando existem reservas vinculadas.
    else if (resposta.status === 409) {

        const erro = await resposta.json();

        alert(erro.erro);

    }

    else {

        alert("Não foi possível excluir a sala.");
    }
}


// ==================================================
// INICIAR A PÁGINA
// ==================================================

// Executa a função quando a página for carregada.
carregarSalas();