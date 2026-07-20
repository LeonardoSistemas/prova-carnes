import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { Sidebar } from './Sidebar';

describe('Sidebar', () => {
  const renderSidebar = () => {
    return render(
      <BrowserRouter>
        <Sidebar />
      </BrowserRouter>,
    );
  };

  it('renderiza o botão hamburger', () => {
    renderSidebar();

    const button = screen.getByRole('button', { name: 'Abrir menu' });
    expect(button).toBeInTheDocument();
  });

  it('renderiza os links de navegação (Dashboard, Carnes, Compradores, Pedidos)', () => {
    renderSidebar();

    expect(screen.getByRole('link', { name: 'Dashboard' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Carnes' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Compradores' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Pedidos' })).toBeInTheDocument();
  });

  it('renderiza o brand "Prova Carnes"', () => {
    renderSidebar();

    expect(screen.getByText('Prova Carnes')).toBeInTheDocument();
  });

  it('inicialmente o menu está fechado — sidebar não tem classe sidebar--mobile-open', () => {
    renderSidebar();

    const nav = screen.getByRole('navigation');
    expect(nav).not.toHaveClass('sidebar--mobile-open');
  });

  it('clicar no botão hamburger abre o menu — sidebar recebe classe sidebar--mobile-open', async () => {
    const user = userEvent.setup();
    renderSidebar();

    const button = screen.getByRole('button', { name: 'Abrir menu' });
    const nav = screen.getByRole('navigation');

    expect(nav).not.toHaveClass('sidebar--mobile-open');

    await user.click(button);

    expect(nav).toHaveClass('sidebar--mobile-open');
  });

  it('clicar novamente no botão hamburger fecha o menu', async () => {
    const user = userEvent.setup();
    renderSidebar();

    const button = screen.getByRole('button', { name: 'Abrir menu' });
    const nav = screen.getByRole('navigation');

    // Abre o menu
    await user.click(button);
    expect(nav).toHaveClass('sidebar--mobile-open');

    // Fecha o menu
    await user.click(button);
    expect(nav).not.toHaveClass('sidebar--mobile-open');
  });

  it('clicar em um link de navegação fecha o menu automaticamente', async () => {
    const user = userEvent.setup();
    renderSidebar();

    const button = screen.getByRole('button', { name: 'Abrir menu' });
    const nav = screen.getByRole('navigation');
    const carnesLink = screen.getByRole('link', { name: 'Carnes' });

    // Abre o menu
    await user.click(button);
    expect(nav).toHaveClass('sidebar--mobile-open');

    // Clica em um link
    await user.click(carnesLink);

    // Menu deve fechar
    expect(nav).not.toHaveClass('sidebar--mobile-open');
  });

  it('clicar no overlay fecha o menu', async () => {
    const user = userEvent.setup();
    renderSidebar();

    const button = screen.getByRole('button', { name: 'Abrir menu' });
    const nav = screen.getByRole('navigation');

    // Abre o menu (o overlay aparece no DOM)
    await user.click(button);
    expect(nav).toHaveClass('sidebar--mobile-open');

    // Encontra o overlay (é um div com classe sidebar-overlay)
    const overlay = document.querySelector('.sidebar-overlay');
    expect(overlay).toBeInTheDocument();

    // Clica no overlay
    await user.click(overlay!);

    // Menu deve fechar
    expect(nav).not.toHaveClass('sidebar--mobile-open');

    // Overlay deve desaparecer
    expect(document.querySelector('.sidebar-overlay')).not.toBeInTheDocument();
  });

  it('o overlay só existe quando o menu está aberto', async () => {
    const user = userEvent.setup();
    renderSidebar();

    // Inicialmente não há overlay
    expect(document.querySelector('.sidebar-overlay')).not.toBeInTheDocument();

    // Abre o menu
    const button = screen.getByRole('button', { name: 'Abrir menu' });
    await user.click(button);

    // Agora há overlay
    expect(document.querySelector('.sidebar-overlay')).toBeInTheDocument();

    // Fecha o menu
    await user.click(button);

    // Overlay desaparece
    expect(document.querySelector('.sidebar-overlay')).not.toBeInTheDocument();
  });
});
