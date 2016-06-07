using System.Windows.Forms;

namespace xr
{
    public partial class ConsoleWindow : Form
    {
        public ConsoleWindow()
        {
            InitializeComponent();
        }

        public Console Console
        {
            get { return xrConsole; }
        }
    }
}
