using System.Windows.Forms;

namespace XrConsoleProject
{
    public partial class ConsoleWindow : Form
    {
        public ConsoleWindow()
        {
            InitializeComponent();
        }

        public XrConsole Console
        {
            get { return xrConsole; }
        }
    }
}
