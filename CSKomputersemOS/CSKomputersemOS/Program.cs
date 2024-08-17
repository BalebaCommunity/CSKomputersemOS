using CSKomputersemOS;
using System.Windows.Forms;
using System;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        BiosForm biosForm = new BiosForm();
        biosForm.ShowDialog();

        if (biosForm.DialogResult == DialogResult.OK)
        {
            Application.Run(new MainForm());
        }
    }
}