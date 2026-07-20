/**
 * Tipos TypeScript espelhando os DTOs do backend (.NET 8) — ver contrato em
 * TASKS.md. Nenhum `any`: todo shape de resposta da API é tipado
 * aqui e reutilizado pelos módulos de `src/api/*Api.ts` e pelos hooks.
 *
 * IMPORTANTE sobre os enums: o backend NÃO usa JsonStringEnumConverter — os
 * controllers usam o serializador padrão do ASP.NET Core, que serializa
 * enum como o valor NUMÉRICO subjacente (verificado rodando o backend real:
 * `POST /api/carnes` com `OrigemCarne.Suina` retornou `{"origem":2}`, não
 * `"origem":"Suina"`). Por isso os enums abaixo são `enum` numéricos do
 * TypeScript com os MESMOS valores do C# (`OrigemCarne`/`Moeda`), não union
 * types de string.
 */

/**
 * `erasableSyntaxOnly` (tsconfig) proíbe `enum` do TypeScript (gera código
 * em runtime, não é só um tipo apagável na compilação) — por isso os
 * "enums" abaixo são objetos `const` + tipo union derivado, padrão
 * recomendado para TS moderno com essa flag. O valor numérico de cada
 * membro é o mesmo do enum C# correspondente.
 */
export const OrigemCarne = {
  Bovina: 1,
  Suina: 2,
  Aves: 3,
  Peixes: 4,
} as const;
export type OrigemCarne = (typeof OrigemCarne)[keyof typeof OrigemCarne];

export const Moeda = {
  BRL: 1,
  USD: 2,
  EUR: 3,
} as const;
export type Moeda = (typeof Moeda)[keyof typeof Moeda];

// ---- Carne ----

export interface CarneDto {
  descricao: string;
  origem: OrigemCarne;
}

export interface CarneResponseDto {
  id: number;
  descricao: string;
  origem: OrigemCarne;
}

// ---- Comprador ----

export interface CompradorDto {
  nome: string;
  documento: string;
  cidadeId: number;
}

export interface CompradorResponseDto {
  id: number;
  nome: string;
  documento: string;
  cidadeId: number;
}

// ---- Estado / Cidade (somente leitura) ----

export interface CidadeDto {
  id: number;
  nome: string;
  estadoId: number;
}

export interface EstadoComCidadesDto {
  id: number;
  nome: string;
  uf: string;
  cidades: CidadeDto[];
}

// ---- Pedido ----

export interface PedidoItemDto {
  carneId: number;
  preco: number;
  moeda: Moeda;
}

export interface PedidoDto {
  data: string;
  compradorId: number;
  itens: PedidoItemDto[];
}

export interface PedidoItemResponseDto {
  id: number;
  carneId: number;
  preco: number;
  moeda: Moeda;
  cotacaoUsada: number;
  valorEmReal: number;
}

export interface PedidoResponseDto {
  id: number;
  data: string;
  compradorId: number;
  itens: PedidoItemResponseDto[];
  valorTotalEmReal: number;
}

// ---- Dashboard ----

/**
 * Período de agregação aceito por `GET /api/dashboard` (T62). Diferente de
 * `OrigemCarne`/`Moeda` acima, este NÃO é um enum serializado em campo de
 * resposta — é um valor de query string (`?periodo=hoje`) que o backend faz
 * parse manual via `Enum.TryParse<PeriodoDashboard>` (case-insensitive, ver
 * `DashboardController`). Por isso é representado aqui como union type de
 * string (os valores em minúsculo do C# `PeriodoDashboard`: `Hoje`, `Semana`,
 * `Mes`), não como enum numérico.
 */
export type PeriodoDashboard = 'hoje' | 'semana' | 'mes';

export interface DashboardResumoDto {
  totalPedidos: number;
  faturamentoTotal: number;
  ticketMedio: number;
  compradoresAtivos: number;
  compradoresCadastrados: number;
}

export interface TopCarneDto {
  carneId: number;
  descricao: string;
  valorTotal: number;
}

export interface TopCompradorDto {
  compradorId: number;
  nome: string;
  valorTotal: number;
}

export interface DashboardTopDto {
  topCarnes: TopCarneDto[];
  topCompradores: TopCompradorDto[];
}

export interface DashboardDto {
  resumo: DashboardResumoDto;
  top: DashboardTopDto;
}

export interface FaturamentoPorDiaDto {
  data: string;
  faturamento: number;
}

// ---- Erro (formato único para 400/404/409/422/500) ----

export interface ErroRespostaDto {
  erros: string[];
}
