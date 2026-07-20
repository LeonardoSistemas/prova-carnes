using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Prova.Api.Middlewares;
using Prova.Data.Context;
using Prova.Data.Repositories;
using Prova.Service.Cotacao;
using Prova.Service.Services;
using Prova.Service.Validators;

var builder = WebApplication.CreateBuilder(args);

// Nome da política de CORS restrita à origem do frontend em desenvolvimento
// (nunca AllowAnyOrigin()).
const string FrontendCorsPolicy = "FrontendCorsPolicy";

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<ICarneService, CarneService>();
builder.Services.AddScoped<ICompradorService, CompradorService>();
builder.Services.AddScoped<IEstadoService, EstadoService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddValidatorsFromAssemblyContaining<CarneDtoValidator>();

// Typed HttpClient — nunca "new HttpClient()" direto (evita esgotamento de
// sockets / DNS obsoleto). BcbCotacaoService e
// AwesomeApiCotacaoService são registrados pelos próprios tipos concretos
// (nunca como ICotacaoService diretamente) — quem enxerga ICotacaoService é
// só o CotacaoServiceComFallback, montado logo abaixo (ver XML doc de
// CotacaoServiceComFallback, que documenta essa decisão de registro).
builder.Services.AddHttpClient<BcbCotacaoService>();
builder.Services.AddHttpClient<AwesomeApiCotacaoService>();

// ICotacaoService resolve para o decorator de fallback: AwesomeAPI como fonte
// primária, BCB (com sua janela interna de 7 dias) como fallback (ver
// CotacaoServiceComFallback).
// Lifetime "Transient" (via AddTransient), não "Scoped" como o restante do
// Program.cs: AddHttpClient<T>() registra os typed clients concretos
// (BcbCotacaoService/AwesomeApiCotacaoService) como Transient por padrão do
// IHttpClientFactory, e a factory abaixo resolve ambos do container a cada
// injeção — usar Transient aqui evita inconsistência de lifetime entre o
// decorator e as dependências que ele engloba.
builder.Services.AddTransient<ICotacaoService>(sp => new CotacaoServiceComFallback(
    sp.GetRequiredService<AwesomeApiCotacaoService>(),
    sp.GetRequiredService<BcbCotacaoService>()));

builder.Services.AddCors(options =>
{
    var frontendOrigin = builder.Configuration["Cors:FrontendOrigin"] ?? "http://localhost:5173";

    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(frontendOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Middleware de exceção global — precisa vir antes de qualquer outro
// middleware para capturar exceções lançadas em qualquer ponto do pipeline
// (nunca try/catch disperso pelo código).
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.MapControllers();

app.Run();

// Necessário como partial class para o WebApplicationFactory<Program> dos
// testes de integração (T21) conseguir localizar o entry point.
public partial class Program
{
}
