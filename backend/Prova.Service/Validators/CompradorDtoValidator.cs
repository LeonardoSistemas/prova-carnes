using FluentValidation;
using Prova.Service.Dtos;

namespace Prova.Service.Validators;

/// <summary>
/// Validação estrutural de <see cref="CompradorDto"/>. A existência de
/// <c>CidadeId</c> no banco é regra de negócio (depende de I/O) e é checada
/// na <see cref="Services.CompradorService"/>, não aqui.
/// </summary>
public class CompradorDtoValidator : AbstractValidator<CompradorDto>
{
    public CompradorDtoValidator()
    {
        RuleFor(c => c.Nome)
            .NotEmpty()
            .WithMessage("Nome do comprador é obrigatório.")
            .MaximumLength(150);

        RuleFor(c => c.Documento)
            .NotEmpty()
            .WithMessage("Documento do comprador é obrigatório.")
            .MaximumLength(20);

        RuleFor(c => c.CidadeId)
            .GreaterThan(0)
            .WithMessage("Cidade do comprador é obrigatória.");
    }
}
