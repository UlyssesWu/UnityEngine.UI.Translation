extern alias U;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Font = U::UnityEngine.Font;
namespace UnityEngine.UI.Translation
{
    [BaseTypeOf("UnityEngine.UI.Text", "UnityEngine.UI.dll")]
    public class MaskableGraphicText : MaskableGraphic
	{
		private bool textchanged;

		private string textcached;

		private string text2translate;

		private string text4translatetag1;

		private string text4translatetag2;

		private string _text4translate;

		private string texttranslated;

		private bool firstprediction;

		private bool predictionsfailed;

		private KeyTranslationData[] txtpredictions;

		private static List<Text> cpy2clip;

		private const float IDLECLIPTIME = 3f;

		private static float idlecliptime;

		private static float cliptime;

		private bool retranslatetext;

		private string text4translate
		{
			get
			{
				return this._text4translate;
			}
			set
			{
				this.text4translatetag1 = null;
				this._text4translate = value;
				this.text4translatetag2 = null;
				if (!string.IsNullOrEmpty(value))
				{
					this._text4translate = this._text4translate.TrimStart(new char[0]);
					this.text4translatetag1 = value.Substring(0, value.Length - this._text4translate.Length);
					this._text4translate = this._text4translate.TrimEnd(new char[0]);
					this.text4translatetag2 = value.Remove(0, this.text4translatetag1.Length + this._text4translate.Length);
					this.text4translatetag2 = this.text4translatetag2.TrimEnd(new char[] { '\r', '\n' });
				}
			}
		}

		public bool Translate
		{
			get;
			set;
		}

		static MaskableGraphicText()
		{
			IniSettings.FindTextChanged += new Action<bool>((bool value) => MaskableGraphicText.Retranslate());
			MaskableGraphicText.cpy2clip = new List<Text>();
		}

		protected MaskableGraphicText()
		{
			this.Translate = true;
			MaskableGraphicText.retranslate += new Action(this.RetranslateText);
		}

		private bool CanUseCache(string value)
		{
			if (value[0] != this.textcached[0] || value.Length <= this.textcached.Length)
			{
				return false;
			}
			bool flag = false;
			int num = 1;
			int num1 = 1;
			while (num1 < this.textcached.Length)
			{
				if (!flag)
				{
					if (value[num] != this.textcached[num1])
					{
						if (value[num] != '\n')
						{
							return false;
						}
						flag = true;
					}
					else
					{
						num1++;
					}
				}
				else if (!char.IsWhiteSpace(value[num]))
				{
					if (value[num] != this.textcached[num1])
					{
						return false;
					}
					flag = false;
					num1++;
				}
				else if (value[num] == this.textcached[num1])
				{
					num1++;
				}
				num++;
			}
			while (num < value.Length)
			{
				if (!flag)
				{
					if (value[num] != '\n')
					{
						return false;
					}
					flag = true;
				}
				else if (!char.IsWhiteSpace(value[num]))
				{
					return false;
				}
				num++;
			}
			return true;
		}


        ~MaskableGraphicText()
        {
            retranslate -= new Action(this.RetranslateText);
        }

        private string InitialPrediction()
		{
			return this.text4translate;
		}

