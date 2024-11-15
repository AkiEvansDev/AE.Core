using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AE.Core.WPF.Controls
{
    public delegate void ColorChangedEventHandler(object sender, Color color);

    public partial class ColorPicker : Grid
    {
        public event ColorChangedEventHandler ColorChanged;

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            nameof(Color), typeof(Color), typeof(ColorPicker), new FrameworkPropertyMetadata(defaultValue: Colors.White, ColorPropertyChange));

        public static readonly DependencyProperty AProperty = DependencyProperty.Register(
            nameof(A), typeof(byte), typeof(ColorPicker), new FrameworkPropertyMetadata(defaultValue: (byte)255, APropertyChange));

        public static readonly DependencyProperty RProperty = DependencyProperty.Register(
            nameof(R), typeof(byte), typeof(ColorPicker), new FrameworkPropertyMetadata(defaultValue: (byte)255, RGBPropertyChange));

        public static readonly DependencyProperty GProperty = DependencyProperty.Register(
            nameof(G), typeof(byte), typeof(ColorPicker), new FrameworkPropertyMetadata(defaultValue: (byte)255, RGBPropertyChange));

        public static readonly DependencyProperty BProperty = DependencyProperty.Register(
            nameof(B), typeof(byte), typeof(ColorPicker), new FrameworkPropertyMetadata(defaultValue: (byte)255, RGBPropertyChange));

        public static readonly DependencyProperty HProperty = DependencyProperty.Register(
            nameof(H), typeof(double), typeof(ColorPicker), new FrameworkPropertyMetadata(defaultValue: (double)0, HSVPropertyChange));

        public static readonly DependencyProperty SProperty = DependencyProperty.Register(
            nameof(S), typeof(double), typeof(ColorPicker), new FrameworkPropertyMetadata(defaultValue: (double)0, HSVPropertyChange));

        public static readonly DependencyProperty VProperty = DependencyProperty.Register(
            nameof(V), typeof(double), typeof(ColorPicker), new FrameworkPropertyMetadata(defaultValue: (double)1, HSVPropertyChange));

        public static readonly DependencyProperty ShowAlphaProperty = DependencyProperty.Register(
            nameof(ShowAlpha), typeof(bool), typeof(ColorPicker), new FrameworkPropertyMetadata(defaultValue: true, ShowAlphaPropertyChange));

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public byte A
        {
            get => (byte)GetValue(AProperty);
            set => SetValue(AProperty, value);
        }

        public byte R
        {
            get => (byte)GetValue(RProperty);
            set => SetValue(RProperty, value);
        }

        public byte G
        {
            get => (byte)GetValue(GProperty);
            set => SetValue(GProperty, value);
        }

        public byte B
        {
            get => (byte)GetValue(BProperty);
            set => SetValue(BProperty, value);
        }

        public double H
        {
            get => (double)GetValue(HProperty);
            set => SetValue(HProperty, value);
        }

        public double S
        {
            get => (double)GetValue(SProperty);
            set => SetValue(SProperty, value);
        }

        public double V
        {
            get => (double)GetValue(VProperty);
            set => SetValue(VProperty, value);
        }

        public bool ShowAlpha
        {
            get => (bool)GetValue(ShowAlphaProperty);
            set => SetValue(ShowAlphaProperty, value);
        }

        private bool IsChangeFromMe = false;

        public ColorPicker()
        {
            InitializeComponent();
        }

        private static void ColorPropertyChange(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
                return;

            var color = (ColorPicker)source;

            if (color.IsChangeFromMe)
                return;

            color.IsChangeFromMe = true;

            ColorExtensions.ColorToHSV(color.Color, out var h, out var s, out var v);

            color.A = color.Color.A;
            color.R = color.Color.R;
            color.G = color.Color.G;
            color.B = color.Color.B;

            color.H = h;
            color.S = s;
            color.V = v;

            color.BoxColor.Color = ColorExtensions.ColorFromHSV(h, s, v);
            color.ColorStop.Color = ColorExtensions.ColorFromHSV(h, 1, 1);
            color.AColor.Opacity = color.A / 255.0;

            Canvas.SetLeft(color.ASliderCursor, color.A * color.ASlider.ActualWidth / 255);
            Canvas.SetLeft(color.SliderCursor, h * color.Slider.ActualWidth / 360);
            Canvas.SetLeft(color.BoxCursor, color.Box.ActualWidth * s);
            Canvas.SetTop(color.BoxCursor, color.Box.ActualHeight - color.Box.ActualHeight * v);

            color.IsChangeFromMe = false;
        }

        private static void APropertyChange(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
                return;

            var color = (ColorPicker)source;

            if (color.IsChangeFromMe)
                return;

            color.IsChangeFromMe = true;

            color.Color = Color.FromArgb(color.A, color.R, color.G, color.B);
            color.AColor.Opacity = color.A / 255.0;

            Canvas.SetLeft(color.ASliderCursor, color.A * color.ASlider.ActualWidth / 255);

            color.IsChangeFromMe = false;

            color.ColorChanged?.Invoke(color, color.Color);
        }

        private static void RGBPropertyChange(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
                return;

            var color = (ColorPicker)source;

            if (color.IsChangeFromMe)
                return;

            color.IsChangeFromMe = true;

            color.Color = Color.FromArgb(color.A, color.R, color.G, color.B);
            ColorExtensions.ColorToHSV(color.Color, out var h, out var s, out var v);

            color.H = h;
            color.S = s;
            color.V = v;

            color.BoxColor.Color = ColorExtensions.ColorFromHSV(h, s, v);
            color.ColorStop.Color = ColorExtensions.ColorFromHSV(h, 1, 1);

            Canvas.SetLeft(color.SliderCursor, h * color.Slider.ActualWidth / 360);
            Canvas.SetLeft(color.BoxCursor, color.Box.ActualWidth * s);
            Canvas.SetTop(color.BoxCursor, color.Box.ActualHeight - color.Box.ActualHeight * v);

            color.IsChangeFromMe = false;

            color.ColorChanged?.Invoke(color, color.Color);
        }

        private static void HSVPropertyChange(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
                return;

            var color = (ColorPicker)source;

            if (color.IsChangeFromMe)
                return;

            color.IsChangeFromMe = true;

            color.BoxColor.Color = ColorExtensions.ColorFromHSV(color.H, color.S, color.V);
            color.Color = color.BoxColor.Color.WithAlpha(color.A);

            color.R = color.Color.R;
            color.G = color.Color.G;
            color.B = color.Color.B;

            if (e.Property.Name == nameof(S) || e.Property.Name == nameof(V))
            {
                Canvas.SetLeft(color.BoxCursor, color.Box.ActualWidth * color.S);
                Canvas.SetTop(color.BoxCursor, color.Box.ActualHeight - color.Box.ActualHeight * color.V);
            }

            if (e.Property.Name == nameof(H))
            {
                color.ColorStop.Color = ColorExtensions.ColorFromHSV(color.H, 1, 1);

                Canvas.SetLeft(color.SliderCursor, color.H * color.Slider.ActualWidth / 360);
            }

            color.IsChangeFromMe = false;

            color.ColorChanged?.Invoke(color, color.Color);
        }

        private static void ShowAlphaPropertyChange(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
                return;

            var color = (ColorPicker)source;

            color.ASliderBG.Visibility = color.ShowAlpha ? Visibility.Visible : Visibility.Collapsed;
            color.ASlider.Visibility = color.ShowAlpha ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool BoxMove = false;
        private bool SliderMove = false;
        private bool ASliderMove = false;

        private void OnBoxMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                BoxMove = true;
                Box.CaptureMouse();

                SetSV(e.GetPosition(Box));
            }
        }

        private void OnBoxMouseMove(object sender, MouseEventArgs e)
        {
            if (BoxMove && e.LeftButton == MouseButtonState.Pressed)
            {
                SetSV(e.GetPosition(Box));
            }
        }

        private void OnBoxMouseUp(object sender, MouseButtonEventArgs e)
        {
            BoxMove = false;
            Box.ReleaseMouseCapture();
        }

        private void OnSliderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SliderMove = true;
                Slider.CaptureMouse();

                SetH(e.GetPosition(Slider).X);
            }
        }

        private void OnSliderMouseMove(object sender, MouseEventArgs e)
        {
            if (SliderMove && e.LeftButton == MouseButtonState.Pressed)
            {
                SetH(e.GetPosition(Slider).X);
            }
        }

        private void OnSliderMouseUp(object sender, MouseButtonEventArgs e)
        {
            SliderMove = false;
            Slider.ReleaseMouseCapture();
        }

        private void OnASliderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ASliderMove = true;
                ASlider.CaptureMouse();

                SetA(e.GetPosition(ASlider).X);
            }
        }

        private void OnASliderMouseMove(object sender, MouseEventArgs e)
        {
            if (ASliderMove && e.LeftButton == MouseButtonState.Pressed)
            {
                SetA(e.GetPosition(ASlider).X);
            }
        }

        private void OnASliderMouseUp(object sender, MouseButtonEventArgs e)
        {
            ASliderMove = false;
            ASlider.ReleaseMouseCapture();
        }

        private void SetSV(Point pos)
        {
            if (pos.X < 0)
                pos.X = 0;

            if (pos.Y < 0)
                pos.Y = 0;

            if (pos.X > Box.ActualWidth)
                pos.X = Box.ActualWidth;

            if (pos.Y > Box.ActualHeight)
                pos.Y = Box.ActualHeight;

            S = pos.X / Box.ActualWidth;
            V = (100.0 - pos.Y * 100.0 / Box.ActualHeight) / 100.0;
        }

        private void SetH(double x)
        {
            if (x < 0)
                x = 0;

            if (x > Slider.ActualWidth)
                x = Slider.ActualWidth;

            H = x / Slider.ActualWidth * 360.0;
        }

        private void SetA(double x)
        {
            if (x < 0)
                x = 0;

            if (x > ASlider.ActualWidth)
                x = ASlider.ActualWidth;

            A = (byte)Math.Round(255 * x / ASlider.ActualWidth);
        }
    }
}
