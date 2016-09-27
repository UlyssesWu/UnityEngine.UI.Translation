extern alias U;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Canvas = U::UnityEngine.Canvas;
using Font = U::UnityEngine.Font;
using RenderMode = U::UnityEngine.RenderMode;
using GameObject = U::UnityEngine.GameObject;
using TextAnchor = U::UnityEngine.TextAnchor;
using Vector2 = U::UnityEngine.Vector2;
using Color = U::UnityEngine.Color;
using FontStyle = U::UnityEngine.FontStyle;
using Screen = U::UnityEngine.Screen;
using Mathf = U::UnityEngine.Mathf;

namespace UnityEngine.UI.Translation
{
    internal class SubtitleCanvas : SubtitleUserInterfaceBase<SubtitleCanvas>, ISubtitleBorder, ISubtitleShadow
    {
        private int _BorderWidth = 1;
        private int _ShadowOffset = 1;
        private Dictionary<TextPosition, TextData> anchors = new Dictionary<TextPosition, TextData>();
        private Canvas canvas;
        private static Func<string, int, Font> CreateDynamicFontFromOSFont;
        private const int DEFAULT_BORDER_WIDTH = 1;
        private float dpi;

        static SubtitleCanvas()
        {
            System.Type[] types = new System.Type[] { typeof(string), typeof(int) };
            MethodInfo method = typeof(Font).GetMethod("CreateDynamicFontFromOSFont", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, types, null);
            if (method != null)
            {
                CreateDynamicFontFromOSFont = (Func<string, int, Font>) Delegate.CreateDelegate(typeof(Func<string, int, Font>), method);
            }
        }

        protected override void Awake()
        {
            this.canvas = base.gameObject.AddComponent<Canvas>();
            this.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            this.canvas.sortingOrder = 0x7fff;
            HashSet<TextPosition> set = new HashSet<TextPosition>();
            foreach (TextPosition position in (TextPosition[]) Enum.GetValues(typeof(TextPosition)))
            {
                if (!set.Contains(position))
                {
                    set.Add(position);
                    GameObject obj1 = new GameObject("Subtitle" + position.ToString());
                    obj1.transform.SetParent(base.transform, false);
                    Text text = obj1.AddComponent<Text>();
                    text.Translate = false;
                    this.SetTextFont(text, base.FontName);
                    this.SetTextSize(text, base.FontSize);
                    this.SetTextColor(text, base.FontColor);
                    this.SetTextStyle(text, false, false);
                    this.SetTextAlignment(text, position);
                    this.SetTextMargin(text, 0f, 0f, 0f, 0f);
                    Outline border = obj1.AddComponent<Outline>();
                    this.SetTextBorder(border, this.BorderWidth);
                    Shadow shadow = obj1.AddComponent<Shadow>();
                    this.SetTextShadow(shadow, this.ShadowOffset);
                    this.anchors.Add(position, new TextData(text, border, shadow));
                }
            }
        }

        private void ChangeTextSettings(Action<TextPosition, TextData> action)
        {
            if (action != null)
            {
                foreach (KeyValuePair<TextPosition, TextData> pair in this.anchors)
                {
                    action(pair.Key, pair.Value);
                }
            }
        }

        public void Clear()
        {
            foreach (KeyValuePair<TextPosition, TextData> pair in this.anchors)
            {
                pair.Value.Text.text = string.Empty;
            }
        }

        private void LateUpdate()
        {
            this.UpdateCanvasScaleFactor();
        }

        protected virtual void OnFontBorderChanged(object sender, FontBorderChangedEventArgs e)
        {
            this.ChangeTextSettings((position, data) => this.SetTextBorder(data.Border, e.BorderWidth));
        }

        protected override void OnFontColorChanged(object sender, FontColorChangedEventArgs e)
        {
            this.ChangeTextSettings((position, data) => this.SetTextColor(data.Text, e.FontColor));
        }

        protected override void OnFontMarginChanged(object sender, FontMarginChangedEventArgs e)
        {
            this.ChangeTextSettings((position, data) => this.SetTextMargin(data.Text, e.MarginLeft, e.MarginTop, e.MarginRight, e.MarginBottom));
        }

        protected override void OnFontNameChanged(object sender, FontNameChangedEventArgs e)
        {
            this.ChangeTextSettings((position, data) => this.SetTextFont(data.Text, e.FontName));
        }

        protected virtual void OnFontShadowChanged(object sender, FontShadowChangedEventArgs e)
        {
            this.ChangeTextSettings((position, data) => this.SetTextShadow(data.Shadow, e.ShadowOffset));
        }

        protected override void OnFontSizeChanged(object sender, FontSizeChangedEventArgs e)
        {
            this.ChangeTextSettings((position, data) => this.SetTextSize(data.Text, e.FontSize));
        }

        protected override void OnFontStyleChanged(object sender, FontStyleChangedEventArgs e)
        {
            this.ChangeTextSettings((position, data) => this.SetTextStyle(data.Text, e.Bold, e.Italic));
        }

