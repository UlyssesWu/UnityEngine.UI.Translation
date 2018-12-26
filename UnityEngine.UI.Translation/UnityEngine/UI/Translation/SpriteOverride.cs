extern alias U;
using System;
using System.Threading;
using Sprite = U::UnityEngine.Sprite;
using Texture2D = U::UnityEngine.Texture2D;
namespace UnityEngine.UI.Translation
{
    [BaseTypeOf("UnityEngine.Sprite", "UnityEngine.dll")]
    public class SpriteOverride : U::UnityEngine.Object
    {
        private Texture2DOverrideData overrideData;

        protected SpriteOverride()
        {
        }

        ~SpriteOverride()
        {
            if (base.GetType() == typeof(Sprite))
            {
                ThreadPool.QueueUserWorkItem(delegate (object x) {
                    if (this.overrideData.OriginalTexture2D != null)
                    {
                        Texture2DOverride.UnloadTexture2D(ref this.overrideData);
                    }
                });
            }
        }

        protected Texture2D GetTexture()
        {
            if (base.GetType() != typeof(Sprite))
            {
                return null;
            }
            Texture2D textured = ((Sprite) this)._get_texture();
            try
            {
                if (textured == null)
                {
                    return textured;
                }
                if (this.overrideData.OriginalTexture2D != null)
                {
                    return this.overrideData.OverrideTexture2D;
                }
                Texture2DOverride.LoadTexture2D(((Sprite) this).name, textured, out this.overrideData);
                if (this.overrideData.OverrideTexture2D != null)
                {
                    textured = this.overrideData.OverrideTexture2D;
                }
            }
            catch (Exception exception)
            {
                IniSettings.Error("SpriteOverride:\n" + exception.ToString());
            }
            return textured;
        }
    }
}

