import { useEffect, useRef } from 'react';
import type { CarneResponseDto } from '../../api/types';
import { FieldError } from '../../components/FieldError';
import { criarItemPedidoVazio } from '../../validation/validatePedidoForm';
import type { PedidoItemFormErrors, PedidoItemFormValues } from '../../validation/validatePedidoForm';
import { PedidoItemRow } from './PedidoItemRow';

interface PedidoItensFormProps {
  itens: PedidoItemFormValues[];
  itensErrors: PedidoItemFormErrors[];
  erroGeral: string | undefined;
  carnes: CarneResponseDto[];
  onChange: (itens: PedidoItemFormValues[]) => void;
}

/** Lista de itens dinâmica do Pedido — permite adicionar/remover linhas (carne + preço + moeda). */
export function PedidoItensForm({ itens, itensErrors, erroGeral, carnes, onChange }: PedidoItensFormProps) {
  const ultimoItemRef = useRef<HTMLDivElement>(null);

  function handleAdicionarItem() {
    onChange([...itens, criarItemPedidoVazio()]);
  }

  function handleAlterarItem(index: number, item: PedidoItemFormValues) {
    const proximosItens = [...itens];
    proximosItens[index] = item;
    onChange(proximosItens);
  }

  function handleRemoverItem(index: number) {
    onChange(itens.filter((_, itemIndex) => itemIndex !== index));
  }

  // Scroll suave até o novo item quando itens são adicionados
  useEffect(() => {
    if (itens.length > 0 && ultimoItemRef.current) {
      ultimoItemRef.current.scrollIntoView?.({ behavior: 'smooth', block: 'nearest' });
    }
  }, [itens.length]);

  return (
    <div className="pedido-itens">
      <h2>Itens do pedido</h2>
      <FieldError message={erroGeral} />

      {itens.length === 0 ? (
        <p className="itens-vazio">
          Nenhum item adicionado ainda — clique em "Adicionar item" para começar.
        </p>
      ) : (
        itens.map((item, index) => (
          <PedidoItemRow
            key={item.chave}
            ref={index === itens.length - 1 ? ultimoItemRef : null}
            item={item}
            errors={itensErrors[index]}
            carnes={carnes}
            onChange={(proximoItem) => handleAlterarItem(index, proximoItem)}
            onRemove={() => handleRemoverItem(index)}
          />
        ))
      )}

      <button type="button" onClick={handleAdicionarItem}>
        Adicionar item
      </button>
    </div>
  );
}
