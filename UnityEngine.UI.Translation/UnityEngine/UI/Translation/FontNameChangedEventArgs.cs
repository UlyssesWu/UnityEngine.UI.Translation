namespace UnityEngine.UI.Translation
{
    using System;

    internal class FontNameChangedEventArgs : EventArgs
    {
        private readonly string _FontName;

        public FontNameChangedEventArgs(string name)
        {
            this._FontName = name;
        }

        public string FontName
        {
            get
            {
                return this._FontName;
            }
        }
    }
}

