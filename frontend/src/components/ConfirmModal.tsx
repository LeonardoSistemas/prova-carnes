import { ApiErrorBanner } from './ApiErrorBanner';
import { LoadingButton } from './LoadingButton';

interface ConfirmModalProps {
  isOpen: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  isConfirming?: boolean;
  /** Mensagens de erro da última tentativa de confirmação (ex.: 409 por vínculo) — exibidas dentro do próprio modal. */
  errors?: string[] | null;
  onConfirm: () => void;
  onCancel: () => void;
}

/**
 * Modal de confirmação único, reutilizado pelas 3 telas antes de qualquer
 * exclusão. Centralizar aqui evita duplicar o mesmo componente/lógica em Carnes,
 * Compradores e Pedidos.
 */
export function ConfirmModal({
  isOpen,
  title,
  message,
  confirmLabel = 'Confirmar',
  cancelLabel = 'Cancelar',
  isConfirming = false,
  errors,
  onConfirm,
  onCancel,
}: ConfirmModalProps) {
  if (!isOpen) {
    return null;
  }

  return (
    <div className="modal-overlay" role="presentation" onClick={onCancel}>
      <div
        className="modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="confirm-modal-title"
        onClick={(event) => event.stopPropagation()}
      >
        <h2 id="confirm-modal-title">{title}</h2>
        <p>{message}</p>
        <ApiErrorBanner errors={errors} />
        <div className="modal-actions">
          <LoadingButton type="button" onClick={onCancel} isLoading={isConfirming}>
            {cancelLabel}
          </LoadingButton>
          <LoadingButton type="button" className="danger" onClick={onConfirm} isLoading={isConfirming}>
            {confirmLabel}
          </LoadingButton>
        </div>
      </div>
    </div>
  );
}
