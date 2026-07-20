import { describe, it, expect } from 'vitest';
import { formatarCpf, removerMascaraCpf } from './cpf';

describe('formatarCpf', () => {
  it('deve retornar string vazia para entrada vazia', () => {
    expect(formatarCpf('')).toBe('');
  });

  it('deve formatar números digitados progressivamente', () => {
    expect(formatarCpf('1')).toBe('1');
    expect(formatarCpf('12')).toBe('12');
    expect(formatarCpf('123')).toBe('123');
    expect(formatarCpf('1234')).toBe('123.4');
    expect(formatarCpf('12345')).toBe('123.45');
    expect(formatarCpf('123456')).toBe('123.456');
    expect(formatarCpf('1234567')).toBe('123.456.7');
    expect(formatarCpf('12345678')).toBe('123.456.78');
    expect(formatarCpf('123456789')).toBe('123.456.789');
    expect(formatarCpf('1234567890')).toBe('123.456.789-0');
    expect(formatarCpf('12345678900')).toBe('123.456.789-00');
  });

  it('deve aceitar CPF já formatado e reformatar', () => {
    expect(formatarCpf('123.456.789-00')).toBe('123.456.789-00');
  });

  it('deve aceitar CPF sem máscara e formatar', () => {
    expect(formatarCpf('12345678900')).toBe('123.456.789-00');
  });

  it('deve remover caracteres não-dígito e formatar', () => {
    expect(formatarCpf('123abc456def789ghi00')).toBe('123.456.789-00');
  });

  it('deve limitar a 11 dígitos', () => {
    expect(formatarCpf('123456789001234')).toBe('123.456.789-00');
  });

  it('deve lidar com espaços e caracteres especiais', () => {
    expect(formatarCpf('123 456 789 00')).toBe('123.456.789-00');
    expect(formatarCpf('123-456-789-00')).toBe('123.456.789-00');
  });
});

describe('removerMascaraCpf', () => {
  it('deve remover máscara de CPF formatado', () => {
    expect(removerMascaraCpf('123.456.789-00')).toBe('12345678900');
  });

  it('deve retornar apenas dígitos de um CPF sem máscara', () => {
    expect(removerMascaraCpf('12345678900')).toBe('12345678900');
  });

  it('deve remover caracteres especiais e retornar apenas dígitos', () => {
    expect(removerMascaraCpf('123abc456def789ghi00')).toBe('12345678900');
  });

  it('deve retornar string vazia para entrada vazia', () => {
    expect(removerMascaraCpf('')).toBe('');
  });

  it('deve lidar com espaços e outros caracteres especiais', () => {
    expect(removerMascaraCpf('123 456 789 00')).toBe('12345678900');
    expect(removerMascaraCpf('123-456-789-00')).toBe('12345678900');
  });
});
