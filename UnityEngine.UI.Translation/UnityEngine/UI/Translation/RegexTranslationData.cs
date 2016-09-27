namespace UnityEngine.UI.Translation
{
    internal class RegexTranslationData : TranslationData
    {
        public RegexTranslationData()
        {
        }

        public RegexTranslationData(string path, string key, string value) : base(path, key, value)
        {
        }

        public override string ToString()
        {
            return string.Format("{0} {{ \"{1}\", \"{2}\" }}", base.Path, base.Key, base.Value);
        }
    }
}