		private void LateUpdate()
		{
			TimeSpan timeSpan;
			if (!TextTranslator.Initialized)
			{
				return;
			}
			if (base.GetType() == typeof(Text))
			{
				try
				{
                    if (MaskableGraphicText.cpy2clip.Count > 0 && (U::UnityEngine.Time.time >= MaskableGraphicText.cliptime || U::UnityEngine.Time.time >= MaskableGraphicText.idlecliptime))
					{
						if (MaskableGraphicText.NativeMethods.OpenClipboard(IntPtr.Zero))
						{
							if (MaskableGraphicText.NativeMethods.EmptyClipboard())
							{
								string empty = string.Empty;
								foreach (Text text in MaskableGraphicText.cpy2clip)
								{
									if (string.IsNullOrEmpty(text.text4translate))
									{
										continue;
									}
									empty = string.Concat(empty, text.text4translate, Environment.NewLine);
								}
								uint length = (uint)((empty.Length + 1) * 2);
								IntPtr intPtr = MaskableGraphicText.NativeMethods.GlobalAlloc(66, new UIntPtr(length));
								if (intPtr != IntPtr.Zero)
								{
									IntPtr hGlobalUni = Marshal.StringToHGlobalUni(empty);
									IntPtr intPtr1 = MaskableGraphicText.NativeMethods.GlobalLock(intPtr);
									if (intPtr1 != IntPtr.Zero)
									{
										MaskableGraphicText.NativeMethods.CopyMemory(intPtr1, hGlobalUni, length);
										MaskableGraphicText.NativeMethods.GlobalUnlock(intPtr1);
										MaskableGraphicText.NativeMethods.SetClipboardData(13, intPtr);
									}
									Marshal.FreeHGlobal(hGlobalUni);
									MaskableGraphicText.NativeMethods.GlobalFree(intPtr);
								}
							}
							MaskableGraphicText.NativeMethods.CloseClipboard();
						}
						MaskableGraphicText.idlecliptime = 0f;
						MaskableGraphicText.cliptime = 0f;
						MaskableGraphicText.cpy2clip.Clear();
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					MaskableGraphicText.idlecliptime = 0f;
					MaskableGraphicText.cliptime = 0f;
					MaskableGraphicText.cpy2clip.Clear();
					IniSettings.Error(string.Concat("MaskableGraphicText::LateUpdate::Copy2Clipboard:\n", exception.ToString()));
				}
				try
				{
					if (this.textchanged)
					{
						this.textchanged = false;
						this.textcached = null;
						if (!TextTranslator.IsNotValidString(this.text4translate))
						{
							if (IniSettings.UseCopy2Clipboard)
							{
								timeSpan = (IniSettings.Copy2ClipboardTime >= 1 ? TimeSpan.FromMilliseconds((double)IniSettings.Copy2ClipboardTime) : TimeSpan.FromMilliseconds(1));
								float single = U::UnityEngine.Time.time + Convert.ToSingle(timeSpan.TotalSeconds);
								MaskableGraphicText.cliptime = single;
								if (MaskableGraphicText.idlecliptime == 0f)
								{
									MaskableGraphicText.idlecliptime = single + 3f;
								}
								if (!MaskableGraphicText.cpy2clip.Contains((Text)this))
								{
									MaskableGraphicText.cpy2clip.Add((Text)this);
								}
							}
							string value = this.text4translate;
							if (this.txtpredictions != null || this.firstprediction)
							{
								if (this.firstprediction)
								{
									this.firstprediction = false;
									this.txtpredictions = TextTranslator.FindPredictions(this.text4translate, null);
								}
								else if ((int)this.txtpredictions.Length > 1)
								{
									this.txtpredictions = TextTranslator.FindPredictions(this.text4translate, this.txtpredictions);
								}
								if (this.txtpredictions.Length == 0)
								{
									this.predictionsfailed = true;
									this.txtpredictions = null;
									TextTranslator.Translate(ref value);
								}
								else if ((int)this.txtpredictions.Length != 1)
								{
									value = this.InitialPrediction();
								}
								else if (this.txtpredictions[0].Key.Length > this.text4translate.Length && this.txtpredictions[0].Key.StartsWith(this.text4translate))
								{
									if (!string.IsNullOrEmpty(this.txtpredictions[0].Value))
									{
										float length1 = (float)this.text4translate.Length / (float)this.txtpredictions[0].Key.Length;
										int num = Convert.ToInt32((float)this.txtpredictions[0].Value.Length * length1);
										value = this.txtpredictions[0].Value.Substring(0, num);
										Text text1 = this as Text;
										TextTranslator.ApplyFontSettings(text1);
                                        U::UnityEngine.TextGenerationSettings generationSettings = text1.GetGenerationSettings(text1.rectTransform.rect.size);
                                        U::UnityEngine.TextGenerator textGenerator = new U::UnityEngine.TextGenerator();
										textGenerator.Populate(this.txtpredictions[0].Value, generationSettings);
										if (textGenerator.lineCount > 1)
										{
											int num1 = textGenerator.lineCount - 1;
											while (num1 >= 1)
											{
                                                U::UnityEngine.UILineInfo item = textGenerator.lines[num1];
												if (value.Length < item.startCharIdx)
												{
													num1--;
												}
												else
												{
													if (value.Length == item.startCharIdx || value == this.txtpredictions[0].Value || value[item.startCharIdx - 1] == '\n')
													{
														goto Label1;
													}
													value = value.Insert(item.startCharIdx, "\n");
													goto Label1;
												}
											}
										}
									}
								}
								else if (this.txtpredictions[0].Key != this.text4translate)
								{
									this.predictionsfailed = true;
									this.txtpredictions = null;
								}
								else
								{
									if (!string.IsNullOrEmpty(this.txtpredictions[0].Value))
									{
										value = this.txtpredictions[0].Value;
									}
									this.txtpredictions = null;
								}
							}
							else if (!TextTranslator.Translate(ref value) && IniSettings.UseTextPrediction && !this.predictionsfailed)
							{
								this.firstprediction = true;
							}
						Label1:
							if (value != this.text4translate)
							{
								value = string.Concat(this.text4translatetag1, value, this.text4translatetag2);
								this.texttranslated = value;
								((Text)this).text = value;
								TextTranslator.ApplyFontSettings((Text)this);
							}
						}
						else
						{
							return;
						}
					}
					else if (this.retranslatetext)
					{
						this.retranslatetext = false;
						if (!string.IsNullOrEmpty(this.text4translate))
						{
							string str = this.text4translate;
							string str1 = this.text2translate;
							this.ResetTextPredictions();
							this.text2translate = null;
							this.text4translate = null;
							this.texttranslated = null;
							((Text)this).text = str;
							((Text)this).text = str1;
							this.LateUpdate();
						}
					}
				}
				catch (Exception exception3)
				{
					Exception exception2 = exception3;
					this.textchanged = false;
					this.textcached = null;
					IniSettings.Error(string.Concat("MaskableGraphicText::LateUpdate:\n", exception2.ToString()));
				}
			}
		}

		private void ResetTextPredictions()
		{
			this.firstprediction = false;
			this.predictionsfailed = false;
			this.txtpredictions = null;
		}

		public static void Retranslate()
		{
			Action action = MaskableGraphicText.retranslate;
			if (action != null)
			{
				action();
			}
		}

		private void RetranslateText()
		{
			this.retranslatetext = true;
		}

		protected void SetText(ref string value)
		{
			if (!TextTranslator.Initialized || !this.Translate)
			{
				return;
			}
			if (base.GetType() == typeof(Text))
			{
				if (value == this.texttranslated)
				{
					return;
				}
				if (value == this.text2translate)
				{
					value = this.texttranslated;
					return;
				}
				if (string.IsNullOrEmpty(value))
				{
					this.textchanged = false;
					this.textcached = null;
					this.ResetTextPredictions();
					this.text2translate = value;
					this.text4translate = value;
					this.texttranslated = value;
					return;
				}
				if (this.textcached == null)
				{
					this.textchanged = true;
					this.textcached = value;
					if (!string.IsNullOrEmpty(this.text4translate) && !value.StartsWith(this.text4translate))
					{
						this.ResetTextPredictions();
					}
					this.text2translate = value;
					this.text4translate = value;
					this.texttranslated = value;
					return;
				}
				if (this.CanUseCache(value))
				{
					this.text2translate = value;
					this.text4translate = this.textcached;
					this.texttranslated = value;
					return;
				}
				this.text2translate = value;
				this.text4translate = value;
				this.texttranslated = value;
			}
		}

		protected override void Start()
		{
			base.Start();
			this.TranslateText();
		}

		private void TranslateText()
		{
			if (base.GetType() == typeof(Text))
			{
				this.retranslatetext = false;
				((Text)this).text = ((Text)this).text;
			}
		}

		private static event Action retranslate;

		private class NativeMethods
		{
			public NativeMethods()
			{
			}

			[DllImport("user32.dll", CharSet=CharSet.None, ExactSpelling=false)]
			internal static extern bool CloseClipboard();

			[DllImport("msvcrt.dll", CharSet=CharSet.None, EntryPoint="memcpy", ExactSpelling=false)]
			internal static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

			[DllImport("user32.dll", CharSet=CharSet.None, ExactSpelling=false)]
			internal static extern bool EmptyClipboard();

			[DllImport("kernel32.dll", CharSet=CharSet.None, ExactSpelling=false)]
			internal static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

			[DllImport("kernel32.dll", CharSet=CharSet.None, ExactSpelling=false)]
			internal static extern IntPtr GlobalFree(IntPtr hMem);

			[DllImport("kernel32.dll", CharSet=CharSet.None, ExactSpelling=false)]
			internal static extern IntPtr GlobalLock(IntPtr hMem);

			[DllImport("kernel32.dll", CharSet=CharSet.None, ExactSpelling=false)]
			internal static extern bool GlobalUnlock(IntPtr hMem);

			[DllImport("user32.dll", CharSet=CharSet.None, ExactSpelling=false)]
			internal static extern bool OpenClipboard(IntPtr hWndNewOwner);

			[DllImport("user32.dll", CharSet=CharSet.None, ExactSpelling=false)]
			internal static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);
		}
	}
}