extern alias U;
using Texture2D = U::UnityEngine.Texture2D;
using Material = U::UnityEngine.Material;
using Texture = U::UnityEngine.Texture;
namespace UnityEngine.UI.Translation
{
    using System;
    using System.Threading;


    public class MaterialOverride : U::UnityEngine.Object
    {
        private Texture2DOverrideData overrideData;

        protected MaterialOverride()
        {
        }

        ~MaterialOverride()
        {
            if (base.GetType() == typeof(Material))
            {
                ThreadPool.QueueUserWorkItem(delegate (object x) {
                    if (this.overrideData.OriginalTexture2D != null)
                    {
                        Texture2DOverride.UnloadTexture2D(ref this.overrideData);
                    }
                });
            }
        }

        protected void Swap(ref Texture value)
        {
            if (base.GetType() == typeof(Material))
            {
                try
                {
                    if (value == null)
                    {
                        if (this.overrideData.OriginalTexture2D != null)
                        {
                            Texture2DOverride.UnloadTexture2D(ref this.overrideData);
                        }
                    }
                    else
                    {
                        Texture2D textured = value as Texture2D;
                        if (textured != null)
                        {
                            if (this.overrideData.OriginalTexture2D != null)
                            {
                                int instanceID = textured.GetInstanceID();
                                if ((this.overrideData.InstanceID == instanceID) || (this.overrideData.OverrideID == instanceID))
                                {
                                    value = this.overrideData.OverrideTexture2D;
                                    return;
                                }
                                Texture2DOverride.UnloadTexture2D(ref this.overrideData);
                            }
                            Texture2DOverride.LoadTexture2D(((Material) this).name, textured, out this.overrideData);
                            if (this.overrideData.OverrideTexture2D != null)
                            {
                                value = this.overrideData.OverrideTexture2D;
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    IniSettings.Error("MaterialOverride:\n" + exception.ToString());
                }
            }
        }
    }
}

