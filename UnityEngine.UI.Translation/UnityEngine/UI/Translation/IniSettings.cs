using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;

namespace UnityEngine.UI.Translation
{
    extern alias U;

    internal static class IniSettings
    {
        internal const string DIR1 = ".\\Plugins\\";

        internal const string DIR2 = "UITranslation\\";

        internal const string DIR3 = "Text\\";

        internal const string DIR4 = "Image\\";

        internal const string DIR5 = "Audio\\";

        private const string INI = "Translation.ini";

        private const string LOG = "Translation.log";

        internal const string SECTION = "Translation";

        private const string DEBUGMODEKEY = "bDebugMode";

        private const string FINDTEXTKEY = "bFindText";

        private const string DUMPTEXTBYLEVELKEY = "bDumpTextByLevel";

        private const string FINDAUDIOKEY = "bFindAudio";

        private const string DUMPAUDIOBYLEVELKEY = "bDumpAudioByLevel";

        private const string FINDIMAGEKEY = "bFindImage";

        private const string LANGUAGEKEY = "sLanguage";

        private const string USEREGEXKEY = "bUseRegEx";

        private const string USETEXTPREDICTIONKEY = "bUseTextPrediction";

        private const string USECOPY2CLIPBOARDKEY = "bUseCopy2Clipboard";

        private const string COPY2CLIPBOARDTIMEKEY = "iCopy2ClipboardTime(ms)";

        private static bool debugmode;

        private static string language;

        private static string languagedir;

        private static bool findimage;

        private static bool findaudio;

        private static bool dumpaudiobylevel;

        private static bool findtext;

        private static bool dumptextbylevel;

        private static bool useregex;

        private static bool usetextprediction;

        private static bool usecopy2clipboard;

        private static int copy2clipboardtime;

        private static string processpath;

        private static string processpathdir;

        private static string PROCESSPATHKEY;

        private static string processname;

        private static string processfile;

        private static bool initialized;

        private static string lastraisedfile;

        private static DateTime lastraisedtime;

        private static FileSystemWatcher iniw;

        private static StringBuilder sb;

        private static System.Timers.Timer timer;

        private static int writetime;

        private readonly static object LoadLock;

        private readonly static object LogLock;

        internal static int Copy2ClipboardTime
        {
            get
            {
                return IniSettings.copy2clipboardtime;
            }
            private set
            {
                if (value != IniSettings.copy2clipboardtime)
                {
                    IniSettings.copy2clipboardtime = value;
                    if (IniSettings.Copy2ClipboardTimeChanged != null && IniSettings.initialized)
                    {
                        IniSettings.Copy2ClipboardTimeChanged(value);
                    }
                }
            }
        }

        internal static bool DebugMode
        {
            get
            {
                return IniSettings.debugmode;
            }
            private set
            {
                if (value != IniSettings.debugmode)
                {
                    IniSettings.debugmode = value;
                    if (IniSettings.DebugModeChanged != null && IniSettings.initialized)
                    {
                        IniSettings.DebugModeChanged(value);
                    }
                }
            }
        }

        internal static bool DumpAudioByLevel
        {
            get
            {
                return IniSettings.dumpaudiobylevel;
            }
            private set
            {
                if (value != IniSettings.dumpaudiobylevel)
                {
                    IniSettings.dumpaudiobylevel = value;
                    if (IniSettings.DumpAudioByLevelChanged != null && IniSettings.initialized)
                    {
                        IniSettings.DumpAudioByLevelChanged(value);
                    }
                }
            }
        }

        internal static bool DumpTextByLevel
        {
            get
            {
                return IniSettings.dumptextbylevel;
            }
            private set
            {
                if (value != IniSettings.dumptextbylevel)
                {
                    IniSettings.dumptextbylevel = value;
                    if (IniSettings.DumpTextByLevelChanged != null && IniSettings.initialized)
                    {
                        IniSettings.DumpTextByLevelChanged(value);
                    }
                }
            }
        }

