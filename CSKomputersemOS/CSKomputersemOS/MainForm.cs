using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Linq;

namespace CSKomputersemOS
{
    public partial class MainForm : Form
    {
        private string DesktopPath;
        private string BackgroundImagePath;
        private Form startMenu;
        private Panel desktopPanel;
        private Point dragStartPosition;
        private Control draggedControl;

        public MainForm()
        {
            InitializeComponent();
            LoadConfiguration();
            InitializeDesktop();
        }

        private void LoadConfiguration()
        {
            string configPath = Path.Combine(Application.StartupPath, "config.txt");
            if (!File.Exists(configPath))
            {
                using (ConfigForm configForm = new ConfigForm())
                {
                    if (configForm.ShowDialog() == DialogResult.OK)
                    {
                        string[] config = File.ReadAllLines(configPath);
                        DesktopPath = config[0];
                        BackgroundImagePath = config[1];
                    }
                    else
                    {
                        MessageBox.Show("Configuration is required to run the application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Exit();
                    }
                }
            }
            else
            {
                string[] config = File.ReadAllLines(configPath);
                DesktopPath = config[0];
                BackgroundImagePath = config[1];
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Name = "MainForm";
            this.Text = "CSKomputersemOS Desktop";
            this.ResumeLayout(false);
        }

        private void InitializeDesktop()
        {
            this.Size = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Set background image
            try
            {
                this.BackgroundImage = Image.FromFile(BackgroundImagePath);
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading background image: {ex.Message}");
                this.BackColor = Color.LightSkyBlue; // Fallback color
            }

            desktopPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            desktopPanel.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(desktopPanel, true, null);
            this.Controls.Add(desktopPanel);

            // Create desktop icons
            CreateDesktopIcons(desktopPanel);

            // Add context menu to desktop
            ContextMenuStrip desktopContextMenu = new ContextMenuStrip();
            desktopContextMenu.Items.Add("New Text File", null, CreateNewTextFile);
            desktopContextMenu.Items.Add("New Folder", null, CreateNewFolder);
            desktopContextMenu.Items.Add("Import File", null, ImportFile);
            desktopContextMenu.Items.Add("Change Background", null, (sender, e) => ChangeBackground());
            desktopPanel.ContextMenuStrip = desktopContextMenu;

            // Add a semi-transparent taskbar
            Panel taskbar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(180, 200, 200, 200) // Semi-transparent gray
            };
            this.Controls.Add(taskbar);

            // Add a start button
            Button startButton = new Button
            {
                Text = "Start",
                Location = new Point(10, 5),
                Size = new Size(75, 30),
                FlatStyle = FlatStyle.Flat
            };
            startButton.FlatAppearance.BorderSize = 0;
            startButton.Click += StartButton_Click;
            taskbar.Controls.Add(startButton);

            // Add a clock
            Timer clockTimer = new Timer { Interval = 1000 };
            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Start();

            Label clockLabel = new Label
            {
                AutoSize = true,
                Location = new Point(taskbar.Width - 100, 10),
                Font = new Font("Arial", 12),
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };
            taskbar.Controls.Add(clockLabel);
        }

        private void CreateDesktopIcons(Panel desktopPanel)
        {
            if (!Directory.Exists(DesktopPath))
            {
                MessageBox.Show($"Desktop directory not found: {DesktopPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string[] files = Directory.GetFiles(DesktopPath);
            string[] folders = Directory.GetDirectories(DesktopPath);
            int x = 20, y = 20;

            // Create icons for folders
            foreach (string folder in folders)
            {
                CreateIcon(desktopPanel, folder, true, ref x, ref y);
            }

            // Create icons for files
            foreach (string file in files)
            {
                CreateIcon(desktopPanel, file, false, ref x, ref y);
            }
            InitializeDragging();
        }

        private void CreateIcon(Panel desktopPanel, string path, bool isFolder, ref int x, ref int y)
        {
            Icon iconToUse;
            if (isFolder)
            {
                iconToUse = GetFolderIcon(false);
            }
            else
            {
                iconToUse = Icon.ExtractAssociatedIcon(path) ?? GetFileIcon(path, false);
            }

            PictureBox icon = new PictureBox
            {
                Image = iconToUse.ToBitmap(),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(32, 32),
                Location = new Point(x, y),
                BackColor = Color.Transparent
            };

            Label label = new Label
            {
                Text = Path.GetFileName(path),
                AutoSize = true,
                Location = new Point(x, y + 35),
                MaximumSize = new Size(64, 0),
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };

            desktopPanel.Controls.Add(icon);
            desktopPanel.Controls.Add(label);

            y += 70;
            if (y > desktopPanel.Height - 100)
            {
                y = 20;
                x += 80;
            }
        }

        private Icon GetFolderIcon(bool largeIcon)
        {
            return GetFileIcon("folder", largeIcon);
        }

        private Icon GetFileIcon(string name, bool largeIcon)
        {
            SHFILEINFO shfi = new SHFILEINFO();
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;

            if (largeIcon)
                flags |= SHGFI_LARGEICON;
            else
                flags |= SHGFI_SMALLICON;

            SHGetFileInfo(name,
                FILE_ATTRIBUTE_NORMAL,
                ref shfi,
                (uint)Marshal.SizeOf(shfi),
                flags);

            Icon icon = Icon.FromHandle(shfi.hIcon);
            return icon;
        }

        private void InitializeDragging()
        {
            foreach (Control control in desktopPanel.Controls)
            {
                if (control is PictureBox || control is Label)
                {
                    control.MouseDown += Control_MouseDown;
                    control.MouseMove += Control_MouseMove;
                    control.MouseUp += Control_MouseUp;
                }
            }
        }

        private bool isDragging = false;
        private const int MinDragDistance = 5;

        private void Control_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                draggedControl = (Control)sender;
                dragStartPosition = e.Location;
                isDragging = false;
            }
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggedControl != null)
            {
                int deltaX = e.X - dragStartPosition.X;
                int deltaY = e.Y - dragStartPosition.Y;

                if (!isDragging && (Math.Abs(deltaX) > MinDragDistance || Math.Abs(deltaY) > MinDragDistance))
                {
                    isDragging = true;
                }

                if (isDragging)
                {
                    Point newLocation = new Point(
                        draggedControl.Left + deltaX,
                        draggedControl.Top + deltaY
                    );

                    Control associatedControl = null;

                    if (draggedControl is PictureBox)
                    {
                        associatedControl = desktopPanel.Controls.OfType<Label>()
                            .FirstOrDefault(l => l.Location.X == draggedControl.Location.X && l.Location.Y == draggedControl.Location.Y + 35);
                    }
                    else if (draggedControl is Label)
                    {
                        associatedControl = desktopPanel.Controls.OfType<PictureBox>()
                            .FirstOrDefault(p => p.Location.X == draggedControl.Location.X && p.Location.Y == draggedControl.Location.Y - 35);
                    }

                    draggedControl.Location = newLocation;

                    if (associatedControl != null)
                    {
                        if (draggedControl is PictureBox)
                        {
                            associatedControl.Location = new Point(newLocation.X, newLocation.Y + 35);
                        }
                        else
                        {
                            associatedControl.Location = new Point(newLocation.X, newLocation.Y - 35);
                        }
                    }

                    desktopPanel.Invalidate();
                }
            }
        }

