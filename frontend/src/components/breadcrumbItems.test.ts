import { describe, expect, it } from 'vitest';
import { buildBreadcrumbItems } from './breadcrumbItems';

describe('buildBreadcrumbItems', () => {
  it('retorna um único item, sem link, na rota de listagem', () => {
    expect(buildBreadcrumbItems('/carnes')).toEqual([{ label: 'Carnes' }]);
  });

  it('retorna "Carnes > Editar" com link para /carnes na rota de edição', () => {
    expect(buildBreadcrumbItems('/carnes/5/editar')).toEqual([
      { label: 'Carnes', path: '/carnes' },
      { label: 'Editar' },
    ]);
  });

  it('retorna "Pedidos > Novo pedido" na rota de novo pedido', () => {
    expect(buildBreadcrumbItems('/pedidos/novo')).toEqual([
      { label: 'Pedidos', path: '/pedidos' },
      { label: 'Novo pedido' },
    ]);
  });

  it('retorna "Compradores > Novo comprador" na rota de novo comprador', () => {
    expect(buildBreadcrumbItems('/compradores/novo')).toEqual([
      { label: 'Compradores', path: '/compradores' },
      { label: 'Novo comprador' },
    ]);
  });

  it('retorna lista vazia para rotas desconhecidas', () => {
    expect(buildBreadcrumbItems('/')).toEqual([]);
    expect(buildBreadcrumbItems('/rota-inexistente')).toEqual([]);
  });
});
