import { Moeda, OrigemCarne } from '../api/types';

/** Rótulos de exibição dos enums — único lugar que traduz o valor numérico para texto amigável. */
export const ORIGEM_CARNE_LABELS: Record<OrigemCarne, string> = {
  [OrigemCarne.Bovina]: 'Bovina',
  [OrigemCarne.Suina]: 'Suína',
  [OrigemCarne.Aves]: 'Aves',
  [OrigemCarne.Peixes]: 'Peixes',
};

export const MOEDA_LABELS: Record<Moeda, string> = {
  [Moeda.BRL]: 'BRL - Real',
  [Moeda.USD]: 'USD - Dólar',
  [Moeda.EUR]: 'EUR - Euro',
};

export const ORIGEM_CARNE_OPCOES = Object.values(OrigemCarne).filter(
  (valor): valor is OrigemCarne => typeof valor === 'number',
);

export const MOEDA_OPCOES = Object.values(Moeda).filter((valor): valor is Moeda => typeof valor === 'number');
