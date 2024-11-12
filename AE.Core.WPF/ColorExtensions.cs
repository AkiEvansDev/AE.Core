using AE.Dal;
using System.Windows;
using DColor = System.Drawing.Color;
using MColor = System.Windows.Media.Color;

namespace AE.Core.WPF
{
    /// <summary>
    /// Expansions
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// HEX string to color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="factorType"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static MColor ToColor(this string color, FactorType factorType = FactorType.Color, int factor = 0)
        {
            return ColorPath.GetColor(color, factorType, factor).ToColor();
        }

        /// <summary>
        /// System.Drawing.Color to System.Windows.Media.Color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static MColor ToColor(this DColor color)
        {
            return MColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// System.Windows.Media.Color to System.Drawing.Color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static DColor ToColor(this MColor color)
        {
            return DColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Color with alpha
        /// </summary>
        /// <param name="color"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public static MColor WithAlpha(this MColor color, byte alpha)
        {
            return MColor.FromArgb(alpha, color.R, color.G, color.B);
        }

        /// <summary>
        /// Color with alpha
        /// </summary>
        /// <param name="color"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public static DColor WithAlpha(this DColor color, byte alpha)
        {
            return DColor.FromArgb(alpha, color.R, color.G, color.B);
        }

        /// <summary>
        /// Get resource color
        /// </summary>
        /// <param name="resource">resource name (string)</param>
        /// <returns></returns>
        public static MColor GetResourseColor(this object resource)
        {
            return Application.Current.Resources[resource.ToString()].ToString().ToColor();
        }

        /// <summary>
        /// Get hue, saturation and value from color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="hue"></param>
        /// <param name="saturation"></param>
        /// <param name="value"></param>
        public static void ColorToHSV(MColor color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.ToColor().GetHue();
            saturation = (max == 0) ? 0 : 1.0 - (1.0 * min / max);
            value = max / 255.0;
        }

        public static MColor ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            var v = Convert.ToByte(value);
            var p = Convert.ToByte(value * (1 - saturation));
            var q = Convert.ToByte(value * (1 - f * saturation));
            var t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return MColor.FromArgb(255, v, t, p);
            else if (hi == 1)
                return MColor.FromArgb(255, q, v, p);
            else if (hi == 2)
                return MColor.FromArgb(255, p, v, t);
            else if (hi == 3)
                return MColor.FromArgb(255, p, q, v);
            else if (hi == 4)
                return MColor.FromArgb(255, t, p, v);
            else
                return MColor.FromArgb(255, v, p, q);
        }
    }
}
