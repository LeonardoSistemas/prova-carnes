import { isBlank } from './common';

export interface CompradorFormValues {
  nome: string;
  documento: string;
  estadoId: number | '';
  cidadeId: number | '';
}

export interface CompradorFormErrors {
  nome?: string;
  documento?: string;
  estadoId?: string;
  cidadeId?: string;
}

export const COMPRADOR_FORM_INICIAL: CompradorFormValues = {
  nome: '',
  documento: '',
  estadoId: '',
  cidadeId: '',
};

/**
 * Validação client-side de Comprador. `estadoId` só existe no formulário
 * para viabilizar o combobox em cascata — o backend não recebe esse campo
 * (só `cidadeId`), mas ainda assim é obrigatório selecioná-lo na UI porque é
 * o único jeito de restringir a lista de Cidade a um Estado.
 */
export function validateCompradorForm(values: CompradorFormValues): CompradorFormErrors {
  const errors: CompradorFormErrors = {};

  if (isBlank(values.nome)) {
    errors.nome = 'Nome é obrigatório.';
  }

  if (isBlank(values.documento)) {
    errors.documento = 'Documento é obrigatório.';
  }

  if (values.estadoId === '') {
    errors.estadoId = 'Estado é obrigatório.';
  }

  if (values.cidadeId === '') {
    errors.cidadeId = 'Cidade é obrigatória.';
  }

  return errors;
}

export function isCompradorFormValid(errors: CompradorFormErrors): boolean {
  return Object.keys(errors).length === 0;
}
