# Tasks - Cadastro de Carnes, Compradores e Pedidos

Referência: [docs/PRD.md](docs/PRD.md), [docs/BACKLOG.md](docs/BACKLOG.md).
Ordem de camada por recurso: Model → Data → Service → Controller.

## Backend — Model

- [x] T01 Criar enums de domínio `OrigemCarne` (Bovina, Suína, Aves,
      Peixes) e `Moeda` (BRL, USD, EUR) no projeto Model.
      Pronto quando: enums compilam, sem dependência de EF Core.
      Depende de: —

- [x] T02 Criar entidades de apoio `Estado` e `Cidade` (1:N) no
      projeto Model.
      Pronto quando: `Estado` tem coleção de `Cidade`; `Cidade` referencia
      `EstadoId`; compilam sem EF Core.
      Depende de: —

- [x] T03 Criar entidade `Carne` no projeto Model (Descrição,
      Origem).
      Pronto quando: compila; Origem é do tipo enum `OrigemCarne` (T01).
      Depende de: T01 (validada)

- [x] T04 Criar entidade `Comprador` no projeto Model (Nome,
      Documento, referência a Cidade/Estado).
      Pronto quando: compila; `Comprador` referencia `CidadeId`.
      Depende de: T02 (validada)

- [x] T05 Criar entidades `Pedido` e `PedidoItem` no projeto
      Model (Pedido: Data, Comprador, coleção de itens; PedidoItem: Carne,
      Preço, Moeda, cotação persistida).
      Pronto quando: `PedidoItem` tem campo de cotação (ex: `CotacaoUsada`)
      além de Preço/Moeda; `Pedido` expõe valor total calculável a partir dos
      itens; compilam sem EF Core.
      Depende de: T01 (validada), T03 (validada), T04 (validada)

## Backend — Data

- [x] T06 Configurar `DbContext` com todos os `DbSet`s e Fluent
      API (relacionamentos, FKs, restrição de cascade onde aplicável) no
      projeto Data.
      Pronto quando: `dotnet build` do projeto Data passa sem erro; FK
      Pedido→Comprador, PedidoItem→Pedido/Carne, Cidade→Estado configuradas.
      Depende de: T01-T05 (validadas)

- [x] T07 Criar migration inicial e seed de Estado/Cidade via
      `HasData` ou seed programático.
      Pronto quando: `dotnet ef migrations add Initial` roda sem erro; banco
      criado localmente contém Estados e Cidades populados.
      Depende de: T06 (validada)

- [x] T08 Implementar Repositório genérico + Unit of Work no
      projeto Data.
      Pronto quando: interface genérica cobre GetAll/GetById/Add/Update/Delete
      + `SaveChangesAsync` via UoW; nenhum Service ainda depende disso (task
      isolada de infraestrutura).
      Depende de: T06 (validada)

- [x] T09 Gerar script SQL standalone (schema + FKs/PKs + seed
      de Estado/Cidade) a partir das migrations, para o critério US16 do
      backlog.
      Pronto quando: `dotnet ef migrations script` gerado e salvo em
      `docs/database/schema.sql` (ou local equivalente); README (T33) referencia
      esse arquivo como forma de subir o banco sem `dotnet ef`.
      Depende de: T07 (validada)

## Backend — Service

- [x] T10 Implementar cliente da AwesomeAPI (cotação
      USD→BRL e EUR→BRL) como serviço injetável com interface própria
      (`ICotacaoService` ou similar).
      Pronto quando: interface abstrai a chamada HTTP; timeout/erro de rede
      lança exceção de domínio específica (não deixa exception genérica
      subir); tem teste unitário com mock do `HttpClient`/`HttpMessageHandler`
      simulando sucesso e indisponibilidade.
      Depende de: T08 (validada)

- [x] T11 Service de Carne: CRUD + regra de bloqueio de delete
      quando existir `PedidoItem` vinculado.
      Pronto quando: teste unitário xUnit cobre (a) delete bloqueado com
      PedidoItem vinculado, (b) delete permitido sem vínculo, (c) validação de
      descrição obrigatória.
      Depende de: T08 (validada)

- [x] T12 Service de Comprador: CRUD + regra de bloqueio de
      delete quando existir `Pedido` vinculado + validação de Cidade/Estado
      existente.
      Pronto quando: teste unitário cobre (a) delete bloqueado com pedido
      vinculado, (b) delete permitido sem vínculo, (c) rejeição de
      combinação Cidade/Estado inexistente.
      Depende de: T08 (validada)

- [x] T13 Service de Pedido — criação (POST): validação de
      itens (mínimo 1, preço > 0, moeda válida, carne/comprador existentes),
      busca de cotação via T10 para moedas estrangeiras usadas, cálculo e
      persistência do valor total em Real.
      Pronto quando: teste unitário cobre (a) sucesso com item BRL e item
      USD/EUR, (b) rejeição de lista de itens vazia, (c) erro de domínio
      quando a cotação falha (sem criar pedido parcial).
      Depende de: T08 (validada), T10 (validada), T05 (validada)

- [x] T14 Service de Pedido — edição (PUT): recotização
      condicional (só busca cotação nova se itens/preço/moeda mudarem) com a
      mesma regra de falha do POST.
      Pronto quando: teste unitário cobre (a) edição só de data/comprador não
      chama a cotação novamente, (b) edição de item dispara nova cotação,
      (c) falha de cotação não aplica a edição (estado anterior preservado).
      Depende de: T13 (validada)

- [x] T15 Service de Estado/Cidade (somente leitura, para
      alimentar combobox do frontend).
      Pronto quando: retorna Estados com suas Cidades (ou endpoint de Cidades
      filtrado por Estado).
      Depende de: T08 (validada)

## Backend — Controller

- [x] T16 DTOs de Carne (Create/Update sem Id editável,
      Response) + `CarneController` (GET, GET/{id}, POST, PUT, DELETE).
      Pronto quando: POST retorna 201, PUT em Id inexistente retorna 404,
      DELETE bloqueado retorna 409, GET lista vazia retorna 200 com array
      vazio.
      Depende de: T11 (validada)

- [x] T17 DTOs de Comprador (Create/Update sem Id editável,
      Response) + `CompradorController`.
      Pronto quando: mesmos critérios de status code de T16 aplicados a
      Comprador; DELETE bloqueado por pedido vinculado retorna 409.
      Depende de: T12 (validada)

