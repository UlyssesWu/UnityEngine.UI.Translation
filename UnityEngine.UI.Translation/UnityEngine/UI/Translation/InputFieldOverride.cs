using UnityEngine.UI;

namespace UnityEngine.UI.Translation
{
    [BaseTypeOf("UnityEngine.UI.InputField","UnityEngine.UI.dll")]
    public class InputFieldOverride : Selectable
    {
        public void SetPlaceholder(Graphic value)
        {
            if (base.GetType() == typeof(InputField))
            {
                Text text = value as Text;
                if (text != null)
                {
                    text.Translate = false;
                }
            }
        }

        public void SetTextComponent(Text value)
        {
            if ((base.GetType() == typeof(InputField)) && (value != null))
            {
                value.Translate = false;
            }
        }

        protected override void Start()
        {
            base.Start();
            if (base.GetType() == typeof(InputField))
            {
                InputField field1 = this as InputField;
                field1.placeholder = field1.placeholder;
                field1.textComponent = field1.textComponent;
            }
        }
    }
}

