import type { ButtonHTMLAttributes } from 'react';
import './LoadingButton.css';

interface LoadingButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  isLoading?: boolean;
}

/**
 * Botão reutilizável com indicador visual de loading (spinner CSS).
 *
 * Quando `isLoading` é true:
 * - O botão fica automaticamente `disabled`
 * - Um spinner é exibido ao lado/no lugar do texto
 * - O conteúdo do botão é ocultado visualmente
 *
 * @example
 * <LoadingButton isLoading={isPending} type="submit">
 *   Salvar
 * </LoadingButton>
 */
export function LoadingButton({ isLoading = false, disabled, children, ...props }: LoadingButtonProps) {
  const isActuallyDisabled = isLoading || disabled;

  return (
    <button {...props} disabled={isActuallyDisabled} className={`loading-button ${isLoading ? 'loading-button--loading' : ''} ${props.className || ''}`}>
      {isLoading && <span className="loading-button__spinner" aria-hidden="true" />}
      <span className={`loading-button__text ${isLoading ? 'loading-button__text--hidden' : ''}`}>
        {children}
      </span>
    </button>
  );
}
