using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CSKomputersemOS
{
    public class Notepad : Form
    {
        private TextBox textBox;
        private MenuStrip menuStrip;
        private string currentFilePath;

        public Notepad()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "CSKomputersemOS Notepad";
            this.Size = new Size(600, 400);

            // Create TextBox
            textBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical
            };

            // Create MenuStrip
            menuStrip = new MenuStrip();
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("New", null, NewFile);
            fileMenu.DropDownItems.Add("Open", null, OpenFile);
            fileMenu.DropDownItems.Add("Save", null, SaveFile);
            fileMenu.DropDownItems.Add("Save As", null, SaveFileAs);
            fileMenu.DropDownItems.Add("Exit", null, Exit);

            menuStrip.Items.Add(fileMenu);

            // Add controls to form
            this.Controls.Add(textBox);
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;
        }

        private void NewFile(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox.Text))
            {
                DialogResult result = MessageBox.Show("Do you want to save changes?", "Save", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    SaveFile(sender, e);
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }
            textBox.Clear();
            currentFilePath = null;
        }

        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                currentFilePath = openFileDialog.FileName;
                textBox.Text = File.ReadAllText(currentFilePath);
            }
        }

        private void SaveFile(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                SaveFileAs(sender, e);
            }
            else
            {
                File.WriteAllText(currentFilePath, textBox.Text);
            }
        }

        private void SaveFileAs(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                currentFilePath = saveFileDialog.FileName;
                File.WriteAllText(currentFilePath, textBox.Text);
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
