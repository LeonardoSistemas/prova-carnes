/** Formatação de valores monetários e datas — única fonte de formatação, evita duplicar `Intl.NumberFormat` pela UI. */
export function formatarReal(valor: number): string {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor);
}

export function formatarDataBR(isoDate: string): string {
  // Extrai só a parte yyyy-MM-dd e monta a data em UTC explicitamente —
  // `new Date(isoDate)` interpretaria uma string sem sufixo de timezone
  // (ex.: "2026-07-18T00:00:00", vinda de um DateTime sem Kind explícito no
  // backend) como hora LOCAL do navegador, o que desalinha com a exibição
  // forçada em UTC abaixo e pode mostrar um dia a menos em fusos com offset
  // positivo em relação a UTC.
  const [ano, mes, dia] = isoDate.slice(0, 10).split('-').map(Number);
  if (!Number.isInteger(ano) || !Number.isInteger(mes) || !Number.isInteger(dia)) {
    return isoDate;
  }
  const data = new Date(Date.UTC(ano, mes - 1, dia));
  return new Intl.DateTimeFormat('pt-BR', { timeZone: 'UTC' }).format(data);
}

/** Converte uma data ISO (vinda da API) para o formato aceito por `<input type="date">` (yyyy-MM-dd). */
export function paraInputDate(isoDate: string): string {
  return isoDate.slice(0, 10);
}
