namespace Prova.Model.Enums;

/// <summary>
/// Período de agregação usado pelas métricas de topo do Dashboard (T59/T60).
/// Ver <c>DashboardService</c> para a conversão de cada valor em um intervalo
/// de datas concreto.
/// </summary>
public enum PeriodoDashboard
{
    Hoje = 1,
    Semana = 2,
    Mes = 3
}
