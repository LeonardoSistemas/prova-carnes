import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import toast from 'react-hot-toast';
import { useNavigate, useParams } from 'react-router-dom';
import type { Moeda, PedidoDto } from '../../api/types';
import { getErrorMessages } from '../../api/getErrorMessages';
import { ApiErrorBanner } from '../../components/ApiErrorBanner';
import { Breadcrumb } from '../../components/Breadcrumb';
import { FieldError } from '../../components/FieldError';
import { LoadingButton } from '../../components/LoadingButton';
import { useCarnes } from '../../hooks/useCarnes';
import { useCompradores } from '../../hooks/useCompradores';
import { useCreatePedido, usePedido, useUpdatePedido } from '../../hooks/usePedidos';
import {
  PEDIDO_FORM_INICIAL,
  isPedidoFormValid,
  validatePedidoForm,
  type PedidoFormErrors,
  type PedidoFormValues,
} from '../../validation/validatePedidoForm';
import { mapPedidoResponseToFormValues } from './mapPedidoResponseToFormValues';
import { PedidoItensForm } from './PedidoItensForm';

export function PedidoFormPage() {
  const { id } = useParams<{ id?: string }>();
  const pedidoId = id ? Number(id) : undefined;
  const modoEdicao = pedidoId !== undefined;

  const navigate = useNavigate();

  const { data: carnes = [] } = useCarnes();
  const { data: compradores = [] } = useCompradores();
  const { data: pedidoExistente, isLoading: carregandoPedido } = usePedido(pedidoId);

  const criarPedido = useCreatePedido();
  const atualizarPedido = useUpdatePedido();
  const mutationAtiva = modoEdicao ? atualizarPedido : criarPedido;

  const [values, setValues] = useState<PedidoFormValues>(PEDIDO_FORM_INICIAL);
  const [errors, setErrors] = useState<PedidoFormErrors>({ itensDetalhes: [] });
  const [foiSubmetido, setFoiSubmetido] = useState(false);

  useEffect(() => {
    if (pedidoExistente) {
      setValues(mapPedidoResponseToFormValues(pedidoExistente));
    }
  }, [pedidoExistente]);

  function atualizarValores(proximosValues: PedidoFormValues) {
    setValues(proximosValues);
    if (foiSubmetido) {
      setErrors(validatePedidoForm(proximosValues));
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFoiSubmetido(true);

    const errosValidacao = validatePedidoForm(values);
    setErrors(errosValidacao);

    if (!isPedidoFormValid(errosValidacao)) {
      return;
    }

    const dto: PedidoDto = {
      data: values.data,
      compradorId: values.compradorId as number,
      itens: values.itens.map((item) => ({
        carneId: item.carneId as number,
        preco: Number(item.preco),
        moeda: item.moeda as Moeda,
      })),
    };

    if (modoEdicao) {
      atualizarPedido.mutate(
        { id: pedidoId as number, dto },
        {
          onSuccess: () => {
            toast.success('Pedido atualizado com sucesso.');
            navigate('/pedidos');
          },
          onError: (error) => {
            // O botão volta ao estado normal automaticamente — `isPending`
            // é controlado pelo TanStack Query e já cai para `false` assim
            // que a mutation rejeita (nunca fica em loading infinito, nem
            // no caso de 422 por falha de cotação).
            toast.error(getErrorMessages(error).join(' '));
          },
        },
      );
      return;
    }

    criarPedido.mutate(dto, {
      onSuccess: () => {
        toast.success('Pedido cadastrado com sucesso.');
        navigate('/pedidos');
      },
      onError: (error) => {
        toast.error(getErrorMessages(error).join(' '));
      },
    });
  }

  if (modoEdicao && carregandoPedido) {
    return <p>Carregando pedido...</p>;
  }

  return (
    <section>
      <Breadcrumb />
      <h1>{modoEdicao ? `Editar pedido #${pedidoId}` : 'Novo pedido'}</h1>

      <form onSubmit={handleSubmit} noValidate>
        <div className="form-field">
          <label htmlFor="pedido-data">Data</label>
          <input
            id="pedido-data"
            type="date"
            value={values.data}
            onChange={(event) => atualizarValores({ ...values, data: event.target.value })}
          />
          <FieldError message={errors.data} />
        </div>

        <div className="form-field">
          <label htmlFor="pedido-comprador">Comprador</label>
          <select
            id="pedido-comprador"
            value={values.compradorId}
            onChange={(event) =>
              atualizarValores({
                ...values,
                compradorId: event.target.value === '' ? '' : Number(event.target.value),
              })
            }
          >
            <option value="">Selecione...</option>
            {compradores.map((comprador) => (
              <option key={comprador.id} value={comprador.id}>
                {comprador.nome}
              </option>
            ))}
          </select>
          <FieldError message={errors.compradorId} />
        </div>

        <PedidoItensForm
          itens={values.itens}
          itensErrors={errors.itensDetalhes}
          erroGeral={errors.itens}
          carnes={carnes}
          onChange={(itens) => atualizarValores({ ...values, itens })}
        />

        <ApiErrorBanner errors={mutationAtiva.isError ? getErrorMessages(mutationAtiva.error) : null} />

        <div className="form-actions">
          <LoadingButton type="submit" isLoading={mutationAtiva.isPending}>
            Salvar pedido
          </LoadingButton>
          <LoadingButton type="button" onClick={() => navigate('/pedidos')} isLoading={mutationAtiva.isPending}>
            Cancelar
          </LoadingButton>
        </div>
      </form>
    </section>
  );
}
