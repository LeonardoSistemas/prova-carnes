using System.Net;
using System.Net.Http.Json;
using Prova.Api.Tests.Infrastructure;
using Prova.Model.Enums;
using Prova.Service.Dtos;

namespace Prova.Api.Tests.Controllers;

/// <summary>
/// Testes de integração de <c>PedidoController</c> (T21/T19): cobrem 201 na
/// criação bem-sucedida, 422 quando a cotação está indisponível (dublê de
/// <c>ICotacaoService</c> forçando a falha), 404 em Id inexistente e 204 na
/// exclusão.
/// </summary>
public class PedidoControllerTests
{
    [Fact]
    public async Task Post_ComCotacaoDisponivel_Retorna201()
    {
        using var factory = new ProvaApiFactory(CotacaoServiceFake.ComSucesso());
        using var client = factory.CreateClient();

        var cidade = await factory.SemearCidadeAsync();
        var carne = await CriarCarneAsync(client);
        var comprador = await CriarCompradorAsync(client, cidade.Id);

        var dto = new PedidoDto(
            DateTime.UtcNow,
            comprador.Id,
            new List<PedidoItemDto> { new(carne.Id, 100m, Moeda.USD) });

        var resposta = await client.PostAsJsonAsync("/api/pedidos", dto);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var pedidoCriado = await resposta.Content.ReadFromJsonAsync<PedidoResponseDto>();
        Assert.NotNull(pedidoCriado);
        Assert.Equal(500m, pedidoCriado!.ValorTotalEmReal); // 100 USD * cotação fake 5
    }

    [Fact]
    public async Task Post_ComCotacaoIndisponivel_Retorna422()
    {
        using var factory = new ProvaApiFactory(CotacaoServiceFake.Indisponivel());
        using var client = factory.CreateClient();

        var cidade = await factory.SemearCidadeAsync();
        var carne = await CriarCarneAsync(client);
        var comprador = await CriarCompradorAsync(client, cidade.Id);

        var dto = new PedidoDto(
            DateTime.UtcNow,
            comprador.Id,
            new List<PedidoItemDto> { new(carne.Id, 100m, Moeda.USD) });

        var resposta = await client.PostAsJsonAsync("/api/pedidos", dto);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task Get_ComFiltroDeCompradorId_RetornaApenasPedidosDoComprador()
    {
        using var factory = new ProvaApiFactory(CotacaoServiceFake.ComSucesso());
        using var client = factory.CreateClient();

        var cidade = await factory.SemearCidadeAsync();
        var carne = await CriarCarneAsync(client);
        var compradorA = await CriarCompradorAsync(client, cidade.Id);
        var compradorB = await CriarCompradorAsync(client, cidade.Id);

        await client.PostAsJsonAsync(
            "/api/pedidos",
            new PedidoDto(DateTime.UtcNow, compradorA.Id, new List<PedidoItemDto> { new(carne.Id, 50m, Moeda.BRL) }));
        await client.PostAsJsonAsync(
            "/api/pedidos",
            new PedidoDto(DateTime.UtcNow, compradorB.Id, new List<PedidoItemDto> { new(carne.Id, 70m, Moeda.BRL) }));

        var resposta = await client.GetAsync($"/api/pedidos?compradorId={compradorA.Id}");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var pedidos = await resposta.Content.ReadFromJsonAsync<List<PedidoResponseDto>>();
        Assert.NotNull(pedidos);
        Assert.All(pedidos!, p => Assert.Equal(compradorA.Id, p.CompradorId));
        Assert.Contains(pedidos!, p => p.CompradorId == compradorA.Id);
    }

    [Fact]
    public async Task GetPorId_Inexistente_Retorna404()
    {
        using var factory = new ProvaApiFactory();
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync("/api/pedidos/999999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Delete_PedidoExistente_Retorna204()
    {
        using var factory = new ProvaApiFactory(CotacaoServiceFake.ComSucesso());
        using var client = factory.CreateClient();

        var cidade = await factory.SemearCidadeAsync();
        var carne = await CriarCarneAsync(client);
        var comprador = await CriarCompradorAsync(client, cidade.Id);

        var dto = new PedidoDto(
            DateTime.UtcNow,
            comprador.Id,
            new List<PedidoItemDto> { new(carne.Id, 50m, Moeda.BRL) });

        var respostaPedido = await client.PostAsJsonAsync("/api/pedidos", dto);
        var pedidoCriado = await respostaPedido.Content.ReadFromJsonAsync<PedidoResponseDto>();

        var resposta = await client.DeleteAsync($"/api/pedidos/{pedidoCriado!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
    }

    private static async Task<CarneResponseDto> CriarCarneAsync(HttpClient client)
    {
        var resposta = await client.PostAsJsonAsync("/api/carnes", new CarneDto("Fraldinha", OrigemCarne.Bovina));
        return (await resposta.Content.ReadFromJsonAsync<CarneResponseDto>())!;
    }

    private static async Task<CompradorResponseDto> CriarCompradorAsync(HttpClient client, int cidadeId)
    {
        var resposta = await client.PostAsJsonAsync(
            "/api/compradores", new CompradorDto("Comprador Pedido", "55566677788", cidadeId));
        return (await resposta.Content.ReadFromJsonAsync<CompradorResponseDto>())!;
    }
}
