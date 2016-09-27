namespace UnityEngine.UI.Translation
{
    using System;

    internal class FontShadowChangedEventArgs : EventArgs
    {
        private readonly int _ShadowOffset;

        public FontShadowChangedEventArgs(int shadowOffset)
        {
            this._ShadowOffset = shadowOffset;
        }

        public int ShadowOffset
        {
            get
            {
                return this._ShadowOffset;
            }
        }
    }
}

