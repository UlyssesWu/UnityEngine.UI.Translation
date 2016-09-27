extern alias U;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using Texture2D = U::UnityEngine.Texture2D;
using TextureFormat = U::UnityEngine.TextureFormat;

namespace UnityEngine.UI.Translation
{
    internal static class Texture2DOverride
	{
		private static Dictionary<string, string> uniqueimagesdata;

		private const int Interval = 15;

		private static System.Timers.Timer timer;

		private readonly static object DataLock;

		private static Dictionary<int, DateTime> unused;

		private static Dictionary<int, Texture2DCacheData> loaded;

		private static string lastraisedfile;

		private static DateTime lastraisedtime;

		private static FileSystemWatcher gniw;

		private static FileSystemWatcher iniw;

		private static IniFile mainini;

		private static Dictionary<string, Texture2DData> inidata;

		private readonly static object WriterLock;

		private static List<Texture2DDumpData> writerdata;

		private static System.Timers.Timer writertimer;

		private static string MAINFOLDERKEY;

		private const string TEXTUREPATHKEY = "sPath";

		private const string FLIPTEXTUREKEY = "bFlipTexture";

		private const string INI = "{0}.ini";

		private static string mainfoldersection;

		private static string mainfolder;

		private static string mainfolderdir;

		internal static string GlobalImageDir
		{
			get
			{
				return string.Concat(IniSettings.MainDir, IniSettings.LanguageDir, "Image\\");
			}
		}

		internal static string GlobalTranslationImageDir
		{
			get
			{
				return Texture2DOverride.GlobalImageDir;
			}
		}

		internal static string GlobalTranslationImageIniFileDir
		{
			get
			{
				return Texture2DOverride.GlobalImageDir;
			}
		}

		internal static string GlobalTranslationImageIniFileName
		{
			get
			{
				return string.Format("{0}.ini", IniSettings.ProcessName);
			}
		}

		internal static string GlobalTranslationImageIniFilePath
		{
			get
			{
				return string.Concat(Texture2DOverride.GlobalTranslationImageIniFileDir, Texture2DOverride.GlobalTranslationImageIniFileName);
			}
		}

		internal static string ImageDir
		{
			get
			{
				return string.Concat(IniSettings.MainDir, IniSettings.ProcessPathDir, IniSettings.LanguageDir, "Image\\");
			}
		}

		internal static string MainFolder
		{
			get
			{
				if (Texture2DOverride.mainfolder == null)
				{
					if (!string.IsNullOrEmpty(IniSettings.ProcessPathDir))
					{
						Texture2DOverride.mainfolder = string.Empty;
					}
					else
					{
						Texture2DOverride.mainfolder = IniSettings.ProcessName;
					}
				}
				return Texture2DOverride.mainfolder;
			}
			private set
			{
				char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
				if (value == null)
				{
					if (!string.IsNullOrEmpty(IniSettings.ProcessPathDir))
					{
						Texture2DOverride.mainfolder = string.Empty;
					}
					else
					{
						Texture2DOverride.mainfolder = IniSettings.ProcessName;
					}
				}
				else if (value != string.Empty)
				{
					value = value.Trim();
					if (value != string.Empty && value.IndexOfAny(invalidFileNameChars) != -1)
					{
						if (!string.IsNullOrEmpty(IniSettings.ProcessPathDir))
						{
							value = string.Empty;
						}
						else
						{
							value = IniSettings.ProcessName;
						}
					}
				}
				if (value != Texture2DOverride.mainfolder)
				{
					Texture2DOverride.mainfolder = value;
					Texture2DOverride.mainfolderdir = value;
					if (!string.IsNullOrEmpty(value))
					{
						Texture2DOverride.mainfolderdir = string.Concat(Texture2DOverride.mainfolderdir, "\\");
					}
				}
			}
		}

		internal static string MainFolderDir
		{
			get
			{
				if (Texture2DOverride.mainfolderdir == null)
				{
					if (!string.IsNullOrEmpty(Texture2DOverride.MainFolder))
					{
						Texture2DOverride.mainfolderdir = string.Concat(Texture2DOverride.MainFolder, "\\");
					}
					else
					{
						Texture2DOverride.mainfolderdir = string.Empty;
					}
				}
				return Texture2DOverride.mainfolderdir;
			}
		}

