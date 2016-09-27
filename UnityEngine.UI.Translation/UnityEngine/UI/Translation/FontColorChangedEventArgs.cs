extern alias U;
namespace UnityEngine.UI.Translation
{
    using System;

    internal class FontColorChangedEventArgs : EventArgs
    {
        private readonly U::UnityEngine.Color _FontColor;

        public FontColorChangedEventArgs(U::UnityEngine.Color color)
        {
            this._FontColor = color;
        }

        public U::UnityEngine.Color FontColor
        {
            get
            {
                return this._FontColor;
            }
        }
    }
}

