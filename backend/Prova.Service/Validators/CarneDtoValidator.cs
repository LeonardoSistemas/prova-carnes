using FluentValidation;
using Prova.Service.Dtos;

namespace Prova.Service.Validators;

/// <summary>
/// Validação estrutural/de formato de <see cref="CarneDto"/>. Regras que
/// dependem de acesso a dados (ex.: existência de referência) ficam na
/// Service, não aqui — separação entre "formato válido" (FluentValidation) e
/// "regra de negócio que depende do banco" (Service, com exceções de
/// domínio próprias).
/// </summary>
public class CarneDtoValidator : AbstractValidator<CarneDto>
{
    public CarneDtoValidator()
    {
        RuleFor(c => c.Descricao)
            .NotEmpty()
            .WithMessage("Descrição da carne é obrigatória.")
            .MaximumLength(200);

        RuleFor(c => c.Origem)
            .IsInEnum()
            .WithMessage("Origem da carne inválida.");
    }
}
