namespace UnityEngine.UI.Translation
{
    using System.Collections.Generic;

    internal class SubtitleDataBase
    {
        public SubtitleDataBase()
        {
        }

        public SubtitleDataBase(string path) : this(path, new List<SubtitleLine>())
        {
        }

        public SubtitleDataBase(string path, List<SubtitleLine> value)
        {
            this.Path = path;
            this.Value = value;
        }

        public override string ToString()
        {
            return string.Format("{0} {{ \"{1}\" }}", this.Path, this.Value);
        }

        public string Path { get; protected set; }

        public List<SubtitleLine> Value { get; protected set; }
    }
}

