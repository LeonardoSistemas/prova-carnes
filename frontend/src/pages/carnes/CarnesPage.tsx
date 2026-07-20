import { useState } from 'react';
import toast from 'react-hot-toast';
import { Link } from 'react-router-dom';
import { getErrorMessages } from '../../api/getErrorMessages';
import type { CarneResponseDto } from '../../api/types';
import { ApiErrorBanner } from '../../components/ApiErrorBanner';
import { Breadcrumb } from '../../components/Breadcrumb';
import { ConfirmModal } from '../../components/ConfirmModal';
import { useCarnes, useDeleteCarne } from '../../hooks/useCarnes';
import { CarneTable } from './CarneTable';

export function CarnesPage() {
  const { data: carnes = [], isLoading, isError: erroAoCarregar } = useCarnes();
  const excluirCarne = useDeleteCarne();

  const [carneParaExcluir, setCarneParaExcluir] = useState<CarneResponseDto | null>(null);

  function handleConfirmarExclusao() {
    if (!carneParaExcluir) {
      return;
    }

    excluirCarne.mutate(carneParaExcluir.id, {
      onSuccess: () => {
        toast.success('Carne excluída com sucesso.');
        setCarneParaExcluir(null);
      },
      onError: () => {
        // Erro (ex.: 409 por PedidoItem vinculado) fica visível no banner
        // abaixo do modal — não fecha o modal sozinho para o usuário ver a
        // mensagem antes de decidir o próximo passo.
      },
    });
  }

  return (
    <section>
      <Breadcrumb />
      <div className="page-header">
        <h1>Carnes</h1>
        <Link to="/carnes/novo" className="button-link">
          Novo
        </Link>
      </div>

      {isLoading && <p>Carregando carnes...</p>}
      {erroAoCarregar && <ApiErrorBanner errors={['Não foi possível carregar a lista de carnes.']} />}

      {!isLoading && !erroAoCarregar && <CarneTable carnes={carnes} onDeleteRequest={setCarneParaExcluir} />}

      <ConfirmModal
        isOpen={carneParaExcluir !== null}
        title="Excluir carne"
        message={`Tem certeza que deseja excluir "${carneParaExcluir?.descricao}"?`}
        isConfirming={excluirCarne.isPending}
        errors={excluirCarne.isError ? getErrorMessages(excluirCarne.error) : null}
        onConfirm={handleConfirmarExclusao}
        onCancel={() => {
          setCarneParaExcluir(null);
          excluirCarne.reset();
        }}
      />
    </section>
  );
}