        private void Control_MouseUp(object sender, MouseEventArgs e)
        {
            if (draggedControl != null)
            {
                if (!isDragging)
                {
                    // Handle click event (open file or folder)
                    string path = GetAssociatedPath(draggedControl);
                    if (Directory.Exists(path))
                    {
                        System.Diagnostics.Process.Start("explorer.exe", path);
                    }
                    else if (File.Exists(path))
                    {
                        System.Diagnostics.Process.Start(path);
                    }
                }
                draggedControl = null;
                isDragging = false;
            }
        }

        private string GetAssociatedPath(Control control)
        {
            string fileName;
            if (control is PictureBox)
            {
                Label associatedLabel = desktopPanel.Controls.OfType<Label>()
                    .FirstOrDefault(l => l.Location.X == control.Location.X && l.Location.Y == control.Location.Y + 35);
                fileName = associatedLabel?.Text;
            }
            else if (control is Label)
            {
                fileName = control.Text;
            }
            else
            {
                return null;
            }

            return Path.Combine(DesktopPath, fileName);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x1;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x10;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (startMenu == null || startMenu.IsDisposed)
            {
                CreateStartMenu();
            }
            else
            {
                startMenu.Close();
                startMenu = null;
            }
        }

        private void CreateStartMenu()
        {
            startMenu = new Form
            {
                Size = new Size(200, 300),
                StartPosition = FormStartPosition.Manual,
                Location = new Point(this.Location.X + 10, this.Location.Y + this.Height - 340),
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            FlowLayoutPanel menuPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };

            AddMenuButton(menuPanel, "My Computer", OpenMyComputer);
            AddMenuButton(menuPanel, "Notepad", OpenNotepad);
            AddMenuButton(menuPanel, "Calculator", OpenCalculator);
            AddMenuButton(menuPanel, "Shut Down", ShutDown);
            AddMenuButton(menuPanel, "Image Viewer", OpenImageViewer);

            startMenu.Controls.Add(menuPanel);
            startMenu.Show(this);
        }

