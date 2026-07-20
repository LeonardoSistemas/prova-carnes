using Microsoft.AspNetCore.Mvc;
using Prova.Service.Dtos;
using Prova.Service.Services;

namespace Prova.Api.Controllers;

/// <summary>
/// Endpoints de Carne. Nenhuma regra de negócio aqui — apenas tradução
/// HTTP↔Service. Exceções de domínio lançadas pela <see cref="ICarneService"/>
/// (EntidadeNaoEncontradaException, EntidadeVinculadaException, exceções de
/// validação do FluentValidation) são tratadas pelo middleware global de
/// exceção (ver <c>Program.cs</c>), nunca por try/catch aqui.
/// </summary>
[ApiController]
[Route("api/carnes")]
public class CarneController : ControllerBase
{
    private readonly ICarneService _carneService;

    public CarneController(ICarneService carneService)
    {
        _carneService = carneService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CarneResponseDto>>> ObterTodas()
    {
        var carnes = await _carneService.ObterTodasAsync();
        return Ok(carnes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CarneResponseDto>> ObterPorId(int id)
    {
        var carne = await _carneService.ObterPorIdAsync(id);

        return carne is null ? NotFound() : Ok(carne);
    }

    [HttpPost]
    public async Task<ActionResult<CarneResponseDto>> Criar(CarneDto dto)
    {
        var carneCriada = await _carneService.CriarAsync(dto);

        return CreatedAtAction(nameof(ObterPorId), new { id = carneCriada.Id }, carneCriada);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, CarneDto dto)
    {
        await _carneService.AtualizarAsync(id, dto);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        await _carneService.ExcluirAsync(id);

        return NoContent();
    }
}
