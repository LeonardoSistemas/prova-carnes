using Microsoft.AspNetCore.Mvc;
using Prova.Service.Dtos;
using Prova.Service.Services;

namespace Prova.Api.Controllers;

/// <summary>
/// Endpoints de Pedido. Depende apenas de <see cref="IPedidoService"/> —
/// nunca injeta/chama <c>ICotacaoService</c> diretamente (a cotação só é
/// buscada dentro da Service, no POST/PUT; a listagem usa a cotação já
/// persistida em <c>PedidoItem.CotacaoUsada</c>).
/// </summary>
[ApiController]
[Route("api/pedidos")]
public class PedidoController : ControllerBase
{
    private readonly IPedidoService _pedidoService;

    public PedidoController(IPedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    /// <summary>
    /// Lista pedidos. Filtros são opcionais e combináveis (AND): sem nenhum
    /// query param retorna todos os pedidos.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PedidoResponseDto>>> ObterTodos(
        [FromQuery] int? compradorId,
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim)
    {
        var pedidos = await _pedidoService.ObterTodosAsync(compradorId, dataInicio, dataFim);
        return Ok(pedidos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoResponseDto>> ObterPorId(int id)
    {
        var pedido = await _pedidoService.ObterPorIdAsync(id);

        return pedido is null ? NotFound() : Ok(pedido);
    }

    [HttpPost]
    public async Task<ActionResult<PedidoResponseDto>> Criar(PedidoDto dto)
    {
        var pedidoCriado = await _pedidoService.CriarAsync(dto);

        return CreatedAtAction(nameof(ObterPorId), new { id = pedidoCriado.Id }, pedidoCriado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, PedidoDto dto)
    {
        await _pedidoService.AtualizarAsync(id, dto);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        await _pedidoService.ExcluirAsync(id);

        return NoContent();
    }
}
