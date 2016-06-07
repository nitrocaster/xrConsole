using System;

namespace xr
{
    public class ConsoleLabel
    {
        private string text;

        public ConsoleLabel()
        {
            text = String.Empty;
            Color = ConsoleColors.Default;
        }

        public string Text
        {
            get { return text; }
            set
            {
                if (value == text)
                {
                    return;
                }
                text = value;
                NeedRedraw = true;
            }
        }

        public uint Color { get; set; }

        internal bool NeedRedraw { get; set; }
    }
}