- [x] T18 DTOs de Estado/Cidade (somente leitura) +
      `EstadoController`/`CidadeController` (GET apenas, para popular
      combobox).
      Pronto quando: GET /estados retorna Estados com Cidades (ou GET
      /cidades?estadoId= filtra corretamente).
      Depende de: T15 (validada)

- [x] T19 DTOs de Pedido/PedidoItem (Create/Update sem Id
      editável, Response com valor total em Real) + `PedidoController` (GET,
      GET/{id}, POST, PUT, DELETE).
      Pronto quando: POST com AwesomeAPI indisponível retorna 422 com
      mensagem clara (mock do serviço de cotação simulando falha); GET lista
      usa cotação persistida (não chama AwesomeAPI de novo — verificável por
      não haver dependência de `ICotacaoService` no fluxo de leitura); DELETE
      retorna 204 e remove itens em cascade.
      Depende de: T13 (validada), T14 (validada)

- [x] T20 Middleware global de tratamento de exceções +
      configuração de CORS restrito a `http://localhost:5173` no `Program.cs`.
      Pronto quando: exception não tratada retorna resposta genérica (sem
      stack trace/mensagem de exception crua) em qualquer ambiente; CORS não
      usa `AllowAnyOrigin()`.
      Depende de: T16, T17, T18, T19 (validadas)

- [x] T21 Testes de integração `WebApplicationFactory` para os
      4 controllers, cobrindo os status codes principais (200/201/404/422/409)
      de cada um.
      Pronto quando: suíte de integração roda via `dotnet test` e passa,
      usando banco em memória ou SQLite in-memory (não o SQL Server de
      desenvolvimento).
      Depende de: T20 (validada)

## Frontend

- [x] T22 Setup do projeto (Vite + TypeScript, TanStack
      Query, cliente HTTP centralizado, roteamento básico entre as 3 telas).
      Pronto quando: projeto sobe com `npm run dev`; cliente HTTP central
      aponta para a base URL da API; nenhuma chamada `fetch`/`axios` direta
      ainda espalhada (esqueleto vazio).
      Depende de: —

- [x] T23 Tela de Carnes: listagem + formulário CRUD + modal
      de confirmação de exclusão.
      Pronto quando: form controlado valida descrição obrigatória e origem
      dentre as 4 opções; exclusão passa por modal; mensagem de erro do
      backend (409) é exibida ao usuário, não um erro genérico; teste
      Vitest/RTL cobre validação de campo obrigatório e comportamento do
      modal (cancelar não dispara request).
      Depende de: T16 (validada), T22 (validada)

- [x] T24 Tela de Compradores: listagem + formulário CRUD com
      combobox de Cidade/Estado (populado via T18) + modal de confirmação.
      Pronto quando: combobox de Estado filtra Cidade corretamente; mesmos
      critérios de validação/modal/erro de T23 aplicados a Comprador; teste
      Vitest/RTL cobre validação e modal.
      Depende de: T17 (validada), T18 (validada), T22 (validada)

- [x] T25 Tela de Pedidos — listagem: Id, comprador, valor
      total em Real, modal de confirmação de exclusão.
      Pronto quando: lista consome GET /pedidos sem recalcular cotação no
      frontend; exclusão passa por modal; teste Vitest/RTL cobre o modal.
      Depende de: T19 (validada), T22 (validada)

- [x] T26 Tela de Pedidos — formulário de criação/edição:
      data, comprador, itens dinâmicos (carne + preço + moeda), feedback
      explícito de erro 422 por falha de cotação.
      Pronto quando: form permite adicionar/remover itens; preço validado
      como positivo; erro 422 exibido com a mensagem da API (não texto
      genérico) e sem loading infinito; teste Vitest/RTL cobre validação de
      preço positivo e de lista de itens não vazia.
      Depende de: T25 (validada)

## Validação e testes

- [x] T27 Revisar camada Model completa (T01-T05).
      Depende de: T05

- [x] T28 Revisar camada Data completa (T06-T09): DbContext,
      migrations/seed, repositório/UoW, script SQL exportado.
      Depende de: T09

- [x] T29 Revisar camada Service completa (T10-T15): regras de
      negócio, cobertura de teste unitário por regra.
      Depende de: T15

- [x] T30 Revisar camada Controller completa (T16-T21): DTOs,
      status codes, middleware de exceção, CORS, testes de integração.
      Depende de: T21

- [x] T31 Revisar telas de Carnes e Compradores (T23-T24).
      Depende de: T24

- [x] T32 Revisar telas de Pedidos — listagem e formulário
      (T25-T26).
      Depende de: T26

- [x] T33 README com instruções de execução local (backend,
      frontend, banco via script SQL de T09) e decisões assumidas do PRD
      (incluindo ausência de autenticação como decisão de escopo).
      Depende de: T30 (validada), T32 (validada)

- [x] T34 Smoke test de ponta a ponta: roda `dotnet test` e
      `vitest run` primeiro (para na hora se algo estiver quebrado), depois
      exercita manualmente os fluxos de Carne, Comprador e Pedido
      multi-moeda, incluindo os bloqueios de delete e o erro 422 de cotação
      indisponível.
      Depende de: T33

## Could (fora do caminho crítico Must)

- [x] T35 Filtro de listagem de pedidos por `compradorId`
      e/ou intervalo de data (query params opcionais, combináveis).
      Pronto quando: GET /pedidos sem filtros mantém comportamento atual;
      filtros combinados aplicam AND.
      Depende de: T19 (validada) — pode ser feito em paralelo com T22-T26,
      não bloqueia nenhuma outra task.

- [x] T36 UI de filtro por comprador/data na tela de
      listagem de Pedidos (T25).
      Depende de: T35 (validada), T25 (validada)

## Reforço de cobertura — revisão sênior de risco

Origem: revisão de coverage pós-T34 identificou que % de linha bruto não é o
critério certo — o que importa é cobertura de branch nos pontos de risco de
negócio/segurança abaixo, hoje abaixo de 90%. Não é feature nova: são testes
adicionais sobre camadas já implementadas e validadas (T11, T12, T13/T14, T20).

