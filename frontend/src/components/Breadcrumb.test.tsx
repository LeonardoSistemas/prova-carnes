import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { Breadcrumb } from './Breadcrumb';

function renderBreadcrumbAt(pathname: string) {
  return render(
    <MemoryRouter initialEntries={[pathname]}>
      <Breadcrumb />
    </MemoryRouter>,
  );
}

describe('Breadcrumb', () => {
  it('renderiza apenas "Carnes" na listagem de carnes (item único, sem link)', () => {
    renderBreadcrumbAt('/carnes');

    const nav = screen.getByRole('navigation', { name: 'Caminho de navegação' });
    expect(nav).toHaveTextContent('Carnes');
    expect(screen.queryByRole('link', { name: 'Carnes' })).not.toBeInTheDocument();
  });

  it('renderiza "Carnes > Editar" na rota de edição de carne, com "Carnes" como link', () => {
    renderBreadcrumbAt('/carnes/3/editar');

    const nav = screen.getByRole('navigation', { name: 'Caminho de navegação' });
    expect(nav).toHaveTextContent('Carnes');
    expect(nav).toHaveTextContent('Editar');

    const link = screen.getByRole('link', { name: 'Carnes' });
    expect(link).toHaveAttribute('href', '/carnes');
  });

  it('renderiza "Pedidos > Novo pedido" na rota de novo pedido', () => {
    renderBreadcrumbAt('/pedidos/novo');

    const nav = screen.getByRole('navigation', { name: 'Caminho de navegação' });
    expect(nav).toHaveTextContent('Pedidos');
    expect(nav).toHaveTextContent('Novo pedido');

    const link = screen.getByRole('link', { name: 'Pedidos' });
    expect(link).toHaveAttribute('href', '/pedidos');
  });

  it('renderiza "Compradores > Novo comprador" na rota de novo comprador', () => {
    renderBreadcrumbAt('/compradores/novo');

    const nav = screen.getByRole('navigation', { name: 'Caminho de navegação' });
    expect(nav).toHaveTextContent('Compradores');
    expect(nav).toHaveTextContent('Novo comprador');
  });

  it('renderiza apenas "Dashboard" na rota do dashboard', () => {
    renderBreadcrumbAt('/dashboard');

    const nav = screen.getByRole('navigation', { name: 'Caminho de navegação' });
    expect(nav).toHaveTextContent('Dashboard');
  });

  it('não renderiza nada em uma rota desconhecida (ex.: "/")', () => {
    renderBreadcrumbAt('/');

    expect(screen.queryByRole('navigation', { name: 'Caminho de navegação' })).not.toBeInTheDocument();
  });
});
