extern alias U;
using Texture2D = U::UnityEngine.Texture2D;
namespace UnityEngine.UI.Translation
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Texture2DOverrideData
    {
        private int instanceID;
        private int overrideID;
        private int cacheID;
        private string originalname;
        private Texture2D originalTexture2D;
        private string texturepath;
        private Texture2D overrideTexture2D;
        private Texture2D overrideTextureCache;
        public int InstanceID
        {
            get
            {
                return this.instanceID;
            }
        }
        public int OverrideID
        {
            get
            {
                return this.overrideID;
            }
        }
        public int CacheID
        {
            get
            {
                return this.cacheID;
            }
        }
        public string OriginalName
        {
            get
            {
                return this.originalname;
            }
        }
        public Texture2D OriginalTexture2D
        {
            get
            {
                return this.originalTexture2D;
            }
        }
        public string TexturePath
        {
            get
            {
                return this.texturepath;
            }
        }
        public Texture2D OverrideTexture2D
        {
            get
            {
                return this.overrideTexture2D;
            }
        }
        public Texture2D OverrideTextureCache
        {
            get
            {
                return this.overrideTextureCache;
            }
            set
            {
                this.cacheID = (value == null) ? 0 : value.GetInstanceID();
                this.overrideTextureCache = value;
            }
        }
        public Texture2DOverrideData(Texture2D originalTexture2D, string texturepath, Texture2D overrideTexture2D)
        {
            if (texturepath != null)
            {
                this.texturepath = texturepath;
            }
            else
            {
                this.texturepath = string.Empty;
            }
            if (originalTexture2D != null)
            {
                this.instanceID = originalTexture2D.GetInstanceID();
                this.originalname = originalTexture2D.name;
            }
            else
            {
                this.instanceID = 0;
                this.originalname = string.Empty;
            }
            this.originalTexture2D = originalTexture2D;
            if (overrideTexture2D != null)
            {
                this.overrideID = overrideTexture2D.GetInstanceID();
            }
            else
            {
                this.overrideID = 0;
            }
            this.overrideTexture2D = overrideTexture2D;
            this.cacheID = 0;
            this.overrideTextureCache = null;
        }
    }
}

