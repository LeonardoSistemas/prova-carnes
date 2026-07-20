import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ApiErrorBanner } from './ApiErrorBanner';

describe('ApiErrorBanner', () => {
  it('não renderiza nada quando errors é null', () => {
    const { container } = render(<ApiErrorBanner errors={null} />);
    expect(container.firstChild).toBeNull();
  });

  it('não renderiza nada quando errors é undefined', () => {
    const { container } = render(<ApiErrorBanner errors={undefined} />);
    expect(container.firstChild).toBeNull();
  });

  it('não renderiza nada quando errors é array vazio', () => {
    const { container } = render(<ApiErrorBanner errors={[]} />);
    expect(container.firstChild).toBeNull();
  });

  it('renderiza uma mensagem quando há um erro', () => {
    render(<ApiErrorBanner errors={['Erro ao salvar.']} />);

    expect(screen.getByText('Erro ao salvar.')).toBeInTheDocument();
  });

  it('renderiza múltiplas mensagens em múltiplos <p>', () => {
    render(
      <ApiErrorBanner
        errors={['Erro 1', 'Erro 2', 'Erro 3']}
      />,
    );

    expect(screen.getByText('Erro 1')).toBeInTheDocument();
    expect(screen.getByText('Erro 2')).toBeInTheDocument();
    expect(screen.getByText('Erro 3')).toBeInTheDocument();

    // Verificar que cada mensagem está em seu próprio <p>
    const paragraphs = screen.getAllByRole('paragraph');
    expect(paragraphs).toHaveLength(3);
  });

  it('renderiza com role="alert" para acessibilidade', () => {
    render(<ApiErrorBanner errors={['Ocorreu um erro.']} />);

    expect(screen.getByRole('alert')).toBeInTheDocument();
  });

  it('renderiza em div com classe api-error-banner', () => {
    const { container } = render(<ApiErrorBanner errors={['Erro']} />);

    const banner = container.querySelector('.api-error-banner');
    expect(banner).toBeInTheDocument();
  });

  it('renderiza mensagens com caracteres especiais corretamente', () => {
    render(
      <ApiErrorBanner
        errors={['Não foi possível conectar ao servidor.', 'Erro: & < >']}
      />,
    );

    expect(screen.getByText('Não foi possível conectar ao servidor.')).toBeInTheDocument();
    expect(screen.getByText('Erro: & < >')).toBeInTheDocument();
  });

  it('renderiza mensagens longas corretamente', () => {
    const mensagemLonga = 'Esta é uma mensagem de erro muito longa que descreve em detalhes o que deu errado com a operação.';
    render(<ApiErrorBanner errors={[mensagemLonga]} />);

    expect(screen.getByText(mensagemLonga)).toBeInTheDocument();
  });
});
