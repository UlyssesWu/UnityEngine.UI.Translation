namespace UnityEngine.UI.Translation
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SubtitleLine
    {
        public TextPosition Position { get; set; }
        public float StartTime { get; set; }
        public float EndTime { get; set; }
        public string Text { get; set; }
    }
}

