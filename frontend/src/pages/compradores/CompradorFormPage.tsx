import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import toast from 'react-hot-toast';
import { useNavigate, useParams } from 'react-router-dom';
import { getErrorMessages } from '../../api/getErrorMessages';
import { Breadcrumb } from '../../components/Breadcrumb';
import { FieldError } from '../../components/FieldError';
import { LoadingButton } from '../../components/LoadingButton';
import { formatarCpf, removerMascaraCpf } from '../../format/cpf';
import { encontrarEstadoDaCidade } from '../../format/enderecoLookup';
import {
  useComprador,
  useCreateComprador,
  useUpdateComprador,
} from '../../hooks/useCompradores';
import { useEstados } from '../../hooks/useEstados';
import {
  COMPRADOR_FORM_INICIAL,
  isCompradorFormValid,
  validateCompradorForm,
  type CompradorFormErrors,
  type CompradorFormValues,
} from '../../validation/validateCompradorForm';

export function CompradorFormPage() {
  const { id } = useParams<{ id?: string }>();
  const compradorId = id ? Number(id) : undefined;
  const modoEdicao = compradorId !== undefined;

  const navigate = useNavigate();

  const { data: compradorExistente, isLoading: carregandoComprador } = useComprador(compradorId);
  const { data: estados = [], isLoading: carregandoEstados } = useEstados();

  const criarComprador = useCreateComprador();
  const atualizarComprador = useUpdateComprador();
  const mutationAtiva = modoEdicao ? atualizarComprador : criarComprador;

  const [values, setValues] = useState<CompradorFormValues>(COMPRADOR_FORM_INICIAL);
  const [errors, setErrors] = useState<CompradorFormErrors>({});
  const [foiSubmetido, setFoiSubmetido] = useState(false);

  useEffect(() => {
    if (compradorExistente) {
      setValues({
        nome: compradorExistente.nome,
        documento: formatarCpf(compradorExistente.documento),
        estadoId: encontrarEstadoDaCidade(estados, compradorExistente.cidadeId)?.id ?? '',
        cidadeId: compradorExistente.cidadeId,
      });
    }
  }, [compradorExistente, estados]);

  function atualizarValores(proximosValues: CompradorFormValues) {
    setValues(proximosValues);
    if (foiSubmetido) {
      setErrors(validateCompradorForm(proximosValues));
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFoiSubmetido(true);

    const errosValidacao = validateCompradorForm(values);
    setErrors(errosValidacao);

    if (!isCompradorFormValid(errosValidacao)) {
      return;
    }

    const dto = {
      nome: values.nome.trim(),
      documento: removerMascaraCpf(values.documento.trim()),
      cidadeId: values.cidadeId as number,
    };

    if (modoEdicao) {
      atualizarComprador.mutate(
        { id: compradorId as number, dto },
        {
          onSuccess: () => {
            toast.success('Comprador atualizado com sucesso.');
            navigate('/compradores');
          },
          onError: (error) => {
            toast.error(getErrorMessages(error).join(' '));
          },
        },
      );
      return;
    }

    criarComprador.mutate(dto, {
      onSuccess: () => {
        toast.success('Comprador cadastrado com sucesso.');
        navigate('/compradores');
      },
      onError: (error) => {
        toast.error(getErrorMessages(error).join(' '));
      },
    });
  }

  const estadoSelecionado = estados.find((estado) => estado.id === values.estadoId);
  const cidadesDisponiveis = estadoSelecionado?.cidades ?? [];

  function handleEstadoChange(estadoTexto: string) {
    atualizarValores({
      ...values,
      estadoId: estadoTexto === '' ? '' : Number(estadoTexto),
      cidadeId: '',
    });
  }

  if ((modoEdicao && carregandoComprador) || carregandoEstados) {
    return <p>Carregando...</p>;
  }

  return (
    <section>
      <Breadcrumb />
      <h1>{modoEdicao ? `Editar comprador #${compradorId}` : 'Novo comprador'}</h1>

      <form onSubmit={handleSubmit} noValidate>
        <div className="form-field">
          <label htmlFor="comprador-nome">
            Nome <span style={{ color: 'var(--danger)' }}>*</span>
          </label>
          <input
            id="comprador-nome"
            type="text"
            placeholder="Nome completo ou razão social"
            value={values.nome}
            onChange={(event) => atualizarValores({ ...values, nome: event.target.value })}
            autoFocus
          />
          <FieldError message={errors.nome} />
        </div>

        <div className="form-field">
          <label htmlFor="comprador-documento">
            Documento <span style={{ color: 'var(--danger)' }}>*</span>
          </label>
          <input
            id="comprador-documento"
            type="text"
            placeholder="000.000.000-00"
            value={values.documento}
            onChange={(event) =>
              atualizarValores({
                ...values,
                documento: formatarCpf(event.target.value),
              })
            }
          />
          <FieldError message={errors.documento} />
        </div>

        <div className="form-field">
          <label htmlFor="comprador-estado">
            Estado <span style={{ color: 'var(--danger)' }}>*</span>
          </label>
          <select
            id="comprador-estado"
            value={values.estadoId}
            onChange={(event) => handleEstadoChange(event.target.value)}
          >
            <option value="">Selecione...</option>
            {estados.map((estado) => (
              <option key={estado.id} value={estado.id}>
                {estado.nome} ({estado.uf})
              </option>
            ))}
          </select>
          <FieldError message={errors.estadoId} />
        </div>

        <div className="form-field">
          <label htmlFor="comprador-cidade">
            Cidade <span style={{ color: 'var(--danger)' }}>*</span>
          </label>
          <select
            id="comprador-cidade"
            value={values.cidadeId}
            disabled={!estadoSelecionado}
            onChange={(event) =>
              atualizarValores({ ...values, cidadeId: event.target.value === '' ? '' : Number(event.target.value) })
            }
          >
            <option value="">Selecione...</option>
            {cidadesDisponiveis.map((cidade) => (
              <option key={cidade.id} value={cidade.id}>
                {cidade.nome}
              </option>
            ))}
          </select>
          <FieldError message={errors.cidadeId} />
        </div>

        <div className="form-actions">
          <LoadingButton type="submit" isLoading={mutationAtiva.isPending}>
            {modoEdicao ? 'Salvar alterações' : 'Cadastrar comprador'}
          </LoadingButton>
          <LoadingButton type="button" onClick={() => navigate('/compradores')} isLoading={mutationAtiva.isPending}>
            Cancelar
          </LoadingButton>
        </div>
      </form>
    </section>
  );
}
