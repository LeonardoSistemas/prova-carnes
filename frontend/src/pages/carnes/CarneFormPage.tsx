import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import toast from 'react-hot-toast';
import { useNavigate, useParams } from 'react-router-dom';
import type { OrigemCarne } from '../../api/types';
import { getErrorMessages } from '../../api/getErrorMessages';
import { Breadcrumb } from '../../components/Breadcrumb';
import { FieldError } from '../../components/FieldError';
import { LoadingButton } from '../../components/LoadingButton';
import { useCarne, useCreateCarne, useUpdateCarne } from '../../hooks/useCarnes';
import {
  CARNE_FORM_INICIAL,
  isCarneFormValid,
  validateCarneForm,
  type CarneFormErrors,
  type CarneFormValues,
} from '../../validation/validateCarneForm';

export function CarneFormPage() {
  const { id } = useParams<{ id?: string }>();
  const carneId = id ? Number(id) : undefined;
  const modoEdicao = carneId !== undefined;

  const navigate = useNavigate();

  const { data: carneExistente, isLoading: carregandoCarne } = useCarne(carneId);

  const criarCarne = useCreateCarne();
  const atualizarCarne = useUpdateCarne();
  const mutationAtiva = modoEdicao ? atualizarCarne : criarCarne;

  const [values, setValues] = useState<CarneFormValues>(CARNE_FORM_INICIAL);
  const [errors, setErrors] = useState<CarneFormErrors>({});
  const [foiSubmetido, setFoiSubmetido] = useState(false);

  useEffect(() => {
    if (carneExistente) {
      setValues({
        descricao: carneExistente.descricao,
        origem: carneExistente.origem,
      });
    }
  }, [carneExistente]);

  function atualizarValores(proximosValues: CarneFormValues) {
    setValues(proximosValues);
    if (foiSubmetido) {
      setErrors(validateCarneForm(proximosValues));
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFoiSubmetido(true);

    const errosValidacao = validateCarneForm(values);
    setErrors(errosValidacao);

    if (!isCarneFormValid(errosValidacao)) {
      return;
    }

    const dto = { descricao: values.descricao.trim(), origem: values.origem as OrigemCarne };

    if (modoEdicao) {
      atualizarCarne.mutate(
        { id: carneId as number, dto },
        {
          onSuccess: () => {
            toast.success('Carne atualizada com sucesso.');
            navigate('/carnes');
          },
          onError: (error) => {
            toast.error(getErrorMessages(error).join(' '));
          },
        },
      );
      return;
    }

    criarCarne.mutate(dto, {
      onSuccess: () => {
        toast.success('Carne cadastrada com sucesso.');
        navigate('/carnes');
      },
      onError: (error) => {
        toast.error(getErrorMessages(error).join(' '));
      },
    });
  }

  if (modoEdicao && carregandoCarne) {
    return <p>Carregando carne...</p>;
  }

  return (
    <section>
      <Breadcrumb />
      <h1>{modoEdicao ? `Editar carne #${carneId}` : 'Nova carne'}</h1>

      <form onSubmit={handleSubmit} noValidate>
        <div className="form-field">
          <label htmlFor="carne-descricao">
            Descrição <span style={{ color: 'var(--danger)' }}>*</span>
          </label>
          <input
            id="carne-descricao"
            type="text"
            value={values.descricao}
            onChange={(event) => atualizarValores({ ...values, descricao: event.target.value })}
            autoFocus
          />
          <FieldError message={errors.descricao} />
        </div>

        <div className="form-field">
          <label htmlFor="carne-origem">
            Origem <span style={{ color: 'var(--danger)' }}>*</span>
          </label>
          <select
            id="carne-origem"
            value={values.origem}
            onChange={(event) =>
              atualizarValores({
                ...values,
                origem: event.target.value === '' ? ('' as const) : (Number(event.target.value) as OrigemCarne),
              })
            }
          >
            <option value="">Selecione...</option>
            <option value={0}>Bovina</option>
            <option value={1}>Suína</option>
            <option value={2}>Frango</option>
            <option value={3}>Outro</option>
          </select>
          <FieldError message={errors.origem} />
        </div>

        <div className="form-actions">
          <LoadingButton type="submit" isLoading={mutationAtiva.isPending}>
            {modoEdicao ? 'Salvar alterações' : 'Cadastrar carne'}
          </LoadingButton>
          <LoadingButton type="button" onClick={() => navigate('/carnes')} isLoading={mutationAtiva.isPending}>
            Cancelar
          </LoadingButton>
        </div>
      </form>
    </section>
  );
}
