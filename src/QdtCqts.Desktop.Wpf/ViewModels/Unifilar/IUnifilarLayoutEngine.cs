using System.Collections.Generic;
using QdtCqts.Domain;

namespace QdtCqts.Desktop.Wpf.ViewModels.Unifilar;

/// <summary>
/// Motor geométrico de layout para o diagrama unifilar (Fase 27B).
/// Opera estritamente sobre relações topológicas e geometria gráfica,
/// sem qualquer dependência ou uso de grandezas elétricas.
/// </summary>
public interface IUnifilarLayoutEngine
{
    IReadOnlyDictionary<string, (double X, double Y)> ComputeLayout(
        NetworkModel model,
        TopologyValidationResult topology,
        double startX = 80,
        double startY = 80,
        double horizontalSpacing = 160,
        double verticalSpacing = 90);
}
