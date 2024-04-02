using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace AE.Dal
{
	[AESerializable]
	public struct ColorPath
	{
		public static IReadOnlyDictionary<ColorKey, Color> Colors { get; } = new ReadOnlyDictionary<ColorKey, Color>(
			new Dictionary<ColorKey, Color>
			{
				{ ColorKey.SilverNight,        Color.FromArgb(176, 181, 185) },
				{ ColorKey.CrimsonRed,         Color.FromArgb(149, 0, 0)     },
				{ ColorKey.TropicalRainForest, Color.FromArgb(0, 111, 101)   },
				{ ColorKey.Eucalyptus,         Color.FromArgb(30, 227, 175)  },
				{ ColorKey.Celadon,            Color.FromArgb(173, 227, 174) },
				{ ColorKey.HarlequinGreen,     Color.FromArgb(55, 213, 0)    },
				{ ColorKey.PaleViolet,         Color.FromArgb(205, 130, 254) },
				{ ColorKey.ShockingPink,       Color.FromArgb(254, 0, 212)   },
				{ ColorKey.ChinesePink,        Color.FromArgb(232, 124, 170) },
				{ ColorKey.RipeMango,          Color.FromArgb(255, 202, 34)  },
				{ ColorKey.BlueEyes,           Color.FromArgb(160, 180, 254) },
				{ ColorKey.PhilippineOrange,   Color.FromArgb(254, 115, 0)   },
				{ ColorKey.AbsoluteZero,       Color.FromArgb(0, 88, 203)    },
				{ ColorKey.Calamansi,          Color.FromArgb(240, 252, 168) },
				{ ColorKey.VividCerulean,      Color.FromArgb(0, 168, 242)   },
			});

		public ColorKey Key { get; set; }
		public ColorType Type { get; set; }
		public int Factor { get; set; }
		public byte Opacity { get; set; }

		public ColorPath(ColorKey key, ColorType type = ColorType.Color, int factor = 0, byte opacity = 255)
		{
			if (!Colors.ContainsKey(key))
				throw new KeyNotFoundException(nameof(key));

			Key = key;
			Type = type;
			Factor = factor;
			Opacity = opacity;
		}

		public readonly Color ToColor()
		{
			var color = Colors[Key];
			return GetColor(Opacity, color.R, color.G, color.B, Type, Factor);
		}

		public static Color GetColor(byte a, byte r, byte g, byte b, ColorType type, int factor)
		{
			if (factor < 0 || factor > 10)
				throw new ArgumentOutOfRangeException(nameof(factor));

			if (type == ColorType.Shade)
			{
				var f = (10 - factor) / 10.0;

				r = (byte)Math.Round(r * f);
				g = (byte)Math.Round(g * f);
				b = (byte)Math.Round(b * f);
			}
			else if (type == ColorType.Tint)
			{
				var f = factor / 10.0;

				r = (byte)Math.Round(r + ((255 - r) * f));
				g = (byte)Math.Round(g + ((255 - g) * f));
				b = (byte)Math.Round(b + ((255 - b) * f));
			}

			return Color.FromArgb(a, r, g, b);
		}
	}
}
