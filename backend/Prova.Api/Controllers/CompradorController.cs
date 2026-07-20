using Microsoft.AspNetCore.Mvc;
using Prova.Service.Dtos;
using Prova.Service.Services;

namespace Prova.Api.Controllers;

/// <summary>
/// Endpoints de Comprador. Mesma decisão de <see cref="CarneController"/>:
/// nenhuma regra de negócio aqui, exceções de domínio tratadas pelo
/// middleware global.
/// </summary>
[ApiController]
[Route("api/compradores")]
public class CompradorController : ControllerBase
{
    private readonly ICompradorService _compradorService;

    public CompradorController(ICompradorService compradorService)
    {
        _compradorService = compradorService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CompradorResponseDto>>> ObterTodos()
    {
        var compradores = await _compradorService.ObterTodosAsync();
        return Ok(compradores);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompradorResponseDto>> ObterPorId(int id)
    {
        var comprador = await _compradorService.ObterPorIdAsync(id);

        return comprador is null ? NotFound() : Ok(comprador);
    }

    [HttpPost]
    public async Task<ActionResult<CompradorResponseDto>> Criar(CompradorDto dto)
    {
        var compradorCriado = await _compradorService.CriarAsync(dto);

        return CreatedAtAction(nameof(ObterPorId), new { id = compradorCriado.Id }, compradorCriado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, CompradorDto dto)
    {
        await _compradorService.AtualizarAsync(id, dto);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        await _compradorService.ExcluirAsync(id);

        return NoContent();
    }
}