        internal static bool FindAudio
        {
            get
            {
                return IniSettings.findaudio;
            }
            private set
            {
                if (value != IniSettings.findaudio)
                {
                    IniSettings.findaudio = value;
                    if (IniSettings.FindAudioChanged != null && IniSettings.initialized)
                    {
                        IniSettings.FindAudioChanged(value);
                    }
                }
            }
        }

        internal static bool FindImage
        {
            get
            {
                return IniSettings.findimage;
            }
            private set
            {
                if (value != IniSettings.findimage)
                {
                    IniSettings.findimage = value;
                    if (IniSettings.FindImageChanged != null && IniSettings.initialized)
                    {
                        IniSettings.FindImageChanged(value);
                    }
                }
            }
        }

        internal static bool FindText
        {
            get
            {
                return IniSettings.findtext;
            }
            private set
            {
                if (value != IniSettings.findtext)
                {
                    IniSettings.findtext = value;
                    if (IniSettings.FindTextChanged != null && IniSettings.initialized)
                    {
                        IniSettings.FindTextChanged(value);
                    }
                }
            }
        }

        internal static string Language
        {
            get
            {
                if (IniSettings.language == null)
                {
                    IniSettings.language = string.Empty;
                }
                return IniSettings.language;
            }
            private set
            {
                char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
                if (value == null)
                {
                    value = string.Empty;
                }
                else if (value != string.Empty)
                {
                    value = value.Trim();
                    if (value != string.Empty)
                    {
                        if (value.Length > 5)
                        {
                            value = value.Substring(0, 5);
                        }
                        if (value.IndexOfAny(invalidFileNameChars) != -1)
                        {
                            value = string.Empty;
                        }
                    }
                }
                if (value != IniSettings.language)
                {
                    IniSettings.language = value;
                    if (IniSettings.LanguageChanged != null && IniSettings.initialized)
                    {
                        IniSettings.LanguageChanged(value);
                    }
                    IniSettings.languagedir = value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        IniSettings.languagedir = string.Concat(IniSettings.languagedir, "\\");
                    }
                    if (IniSettings.LanguageDirChanged != null && IniSettings.initialized)
                    {
                        IniSettings.LanguageDirChanged(value);
                    }
                }
            }
        }

        internal static string LanguageDir
        {
            get
            {
                if (IniSettings.languagedir == null)
                {
                    if (!string.IsNullOrEmpty(IniSettings.Language))
                    {
                        IniSettings.languagedir = string.Concat(IniSettings.Language, "\\");
                    }
                    else
                    {
                        IniSettings.languagedir = string.Empty;
                    }
                }
                return IniSettings.languagedir;
            }
        }

        internal static string LogFileDir
        {
            get
            {
                return IniSettings.MainDir;
            }
        }

        internal static string LogFileName
        {
            get
            {
                return "Translation.log";
            }
        }

        internal static string LogFilePath
        {
            get
            {
                return string.Concat(IniSettings.LogFileDir, IniSettings.LogFileName);
            }
        }

        internal static int LogWriterTime
        {
            get
            {
                return IniSettings.writetime;
            }
            set
            {
                if (value < 1)
                {
                    value = 1;
                }
                IniSettings.writetime = value;
            }
        }

        internal static string MainDir
        {
            get
            {
                return string.Concat(IniSettings.PluginDir, "UITranslation\\");
            }
        }

        internal static string PluginDir
        {
            get
            {
                return ".\\Plugins\\";
            }
        }

        internal static string ProcessFile
        {
            get
            {
                return IniSettings.processfile;
            }
        }

        internal static string ProcessName
        {
            get
            {
                return IniSettings.processname;
            }
        }

