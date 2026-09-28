namespace QdtCqts.Desktop.Wpf.ViewModels.Unifilar;

/// <summary>
/// Filtros visuais de condição elétrica para o diagrama unifilar (Fase 27D).
/// Opera estritamente sobre contratos e grandezas já calculadas pelo backend.
/// </summary>
public enum UnifilarVisualFilter
{
    /// <summary>Todos os elementos da rede são exibidos com opacidade total (padrão).</summary>
    Todos,

    /// <summary>Destaca trechos com sobrecarga confirmada pelo backend (IsOverloaded == true).</summary>
    Sobrecarga,

    /// <summary>Destaca nós/trechos com queda de tensão acima do limite configurável ou regulatório.</summary>
    QuedaTensaoLimite,

    /// <summary>Destaca elementos quando há bloqueio de evidência de proteção (EvidenceBlocked).</summary>
    EvidenceBlocked,

    /// <summary>Foca exclusivamente no elemento selecionado.</summary>
    Selecionados,

    /// <summary>Foca exclusivamente no caminho elétrico rastreado (Trace da raiz até o elemento selecionado).</summary>
    Trace
}