        private void AddMenuButton(FlowLayoutPanel panel, string text, EventHandler clickEvent)
        {
            Button button = new Button
            {
                Text = text,
                Size = new Size(180, 40),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            button.Click += clickEvent;
            panel.Controls.Add(button);
        }

        private void OpenMyComputer(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe");
        }

        private void OpenNotepad(object sender, EventArgs e)
        {
            Notepad notepad = new Notepad();
            notepad.Show();
        }

        private void OpenImageViewer(object sender, EventArgs e)
        {
            ImageViewer imageViewer = new ImageViewer();
            imageViewer.Show();
        }

        private void OpenCalculator(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("calc.exe");
        }

        private void ShutDown(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            Panel taskbar = (Panel)this.Controls[this.Controls.Count - 1];
            Label clockLabel = (Label)taskbar.Controls[taskbar.Controls.Count - 1];
            clockLabel.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void CreateNewTextFile(object sender, EventArgs e)
        {
            string fileName = "New Text File.txt";
            string filePath = Path.Combine(DesktopPath, fileName);
            int count = 1;

            while (File.Exists(filePath))
            {
                fileName = $"New Text File ({count}).txt";
                filePath = Path.Combine(DesktopPath, fileName);
                count++;
            }

            File.WriteAllText(filePath, "");
            RefreshDesktop();
        }

        private void CreateNewFolder(object sender, EventArgs e)
        {
            string folderName = "New Folder";
            string folderPath = Path.Combine(DesktopPath, folderName);
            int count = 1;

            while (Directory.Exists(folderPath))
            {
                folderName = $"New Folder ({count})";
                folderPath = Path.Combine(DesktopPath, folderName);
                count++;
            }

            try
            {
                Directory.CreateDirectory(folderPath);
                RefreshDesktop();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating folder: {ex.Message}", "Folder Creation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImportFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select a file to import";
            openFileDialog.Filter = "All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string sourceFile = openFileDialog.FileName;
                string destFile = Path.Combine(DesktopPath, Path.GetFileName(sourceFile));

                try
                {
                    File.Copy(sourceFile, destFile, true);
                    RefreshDesktop();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing file: {ex.Message}", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void RefreshDesktop()
        {
            desktopPanel.Controls.Clear();
            CreateDesktopIcons(desktopPanel);
        }

        private void ChangeBackground()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select a new background image";
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Image newBackground = Image.FromFile(openFileDialog.FileName);
                    this.BackgroundImage = newBackground;
                    this.BackgroundImageLayout = ImageLayout.Stretch;

                    // Update the configuration file with the new background path
                    BackgroundImagePath = openFileDialog.FileName;
                    UpdateConfigFile();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading new background image: {ex.Message}", "Background Change Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateConfigFile()
        {
            string configPath = Path.Combine(Application.StartupPath, "config.txt");
            File.WriteAllLines(configPath, new[]
            {
                DesktopPath,
                BackgroundImagePath
            });
        }
    }
}
