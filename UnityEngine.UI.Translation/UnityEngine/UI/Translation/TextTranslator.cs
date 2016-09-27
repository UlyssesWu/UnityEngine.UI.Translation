extern alias U;
using Application = U::UnityEngine.Application;
using HorizontalWrapMode = U::UnityEngine.HorizontalWrapMode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;

namespace UnityEngine.UI.Translation
{
    internal static class TextTranslator
	{
		private const string REGEX = "R";

		private const string EXT = ".txt";

		private const string EXTREGEX = ".regex.txt";

		private const string EXTNOREGEX = ".noregex.txt";

		private const string FILENAME = "Translation";

		private const string FILE1 = "Translation.txt";

		private const string FILE2 = "Translation.{0}.txt";

		private const string LVFILE = "{0}.{1}.txt";

		private const string IGNORE = ".";

		private const char SEPARATOR = '=';

		private const string COMMENT = "//";

		private const string PATTERNR = "^R\\s*\\((.+)\\)$";

		private readonly static object TranslationLock;

		private static Dictionary<int, OrderedDictionary> translationsLv;

		private static Dictionary<int, List<RegexTranslationData>> translationsLvR;

		private static OrderedDictionary translations;

		private static List<RegexTranslationData> translationsR;

		private static string mainfilefullpath;

		private readonly static object NoticeLock;

		private static string lastraisedfile;

		private static DateTime lastraisedtime;

		private static FileSystemWatcher mfsw;

		private static FileSystemWatcher gfsw;

		private static FileSystemWatcher tfsw;

		private readonly static object WriterLock;

		private static Dictionary<string, StringBuilder> writerdata;

		private static System.Timers.Timer writertimer;

		internal static string GlobalTextDir
		{
			get
			{
				return string.Concat(IniSettings.MainDir, IniSettings.LanguageDir, "Text\\");
			}
		}

		internal static string GlobalTextDirFiles
		{
			get
			{
				return string.Concat("*", ".txt");
			}
		}

		internal static bool Initialized
		{
			get;
			private set;
		}

		internal static string LvFileDir
		{
			get
			{
				return TextTranslator.TextDir;
			}
		}

		internal static string LvFileName
		{
			get
			{
				string str = Application.loadedLevelName;
				if (string.IsNullOrEmpty(str))
				{
					str = "Translation";
				}
				return string.Format("{0}.{1}.txt", str, Application.loadedLevel);
			}
		}

		internal static string LvFilePath
		{
			get
			{
				return Path.Combine(TextTranslator.LvFileDir, TextTranslator.LvFileName);
			}
		}

		internal static string MainFileDir
		{
			get
			{
				return string.Concat(IniSettings.MainDir, IniSettings.ProcessPathDir);
			}
		}

		internal static string MainFileFullPath
		{
			get
			{
				if (TextTranslator.mainfilefullpath == null)
				{
					TextTranslator.mainfilefullpath = Path.GetFullPath(TextTranslator.MainFilePath);
				}
				return TextTranslator.mainfilefullpath;
			}
		}

		internal static string MainFileName
		{
			get
			{
				if (string.IsNullOrEmpty(IniSettings.Language))
				{
					return "Translation.txt";
				}
				return string.Format("Translation.{0}.txt", IniSettings.Language);
			}
		}

		internal static string MainFilePath
		{
			get
			{
				return Path.Combine(TextTranslator.MainFileDir, TextTranslator.MainFileName);
			}
		}

		internal static string TextDir
		{
			get
			{
				return string.Concat(IniSettings.MainDir, IniSettings.ProcessPathDir, IniSettings.LanguageDir, "Text\\");
			}
		}

		internal static string TextDirFiles
		{
			get
			{
				return string.Concat("*", ".txt");
			}
		}

		static TextTranslator()
		{
			TextTranslator.TranslationLock = new object();
			TextTranslator.translationsLv = new Dictionary<int, OrderedDictionary>();
			TextTranslator.translationsLvR = new Dictionary<int, List<RegexTranslationData>>();
			TextTranslator.translations = new OrderedDictionary();
			TextTranslator.translationsR = new List<RegexTranslationData>();
			TextTranslator.NoticeLock = new object();
			TextTranslator.WriterLock = new object();
			TextTranslator.Initialize();
		}