		internal static string MainFolderSection
		{
			get
			{
				if (Texture2DOverride.mainfoldersection == null)
				{
					Texture2DOverride.mainfoldersection = Texture2DOverride.Encode(IniSettings.ProcessFile);
				}
				return Texture2DOverride.mainfoldersection;
			}
		}

		internal static string TranslationImageDir
		{
			get
			{
				if (!(IniSettings.ProcessPathDir == string.Empty) || string.IsNullOrEmpty(Texture2DOverride.MainFolderDir))
				{
					return Texture2DOverride.ImageDir;
				}
				return string.Concat(Texture2DOverride.ImageDir, Texture2DOverride.MainFolderDir);
			}
		}

		internal static string TranslationImageIniFileDir
		{
			get
			{
				return Texture2DOverride.ImageDir;
			}
		}

		internal static string TranslationImageIniFileName
		{
			get
			{
				return string.Format("{0}.ini", IniSettings.ProcessName);
			}
		}

		internal static string TranslationImageIniFilePath
		{
			get
			{
				return string.Concat(Texture2DOverride.TranslationImageIniFileDir, Texture2DOverride.TranslationImageIniFileName);
			}
		}

		static Texture2DOverride()
		{
			Texture2DOverride.DataLock = new object();
			Texture2DOverride.unused = new Dictionary<int, DateTime>();
			Texture2DOverride.loaded = new Dictionary<int, Texture2DCacheData>();
			Texture2DOverride.WriterLock = new object();
			Texture2DOverride.MAINFOLDERKEY = "sMainFolder";
			Texture2DOverride.Load();
			TimeSpan timeSpan = TimeSpan.FromSeconds(15);
			Texture2DOverride.timer = new System.Timers.Timer(timeSpan.TotalMilliseconds)
			{
				AutoReset = true
			};
			Texture2DOverride.timer.Elapsed += new ElapsedEventHandler(Texture2DOverride.timer_Elapsed);
			Texture2DOverride.timer.Start();
			Texture2DOverride.writerdata = new List<Texture2DDumpData>();
			timeSpan = TimeSpan.FromSeconds(15);
			Texture2DOverride.writertimer = new System.Timers.Timer(timeSpan.TotalMilliseconds)
			{
				AutoReset = false
			};
			Texture2DOverride.writertimer.Elapsed += new ElapsedEventHandler(Texture2DOverride.WriterTimerElapsed);
			IniSettings.LanguageDirChanged += new Action<string>((string value) => Texture2DOverride.Load());
			IniSettings.ProcessPathDirChanged += new Action<string>((string value) => Texture2DOverride.Load());
		}

		private static string Encode(string text)
		{
			StringBuilder stringBuilder = new StringBuilder(text);
			stringBuilder.Replace("[", "0x5B");
			stringBuilder.Replace("]", "0x5D");
			return stringBuilder.ToString();
		}

        private static U::UnityEngine.Texture2D FlipTextureY(U::UnityEngine.Texture2D original)
		{
            U::UnityEngine.Texture2D texture2D;
			try
			{
                texture2D = new U::UnityEngine.Texture2D(original.width, original.height, original.format, false);
				int num = original.width;
				int num1 = original.height;
				for (int i = 0; i < num1; i++)
				{
					for (int j = 0; j < num; j++)
					{
						texture2D.SetPixel(j, num1 - i - 1, original.GetPixel(j, i));
					}
				}
				texture2D.Apply();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				texture2D = original;
				IniSettings.Error(string.Concat("FlipTextureY:\n", exception.ToString()));
			}
			return texture2D;
		}

