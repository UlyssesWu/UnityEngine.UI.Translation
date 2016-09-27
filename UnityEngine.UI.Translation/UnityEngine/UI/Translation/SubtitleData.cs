namespace UnityEngine.UI.Translation
{
    using System.Collections.Generic;

    internal class SubtitleData : SubtitleDataBase
    {
        public SubtitleData()
        {
        }

        public SubtitleData(string path) : base(path)
        {
        }

        public SubtitleData(string path, List<SubtitleLine> value) : base(path, value)
        {
        }

        public SubtitleData(string path, string key, List<SubtitleLine> value) : base(path, value)
        {
            this.Key = key;
        }

        public override string ToString()
        {
            return string.Format("{0} {{ \"{1}\" ({2}) }}", base.Path, this.Key, base.Value.Count);
        }

        public string Key { get; protected set; }
    }
}

