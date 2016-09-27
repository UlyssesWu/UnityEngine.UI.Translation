namespace UnityEngine.UI.Translation
{
    using System;

    internal class FontSizeChangedEventArgs : EventArgs
    {
        private readonly int _FontSize;

        public FontSizeChangedEventArgs(int size)
        {
            this._FontSize = size;
        }

        public int FontSize
        {
            get
            {
                return this._FontSize;
            }
        }
    }
}

