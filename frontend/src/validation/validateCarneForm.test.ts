import { describe, expect, it } from 'vitest';
import { OrigemCarne } from '../api/types';
import { isCarneFormValid, validateCarneForm } from './validateCarneForm';

describe('validateCarneForm', () => {
  it('exige descrição preenchida', () => {
    const errors = validateCarneForm({ descricao: '', origem: OrigemCarne.Bovina });

    expect(errors.descricao).toBe('Descrição é obrigatória.');
  });

  it('considera descrição só com espaços como vazia', () => {
    const errors = validateCarneForm({ descricao: '   ', origem: OrigemCarne.Bovina });

    expect(errors.descricao).toBe('Descrição é obrigatória.');
  });

  it('exige origem selecionada', () => {
    const errors = validateCarneForm({ descricao: 'Picanha', origem: '' });

    expect(errors.origem).toBe('Origem é obrigatória.');
  });

  it('não retorna erros quando o formulário é válido', () => {
    const errors = validateCarneForm({ descricao: 'Picanha', origem: OrigemCarne.Bovina });

    expect(isCarneFormValid(errors)).toBe(true);
  });

  it('isCarneFormValid retorna false quando há erros', () => {
    const errors = validateCarneForm({ descricao: '', origem: '' });

    expect(isCarneFormValid(errors)).toBe(false);
  });
});
