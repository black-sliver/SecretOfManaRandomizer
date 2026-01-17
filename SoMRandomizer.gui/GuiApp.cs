using System.Windows.Forms;
using SoMRandomizer.gui.forms;

namespace SoMRandomizer.gui
{
    internal static class GuiApp
    {
        internal static void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
