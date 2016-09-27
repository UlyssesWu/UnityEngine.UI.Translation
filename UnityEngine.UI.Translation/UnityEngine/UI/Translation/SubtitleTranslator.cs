extern alias U;
using Application = U::UnityEngine.Application;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;

namespace UnityEngine.UI.Translation
{
    internal static class SubtitleTranslator
	{
		private const string EXT = ".txt";

		private const string FILENAME = "Subtitle";

		private const string FILE = "{0}.txt";

		private const string LVFILE = "{0}.{1}.txt";

		private const string IGNORE = ".";

		private const string COMMENT = "//";

		private readonly static object TranslationLock;

		private static Dictionary<int, OrderedDictionary> translationsLv;

		private static OrderedDictionary translations;

		private readonly static object NoticeLock;

		private static string lastraisedfile;

		private static DateTime lastraisedtime;

		private static FileSystemWatcher gfsw;

		private static FileSystemWatcher sfsw;

		private readonly static object WriterLock;

		private static Dictionary<string, StringBuilder> writerdata;

		private static System.Timers.Timer writertimer;

		internal static string FileDir
		{
			get
			{
				return SubtitleTranslator.SubtitleDir;
			}
		}

		internal static string FileName
		{
			get
			{
				return string.Format("{0}.txt", "Subtitle");
			}
		}

		internal static string FilePath
		{
			get
			{
				return Path.Combine(SubtitleTranslator.FileDir, SubtitleTranslator.FileName);
			}
		}

		internal static string GlobalSubtitleDir
		{
			get
			{
				return string.Concat(IniSettings.MainDir, IniSettings.LanguageDir, "Audio\\");
			}
		}