		public static void ApplyFontSettings(Text text)
		{
			text.resizeTextForBestFit = true;
			text.resizeTextMinSize = 1;
			text.resizeTextMaxSize = text.fontSize;
			text.horizontalOverflow = HorizontalWrapMode.Wrap;
		}

		private static string Decode(string text)
		{
			string str = TextTranslator.HexEncode("//", true);
			string str1 = '='.ToString();
			string str2 = TextTranslator.HexEncode(str1, true);
			StringBuilder stringBuilder = new StringBuilder(text);
			if (text.StartsWith(str))
			{
				stringBuilder.Remove(0, str.Length);
				stringBuilder.Insert(0, "//");
			}
			stringBuilder.Replace(str2, str1);
			stringBuilder.Replace(str2.ToLower(), str1);
			stringBuilder.Replace("\\r", "\r");
			stringBuilder.Replace("\\n", "\n");
			stringBuilder.Replace("\\t", "\t");
			stringBuilder.Replace(TextTranslator.HexEncode("x", false), "0x");
			return stringBuilder.ToString();
		}

		private static void DumpText(string path, string text)
		{
			StringBuilder stringBuilder;
			object writerLock = TextTranslator.WriterLock;
			Monitor.Enter(writerLock);
			try
			{
				string str = string.Concat(Path.GetDirectoryName(path), "\\");
				if (!(str != TextTranslator.MainFileDir) || !(str != TextTranslator.LvFileDir))
				{
					if (!TextTranslator.writerdata.TryGetValue(path, out stringBuilder))
					{
						stringBuilder = new StringBuilder();
						TextTranslator.writerdata.Add(path, stringBuilder);
					}
					stringBuilder.AppendLine(string.Format("{0} {1} ", TextTranslator.Encode(text), '='));
					TextTranslator.writertimer.Start();
				}
			}
			finally
			{
				Monitor.Exit(writerLock);
			}
		}

		private static string Encode(string text)
		{
			string str = TextTranslator.HexEncode("//", true);
			string str1 = '='.ToString();
			string str2 = TextTranslator.HexEncode(str1, true);
			StringBuilder stringBuilder = new StringBuilder(text);
			stringBuilder.Replace("0x", TextTranslator.HexEncode("x", false));
			if (text.StartsWith("//"))
			{
				stringBuilder.Remove(0, "//".Length);
				stringBuilder.Insert(0, str);
			}
			stringBuilder.Replace(str1, str2);
			stringBuilder.Replace("\r", "\\r");
			stringBuilder.Replace("\n", "\\n");
			stringBuilder.Replace("\t", "\\t");
			return stringBuilder.ToString();
		}

