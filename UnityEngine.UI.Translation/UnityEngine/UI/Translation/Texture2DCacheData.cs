namespace UnityEngine.UI.Translation
{
    internal class Texture2DCacheData
    {
        private long count;
        private string objectname;
        private Texture2DOverrideData overrideData;

        public Texture2DCacheData(string objectname, Texture2DOverrideData overrideData)
        {
            this.objectname = objectname;
            this.count = 0L;
            this.overrideData = overrideData;
        }

        public void DecreaseAmount(long amount = 1L)
        {
            this.count -= amount;
        }

        public void IncreaseAmount(long amount = 1L)
        {
            this.count += amount;
        }

        public long Count
        {
            get
            {
                return this.count;
            }
        }

        public string ObjectName
        {
            get
            {
                return this.objectname;
            }
        }

        public Texture2DOverrideData OverrideData
        {
            get
            {
                return this.overrideData;
            }
        }
    }
}

