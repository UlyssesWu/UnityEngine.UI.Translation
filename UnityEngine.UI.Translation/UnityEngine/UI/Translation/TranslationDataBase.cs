namespace UnityEngine.UI.Translation
{
    internal class TranslationDataBase
    {
        public TranslationDataBase()
        {
        }

        public TranslationDataBase(string path) : this(path, string.Empty)
        {
        }

        public TranslationDataBase(string path, string value)
        {
            this.Path = path;
            this.Value = value;
        }

        public override string ToString()
        {
            return string.Format("{0} {{ \"{1}\" }}", this.Path, this.Value);
        }

        public string Path { get; protected set; }

        public string Value { get; protected set; }
    }
}