		public static KeyTranslationData[] FindPredictions(string text, KeyTranslationData[] predictions = null)
		{
			OrderedDictionary orderedDictionaries;
			Dictionary<string, KeyTranslationData> strs = new Dictionary<string, KeyTranslationData>();
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					if (predictions == null)
					{
						if (TextTranslator.translationsLv.TryGetValue(Application.loadedLevel, out orderedDictionaries))
						{
							TextTranslator.MakePredictions(text, orderedDictionaries, strs);
						}
						TextTranslator.MakePredictions(text, TextTranslator.translations, strs);
					}
					else
					{
						OrderedDictionary orderedDictionaries1 = new OrderedDictionary();
						KeyTranslationData[] keyTranslationDataArray = predictions;
						for (int i = 0; i < (int)keyTranslationDataArray.Length; i++)
						{
							KeyTranslationData keyTranslationDatum = keyTranslationDataArray[i];
							orderedDictionaries1.Add(keyTranslationDatum.Key, keyTranslationDatum);
						}
						TextTranslator.MakePredictions(text, orderedDictionaries1, strs);
					}
				}
				catch (Exception exception)
				{
					IniSettings.Error(string.Concat("TextTranslator::PredictTranslation:\n", exception.ToString()));
				}
			}
			KeyTranslationData[] keyTranslationDataArray1 = new KeyTranslationData[strs.Count];
			strs.Values.CopyTo(keyTranslationDataArray1, 0);
			return keyTranslationDataArray1;
		}

		private static int GetFileLevel(string file)
		{
			int num;
			file = Path.GetFileName(file);
			if (!file.StartsWith("."))
			{
				string str = null;
				if (file.EndsWith(".noregex.txt"))
				{
					str = file.Substring(0, file.Length - ".noregex.txt".Length);
				}
				else if (file.EndsWith(".regex.txt"))
				{
					str = file.Substring(0, file.Length - ".regex.txt".Length);
				}
				else if (file.EndsWith(".txt"))
				{
					str = file.Substring(0, file.Length - ".txt".Length);
				}
				if (!string.IsNullOrEmpty(str))
				{
					string extension = Path.GetExtension(str);
					if (extension.StartsWith("."))
					{
						extension = extension.Remove(0, 1);
						if (int.TryParse(extension, out num) && num > -1)
						{
							return num;
						}
					}
				}
			}
			return -1;
		}

		private static string HexEncode(string text, bool upper = false)
		{
			string str = null;
			string str1 = text;
			for (int i = 0; i < str1.Length; i++)
			{
				byte num = Convert.ToByte(str1[i]);
				string str2 = num.ToString("x2");
				str2 = (upper ? str2.ToUpper() : str2.ToLower());
				str = string.Concat(str, "0x", str2);
			}
			return str;
		}

		internal static void Initialize()
		{
			try
			{
				if (!TextTranslator.Initialized)
				{
					double totalMilliseconds = TimeSpan.FromSeconds((double)IniSettings.LogWriterTime).TotalMilliseconds;
					TextTranslator.writerdata = new Dictionary<string, StringBuilder>();
					TextTranslator.writertimer = new System.Timers.Timer(totalMilliseconds)
					{
						AutoReset = false
					};
					TextTranslator.writertimer.Elapsed += new ElapsedEventHandler(TextTranslator.WriterTimerElapsed);
					TextTranslator.Load();
					IniSettings.LanguageDirChanged += new Action<string>((string value) => {
						TextTranslator.Load();
						MaskableGraphicText.Retranslate();
					});
					IniSettings.ProcessPathDirChanged += new Action<string>((string value) => {
						TextTranslator.Load();
						MaskableGraphicText.Retranslate();
					});
					IniSettings.UseRegExChanged += new Action<bool>((bool value) => TextTranslator.Load());
					TextTranslator.Initialized = true;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				TextTranslator.Initialized = false;
				IniSettings.Error(string.Concat("TextTranslator::Initialize:\n", exception.ToString()));
			}
		}

		public static bool IsNotValidString(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return true;
			}
			string str = text;
			for (int i = 0; i < str.Length; i++)
			{
				if (char.IsLetter(str[i]))
				{
					return false;
				}
			}
			return true;
		}

		private static void Load()
		{
			TextTranslator.StopWatchTextFiles();
			TextTranslator.mainfilefullpath = null;
			TextTranslator.translationsLv.Clear();
			TextTranslator.translationsLvR.Clear();
			TextTranslator.translations.Clear();
			TextTranslator.translationsR.Clear();
			if (File.Exists(TextTranslator.MainFilePath))
			{
				TextTranslator.LoadTranslations(TextTranslator.MainFilePath, false);
			}
			if (TextTranslator.GlobalTextDir != TextTranslator.TextDir)
			{
				TextTranslator.LoadAllFromGlobalTranslationDir();
			}
			TextTranslator.LoadAllFromTranslationDir();
			TextTranslator.WatchTextFiles();
			if (IniSettings.DebugMode || IniSettings.FindText)
			{
				int count = 0;
				foreach (OrderedDictionary value in TextTranslator.translationsLv.Values)
				{
					count = count + value.Count;
				}
				foreach (List<RegexTranslationData> regexTranslationDatas in TextTranslator.translationsLvR.Values)
				{
					count = count + regexTranslationDatas.Count;
				}
				count = count + TextTranslator.translations.Count;
				count = count + TextTranslator.translationsR.Count;
				IniSettings.Log(string.Format("Translations Loaded: {0}", count));
			}
		}

		private static void LoadAllFromGlobalTranslationDir()
		{
			if (!Directory.Exists(TextTranslator.GlobalTextDir))
			{
				return;
			}
			TextTranslator.LoadAllFromTranslationDir(Directory.GetFiles(TextTranslator.GlobalTextDir, TextTranslator.GlobalTextDirFiles));
		}

		private static void LoadAllFromTranslationDir()
		{
			if (!Directory.Exists(TextTranslator.TextDir))
			{
				return;
			}
			TextTranslator.LoadAllFromTranslationDir(Directory.GetFiles(TextTranslator.TextDir, TextTranslator.TextDirFiles));
		}

		private static void LoadAllFromTranslationDir(string[] files)
		{
			try
			{
				if (files != null && files.Length != 0)
				{
					HashSet<string> strs = new HashSet<string>();
					bool useRegEx = IniSettings.UseRegEx;
					string[] strArrays = files;
					for (int i = 0; i < (int)strArrays.Length; i++)
					{
						string str = strArrays[i];
						if (!strs.Contains(str))
						{
							string str1 = null;
							if (str.EndsWith(".noregex.txt"))
							{
								if (useRegEx)
								{
									continue;
								}
								str1 = string.Concat(str.Substring(0, str.Length - ".noregex.txt".Length), ".txt");
							}
							else if (str.EndsWith(".regex.txt"))
							{
								if (!useRegEx)
								{
									continue;
								}
								str1 = string.Concat(str.Substring(0, str.Length - ".regex.txt".Length), ".txt");
							}
							if (!string.IsNullOrEmpty(str1))
							{
								if (!strs.Contains(str1) && File.Exists(str1))
								{
									TextTranslator.LoadTranslations(str1, false);
									strs.Add(str1);
								}
								TextTranslator.LoadTranslations(str, false);
								strs.Add(str);
							}
							else
							{
								TextTranslator.LoadTranslations(str, false);
								strs.Add(str);
								str1 = (!useRegEx ? string.Concat(str.Substring(0, str.Length - ".txt".Length), ".noregex.txt") : string.Concat(str.Substring(0, str.Length - ".txt".Length), ".regex.txt"));
								if (!strs.Contains(str1) && File.Exists(str1))
								{
									TextTranslator.LoadTranslations(str1, false);
									strs.Add(str1);
								}
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				IniSettings.Error(string.Concat("TextTranslator::LoadFromTranslationDir:\n", exception.ToString()));
			}
		}

		private static void LoadTranslations(string file, bool retranslate = false)
		{
			string str = file;
			object translationLock = TextTranslator.TranslationLock;
			Monitor.Enter(translationLock);
			try
			{
				try
				{
					if (Path.GetExtension(str).ToLower() == ".txt")
					{
						if (!Path.GetFileName(str).StartsWith("."))
						{
							if (str.StartsWith(Environment.CurrentDirectory))
							{
								str = str.Remove(0, Environment.CurrentDirectory.Length);
								if (!str.StartsWith("\\"))
								{
									str = string.Concat("\\", str);
								}
								str = string.Concat(".", str);
							}
							int fileLevel = TextTranslator.GetFileLevel(str);
							bool flag = fileLevel > -1;
							OrderedDictionary orderedDictionaries = null;
							List<RegexTranslationData> regexTranslationDatas = null;
							if (!flag)
							{
								TextTranslator.RemoveAllTranslation(TextTranslator.translations, str);
								TextTranslator.translationsR.RemoveAll((RegexTranslationData x) => x.Path == str);
							}
							else
							{
								TextTranslator.translationsLv.TryGetValue(fileLevel, out orderedDictionaries);
								TextTranslator.translationsLvR.TryGetValue(fileLevel, out regexTranslationDatas);
								if (orderedDictionaries != null)
								{
									TextTranslator.RemoveAllTranslation(orderedDictionaries, str);
								}
								if (regexTranslationDatas != null)
								{
									regexTranslationDatas.RemoveAll((RegexTranslationData x) => x.Path == str);
								}
							}
							using (StreamReader streamReader = new StreamReader(str, Encoding.UTF8))
							{
								RegexOptions regexOption = RegexOptions.None;
								regexOption = regexOption | RegexOptions.Singleline;
								regexOption = regexOption | RegexOptions.IgnoreCase;
								while (!streamReader.EndOfStream)
								{
									string str1 = streamReader.ReadLine();
									if (str1.Length == 0 || str1.StartsWith("//"))
									{
										continue;
									}
									string[] strArrays = str1.Split(new char[] { '=' }, StringSplitOptions.None);
									if ((int)strArrays.Length != 2)
									{
										if (!IniSettings.FindText)
										{
											continue;
										}
										try
										{
											if (!Directory.Exists(IniSettings.LogFileDir))
											{
												Directory.CreateDirectory(IniSettings.LogFileDir);
											}
											if ((int)strArrays.Length <= 2)
											{
												IniSettings.Log(string.Concat(new string[] { "Error_On__File: ", str, Environment.NewLine, "Unknown__Value: ", str1, Environment.NewLine, Environment.NewLine, Environment.NewLine }));
											}
											else
											{
												string[] newLine = new string[] { "Error_On__File: ", str, Environment.NewLine, "SeparatorError: ", str1, Environment.NewLine, "  -> Replace the \"=\" after the first \"=\" by \"", null, null, null, null };
												char chr = '=';
												newLine[7] = TextTranslator.HexEncode(chr.ToString(), true);
												newLine[8] = "\"";
												newLine[9] = Environment.NewLine;
												newLine[10] = Environment.NewLine;
												IniSettings.Log(string.Concat(newLine));
											}
										}
										catch
										{
										}
									}
									else
									{
										TranslationDataBase item = null;
										bool flag1 = false;
										string value = TextTranslator.Decode(strArrays[0].Trim());
										if (string.IsNullOrEmpty(value))
										{
											continue;
										}
										string str2 = TextTranslator.Decode(strArrays[1].Trim());
										if (value[0].ToString().ToUpper() == "R")
										{
											Match match = Regex.Match(value, "^R\\s*\\((.+)\\)$", regexOption);
											if (match.Success)
											{
												if (!IniSettings.UseRegEx)
												{
													continue;
												}
												flag1 = true;
												value = match.Groups[1].Value;
											}
										}
										if (flag1)
										{
											if (!flag)
											{
												item = TextTranslator.translationsR.Find((RegexTranslationData t) => t.Key == value);
											}
											else if (regexTranslationDatas != null)
											{
												item = regexTranslationDatas.Find((RegexTranslationData t) => t.Key == value);
											}
										}
										else if (!flag)
										{
											item = TextTranslator.translations[value] as TranslationDataBase;
										}
										else if (orderedDictionaries != null)
										{
											item = orderedDictionaries[value] as TranslationDataBase;
										}
										if (item != null)
										{
											if (flag1)
											{
												if (!flag)
												{
													TextTranslator.translationsR.Remove((RegexTranslationData)item);
												}
												else
												{
													regexTranslationDatas.Remove((RegexTranslationData)item);
												}
											}
											else if (!flag)
											{
												TextTranslator.translations.Remove(value);
											}
											else
											{
												orderedDictionaries.Remove(value);
											}
											if (IniSettings.FindText)
											{
												try
												{
													if (!Directory.Exists(IniSettings.LogFileDir))
													{
														Directory.CreateDirectory(IniSettings.LogFileDir);
													}
													string[] value1 = new string[] { "Replacing__Key: ", value, "=", item.Value, Environment.NewLine, "FromLoadedFile: ", item.Path, Environment.NewLine, "By__Next___Key: ", value, "=", str2, Environment.NewLine, "From_Next_File: ", str, Environment.NewLine, "These__Keys_Do: ", null, null, null };
													value1[17] = (item.Value != str2 ? "Not Match" : "Match");
													value1[18] = Environment.NewLine;
													value1[19] = Environment.NewLine;
													IniSettings.Log(string.Concat(value1));
												}
												catch
												{
												}
											}
										}
										if (!flag1)
										{
											item = new TranslationDataBase(str, str2);
											if (!flag)
											{
												TextTranslator.translations.Add(value, item);
											}
											else
											{
												if (orderedDictionaries == null)
												{
													orderedDictionaries = new OrderedDictionary();
													TextTranslator.translationsLv.Add(fileLevel, orderedDictionaries);
												}
												orderedDictionaries.Add(value, item);
											}
										}
										else
										{
											try
											{
												try
												{
													item = new RegexTranslationData(str, value, str2);
													TextTranslator.UpdateRegexCacheSize();
													Regex.Replace("A", value, str2);
												}
												catch (Exception exception1)
												{
													Exception exception = exception1;
													item = null;
													if (IniSettings.FindText)
													{
														try
														{
															IniSettings.Log(string.Concat(new string[] { "Error_On__File: ", str, Environment.NewLine, "Regex____Error: ", str1, Environment.NewLine, "  ", exception.Message, Environment.NewLine, Environment.NewLine, Environment.NewLine }));
														}
														catch
														{
														}
													}
												}
											}
											finally
											{
												if (item != null)
												{
													if (!flag)
													{
														TextTranslator.translationsR.Add((RegexTranslationData)item);
													}
													else
													{
														if (regexTranslationDatas == null)
														{
															regexTranslationDatas = new List<RegexTranslationData>();
															TextTranslator.translationsLvR.Add(fileLevel, regexTranslationDatas);
														}
														regexTranslationDatas.Add((RegexTranslationData)item);
													}
												}
											}
										}
									}
								}
							}
							if (retranslate)
							{
								MaskableGraphicText.Retranslate();
							}
							if (IniSettings.DebugMode || IniSettings.FindText)
							{
								IniSettings.Log(string.Concat("Loaded: ", str));
							}
						}
					}
				}
				catch (Exception exception2)
				{
					IniSettings.Error(string.Concat("LoadTranslations:\n", exception2.ToString()));
				}
			}
			finally
			{
				Monitor.Exit(translationLock);
			}
		}

		private static void MakePredictions(string text, OrderedDictionary entries, Dictionary<string, KeyTranslationData> predictions)
		{
			string key = null;
			TranslationDataBase value = null;
			foreach (DictionaryEntry entry in entries)
			{
				key = entry.Key as string;
				if (key != null)
				{
					value = entry.Value as TranslationDataBase;
					if (value == null)
					{
						return;
					}
					else if (key.Length <= text.Length || !key.StartsWith(text))
					{
						if (key != text)
						{
							continue;
						}
						predictions.Clear();
						predictions.Add(key, new KeyTranslationData(key, value));
						return;
					}
					else
					{
						if (predictions.ContainsKey(key))
						{
							continue;
						}
						predictions.Add(key, new KeyTranslationData(key, value));
					}
				}
				else
				{
					return;
				}
			}
		}

		private static void RemoveAllTranslation(OrderedDictionary od, string fromfile)
		{
			for (int i = od.Count - 1; i >= 0; i--)
			{
				TranslationDataBase item = od[i] as TranslationDataBase;
				if (item != null && item.Path == fromfile)
				{
					od.RemoveAt(i);
				}
			}
		}

		private static void StopWatchTextFiles()
		{
			if (TextTranslator.mfsw != null)
			{
				TextTranslator.mfsw.Dispose();
				TextTranslator.mfsw = null;
			}
			if (TextTranslator.gfsw != null)
			{
				TextTranslator.gfsw.Dispose();
				TextTranslator.gfsw = null;
			}
			if (TextTranslator.tfsw != null)
			{
				TextTranslator.tfsw.Dispose();
				TextTranslator.tfsw = null;
			}
		}

		public static bool Translate(ref string text)
		{
			OrderedDictionary orderedDictionaries;
			List<RegexTranslationData> regexTranslationDatas;
			bool flag;
			OrderedDictionary orderedDictionaries1;
			object translationLock = TextTranslator.TranslationLock;
			Monitor.Enter(translationLock);
			try
			{
				try
				{
					string value = null;
					if (TextTranslator.translationsLv.TryGetValue(Application.loadedLevel, out orderedDictionaries))
					{
						TranslationDataBase item = orderedDictionaries[text] as TranslationDataBase;
						if (item == null)
						{
							goto Label2;
						}
						value = item.Value;
						goto Label1;
					}
				Label2:
					if (IniSettings.UseRegEx)
					{
						TextTranslator.UpdateRegexCacheSize();
						if (TextTranslator.translationsLvR.TryGetValue(Application.loadedLevel, out regexTranslationDatas))
						{
							foreach (RegexTranslationData regexTranslationDatum in regexTranslationDatas)
							{
								Match match = Regex.Match(text, regexTranslationDatum.Key);
								if (!match.Success || !(match.Groups[0].Value == text))
								{
									continue;
								}
								value = match.Result(regexTranslationDatum.Value);
								goto Label1;
							}
						}
					}
					TranslationDataBase translationDataBase = TextTranslator.translations[text] as TranslationDataBase;
					if (translationDataBase != null)
					{
						value = translationDataBase.Value;
					}
					else if (IniSettings.UseRegEx)
					{
						TextTranslator.UpdateRegexCacheSize();
						foreach (RegexTranslationData regexTranslationDatum1 in TextTranslator.translationsR)
						{
							Match match1 = Regex.Match(text, regexTranslationDatum1.Key);
							if (!match1.Success || !(match1.Groups[0].Value == text))
							{
								continue;
							}
							value = match1.Result(regexTranslationDatum1.Value);
							goto Label1;
						}
					}
                    Label1:
					if (value != null)
					{
						if (!string.IsNullOrEmpty(value))
						{
							text = value;
							flag = true;
							return flag;
						}
					}
					else if (IniSettings.FindText)
					{
						if (!IniSettings.DumpTextByLevel)
						{
							string mainFilePath = TextTranslator.MainFilePath;
							TextTranslator.translations.Add(text, new TranslationDataBase(mainFilePath));
							TextTranslator.DumpText(mainFilePath, text);
						}
						else
						{
							if (!TextTranslator.translationsLv.TryGetValue(Application.loadedLevel, out orderedDictionaries1))
							{
								orderedDictionaries1 = new OrderedDictionary();
								TextTranslator.translationsLv.Add(Application.loadedLevel, orderedDictionaries1);
							}
							string lvFilePath = TextTranslator.LvFilePath;
							orderedDictionaries1.Add(text, new TranslationDataBase(lvFilePath));
							TextTranslator.DumpText(lvFilePath, text);
						}
					}
				}
				catch (Exception exception)
				{
					IniSettings.Error(string.Concat("TextTranslator::Translate:\n", exception.ToString()));
				}
				flag = false;
			}
			finally
			{
				Monitor.Exit(translationLock);
			}
			return flag;
		}

		private static void UpdateRegexCacheSize()
		{
			if (Regex.CacheSize < 2147483647)
			{
				long count = (long)0;
				foreach (List<RegexTranslationData> value in TextTranslator.translationsLvR.Values)
				{
					count = count + (long)value.Count;
				}
				count = count + (long)TextTranslator.translationsR.Count;
				if ((long)Regex.CacheSize + count + (long)15 > (long)2147483647)
				{
					Regex.CacheSize = 2147483647;
				}
				else if (Regex.CacheSize < TextTranslator.translationsR.Count + 15)
				{
					Regex.CacheSize = TextTranslator.translationsR.Count + 15;
					return;
				}
			}
		}

		private static void WatcherNotice(object sender, FileSystemEventArgs e)
		{
			object noticeLock = TextTranslator.NoticeLock;
			Monitor.Enter(noticeLock);
			try
			{
				if (!(TextTranslator.lastraisedfile == e.FullPath) || !(DateTime.Now < TextTranslator.lastraisedtime))
				{
					TextTranslator.lastraisedfile = e.FullPath;
					TextTranslator.lastraisedtime = DateTime.Now.AddSeconds(1);
					if (e.FullPath.EndsWith(".noregex.txt"))
					{
						if (!IniSettings.UseRegEx)
						{
							TextTranslator.LoadTranslations(e.FullPath, true);
						}
						else
						{
							return;
						}
					}
					else if (e.FullPath.EndsWith(".regex.txt"))
					{
						if (IniSettings.UseRegEx)
						{
							TextTranslator.LoadTranslations(e.FullPath, true);
						}
						else
						{
							return;
						}
					}
					else if (e.FullPath != TextTranslator.MainFileFullPath)
					{
						string str = e.FullPath.Substring(0, e.FullPath.Length - ".txt".Length);
						str = string.Concat(str, (IniSettings.UseRegEx ? ".regex.txt" : ".noregex.txt"));
						if (!File.Exists(str))
						{
							TextTranslator.LoadTranslations(e.FullPath, true);
						}
						else
						{
							TextTranslator.LoadTranslations(e.FullPath, false);
							TextTranslator.LoadTranslations(str, true);
						}
					}
					else
					{
						TextTranslator.LoadTranslations(e.FullPath, true);
					}
					TextTranslator.WatchTextFiles();
				}
			}
			finally
			{
				Monitor.Exit(noticeLock);
			}
		}

		private static void WatchTextFiles()
		{
			try
			{
				if (TextTranslator.mfsw == null && Directory.Exists(TextTranslator.MainFileDir))
				{
					TextTranslator.mfsw = new FileSystemWatcher(TextTranslator.MainFileDir, TextTranslator.MainFileName)
					{
						NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
						IncludeSubdirectories = false
					};
					TextTranslator.mfsw.Changed += new FileSystemEventHandler(TextTranslator.WatcherNotice);
					TextTranslator.mfsw.Created += new FileSystemEventHandler(TextTranslator.WatcherNotice);
					TextTranslator.mfsw.Error += new ErrorEventHandler((object sender, ErrorEventArgs e) => IniSettings.Error(e.GetException().ToString()));
					TextTranslator.mfsw.EnableRaisingEvents = true;
				}
				if (TextTranslator.GlobalTextDir != TextTranslator.TextDir && TextTranslator.gfsw == null && Directory.Exists(TextTranslator.GlobalTextDir))
				{
					TextTranslator.gfsw = new FileSystemWatcher(TextTranslator.GlobalTextDir, TextTranslator.GlobalTextDirFiles)
					{
						NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
						IncludeSubdirectories = false
					};
					TextTranslator.gfsw.Changed += new FileSystemEventHandler(TextTranslator.WatcherNotice);
					TextTranslator.gfsw.Created += new FileSystemEventHandler(TextTranslator.WatcherNotice);
					TextTranslator.gfsw.Error += new ErrorEventHandler((object sender, ErrorEventArgs e) => IniSettings.Error(e.GetException().ToString()));
					TextTranslator.gfsw.EnableRaisingEvents = true;
				}
				if (TextTranslator.tfsw == null && Directory.Exists(TextTranslator.TextDir))
				{
					TextTranslator.tfsw = new FileSystemWatcher(TextTranslator.TextDir, TextTranslator.TextDirFiles)
					{
						NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
						IncludeSubdirectories = false
					};
					TextTranslator.tfsw.Changed += new FileSystemEventHandler(TextTranslator.WatcherNotice);
					TextTranslator.tfsw.Created += new FileSystemEventHandler(TextTranslator.WatcherNotice);
					TextTranslator.tfsw.Error += new ErrorEventHandler((object sender, ErrorEventArgs e) => IniSettings.Error(e.GetException().ToString()));
					TextTranslator.tfsw.EnableRaisingEvents = true;
				}
			}
			catch (Exception exception)
			{
				IniSettings.Error(string.Concat("WatchTextFiles:\n", exception.ToString()));
			}
		}

		private static void WriterTimerElapsed(object sender, ElapsedEventArgs e)
		{
			object writerLock = TextTranslator.WriterLock;
			Monitor.Enter(writerLock);
			try
			{
				TextTranslator.StopWatchTextFiles();
				try
				{
					foreach (KeyValuePair<string, StringBuilder> writerdatum in TextTranslator.writerdata)
					{
						string key = writerdatum.Key;
						string directoryName = Path.GetDirectoryName(key);
						if (!Directory.Exists(directoryName))
						{
							Directory.CreateDirectory(directoryName);
						}
						if (!File.Exists(key) && key != TextTranslator.MainFilePath)
						{
							string str = key.Substring(0, key.Length - ".txt".Length);
							string[] strArrays = new string[] { string.Concat(str, ".noregex.txt"), string.Concat(str, ".regex.txt") };
							for (int i = 0; i < (int)strArrays.Length; i++)
							{
								string str1 = strArrays[i];
								if (!File.Exists(str1))
								{
									File.Create(str1).Dispose();
								}
							}
						}
						using (StreamWriter streamWriter = new StreamWriter(key, true, Encoding.UTF8))
						{
							streamWriter.Write(writerdatum.Value.ToString());
						}
					}
				}
				catch (Exception exception)
				{
					IniSettings.Error(string.Concat("TextTranslator::DumpText:\n", exception.ToString()));
				}
				TextTranslator.writerdata.Clear();
				TextTranslator.WatchTextFiles();
			}
			finally
			{
				Monitor.Exit(writerLock);
			}
		}
	}
}