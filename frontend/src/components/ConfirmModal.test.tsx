import { describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ConfirmModal } from './ConfirmModal';

describe('ConfirmModal', () => {
  it('não renderiza nada quando isOpen é false', () => {
    render(
      <ConfirmModal
        isOpen={false}
        title="Excluir carne"
        message="Tem certeza?"
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />,
    );

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  it('exibe título e mensagem quando aberto', () => {
    render(
      <ConfirmModal
        isOpen
        title="Excluir carne"
        message='Tem certeza que deseja excluir "Picanha"?'
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />,
    );

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(screen.getByText('Excluir carne')).toBeInTheDocument();
    expect(screen.getByText('Tem certeza que deseja excluir "Picanha"?')).toBeInTheDocument();
  });

  it('cancelar chama onCancel e NUNCA onConfirm', async () => {
    const user = userEvent.setup();
    const onConfirm = vi.fn();
    const onCancel = vi.fn();

    render(<ConfirmModal isOpen title="Excluir carne" message="Tem certeza?" onConfirm={onConfirm} onCancel={onCancel} />);

    await user.click(screen.getByRole('button', { name: 'Cancelar' }));

    expect(onCancel).toHaveBeenCalledTimes(1);
    expect(onConfirm).not.toHaveBeenCalled();
  });

  it('confirmar chama onConfirm', async () => {
    const user = userEvent.setup();
    const onConfirm = vi.fn();
    const onCancel = vi.fn();

    render(<ConfirmModal isOpen title="Excluir carne" message="Tem certeza?" onConfirm={onConfirm} onCancel={onCancel} />);

    await user.click(screen.getByRole('button', { name: 'Confirmar' }));

    expect(onConfirm).toHaveBeenCalledTimes(1);
    expect(onCancel).not.toHaveBeenCalled();
  });

  it('exibe as mensagens de erro reais quando `errors` é informado', () => {
    render(
      <ConfirmModal
        isOpen
        title="Excluir carne"
        message="Tem certeza?"
        errors={['Não é possível excluir: existem pedidos vinculados a esta carne.']}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />,
    );

    expect(
      screen.getByText('Não é possível excluir: existem pedidos vinculados a esta carne.'),
    ).toBeInTheDocument();
  });

  it('desabilita os botões enquanto isConfirming é true', () => {
    render(
      <ConfirmModal
        isOpen
        title="Excluir carne"
        message="Tem certeza?"
        isConfirming
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />,
    );

    const buttons = screen.getAllByRole('button');
    // Primeiro botão é "Cancelar", segundo é "Confirmar" (danger)
    expect(buttons[0]).toBeDisabled();
    expect(buttons[1]).toBeDisabled();
  });
});
