import { useState } from 'react';
import toast from 'react-hot-toast';
import { Link } from 'react-router-dom';
import { getErrorMessages } from '../../api/getErrorMessages';
import type { PedidoResponseDto } from '../../api/types';
import { ApiErrorBanner } from '../../components/ApiErrorBanner';
import { Breadcrumb } from '../../components/Breadcrumb';
import { ConfirmModal } from '../../components/ConfirmModal';
import { FieldError } from '../../components/FieldError';
import { useCompradores } from '../../hooks/useCompradores';
import { useDeletePedido, usePedidos } from '../../hooks/usePedidos';
import {
  PEDIDO_FILTRO_INICIAL,
  intervaloDeDatasInvalido,
  paraFiltroApi,
  type PedidoFiltroFormValues,
} from '../../validation/validatePedidoFiltro';
import { PedidoTable } from './PedidoTable';

export function PedidosListPage() {
  const [filtro, setFiltro] = useState<PedidoFiltroFormValues>(PEDIDO_FILTRO_INICIAL);
  const erroIntervaloDatas = intervaloDeDatasInvalido(filtro);

  const { data: pedidos = [], isLoading, isError: erroAoCarregar } = usePedidos(paraFiltroApi(filtro));
  const { data: compradores = [], isLoading: carregandoCompradores } = useCompradores();
  const excluirPedido = useDeletePedido();

  const [pedidoParaExcluir, setPedidoParaExcluir] = useState<PedidoResponseDto | null>(null);

  function handleConfirmarExclusao() {
    if (!pedidoParaExcluir) {
      return;
    }

    excluirPedido.mutate(pedidoParaExcluir.id, {
      onSuccess: () => {
        toast.success('Pedido excluído com sucesso.');
        setPedidoParaExcluir(null);
      },
    });
  }

  const carregando = isLoading || carregandoCompradores;

  function handleLimparFiltro() {
    setFiltro(PEDIDO_FILTRO_INICIAL);
  }

  return (
    <section>
      <Breadcrumb />
      <div className="page-header">
        <h1>Pedidos</h1>
        <Link to="/pedidos/novo" className="button-link">
          Novo pedido
        </Link>
      </div>

      <div className="filtro-pedidos">
        <div className="form-field">
          <label htmlFor="filtro-comprador">Comprador</label>
          <select
            id="filtro-comprador"
            value={filtro.compradorId}
            onChange={(evento) =>
              setFiltro((atual) => ({
                ...atual,
                compradorId: evento.target.value === '' ? '' : Number(evento.target.value),
              }))
            }
          >
            <option value="">Todos</option>
            {compradores.map((comprador) => (
              <option key={comprador.id} value={comprador.id}>
                {comprador.nome}
              </option>
            ))}
          </select>
        </div>

        <div className="form-field">
          <label htmlFor="filtro-data-inicio">Data início</label>
          <input
            id="filtro-data-inicio"
            type="date"
            value={filtro.dataInicio}
            onChange={(evento) => setFiltro((atual) => ({ ...atual, dataInicio: evento.target.value }))}
          />
        </div>

        <div className="form-field">
          <label htmlFor="filtro-data-fim">Data fim</label>
          <input
            id="filtro-data-fim"
            type="date"
            value={filtro.dataFim}
            onChange={(evento) => setFiltro((atual) => ({ ...atual, dataFim: evento.target.value }))}
          />
        </div>

        <button type="button" onClick={handleLimparFiltro}>
          Limpar filtro
        </button>

        {erroIntervaloDatas && <FieldError message="Data início não pode ser posterior à data fim." />}
      </div>

      {carregando && <p>Carregando pedidos...</p>}
      {erroAoCarregar && <ApiErrorBanner errors={['Não foi possível carregar a lista de pedidos.']} />}

      {!carregando && !erroAoCarregar && (
        <PedidoTable pedidos={pedidos} compradores={compradores} onDeleteRequest={setPedidoParaExcluir} />
      )}

      <ConfirmModal
        isOpen={pedidoParaExcluir !== null}
        title="Excluir pedido"
        message={`Tem certeza que deseja excluir o pedido #${pedidoParaExcluir?.id}?`}
        isConfirming={excluirPedido.isPending}
        errors={excluirPedido.isError ? getErrorMessages(excluirPedido.error) : null}
        onConfirm={handleConfirmarExclusao}
        onCancel={() => {
          setPedidoParaExcluir(null);
          excluirPedido.reset();
        }}
      />
    </section>
  );
}
