using System.Diagnostics;

namespace System
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public class IniFile
    {
        private Dictionary<string, Dictionary<string, string>> ini;
        private readonly string name = Process.GetCurrentProcess().ProcessName;
        private readonly string path;

        public IniFile(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                this.path = this.name + ".ini";
            }
            this.path = Path.GetFullPath(path);
            this.Load();
        }

        public string[] GetKeysInSection(string section = null)
        {
            Dictionary<string, string> dictionary;
            if (string.IsNullOrEmpty(section))
            {
                section = this.name;
            }
            if (this.ini.TryGetValue(section, out dictionary))
            {
                string[] array = new string[dictionary.Keys.Count];
                dictionary.Keys.CopyTo(array, 0);
                return array;
            }
            return new string[0];
        }

        public string[] GetSections()
        {
            string[] array = new string[this.ini.Keys.Count];
            this.ini.Keys.CopyTo(array, 0);
            return array;
        }

        public string GetValue(string section = null, string key = null, object @default = null)
        {
            Dictionary<string, string> dictionary;
            string str;
            if (string.IsNullOrEmpty(section))
            {
                section = this.name;
            }
            if (string.IsNullOrEmpty(key))
            {
                key = this.name;
            }
            if (this.ini.TryGetValue(section, out dictionary) && dictionary.TryGetValue(key, out str))
            {
                return str;
            }
            if (@default != null)
            {
                return @default.ToString();
            }
            return null;
        }

        private void Load()
        {
            this.ini = new Dictionary<string, Dictionary<string, string>>();
            this.ini.Add(this.name, new Dictionary<string, string>());
            if (File.Exists(this.path))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(this.path, Encoding.UTF8))
                    {
                        string name = this.name;
                        while (!reader.EndOfStream)
                        {
                            string str = reader.ReadLine().Trim();
                            if (str.Length != 0)
                            {
                                if (str.StartsWith("[") && str.EndsWith("]"))
                                {
                                    name = str.Substring(1, str.Length - 2);
                                    if (name.Length == 0)
                                    {
                                        name = this.name;
                                    }
                                    if (!this.ini.ContainsKey(name))
                                    {
                                        this.ini.Add(name, new Dictionary<string, string>());
                                    }
                                }
                                else if (!str.StartsWith(";"))
                                {
                                    Dictionary<string, string> dictionary;
                                    char[] separator = new char[] { '=' };
                                    string[] strArray = str.Split(separator, 2);
                                    strArray[0] = strArray[0].Trim();
                                    if (!string.IsNullOrEmpty(strArray[0]) && this.ini.TryGetValue(name, out dictionary))
                                    {
                                        dictionary.Remove(strArray[0]);
                                        if (strArray.Length == 2)
                                        {
                                            char[] chArray2 = new char[] { ';' };
                                            string[] strArray2 = strArray[1].Split(chArray2, 2);
                                            strArray2[0] = strArray2[0].Trim();
                                            dictionary.Add(strArray[0], strArray2[0]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception exception1)
                {
                    throw exception1;
                }
            }
        }

        public void WriteValue(string section = null, string key = null, object value = null)
        {
            Dictionary<string, string> dictionary;
            if (string.IsNullOrEmpty(section))
            {
                section = this.name;
            }
            if (string.IsNullOrEmpty(key))
            {
                key = this.name;
            }
            string str = (value == null) ? string.Empty : value.ToString();
            if (!NativeMethods.WritePrivateProfileString(section, key, str, this.path))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            if (this.ini.TryGetValue(section, out dictionary))
            {
                dictionary.Remove(key);
                dictionary.Add(key, str);
            }
            else
            {
                dictionary = new Dictionary<string, string> {
                    { 
                        key,
                        str
                    }
                };
                this.ini.Add(section, dictionary);
            }
        }

        private class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
            internal static extern int GetPrivateProfileString(string Section, string Key, string Default, string Result, int Size, string FilePath);
            [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
            internal static extern bool WritePrivateProfileString(string Section, string Key, string Value, string FilePath);
        }
    }
}

