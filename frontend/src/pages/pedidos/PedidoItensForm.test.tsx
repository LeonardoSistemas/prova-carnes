import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render } from '@testing-library/react';
import { OrigemCarne, type CarneResponseDto } from '../../api/types';
import { criarItemPedidoVazio, type PedidoItemFormErrors, type PedidoItemFormValues } from '../../validation/validatePedidoForm';
import { PedidoItensForm } from './PedidoItensForm';

const CARNE: CarneResponseDto = { id: 1, descricao: 'Picanha', origem: OrigemCarne.Bovina };
const CARNES: CarneResponseDto[] = [CARNE];

describe('PedidoItensForm - mensagem de estado vazio', () => {
  it('exibe a mensagem de estado vazio quando não há itens', () => {
    const onChange = vi.fn();
    render(
      <PedidoItensForm
        itens={[]}
        itensErrors={[]}
        erroGeral={undefined}
        carnes={CARNES}
        onChange={onChange}
      />,
    );

    expect(
      screen.getByText('Nenhum item adicionado ainda — clique em "Adicionar item" para começar.'),
    ).toBeInTheDocument();
  });

  it('não exibe a mensagem de estado vazio quando há pelo menos um item', () => {
    const item: PedidoItemFormValues = criarItemPedidoVazio();
    const onChange = vi.fn();
    render(
      <PedidoItensForm
        itens={[item]}
        itensErrors={[{}]}
        erroGeral={undefined}
        carnes={CARNES}
        onChange={onChange}
      />,
    );

    expect(
      screen.queryByText('Nenhum item adicionado ainda — clique em "Adicionar item" para começar.'),
    ).not.toBeInTheDocument();
  });

  it('a mensagem desaparece quando um item é adicionado', async () => {
    const user = userEvent.setup();
    let itens: PedidoItemFormValues[] = [];
    const itensErrors: PedidoItemFormErrors[] = [];

    const onChange = vi.fn((proximosItens: PedidoItemFormValues[]) => {
      itens = proximosItens;
    });

    const { rerender } = render(
      <PedidoItensForm
        itens={itens}
        itensErrors={itensErrors}
        erroGeral={undefined}
        carnes={CARNES}
        onChange={onChange}
      />,
    );

    // Verifica que a mensagem está presente inicialmente
    expect(
      screen.getByText('Nenhum item adicionado ainda — clique em "Adicionar item" para começar.'),
    ).toBeInTheDocument();

    // Adiciona um item
    await user.click(screen.getByRole('button', { name: 'Adicionar item' }));

    // Simula o update após onChange
    itens = [criarItemPedidoVazio()];
    itensErrors.push({});
    rerender(
      <PedidoItensForm
        itens={itens}
        itensErrors={itensErrors}
        erroGeral={undefined}
        carnes={CARNES}
        onChange={onChange}
      />,
    );

    // Verifica que a mensagem desapareceu
    expect(
      screen.queryByText('Nenhum item adicionado ainda — clique em "Adicionar item" para começar.'),
    ).not.toBeInTheDocument();
  });

  it('permite remover um item deixando a lista vazia novamente', async () => {
    const user = userEvent.setup();
    const item: PedidoItemFormValues = criarItemPedidoVazio();
    let itens: PedidoItemFormValues[] = [item];
    const itensErrors: PedidoItemFormErrors[] = [{}];

    const onChange = vi.fn((proximosItens: PedidoItemFormValues[]) => {
      itens = proximosItens;
    });

    const { rerender } = render(
      <PedidoItensForm
        itens={itens}
        itensErrors={itensErrors}
        erroGeral={undefined}
        carnes={CARNES}
        onChange={onChange}
      />,
    );

    // Inicialmente não deve ter a mensagem de vazio
    expect(
      screen.queryByText('Nenhum item adicionado ainda — clique em "Adicionar item" para começar.'),
    ).not.toBeInTheDocument();

    // Remove o item
    await user.click(screen.getByRole('button', { name: 'Remover item' }));

    // Simula o update após onChange
    itens = [];
    rerender(
      <PedidoItensForm
        itens={itens}
        itensErrors={[]}
        erroGeral={undefined}
        carnes={CARNES}
        onChange={onChange}
      />,
    );

    // Verifica que a mensagem reaparece
    expect(
      screen.getByText('Nenhum item adicionado ainda — clique em "Adicionar item" para começar.'),
    ).toBeInTheDocument();
  });
});

describe('PedidoItensForm - adição de itens', () => {
  it('permite adicionar um novo item', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    render(
      <PedidoItensForm
        itens={[]}
        itensErrors={[]}
        erroGeral={undefined}
        carnes={CARNES}
        onChange={onChange}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Adicionar item' }));

    expect(onChange).toHaveBeenCalledWith([expect.objectContaining({ carneId: '', preco: '', moeda: '' })]);
  });

  it('permite adicionar múltiplos itens', async () => {
    const user = userEvent.setup();
    let itens: PedidoItemFormValues[] = [];
    const onChange = vi.fn((proximosItens: PedidoItemFormValues[]) => {
      itens = proximosItens;
    });

    const { rerender } = render(
      <PedidoItensForm
        itens={itens}
        itensErrors={[]}
        erroGeral={undefined}
        carnes={CARNES}
        onChange={onChange}
      />,
    );

    // Primeiro item
    await user.click(screen.getByRole('button', { name: 'Adicionar item' }));
    itens = [criarItemPedidoVazio()];
    rerender(
      <PedidoItensForm
        itens={itens}
        itensErrors={[{}]}
        erroGeral={undefined}
        carnes={CARNES}
        onChange={onChange}
      />,
    );

    // Segundo item
    await user.click(screen.getByRole('button', { name: 'Adicionar item' }));
    expect(onChange).toHaveBeenLastCalledWith([
      expect.any(Object),
      expect.objectContaining({ carneId: '', preco: '', moeda: '' }),
    ]);
  });
});

describe('PedidoItensForm - remoção de itens', () => {
  it('permite remover um item específico', async () => {
    const user = userEvent.setup();
    const item1: PedidoItemFormValues = criarItemPedidoVazio();
    const item2: PedidoItemFormValues = criarItemPedidoVazio();
    const onChange = vi.fn();

    render(
      <PedidoItensForm
        itens={[item1, item2]}
        itensErrors={[{}, {}]}
        erroGeral={undefined}
        carnes={CARNES}
        onChange={onChange}
      />,
    );

    const botõesRemover = screen.getAllByRole('button', { name: 'Remover item' });
    await user.click(botõesRemover[0]);

    expect(onChange).toHaveBeenCalledWith([item2]);
  });
});
