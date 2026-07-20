import { describe, expect, it } from 'vitest';
import { isCompradorFormValid, validateCompradorForm } from './validateCompradorForm';

describe('validateCompradorForm', () => {
  it('exige nome preenchido', () => {
    const errors = validateCompradorForm({ nome: '', documento: '123', estadoId: 1, cidadeId: 1 });

    expect(errors.nome).toBe('Nome é obrigatório.');
  });

  it('exige documento preenchido', () => {
    const errors = validateCompradorForm({ nome: 'Fulano', documento: '', estadoId: 1, cidadeId: 1 });

    expect(errors.documento).toBe('Documento é obrigatório.');
  });

  it('exige estado selecionado', () => {
    const errors = validateCompradorForm({ nome: 'Fulano', documento: '123', estadoId: '', cidadeId: '' });

    expect(errors.estadoId).toBe('Estado é obrigatório.');
  });

  it('exige cidade selecionada', () => {
    const errors = validateCompradorForm({ nome: 'Fulano', documento: '123', estadoId: 1, cidadeId: '' });

    expect(errors.cidadeId).toBe('Cidade é obrigatória.');
  });

  it('não retorna erros quando o formulário é válido', () => {
    const errors = validateCompradorForm({ nome: 'Fulano', documento: '123', estadoId: 1, cidadeId: 2 });

    expect(isCompradorFormValid(errors)).toBe(true);
  });
});
