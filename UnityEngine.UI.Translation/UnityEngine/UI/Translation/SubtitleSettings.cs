extern alias U;
using System;
using System.Globalization;
using System.IO;
using Color32 = U::UnityEngine.Color32;
using Color = U::UnityEngine.Color;
using JsonUtility = U::UnityEngine.JsonUtility;

namespace UnityEngine.UI.Translation
{
    internal static class ColorUtility
    {
        public static Color32 ToColor(string hex)
        {
            if ((string.IsNullOrEmpty(hex) || !hex.StartsWith("#")) || (hex.Length != 7))
            {
                return Color.white;
            }
            byte g = byte.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
            return new Color32(byte.Parse(hex.Substring(1, 2), NumberStyles.HexNumber), g, byte.Parse(hex.Substring(5, 2), NumberStyles.HexNumber), 0xff);
        }

        public static string ToHex(Color32 color)
        {
            return (("#" + color.r.ToString("X2")) + color.g.ToString("X2") + color.b.ToString("X2"));
        }
    }

    internal static class SubtitleSettings
	{
        internal static string SettingsFileName => "Subtitle.json";//"Translation.ini";

        internal static string SettingsFilePath => string.Concat(IniSettings.SettingsFileDir, SettingsFileName);

	    private const string SECTION = "Subtitles";

		private const string ANCHORKEY = "iAnchor";

		private const string FONTNAMEKEY = "sFontName";

		private const string FONTSIZEKEY = "iFontSize";

		private const string FONTCOLORKEY = "sFontColor";

		private const string BOLDKEY = "bBold";

		private const string ITALICKEY = "bItalic";

		private const string BORDERWIDTHKEY = "iBorderWidth";

		private const string SHADOWOFFSETKEY = "iShadowOffset";

		private const string MARGINLEFTKEY = "iMarginLeft";

		private const string MARGINTOPKEY = "iMarginTop";

		private const string MARGINRIGHTKEY = "iMarginRight";

		private const string MARGINBOTTOMKEY = "iMarginBottom";

		private static TextPosition _Anchor;

		private static string _FontName;

		private static int _FontSize;

		private static Color _FontColor;

		private static bool _Bold;

		private static bool _Italic;

		private static int _BorderWidth;

		private static int _ShadowOffset;

		private static int _MarginLeft;

		private static int _MarginTop;

		private static int _MarginRight;

		private static int _MarginBottom;

		private static bool initialized;

		internal static TextPosition Anchor
		{
			get
			{
				return SubtitleSettings._Anchor;
			}
			private set
			{
				if (value != SubtitleSettings._Anchor)
				{
					SubtitleSettings._Anchor = value;
					Action<TextPosition> action = SubtitleSettings.AnchorChanged;
					if (action != null && SubtitleSettings.initialized)
					{
						action(value);
					}
				}
			}
		}

		internal static bool Bold
		{
			get
			{
				return SubtitleSettings._Bold;
			}
			private set
			{
				if (value != SubtitleSettings._Bold)
				{
					SubtitleSettings._Bold = value;
					Action<bool> action = SubtitleSettings.BoldChanged;
					if (action != null && SubtitleSettings.initialized)
					{
						action(value);
					}
				}
			}
		}

		internal static int BorderWidth
		{
			get
			{
				return SubtitleSettings._BorderWidth;
			}
			private set
			{
				if (value != SubtitleSettings._BorderWidth)
				{
					SubtitleSettings._BorderWidth = value;
					Action<int> action = SubtitleSettings.BorderWidthChanged;
					if (action != null && SubtitleSettings.initialized)
					{
						action(value);
					}
				}
			}
		}

		internal static Color FontColor
		{
			get
			{
				return SubtitleSettings._FontColor;
			}
			private set
			{
				if (value != SubtitleSettings._FontColor)
				{
					SubtitleSettings._FontColor = value;
					Action<Color> action = SubtitleSettings.FontColorChanged;
					if (action != null && SubtitleSettings.initialized)
					{
						action(value);
					}
				}
			}
		}

