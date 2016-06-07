namespace xr
{
    public sealed class XRayLineColorProvider : ILineColorProvider
    {
        public uint GetLineColor(string line, out int hiddenCharCount)
        {
            if (line.Length == 0)
            {
                hiddenCharCount = 0;
                return ConsoleColors.White;
            }
            hiddenCharCount = 2;
            return GetColorByChar(line[0]);
        }

        private static uint GetColorByChar(char c)
        {
            uint result;
            switch (c)
            {
            case '!': //0x21: //'!'
                result = ConsoleColors.Red;
                break;
            case '#': //0x23: //'#'
                result = ConsoleColors.Cyan;
                break;
            case '$': //0x24: //'$'
                result = ConsoleColors.Magneta;
                break;
            case '%': //0x25: //'%'
                result = ConsoleColors.DarkMagneta;
                break;
            case '&': //0x26: //'&'
                result = ConsoleColors.Yellow;
                break;
            case '*': //0x2a: //'*'
                result = ConsoleColors.DarkGray;
                break;
            case '+': //0x2b: //'+'
                result = ConsoleColors.LightCyan;
                break;
            case '-': //0x2d: //'-'
                result = ConsoleColors.Lime;
                break;
            case '/': //0x2f: //'/'
                result = ConsoleColors.DarkBlue;
                break;
            case '=': //0x3d: //'='
                result = ConsoleColors.LightYellow;
                break;
            case '@': //0x40: //'@'
                result = ConsoleColors.Blue;
                break;
            case '^': //0x5e: //'^'
                result = ConsoleColors.DarkGreen;
                break;
            case '~': //0x7e: //'~':
                result = ConsoleColors.DarkYellow;
                break;
            default:
                result = ConsoleColors.White;
                break;
            }
            return result;
        }
    }
}
