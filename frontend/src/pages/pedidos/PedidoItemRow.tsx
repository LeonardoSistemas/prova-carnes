import { forwardRef } from 'react';
import type { CarneResponseDto, Moeda } from '../../api/types';
import { FieldError } from '../../components/FieldError';
import { MOEDA_LABELS, MOEDA_OPCOES } from '../../format/enumLabels';
import type { PedidoItemFormErrors, PedidoItemFormValues } from '../../validation/validatePedidoForm';

interface PedidoItemRowProps {
  item: PedidoItemFormValues;
  errors: PedidoItemFormErrors | undefined;
  carnes: CarneResponseDto[];
  onChange: (item: PedidoItemFormValues) => void;
  onRemove: () => void;
}

/** Uma linha do formulário de itens do Pedido: carne + preço + moeda + remover. */
export const PedidoItemRow = forwardRef<HTMLDivElement, PedidoItemRowProps>(
  ({ item, errors, carnes, onChange, onRemove }, ref) => {
    return (
      <div ref={ref} className="pedido-item-row" role="group" aria-label="Item do pedido">
      <div className="form-field">
        <label htmlFor={`item-carne-${item.chave}`}>Carne</label>
        <select
          id={`item-carne-${item.chave}`}
          value={item.carneId}
          onChange={(event) =>
            onChange({ ...item, carneId: event.target.value === '' ? '' : Number(event.target.value) })
          }
        >
          <option value="">Selecione...</option>
          {carnes.map((carne) => (
            <option key={carne.id} value={carne.id}>
              {carne.descricao}
            </option>
          ))}
        </select>
        <FieldError message={errors?.carneId} />
      </div>

      <div className="form-field">
        <label htmlFor={`item-preco-${item.chave}`}>Preço</label>
        <input
          id={`item-preco-${item.chave}`}
          type="number"
          step="0.01"
          value={item.preco}
          onChange={(event) => onChange({ ...item, preco: event.target.value })}
        />
        <FieldError message={errors?.preco} />
      </div>

      <div className="form-field">
        <label htmlFor={`item-moeda-${item.chave}`}>Moeda</label>
        <select
          id={`item-moeda-${item.chave}`}
          value={item.moeda}
          onChange={(event) =>
            onChange({ ...item, moeda: event.target.value === '' ? '' : (Number(event.target.value) as Moeda) })
          }
        >
          <option value="">Selecione...</option>
          {MOEDA_OPCOES.map((moeda) => (
            <option key={moeda} value={moeda}>
              {MOEDA_LABELS[moeda]}
            </option>
          ))}
        </select>
        <FieldError message={errors?.moeda} />
      </div>

      <button type="button" className="danger" onClick={onRemove} aria-label="Remover item">
        Remover
      </button>
    </div>
  );
  }
);
