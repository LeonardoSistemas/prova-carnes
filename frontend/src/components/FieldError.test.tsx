import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import { FieldError } from './FieldError';

describe('FieldError', () => {
  it('não renderiza nada quando message é undefined', () => {
    const { container } = render(<FieldError message={undefined} />);
    expect(container.firstChild).toBeNull();
  });

  it('não renderiza nada quando message é string vazia', () => {
    const { container } = render(<FieldError message="" />);
    expect(container.firstChild).toBeNull();
  });

  it('renderiza mensagem de erro quando fornecida', () => {
    render(<FieldError message="Campo obrigatório." />);

    expect(screen.getByText('Campo obrigatório.')).toBeInTheDocument();
  });

  it('renderiza com role="alert" para acessibilidade', () => {
    render(<FieldError message="Erro no campo." />);

    expect(screen.getByRole('alert')).toBeInTheDocument();
  });

  it('renderiza com classe field-error', () => {
    const { container } = render(<FieldError message="Erro" />);

    expect(container.querySelector('.field-error')).toBeInTheDocument();
  });

  it('renderiza mensagem longa corretamente', () => {
    const mensagem = 'Este é um erro muito longo que descreve por que o campo não foi preenchido corretamente.';
    render(<FieldError message={mensagem} />);

    expect(screen.getByText(mensagem)).toBeInTheDocument();
  });

  it('renderiza sem message prop (padrão undefined)', () => {
    const { container } = render(<FieldError />);
    expect(container.firstChild).toBeNull();
  });
});
