extern alias U;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Object = U::UnityEngine.Object;
namespace UnityEngine.UI.Translation
{
    internal class AudioSourceSubtitle : U::UnityEngine.MonoBehaviour
    {
		private static AudioSourceSubtitle _Instance;

		private bool reloadsubtitles;

		private OrderedDictionary subtitles;

		private Dictionary<TextPosition, StringBuilder> content;

		public static AudioSourceSubtitle Instance
		{
			get
			{
				if (AudioSourceSubtitle._Instance == null)
				{
					AudioSourceSubtitle._Instance = U::UnityEngine.Object.FindObjectOfType<AudioSourceSubtitle>();
					if (AudioSourceSubtitle._Instance == null)
					{
                        U::UnityEngine.GameObject gameObject = new U::UnityEngine.GameObject("AudioSourceSubtitle");
						AudioSourceSubtitle._Instance = gameObject.AddComponent<AudioSourceSubtitle>();
                        U::UnityEngine.Object.DontDestroyOnLoad(gameObject);
					}
				}
				return AudioSourceSubtitle._Instance;
			}
		}

		static AudioSourceSubtitle()
		{
			SubtitleSettings.FontNameChanged += new Action<string>((string value) => SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.FontName = value);
			SubtitleSettings.FontSizeChanged += new Action<int>((int value) => SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.FontSize = value);
			SubtitleSettings.FontColorChanged += value => SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.FontColor = value;
			SubtitleSettings.BoldChanged += new Action<bool>((bool value) => SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.Bold = value);
			SubtitleSettings.ItalicChanged += new Action<bool>((bool value) => SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.Italic = value);
			SubtitleSettings.BorderWidthChanged += new Action<int>((int value) => SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.BorderWidth = value);
			SubtitleSettings.ShadowOffsetChanged += new Action<int>((int value) => SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.ShadowOffset = value);
			SubtitleSettings.MarginLeftChanged += new Action<int>((int value) => SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.MarginLeft = (float)value);
			SubtitleSettings.MarginTopChanged += new Action<int>((int value) => SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.MarginTop = (float)value);
			SubtitleSettings.MarginRightChanged += new Action<int>((int value) => SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.MarginRight = (float)value);
			SubtitleSettings.MarginBottomChanged += new Action<int>((int value) => SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.MarginBottom = (float)value);
		}

		public AudioSourceSubtitle()
		{
			this.subtitles = new OrderedDictionary();
			this.content = new Dictionary<TextPosition, StringBuilder>();
		}

        public void Add(U::UnityEngine.AudioSource source)
		{
			try
			{
				this.subtitles.Remove(source);
				this.subtitles.Insert(0, source, new Subtitle(SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.Anchors, source));
			}
			catch (Exception exception)
			{
				IniSettings.Error(string.Concat("AudioSourceSubtitle::Load:\n", exception.ToString()));
			}
		}

		private void Awake()
		{
			if (this != AudioSourceSubtitle.Instance)
			{
				Object.DestroyImmediate(this);
			}
			foreach (TextPosition anchor in SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.Anchors)
			{
				this.content.Add(anchor, new StringBuilder(512));
			}
			SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.FontName = SubtitleSettings.FontName;
			SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.FontSize = SubtitleSettings.FontSize;
			SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.FontColor = SubtitleSettings.FontColor;
			SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.Bold = SubtitleSettings.Bold;
			SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.Italic = SubtitleSettings.Italic;
			SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.BorderWidth = SubtitleSettings.BorderWidth;
			SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.ShadowOffset = SubtitleSettings.ShadowOffset;
			SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.MarginLeft = (float)SubtitleSettings.MarginLeft;
			SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.MarginTop = (float)SubtitleSettings.MarginTop;
			SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.MarginRight = (float)SubtitleSettings.MarginRight;
			SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.MarginBottom = (float)SubtitleSettings.MarginBottom;
		}

		private void LateUpdate()
		{
			try
			{
				if (this.subtitles.Count != 0)
				{
					foreach (KeyValuePair<TextPosition, StringBuilder> keyValuePair in this.content)
					{
						if (keyValuePair.Value.Length <= 0)
						{
							continue;
						}
						keyValuePair.Value.Length = 0;
					}
					for (int i = this.subtitles.Count - 1; i >= 0; i--)
					{
						Subtitle item = this.subtitles[i] as Subtitle;
						if (item == null)
						{
							this.subtitles.RemoveAt(i);
						}
						else if (item.Source != null)
						{
							if (this.reloadsubtitles)
							{
								item.Reload();
							}
							item.LateUpdate();
							foreach (TextPosition anchor in SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.Anchors)
							{
								string str = item[anchor];
								if (str.Length <= 0)
								{
									continue;
								}
								if (this.content[anchor].Length > 0)
								{
									this.content[anchor].Append('\n');
								}
								this.content[anchor].Append(str);
							}
						}
						else
						{
							this.subtitles.RemoveAt(i);
						}
					}
					this.reloadsubtitles = false;
					foreach (TextPosition textPosition in SubtitleUserInterfaceBase<SubtitleCanvas>.Instance.Anchors)
					{
						SubtitleUserInterfaceBase<SubtitleCanvas>.Instance[textPosition] = this.content[textPosition].ToString();
					}
				}
			}
			catch (Exception exception)
			{
				IniSettings.Error(string.Concat("AudioSourceSubtitle::LateUpdate:\n", exception.ToString()));
			}
		}

		public void Reload()
		{
			if (this.subtitles.Count > 0)
			{
				this.reloadsubtitles = true;
			}
		}
	}
}