import { describe, expect, it } from 'vitest';
import { getErrorMessages } from './getErrorMessages';
import { ApiError } from './client';

describe('getErrorMessages', () => {
  it('retorna o array de erros quando erro é ApiError', () => {
    const erros = ['Erro 1', 'Erro 2', 'Erro 3'];
    const error = new ApiError(422, erros);

    const resultado = getErrorMessages(error);

    expect(resultado).toEqual(erros);
  });

  it('retorna apenas uma mensagem quando ApiError tem um único erro', () => {
    const error = new ApiError(404, ['Recurso não encontrado.']);

    const resultado = getErrorMessages(error);

    expect(resultado).toEqual(['Recurso não encontrado.']);
  });

  it('retorna mensagem genérica quando erro não é ApiError', () => {
    const error = new Error('Algo deu errado');

    const resultado = getErrorMessages(error);

    expect(resultado).toEqual(['Ocorreu um erro inesperado. Tente novamente.']);
  });

  it('retorna mensagem genérica quando erro é null', () => {
    const resultado = getErrorMessages(null);

    expect(resultado).toEqual(['Ocorreu um erro inesperado. Tente novamente.']);
  });

  it('retorna mensagem genérica quando erro é undefined', () => {
    const resultado = getErrorMessages(undefined);

    expect(resultado).toEqual(['Ocorreu um erro inesperado. Tente novamente.']);
  });

  it('retorna mensagem genérica quando erro é um valor primitivo', () => {
    expect(getErrorMessages('string')).toEqual(['Ocorreu um erro inesperado. Tente novamente.']);
    expect(getErrorMessages(123)).toEqual(['Ocorreu um erro inesperado. Tente novamente.']);
    expect(getErrorMessages(true)).toEqual(['Ocorreu um erro inesperado. Tente novamente.']);
  });

  it('retorna mensagem genérica quando erro é um objeto que não é ApiError', () => {
    const error = { mensagem: 'Erro customizado' };

    const resultado = getErrorMessages(error);

    expect(resultado).toEqual(['Ocorreu um erro inesperado. Tente novamente.']);
  });

  it('preserva a mensagem da API mesmo com caracteres especiais', () => {
    const erros = ['Não foi possível & desconectar < > "quotas"'];
    const error = new ApiError(500, erros);

    const resultado = getErrorMessages(error);

    expect(resultado).toEqual(erros);
  });

  it('retorna array com um elemento (nunca vazio ou falsy)', () => {
    const error = new Error('Erro genérico');

    const resultado = getErrorMessages(error);

    expect(Array.isArray(resultado)).toBe(true);
    expect(resultado.length).toBeGreaterThan(0);
  });
});
