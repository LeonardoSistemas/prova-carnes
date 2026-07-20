import { useState } from 'react';
import toast from 'react-hot-toast';
import { Link } from 'react-router-dom';
import { getErrorMessages } from '../../api/getErrorMessages';
import type { CompradorResponseDto } from '../../api/types';
import { ApiErrorBanner } from '../../components/ApiErrorBanner';
import { Breadcrumb } from '../../components/Breadcrumb';
import { ConfirmModal } from '../../components/ConfirmModal';
import { useCompradores, useDeleteComprador } from '../../hooks/useCompradores';
import { useEstados } from '../../hooks/useEstados';
import { CompradorTable } from './CompradorTable';

export function CompradoresPage() {
  const { data: compradores = [], isLoading, isError: erroAoCarregar } = useCompradores();
  const { data: estados = [], isLoading: carregandoEstados } = useEstados();
  const excluirComprador = useDeleteComprador();

  const [compradorParaExcluir, setCompradorParaExcluir] = useState<CompradorResponseDto | null>(null);

  function handleConfirmarExclusao() {
    if (!compradorParaExcluir) {
      return;
    }

    excluirComprador.mutate(compradorParaExcluir.id, {
      onSuccess: () => {
        toast.success('Comprador excluído com sucesso.');
        setCompradorParaExcluir(null);
      },
      onError: () => {
        // Erro (ex.: 409 por Pedido vinculado) fica visível no banner
        // abaixo do modal — não fecha o modal sozinho para o usuário ver a
        // mensagem antes de decidir o próximo passo.
      },
    });
  }

  return (
    <section>
      <Breadcrumb />
      <div className="page-header">
        <h1>Compradores</h1>
        <Link to="/compradores/novo" className="button-link">
          Novo
        </Link>
      </div>

      {isLoading && <p>Carregando compradores...</p>}
      {erroAoCarregar && <ApiErrorBanner errors={['Não foi possível carregar a lista de compradores.']} />}

      {!isLoading && !erroAoCarregar && !carregandoEstados && (
        <CompradorTable compradores={compradores} estados={estados} onDeleteRequest={setCompradorParaExcluir} />
      )}

      <ConfirmModal
        isOpen={compradorParaExcluir !== null}
        title="Excluir comprador"
        message={`Tem certeza que deseja excluir "${compradorParaExcluir?.nome}"?`}
        isConfirming={excluirComprador.isPending}
        errors={excluirComprador.isError ? getErrorMessages(excluirComprador.error) : null}
        onConfirm={handleConfirmarExclusao}
        onCancel={() => {
          setCompradorParaExcluir(null);
          excluirComprador.reset();
        }}
      />
    </section>
  );
}