		internal static string FontName
		{
			get
			{
				if (string.IsNullOrEmpty(SubtitleSettings._FontName))
				{
					SubtitleSettings._FontName = "Arial";
				}
				return SubtitleSettings._FontName;
			}
			private set
			{
				if (string.IsNullOrEmpty(value))
				{
					value = "Arial";
				}
				if (value != SubtitleSettings._FontName)
				{
					SubtitleSettings._FontName = value;
					Action<string> action = SubtitleSettings.FontNameChanged;
					if (action != null && SubtitleSettings.initialized)
					{
						action(value);
					}
				}
			}
		}

		internal static int FontSize
		{
			get
			{
				return SubtitleSettings._FontSize;
			}
			private set
			{
				if (value != SubtitleSettings._FontSize)
				{
					SubtitleSettings._FontSize = value;
					Action<int> action = SubtitleSettings.FontSizeChanged;
					if (action != null && SubtitleSettings.initialized)
					{
						action(value);
					}
				}
			}
		}

		internal static bool Italic
		{
			get
			{
				return SubtitleSettings._Italic;
			}
			private set
			{
				if (value != SubtitleSettings._Italic)
				{
					SubtitleSettings._Italic = value;
					Action<bool> action = SubtitleSettings.ItalicChanged;
					if (action != null && SubtitleSettings.initialized)
					{
						action(value);
					}
				}
			}
		}

		internal static int MarginBottom
		{
			get
			{
				return SubtitleSettings._MarginBottom;
			}
			private set
			{
				if (value != SubtitleSettings._MarginBottom)
				{
					SubtitleSettings._MarginBottom = value;
					Action<int> action = SubtitleSettings.MarginBottomChanged;
					if (action != null && SubtitleSettings.initialized)
					{
						action(value);
					}
				}
			}
		}

		internal static int MarginLeft
		{
			get
			{
				return SubtitleSettings._MarginLeft;
			}
			private set
			{
				if (value != SubtitleSettings._MarginLeft)
				{
					SubtitleSettings._MarginLeft = value;
					Action<int> action = SubtitleSettings.MarginLeftChanged;
					if (action != null && SubtitleSettings.initialized)
					{
						action(value);
					}
				}
			}
		}

		internal static int MarginRight
		{
			get
			{
				return SubtitleSettings._MarginRight;
			}
			private set
			{
				if (value != SubtitleSettings._MarginRight)
				{
					SubtitleSettings._MarginRight = value;
					Action<int> action = SubtitleSettings.MarginRightChanged;
					if (action != null && SubtitleSettings.initialized)
					{
						action(value);
					}
				}
			}
		}

		internal static int MarginTop
		{
			get
			{
				return SubtitleSettings._MarginTop;
			}
			private set
			{
				if (value != SubtitleSettings._MarginTop)
				{
					SubtitleSettings._MarginTop = value;
					Action<int> action = SubtitleSettings.MarginTopChanged;
					if (action != null && SubtitleSettings.initialized)
					{
						action(value);
					}
				}
			}
		}

		internal static int ShadowOffset
		{
			get
			{
				return SubtitleSettings._ShadowOffset;
			}
			private set
			{
				if (value != SubtitleSettings._ShadowOffset)
				{
					SubtitleSettings._ShadowOffset = value;
					Action<int> action = SubtitleSettings.ShadowOffsetChanged;
					if (action != null && SubtitleSettings.initialized)
					{
						action(value);
					}
				}
			}
		}

		static SubtitleSettings()
        {
#if UNITY4
            IniSettings.LoadSettings += new Action<IniFile>(SubtitleSettings.LoadIni);
            IniSettings.LoadIni();
#else
            Load();
#endif
        }
        private static void Load()
        {
            var config = JsonUtility.FromJson<SubtitleConfig>(SettingsFilePath);
            if (config == null)
            {
                config = new SubtitleConfig();
                File.WriteAllText(SettingsFilePath, JsonUtility.ToJson(config, true));
            }
            Anchor = Anchor.Parse(config.Anchor.ToString(), SubtitleSettings.Anchor);
            FontName = config.FontName;
            FontSize = config.FontSize;
            FontColor = ColorUtility.ToColor(config.FontColor);
            Bold = config.Bold;
            Italic = config.Italic;
            BorderWidth = config.BorderWidth;
            ShadowOffset = config.ShadowOffset;
            MarginLeft = config.MarginLeft;
            MarginTop = config.MarginTop;
            MarginRight = config.MarginRight;
            MarginBottom = config.MarginBottom;
            SubtitleSettings.initialized = true;
        }