        internal static string ProcessPath
        {
            get
            {
                if (IniSettings.processpath == null)
                {
                    IniSettings.ProcessPath = IniSettings.processname;
                }
                return IniSettings.processpath;
            }
            private set
            {
                char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
                if (value == null)
                {
                    value = IniSettings.processname;
                }
                else if (value != string.Empty)
                {
                    value = value.Trim();
                    if (value != string.Empty && value.IndexOfAny(invalidFileNameChars) != -1)
                    {
                        value = IniSettings.processname;
                    }
                }
                if (value != IniSettings.processpath)
                {
                    IniSettings.processpath = value;
                    if (IniSettings.ProcessPathChanged != null && IniSettings.initialized)
                    {
                        IniSettings.ProcessPathChanged(value);
                    }
                    IniSettings.processpathdir = value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        IniSettings.processpathdir = string.Concat(IniSettings.processpathdir, "\\");
                    }
                    if (IniSettings.ProcessPathDirChanged != null && IniSettings.initialized)
                    {
                        IniSettings.ProcessPathDirChanged(value);
                    }
                }
            }
        }

        internal static string ProcessPathDir
        {
            get
            {
                if (IniSettings.processpathdir == null)
                {
                    if (!string.IsNullOrEmpty(IniSettings.ProcessPath))
                    {
                        IniSettings.processpathdir = string.Concat(IniSettings.ProcessPath, "\\");
                    }
                    else
                    {
                        IniSettings.processpathdir = string.Empty;
                    }
                }
                return IniSettings.processpathdir;
            }
        }

        internal static string SettingsFileDir
        {
            get
            {
                return IniSettings.MainDir;
            }
        }

        internal static string SettingsFileName => "Translation.json";//"Translation.ini";


        internal static string SettingsFilePath
        {
            get
            {
                return string.Concat(IniSettings.SettingsFileDir, IniSettings.SettingsFileName);
            }
        }

        internal static bool UseCopy2Clipboard
        {
            get
            {
                return IniSettings.usecopy2clipboard;
            }
            private set
            {
                if (value != IniSettings.usecopy2clipboard)
                {
                    IniSettings.usecopy2clipboard = value;
                    if (IniSettings.UseCopy2ClipboardChanged != null && IniSettings.initialized)
                    {
                        IniSettings.UseCopy2ClipboardChanged(value);
                    }
                }
            }
        }

        internal static bool UseRegEx
        {
            get
            {
                return IniSettings.useregex;
            }
            private set
            {
                if (value != IniSettings.useregex)
                {
                    IniSettings.useregex = value;
                    if (IniSettings.UseRegExChanged != null && IniSettings.initialized)
                    {
                        IniSettings.UseRegExChanged(value);
                    }
                }
            }
        }

        internal static bool UseTextPrediction
        {
            get
            {
                return IniSettings.usetextprediction;
            }
            private set
            {
                if (value != IniSettings.usetextprediction)
                {
                    IniSettings.usetextprediction = value;
                    if (IniSettings.UseTextPredictionChanged != null && IniSettings.initialized)
                    {
                        IniSettings.UseTextPredictionChanged(value);
                    }
                }
            }
        }

        static IniSettings()
        {
            IniSettings.initialized = false;
            IniSettings.writetime = 3;
            IniSettings.LoadLock = new object();
            IniSettings.LogLock = new object();
            IniSettings.processname = Process.GetCurrentProcess().ProcessName;
            IniSettings.processfile = string.Concat(IniSettings.processname, ".exe");
            IniSettings.PROCESSPATHKEY = string.Concat(IniSettings.processname, "_Folder");
            IniSettings.sb = new StringBuilder();
            TimeSpan timeSpan = TimeSpan.FromSeconds((double)IniSettings.LogWriterTime);
            IniSettings.timer = new System.Timers.Timer(timeSpan.TotalMilliseconds)
            {
                AutoReset = false
            };
            IniSettings.timer.Elapsed += new ElapsedEventHandler(IniSettings.timer_Elapsed);
            try
            {
                if (File.Exists(IniSettings.LogFilePath))
                {
                    File.Delete(IniSettings.LogFilePath);
                }
            }
            catch (Exception exception)
            {
                IniSettings.Error(string.Concat("IniSettings:\n", exception.ToString()));
            }
#if UNITY4
            LoadIni();
#else
            IniSettings.Load();
#endif
            IniSettings.WatchTextFiles();
        }

        internal static void Error(object obj = null)
        {
            if (IniSettings.DebugMode)
            {
                IniSettings.Log(obj);
            }
        }

        internal static IniFile GetINIFile()
        {
            return new IniFile(IniSettings.SettingsFilePath);
        }

        internal static TranslationConfig GetJsonConfig()
        {
            var j = U::UnityEngine.JsonUtility.FromJson<TranslationConfig>(File.ReadAllText(IniSettings.SettingsFilePath));
            if (j == null)
            {
                j = new TranslationConfig();
                File.WriteAllText(IniSettings.SettingsFilePath, U::UnityEngine.JsonUtility.ToJson(j, true));
            }
            return j;
        }

        internal static void LoadIni()
        {
            int num;
            bool flag;
            object loadLock = IniSettings.LoadLock;
            Monitor.Enter(loadLock);
            try
            {
                try
                {
                    if (IniSettings.iniw != null)
                    {
                        IniSettings.iniw.Dispose();
                        IniSettings.iniw = null;
                    }
                    if (!Directory.Exists(IniSettings.SettingsFileDir))
                    {
                        Directory.CreateDirectory(IniSettings.SettingsFileDir);
                    }
                    IniFile nIFile = IniSettings.GetINIFile();
                    string pROCESSPATHKEY = "bDebugMode";
                    string value = nIFile.GetValue("Translation", pROCESSPATHKEY, null);
                    if (value == null || !bool.TryParse(value, out flag))
                    {
                        flag = false;
                        nIFile.WriteValue("Translation", pROCESSPATHKEY, flag);
                    }
                    IniSettings.DebugMode = flag;
                    pROCESSPATHKEY = "sLanguage";
                    value = nIFile.GetValue("Translation", pROCESSPATHKEY, null);
                    IniSettings.Language = value;
                    if (value != IniSettings.Language)
                    {
                        nIFile.WriteValue("Translation", pROCESSPATHKEY, IniSettings.Language);
                    }
                    pROCESSPATHKEY = "bFindImage";
                    value = nIFile.GetValue("Translation", pROCESSPATHKEY, null);
                    if (value == null || !bool.TryParse(value, out flag))
                    {
                        flag = false;
                        nIFile.WriteValue("Translation", pROCESSPATHKEY, flag);
                    }
                    IniSettings.FindImage = flag;
                    pROCESSPATHKEY = "bFindAudio";
                    value = nIFile.GetValue("Translation", pROCESSPATHKEY, null);
                    if (value == null || !bool.TryParse(value, out flag))
                    {
                        flag = false;
                        nIFile.WriteValue("Translation", pROCESSPATHKEY, flag);
                    }
                    IniSettings.FindAudio = flag;
                    pROCESSPATHKEY = "bDumpAudioByLevel";
                    value = nIFile.GetValue("Translation", pROCESSPATHKEY, null);
                    if (value == null || !bool.TryParse(value, out flag))
                    {
                        flag = true;
                        nIFile.WriteValue("Translation", pROCESSPATHKEY, flag);
                    }
                    IniSettings.DumpAudioByLevel = flag;
                    pROCESSPATHKEY = "bFindText";
                    value = nIFile.GetValue("Translation", pROCESSPATHKEY, null);
                    if (value == null || !bool.TryParse(value, out flag))
                    {
                        flag = false;
                        nIFile.WriteValue("Translation", pROCESSPATHKEY, flag);
                    }
                    IniSettings.FindText = flag;
                    pROCESSPATHKEY = "bDumpTextByLevel";
                    value = nIFile.GetValue("Translation", pROCESSPATHKEY, null);
                    if (value == null || !bool.TryParse(value, out flag))
                    {
                        flag = true;
                        nIFile.WriteValue("Translation", pROCESSPATHKEY, flag);
                    }
                    IniSettings.DumpTextByLevel = flag;
                    pROCESSPATHKEY = "bUseRegEx";
                    value = nIFile.GetValue("Translation", pROCESSPATHKEY, null);
                    if (value == null || !bool.TryParse(value, out flag))
                    {
                        flag = true;
                        nIFile.WriteValue("Translation", pROCESSPATHKEY, flag);
                    }
                    IniSettings.UseRegEx = flag;
                    pROCESSPATHKEY = "bUseTextPrediction";
                    value = nIFile.GetValue("Translation", pROCESSPATHKEY, null);
                    if (value == null || !bool.TryParse(value, out flag))
                    {
                        flag = true;
                        nIFile.WriteValue("Translation", pROCESSPATHKEY, flag);
                    }
                    IniSettings.UseTextPrediction = flag;
                    pROCESSPATHKEY = "bUseCopy2Clipboard";
                    value = nIFile.GetValue("Translation", pROCESSPATHKEY, null);
                    if (value == null || !bool.TryParse(value, out flag))
                    {
                        flag = false;
                        nIFile.WriteValue("Translation", pROCESSPATHKEY, flag);
                    }
                    IniSettings.UseCopy2Clipboard = flag;
                    pROCESSPATHKEY = "iCopy2ClipboardTime(ms)";
                    value = nIFile.GetValue("Translation", pROCESSPATHKEY, null);
                    if (value == null || !int.TryParse(value, out num))
                    {
                        num = 250;
                        nIFile.WriteValue("Translation", pROCESSPATHKEY, num);
                    }
                    IniSettings.Copy2ClipboardTime = num;
                    pROCESSPATHKEY = IniSettings.PROCESSPATHKEY;
                    value = nIFile.GetValue("Translation", pROCESSPATHKEY, null);
                    IniSettings.ProcessPath = value;
                    if (value != IniSettings.ProcessPath)
                    {
                        nIFile.WriteValue("Translation", pROCESSPATHKEY, IniSettings.ProcessPath);
                    }
                    IniSettings.initialized = true;
                    try
                    {
                        Action<IniFile> action = IniSettings.LoadSettings;
                        if (action != null)
                        {
                            action(nIFile);
                        }
                    }
                    catch (Exception exception)
                    {
                        IniSettings.Error(string.Concat("LoadSettings:\n", exception.ToString()));
                    }
                    IniSettings.WatchTextFiles();
                }
                catch (Exception exception1)
                {
                    IniSettings.Error(string.Concat("LoadSettings:\n", exception1.ToString()));
                }
            }
            finally
            {
                Monitor.Exit(loadLock);
            }
        }

        internal static void Load()
        {
            int num;
            bool flag;
            object loadLock = IniSettings.LoadLock;
            Monitor.Enter(loadLock);
            try
            {
                try
                {
                    if (IniSettings.iniw != null)
                    {
                        IniSettings.iniw.Dispose();
                        IniSettings.iniw = null;
                    }
                    if (!Directory.Exists(IniSettings.SettingsFileDir))
                    {
                        Directory.CreateDirectory(IniSettings.SettingsFileDir);
                    }
                    TranslationConfig config = IniSettings.GetJsonConfig();
                    IniSettings.DebugMode = config.DebugMode;
                    IniSettings.Language = config.Language;
                    IniSettings.FindImage = config.FindImage;
                    IniSettings.FindAudio = config.FindAudio;
                    IniSettings.DumpAudioByLevel = config.DumpAudioByLevel;
                    IniSettings.FindText = config.FindText;
                    IniSettings.DumpTextByLevel = config.DumpTextByLevel;
                    IniSettings.UseRegEx = config.UseRegEx;
                    IniSettings.UseTextPrediction = config.UseTextPrediction;
                    IniSettings.UseCopy2Clipboard = config.UseCopy2Clipboard;
                    IniSettings.Copy2ClipboardTime = config.Copy2ClipboardTimeMs;

                    IniSettings.initialized = true;
                    IniSettings.WatchTextFiles();
                }
                catch (Exception exception1)
                {
                    IniSettings.Error(string.Concat("LoadSettings:\n", exception1.ToString()));
                }
            }
            finally
            {
                Monitor.Exit(loadLock);
            }
        }

        internal static void Log(object obj = null)
        {
            object logLock = IniSettings.LogLock;
            Monitor.Enter(logLock);
            try
            {
                if (obj == null)
                {
                    obj = "null";
                }
                IniSettings.sb.AppendLine(obj.ToString());
                IniSettings.timer.Start();
            }
            finally
            {
                Monitor.Exit(logLock);
            }
        }

        private static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            object logLock = IniSettings.LogLock;
            Monitor.Enter(logLock);
            try
            {
                try
                {
                    if (!Directory.Exists(IniSettings.LogFileDir))
                    {
                        Directory.CreateDirectory(IniSettings.LogFileDir);
                    }
                    using (StreamWriter streamWriter = new StreamWriter(IniSettings.LogFilePath, true, Encoding.UTF8))
                    {
                        streamWriter.Write(IniSettings.sb.ToString());
                        IniSettings.sb.Length = 0;
                    }
                }
                catch
                {
                }
            }
            finally
            {
                Monitor.Exit(logLock);
            }
        }

        private static void WatcherNotice(object sender, FileSystemEventArgs e)
        {
            if (IniSettings.lastraisedfile == e.FullPath && DateTime.Now < IniSettings.lastraisedtime)
            {
                return;
            }
            IniSettings.lastraisedfile = e.FullPath;
            IniSettings.lastraisedtime = DateTime.Now.AddSeconds(1);
            IniSettings.Load();
        }

        private static void WatchTextFiles()
        {
            try
            {
                if (IniSettings.iniw == null && Directory.Exists(IniSettings.SettingsFileDir))
                {
                    IniSettings.iniw = new FileSystemWatcher(IniSettings.SettingsFileDir, IniSettings.SettingsFileName)
                    {
                        NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                        IncludeSubdirectories = false
                    };
                    IniSettings.iniw.Changed += new FileSystemEventHandler(IniSettings.WatcherNotice);
                    IniSettings.iniw.Created += new FileSystemEventHandler(IniSettings.WatcherNotice);
                    IniSettings.iniw.Error += new ErrorEventHandler((object sender, ErrorEventArgs e) => IniSettings.Error(e.GetException().ToString()));
                    IniSettings.iniw.EnableRaisingEvents = true;
                }
            }
            catch (Exception exception)
            {
                IniSettings.Error(string.Concat("WatchTextFiles:\n", exception.ToString()));
            }
        }

        internal static event Action<int> Copy2ClipboardTimeChanged;

        internal static event Action<bool> DebugModeChanged;

        internal static event Action<bool> DumpAudioByLevelChanged;

        internal static event Action<bool> DumpTextByLevelChanged;

        internal static event Action<bool> FindAudioChanged;

        internal static event Action<bool> FindImageChanged;

        internal static event Action<bool> FindTextChanged;

        internal static event Action<string> LanguageChanged;

        internal static event Action<string> LanguageDirChanged;

        internal static event Action<IniFile> LoadSettings;

        internal static event Action<string> ProcessPathChanged;

        internal static event Action<string> ProcessPathDirChanged;

        internal static event Action<bool> UseCopy2ClipboardChanged;

        internal static event Action<bool> UseRegExChanged;

        internal static event Action<bool> UseTextPredictionChanged;

        private class NativeMethods
        {
            internal const int ATTACH_PARENT_PROCESS = -1;

            public NativeMethods()
            {
            }

            [DllImport("kernel32.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
            internal static extern bool AllocConsole();

            [DllImport("kernel32.dll", CharSet = CharSet.None, ExactSpelling = false, SetLastError = true)]
            internal static extern bool AttachConsole(int dwProcessId);
        }
    }
}