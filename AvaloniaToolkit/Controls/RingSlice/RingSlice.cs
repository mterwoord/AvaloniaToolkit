using System;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Media;

namespace AvaloniaToolkit.Controls
{
    /// <summary>
    /// A Path that represents a ring slice with a given
    /// (outer) Radius,
    /// InnerRadius,
    /// StartAngle,
    /// EndAngle and
    /// Center.
    /// </summary>
    public class RingSlice : Path
    {
        public RingSlice()
        {
            IsVisible = false;
        }

        private bool _isUpdating;

        #region StartAngle property

        public static readonly DirectProperty<RingSlice, double> StartAngleProperty = AvaloniaProperty.RegisterDirect<RingSlice, double>(nameof(StartAngle), o => o.StartAngle, (o, v) => o.StartAngle = v);

        private double _startAngle;

        public double StartAngle
        {
            get => _startAngle;
            set
            {
                if (SetAndRaise(StartAngleProperty, ref _startAngle, value))
                {
                    UpdatePath();
                }
            }
        }

        #endregion StartAngle property

        #region EndAngle property

        public static readonly DirectProperty<RingSlice, double> EndAngleProperty = AvaloniaProperty.RegisterDirect<RingSlice, double>(nameof(EndAngle), o => o.EndAngle, (o, v) => o.EndAngle = v);

        private double _endAngle;

        public double EndAngle
        {
            get => _endAngle;
            set
            {
                if (SetAndRaise(EndAngleProperty, ref _endAngle, value))
                {
                    UpdatePath();
                }
            }
        }

        #endregion EndAngle property

        #region Radius property

        public static readonly DirectProperty<RingSlice, double> RadiusProperty = AvaloniaProperty.RegisterDirect<RingSlice, double>(nameof(Radius), o => o.Radius, (o, v) => o.Radius = v);

        private double _radius = 0d;

        public double Radius
        {
            get => _radius;
            set
            {
                if (SetAndRaise(RadiusProperty, ref _radius, value))
                {
                    this.Width = this.Height = 2 * Radius;
                    UpdatePath();
                }
            }
        }


        #endregion Radius property

        #region InnerRadius property

        public static readonly DirectProperty<RingSlice, double> InnerRadiusProperty = AvaloniaProperty.RegisterDirect<RingSlice, double>(nameof(InnerRadius), o => o.InnerRadius, (o, v) => o.InnerRadius = v);

        private double _innerRadius = 0d;
        public double InnerRadius
        {
            get => _innerRadius;
            set
            {
                if (SetAndRaise(InnerRadiusProperty, ref _innerRadius, value))
                {
                    if (value < 0)
                    {
                        throw new ArgumentException("InnerRadius can't be a negative value.", "InnerRadius");
                    }

                    UpdatePath();
                }
            }
        }

        #endregion InnerRadius property

        #region Center property

        public static readonly DirectProperty<RingSlice, Point?> CenterProperty = AvaloniaProperty.RegisterDirect<RingSlice, Point?>(nameof(Center), o => o.Center, (o, v) => o.Center = v);

        private Point? _center;

        public Point? Center
        {
            get => _center;
            set
            {
                if (SetAndRaise(CenterProperty, ref _center, value))
                {
                    UpdatePath();
                }
            }
        }

        #endregion Center property

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == StrokeThicknessProperty)
            {
                OnStrokeThicknessChanged();
                return;
            }
            if (e.Property == DesiredSizeProperty)
            {
                OnSizeChanged();
            }
        }

        private void OnStrokeThicknessChanged()
        {
            UpdatePath();
        }

        private void OnSizeChanged()
        {
            UpdatePath();
        }

        /// <summary>
        /// Suspends path updates until EndUpdate is called;
        /// </summary>
        public void BeginUpdate()
        {
            _isUpdating = true;
        }

        /// <summary>
        /// Resumes immediate path updates every time a component property value changes. Updates the path.
        /// </summary>
        public void EndUpdate()
        {
            _isUpdating = false;
            UpdatePath();
        }

        private void UpdatePath()
        {
            var innerRadius = this.InnerRadius + this.StrokeThickness / 2;
            var outerRadius = this.Radius - this.StrokeThickness / 2;

            if (_isUpdating ||
                Width == 0 ||
                innerRadius <= 0 ||
                outerRadius < innerRadius ||
                double.IsNaN(innerRadius) ||
                double.IsNaN(outerRadius))
            {
                return;
            }



            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure();
            pathFigure.IsClosed = true;

            var center =
                this.Center ??
                new Point(
                    outerRadius + this.StrokeThickness / 2,
                    outerRadius + this.StrokeThickness / 2);

            // Starting Point
            pathFigure.StartPoint =
                new Point(
                    center.X + Math.Sin(StartAngle * Math.PI / 180) * innerRadius,
                    center.Y - Math.Cos(StartAngle * Math.PI / 180) * innerRadius);

            // Inner Arc
            var innerArcSegment = new ArcSegment();
            innerArcSegment.IsLargeArc = (EndAngle - StartAngle) >= 180.0;
            innerArcSegment.Point =
                new Point(
                    center.X + Math.Sin(EndAngle * Math.PI / 180) * innerRadius,
                    center.Y - Math.Cos(EndAngle * Math.PI / 180) * innerRadius);
            innerArcSegment.Size = new Size(innerRadius, innerRadius);
            innerArcSegment.SweepDirection = SweepDirection.Clockwise;

            var lineSegment =
                new LineSegment
                {
                    Point = new Point(
                        center.X + Math.Sin(EndAngle * Math.PI / 180) * outerRadius,
                        center.Y - Math.Cos(EndAngle * Math.PI / 180) * outerRadius)
                };

            // Outer Arc
            var outerArcSegment = new ArcSegment();
            outerArcSegment.IsLargeArc = (EndAngle - StartAngle) >= 180.0;
            outerArcSegment.Point =
                new Point(
                        center.X + Math.Sin(StartAngle * Math.PI / 180) * outerRadius,
                        center.Y - Math.Cos(StartAngle * Math.PI / 180) * outerRadius);
            outerArcSegment.Size = new Size(outerRadius, outerRadius);
            outerArcSegment.SweepDirection = SweepDirection.CounterClockwise;

            pathFigure.Segments.Add(innerArcSegment);
            pathFigure.Segments.Add(lineSegment);
            pathFigure.Segments.Add(outerArcSegment);
            pathGeometry.Figures.Add(pathFigure);
            this.InvalidateArrange();
            
            this.Data = pathGeometry;
            IsVisible = true;
        }
    }
}