using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaToolkit.Async;
using AvaloniaToolkit.AwaitableUI;
using AvaloniaToolkit.Imaging;

namespace AvaloniaToolkit.Controls
{
    /// <summary>
    /// The Value is the 0..360deg range hue.
    /// </summary>
    //[TemplatePart(Name = ContainerGridName, Type = typeof(Grid))]
    //[TemplatePart(Name = HueRingImageName, Type = typeof(Image))]
    //[TemplatePart(Name = RingThumbName, Type = typeof(RingSlice))]
    public class HueRingPicker : RangeBase
    {
        private const string ContainerGridName = "PART_ContainerGrid";
        private const string HueRingImageName = "PART_HueRingImage";
        private const string RingThumbName = "PART_RingThumb";

        private Grid _containerGrid;
        private Image _hueRingImage;
        private RingSlice _ringThumb;

        private bool _isLoaded;
        private AsyncAutoResetEvent _bitmapUpdateRequired = new AsyncAutoResetEvent();

        #region RingThickness property

        public static readonly DirectProperty<HueRingPicker, double> RingThicknessProperty = AvaloniaProperty.RegisterDirect<HueRingPicker, double>(nameof(RingThickness), o => o.RingThickness, (o, v) => o.RingThickness = v);

        private double mRingThickness;
        public double RingThickness
        {
            get => mRingThickness;
            set
            {
                if (SetAndRaise(RingThicknessProperty, ref mRingThickness, value))
                {
                    UpdateVisuals();
                }
            }
        }

        #endregion RingThickness property

        #region ThumbArcAngle property

        public static readonly DirectProperty<HueRingPicker, double> ThumbArcAngleProperty = AvaloniaProperty.RegisterDirect<HueRingPicker, double>(nameof(ThumbArcAngle), o => o.ThumbArcAngle, (o, v) => o.ThumbArcAngle = v);

        private double mThumbArcAngle;
        public double ThumbArcAngle
        {
            get => mThumbArcAngle;
            set
            {
                if (value < 0 || value > 180)
                {
                    throw new ArgumentException("ThumbArcAngle only supports values in the 0..180 range.");
                }
                if (SetAndRaise(ThumbArcAngleProperty, ref mThumbArcAngle, value))
                {
                    UpdateRingThumb();
                }
            }
        }

        #endregion ThumbArcAngle property

        #region ThumbBorderBrush property

        public static readonly DirectProperty<HueRingPicker, Brush> ThumbBorderBrushProperty = AvaloniaProperty.RegisterDirect<HueRingPicker, Brush>(nameof(ThumbBorderBrush), o => o.ThumbBorderBrush, (o, v) => o.ThumbBorderBrush = v);

        private Brush mThumbBorderBrush;
        public Brush ThumbBorderBrush
        {
            get => mThumbBorderBrush;
            set => SetAndRaise(ThumbBorderBrushProperty, ref mThumbBorderBrush, value);
        }

        #endregion ThumbBorderBrush property

        #region ThumbBorderThickness property

        public static readonly DirectProperty<HueRingPicker, double> ThumbBorderThicknessProperty = AvaloniaProperty.RegisterDirect<HueRingPicker, double>(nameof(ThumbBorderThickness), o => o.ThumbBorderThickness, (o, v) => o.ThumbBorderThickness = v);

        private double mThumbBorderThickness;
        public double ThumbBorderThickness
        {
            get => mThumbBorderThickness;
            set
            {
                if (SetAndRaise(ThumbBorderThicknessProperty, ref mThumbBorderThickness, value))
                {
                    UpdateRingThumb();
                }
            }
        }

        #endregion ThumbBorderThickness property

        #region ThumbBackground property

        public static readonly DirectProperty<HueRingPicker, Brush> ThumbBackgroundProperty = AvaloniaProperty.RegisterDirect<HueRingPicker, Brush>(nameof(ThumbBackground), o => o.ThumbBackground, (o, v) => o.ThumbBackground = v);

        private Brush mThumbBackground;
        public Brush ThumbBackground
        {
            get => mThumbBackground;
            set => SetAndRaise(ThumbBackgroundProperty, ref mThumbBackground, value);
        }

