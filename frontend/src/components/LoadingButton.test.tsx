import { describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { LoadingButton } from './LoadingButton';

describe('LoadingButton', () => {
  it('renderiza o children normalmente quando isLoading é false', () => {
    render(<LoadingButton>Salvar</LoadingButton>);

    expect(screen.getByText('Salvar')).toBeInTheDocument();
    expect(screen.queryByLabelText('Carregando')).not.toBeInTheDocument();
  });

  it('renderiza o children normalmente quando isLoading não é informado', () => {
    render(<LoadingButton>Salvar</LoadingButton>);

    expect(screen.getByText('Salvar')).toBeInTheDocument();
  });

  it('fica desabilitado quando isLoading é true', () => {
    render(<LoadingButton isLoading>Salvar</LoadingButton>);

    const button = screen.getByRole('button');
    expect(button).toBeDisabled();
  });

  it('fica desabilitado quando disabled é true (mesmo que isLoading seja false)', () => {
    render(<LoadingButton disabled>Salvar</LoadingButton>);

    expect(screen.getByRole('button')).toBeDisabled();
  });

  it('mostra spinner quando isLoading é true', () => {
    render(<LoadingButton isLoading>Salvar</LoadingButton>);

    const spinner = screen.getByRole('button').querySelector('.loading-button__spinner');
    expect(spinner).toBeInTheDocument();
  });

  it('oculta o texto quando isLoading é true, mostrando apenas o spinner', () => {
    const { container } = render(<LoadingButton isLoading>Salvar</LoadingButton>);

    const button = screen.getByRole('button');
    expect(button).toHaveClass('loading-button--loading');

    // O spinner está lá
    expect(button.querySelector('.loading-button__spinner')).toBeInTheDocument();

    // O texto está na DOM mas oculto (aria-hidden não, apenas visualmente)
    expect(container.textContent).toContain('Salvar');
  });

  it('fica desabilitado mesmo que disabled=false quando isLoading=true', () => {
    render(
      <LoadingButton isLoading disabled={false}>
        Salvar
      </LoadingButton>,
    );

    expect(screen.getByRole('button')).toBeDisabled();
  });

  it('não chama onClick enquanto isLoading é true', async () => {
    const user = userEvent.setup();
    const onClick = vi.fn();

    render(
      <LoadingButton isLoading onClick={onClick}>
        Salvar
      </LoadingButton>,
    );

    await user.click(screen.getByRole('button'));

    // Botão desabilitado não dispara click
    expect(onClick).not.toHaveBeenCalled();
  });

  it('chama onClick normalmente quando isLoading é false', async () => {
    const user = userEvent.setup();
    const onClick = vi.fn();

    render(<LoadingButton onClick={onClick}>Salvar</LoadingButton>);

    await user.click(screen.getByRole('button'));

    expect(onClick).toHaveBeenCalled();
  });

  it('aceita type="submit" e outras props de HTMLButtonElement', () => {
    render(
      <LoadingButton type="submit" className="custom-class" data-testid="my-button">
        Salvar
      </LoadingButton>,
    );

    const button = screen.getByTestId('my-button');
    expect(button).toHaveAttribute('type', 'submit');
    expect(button).toHaveClass('custom-class');
  });

  it('combina isLoading com disabled passado explicitamente', () => {
    const onClick = vi.fn();
    render(
      <LoadingButton isLoading disabled onClick={onClick}>
        Salvar
      </LoadingButton>,
    );

    const button = screen.getByRole('button');
    expect(button).toBeDisabled();
  });
});
