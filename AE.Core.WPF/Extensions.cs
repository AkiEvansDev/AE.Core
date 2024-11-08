using AE.Dal;
using System.Windows;
using DColor = System.Drawing.Color;
using MColor = System.Windows.Media.Color;

namespace AE.Core.WPF
{
    /// <summary>
    /// Expansions
    /// </summary>
    public static class Extensions
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
        /// Get resource color
        /// </summary>
        /// <param name="resource">resource name (string)</param>
        /// <returns></returns>
        public static MColor GetResourseColor(this object resource)
        {
            return Application.Current.Resources[resource.ToString()].ToString().ToColor();
        }
    }
}
