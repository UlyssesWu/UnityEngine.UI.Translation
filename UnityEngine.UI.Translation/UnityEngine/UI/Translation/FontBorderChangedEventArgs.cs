namespace UnityEngine.UI.Translation
{
    using System;

    internal class FontBorderChangedEventArgs : EventArgs
    {
        private readonly int _BorderWidth;

        public FontBorderChangedEventArgs(int borderWidth)
        {
            this._BorderWidth = borderWidth;
        }

        public int BorderWidth
        {
            get
            {
                return this._BorderWidth;
            }
        }
    }
}

