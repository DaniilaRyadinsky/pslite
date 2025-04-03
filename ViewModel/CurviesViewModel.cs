using ImageLinker2.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.Graphics.Imaging;


namespace ImageLinker2.ViewModel
{
    public class CurviesViewModel : BaseViewModel
    {
        private Visibility _Visibility = Visibility.Collapsed;
        public Visibility Visibility
        {
            get => _Visibility;
            set
            {
                if (_Visibility != value)
                {
                    _Visibility = value;
                    OnPropertyChanged(nameof(Visibility));
                }
            }
        }

        private WriteableBitmap? _HistogramSource;
        public WriteableBitmap? HistogramSource
        {
            get => _HistogramSource;
            set
            {
                if (_HistogramSource != value)
                {
                    _HistogramSource = value;
                    OnPropertyChanged(nameof(HistogramSource));
                }
            }
        }

        private List<Point> _Points;
        private List<Point> Points
        {
            get => _Points;
            set
            {
                if (_Points != value)
                {
                    _Points = value;
                    OnPropertyChanged(nameof(Points));
                }
            }
        }

        public WriteableBitmap? ViewReference;
        public ViewPortViewModel ViewPortVM;

        private Canvas _curveViewCanvas;
        public Canvas CurveViewCanvas
        {
            get => _curveViewCanvas;
            set
            {
                _curveViewCanvas = value;
                OnPropertyChanged(nameof(CurveViewCanvas));
                if (_curveViewCanvas != null) DrawCurvies();
            }
        }

        private Point offset;
        private Rectangle? selectedPoint = null;
        private int? selectedPointIndex = null;

        public CurviesViewModel(ViewPortViewModel viewPortVM)
        {
            Points = [new Point(0, 255), new Point(255, 0)];
            ViewReference = null;
            ViewPortVM = viewPortVM;
        }

        private async void RenderHistogram(WriteableBitmap? ViewSource)
        {
            var writableBitmap = await Curve.MakeHistogram(ViewSource);
            HistogramSource = writableBitmap;
            
        }

        public void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var currentPosition = e.GetCurrentPoint(CurveViewCanvas).Position;

            if (e.GetCurrentPoint(CurveViewCanvas).Properties.IsRightButtonPressed)
            {
                RemovePoint(currentPosition);
            }
            else
            {
                selectedPointIndex = FindPoint(currentPosition);

                if (selectedPointIndex != null)
                {
                    selectedPoint = FindEllipseAt(currentPosition);
                    offset = new Point(
                        currentPosition.X - Points[selectedPointIndex.Value].X,
                        currentPosition.Y - Points[selectedPointIndex.Value].Y);

                    if (selectedPoint != null)
                    {
                        Canvas.SetZIndex(selectedPoint, 3);
                    }
                }
                else
                {
                    Points.Add(currentPosition);
                }
            }
            DrawCurvies();
            RenderViewPort();
        }

        public void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (selectedPointIndex != null && e.Pointer.IsInContact)
            {
                var currentPosition = e.GetCurrentPoint(CurveViewCanvas).Position;
                var newPosition = new Point(
                    currentPosition.X - offset.X,
                    currentPosition.Y - offset.Y);

                Points[selectedPointIndex.Value] = newPosition;

                if (selectedPoint != null)
                {
                    DrawCurvies();
                    RenderViewPort();
                }
            }
        }

        public void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (selectedPoint != null)
            {
                Canvas.SetZIndex(selectedPoint, 1);
            }
            selectedPoint = null;
            selectedPointIndex = null;
        }

        private Rectangle? FindEllipseAt(Point position)
        {
            foreach (var child in CurveViewCanvas.Children.OfType<Rectangle>())
            {
                var left = Canvas.GetLeft(child);
                var top = Canvas.GetTop(child);

                if (position.X >= left && position.X <= left + child.Width &&
                    position.Y >= top && position.Y <= top + child.Height)
                {
                    return child;
                }
            }
            return null;
        }

        private void RemovePoint(Point point)
        {
            int? index = FindPoint(point);
            if (index != null)
                Points.RemoveAt((int)index);
        }

        private int? FindPoint(Point point)
        {
            for (int i = 1; i < Points.Count-1; i++)
            {
                if (Math.Abs(Points[i].X - point.X) <= 3)
                {
                    return i;
                }
            }
            return null;
        }

        private void DrawCurvies()
        {
            CurveViewCanvas.Children.Clear();
            Points.Sort((a, b) => a.X.CompareTo(b.X));
            var point1 = Points[0];
            Rectangle rec = new()
            {
                Width = 6,
                Height = 6,
                Fill = new SolidColorBrush(Colors.Blue)
            };

            Canvas.SetLeft(rec, Points[0].X - 3);
            Canvas.SetTop(rec, Points[0].Y - 3);

            CurveViewCanvas.Children.Add(rec);
            for (int i = 1; i < Points.Count; i++)
            {
                Line line = new()
                {
                    X1 = Points[i - 1].X,
                    Y1 = Points[i - 1].Y,
                    X2 = Points[i].X,
                    Y2 = Points[i].Y,
                    Stroke = new SolidColorBrush(Colors.Red),
                    StrokeThickness = 1
                };
                rec = new()
                {
                    Width = 6,
                    Height = 6,
                    Fill = new SolidColorBrush(Colors.Blue)
                };

                Canvas.SetLeft(rec, Points[i].X - 3);
                Canvas.SetTop(rec, Points[i].Y - 3);

                CurveViewCanvas.Children.Add(line);
                CurveViewCanvas.Children.Add(rec);
            }
        }

        public void RenderViewPort()
        {
            Dictionary<int, int> map = Curve.GenerateTransformMap(Points);
            ViewPortVM.Render(ViewReference, map);
            RenderHistogram(ViewPortVM.View);
        }

    }
}
