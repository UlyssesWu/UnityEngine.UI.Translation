namespace UnityEngine.UI.Translation
{
    using System;

    internal class KeyTranslationData : TranslationDataBase
    {
        public KeyTranslationData()
        {
        }

        public KeyTranslationData(string key, TranslationDataBase data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            base.Path = data.Path;
            this.Key = key;
            base.Value = data.Value;
        }

        public KeyTranslationData(string path, string key, string value) : base(path, value)
        {
            this.Key = key;
        }

        public override string ToString()
        {
            return string.Format("{0} {{ \"{1}\", \"{2}\" }}", base.Path, this.Key, base.Value);
        }

        public string Key { get; private set; }
    }
}

