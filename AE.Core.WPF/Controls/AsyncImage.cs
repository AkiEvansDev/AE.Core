using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AE.Core.WPF.Controls
{
    public delegate void AsyncImageLoadEventHandler(AsyncImage image);

    public class AsyncImage : Image
    {
        public static readonly DependencyProperty ImagePathProperty =
            DependencyProperty.Register(nameof(ImagePath), typeof(string), typeof(AsyncImage), new PropertyMetadata(async (o, e) => await ((AsyncImage)o).LoadImageAsync((string)e.NewValue)));

        public static readonly DependencyProperty ImageLoadCommandProperty =
            DependencyProperty.Register(nameof(ImageLoadCommand), typeof(ICommand), typeof(AsyncImage), new PropertyMetadata());

        public static readonly DependencyProperty DecodePixelWidthProperty =
            DependencyProperty.Register(nameof(DecodePixelWidth), typeof(int), typeof(AsyncImage), new PropertyMetadata());

        public static readonly DependencyProperty DecodePixelHeightProperty =
            DependencyProperty.Register(nameof(DecodePixelHeight), typeof(int), typeof(AsyncImage), new PropertyMetadata());

        public event AsyncImageLoadEventHandler ImageLoad;

        public string ImagePath
        {
            get => (string)GetValue(ImagePathProperty);
            set => SetValue(ImagePathProperty, value);
        }

        public ICommand ImageLoadCommand
        {
            get => (ICommand)GetValue(ImageLoadCommandProperty);
            set => SetValue(ImageLoadCommandProperty, value);
        }

        public int DecodePixelWidth
        {
            get => (int)GetValue(DecodePixelWidthProperty);
            set => SetValue(DecodePixelWidthProperty, value);
        }

        public int DecodePixelHeight
        {
            get => (int)GetValue(DecodePixelHeightProperty);
            set => SetValue(DecodePixelHeightProperty, value);
        }

        private async Task LoadImageAsync(string imagePath)
        {
            Source = await Task.Run(() =>
            {
                using var stream = File.OpenRead(imagePath);
                var bi = new BitmapImage();

                bi.BeginInit();

                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.DecodePixelWidth = DecodePixelWidth;
                bi.DecodePixelHeight = DecodePixelHeight;
                bi.StreamSource = stream;

                bi.EndInit();
                bi.Freeze();

                return bi;
            });

            ImageLoad?.Invoke(this);
            ImageLoadCommand?.Execute(this);
        }
    }
}
