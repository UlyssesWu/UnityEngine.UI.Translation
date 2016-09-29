extern alias U;
using Mathf = U::UnityEngine.Mathf;
using GameObject = U::UnityEngine.GameObject;

namespace UnityEngine.UI.Translation
{
    using System.Collections.Generic;

    internal abstract class SubtitleUserInterfaceBase<T> : U::UnityEngine.MonoBehaviour, ISubtitleUserInterface where T : U::UnityEngine.MonoBehaviour
    {
        private bool _Bold;
        private U::UnityEngine.Color _FontColor;
        private string _FontName;
        private int _FontSize;
        private static T _instance;
        private bool _Italic;
        private float _MarginBottom;
        private float _MarginLeft;
        private float _MarginRight;
        private float _MarginTop;
        private const string DEFAULT_FONT_NAME = "Arial";
        private const int DEFAULT_FONT_SIZE = 16;
        public const int MAX_FONT_SIZE = 72;
        public const int MIN_FONT_SIZE = 8;
        public const float TARGET_DPI = 72f;

        public SubtitleUserInterfaceBase()
        {
            this._FontName = "Arial";
            this._FontSize = 16;
            this._FontColor = U::UnityEngine.Color.white;
        }

        protected virtual void Awake()
        {
            if (this != SubtitleUserInterfaceBase<T>.Instance)
            {
                U::UnityEngine.Object.DestroyImmediate(this);
            }
        }

        protected virtual void OnFontColorChanged(object sender, FontColorChangedEventArgs e)
        {
        }

        protected virtual void OnFontMarginChanged(object sender, FontMarginChangedEventArgs e)
        {
        }

        protected virtual void OnFontNameChanged(object sender, FontNameChangedEventArgs e)
        {
        }

        protected virtual void OnFontSizeChanged(object sender, FontSizeChangedEventArgs e)
        {
        }

        protected virtual void OnFontStyleChanged(object sender, FontStyleChangedEventArgs e)
        {
        }

        public abstract IEnumerable<TextPosition> Anchors { get; }

        public bool Bold
        {
            get
            {
                return this._Bold;
            }
            set
            {
                if (value != this._Bold)
                {
                    this._Bold = value;
                    this.OnFontStyleChanged(this, new FontStyleChangedEventArgs(value, this.Italic));
                }
            }
        }

        public U::UnityEngine.Color FontColor
        {
            get
            {
                return this._FontColor;
            }
            set
            {
                this._FontColor.a = 1f;
                if (value != this._FontColor)
                {
                    this._FontColor = value;
                    this.OnFontColorChanged(this, new FontColorChangedEventArgs(value));
                }
            }
        }

        public string FontName
        {
            get
            {
                return this._FontName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = "Arial";
                }
                if (value != this._FontName)
                {
                    this._FontName = value;
                    this.OnFontNameChanged(this, new FontNameChangedEventArgs(value));
                }
            }
        }

        public int FontSize
        {
            get
            {
                return this._FontSize;
            }
            set
            {
                value = Mathf.Min(Mathf.Max(value, 8), 0x48);
                if (value != this._FontSize)
                {
                    this._FontSize = value;
                    this.OnFontSizeChanged(this, new FontSizeChangedEventArgs(value));
                }
            }
        }

        public static T Instance
        {
            get
            {
                if (SubtitleUserInterfaceBase<T>._instance == null)
                {
                    SubtitleUserInterfaceBase<T>._instance = U::UnityEngine.Object.FindObjectOfType<T>();
                    if (SubtitleUserInterfaceBase<T>._instance == null)
                    {
                        GameObject target = new GameObject("SubtitleUserInterface");
                        SubtitleUserInterfaceBase<T>._instance = target.AddComponent<T>();
                        U::UnityEngine.Object.DontDestroyOnLoad(target);
                    }
                }
                return SubtitleUserInterfaceBase<T>._instance;
            }
        }

        public bool Italic
        {
            get
            {
                return this._Italic;
            }
            set
            {
                if (value != this._Italic)
                {
                    this._Italic = value;
                    this.OnFontStyleChanged(this, new FontStyleChangedEventArgs(this.Bold, value));
                }
            }
        }

        public abstract string this[TextPosition anchor] { get; set; }

        public float MarginBottom
        {
            get
            {
                return this._MarginBottom;
            }
            set
            {
                if (value != this._MarginBottom)
                {
                    this._MarginBottom = value;
                    this.OnFontMarginChanged(this, new FontMarginChangedEventArgs(this.MarginLeft, this.MarginTop, this.MarginRight, value));
                }
            }
        }

        public float MarginLeft
        {
            get
            {
                return this._MarginLeft;
            }
            set
            {
                if (value != this._MarginLeft)
                {
                    this._MarginLeft = value;
                    this.OnFontMarginChanged(this, new FontMarginChangedEventArgs(value, this.MarginTop, this.MarginRight, this.MarginBottom));
                }
            }
        }

        public float MarginRight
        {
            get
            {
                return this._MarginRight;
            }
            set
            {
                if (value != this._MarginRight)
                {
                    this._MarginRight = value;
                    this.OnFontMarginChanged(this, new FontMarginChangedEventArgs(this.MarginLeft, this.MarginTop, value, this.MarginBottom));
                }
            }
        }

        public float MarginTop
        {
            get
            {
                return this._MarginTop;
            }
            set
            {
                if (value != this._MarginTop)
                {
                    this._MarginTop = value;
                    this.OnFontMarginChanged(this, new FontMarginChangedEventArgs(this.MarginLeft, value, this.MarginRight, this.MarginBottom));
                }
            }
        }
    }
}

