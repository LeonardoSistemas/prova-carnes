using FluentValidation;
using Prova.Service.Dtos;

namespace Prova.Service.Validators;

/// <summary>
/// Validação estrutural de <see cref="PedidoDto"/>: exige ao menos um item
/// (regra do PRD) e delega a validação de cada item para
/// <see cref="PedidoItemDtoValidator"/>. Existência de Comprador/Carne no
/// banco é responsabilidade da <see cref="Services.PedidoService"/>.
/// </summary>
public class PedidoDtoValidator : AbstractValidator<PedidoDto>
{
    public PedidoDtoValidator()
    {
        RuleFor(p => p.CompradorId)
            .GreaterThan(0)
            .WithMessage("Comprador do pedido é obrigatório.");

        RuleFor(p => p.Itens)
            .NotEmpty()
            .WithMessage("O pedido deve ter ao menos um item.");

        RuleForEach(p => p.Itens)
            .SetValidator(new PedidoItemDtoValidator());
    }
}