        private static void LoadIni(IniFile ini)
		{
			int num;
			bool flag;
			string str = "iAnchor";
			string value = ini.GetValue("Subtitles", str, null);
			if (value == null || !int.TryParse(value, out num))
			{
				num = 2;
				ini.WriteValue("Subtitles", str, num);
			}
			SubtitleSettings.Anchor = SubtitleSettings.Anchor.Parse(num.ToString(), SubtitleSettings.Anchor);
			str = "sFontName";
			value = ini.GetValue("Subtitles", str, null);
			SubtitleSettings.FontName = value;
			if (value != SubtitleSettings.FontName)
			{
				ini.WriteValue("Subtitles", str, SubtitleSettings.FontName);
			}
			str = "iFontSize";
			value = ini.GetValue("Subtitles", str, null);
			if (value == null || !int.TryParse(value, out num))
			{
				num = 16;
				ini.WriteValue("Subtitles", str, num);
			}
			SubtitleSettings.FontSize = num;
			str = "sFontColor";
			value = ini.GetValue("Subtitles", str, null);
			SubtitleSettings.FontColor = ColorUtility.ToColor(value);
			string hex = ColorUtility.ToHex(SubtitleSettings.FontColor);
			if (value != hex)
			{
				ini.WriteValue("Subtitles", str, hex);
			}
			str = "bBold";
			value = ini.GetValue("Subtitles", str, null);
			if (value == null || !bool.TryParse(value, out flag))
			{
				flag = true;
				ini.WriteValue("Subtitles", str, flag);
			}
			SubtitleSettings.Bold = flag;
			str = "bItalic";
			value = ini.GetValue("Subtitles", str, null);
			if (value == null || !bool.TryParse(value, out flag))
			{
				flag = false;
				ini.WriteValue("Subtitles", str, flag);
			}
			SubtitleSettings.Italic = flag;
			str = "iBorderWidth";
			value = ini.GetValue("Subtitles", str, null);
			if (value == null || !int.TryParse(value, out num))
			{
				num = 2;
				ini.WriteValue("Subtitles", str, num);
			}
			SubtitleSettings.BorderWidth = num;
			str = "iShadowOffset";
			value = ini.GetValue("Subtitles", str, null);
			if (value == null || !int.TryParse(value, out num))
			{
				num = 3;
				ini.WriteValue("Subtitles", str, num);
			}
			SubtitleSettings.ShadowOffset = num;
			str = "iMarginLeft";
			value = ini.GetValue("Subtitles", str, null);
			if (value == null || !int.TryParse(value, out num))
			{
				num = 20;
				ini.WriteValue("Subtitles", str, num);
			}
			SubtitleSettings.MarginLeft = num;
			str = "iMarginTop";
			value = ini.GetValue("Subtitles", str, null);
			if (value == null || !int.TryParse(value, out num))
			{
				num = 20;
				ini.WriteValue("Subtitles", str, num);
			}
			SubtitleSettings.MarginTop = num;
			str = "iMarginRight";
			value = ini.GetValue("Subtitles", str, null);
			if (value == null || !int.TryParse(value, out num))
			{
				num = 20;
				ini.WriteValue("Subtitles", str, num);
			}
			SubtitleSettings.MarginRight = num;
			str = "iMarginBottom";
			value = ini.GetValue("Subtitles", str, null);
			if (value == null || !int.TryParse(value, out num))
			{
				num = 20;
				ini.WriteValue("Subtitles", str, num);
			}
			SubtitleSettings.MarginBottom = num;
			SubtitleSettings.initialized = true;
		}

		internal static event Action<TextPosition> AnchorChanged;

		internal static event Action<bool> BoldChanged;

		internal static event Action<int> BorderWidthChanged;

		internal static event Action<Color> FontColorChanged;

		internal static event Action<string> FontNameChanged;

		internal static event Action<int> FontSizeChanged;

		internal static event Action<bool> ItalicChanged;

		internal static event Action<int> MarginBottomChanged;

		internal static event Action<int> MarginLeftChanged;

		internal static event Action<int> MarginRightChanged;

		internal static event Action<int> MarginTopChanged;

		internal static event Action<int> ShadowOffsetChanged;
	}
}