namespace QdtCqts.Desktop.Wpf.ViewModels.Unifilar;

/// <summary>
/// Métricas visuais e constantes de apresentação geométrica do diagrama unifilar (Fases 27C / 27D).
/// Centraliza dimensões de nós, transformadores, margens e parâmetros de enquadramento (FitToView).
/// </summary>
public static class UnifilarVisualMetrics
{
    /// <summary>Largura padrão de apresentação de um nó/poste (pixels no canvas).</summary>
    public const double NodeWidth = 74.0;

    /// <summary>Altura padrão de apresentação de um nó/poste (pixels no canvas).</summary>
    public const double NodeHeight = 48.0;

    /// <summary>Largura padrão de apresentação do transformador (pixels no canvas).</summary>
    public const double TransformerWidth = 110.0;

    /// <summary>Altura padrão de apresentação do transformador (pixels no canvas).</summary>
    public const double TransformerHeight = 68.0;

    /// <summary>Margem de respiro ao redor dos limites da rede para enquadramento.</summary>
    public const double DefaultMargin = 40.0;

    /// <summary>Fator de preenchimento do viewport durante o enquadramento (8% de respiro periférico).</summary>
    public const double ViewportPaddingRatio = 0.92;

    /// <summary>Zoom mínimo absoluto suportado pela apresentação.</summary>
    public const double MinZoom = 0.25;

    /// <summary>Zoom máximo suportado na navegação livre.</summary>
    public const double MaxZoom = 4.0;

    /// <summary>
    /// Limite superior intencional para o comando FitToView automático.
    /// Evita distorção e gigantismo visual de fontes e nós em redes com poucos elementos.
    /// </summary>
    public const double FitToViewMaxZoom = 2.5;
}