        private void SetTextAlignment(Text text, TextPosition position)
        {
            switch (position)
            {
                case TextPosition.LowerLeft:
                    text.alignment = TextAnchor.LowerLeft;
                    return;

                case TextPosition.LowerCenter:
                    text.alignment = TextAnchor.LowerCenter;
                    return;

                case TextPosition.LowerRight:
                    text.alignment = TextAnchor.LowerRight;
                    return;

                case TextPosition.MiddleLeft:
                    text.alignment = TextAnchor.MiddleLeft;
                    return;

                case TextPosition.MiddleCenter:
                    text.alignment = TextAnchor.MiddleCenter;
                    return;

                case TextPosition.MiddleRight:
                    text.alignment = TextAnchor.MiddleRight;
                    return;

                case TextPosition.UpperLeft:
                    text.alignment = TextAnchor.UpperLeft;
                    return;

                case TextPosition.UpperCenter:
                    text.alignment = TextAnchor.UpperCenter;
                    return;

                case TextPosition.UpperRight:
                    text.alignment = TextAnchor.UpperRight;
                    return;
            }
            text.alignment = TextAnchor.LowerCenter;
        }

        private void SetTextBorder(Outline border, int distance)
        {
            border.enabled = distance > 0;
            border.effectDistance = new Vector2((float) distance, (float) -distance);
        }

        private void SetTextColor(Text text, Color color)
        {
            text.color = color;
        }

        private void SetTextFont(Text text, string name)
        {
            if (CreateDynamicFontFromOSFont != null)
            {
                text.font = CreateDynamicFontFromOSFont(name, text.fontSize);
            }
            else
            {
                text.font = U::UnityEngine.Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
        }

        private void SetTextMargin(Text text, float left, float top, float right, float bottom)
        {
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.offsetMin = new Vector2(left, bottom);
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMax = new Vector2(-right, -top);
        }

        private void SetTextShadow(Shadow shadow, int distance)
        {
            shadow.enabled = distance > 0;
            shadow.effectDistance = new Vector2((float) distance, (float) -distance);
        }

        private void SetTextSize(Text text, int size)
        {
            text.fontSize = size;
        }

        private void SetTextStyle(Text text, bool bold, bool italic)
        {
            if (bold)
            {
                if (italic)
                {
                    text.fontStyle = FontStyle.BoldAndItalic;
                }
                else
                {
                    text.fontStyle = FontStyle.Bold;
                }
            }
            else if (italic)
            {
                text.fontStyle = FontStyle.Italic;
            }
            else
            {
                text.fontStyle = FontStyle.Normal;
            }
        }

        private void UpdateCanvasScaleFactor()
        {
            float dpi = Screen.dpi;
            if (this.dpi != dpi)
            {
                this.dpi = dpi;
                dpi = (this.dpi == 0f) ? 72f : this.dpi;
                this.canvas.scaleFactor = dpi / 72f;
            }
        }

        public override IEnumerable<TextPosition> Anchors
        {
            get
            {
                return this.anchors.Keys;
            }
        }

        public int BorderWidth
        {
            get
            {
                return this._BorderWidth;
            }
            set
            {
                value = Mathf.Min(Mathf.Max(value, 0), 1);
                if (value != this._BorderWidth)
                {
                    this._BorderWidth = value;
                    this.OnFontBorderChanged(this, new FontBorderChangedEventArgs(value));
                }
            }
        }

        public override string this[TextPosition anchor]
        {
            get
            {
                TextData data;
                if (!this.anchors.TryGetValue(anchor, out data))
                {
                    throw new ArgumentOutOfRangeException("anchor");
                }
                return data.Text.text;
            }
            set
            {
                TextData data;
                if (!this.anchors.TryGetValue(anchor, out data))
                {
                    throw new ArgumentOutOfRangeException("anchor");
                }
                if (value == null)
                {
                    value = string.Empty;
                }
                if (data.Text.text != value)
                {
                    data.Text.text = value;
                }
            }
        }

        public int ShadowOffset
        {
            get
            {
                return this._ShadowOffset;
            }
            set
            {
                value = Mathf.Min(Mathf.Max(value, 0), 1);
                if (value != this._ShadowOffset)
                {
                    this._ShadowOffset = value;
                    this.OnFontShadowChanged(this, new FontShadowChangedEventArgs(value));
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TextData
        {
            private UnityEngine.UI.Text _Text;
            private Outline _Border;
            private UnityEngine.UI.Shadow _Shadow;
            public UnityEngine.UI.Text Text
            {
                get
                {
                    return this._Text;
                }
            }
            public Outline Border
            {
                get
                {
                    return this._Border;
                }
            }
            public UnityEngine.UI.Shadow Shadow
            {
                get
                {
                    return this._Shadow;
                }
            }
            public TextData(UnityEngine.UI.Text text, Outline border, UnityEngine.UI.Shadow shadow)
            {
                this._Text = text;
                this._Border = border;
                this._Shadow = shadow;
            }
        }
    }
}

