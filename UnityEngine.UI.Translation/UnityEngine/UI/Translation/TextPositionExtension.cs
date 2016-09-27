namespace UnityEngine.UI.Translation
{
    using System;

    internal static class TextPositionExtension
    {
        public static TextPosition Parse(this TextPosition tp, string value, TextPosition defaultValue = TextPosition.LowerCenter)
        {
            TextPosition position = (TextPosition) 0;
            try
            {
                position = (TextPosition) Enum.Parse(typeof(TextPosition), value, true);
            }
            catch
            {
            }
            if (!Enum.IsDefined(typeof(TextPosition), position))
            {
                position = defaultValue;
            }
            return position;
        }
    }
}

