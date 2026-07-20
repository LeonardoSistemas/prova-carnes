using FluentValidation;
using Prova.Service.Dtos;

namespace Prova.Service.Validators;

/// <summary>Validação estrutural de cada item de um Pedido.</summary>
public class PedidoItemDtoValidator : AbstractValidator<PedidoItemDto>
{
    public PedidoItemDtoValidator()
    {
        RuleFor(i => i.CarneId)
            .GreaterThan(0)
            .WithMessage("Carne do item é obrigatória.");

        RuleFor(i => i.Preco)
            .GreaterThan(0m)
            .WithMessage("Preço do item deve ser maior que zero.");

        RuleFor(i => i.Moeda)
            .IsInEnum()
            .WithMessage("Moeda do item inválida.");
    }
}
