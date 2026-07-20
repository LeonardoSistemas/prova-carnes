# Backlog - User Stories

Referência: [PRD.md](./PRD.md). Prioridade MoSCoW indicada em cada story.

## Carnes

### US01 - Cadastrar carne (Must)
Como usuário do sistema, quero cadastrar uma carne com descrição e origem,
para que ela fique disponível para uso em pedidos.

Critérios de aceite:
- [ ] Descrição é obrigatória e não pode ser vazia/whitespace.
- [ ] Origem deve ser uma das 4 opções válidas (Bovina, Suína, Aves, Peixes);
      valor fora dessa lista retorna 400.
- [ ] POST /carnes retorna 201 com o recurso criado (incluindo Id).
- [ ] Após salvar, a carne aparece na listagem imediatamente (sem reload manual).

### US02 - Listar carnes (Must)
Como usuário, quero ver todas as carnes cadastradas, para escolher entre elas
ao montar um pedido.

Critérios de aceite:
- [ ] GET /carnes retorna Id, descrição e origem de cada carne.
- [ ] Lista vazia retorna 200 com array vazio (não erro).

### US03 - Editar carne (Must)
Como usuário, quero editar descrição/origem de uma carne, para corrigir dados
cadastrados incorretamente.

Critérios de aceite:
- [ ] PUT /carnes/{id} valida os mesmos campos obrigatórios do cadastro.
- [ ] Id inexistente retorna 404.
- [ ] Alteração refletida na listagem sem reload manual.

### US04 - Excluir carne com bloqueio de integridade (Must)
Como usuário, quero ser impedido de excluir uma carne que já foi usada em
algum pedido, para não corromper o histórico de pedidos.

Critérios de aceite:
- [ ] DELETE /carnes/{id} retorna 409 (ou 422) se existir PedidoItem
      referenciando essa carne, com mensagem explicando o motivo.
- [ ] DELETE de carne sem vínculo retorna 204 e remove da listagem.
- [ ] Frontend exibe a mensagem de erro do backend ao usuário (não um erro
      genérico) quando a exclusão é bloqueada.

## Compradores

### US05 - Cadastrar comprador (Must)
Como usuário, quero cadastrar um comprador com nome, documento, cidade e
estado, para associá-lo a pedidos.

Critérios de aceite:
- [ ] Nome é obrigatório.
- [ ] Documento é obrigatório (texto livre, sem validação de dígito).
- [ ] Cidade e Estado são selecionados de listas pré-cadastradas (não texto
      livre) e a combinação deve existir no banco.
- [ ] POST /compradores retorna 201 com o recurso criado.

### US06 - Listar compradores (Must)
Como usuário, quero ver todos os compradores cadastrados, para escolher entre
eles ao montar um pedido.

Critérios de aceite:
- [ ] GET /compradores retorna Id, nome e documento de cada comprador.

### US07 - Editar comprador (Must)
Como usuário, quero editar os dados de um comprador, para manter o cadastro
atualizado.

Critérios de aceite:
- [ ] PUT /compradores/{id} valida os mesmos campos obrigatórios do cadastro.
- [ ] Id inexistente retorna 404.

### US08 - Excluir comprador com bloqueio de integridade (Must)
Como usuário, quero ser impedido de excluir um comprador que já tem pedidos,
para preservar o histórico de pedidos.

Critérios de aceite:
- [ ] DELETE /compradores/{id} retorna 409 (ou 422) se existir Pedido
      vinculado a esse comprador, com mensagem clara.
- [ ] DELETE de comprador sem pedidos retorna 204.

## Pedidos

### US09 - Criar pedido com itens multi-moeda (Must)
Como usuário, quero criar um pedido com data, comprador e um ou mais itens de
carne (cada um com preço e moeda), para registrar uma compra.

Critérios de aceite:
- [ ] Data e Comprador são obrigatórios; comprador deve existir.
- [ ] Pedido precisa de ao menos 1 item; lista vazia é rejeitada com 400.
- [ ] Cada item exige carne existente, preço > 0 e moeda válida (BRL/USD/EUR).
- [ ] No POST, cotação de moedas estrangeiras usadas é buscada na AwesomeAPI e
      persistida junto ao pedido/itens.
