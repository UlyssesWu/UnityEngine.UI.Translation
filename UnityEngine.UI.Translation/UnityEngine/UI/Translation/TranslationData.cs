namespace UnityEngine.UI.Translation
{
    internal class TranslationData : TranslationDataBase
    {
        public TranslationData()
        {
        }

        public TranslationData(string path) : base(path)
        {
        }

        public TranslationData(string path, string value) : base(path, value)
        {
        }

        public TranslationData(string path, string key, string value) : base(path, value)
        {
            this.Key = key;
        }

        public override string ToString()
        {
            return string.Format("{0} {{ \"{1}\" = \"{2}\" }}", base.Path, this.Key, base.Value);
        }

        public string Key { get; protected set; }
    }
}