        #endregion ThumbBackground property
        
        /// <summary>
        /// Initializes a new instance of the <see cref="HueRingPicker"/> class.
        /// </summary>
        public HueRingPicker()
        {
            InitializeMinMaxCoercion();
            
            DelayedUpdateWorkaround();
        }

        protected override async void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            await this.WaitForLoadedAsync();
            OnLoaded();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            OnUnloaded();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == BoundsProperty)
            {
                OnSizeChanged();
            }
            if (e.Property == ValueProperty)
            {
                UpdateRingThumb();
            }
            if (e.Property == MaximumProperty)
            {
                OnMaximumChanged((double)e.OldValue, (double)e.NewValue);
            }
            if (e.Property == MinimumProperty)
            {
                OnMaximumChanged((double)e.OldValue, (double)e.NewValue);
            }
            
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var xResult = base.MeasureOverride(availableSize);
            ;
            return xResult;
        }

        protected override Size MeasureCore(Size availableSize)
        {
            var xSize = base.MeasureCore(availableSize);
            ;
            return xSize;
        }

        private void OnLoaded()
        {
            _isLoaded = true;
            _bitmapUpdateRequired.Set();
            RunBitmapUpdaterAsync();
        }

        private void OnUnloaded()
        {
            _isLoaded = false;
            _bitmapUpdateRequired.Set();
        }

        private async void RunBitmapUpdaterAsync()
        {
            do
            {
                await _bitmapUpdateRequired.WaitAsync();

                if (_isLoaded)
                {
                    if (_hueRingImage == null)
                    {
                        continue;
                    }
                    if (double.IsNaN(_containerGrid.Bounds.Width)
                        || double.IsNaN(_containerGrid.Bounds.Height))
                    {
                        continue;
                    }

                    if (_containerGrid.Bounds.Height <= 2 * (RingThickness + ThumbBorderThickness) ||
                        _containerGrid.Bounds.Width <= 2 * (RingThickness + ThumbBorderThickness))
                    {
                        await Task.Delay(50);
                        if (_containerGrid.Bounds.Height <= 2 * (RingThickness + ThumbBorderThickness)
                            || _containerGrid.Bounds.Width <= 2 * (RingThickness + ThumbBorderThickness))
                        {
                            continue;
                        }
                    }

                    var hueRingSize = (int)Math.Min(
                        _containerGrid.Bounds.Width - 2 * ThumbBorderThickness,
                        _containerGrid.Bounds.Height - 2 * ThumbBorderThickness);
                    
                    var wb = new WritableBitmap(hueRingSize, hueRingSize);
                    await wb.RenderColorPickerHueRingAsync(hueRingSize / 2 - (int)RingThickness);
                    _hueRingImage.Source = wb;
                }
            } while (_isLoaded);
        }

        private async void DelayedUpdateWorkaround()
        {
            // Not sure why the thumb starts at the wrong position without this code.
            // Perhaps something with path positioning overall, but I don't want to dig into this now.
            await Task.Delay(10);
            UpdateRingThumb();
        }

        private void OnSizeChanged()
        {
            UpdateVisuals();
        }
        
        /// <summary>
        /// Invoked whenever application code or internal processes (such as a rebuilding layout pass) call ApplyTemplate. In simplest terms, this means the method is called just before a UI element displays in your app. Override this method to influence the default post-template logic of a class.
        /// </summary>
        protected override async void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);

            _containerGrid = e.NameScope.Find<Grid>(ContainerGridName);
            _hueRingImage = e.NameScope.Find<Image>(HueRingImageName);
            _ringThumb = e.NameScope.Find<RingSlice>(RingThumbName);
            _hueRingImage.PointerPressed += OnPointerPressed;
            _hueRingImage.PointerMoved += OnPointerMoved;

            await this.WaitForLoadedAsync();
            UpdateVisuals();
        }

        private void OnPointerMoved(object sender, PointerEventArgs e)
        {
            // todo: IsInContact?
            if (!e.InputModifiers.HasFlag(InputModifiers.LeftMouseButton)
                && !e.InputModifiers.HasFlag(InputModifiers.MiddleMouseButton)
                && !e.InputModifiers.HasFlag(InputModifiers.RightMouseButton))
            {
                return;
            }

            var point = e.Device.GetPosition(_hueRingImage);
            UpdateValueForPoint(point);
        }

        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            //e.Device.Capture(this);
            var point = e.GetPosition(_hueRingImage);
            UpdateValueForPoint(point);
        }

        private void UpdateValueForPoint(Point point)
        {
            var pc = new Point(_hueRingImage.Bounds.Width / 2, _hueRingImage.Bounds.Height / 2);
            var atan2 = Math.Atan2(point.Y - pc.Y, point.X - pc.X) * 180 / Math.PI;
            this.Value = (atan2 + 360 + 90) % 360;
        }

        private void UpdateVisuals()
        {
            UpdateHueRingImage();
            UpdateRingThumb();
        }

        private void UpdateRingThumb()
        {
            if (_ringThumb == null)
            {
                return;
            }

            if (_containerGrid.Bounds.Height <= 2 * (RingThickness + ThumbBorderThickness) ||
                _containerGrid.Bounds.Width <= 2 * (RingThickness + ThumbBorderThickness))
            {
                return;
            }
            
            // Hue ring needs to be smaller than available container space so the thumb fits
            var hueRingSize = Math.Min(
                _containerGrid.Bounds.Width - 2 * ThumbBorderThickness,
                _containerGrid.Bounds.Height - 2 * ThumbBorderThickness);

            //if (_ringThumb.Width != hueRingSize + 2 * ThumbBorderThickness)
            {
                _ringThumb.Width = hueRingSize + 2 * ThumbBorderThickness;
                _ringThumb.Height = hueRingSize + 2 * ThumbBorderThickness;

                _ringThumb.BeginUpdate();

                // Half of the thumb border stroke goes outside the radius and the other half goes inside,
                // so radius needs to be 0.5 border thickness larger than the hue ring's outer radius.
                // Less 1 to make sure there is an overlap.
                _ringThumb.Center =
                    new Point(
                        hueRingSize / 2 + ThumbBorderThickness,
                        hueRingSize / 2 + ThumbBorderThickness);
                _ringThumb.Radius = (hueRingSize + ThumbBorderThickness) / 2 - 1;
                _ringThumb.InnerRadius = _ringThumb.Radius - RingThickness - ThumbBorderThickness + 2;
                _ringThumb.StartAngle = this.Value - ThumbArcAngle / 2;
                _ringThumb.EndAngle = this.Value + ThumbArcAngle / 2;
                //_ringThumb.StartAngle = - ThumbArcAngle / 2;
                //_ringThumb.EndAngle = ThumbArcAngle / 2;
                _ringThumb.EndUpdate();
            }

            _ringThumb.Stroke =
                new SolidColorBrush(
                    ColorExtensions.FromHsv(this.Value, 0.5, 1.0));
        }

        #region UpdateHueRingImage()
        private void UpdateHueRingImage()
        {
            _bitmapUpdateRequired.Set();
        }
        #endregion

        #region Min/Max coercion methods
        /// <summary>
        /// Initializes event handling to coerce Minimum and Maximum properties to the 0..360 range.
        /// </summary>
        private void InitializeMinMaxCoercion()
        {
            this.Minimum = 0.0;
            this.Maximum = 360.0;
            //var minimumChangedEventSource = new PropertyChangeEventSource<double>(
            //    this, "Minimum");
            //minimumChangedEventSource.ValueChanged += OnMinimumChanged;
            //var maximumChangedEventSource = new PropertyChangeEventSource<double>(
            //    this, "Maximum");
            //maximumChangedEventSource.ValueChanged += OnMaximumChanged;
        }

        protected void OnMinimumChanged(double oldMinimum, double newMinimum)
        {
            if (newMinimum < 0)
            {
                this.Minimum = 0;
            }
        }

        protected void OnMaximumChanged(double oldMaximum, double newMaximum)
        {
            if (newMaximum > 360)
            {
                this.Maximum = 360;
            }
        }
        #endregion
    }
}