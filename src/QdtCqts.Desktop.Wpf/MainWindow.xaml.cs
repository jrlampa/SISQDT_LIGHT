using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QdtCqts.Desktop.Wpf.ViewModels;

namespace QdtCqts.Desktop.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml (Fase 27C).
/// Orquestra a interação do viewport do Unifilar (Zoom, Pan, Fit, Seleção).
/// </summary>
public partial class MainWindow : Window
{
    private Point _lastPanPoint;
    private bool _isPanning;

    public MainWindow()
    {
        InitializeComponent();
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    // ── Zoom Buttons ──
    private void BtnZoomIn_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.UnifilarDiagram?.ZoomIn();
    }

    private void BtnZoomOut_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.UnifilarDiagram?.ZoomOut();
    }

    private void BtnResetZoom_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.UnifilarDiagram?.ResetZoom();
    }

    private void BtnFitView_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel?.UnifilarDiagram != null)
        {
            double vw = ViewportBorder.ActualWidth > 0 ? ViewportBorder.ActualWidth : 800;
            double vh = ViewportBorder.ActualHeight > 0 ? ViewportBorder.ActualHeight : 500;
            ViewModel.UnifilarDiagram.FitToView(vw, vh);
        }
    }

    // ── Mouse Pan & Zoom Interactivity ──
    private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (ViewModel?.UnifilarDiagram == null) return;

        if (e.Delta > 0)
        {
            ViewModel.UnifilarDiagram.ZoomIn();
        }
        else if (e.Delta < 0)
        {
            ViewModel.UnifilarDiagram.ZoomOut();
        }
        e.Handled = true;
    }

    private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Middle || e.ChangedButton == MouseButton.Left)
        {
            _isPanning = true;
            _lastPanPoint = e.GetPosition(ViewportBorder);
            ViewportBorder.CaptureMouse();
        }
    }

    private void Viewport_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isPanning || ViewModel?.UnifilarDiagram == null) return;

        var currentPoint = e.GetPosition(ViewportBorder);
        double deltaX = currentPoint.X - _lastPanPoint.X;
        double deltaY = currentPoint.Y - _lastPanPoint.Y;

        ViewModel.UnifilarDiagram.PanX += deltaX;
        ViewModel.UnifilarDiagram.PanY += deltaY;

        _lastPanPoint = currentPoint;
    }

    private void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isPanning)
        {
            _isPanning = false;
            ViewportBorder.ReleaseMouseCapture();
        }
    }

    // ── Selection Handlers ──
    private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement elem && elem.Tag is string nodeId)
        {
            ViewModel?.UnifilarDiagram?.SelectNode(nodeId);
            e.Handled = true;
        }
    }

    private void Edge_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement elem && elem.Tag is string edgeId)
        {
            ViewModel?.UnifilarDiagram?.SelectEdge(edgeId);

            // Sincroniza seleção com a tabela de segmentos
            if (ViewModel != null)
            {
                var match = System.Linq.Enumerable.FirstOrDefault(ViewModel.Segments, s => s.SegmentId == edgeId);
                if (match != null)
                {
                    ViewModel.SelectedSegment = match;
                }
            }

            e.Handled = true;
        }
    }

    private void Transformer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement elem && elem.Tag is string trafoId)
        {
            ViewModel?.UnifilarDiagram?.SelectTransformer(trafoId);
            e.Handled = true;
        }
    }

    private void CmbVisualFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel?.UnifilarDiagram != null && CmbVisualFilter.SelectedItem is ComboBoxItem item && item.Tag is string tag)
        {
            if (Enum.TryParse<ViewModels.Unifilar.UnifilarVisualFilter>(tag, out var filter))
            {
                ViewModel.UnifilarDiagram.CurrentFilter = filter;
            }
        }
    }

    private void BtnClearSelection_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.UnifilarDiagram?.ClearSelection();
        if (ViewModel != null)
        {
            ViewModel.SelectedSegment = null;
        }
    }
}