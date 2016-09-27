extern alias U;
using TextureFormat = U::UnityEngine.TextureFormat;
using System.Runtime.InteropServices;

namespace UnityEngine.UI.Translation
{


    [StructLayout(LayoutKind.Sequential)]
    internal struct Texture2DDumpData
    {
        private string section;
        private string path;
        private TextureFormat format;
        public string Section
        {
            get
            {
                return this.section;
            }
        }
        public string Path
        {
            get
            {
                return this.path;
            }
        }
        public TextureFormat Format
        {
            get
            {
                return this.format;
            }
        }
        public Texture2DDumpData(string section, string path, TextureFormat format)
        {
            this.section = section;
            this.path = path;
            this.format = format;
        }
    }
}

