export interface BreadcrumbItem {
  label: string;
  /** Ausente quando o item é a página atual (não é clicável). */
  path?: string;
}

/** Rótulo da rota-raiz de cada seção (primeiro segmento da URL). */
const BASE_LABELS: Record<string, string> = {
  dashboard: 'Dashboard',
  carnes: 'Carnes',
  compradores: 'Compradores',
  pedidos: 'Pedidos',
};

/** Rótulo do segundo nível quando a rota é "/<base>/novo". */
const NOVO_LABELS: Record<string, string> = {
  carnes: 'Nova carne',
  compradores: 'Novo comprador',
  pedidos: 'Novo pedido',
};

/**
 * Deriva os itens do breadcrumb a partir do pathname atual, sem depender de
 * nenhuma configuração extra por rota — reaproveita o mesmo mapeamento simples
 * (rota → label) tanto para a listagem quanto para as telas de formulário
 * (novo/editar) das 3 entidades + Dashboard.
 *
 * Função pura, separada do componente `Breadcrumb` — lógica de
 * formatação separada de JSX.
 */
export function buildBreadcrumbItems(pathname: string): BreadcrumbItem[] {
  const segments = pathname.split('/').filter(Boolean);

  if (segments.length === 0) {
    return [];
  }

  const [base, ...rest] = segments;
  const baseLabel = BASE_LABELS[base];

  if (!baseLabel) {
    return [];
  }

  if (rest.length === 0) {
    // Está na própria listagem/página-raiz — item único, é a página atual.
    return [{ label: baseLabel }];
  }

  const items: BreadcrumbItem[] = [{ label: baseLabel, path: `/${base}` }];

  if (rest[0] === 'novo') {
    items.push({ label: NOVO_LABELS[base] ?? 'Novo' });
    return items;
  }

  if (rest[1] === 'editar') {
    items.push({ label: 'Editar' });
    return items;
  }

  return items;
}