		private static Dictionary<string, string> GetUniqueImageFiles(string dir)
		{
			Dictionary<string, string> strs = new Dictionary<string, string>();
			try
			{
				if (Texture2DData.ValidExtensions.Count > 0 && dir != null && Directory.Exists(dir))
				{
					HashSet<string> strs1 = new HashSet<string>();
					foreach (Texture2DData value in Texture2DOverride.inidata.Values)
					{
						if (strs1.Contains(value.Path) || !value.Exists)
						{
							continue;
						}
						strs1.Add(value.Path);
					}
					string[] files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
					HashSet<string> strs2 = new HashSet<string>(Texture2DData.ValidExtensions);
					string[] strArrays = files;
					for (int i = 0; i < (int)strArrays.Length; i++)
					{
						string str = strArrays[i];
						if (strs2.Contains(Path.GetExtension(str).ToLower()))
						{
							string str1 = str;
							if (str1.StartsWith(dir))
							{
								str1 = str1.Remove(0, dir.Length);
							}
							string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(str);
							if (!strs.ContainsKey(fileNameWithoutExtension) && !strs1.Contains(str1))
							{
								strs.Add(fileNameWithoutExtension, str1);
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				IniSettings.Error(string.Concat("GetUniqueImageFiles:\n", exception.ToString()));
			}
			return strs;
		}

		private static void Load()
		{
			Texture2DCacheData texture2DCacheDatum;
			object dataLock = Texture2DOverride.DataLock;
			Monitor.Enter(dataLock);
			try
			{
				Texture2DOverride.StopWatchTextFiles();
				Dictionary<int, Texture2DCacheData> nums = new Dictionary<int, Texture2DCacheData>(Texture2DOverride.loaded);
				Texture2DOverride.unused.Clear();
				Texture2DOverride.loaded.Clear();
				Texture2DOverride.LoadMainIniFile();
				Texture2DOverride.inidata = Texture2DOverride.ParseIniData(Texture2DOverride.mainini);
				Texture2DOverride.uniqueimagesdata = Texture2DOverride.GetUniqueImageFiles(Texture2DOverride.TranslationImageDir);
				if (Texture2DOverride.GlobalTranslationImageIniFilePath != Texture2DOverride.TranslationImageDir)
				{
					Dictionary<string, Texture2DData> strs = Texture2DOverride.ParseIniData(new IniFile(Texture2DOverride.GlobalTranslationImageIniFilePath));
					foreach (KeyValuePair<string, Texture2DData> keyValuePair in strs)
					{
						if (Texture2DOverride.inidata.ContainsKey(keyValuePair.Key))
						{
							continue;
						}
						Texture2DOverride.inidata.Add(keyValuePair.Key, keyValuePair.Value);
					}
					strs.Clear();
					strs = null;
					Dictionary<string, string> uniqueImageFiles = Texture2DOverride.GetUniqueImageFiles(Texture2DOverride.GlobalTranslationImageDir);
					foreach (KeyValuePair<string, string> uniqueImageFile in uniqueImageFiles)
					{
						if (Texture2DOverride.uniqueimagesdata.ContainsKey(uniqueImageFile.Key))
						{
							continue;
						}
						Texture2DOverride.uniqueimagesdata.Add(uniqueImageFile.Key, uniqueImageFile.Value);
					}
					uniqueImageFiles.Clear();
					uniqueImageFiles = null;
				}
				Texture2DOverride.WatchTextFiles();
				foreach (KeyValuePair<int, Texture2DCacheData> num in nums)
				{
					Texture2DOverrideData overrideData = num.Value.OverrideData;
					Texture2DOverride.LoadTexture2D(num.Value.ObjectName, overrideData.OriginalTexture2D, out overrideData);
					if (!Texture2DOverride.loaded.TryGetValue(overrideData.InstanceID, out texture2DCacheDatum))
					{
						continue;
					}
					texture2DCacheDatum.IncreaseAmount(num.Value.Count);
				}
				if (IniSettings.DebugMode || IniSettings.FindImage)
				{
					int count = Texture2DOverride.inidata.Count;
					IniSettings.Log(string.Concat("ImageSettingsLoaded: ", count.ToString()));
				}
			}
			finally
			{
				Monitor.Exit(dataLock);
			}
		}

		private static void LoadMainIniFile()
		{
			try
			{
				Texture2DOverride.mainini = new IniFile(Texture2DOverride.TranslationImageIniFilePath);
				if (File.Exists(Texture2DOverride.TranslationImageIniFilePath))
				{
					string mainFolderSection = Texture2DOverride.MainFolderSection;
					string mAINFOLDERKEY = Texture2DOverride.MAINFOLDERKEY;
					string value = Texture2DOverride.mainini.GetValue(mainFolderSection, mAINFOLDERKEY, null);
					Texture2DOverride.MainFolder = value;
					if (value != Texture2DOverride.MainFolder)
					{
						Texture2DOverride.mainini.WriteValue(mainFolderSection, mAINFOLDERKEY, Texture2DOverride.MainFolder);
					}
				}
			}
			catch (Exception exception)
			{
				IniSettings.Error(string.Concat("Texture2D::LoadIniFile:", exception.ToString()));
			}
		}

        internal static void LoadTexture2D(string objectName, U::UnityEngine.Texture2D texture2D, out Texture2DOverrideData overrideData)
		{
			string str;
			Texture2DData texture2DDatum;
			string texturePath;
			overrideData = new Texture2DOverrideData();
			try
			{
				if (!string.IsNullOrEmpty(texture2D.name))
				{
					if (string.IsNullOrEmpty(objectName))
					{
						objectName = string.Empty;
					}
					str = (string.IsNullOrEmpty(objectName) || objectName == texture2D.name ? texture2D.name : string.Format("{0}:{1}", objectName, texture2D.name));
					str = Texture2DOverride.Encode(str);
					if (!Texture2DOverride.inidata.TryGetValue(str, out texture2DDatum))
					{
						if (Texture2DOverride.TryLoadCacheTexture(texture2D, ref overrideData))
						{
							texturePath = overrideData.TexturePath;
						}
						else if (!Texture2DOverride.uniqueimagesdata.TryGetValue(texture2D.name, out texturePath))
						{
							texturePath = texture2D.name;
						}
						else
						{
							bool flag = false;
							string lower = Path.GetExtension(texturePath).ToLower();
							if (lower == ".dds" || lower == ".psd")
							{
								flag = true;
							}
							Texture2DOverride.TryLoadNewTexture(str, texture2D, texturePath, ref overrideData, flag);
						}
						Texture2DOverride.inidata.Add(str, new Texture2DData(texturePath, false));
						if (IniSettings.FindImage)
						{
							object writerLock = Texture2DOverride.WriterLock;
							Monitor.Enter(writerLock);
							try
							{
								Texture2DOverride.writerdata.Add(new Texture2DDumpData(str, texturePath, texture2D.format));
								Texture2DOverride.writertimer.Start();
							}
							finally
							{
								Monitor.Exit(writerLock);
							}
						}
					}
					else if (texture2DDatum.Exists && !Texture2DOverride.TryLoadCacheTexture(texture2D, ref overrideData))
					{
						bool flag1 = false;
						string lower1 = Path.GetExtension(texture2DDatum.Path).ToLower();
						if (lower1 == ".dds" || lower1 == ".psd")
						{
							flag1 = true;
						}
						Texture2DOverride.TryLoadNewTexture(str, texture2D, texture2DDatum.Path, ref overrideData, flag1);
					}
				}
			}
			catch (Exception exception)
			{
				IniSettings.Error(string.Concat("LoadTexture2D:\n", exception.ToString()));
			}
		}

        private static bool LoadTextureData(string path, out U::UnityEngine.Texture2D texture2D)
		{
			bool flag;
			texture2D = null;
			try
			{
				path = string.Concat(Texture2DOverride.TranslationImageDir, path);
				string extension = Path.GetExtension(path);
				if (extension == ".dds")
				{
					flag = DXTLoader.LoadDXT(path, out texture2D);
				}
				else if (extension == ".jpeg" || extension == ".jpg")
				{
					texture2D = new Texture2D(2, 2, TextureFormat.RGB24, false);
					flag = texture2D.LoadImage(File.ReadAllBytes(path));
				}
				else if (extension == ".png")
				{
					texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
					flag = texture2D.LoadImage(File.ReadAllBytes(path));
				}
				else if (extension == ".psd")
				{
					flag = PSDLoader.LoadPSD(path, out texture2D);
				}
				else if (extension == ".tga")
				{
					flag = TGALoader.LoadTGA(path, out texture2D);
				}
				else
				{
					return false;
				}
			}
			catch (Exception exception)
			{
				IniSettings.Error(string.Concat("LoadImage:\n", exception.ToString()));
				return false;
			}
			return flag;
		}

		private static Dictionary<string, Texture2DData> ParseIniData(IniFile ini)
		{
			Dictionary<string, Texture2DData> strs = new Dictionary<string, Texture2DData>();
			try
			{
				string[] sections = ini.GetSections();
				for (int i = 0; i < (int)sections.Length; i++)
				{
					string str = sections[i];
					bool flag = false;
					string value = ini.GetValue(str, "sPath", "");
					string value1 = ini.GetValue(str, "bFlipTexture", null);
					if (!string.IsNullOrEmpty(value1) && !bool.TryParse(value1, out flag))
					{
						flag = true;
					}
					Texture2DData texture2DDatum = new Texture2DData(value, flag);
					strs.Remove(str);
					strs.Add(str, texture2DDatum);
				}
			}
			catch (Exception exception)
			{
				IniSettings.Error(string.Concat("LoadIniData:\n", exception.ToString()));
			}
			return strs;
		}

		private static void StopWatchTextFiles()
		{
			if (Texture2DOverride.gniw != null)
			{
				Texture2DOverride.gniw.Dispose();
				Texture2DOverride.gniw = null;
			}
			if (Texture2DOverride.iniw != null)
			{
				Texture2DOverride.iniw.Dispose();
				Texture2DOverride.iniw = null;
			}
		}

		private static void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			object dataLock = Texture2DOverride.DataLock;
			Monitor.Enter(dataLock);
			try
			{
				try
				{
					List<int> nums = new List<int>();
					foreach (KeyValuePair<int, DateTime> keyValuePair in Texture2DOverride.unused)
					{
						if (DateTime.Now <= keyValuePair.Value.AddSeconds(15))
						{
							continue;
						}
						nums.Add(keyValuePair.Key);
						if (Texture2DOverride.loaded[keyValuePair.Key].OverrideData.OriginalTexture2D == null)
						{
							continue;
						}
						int key = keyValuePair.Key;
						Texture2DOverride.loaded.Remove(key);
					}
					if (nums.Count > 0)
					{
						Dictionary<int, DateTime> nums1 = Texture2DOverride.unused;
						foreach (int num in nums)
						{
							nums1.Remove(num);
						}
						Texture2DOverride.unused = nums1;
					}
				}
				catch (Exception exception)
				{
					IniSettings.Error(string.Concat("timer_Elapsed:\n", exception.ToString()));
				}
			}
			finally
			{
				Monitor.Exit(dataLock);
			}
		}

        private static bool TryLoadCacheTexture(U::UnityEngine.Texture2D texture2D, ref Texture2DOverrideData overrideData)
		{
			Texture2DCacheData texture2DCacheDatum;
			bool flag;
			object dataLock = Texture2DOverride.DataLock;
			Monitor.Enter(dataLock);
			try
			{
				int instanceID = texture2D.GetInstanceID();
				if (!Texture2DOverride.loaded.TryGetValue(instanceID, out texture2DCacheDatum))
				{
					flag = false;
				}
				else
				{
					if (!Texture2DOverride.unused.Remove(instanceID))
					{
						texture2DCacheDatum.IncreaseAmount((long)1);
					}
					overrideData = texture2DCacheDatum.OverrideData;
					flag = true;
				}
			}
			finally
			{
				Monitor.Exit(dataLock);
			}
			return flag;
		}

        private static bool TryLoadNewTexture(string section, U::UnityEngine.Texture2D texture2D, string path, ref Texture2DOverrideData overrideData, bool flip = false)
		{
            U::UnityEngine.Texture2D texture2D1;
			bool flag;
			object dataLock = Texture2DOverride.DataLock;
			Monitor.Enter(dataLock);
			try
			{
				if (!Texture2DOverride.LoadTextureData(path, out texture2D1))
				{
					flag = false;
				}
				else
				{
					if (flip)
					{
						texture2D1 = Texture2DOverride.FlipTextureY(texture2D1);
					}
					texture2D1.name = texture2D.name;
					texture2D1.anisoLevel = texture2D.anisoLevel;
					texture2D1.filterMode = texture2D.filterMode;
					texture2D1.mipMapBias = texture2D.mipMapBias;
					texture2D1.wrapMode = texture2D.wrapMode;
					overrideData = new Texture2DOverrideData(texture2D, path, texture2D1);
					Texture2DOverride.loaded.Add(texture2D.GetInstanceID(), new Texture2DCacheData(section, overrideData));
					flag = true;
				}
			}
			finally
			{
				Monitor.Exit(dataLock);
			}
			return flag;
		}

		internal static void UnloadTexture2D(ref Texture2DOverrideData overrideData)
		{
			Texture2DCacheData texture2DCacheDatum;
			object dataLock = Texture2DOverride.DataLock;
			Monitor.Enter(dataLock);
			try
			{
				try
				{
					try
					{
						if (overrideData.OriginalTexture2D != null)
						{
							int instanceID = overrideData.InstanceID;
							if (Texture2DOverride.loaded.TryGetValue(instanceID, out texture2DCacheDatum))
							{
								if (texture2DCacheDatum.Count <= (long)0)
								{
									Texture2DOverride.unused.Add(instanceID, DateTime.Now);
								}
								else
								{
									texture2DCacheDatum.DecreaseAmount((long)1);
								}
							}
						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						IniSettings.Error(string.Concat(new object[] { "UnloadTexture2D[", overrideData.InstanceID, "]:\n", exception.ToString() }));
					}
				}
				finally
				{
					overrideData = new Texture2DOverrideData();
				}
			}
			finally
			{
				Monitor.Exit(dataLock);
			}
		}

		private static void WatcherNotice(object sender, FileSystemEventArgs e)
		{
			if (Texture2DOverride.lastraisedfile == e.FullPath && DateTime.Now < Texture2DOverride.lastraisedtime)
			{
				return;
			}
			Texture2DOverride.lastraisedfile = e.FullPath;
			Texture2DOverride.lastraisedtime = DateTime.Now.AddSeconds(1);
			Texture2DOverride.Load();
		}

		private static void WatchTextFiles()
		{
			if (Texture2DOverride.GlobalTranslationImageIniFilePath != Texture2DOverride.TranslationImageDir && Texture2DOverride.gniw == null && Directory.Exists(Texture2DOverride.GlobalTranslationImageIniFileDir))
			{
				Texture2DOverride.gniw = new FileSystemWatcher(Texture2DOverride.GlobalTranslationImageIniFileDir, Texture2DOverride.GlobalTranslationImageIniFileName)
				{
					NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
					IncludeSubdirectories = false
				};
				Texture2DOverride.gniw.Changed += new FileSystemEventHandler(Texture2DOverride.WatcherNotice);
				Texture2DOverride.gniw.Created += new FileSystemEventHandler(Texture2DOverride.WatcherNotice);
				Texture2DOverride.gniw.Error += new ErrorEventHandler((object sender, ErrorEventArgs e) => IniSettings.Error(e.GetException().ToString()));
				Texture2DOverride.gniw.EnableRaisingEvents = true;
			}
			if (Texture2DOverride.iniw == null && Directory.Exists(Texture2DOverride.TranslationImageIniFileDir))
			{
				Texture2DOverride.iniw = new FileSystemWatcher(Texture2DOverride.TranslationImageIniFileDir, Texture2DOverride.TranslationImageIniFileName)
				{
					NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
					IncludeSubdirectories = false
				};
				Texture2DOverride.iniw.Changed += new FileSystemEventHandler(Texture2DOverride.WatcherNotice);
				Texture2DOverride.iniw.Created += new FileSystemEventHandler(Texture2DOverride.WatcherNotice);
				Texture2DOverride.iniw.Error += new ErrorEventHandler((object sender, ErrorEventArgs e) => IniSettings.Error(e.GetException().ToString()));
				Texture2DOverride.iniw.EnableRaisingEvents = true;
			}
		}

		private static void WriterTimerElapsed(object sender, ElapsedEventArgs e)
		{
			object writerLock = Texture2DOverride.WriterLock;
			Monitor.Enter(writerLock);
			try
			{
				Texture2DOverride.StopWatchTextFiles();
				try
				{
					if (!Directory.Exists(Texture2DOverride.TranslationImageIniFileDir))
					{
						Directory.CreateDirectory(Texture2DOverride.TranslationImageIniFileDir);
					}
					if (!File.Exists(Texture2DOverride.TranslationImageIniFilePath))
					{
						Texture2DOverride.LoadMainIniFile();
					}
					if (!Directory.Exists(Texture2DOverride.TranslationImageDir))
					{
						Directory.CreateDirectory(Texture2DOverride.TranslationImageDir);
					}
					foreach (Texture2DDumpData writerdatum in Texture2DOverride.writerdata)
					{
						Texture2DOverride.mainini.WriteValue(writerdatum.Section, "sPath", writerdatum.Path);
						if (writerdatum.Format != TextureFormat.DXT1 && writerdatum.Format != TextureFormat.DXT5)
						{
							continue;
						}
						Texture2DOverride.mainini.WriteValue(writerdatum.Section, "bFlipTexture", true);
					}
				}
				catch (Exception exception)
				{
					IniSettings.Error(string.Concat("DumpTexture2D:\n", exception.ToString()));
				}
				Texture2DOverride.writerdata.Clear();
				Texture2DOverride.WatchTextFiles();
			}
			finally
			{
				Monitor.Exit(writerLock);
			}
		}
	}
}