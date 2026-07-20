import { afterEach } from 'vitest';
import { cleanup } from '@testing-library/react';
import '@testing-library/jest-dom/vitest';

// RTL não registra limpeza automática de DOM entre testes quando o Vitest
// não está em modo "globals" (decisão deste projeto: imports explícitos por
// arquivo de teste, sem globals implícitos) — por isso o cleanup é
// registrado manualmente aqui, uma única vez, para todos os testes.
afterEach(() => {
  cleanup();
});