- [x] T37 Teste cobrindo o `ExceptionHandlingMiddleware` no
      caminho de exceção não mapeada (não uma das exceções de domínio
      conhecidas).
      Pronto quando: teste simula uma exceção genérica não tratada por
      nenhum handler específico e verifica que a resposta HTTP nunca contém
      stack trace ou mensagem de exception crua, em nenhum ambiente
      (inclusive Development); cobertura de branch do middleware sobe de
      68.7% para no mínimo 90%.
      Depende de: T21 (validada)

- [x] T38 Teste cobrindo explicitamente o caminho de falha
      da AwesomeAPI no `AwesomeApiCotacaoService` e no fluxo de criação de
      Pedido.
      Pronto quando: teste unitário com mock do `HttpMessageHandler` simula
      timeout/erro HTTP/resposta malformada da AwesomeAPI e verifica que
      `CotacaoIndisponivelException` é lançada; teste no `PedidoService`
      confirma que essa falha resulta em erro 422 e que nenhum pedido é
      persistido, nem parcialmente; cobertura sobe de 89.4% para no mínimo
      90% incluindo esse branch.
      Depende de: T13 (validada), T14 (validada)

- [x] T39 Testes adicionais de `CarneService` e
      `CompradorService` cobrindo edge cases da regra de delete bloqueado
      por vínculo.
      Pronto quando: `CarneService` — teste cobre delete bloqueado quando a
      Carne está vinculada a `PedidoItem` de um Pedido já existente (além do
      caso simples já coberto); `CompradorService` — teste cobre delete
      bloqueado quando o Comprador tem ao menos um Pedido vinculado,
      incluindo Pedido com múltiplos itens; cobertura de ambos os services
      sobe de ~80% para no mínimo 90%.
      Depende de: T11 (validada), T12 (validada)

- [x] T40 Revisar os testes de reforço de cobertura (T37-T39).
      Pronto quando: confirma que os três pontos de risco atingem cobertura
      de branch >= 90% (via `dotnet test --collect:"XPlat Code Coverage"` +
      `reportgenerator`); rejeita qualquer teste novo que seja
      falso-positivo (assert genérico que passa sem exercitar de fato o
      caminho de falha/branch alvo).
      Depende de: T37, T38, T39

## Ajustes pós-teste manual (achados do usuário testando a interface)

