using System;
using System.Drawing;
using System.Windows.Forms;
using XrConsoleProject.ConsoleCommands;

namespace XrConsoleProject
{
    public static class Program
    {
        public static string UserName { get; private set; }
        public static string LogFileName { get; private set; }
        public static XrConsole Console { get; private set; }
        private static ILogger logger;
        private static ConsoleWindow consoleWnd;

        [STAThread]
        private static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
            UserName = Environment.UserName;
            LogFileName = String.Format("xrConsole_{0}.log", UserName);
            logger = new PlainLogger(LogFileName);
            consoleWnd = new ConsoleWindow();
            Console = consoleWnd.Console;
            Console.AttachLogger(logger);
            Console.AddCommand(new Func("quit", Console_Quit));
            Console.AddCommand(new Func("clear_log", Console_ClearLog, "Clear log"));
            Console.AddCommand(new StringVar("font_face",
                new Accessor<string>(Console_GetFontFace, Console_SetFontFace), 255, "Console font face"));
            Console.AddCommand(new FloatVar("font_size",
                new Accessor<float>(Console_GetFontSize, Console_SetFontSize), 5.0f, 20.0f, "Console font size"));
            ScrollHelper.RegisterSelf();
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
            return Console.Font.Name;
        }

        private static void Console_SetFontFace(string fontFace)
        {
            if (!Utils.IsFontInstalled(fontFace))
            {
                Msg("! Font not found: '{0}'", fontFace);
                return;
            }
            var prevFont = Console.Font;
            Console.InvokeAsync(() => Console.Font = new Font(fontFace, prevFont.Size, prevFont.Style));
        }

        private static float Console_GetFontSize()
        {
            return Console.Font.Size;
        }

        private static void Console_SetFontSize(float size)
        {
            var prevFont = Console.Font;
            Console.InvokeAsync(() => Console.Font = new Font(prevFont.Name, size, prevFont.Style));
        }

        public static void Msg(string msg)
        {
            if (logger != null)
            {
                logger.Log(msg);
            }
        }

        public static void Msg(string msg, params object[] args)
        {
            if (logger != null)
            {
                logger.Log(String.Format(msg, args));
            }
        }
    }
}
