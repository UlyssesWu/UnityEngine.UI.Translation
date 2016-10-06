extern alias U;
using System.Runtime.CompilerServices;
using U::UnityEngine.Internal;
using UnityEngine.UI.Translation;
using EngineAudioSource = U::UnityEngine.AudioSource;
namespace UnityEngine
{
    [BaseTypeOf("UnityEngine.AudioSource", "UnityEngine.dll")]
    public class AudioSource : U::UnityEngine.Behaviour
    {
        protected void DoPlay(ulong delay = 0L)
        {
            this.SetSubtitle();
            this.Play(delay);
        }

        protected void DoPlayDelayed(float delay)
        {
            this.SetSubtitle();
            this.PlayDelayed(delay);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void Play([DefaultValue("0")] ulong delay);
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void PlayDelayed(float delay);
        private void SetSubtitle()
        {
            if (base.GetType() == typeof(EngineAudioSource))
            {
                EngineAudioSource source = this as EngineAudioSource;
                AudioSourceSubtitle.Instance.Add(source);
            }
        }
    }
}

