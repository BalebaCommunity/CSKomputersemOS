using System;
using System.Windows.Forms;
using System.IO;

public class ConfigForm : Form
{
    private TextBox desktopPathTextBox;
    private TextBox backgroundPathTextBox;
    private Button saveButton;

    public ConfigForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Initial Configuration";
        this.Size = new System.Drawing.Size(400, 200);
        this.StartPosition = FormStartPosition.CenterScreen;

        Label desktopLabel = new Label
        {
            Text = "Desktop Path:",
            Location = new System.Drawing.Point(20, 20),
            AutoSize = true
        };

        desktopPathTextBox = new TextBox
        {
            Location = new System.Drawing.Point(20, 40),
            Size = new System.Drawing.Size(280, 20)
        };

        Button browseDesktopButton = new Button
        {
            Text = "Browse",
            Location = new System.Drawing.Point(310, 38),
            Size = new System.Drawing.Size(60, 23)
        };
        browseDesktopButton.Click += (sender, e) => BrowseFolder(desktopPathTextBox);

        Label backgroundLabel = new Label
        {
            Text = "Background Image Path:",
            Location = new System.Drawing.Point(20, 70),
            AutoSize = true
        };

        backgroundPathTextBox = new TextBox
        {
            Location = new System.Drawing.Point(20, 90),
            Size = new System.Drawing.Size(280, 20)
        };

        Button browseBackgroundButton = new Button
        {
            Text = "Browse",
            Location = new System.Drawing.Point(310, 88),
            Size = new System.Drawing.Size(60, 23)
        };
        browseBackgroundButton.Click += (sender, e) => BrowseFile(backgroundPathTextBox);

        saveButton = new Button
        {
            Text = "Save Configuration",
            Location = new System.Drawing.Point(130, 130),
            Size = new System.Drawing.Size(120, 30)
        };
        saveButton.Click += SaveButton_Click;

        this.Controls.Add(desktopLabel);
        this.Controls.Add(desktopPathTextBox);
        this.Controls.Add(browseDesktopButton);
        this.Controls.Add(backgroundLabel);
        this.Controls.Add(backgroundPathTextBox);
        this.Controls.Add(browseBackgroundButton);
        this.Controls.Add(saveButton);
    }

    private void BrowseFolder(TextBox textBox)
    {
        using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog())
        {
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                textBox.Text = folderBrowser.SelectedPath;
            }
        }
    }

    private void BrowseFile(TextBox textBox)
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox.Text = openFileDialog.FileName;
            }
        }
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(desktopPathTextBox.Text) || string.IsNullOrWhiteSpace(backgroundPathTextBox.Text))
        {
            MessageBox.Show("Please fill in both paths.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        string configPath = Path.Combine(Application.StartupPath, "config.txt");
        File.WriteAllLines(configPath, new[]
        {
            desktopPathTextBox.Text,
            backgroundPathTextBox.Text
        });

        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}
