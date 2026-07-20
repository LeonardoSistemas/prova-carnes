# PRD - Cadastro de Carnes, Compradores e Pedidos

## Objetivo de negócio
Disponibilizar uma aplicação Web para gestão do processo de compra de carnes,
permitindo cadastrar carnes e compradores e registrar pedidos que associam
ambos, com precificação por item negociada no momento do pedido e conversão
para Real via cotação externa.

## Entidades e regras de negócio

### Carne
- Descrição (texto, obrigatória) e Origem (Bovina, Suína, Aves ou Peixes).
- Origem não tem, na prova, impacto em tabela de preço ou sazonalidade — é
  apenas classificação/exibição (ver premissa abaixo).
- Não pode ser excluída se existir algum PedidoItem vinculado a ela.

### Comprador
- Nome (obrigatório) e Documento (CPF/CNPJ, sem validação de dígito verificador
  exigida pela prova — ver premissa).
- Cidade e Estado selecionados via combobox, populados a partir de tabelas de
  apoio (Estado 1:N Cidade) com dados semeados no script SQL.
- Não pode ser excluído se existir algum Pedido vinculado a ele (mesma regra de
  integridade aplicada à Carne, por consistência).

### Pedido
- Data do pedido e Comprador (obrigatórios).
- Composto por um ou mais itens (PedidoItem), entidade associativa entre
  Pedido e Carne, cada item com **Preço e Moeda próprios** (preço "spot",
  digitado livremente por item — não segue tabela de preço fixa).
- Moeda do item: Real, Dólar ou Euro.
- No momento da criação (POST /pedidos), a cotação de cada moeda estrangeira
  usada é obtida da AwesomeAPI e **persistida junto ao pedido/itens** — não é
  recalculada depois. O valor total em Real exibido na listagem usa essa
  cotação congelada, garantindo que o valor histórico do pedido não mude
  retroativamente com a flutuação do câmbio.
- Se a AwesomeAPI estiver indisponível durante o POST, o pedido não é criado:
  a API retorna 422 com mensagem clara. Não há criação parcial nem fallback
  para cotação antiga.
- Edição de pedido (PUT /pedidos/{id}): se a edição alterar itens, preço ou
  moeda, o sistema busca cotação nova no momento da atualização, seguindo a
  mesma regra de falha do POST (ver premissa abaixo).

## Premissas assumidas (quando a spec não define)

- **Sem campo de quantidade por item.** A prova só pede combobox de carne +
  preço + moeda por item do pedido — não há campo de quantidade. Assume-se que
  o preço lançado já representa o valor daquele item/lote no pedido, não um
  valor unitário multiplicado por quantidade.
- **Mesma carne pode aparecer mais de uma vez no mesmo pedido** como itens
  distintos (ex.: dois lotes com preços diferentes), já que não há regra de
  unicidade carne+pedido na especificação.
- **Pedido precisa de ao menos 1 item** para ser criado; POST com lista de
  itens vazia é rejeitado.
- **Preço é "spot"**, negociado por pedido, não vindo de tabela de preço por
  origem/carne. Não existe histórico de preço por origem nesta versão.
- **Documento (CPF/CNPJ) é um campo de texto livre**, sem validação de dígito
  verificador nem distinção de regras de negócio entre pessoa física e
  jurídica (limite de crédito, forma de pagamento etc.) — fora de escopo.
- **Delete de Comprador bloqueado por pedidos vinculados**, mesma regra da
  Carne, por consistência de integridade, ainda que a especificação só
  mencione isso explicitamente para Carne no bloco de backend.
- **Edição de pedido recotiza moeda estrangeira** apenas quando o conjunto de
  itens/preços/moedas muda; se a AwesomeAPI falhar nesse momento, o PUT também
  retorna 422 e a edição não é aplicada (mesmo comportamento do POST).
- **Cidade e Estado** vêm de tabelas de apoio com relação 1:N (Estado tem
  várias Cidades), populadas via insert inicial no script SQL — não são texto
  livre.
- Validação de dados (preço positivo, nome obrigatório) e modal de confirmação
  de exclusão, listados como "diferenciais" na prova, são tratados como
  **obrigatórios de fato**: já fazem parte das convenções de código do projeto
  e também aparecem nos critérios oficiais de avaliação (usabilidade,
  tratamento de erros).

## Fora de escopo
- Cancelamento de pedido como conceito separado de exclusão — não existe na
  prova; CRUD cobre o ciclo de vida do pedido.
- Autenticação/autorização de usuários — não mencionado na prova.
- Regras diferenciadas por tipo de documento (CPF vs. CNPJ).
- Histórico de preço por origem/carne ou tabela de preço vigente.
- Internacionalização/multi-idioma.

## Backlog priorizado (MoSCoW)

**Must**
- CRUD completo de Carne, Comprador e Pedido conforme os endpoints da prova.
- Bloqueio de exclusão de Carne e Comprador com pedidos vinculados.
- Relacionamento N:N Pedido↔Carne via PedidoItem com Preço e Moeda próprios.
- Conversão de moeda via AwesomeAPI capturada no POST/PUT de pedido, com erro
  422 claro quando a API externa está indisponível (sem loading infinito, sem
  salvar sem cotação).
- Listagem de pedidos exibindo valor total em Real, usando a cotação já
  persistida (não recalculada na leitura).
- Tabelas de apoio Cidade/Estado com dados semeados no script SQL.
- Validação de dados obrigatórios (descrição de carne, nome de comprador,
  preço positivo) na camada Service.
- Feedback visual de sucesso/erro no frontend em toda operação de escrita.
- Modal de confirmação antes de qualquer exclusão.
- Script SQL com criação de banco, tabelas, FKs/PKs e inserts iniciais.
- README com instruções de execução local (backend, frontend, banco).

**Should**
- Middleware global de tratamento de exceções no Controller (sem
  try/catch espalhado).
- Formulários controlados no frontend com validação client-side espelhando as
  regras do backend.
- Smoke test de ponta a ponta cobrindo os fluxos de Carne, Comprador e Pedido.

**Could**
- Filtro por comprador e/ou por data na listagem de pedidos.
- Ordenação/paginação simples nas listagens.

**Won't (por ora)**
- Cancelamento de pedido separado de exclusão.
- Autenticação/autorização.
- Regras diferenciadas PF/PJ para comprador.
- Histórico de preço por origem de carne.
