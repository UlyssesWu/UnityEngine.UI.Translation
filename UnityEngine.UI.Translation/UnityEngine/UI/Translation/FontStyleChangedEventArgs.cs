namespace UnityEngine.UI.Translation
{
    using System;

    internal class FontStyleChangedEventArgs : EventArgs
    {
        private readonly bool _Bold;
        private readonly bool _Italic;

        public FontStyleChangedEventArgs(bool bold, bool italic)
        {
            this._Bold = bold;
            this._Italic = italic;
        }

        public bool Bold
        {
            get
            {
                return this._Bold;
            }
        }

        public bool Italic
        {
            get
            {
                return this._Italic;
            }
        }
    }
}

