using System.Net;
using System.Net.Http.Json;
using Prova.Api.Tests.Infrastructure;
using Prova.Model.Enums;
using Prova.Service.Dtos;

namespace Prova.Api.Tests.Controllers;

/// <summary>
/// Testes de integração de <c>CarneController</c> (T21/T16): cobrem os
/// status codes principais do critério de pronto da task (200 lista vazia,
/// 201 criação, 404 update em Id inexistente, 409 delete bloqueado).
/// </summary>
public class CarneControllerTests : IDisposable
{
    private readonly ProvaApiFactory _factory = new();
    private readonly HttpClient _client;

    public CarneControllerTests()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Get_SemCarnesCadastradas_Retorna200ComArrayVazio()
    {
        var resposta = await _client.GetAsync("/api/carnes");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var carnes = await resposta.Content.ReadFromJsonAsync<List<CarneResponseDto>>();
        Assert.NotNull(carnes);
        Assert.Empty(carnes!);
    }

    [Fact]
    public async Task Post_CarneValida_Retorna201ComLocationHeader()
    {
        var dto = new CarneDto("Picanha", OrigemCarne.Bovina);

        var resposta = await _client.PostAsJsonAsync("/api/carnes", dto);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(resposta.Headers.Location);

        var carneCriada = await resposta.Content.ReadFromJsonAsync<CarneResponseDto>();
        Assert.NotNull(carneCriada);
        Assert.Equal("Picanha", carneCriada!.Descricao);
    }

    [Fact]
    public async Task Put_IdInexistente_Retorna404()
    {
        var dto = new CarneDto("Alcatra", OrigemCarne.Bovina);

        var resposta = await _client.PutAsJsonAsync("/api/carnes/999999", dto);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Delete_CarneComPedidoItemVinculado_Retorna409()
    {
        var cidade = await _factory.SemearCidadeAsync();

        var carneCriada = await CriarCarneAsync();
        var compradorCriado = await CriarCompradorAsync(cidade.Id);

        var pedidoDto = new PedidoDto(
            DateTime.UtcNow,
            compradorCriado.Id,
            new List<PedidoItemDto> { new(carneCriada.Id, 50m, Moeda.BRL) });

        var respostaPedido = await _client.PostAsJsonAsync("/api/pedidos", pedidoDto);
        Assert.Equal(HttpStatusCode.Created, respostaPedido.StatusCode);

        var resposta = await _client.DeleteAsync($"/api/carnes/{carneCriada.Id}");

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    private async Task<CarneResponseDto> CriarCarneAsync()
    {
        var resposta = await _client.PostAsJsonAsync("/api/carnes", new CarneDto("Costela", OrigemCarne.Bovina));
        return (await resposta.Content.ReadFromJsonAsync<CarneResponseDto>())!;
    }

    private async Task<CompradorResponseDto> CriarCompradorAsync(int cidadeId)
    {
        var resposta = await _client.PostAsJsonAsync(
            "/api/compradores", new CompradorDto("Comprador Teste", "12345678900", cidadeId));
        return (await resposta.Content.ReadFromJsonAsync<CompradorResponseDto>())!;
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
