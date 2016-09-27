namespace UnityEngine.UI.Translation
{
    using System;

    internal class FontMarginChangedEventArgs : EventArgs
    {
        private readonly float _MarginBottom;
        private readonly float _MarginLeft;
        private readonly float _MarginRight;
        private readonly float _MarginTop;

        public FontMarginChangedEventArgs(float left, float top, float right, float bottom)
        {
            this._MarginLeft = left;
            this._MarginTop = top;
            this._MarginRight = right;
            this._MarginBottom = bottom;
        }

        public float MarginBottom
        {
            get
            {
                return this._MarginBottom;
            }
        }

        public float MarginLeft
        {
            get
            {
                return this._MarginLeft;
            }
        }

        public float MarginRight
        {
            get
            {
                return this._MarginRight;
            }
        }

        public float MarginTop
        {
            get
            {
                return this._MarginTop;
            }
        }
    }
}

