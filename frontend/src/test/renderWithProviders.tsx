import type { ReactElement } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import { render } from '@testing-library/react';

/**
 * Wrapper de teste único para telas que dependem de TanStack Query e de
 * rotas — evita duplicar esse boilerplate em cada arquivo de teste de
 * página. `retry: false` é essencial: sem isso, uma mutation/query que falha
 * de propósito num teste fica tentando de novo e o teste demora/trava.
 */
export function renderWithProviders(ui: ReactElement, initialRoute = '/') {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialRoute]}>{ui}</MemoryRouter>
    </QueryClientProvider>,
  );
}
