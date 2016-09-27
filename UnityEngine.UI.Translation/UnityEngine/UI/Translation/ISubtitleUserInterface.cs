extern alias U;
namespace UnityEngine.UI.Translation
{
    using System.Collections.Generic;

    internal interface ISubtitleUserInterface
    {
        IEnumerable<TextPosition> Anchors { get; }

        bool Bold { get; set; }

        U::UnityEngine.Color FontColor { get; set; }

        string FontName { get; set; }

        int FontSize { get; set; }

        bool Italic { get; set; }

        string this[TextPosition anchor] { get; set; }

        float MarginBottom { get; set; }

        float MarginLeft { get; set; }

        float MarginRight { get; set; }

        float MarginTop { get; set; }
    }
}