		internal static string GlobalSubtitleDirFiles
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
				return SubtitleTranslator.SubtitleDir;
			}
		}

		internal static string LvFileName
		{
			get
			{
				string str = Application.loadedLevelName;
				if (string.IsNullOrEmpty(str))
				{
					str = "Subtitle";
				}
				return string.Format("{0}.{1}.txt", str, Application.loadedLevel);
			}
		}

		internal static string LvFilePath
		{
			get
			{
				return Path.Combine(SubtitleTranslator.LvFileDir, SubtitleTranslator.LvFileName);
			}
		}

		internal static string SubtitleDir
		{
			get
			{
				return string.Concat(IniSettings.MainDir, IniSettings.ProcessPathDir, IniSettings.LanguageDir, "Audio\\");
			}
		}

		internal static string SubtitleDirFiles
		{
			get
			{
				return string.Concat("*", ".txt");
			}
		}

		static SubtitleTranslator()
		{
			SubtitleTranslator.TranslationLock = new object();
			SubtitleTranslator.translationsLv = new Dictionary<int, OrderedDictionary>();
			SubtitleTranslator.translations = new OrderedDictionary();
			SubtitleTranslator.NoticeLock = new object();
			SubtitleTranslator.WriterLock = new object();
			SubtitleTranslator.Initialize();
		}

		private static void DumpSubtitle(string path, string audio)
		{
			StringBuilder stringBuilder;
			object writerLock = SubtitleTranslator.WriterLock;
			Monitor.Enter(writerLock);
			try
			{
				if (string.Concat(Path.GetDirectoryName(path), "\\") == SubtitleTranslator.SubtitleDir)
				{
					if (!SubtitleTranslator.writerdata.TryGetValue(path, out stringBuilder))
					{
						stringBuilder = new StringBuilder();
						SubtitleTranslator.writerdata.Add(path, stringBuilder);
					}
					stringBuilder.AppendLine(string.Format("#sub \"{0}\"", audio));
					SubtitleTranslator.writertimer.Start();
				}
			}
			finally
			{
				Monitor.Exit(writerLock);
			}
		}

		private static int GetFileLevel(string file)
		{
			int num;
			file = Path.GetFileName(file);
			if (!file.StartsWith("."))
			{
				string str = null;
				if (file.EndsWith(".txt"))
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

		internal static void Initialize()
		{
			try
			{
				if (!SubtitleTranslator.Initialized)
				{
					double totalMilliseconds = TimeSpan.FromSeconds((double)IniSettings.LogWriterTime).TotalMilliseconds;
					SubtitleTranslator.writerdata = new Dictionary<string, StringBuilder>();
					SubtitleTranslator.writertimer = new System.Timers.Timer(totalMilliseconds)
					{
						AutoReset = false
					};
					SubtitleTranslator.writertimer.Elapsed += new ElapsedEventHandler(SubtitleTranslator.WriterTimerElapsed);
					SubtitleTranslator.Load();
					SubtitleSettings.AnchorChanged += new Action<TextPosition>((TextPosition value) => {
						SubtitleTranslator.Load();
						AudioSourceSubtitle.Instance.Reload();
					});
					IniSettings.LanguageDirChanged += new Action<string>((string value) => {
						SubtitleTranslator.Load();
						AudioSourceSubtitle.Instance.Reload();
					});
					IniSettings.ProcessPathDirChanged += new Action<string>((string value) => {
						SubtitleTranslator.Load();
						AudioSourceSubtitle.Instance.Reload();
					});
					IniSettings.FindAudioChanged += new Action<bool>((bool value) => AudioSourceSubtitle.Instance.Reload());
					SubtitleTranslator.Initialized = true;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				SubtitleTranslator.Initialized = false;
				IniSettings.Error(string.Concat("SubtitleTranslator::Initialize:\n", exception.ToString()));
			}
		}

		private static void Load()
		{
			SubtitleTranslator.StopWatchSubtitleFiles();
			SubtitleTranslator.translations.Clear();
			SubtitleTranslator.translationsLv.Clear();
			if (SubtitleTranslator.GlobalSubtitleDir != SubtitleTranslator.SubtitleDir)
			{
				SubtitleTranslator.LoadAllFromGlobalTranslationDir();
			}
			SubtitleTranslator.LoadAllFromTranslationDir();
			SubtitleTranslator.WatchSubtitleFiles();
			if (IniSettings.DebugMode || IniSettings.FindAudio)
			{
				int count = 0;
				count = count + SubtitleTranslator.translations.Count;
				foreach (OrderedDictionary value in SubtitleTranslator.translationsLv.Values)
				{
					count = count + value.Count;
				}
				IniSettings.Log(string.Format("Subtitles Loaded: {0}", count));
			}
		}

		private static void LoadAllFromGlobalTranslationDir()
		{
			if (!Directory.Exists(SubtitleTranslator.GlobalSubtitleDir))
			{
				return;
			}
			SubtitleTranslator.LoadAllFromTranslationDir(Directory.GetFiles(SubtitleTranslator.GlobalSubtitleDir, SubtitleTranslator.GlobalSubtitleDirFiles));
		}

		private static void LoadAllFromTranslationDir()
		{
			if (!Directory.Exists(SubtitleTranslator.SubtitleDir))
			{
				return;
			}
			SubtitleTranslator.LoadAllFromTranslationDir(Directory.GetFiles(SubtitleTranslator.SubtitleDir, SubtitleTranslator.SubtitleDirFiles));
		}

		private static void LoadAllFromTranslationDir(string[] files)
		{
			if (files == null || files.Length == 0)
			{
				return;
			}
			string[] strArrays = files;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				SubtitleTranslator.LoadTranslations(strArrays[i], false);
			}
		}

		private static void LoadTranslations(string file, bool retranslate = false)
		{
			object translationLock = SubtitleTranslator.TranslationLock;
			Monitor.Enter(translationLock);
			try
			{
				try
				{
					if (Path.GetExtension(file).ToLower() == ".txt")
					{
						if (!Path.GetFileName(file).StartsWith("."))
						{
							if (file.StartsWith(Environment.CurrentDirectory))
							{
								file = file.Remove(0, Environment.CurrentDirectory.Length);
								if (!file.StartsWith("\\"))
								{
									file = string.Concat("\\", file);
								}
								file = string.Concat(".", file);
							}
							int fileLevel = SubtitleTranslator.GetFileLevel(file);
							bool flag = fileLevel > -1;
							OrderedDictionary orderedDictionaries = null;
							if (!flag)
							{
								SubtitleTranslator.RemoveAllTranslation(SubtitleTranslator.translations, file);
							}
							else
							{
								SubtitleTranslator.translationsLv.TryGetValue(fileLevel, out orderedDictionaries);
								if (orderedDictionaries != null)
								{
									SubtitleTranslator.RemoveAllTranslation(orderedDictionaries, file);
								}
							}
							using (StreamReader streamReader = new StreamReader(file, Encoding.UTF8))
							{
								bool flag1 = false;
								bool flag2 = true;
								List<SubtitleLine> subtitleLines = null;
								string empty = string.Empty;
								while (!streamReader.EndOfStream)
								{
									string str = streamReader.ReadLine();
									if (str.StartsWith("//") || empty.Length == 0 && str.Length == 0)
									{
										continue;
									}
									Match match = Regex.Match(str.TrimEnd(new char[0]), "^#sub[ ]+\"(.+?)\"(?:[ ]+(?:{\\\\a([\\d]+)})?(.*))?$", RegexOptions.IgnoreCase);
									if (!match.Success)
									{
										if (empty.Length <= 0)
										{
											continue;
										}
										if (!flag1)
										{
											if (str.Length == 0)
											{
												continue;
											}
											Match match1 = Regex.Match(str, "^([\\d]*\\.?[\\d]+)[ ]+-->[ ]+([\\d]*\\.?[\\d]+)$", RegexOptions.None);
											if (!match1.Success)
											{
												flag1 = true;
												subtitleLines.Add(new SubtitleLine());
											}
											else
											{
												if (streamReader.EndOfStream)
												{
													continue;
												}
												flag1 = true;
												SubtitleLine subtitleLine = new SubtitleLine()
												{
													StartTime = float.Parse(match1.Groups[1].Value, CultureInfo.InvariantCulture),
													EndTime = float.Parse(match1.Groups[2].Value, CultureInfo.InvariantCulture)
												};
												subtitleLines.Add(subtitleLine);
												continue;
											}
										}
										if (flag2)
										{
											int count = subtitleLines.Count - 1;
											if (str.Length > 0)
											{
												Match match2 = Regex.Match(str, "^(?:{\\\\a([\\d]+)})?(.*)$", RegexOptions.None);
												if (match2.Success)
												{
													string str1 = match2.Groups[2].Value.Trim();
													if (str1.Length != 0 || !streamReader.EndOfStream)
													{
														SubtitleLine item = subtitleLines[count];
														item.Position = (match2.Groups[1].Success ? item.Position.Parse(match2.Groups[1].Value, SubtitleSettings.Anchor) : SubtitleSettings.Anchor);
														item.Text = str1;
														subtitleLines[count] = item;
														continue;
													}
												}
											}
											flag1 = false;
											flag2 = true;
											subtitleLines.RemoveAt(count);
										}
										else if (str.Length <= 0)
										{
											flag1 = false;
											flag2 = true;
											int num = subtitleLines.Count - 1;
											if (subtitleLines[num].Text.Length != 0)
											{
												continue;
											}
											subtitleLines.RemoveAt(num);
										}
										else
										{
											string str2 = str.Trim();
											int count1 = subtitleLines.Count - 1;
											SubtitleLine item1 = subtitleLines[count1];
											if (str2.Length <= 0)
											{
												if (!streamReader.EndOfStream || item1.Text.Length != 0)
												{
													continue;
												}
												subtitleLines.RemoveAt(count1);
											}
											else
											{
												if (item1.Text.Length > 0)
												{
													item1.Text = string.Concat(item1.Text, "\n");
												}
												item1.Text = string.Concat(item1.Text, str2);
												subtitleLines[count1] = item1;
											}
										}
									}
									else
									{
										flag1 = false;
										flag2 = true;
										SubtitleDataBase subtitleDataBase = null;
										empty = match.Groups[1].Value;
										subtitleLines = new List<SubtitleLine>();
										if (match.Groups[3].Success)
										{
											string str3 = match.Groups[3].Value.Trim();
											if (str3.Length > 0)
											{
												SubtitleLine subtitleLine1 = new SubtitleLine()
												{
													Text = str3
												};
											    subtitleLine1.Position = (match.Groups[2].Success
											        ? subtitleLine1.Position.Parse(match.Groups[2].Value, SubtitleSettings.Anchor)
											        : SubtitleSettings.Anchor);

												subtitleLines.Add(subtitleLine1);
											}
										}
										if (!flag)
										{
											subtitleDataBase = SubtitleTranslator.translations[empty] as SubtitleDataBase;
										}
										else if (orderedDictionaries != null)
										{
											subtitleDataBase = orderedDictionaries[empty] as SubtitleDataBase;
										}
										if (subtitleDataBase != null)
										{
											if (!flag)
											{
												SubtitleTranslator.translations.Remove(empty);
											}
											else
											{
												orderedDictionaries.Remove(empty);
											}
										}
										subtitleDataBase = new SubtitleDataBase(file, subtitleLines);
										if (!flag)
										{
											SubtitleTranslator.translations.Add(empty, subtitleDataBase);
										}
										else
										{
											if (orderedDictionaries == null)
											{
												orderedDictionaries = new OrderedDictionary();
												SubtitleTranslator.translationsLv.Add(fileLevel, orderedDictionaries);
											}
											orderedDictionaries.Add(empty, subtitleDataBase);
										}
									}
								}
							}
							if (retranslate)
							{
								AudioSourceSubtitle.Instance.Reload();
							}
							if (IniSettings.DebugMode || IniSettings.FindAudio)
							{
								IniSettings.Log(string.Concat("Loaded: ", file));
							}
						}
					}
				}
				catch (Exception exception)
				{
					IniSettings.Error(string.Concat("LoadSubtitles:\n", exception.ToString()));
				}
			}
			finally
			{
				Monitor.Exit(translationLock);
			}
		}

		private static void RemoveAllTranslation(OrderedDictionary od, string fromfile)
		{
			for (int i = od.Count - 1; i >= 0; i--)
			{
				SubtitleDataBase item = od[i] as SubtitleDataBase;
				if (item != null && item.Path == fromfile)
				{
					od.RemoveAt(i);
				}
			}
		}

		private static void StopWatchSubtitleFiles()
		{
			if (SubtitleTranslator.gfsw != null)
			{
				SubtitleTranslator.gfsw.Dispose();
				SubtitleTranslator.gfsw = null;
			}
			if (SubtitleTranslator.sfsw != null)
			{
				SubtitleTranslator.sfsw.Dispose();
				SubtitleTranslator.sfsw = null;
			}
		}

		public static bool Translate(string audio, out SubtitleLine[] lines)
		{
			OrderedDictionary orderedDictionaries;
			OrderedDictionary orderedDictionaries1;
			bool count;
			object translationLock = SubtitleTranslator.TranslationLock;
			Monitor.Enter(translationLock);
			try
			{
				try
				{
					List<SubtitleLine> value = null;
					if (SubtitleTranslator.translationsLv.TryGetValue(Application.loadedLevel, out orderedDictionaries))
					{
						SubtitleDataBase item = orderedDictionaries[audio] as SubtitleDataBase;
						if (item == null)
						{
							goto Label2;
						}
						value = item.Value;
						goto Label1;
					}
				Label2:
					SubtitleDataBase subtitleDataBase = SubtitleTranslator.translations[audio] as SubtitleDataBase;
					if (subtitleDataBase != null)
					{
						value = subtitleDataBase.Value;
					}
				Label1:
					if (value == null)
					{
						value = new List<SubtitleLine>();
						if (IniSettings.FindAudio)
						{
							if (!IniSettings.DumpAudioByLevel)
							{
								string filePath = SubtitleTranslator.FilePath;
								SubtitleTranslator.translations.Add(audio, new SubtitleDataBase(filePath, value));
								SubtitleTranslator.DumpSubtitle(filePath, audio);
							}
							else
							{
								if (!SubtitleTranslator.translationsLv.TryGetValue(Application.loadedLevel, out orderedDictionaries1))
								{
									orderedDictionaries1 = new OrderedDictionary();
									SubtitleTranslator.translationsLv.Add(Application.loadedLevel, orderedDictionaries1);
								}
								string lvFilePath = SubtitleTranslator.LvFilePath;
								orderedDictionaries1.Add(audio, new SubtitleDataBase(lvFilePath, value));
								SubtitleTranslator.DumpSubtitle(lvFilePath, audio);
							}
						}
					}
					List<SubtitleLine> subtitleLines = new List<SubtitleLine>();
					foreach (SubtitleLine subtitleLine in value)
					{
						if (string.IsNullOrEmpty(subtitleLine.Text))
						{
							continue;
						}
						subtitleLines.Add(subtitleLine);
					}
					if (subtitleLines.Count == 0 && IniSettings.FindAudio)
					{
						SubtitleLine subtitleLine1 = new SubtitleLine()
						{
							Position = SubtitleSettings.Anchor,
							Text = audio
						};
						subtitleLines.Add(subtitleLine1);
					}
					lines = subtitleLines.ToArray();
					count = subtitleLines.Count > 0;
					return count;
				}
				catch (Exception exception)
				{
					IniSettings.Error(string.Concat("TextTranslator::Translate:\n", exception.ToString()));
				}
				lines = null;
				count = false;
			}
			finally
			{
				Monitor.Exit(translationLock);
			}
			return count;
		}

		private static void WatcherNotice(object sender, FileSystemEventArgs e)
		{
			object noticeLock = SubtitleTranslator.NoticeLock;
			Monitor.Enter(noticeLock);
			try
			{
				if (!(SubtitleTranslator.lastraisedfile == e.FullPath) || !(DateTime.Now < SubtitleTranslator.lastraisedtime))
				{
					SubtitleTranslator.lastraisedfile = e.FullPath;
					SubtitleTranslator.lastraisedtime = DateTime.Now.AddSeconds(1);
					if (e.FullPath.EndsWith(".txt"))
					{
						SubtitleTranslator.LoadTranslations(e.FullPath, true);
					}
					SubtitleTranslator.WatchSubtitleFiles();
				}
			}
			finally
			{
				Monitor.Exit(noticeLock);
			}
		}

		private static void WatchSubtitleFiles()
		{
			try
			{
				if (SubtitleTranslator.GlobalSubtitleDir != SubtitleTranslator.SubtitleDir && SubtitleTranslator.gfsw == null && Directory.Exists(SubtitleTranslator.GlobalSubtitleDir))
				{
					SubtitleTranslator.gfsw = new FileSystemWatcher(SubtitleTranslator.GlobalSubtitleDir, SubtitleTranslator.GlobalSubtitleDirFiles)
					{
						NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
						IncludeSubdirectories = false
					};
					SubtitleTranslator.gfsw.Changed += new FileSystemEventHandler(SubtitleTranslator.WatcherNotice);
					SubtitleTranslator.gfsw.Created += new FileSystemEventHandler(SubtitleTranslator.WatcherNotice);
					SubtitleTranslator.gfsw.Error += new ErrorEventHandler((object sender, ErrorEventArgs e) => IniSettings.Error(e.GetException().ToString()));
					SubtitleTranslator.gfsw.EnableRaisingEvents = true;
				}
				if (SubtitleTranslator.sfsw == null && Directory.Exists(SubtitleTranslator.SubtitleDir))
				{
					SubtitleTranslator.sfsw = new FileSystemWatcher(SubtitleTranslator.SubtitleDir, SubtitleTranslator.SubtitleDirFiles)
					{
						NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
						IncludeSubdirectories = false
					};
					SubtitleTranslator.sfsw.Changed += new FileSystemEventHandler(SubtitleTranslator.WatcherNotice);
					SubtitleTranslator.sfsw.Created += new FileSystemEventHandler(SubtitleTranslator.WatcherNotice);
					SubtitleTranslator.sfsw.Error += new ErrorEventHandler((object sender, ErrorEventArgs e) => IniSettings.Error(e.GetException().ToString()));
					SubtitleTranslator.sfsw.EnableRaisingEvents = true;
				}
			}
			catch (Exception exception)
			{
				IniSettings.Error(string.Concat("WatchSubtitleFiles:\n", exception.ToString()));
			}
		}

		private static void WriterTimerElapsed(object sender, ElapsedEventArgs e)
		{
			object writerLock = SubtitleTranslator.WriterLock;
			Monitor.Enter(writerLock);
			try
			{
				SubtitleTranslator.StopWatchSubtitleFiles();
				try
				{
					foreach (KeyValuePair<string, StringBuilder> writerdatum in SubtitleTranslator.writerdata)
					{
						string key = writerdatum.Key;
						string directoryName = Path.GetDirectoryName(key);
						if (!Directory.Exists(directoryName))
						{
							Directory.CreateDirectory(directoryName);
						}
						using (StreamWriter streamWriter = new StreamWriter(key, true, Encoding.UTF8))
						{
							streamWriter.Write(writerdatum.Value.ToString());
						}
					}
				}
				catch (Exception exception)
				{
					IniSettings.Error(string.Concat("SubtitleTranslator::DumpText:\n", exception.ToString()));
				}
				SubtitleTranslator.writerdata.Clear();
				SubtitleTranslator.WatchSubtitleFiles();
			}
			finally
			{
				Monitor.Exit(writerLock);
			}
		}
	}
}