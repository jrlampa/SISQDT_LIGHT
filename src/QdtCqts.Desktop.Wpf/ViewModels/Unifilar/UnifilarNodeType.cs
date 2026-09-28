namespace QdtCqts.Desktop.Wpf.ViewModels.Unifilar;

/// <summary>
/// Classificação topológica do nó na rede elétrica secundária (Fase 27B).
/// Derivado estritamente da topologia estrutural (NetworkModel + TopologyValidator).
/// </summary>
public enum UnifilarNodeType
{
    /// <summary>Nó raiz/fonte do circuito (alimentação primária da rede BT).</summary>
    Source,

    /// <summary>Ponto de conexão de transformador de distribuição.</summary>
    Transformer,

    /// <summary>Nó intermediário com 1 entrada e 1 saída (passagem simples).</summary>
    PassThrough,

    /// <summary>Nó de bifurcação/derivação com mais de 1 saída (ex.: LID, ramificações de barramento).</summary>
    Branch,

    /// <summary>Nó de extremidade/ponta sem trechos a jusante (terminal de carga / consumidor final).</summary>
    Terminal
}
