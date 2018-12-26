extern alias U;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
//Sadly, Unity 4.x doesn't support JsonUtility.

namespace UnityEngine.UI.Translation
{


    public class SubtitleConfig
    {
        public int Anchor { get; set; } = 2;
        public string FontName { get; set; }
        public int FontSize { get; set; } = 16;
        public string FontColor { get; set; }
        public bool Bold { get; set; } = true;
        public bool Italic { get; set; } = false;
        public int BorderWidth { get; set; } = 2;
        public int ShadowOffset { get; set; } = 3;
        public int MarginLeft { get; set; } = 20;
        public int MarginTop { get; set; } = 20;
        public int MarginRight { get; set; } = 20;
        public int MarginBottom { get; set; } = 20;
    }

    public class TranslationConfig
    {
        public bool DebugMode { get; set; } = false;
        public string Language { get; set; }
        public bool FindImage { get; set; } = false;
        public bool FindAudio { get; set; } = false;
        public bool DumpAudioByLevel { get; set; } = false;
        public bool FindText { get; set; } = false;
        public bool DumpTextByLevel { get; set; } = false;
        public bool UseRegEx { get; set; } = false;
        public bool UseTextPrediction { get; set; } = false;
        public bool UseCopy2Clipboard { get; set; } = false;
        public int Copy2ClipboardTimeMs { get; set; }
    }

}
