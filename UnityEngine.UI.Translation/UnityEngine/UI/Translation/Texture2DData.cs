namespace UnityEngine.UI.Translation
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Texture2DData
    {
        internal const bool DEFAULTDDSFLIPMODE = true;
        private static ReadOnlyCollection<string> validexts;
        private bool exists;
        private string path;
        private bool flip;
        internal static ReadOnlyCollection<string> ValidExtensions
        {
            get
            {
                return validexts;
            }
        }
        internal bool Exists
        {
            get
            {
                return this.exists;
            }
        }
        internal string Path
        {
            get
            {
                return this.path;
            }
        }
        internal bool Flip
        {
            get
            {
                return this.flip;
            }
        }
        static Texture2DData()
        {
            string[] collection = new string[] { ".dds", ".jpeg", ".jpg", ".png", ".psd", ".tga" };
            List<string> list = new List<string>(collection);
            list.Sort();
            validexts = new ReadOnlyCollection<string>(list);
        }

        internal Texture2DData(string path, bool flip = false)
        {
            this.exists = false;
            this.path = path;
            this.flip = flip;
            this.exists = this.GetImagePath(ref this.path);
            System.IO.Path.GetExtension(this.path).ToLower();
        }

        private bool GetImagePath(ref string file)
        {
            if (!string.IsNullOrEmpty(file))
            {
                string path = Texture2DOverride.TranslationImageDir + file;
                if (File.Exists(path))
                {
                    return true;
                }
                foreach (string str2 in validexts)
                {
                    if (File.Exists(path + str2))
                    {
                        file = file + str2;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