- [ ] Se a AwesomeAPI estiver indisponível, o pedido NÃO é criado e a API
      retorna 422 com mensagem explicando a falha de cotação (sem loading
      infinito no frontend).
- [ ] POST /pedidos retorna 201 com o pedido criado, incluindo itens e valor
      total em Real já calculado.

### US10 - Listar pedidos com valor total em Real (Must)
Como usuário, quero ver a listagem de pedidos com Id, comprador e valor total
em Real, para ter visão consolidada das compras.

Critérios de aceite:
- [ ] GET /pedidos retorna carnes e comprador associados a cada pedido.
- [ ] Valor total em Real usa a cotação persistida no momento da criação (não
      chama a AwesomeAPI novamente na listagem).
- [ ] Falha ou indisponibilidade da AwesomeAPI não afeta a listagem, já que a
      cotação já está salva.

### US11 - Editar pedido (Must)
Como usuário, quero editar um pedido existente (data, comprador ou itens),
para corrigir informações antes de finalizado.

Critérios de aceite:
- [ ] PUT /pedidos/{id} permite alterar data, comprador e itens.
- [ ] Se itens/preço/moeda mudarem, cotação é buscada novamente na AwesomeAPI;
      se indisponível, PUT retorna 422 e a edição não é aplicada (estado
      anterior preservado).
- [ ] Id inexistente retorna 404.

### US12 - Excluir pedido (Must)
Como usuário, quero excluir um pedido, para remover um registro incorreto ou
não confirmado.

Critérios de aceite:
- [ ] DELETE /pedidos/{id} remove o pedido e seus itens (cascade).
- [ ] Retorna 204 em caso de sucesso e 404 se o Id não existir.

### US13 - Filtro de pedidos por comprador/data (Could)
Como usuário, quero filtrar a listagem de pedidos por comprador e/ou período
de data, para localizar pedidos específicos rapidamente.

Critérios de aceite:
- [ ] GET /pedidos aceita query params opcionais de compradorId e/ou
      dataInicio/dataFim.
- [ ] Filtros combinados (comprador + data) funcionam em conjunto (AND).
- [ ] Sem filtros, retorna todos os pedidos (comportamento atual preservado).

## Cross-cutting (Frontend/UX)

### US14 - Feedback visual de sucesso/erro (Must)
Como usuário, quero receber feedback visual claro após qualquer operação de
escrita (criar, editar, excluir), para saber se a ação funcionou.

Critérios de aceite:
- [ ] Toda operação de sucesso exibe confirmação visual (ex.: toast/alerta).
- [ ] Toda operação com erro exibe a mensagem retornada pela API (não um erro
      genérico tipo "algo deu errado"), incluindo o caso de 422 por falha de
      cotação.

### US15 - Modal de confirmação de exclusão (Must)
Como usuário, quero confirmar antes de excluir qualquer registro (carne,
comprador ou pedido), para evitar exclusões acidentais.

Critérios de aceite:
- [ ] Clicar em excluir abre modal de confirmação antes de disparar o DELETE.
- [ ] Cancelar no modal não dispara nenhuma requisição.
- [ ] Confirmar dispara o DELETE e fecha o modal ao concluir.

## Banco de Dados

### US16 - Script SQL com schema e seed de cidades/estados (Must)
Como responsável por subir o ambiente, quero um script SQL único que crie o
banco, as tabelas e popule cidades/estados, para rodar o projeto localmente
sem passos manuais extras.

Critérios de aceite:
- [ ] Script cria o banco, as tabelas (Carne, Comprador, Pedido, PedidoItem,
      Estado, Cidade) e as FKs/PKs entre elas.
- [ ] Script inclui inserts iniciais de Estados e Cidades (relação 1:N).
- [ ] Script é idempotente ou claramente documentado como execução única
      (README explica como rodar).
