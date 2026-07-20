import { OrigemCarne } from '../api/types';
import { isBlank } from './common';

export interface CarneFormValues {
  descricao: string;
  origem: OrigemCarne | '';
}

export interface CarneFormErrors {
  descricao?: string;
  origem?: string;
}

export const CARNE_FORM_INICIAL: CarneFormValues = {
  descricao: '',
  origem: '',
};

/** Validação client-side de Carne, espelhando a regra do backend (descrição obrigatória, origem dentre as 4 opções). */
export function validateCarneForm(values: CarneFormValues): CarneFormErrors {
  const errors: CarneFormErrors = {};

  if (isBlank(values.descricao)) {
    errors.descricao = 'Descrição é obrigatória.';
  }

  if (values.origem === '') {
    errors.origem = 'Origem é obrigatória.';
  }

  return errors;
}

export function isCarneFormValid(errors: CarneFormErrors): boolean {
  return Object.keys(errors).length === 0;
}
