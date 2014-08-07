using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using xr.ConsoleCommands;

namespace xr.Example
{
    static class Program
    {
        private static Console console;
        private static ILogger logger;
        private static ConsoleWindow consoleWnd;

        [STAThread]
        private static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
            var logFileName = String.Format("xrConsole_{0}.log", Environment.UserName);
            logger = new PlainLogger(logFileName);
            consoleWnd = new ConsoleWindow();
            console = consoleWnd.Console;
            console.AttachLogger(logger);
            console.AddCommand(new Func(console, "quit", Console_Quit));
            console.AddCommand(new Func(console, "clear_log", Console_ClearLog, "Clear log"));
            console.AddCommand(new StringVar(console, "font_face",
                new Accessor<string>(Console_GetFontFace, Console_SetFontFace), 255, "Console font face"));
            console.AddCommand(new FloatVar(console, "font_size",
                new Accessor<float>(Console_GetFontSize, Console_SetFontSize), 5.0f, 20.0f, "Console font size"));
            consoleWnd.ShowDialog();
            if (logger != null)
            {
                logger.Dispose();
            }
        }

        private static void Console_Quit()
        {
            Action callback = () => consoleWnd.Close();
            if (consoleWnd.InvokeRequired)
            {
                consoleWnd.Invoke(callback);
            }
            else
            {
                callback();
            }
        }

        private static void Console_ClearLog()
        {
            if (logger != null)
            {
                logger.Clear();
            }
            Msg("* Log file has been successfully cleaned");
        }

        private static string Console_GetFontFace()
        {
            return console.Font.Name;
        }

        private static void Console_SetFontFace(string fontFace)
        {
            if (!IsFontInstalled(fontFace))
            {
                Msg("! Font not found: '{0}'", fontFace);
                return;
            }
            var prevFont = console.Font;
            console.InvokeAsync(() => console.Font = new Font(fontFace, prevFont.Size, prevFont.Style));
        }

        private static float Console_GetFontSize()
        {
            return console.Font.Size;
        }

        private static void Console_SetFontSize(float size)
        {
            var prevFont = console.Font;
            console.InvokeAsync(() => console.Font = new Font(prevFont.Name, size, prevFont.Style));
        }

        private static void Msg(string msg)
        {
            if (logger != null)
            {
                logger.Log(msg);
            }
        }

        private static void Msg(string msg, params object[] args)
        {
            if (logger != null)
            {
                logger.Log(String.Format(msg, args));
            }
        }

        private static bool IsFontInstalled(string name)
        {
            using (var fonts = new InstalledFontCollection())
            {
                return fonts.Families.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            }
        }
    }
}
