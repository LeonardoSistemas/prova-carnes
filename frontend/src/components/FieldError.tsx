interface FieldErrorProps {
  message?: string;
}

/** Mensagem de erro inline de um campo de formulário — usada por todos os formulários controlados da aplicação. */
export function FieldError({ message }: FieldErrorProps) {
  if (!message) {
    return null;
  }

  return (
    <span className="field-error" role="alert">
      {message}
    </span>
  );
}