Origem: usuário testou a aplicação manualmente e reportou 4 pontos. Não são
features novas — são correções/ajustes sobre telas já implementadas e
validadas (Carnes, Compradores, Pedidos). O 4º ponto ("layout mais
minimalista") não virou task — ver observação ao final desta seção.

- [x] T41 Corrigir colisão de `queryKey` do TanStack Query
      entre `usePedidos` (listagem) e `usePedido` (detalhe) em
      `frontend/src/hooks/usePedidos.ts`.
      Causa raiz: as duas queries usam a mesma forma de chave
      (`['pedidos', filtro]` e `['pedidos', id]`); quando não há filtro ativo
      e o formulário está em modo criação, ambas colapsam para
      `['pedidos', undefined]`, e `usePedido(undefined)` passa a expor o
      array cacheado da listagem em vez de `undefined` — isso é o que causa
      o crash relatado (`Cannot read properties of undefined (reading
      'map')` em `mapPedidoResponseToFormValues.ts:14`, chamado a partir de
      `PedidoFormPage.tsx:43`).
      Pronto quando: `usePedidos`/`usePedido` usam chaves com segmento
      distinto (ex.: `['pedidos', 'lista', filtro]` /
      `['pedidos', 'detalhe', id]`); `invalidateQueries({ queryKey:
      ['pedidos'] })` nas mutations de create/update/delete continua
      invalidando as duas formas (teste cobrindo isso); teste
      novo/regressão reproduz o cenário relatado (visitar
      `/pedidos` sem filtro, depois abrir `/pedidos/novo`) e confirma que o
      formulário abre vazio, sem lançar exceção.
      Depende de: — (bug isolado em código já validado, T25/T26)

- [x] T42 Limpar o formulário de Carne após cadastro
      bem-sucedido (não após edição, que já sai do modo de edição).
      Causa raiz: `CarneForm` só reresseta estado interno via `useEffect`
      disparado por mudança de referência de `initialValues`; no fluxo de
      criação, `carneEmEdicao` continua `null` antes e depois do submit, e
      `CARNE_FORM_INICIAL` é a mesma referência de objeto, então o efeito
      nunca dispara e os campos preenchidos ficam na tela após salvar.
      Pronto quando: após `criarCarne.mutate` resolver com sucesso, o
      formulário volta a exibir os campos vazios (descrição em branco,
      origem sem seleção); teste Vitest/RTL cobre isso (preencher, submeter,
      confirmar campos vazios após o toast de sucesso); fluxo de edição
      (`atualizarCarne`) não é afetado — continua fechando o modo de edição
      como hoje.
      Depende de: — (bug isolado em código já validado, T23)

- [x] T43 Limpar o formulário de Comprador após cadastro
      bem-sucedido (mesma causa raiz e mesmo critério de pronto da T42,
      aplicado a `CompradorForm`/`CompradoresPage`).
      Depende de: — (bug isolado em código já validado, T24)

- [x] T44 Separar a tela de Carnes em abas "Consultar" e
      "Cadastrar" (hoje formulário e tabela ficam empilhados na mesma
      página, sem nenhuma navegação por aba).
      Premissa assumida (registrar no README se for implementada): esta
      mudança se aplica só a Carnes e Compradores, que hoje não têm nenhuma
      separação entre consulta e cadastro. Pedidos já tem separação
      equivalente por rota (`/pedidos` lista, `/pedidos/novo` cadastro) —
      fora de escopo desta task, não mexer no padrão de Pedidos a menos que
      o usuário confirme que quer unificar tudo em abas.
      Pronto quando: a tela de Carnes tem duas abas navegáveis — "Consultar"
      (mostra `CarneTable`) e "Cadastrar" (mostra `CarneForm`); trocar de aba
      não perde estado de edição em andamento de forma inesperada (ex.:
      clicar "Editar" na aba Consultar leva para a aba Cadastrar já
      preenchida); teste Vitest/RTL cobre a navegação entre abas e o fluxo
      de editar-a-partir-da-tabela.
      Depende de: T42 (validada) — evita reter estado de formulário sujo ao
      trocar de aba.

- [x] T45 Mesma separação em abas da T44, aplicada à tela
      de Compradores.
      Depende de: T43 (validada), T44 (validada) — reaproveitar o
      componente/padrão de abas criado em T44 em vez de duplicar.

- [x] T46 Revisar os ajustes pós-teste manual (T41-T45): confirma
      o fix da colisão de queryKey com teste de regressão real (não
      passa por acidente), confirma que o reset de formulário não quebra o
      fluxo de edição, e confirma que a navegação por abas não introduz
      regressão nos testes já existentes de Carnes/Compradores (T23/T24).
      Depende de: T41, T42, T43, T44, T45

**Observação sobre o 4º ponto reportado ("layout mais minimalista"):** não
virou task. Não tem critério de pronto objetivo — o projeto já tem um design
system mínimo (`frontend/src/index.css`: variáveis de cor, navbar, largura
máxima de conteúdo), e o pedido não especifica o que hoje não está
minimalista nem traz uma referência do que seria suficiente. Antes de virar
task, precisa de uma referência concreta (mockup, screenshot de exemplo, ou
lista específica do que remover/simplificar) — do contrário qualquer
implementação vira palpite sem como o `validator` checar objetivamente se
está "pronto".

## Troca da fonte de cotação: BCB como primária, AwesomeAPI como fallback

Origem: usuário relatou instabilidade da AwesomeAPI ao testar a criação de
pedido (não conseguiu concluir o cadastro). Decisão: usar a API do Banco
Central (BCB, serviço PTAX/Olinda) como fonte primária de cotação, mantendo a
AwesomeAPI como fallback quando o BCB falhar ou não tiver cotação disponível
para o dia. `ICotacaoService.ObterCotacoesAsync` (interface consumida por
`PedidoService`, T13/T14) não muda de contrato — isso é troca de
implementação, não redesenho de negócio.

Contrato do BCB já verificado ao vivo antes de escrever estas tasks (não
precisa reinvestigar): endpoint único, mesma forma de resposta pra USD e EUR
—
`https://olinda.bcb.gov.br/olinda/servico/PTAX/versao/v1/odata/CotacaoMoedaDia(moeda=@moeda,dataCotacao=@dataCotacao)?@moeda='USD'&@dataCotacao='MM-DD-YYYY'&$format=json`
(trocar `'USD'` por `'EUR'` pra outra moeda). Resposta: `{"value": [...]}`,
um array com várias cotações do dia (`tipoBoletim`: "Abertura",
"Intermediário" x N, "Fechamento PTAX") — usar a de `tipoBoletim ==
"Fechamento PTAX"` e o campo `cotacaoCompra`. Em dia sem pregão (fim de
semana/feriado), `value` vem vazio (`[]`) — confirmado testando um sábado;
esse é o gatilho natural pro fallback. Não existe endpoint em lote (1
chamada HTTP por moeda, diferente da AwesomeAPI que resolvia USD+EUR numa
chamada só) — aceito como trade-off, não é bloqueante nem vira task própria.

- [x] T47 Implementar `BcbCotacaoService : ICotacaoService`
      (novo arquivo em `backend/Prova.Service/Cotacao/`) consultando
      `CotacaoMoedaDia` do BCB com a data de hoje, para cada moeda
      estrangeira solicitada.
      Pronto quando: usa a entrada de `tipoBoletim == "Fechamento PTAX"`
      (ou, na ausência dela, a de `dataHoraCotacao` mais recente do array)
      como cotação do dia, lendo `cotacaoCompra`; lança
      `CotacaoIndisponivelException` (já existe, reaproveitar) quando
      `value` vem vazio, quando a chamada HTTP falha/expira, ou quando o
      JSON vem em formato inesperado; teste unitário com mock de
      `HttpMessageHandler` cobre sucesso (USD e EUR), `value` vazio (dia
      sem pregão), erro HTTP, timeout e JSON malformado — mesmo padrão de
      `AwesomeApiCotacaoServiceTests.cs`.
      Depende de: — (classe nova e isolada; `CotacaoIndisponivelException`
      já existe e não muda)

- [x] T48 Implementar `CotacaoServiceComFallback :
      ICotacaoService` (decorator) que tenta `BcbCotacaoService` primeiro e
      cai para `AwesomeApiCotacaoService` se a primária lançar
      `CotacaoIndisponivelException`.
      Pronto quando: teste unitário com mocks das duas dependências (via
      `ICotacaoService` injetado duas vezes, uma pra cada papel) cobre (a)
      BCB responde com sucesso → AwesomeAPI nunca é chamada; (b) BCB lança
      `CotacaoIndisponivelException` → AwesomeAPI é chamada e seu resultado
      é retornado; (c) as duas lançam `CotacaoIndisponivelException` →
      exceção é propagada com mensagem clara (não mascarada/genérica); esta
      é a ÚNICA classe do projeto que conhece a existência do fallback —
      nenhuma outra classe (incluindo `PedidoService`) referencia
      `BcbCotacaoService`/`AwesomeApiCotacaoService` diretamente.
      Depende de: T47 (validada)

- [x] T49 Registrar os dois HttpClients tipados concretos e
      o decorator de fallback como `ICotacaoService` no `Program.cs`.
      Pronto quando: `AddHttpClient<BcbCotacaoService>()` e
      `AddHttpClient<AwesomeApiCotacaoService>()` registrados como tipos
      concretos (não mais como `ICotacaoService` diretamente);
      `ICotacaoService` resolve para `CotacaoServiceComFallback` via DI;
      `dotnet build` sem erro; suíte completa (unitários + integração)
      continua passando **sem nenhuma alteração** em
      `PedidoServiceCriarTests.cs`, `PedidoServiceAtualizarTests.cs` ou
      `PedidoControllerTests.cs` (critério de não regressão explícito —
      esses arquivos de teste já mockam/fake `ICotacaoService` e não devem
      precisar mudar, já que o contrato da interface não mudou).
      Depende de: T48 (validada)

- [x] T50 Atualizar `README.md` (seção
      "Decisões assumidas") documentando: BCB como fonte primária de
      cotação, AwesomeAPI como fallback (motivo: instabilidade relatada),
      uso do campo `cotacaoCompra`, e a limitação de 1 chamada HTTP por
      moeda no BCB (sem endpoint em lote, diferente da AwesomeAPI).
      Depende de: T49 (validada)

- [x] T51 Revisar T47-T50.
      Pronto quando: confirma que `ICotacaoService.ObterCotacoesAsync` não
      mudou de assinatura/contrato; confirma que o fallback só ativa em
      `CotacaoIndisponivelException` (não engole silenciosamente outros
      tipos de exceção); confirma, lendo os arquivos, que nenhum teste
      pré-existente de `PedidoService`/`PedidoController` foi alterado;
      roda a suíte completa (`dotnet test`) e confirma que passa; confirma
      que a documentação (T50) foi atualizada de fato.
      Depende de: T47, T48, T49, T50

## Resiliência do BCB: buscar último dia útil antes do fallback

Origem: em produção, `dotnet run` real expôs dupla falha simultânea — BCB sem
cotação de hoje (fim de semana, comportamento esperado) e AwesomeAPI com
`429 Too Many Requests` no fallback, resultando em 422 pro cliente sem
conseguir criar o pedido. Decisão do usuário: antes de escalar pro fallback
AwesomeAPI, `BcbCotacaoService` deve primeiro tentar o último dia útil
disponível. Não é feature nova — refinamento sobre T47 (já implementada e
validada). `ICotacaoService`/`CotacaoServiceComFallback` (T48) não mudam.

Contrato do BCB já verificado ao vivo (não precisa reinvestigar):
- Endpoint de intervalo, evita loop dia-a-dia:
  `https://olinda.bcb.gov.br/olinda/servico/PTAX/versao/v1/odata/CotacaoMoedaPeriodo(moeda=@moeda,dataInicial=@dataInicial,dataFinalCotacao=@dataFinalCotacao)?@moeda='USD'&@dataInicial='MM-DD-YYYY'&@dataFinalCotacao='MM-DD-YYYY'&$format=json`.
- **Achado importante, confirmado testando os dois endpoints pra mesma data
  (2024-01-15, USD)**: a entrada de fechamento do dia vem com
  `tipoBoletim == "Fechamento PTAX"` no endpoint `CotacaoMoedaDia` (usado em
  T47), mas como `tipoBoletim == "Fechamento"` (SEM "PTAX") no endpoint
  `CotacaoMoedaPeriodo` — mesma cotação, mesma data, rótulo diferente
  dependendo do endpoint. Não depender do texto exato de `tipoBoletim` pra
  identificar o fechamento — usar a entrada de `dataHoraCotacao` mais
  tardia do dia mais recente presente no array (T47 já tinha essa lógica
  como fallback defensivo; aqui vira a estratégia principal).
- Dias sem pregão simplesmente não aparecem no array de resposta (confirmado
  testando um intervalo cobrindo um fim de semana). `value` vazio no
  intervalo inteiro só ocorreria num feriadão maior que a janela escolhida.

- [x] T52 `BcbCotacaoService` busca o último dia útil
      disponível (janela de 7 dias corridos) antes de lançar
      `CotacaoIndisponivelException`.
      Pronto quando: mantém a chamada a `CotacaoMoedaDia` de hoje como
      primeira tentativa (código já validado de T47, não retrabalhar); se
      vier vazia, tenta `CotacaoMoedaPeriodo` com janela de 7 dias corridos
      pra trás, usando a entrada de `dataHoraCotacao` mais tardia do dia
      mais recente presente no array (sem depender do texto de
      `tipoBoletim`); se a janela inteira vier vazia, lança
      `CotacaoIndisponivelException` (aciona o fallback AwesomeAPI via
      `CotacaoServiceComFallback`, que não muda); erros técnicos
      (timeout/HTTP/JSON malformado) em qualquer uma das duas chamadas
      continuam lançando `CotacaoIndisponivelException` sem mudança de
      comportamento; teste cobre (a) sucesso direto hoje sem precisar da
      janela (não regressão do caminho feliz de T47), (b) hoje vazio + dia
      útil anterior disponível na janela → sucesso sem precisar do
      fallback AwesomeAPI, (c) janela inteira vazia → exceção lançada.
      Depende de: T47 (validada)

- [x] T53 Atualizar `README.md` (seção
      "Decisões assumidas") documentando o refinamento: "cotação do dia" na
      prática busca o último dia útil disponível em até 7 dias corridos, não
      estritamente hoje — motivado pelo cenário real de fim de semana +
      rate limit do fallback relatado pelo usuário.
      Depende de: T52 (validada)

- [x] T54 Revisar T52-T53.
      Pronto quando: confirma que `PedidoServiceCriarTests.cs`,
      `PedidoServiceAtualizarTests.cs`, `PedidoControllerTests.cs` e
      `CotacaoServiceComFallbackTests.cs` não foram alterados; confirma
      teste do cenário exato relatado e do cenário de janela vazia; roda a
      suíte completa; confirma documentação atualizada.
      Depende de: T52, T53

## Refresh de UI + Dashboard (a partir de `docs/Interface.txt`)

Origem: usuário trouxe referência visual (paleta, tipografia, layout) e
especificação de uma tela nova de Dashboard com métricas. Duas frentes
independentes na origem, mas com dependência de execução (Dashboard usa os
componentes visuais do refresh, não o estilo antigo).

Decisões de negócio já esclarecidas com o usuário (não reabrir):
- Rankings ("Top 5 carnes"/"Top 5 compradores") são por **valor em R$**
  (soma de `PedidoItem.ValorEmReal`/`Pedido.ValorTotalEmReal`), não por
  quantidade — `PedidoItem` não tem campo de peso/quantidade.
- Os cards de topo usam um **seletor** de período (hoje/semana/mês), não
  período fixo. Trocar o período recalcula os cards de topo E os rankings
  Top 5 juntos.

### Frente A — Design system (Carnes/Compradores/Pedidos já existentes)

- [x] T55 Tokens de cor/tipografia em
      `frontend/src/index.css`: paleta de 4 cores (`#FAFAFA` fundo,
      `#1A1A1A` texto principal, `#6B6B6B` texto secundário, `#A63D2F`
      accent, `#E5E5E5` borda), fonte Inter ou Manrope, preços com
      `font-variant-numeric: tabular-nums`, border-radius consistente
      (6-8px) em variável reutilizável.
      Pronto quando: variáveis CSS atualizadas com os valores exatos acima;
      fonte carregada (Google Fonts ou self-hosted — decidir) aplicada
      globalmente; `tabular-nums` aplicado nas colunas de preço/valor das
      tabelas já existentes (`CarneTable`? não tem preço — aplicar em
      `PedidoTable`/valores monetários); suíte de testes do frontend
      continua passando (mudança é só visual, nenhum teste deveria
      depender de cor/fonte).
      Depende de: —

- [x] T56 Componente de Sidebar substituindo o `NavBar`
      atual (hoje uma barra superior, `frontend/src/components/NavBar.tsx`).
      Pronto quando: novo componente de navegação vertical fina à esquerda
      com os mesmos links (Carnes/Compradores/Pedidos) + item novo
      "Dashboard" apontando pra `/dashboard` (rota criada em T65); `App.tsx`
      usa o novo layout (sidebar + área de conteúdo com padding generoso)
      no lugar do `NavBar`; `NavBar.tsx` removido se não for mais usado em
      lugar nenhum (sem deixar código morto); nenhum teste existente que
      dependa de navegação quebra.
      Depende de: T55 (validada)

- [x] T57 Ajustar Cards e Tabelas das 3 telas existentes
      pro novo padrão: borda fina de 1px em vez de sombra, zebra sutil nas
      linhas de tabela, radius consistente (reaproveitar a variável de T55).
      Pronto quando: `CarneTable`/`CompradorTable`/`PedidoTable` com zebra
      striping sutil; qualquer elemento tipo "card" (ex: painel de
      formulário na aba Cadastrar) usa borda fina, não sombra pesada;
      radius aplicado de forma consistente nas 3 telas; testes RTL
      existentes continuam passando.
      Depende de: T55 (validada), T56 (validada)

- [x] T58 Revisar frente A — design system (T55-T57).
      Pronto quando: confirma paleta/tipografia/radius conforme
      `docs/Interface.txt`; confirma sidebar funcional substituindo o
      NavBar sem quebrar navegação; confirma nenhuma regressão nos testes
      de Carnes/Compradores/Pedidos.
      Depende de: T55, T56, T57

### Frente B — Dashboard (feature nova, endpoint de agregação não existe hoje)

- [x] T59 Service de Dashboard — métricas de topo (pedidos
      no período, faturamento + ticket médio, compradores ativos vs.
      cadastrados), por período `hoje`/`semana`/`mes`.
      Pronto quando: novo DTO de resposta com os campos; `IDashboardService`/
      `DashboardService` no projeto Service calcula os 3 números a partir
      dos repositórios de Pedido/Comprador já existentes, convertendo o
      parâmetro de período num intervalo de datas; ticket médio =
      faturamento ÷ total de pedidos, com guarda explícita pra total = 0
      (retorna 0, não lança/divide por zero); teste unitário cobre os 3
      períodos e o caso de zero pedidos no período.
      Depende de: — (usa `IUnitOfWork`/repositórios já existentes, sem
      mudança de schema)

- [x] T60 Service de Dashboard — Top 5 carnes e Top 5
      compradores por valor em R$, no mesmo período.
      Pronto quando: DTO com as duas listas (nome + valor total cada);
      agrega `PedidoItem.ValorEmReal` por Carne e `Pedido.ValorTotalEmReal`
      por Comprador, ordenado desc, limitado a 5, respeitando o período;
      teste unitário cobre: mais de 5 carnes/compradores (só retorna top
      5), empate de valor (desempate determinístico, ex: por nome), período
      sem nenhum pedido (lista vazia, não erro).
      Depende de: T59 (validada) — reaproveita o parsing de período de lá.

- [x] T61 Service de Dashboard — série de faturamento por
      dia dos últimos N dias corridos (parâmetro, ex: 7 ou 30).
      Pronto quando: retorna lista de {data, faturamento} para os últimos N
      dias a partir de hoje; dias sem pedido aparecem com faturamento 0
      (não ficam ausentes da lista — importante pro gráfico de linha não
      ter buraco); teste unitário cobre N=7, N=30, e dia sem pedido
      aparecendo como 0.
      Depende de: — (independente de T59/T60)

- [x] T62 `DashboardController` expondo os endpoints.
      Pronto quando: `GET /api/dashboard?periodo=hoje|semana|mes` retorna o
      DTO combinado de T59+T60; `GET /api/dashboard/faturamento-por-dia?dias=7`
      (aceita também `dias=30`) retorna a série de T61; parâmetro de
      período inválido retorna 400 com mensagem clara (mesmo padrão de
      validação já usado no projeto); teste de integração
      (`WebApplicationFactory`) cobre os status codes principais.
      Depende de: T59 (validada), T60 (validada), T61 (validada)

- [x] T63 Revisar frente B — backend (T59-T62).
      Pronto quando: confirma que os cálculos batem manualmente contra um
      cenário de teste conhecido (ex: 2 pedidos com valores conhecidos →
      soma/ticket médio/top5 corretos); confirma guarda de divisão por
      zero; confirma que nenhum endpoint/teste existente de
      Pedido/Carne/Comprador foi afetado.
      Depende de: T59, T60, T61, T62

- [x] T64 Adicionar Recharts como dependência (biblioteca
      de gráfico — decisão técnica assumida, projeto não tinha nenhuma) e
      criar hooks TanStack Query consumindo os endpoints de T62.
      Pronto quando: `recharts` no `frontend/package.json`; hooks (ex:
      `useDashboardResumo(periodo)`, `useFaturamentoPorDia(dias)`)
      consumindo os endpoints; tipos TypeScript espelhando os DTOs do
      backend, mesma convenção de `api/types.ts` já usada no projeto.
      Depende de: T63 (validada)

- [x] T65 Tela de Dashboard — cards de métrica + seletor
      de período.
      Pronto quando: rota nova `/dashboard`; 3 cards de métrica usando o
      componente visual de Card da frente A (T57); seletor
      hoje/semana/mês que recalcula os 3 cards ao trocar; teste Vitest/RTL
      cobre a troca de período disparando novo fetch com o parâmetro certo.
      Depende de: T64 (validada), T57 (validada) — usa o Card já
      estilizado, não implementa com estilo antigo pra retrabalhar depois.

- [x] T66 Tela de Dashboard — Top 5 carnes, Top 5
      compradores, gráfico de linha de faturamento.
      Pronto quando: as duas listas Top 5 (com o padrão visual da frente A)
      recalculam junto com o seletor de período de T65; gráfico de linha
      (Recharts) mostra faturamento por dia dos últimos 7 ou 30 dias;
      teste Vitest/RTL cobre a renderização das listas com dado mockado.
      Depende de: T65 (validada)

- [x] T67 Revisar frente B — frontend (T64-T66).
      Pronto quando: confirma que a tela de Dashboard usa os componentes
      visuais da frente A (não implementação paralela com estilo antigo);
      confirma que trocar o período recalcula cards + rankings juntos;
      confirma testes cobrindo os cenários acima; confirma que a sidebar
      (T56) não quebrou navegação pras telas existentes.
      Depende de: T64, T65, T66

- [x] T68 Smoke test do Dashboard com dados reais.
      Pronto quando: roda a suíte automatizada completa (backend + frontend)
      primeiro; sobe a aplicação, cria/reaproveita Pedidos de teste,
      navega pro Dashboard, confirma visualmente que os números dos
      cards/rankings/gráfico batem com o que foi criado, em pelo menos 2
      períodos diferentes do seletor.
      Depende de: T58 (validada), T63 (validada), T67 (validada)

## Reversão: AwesomeAPI volta a ser fonte primária de cotação

Origem: decisão explícita do usuário, confirmada antes de gerar estas tasks
— inverte T47-T54. `CotacaoServiceComFallback` já foi desenhado agnóstico
sobre qual das duas implementações é "primária"/"fallback" (recebe as duas
via construtor), então a inversão é só no fio de DI.

- [x] T69 Inverter o registro de DI em
      `backend/Prova.Api/Program.cs`: `CotacaoServiceComFallback` passa a
      receber `(awesomeApi, bcb)` em vez de `(bcb, awesomeApi)`.
      Pronto quando: `ICotacaoService` resolve com AwesomeAPI como
      primária e BCB (com sua janela interna de 7 dias, T52, intocada)
      como fallback; nenhuma outra classe alterada
      (`BcbCotacaoService`/`AwesomeApiCotacaoService`/
      `CotacaoServiceComFallback` continuam exatamente como estão); suíte
      completa do backend continua passando sem alteração em nenhum teste
      (o comportamento do decorator é simétrico, os testes de
      `CotacaoServiceComFallbackTests.cs` não conhecem qual é qual).
      Depende de: —

- [x] T70 Atualizar `README.md` (seção
      "Decisões assumidas") refletindo a reversão: AwesomeAPI é a fonte
      primária de novo, BCB é fallback (mantendo a janela de 7 dias como
      comportamento interno do BCB quando ele é chamado).
      Depende de: T69 (validada)

- [x] T71 Revisar T69-T70.
      Pronto quando: confirma que só `Program.cs` mudou (nenhuma classe de
      `Cotacao/` foi tocada); confirma suíte completa passando; confirma
      documentação atualizada e consistente com o código.
      Depende de: T69, T70

## Melhorias de UX (a partir de `docs/UX.txt`)

Origem: auditoria de UX trazida pelo usuário (documento tinha conteúdo
duplicado — os 9 itens abaixo já estão deduplicados). Foco: responsividade
mobile/tablet (hoje a aplicação não tem nenhuma media query) e consistência
de padrões de interação.

**Fora de escopo, não virou task:** "validação de email" no cadastro de
Comprador — `Comprador` não tem campo de email no schema
(`backend/Prova.Model/Entities/Comprador.cs`); adicionar um exigiria decisão
de negócio + migration, não é polimento de UX. Se o usuário quiser um campo
de email de verdade, isso volta pra skill `po-comercio-carnes`.

- [x] T72 Estabelecer convenção de breakpoint responsivo
      em `frontend/src/index.css` (projeto não tem nenhuma `@media` hoje).
      Pronto quando: breakpoint de 768px definido de forma centralizada
      (ex: comentário de convenção + uso consistente do valor literal em
      `@media (max-width: 768px)`, já que CSS puro não tem variável de
      media query — documentar a convenção no topo do arquivo pra não
      duplicar o número "768" sem explicação em cada task seguinte).
      Depende de: —

- [x] T73 Sidebar responsivo — hamburger menu com overlay
      em `frontend/src/components/Sidebar.tsx`.
      Pronto quando: em telas `<768px`, a sidebar não fica visível por
      padrão — vira um botão de menu (ícone hamburger) que abre a
      navegação em overlay sobre o conteúdo; em `>=768px` comportamento
      atual preservado; teste Vitest/RTL cobre abrir/fechar o menu em
      viewport simulado mobile e que os links de navegação continuam
      funcionando.
      Depende de: T72 (validada)

- [x] T74 Tabelas (`CarneTable`, `CompradorTable`,
      `PedidoTable`) — layout de cards empilhados em `<768px`, sem scroll
      horizontal forçado.
      Pronto quando: em `<768px`, cada linha da tabela renderiza como um
      card com os campos empilhados verticalmente (rótulo + valor) e as
      ações (Editar/Excluir) no rodapé do card, mesma técnica aplicada nas
      3 tabelas (CSS-only via `data-label` nas células + `display: block`
      em mobile é aceitável, ou reestruturação de componente — decida a
      técnica, mas aplique a mesma nas 3 pra não convergir em 3
      implementações diferentes); em `>=768px` tabela normal preservada;
      testes Vitest/RTL existentes das 3 telas continuam passando (não
      dependem de estrutura de `<table>` especificamente, mas confirme).
      Depende de: T72 (validada)

- [x] T75 Botão "Novo pedido" — reposicionar em mobile
      (`frontend/src/pages/pedidos/PedidosListPage.tsx`).
      Pronto quando: em `<768px`, o botão fica em posição acessível (ex:
      FAB — botão flutuante fixo no canto inferior direito — ou movido pro
      topo da lista antes da tabela, decida uma abordagem concreta e
      documente); em `>=768px` posição atual preservada.
      Depende de: T72 (validada)

- [x] T76 Feedback de carregamento reutilizável nas ações
      de criar/editar/excluir (Carnes, Compradores, Pedidos).
      Pronto quando: um padrão único (ex: variante do `<button>` que aceita
      `isLoading` e mostra spinner + fica `disabled`) é aplicado nos
      botões de submit dos 3 formulários e nos botões de confirmação de
      exclusão dos 3 `ConfirmModal`; não implementar 3 vezes do zero —
      reaproveitar um componente/hook comum; teste Vitest/RTL cobre que o
      botão fica desabilitado e mostra o estado de loading durante a
      mutation pendente em pelo menos uma das 3 telas.
      Depende de: —

- [x] T77 `CarneForm` — indicador de campo obrigatório
      (*) e autofocus no primeiro campo ao abrir o formulário.
      Pronto quando: labels de Descrição/Origem exibem `*` (labels já
      ficam acima dos campos hoje — confirmado lendo o componente, não
      precisa mexer nisso); o campo Descrição recebe foco automaticamente
      quando o formulário é montado (criação) ou reaberto (edição); teste
      Vitest/RTL confirma o autofocus.
      Depende de: —

- [x] T78 `CompradorForm` — máscara de CPF no campo
      Documento e placeholders descritivos nos campos.
      Pronto quando: campo Documento aplica máscara de CPF
      (`000.000.000-00`) enquanto o usuário digita, sem impedir colar um
      valor já formatado ou só dígitos; placeholders adicionados em
      Nome/Documento; teste Vitest/RTL cobre a máscara sendo aplicada ao
      digitar.
      Depende de: —

- [x] T79 Pedidos — feedback ao adicionar item e mensagem
      de estado vazio em "Itens do pedido"
      (`frontend/src/pages/pedidos/PedidoItensForm.tsx`).
      Pronto quando: clicar "Adicionar item" dá algum feedback visual (ex:
      scroll até o novo item, breve destaque); a seção de itens, quando
      vazia, mostra uma mensagem explícita tipo "Nenhum item adicionado
      ainda — clique em Adicionar item" em vez de ficar em branco; teste
      Vitest/RTL cobre a mensagem de estado vazio aparecendo/sumindo.
      Depende de: —

- [x] T80 Filtros de `PedidosListPage` — empilhar
      verticalmente em `<768px`.
      Pronto quando: em `<768px`, os campos de filtro (comprador,
      intervalo de data) ficam um abaixo do outro, com largura total,
      em vez de espremidos numa linha; `>=768px` preservado.
      Depende de: T72 (validada)

- [x] T81 Padronizar "Editar" em `PedidoTable.tsx` como
      `<button onClick>` navegando via `useNavigate`, em vez do `<Link>`
      atual — mesmo padrão já usado em `CarneTable`/`CompradorTable`
      (confirmado por leitura: as outras duas tabelas já usam
      `<button onClick={() => onEdit(...)}>` para Editar; só `PedidoTable`
      diverge com `<Link>`).
      Pronto quando: `PedidoTable.tsx` usa `<button>` para Editar,
      navegando programaticamente pra `/pedidos/{id}/editar`; teste
      Vitest/RTL existente de navegação por Editar continua passando (ou é
      ajustado se dependia do papel `link`).
      Depende de: —

- [x] T82 `DashboardPage` responsivo: cards empilham em
      `<1024px` (quebra em tablet, não só mobile — 3 colunas fixas não
      cabem em 768-1024px), as duas listas Top 5 empilham (uma embaixo da
      outra) em vez de lado a lado em telas menores, gráfico de
      faturamento não corta em mobile.
      Pronto quando: grid de cards vira 1 coluna em `<1024px` (ou 2 em
      tablet + 1 em mobile, critério do frontend-coder); Top 5
      carnes/compradores empilham verticalmente em `<1024px`; `FaturamentoChart`
      dentro de `ResponsiveContainer` (já usado desde T66) redimensiona
      sem cortar conteúdo em `<768px` — confirme que a altura fixa (se
      houver) não força overflow.
      Depende de: T72 (validada)

- [x] T83 `CarnesPage` — remover a aba "Cadastrar" (T44),
      trocar por botão "Novo" no topo abrindo a rota `/carnes/novo`
      (mesmo padrão de rota separada que Pedidos já usa — não modal, para
      manter consistência com o resto do app).
      Pronto quando: aba "Cadastrar" removida, componente `Tabs` (T44) não
      é mais usado em `CarnesPage` (a aba "Consultar" vira a tela
      principal, sem abas); botão "Novo" no topo leva pra `/carnes/novo`
      com o `CarneForm` vazio; "Editar" na tabela leva pra
      `/carnes/{id}/editar` com o form preenchido; testes de
      `CarnesPage.test.tsx` reescritos pra refletir o novo fluxo de
      navegação (os testes de T42/T44 que dependiam de aba deixam de
      fazer sentido e devem ser substituídos, não só remendados).
      Depende de: T72 (validada) — usa breakpoint se o botão "Novo"
      também precisar de tratamento mobile (ligado a T75).

- [x] T84 `CompradoresPage` — mesma mudança da T83
      (reverte T45), reaproveitando o padrão de rota separada criado lá.
      Depende de: T83 (validada)

- [x] T85 Revisar infraestrutura responsiva + navegação
      (T72-T76, T81).
      Pronto quando: confirma breakpoint consistente entre as tasks;
      confirma sidebar/tabelas/botão funcionam em viewport simulado
      `<768px` (via teste ou leitura de CSS); confirma padronização
      Link→Button em Pedidos sem quebrar navegação.
      Depende de: T72, T73, T74, T75, T76, T81

- [x] T86 Revisar formulários + filtros + Dashboard responsivo
      (T77-T80, T82).
      Pronto quando: confirma indicadores/máscara/autofocus/feedback
      implementados conforme critério de cada task; confirma Dashboard não
      quebra em tablet/mobile.
      Depende de: T77, T78, T79, T80, T82

- [x] T87 Revisar remoção das abas Cadastrar (T83-T84).
      Pronto quando: confirma que o fluxo novo (botão + rota separada)
      cobre os mesmos casos que a aba cobria (criar, editar, cancelar
      edição); confirma que os testes reescritos não perderam cobertura
      de regra de negócio (validação de formulário, reset após criar de
      T42/T43) só porque a navegação mudou.
      Depende de: T83, T84

- [x] T88 Smoke test de responsividade.
      **Limitação conhecida:** o `tester` não tem ferramenta de browser
      nesta sessão — não consegue redimensionar viewport nem confirmar
      visualmente. Critério de pronto ajustado: roda a suíte automatizada
      completa (backend + frontend); confirma via leitura/Grep que as
      `@media (max-width: 768px)` esperadas existem nos arquivos certos
      (Sidebar, tabelas, filtros, Dashboard); reporta explicitamente que a
      verificação visual real (redimensionar o navegador ou testar em
      device) fica pendente de confirmação manual do usuário — não afirma
      "funciona visualmente" sem ter visto.
      Depende de: T85, T86, T87
