using System.Linq;
using System.Windows.Forms;

namespace DotnetRuntimeBootstrapper.Demo
{
    public partial class MainForm : Form
    {
        public MainForm(string[] commandLineArgs)
        {
            InitializeComponent();

            // Show command line arguments
            if (commandLineArgs.Any())
                MainLabel.Text = string.Join(" ", commandLineArgs);
        }
    }
}