extern alias U;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnityEngine.UI.Translation
{
    internal class Subtitle : ISubtitle
    {
        private IEnumerable<TextPosition> anchors;
        private Dictionary<TextPosition, StringBuilder> content;
        private Dictionary<TextPosition, HashSet<SubtitleLine>> display;
        private bool invalid;
        private bool loaded;
        private LineData[] subtitles;

        public Subtitle(IEnumerable<TextPosition> anchors, U::UnityEngine.AudioSource source)
        {
            this.anchors = anchors;
            this.content = new Dictionary<TextPosition, StringBuilder>();
            this.display = new Dictionary<TextPosition, HashSet<SubtitleLine>>();
            foreach (TextPosition position in this.anchors)
            {
                this.content.Add(position, new StringBuilder(0x200));
                this.display.Add(position, new HashSet<SubtitleLine>());
            }
            this.Source = source;
            this.Load();
        }

        private StringBuilder ClearContent(StringBuilder sb)
        {
            if (sb.Length > 0)
            {
                sb.Length = 0;
            }
            return sb;
        }

        public void LateUpdate()
        {
            try
            {
                if (((this.Source != null) && (this.Clip != null)) && (this.Source.clip == this.Clip))
                {
                    int num = (this.Clip.frequency == 0) ? 0xac44 : this.Clip.frequency;
                    float num2 = this.Source.timeSamples * (1f / ((float) num));
                    foreach (LineData data in this.subtitles)
                    {
                        if (data != null)
                        {
                            if ((this.Source.isPlaying && ((data.Line.EndTime == 0f) || (num2 < data.Line.EndTime))) && (num2 >= data.Line.StartTime))
                            {
                                this.invalid |= data.Show();
                            }
                            else
                            {
                                this.invalid |= data.Hide();
                            }
                        }
                    }
                    if (this.invalid)
                    {
                        foreach (KeyValuePair<TextPosition, HashSet<SubtitleLine>> pair in this.display)
                        {
                            StringBuilder builder = this.ClearContent(this.content[pair.Key]);
                            foreach (SubtitleLine line2 in pair.Value)
                            {
                                if (builder.Length > 0)
                                {
                                    builder.Append('\n');
                                }
                                builder.Append(line2.Text);
                            }
                        }
                        this.invalid = false;
                    }
                }
                else if (this.loaded)
                {
                    this.Unload();
                }
            }
            catch (Exception exception)
            {
                IniSettings.Error("Subtitle::LateUpdate:\n" + exception.ToString());
            }
        }

        private void Load()
        {
            if ((this.Source != null) && (this.Source.clip != null))
            {
                this.Clip = this.Source.clip;
                this.LoadSubtitles();
                this.invalid = true;
                this.loaded = true;
            }
        }

        private void LoadSubtitles()
        {
            if ((this.Clip == null) || string.IsNullOrEmpty(this.Clip.name))
            {
                this.subtitles = new LineData[0];
            }
            else
            {
                SubtitleLine[] lineArray;
                foreach (TextPosition position in this.anchors)
                {
                    this.display[position].Clear();
                    this.ClearContent(this.content[position]);
                }
                if (SubtitleTranslator.Translate(this.Clip.name, out lineArray))
                {
                    this.subtitles = new LineData[lineArray.Length];
                    for (int i = 0; i < this.subtitles.Length; i++)
                    {
                        HashSet<SubtitleLine> set;
                        if (this.display.TryGetValue(lineArray[i].Position, out set))
                        {
                            this.subtitles[i] = new LineData(lineArray[i], set);
                        }
                    }
                }
                else
                {
                    this.subtitles = new LineData[0];
                }
            }
        }

        public void Reload()
        {
            this.LoadSubtitles();
        }

        private void Unload()
        {
            this.Clip = null;
            this.Source = null;
            this.subtitles = null;
            foreach (TextPosition position in this.display.Keys)
            {
                this.display[position].Clear();
                this.ClearContent(this.content[position]);
            }
            this.invalid = false;
            this.loaded = false;
        }

        public U::UnityEngine.AudioClip Clip { get; private set; }

        public string this[TextPosition anchor]
        {
            get
            {
                return this.content[anchor].ToString();
            }
        }

        public U::UnityEngine.AudioSource Source { get; private set; }

        private class LineData
        {
            private readonly HashSet<SubtitleLine> anchor;

            public LineData(SubtitleLine line, HashSet<SubtitleLine> anchor)
            {
                this.Line = line;
                this.Visible = false;
                this.anchor = anchor;
            }

            public bool Hide()
            {
                bool flag = false;
                if (this.Visible)
                {
                    flag = this.anchor.Remove(this.Line);
                    this.Visible = !flag;
                }
                return flag;
            }

            public bool Show()
            {
                bool flag = false;
                if (!this.Visible)
                {
                    flag = this.anchor.Add(this.Line);
                    this.Visible = flag;
                }
                return flag;
            }

            public SubtitleLine Line { get; private set; }

            public bool Visible { get; private set; }
        }
    }
}

