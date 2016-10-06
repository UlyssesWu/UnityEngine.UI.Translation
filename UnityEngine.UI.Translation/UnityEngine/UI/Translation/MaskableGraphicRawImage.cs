extern alias U;
using System;
using System.Threading;
using UnityEngine.UI;
namespace UnityEngine.UI.Translation
{
    [BaseTypeOf("UnityEngine.UI.RawImage", "UnityEngine.UI.dll")]
    public class MaskableGraphicRawImage : MaskableGraphic
    {
        protected Texture2DOverrideData overrideData;

        protected MaskableGraphicRawImage()
        {
        }

        protected override void Awake()
        {
            base.Awake();
            this.Swap();
        }

        protected override void OnDestroy()
        {
            if (base.GetType() == typeof(RawImage))
            {
                ThreadPool.QueueUserWorkItem(delegate (object x) {
                    if (this.overrideData.OriginalTexture2D != null)
                    {
                        Texture2DOverride.UnloadTexture2D(ref this.overrideData);
                    }
                });
            }
            base.OnDestroy();
        }

        private void SetOurOverrideTexture(ref U::UnityEngine.Texture value)
        {
            if (base.GetType() == typeof(RawImage))
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
                        U::UnityEngine.Texture2D textured = value as U::UnityEngine.Texture2D;
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
                            Texture2DOverride.LoadTexture2D(((RawImage) this).name, textured, out this.overrideData);
                            if (this.overrideData.OverrideTexture2D != null)
                            {
                                value = this.overrideData.OverrideTexture2D;
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    IniSettings.Error("RawImageOverride:\n" + exception.ToString());
                }
            }
        }

        protected void SetTexture(ref U::UnityEngine.Texture value)
        {
            if (base.GetType() == typeof(RawImage))
            {
                this.SetOurOverrideTexture(ref value);
            }
        }

        protected override void Start()
        {
            base.Start();
            this.Swap();
        }

        protected void Swap()
        {
            if (base.GetType() == typeof(RawImage))
            {
                ((RawImage) this).texture = ((RawImage) this).texture;
            }
        }
    }
}

