extern alias U;
using Texture2D = U::UnityEngine.Texture2D;

namespace UnityEngine.UI.Translation
{
    using System;

    public class CursorOverride
    {
        private static Texture2DOverrideData overrideData;

        protected CursorOverride()
        {
        }

        protected static void Swap(ref Texture2D texture)
        {
            try
            {
                if (texture == null)
                {
                    if (overrideData.OriginalTexture2D != null)
                    {
                        Texture2DOverride.UnloadTexture2D(ref overrideData);
                    }
                }
                else
                {
                    if (overrideData.OriginalTexture2D != null)
                    {
                        int instanceID = texture.GetInstanceID();
                        if (overrideData.InstanceID == instanceID)
                        {
                            texture = overrideData.OverrideTextureCache;
                            return;
                        }
                        Texture2DOverride.UnloadTexture2D(ref overrideData);
                    }
                    Texture2DOverride.LoadTexture2D(texture.name, texture, out overrideData);
                    if (overrideData.OverrideTexture2D != null)
                    {
                        if ((texture.width != overrideData.OverrideTexture2D.width) || (texture.height != overrideData.OverrideTexture2D.height))
                        {
                            texture.Resize(overrideData.OverrideTexture2D.width, overrideData.OverrideTexture2D.height);
                        }
                        texture.SetPixels32(overrideData.OverrideTexture2D.GetPixels32());
                        texture.Apply();
                        overrideData.OverrideTextureCache = texture;
                    }
                }
            }
            catch (Exception exception)
            {
                IniSettings.Error("CursorOverride:\n" + exception.ToString());
            }
        }
    }
}

