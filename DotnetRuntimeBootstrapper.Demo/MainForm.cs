using System.Drawing;
using System.Windows.Forms;

namespace DotnetRuntimeBootstrapper.Demo;

public partial class MainForm : Form
{
    public MainForm()
    {
        Icon = Icon.ExtractAssociatedIcon(typeof(MainForm).Assembly.Location);

        InitializeComponent();
    }
}