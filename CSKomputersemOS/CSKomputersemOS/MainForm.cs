using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;
using Button = System.Windows.Forms.Button;
using TextBox = System.Windows.Forms.TextBox;
using System.Threading.Tasks;
using System.Text;

namespace CSKomputersemOS
{
    public partial class MainForm : Form
    {
        // Fields
        private Dictionary<string, Point> iconPositions = new Dictionary<string, Point>();
        private string[] allowedExtensions = {
    // Text and document files
    ".txt", ".rtf", ".doc", ".docx", ".pdf", ".odt", ".md", ".csv",
    
    // Image files
    ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".svg", ".webp",
    
    // Audio files
    ".mp3", ".wav", ".ogg", ".flac", ".aac", ".wma",
    
    // Video files
    ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm",
    
    // Compressed files
    ".zip", ".rar", ".7z", ".tar", ".gz",
    
    // Programming and script files
    ".cs", ".java", ".py", ".js", ".html", ".css", ".php", ".xml", ".json",
    
    // Spreadsheet files
    ".xls", ".xlsx", ".ods",
    
    // Presentation files
    ".ppt", ".pptx", ".odp",
    
    // Executable files (use with caution)
    ".exe", ".bat", ".sh",
    
    // Other common file types
    ".iso", ".torrent", ".db", ".sql", ".lnk"
};
        private string DesktopPath;
        private string BackgroundImagePath;
        private Form startMenu;
        private Panel desktopPanel;
        private Point dragStartPosition;
        private Control draggedControl;
        private Label dateLabel;
        private Panel taskbar;
        private Label clockLabel;
        private Timer dragTimer;
        private Point lastMousePosition;
        private Control lastClickedControl;
        private DateTime lastClickTime;
        private const int DoubleClickTime = 500; // milliseconds
        private PictureBox startButton;
        private Label startLabel;
        private Form welcomeScreen;

        // Constructor
        public MainForm()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            InitializeComponent();
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.FormClosing += MainForm_FormClosing;
            this.Resize += MainForm_Resize;
            LoadConfiguration();
            ShowWelcomeScreen();
        }

        private async void ShowWelcomeScreen()
        {
            welcomeScreen = new Form
            {
                Size = new Size(400, 300),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(0, 120, 215)
            };

            Label welcomeLabel = new Label
            {
                Text = "Welcome to CSKomputersemOS",
                Font = new Font("Arial", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            welcomeLabel.Location = new Point((welcomeScreen.Width - welcomeLabel.Width) / 2, 100);

            PictureBox logo = new PictureBox
            {
                Size = new Size(100, 100),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point((welcomeScreen.Width - 100) / 2, 20)
            };

            string logoPath = System.IO.Path.Combine(Application.StartupPath, "UI", "logo.png");
            if (System.IO.File.Exists(logoPath))
            {
                logo.Image = Image.FromFile(logoPath);
            }

            ProgressBar progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Size = new Size(300, 20),
                Location = new Point((welcomeScreen.Width - 300) / 2, 200)
            };

            welcomeScreen.Controls.Add(welcomeLabel);
            welcomeScreen.Controls.Add(logo);
            welcomeScreen.Controls.Add(progressBar);

            welcomeScreen.Show();
            this.Hide();

            await Task.Delay(3000); // Показываем экран приветствия на 3 секунды

            welcomeScreen.Close();
            this.Show();
            InitializeDesktop();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            UpdateClockPosition();
        }

        private void DesktopPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.All(file => string.IsNullOrEmpty(Path.GetExtension(file)) || allowedExtensions.Contains(Path.GetExtension(file).ToLower())))
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void DesktopPanel_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                if (allowedExtensions.Contains(Path.GetExtension(file).ToLower()))
                {
                    string destFile = Path.Combine(DesktopPath, Path.GetFileName(file));
                    try
                    {
                        File.Copy(file, destFile, false);
                    }
                    catch (IOException)
                    {
                        // If file already exists, create a copy with a number appended
                        int count = 1;
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(destFile);
                        string fileExt = Path.GetExtension(destFile);
                        while (File.Exists(destFile))
                        {
                            destFile = Path.Combine(DesktopPath, $"{fileNameWithoutExt} ({count}){fileExt}");
                            count++;
                        }
                        File.Copy(file, destFile);
                    }
                }
            }
            RefreshDesktop();
        }

        // Methods
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                SaveIconPositions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving icon positions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                        BackgroundImagePath = Path.Combine(Application.StartupPath, "UI", "cskos_bg.png"); // Предполагаемое имя файла
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
                BackgroundImagePath = Path.Combine(Application.StartupPath, "UI", "cskos_bg.png"); // Предполагаемое имя файла

                // Load icon positions
                iconPositions.Clear(); // Clear existing positions before loading
                foreach (string line in config)
                {
                    if (line.StartsWith("IconPosition:"))
                    {
                        string[] parts = line.Substring(13).Split('|');
                        if (parts.Length == 3)
                        {
                            string fileName = parts[0];
                            int x = int.Parse(parts[1]);
                            int y = int.Parse(parts[2]);
                            iconPositions[fileName] = new Point(x, y);
                        }
                    }
                }
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
                string backgroundPath = Path.Combine(Application.StartupPath, "UI", "cskos_bg.png");
                if (File.Exists(backgroundPath))
                {
                    this.BackgroundImage = Image.FromFile(backgroundPath);
                    this.BackgroundImageLayout = ImageLayout.Stretch;
                }
                else
                {
                    MessageBox.Show($"Фоновое изображение не найдено по пути: {backgroundPath}", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.BackColor = Color.LightSkyBlue; // Цвет по умолчанию, если изображение не найдено
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке фонового изображения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.BackColor = Color.LightSkyBlue; // Цвет по умолчанию в случае ошибки
            }

            desktopPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            desktopPanel.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(desktopPanel, true, null);
            this.Controls.Add(desktopPanel);

            desktopPanel.AllowDrop = true;
            desktopPanel.DragEnter += DesktopPanel_DragEnter;
            desktopPanel.DragDrop += DesktopPanel_DragDrop;

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
            taskbar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(180, 200, 200, 200) // Semi-transparent gray
            };
            this.Controls.Add(taskbar);

            // Add start menu button
            startButton = new PictureBox
            {
                Size = new Size(220, 40),
                Location = new Point(4, (taskbar.Height - 40) / 2), // Центрирование по вертикали
                SizeMode = PictureBoxSizeMode.StretchImage,
                Cursor = Cursors.Hand
            };

            string imagePath = Path.Combine(Application.StartupPath, "UI", "start_button.png");

            try
            {
                if (File.Exists(imagePath))
                {
                    startButton.Image = Image.FromFile(imagePath);
                    startButton.Click += StartButton_Click;
                }
                else
                {
                    throw new FileNotFoundException("Start button image not found.", imagePath);
                }
            }
            catch (Exception ex)
            {
                // Показываем всплывающее окно с информацией об ошибке
                MessageBox.Show($"Не удалось загрузить изображение кнопки Start.\n" +
                                $"Ошибка: {ex.Message}\n" +
                                $"Путь поиска: {imagePath}\n\n" +
                                "Будет использована текстовая версия кнопки.",
                                "Ошибка загрузки изображения",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                // Если файл изображения не найден, используем Label поверх PictureBox
                startButton.BackColor = Color.Green;

                startLabel = new Label
                {
                    Text = "Start",
                    Font = new Font("Arial", 12, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };

                startButton.Controls.Add(startLabel);
                startLabel.Click += StartButton_Click;
            }

            taskbar.Controls.Add(startButton);

            // Add a clock
            Timer clockTimer = new Timer { Interval = 1000 };
            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Start();

            clockLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Arial", 12),
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };
            taskbar.Controls.Add(clockLabel);

            // Add a date label
            dateLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Arial", 10),
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };
            taskbar.Controls.Add(dateLabel);

            UpdateClockPosition();
        }

        private void StartIcon_Click(object sender, EventArgs e)
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

        private void CreateIconContextMenu(Control control)
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem openItem = new ToolStripMenuItem("Open");
            openItem.Click += (sender, e) => OpenFileOrFolder(control);
            contextMenu.Items.Add(openItem);

            ToolStripMenuItem deleteItem = new ToolStripMenuItem("Delete");
            deleteItem.Click += (sender, e) => DeleteFileOrFolder(control);
            contextMenu.Items.Add(deleteItem);

            ToolStripMenuItem renameItem = new ToolStripMenuItem("Rename");
            renameItem.Click += (sender, e) => RenameFileOrFolder(control);
            contextMenu.Items.Add(renameItem);

            control.ContextMenuStrip = contextMenu;
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
        private void UpdateClockPosition()
        {
            if (taskbar != null && clockLabel != null && dateLabel != null)
            {
                int rightMargin = 10;
                int topMargin = 5;

                clockLabel.Location = new Point(taskbar.Width - clockLabel.Width - rightMargin, topMargin);
                dateLabel.Location = new Point(taskbar.Width - dateLabel.Width - rightMargin, clockLabel.Bottom + 2);
            }
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
                BackColor = Color.Transparent
            };

            Label label = new Label
            {
                Text = Path.GetFileName(path),
                AutoSize = true,
                MaximumSize = new Size(64, 0),
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };

            string fileName = Path.GetFileName(path);
            if (iconPositions.TryGetValue(fileName, out Point savedPosition))
            {
                icon.Location = savedPosition;
                label.Location = new Point(savedPosition.X, savedPosition.Y + 35);
            }
            else
            {
                icon.Location = new Point(x, y);
                label.Location = new Point(x, y + 35);

                y += 70;
                if (y > desktopPanel.Height - 100)
                {
                    y = 20;
                    x += 80;
                }
            }

            icon.MouseDown += Control_MouseDown;
            icon.MouseUp += Control_MouseUp;
            CreateIconContextMenu(icon);

            label.MouseDown += Control_MouseDown;
            label.MouseUp += Control_MouseUp;
            CreateIconContextMenu(label);

            desktopPanel.Controls.Add(icon);
            desktopPanel.Controls.Add(label);

            // Добавляем проверку на кириллицу и логирование
            if (ContainsCyrillic(fileName))
            {
                Console.WriteLine($"Created icon for file with Cyrillic name: {fileName}");
                Console.WriteLine($"Icon location: {icon.Location}, Label location: {label.Location}");
            }
        }

        // Вспомогательный метод для проверки наличия кириллицы в строке
        private bool ContainsCyrillic(string text)
        {
            return text.Any(c => (c >= 'А' && c <= 'я') || c == 'Ё' || c == 'ё');
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

        private void SaveIconPositions()
        {
            if (desktopPanel == null || desktopPanel.Controls == null)
            {
                return;
            }

            List<string> iconPositions = new List<string>();
            foreach (Control control in desktopPanel.Controls)
            {
                if (control is PictureBox)
                {
                    Label label = GetAssociatedLabel(control);
                    if (label != null)
                    {
                        string fileName = label.Text;
                        iconPositions.Add($"{fileName}|{control.Location.X}|{control.Location.Y}");
                    }
                }
            }

            string configPath = Path.Combine(Application.StartupPath, "config.txt");
            if (File.Exists(configPath))
            {
                List<string> configLines = File.ReadAllLines(configPath).ToList();

                // Remove existing icon positions
                configLines.RemoveAll(line => line.StartsWith("IconPosition:"));

                // Add new icon positions
                foreach (string position in iconPositions)
                {
                    configLines.Add($"IconPosition:{position}");
                }

                File.WriteAllLines(configPath, configLines);
            }
        }

        private void DeleteFileOrFolder(Control control)
        {
            string path = GetAssociatedPath(control);
            if (File.Exists(path))
            {
                if (MessageBox.Show($"Are you sure you want to delete {Path.GetFileName(path)}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    File.Delete(path);
                    RefreshDesktop();
                }
            }
            else if (Directory.Exists(path))
            {
                if (MessageBox.Show($"Are you sure you want to delete the folder {Path.GetFileName(path)} and all its contents?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    Directory.Delete(path, true);
                    RefreshDesktop();
                }
            }
        }

        private void RenameFileOrFolder(Control control)
        {
            string path = GetAssociatedPath(control);
            string oldName = Path.GetFileName(path);

            using (var renameForm = new Form())
            {
                renameForm.Text = "Rename";
                renameForm.Size = new Size(300, 150);
                renameForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                renameForm.StartPosition = FormStartPosition.CenterParent;

                var textBox = new TextBox { Text = oldName, Location = new Point(10, 10), Width = 260 };
                var okButton = new Button { Text = "OK", Location = new Point(10, 40), DialogResult = DialogResult.OK };
                var cancelButton = new Button { Text = "Cancel", Location = new Point(100, 40), DialogResult = DialogResult.Cancel };

                renameForm.Controls.AddRange(new Control[] { textBox, okButton, cancelButton });
                renameForm.AcceptButton = okButton;
                renameForm.CancelButton = cancelButton;

                if (renameForm.ShowDialog() == DialogResult.OK)
                {
                    string newName = textBox.Text.Trim();
                    if (!string.IsNullOrEmpty(newName) && newName != oldName)
                    {
                        string newPath = Path.Combine(Path.GetDirectoryName(path), newName);
                        if (File.Exists(path))
                        {
                            File.Move(path, newPath);
                        }
                        else if (Directory.Exists(path))
                        {
                            Directory.Move(path, newPath);
                        }
                        RefreshDesktop();
                    }
                }
            }
        }

        private Label GetAssociatedLabel(Control control)
        {
            if (control is PictureBox && desktopPanel != null)
            {
                return desktopPanel.Controls.OfType<Label>()
                    .FirstOrDefault(l => l.Location.X == control.Location.X && l.Location.Y == control.Location.Y + 35);
            }
            else if (control is Label label)
            {
                return label;
            }
            return null;
        }

        private string GetAssociatedPath(Control control)
        {
            Label label = GetAssociatedLabel(control);
            if (label != null)
            {
                return Path.Combine(DesktopPath, label.Text);
            }
            return null;
        }


        private void InitializeDragging()
        {
            dragTimer = new Timer { Interval = 16 }; // ~60 FPS
            dragTimer.Tick += DragTimer_Tick;

            foreach (Control control in desktopPanel.Controls)
            {
                if (control is PictureBox || control is Label)
                {
                    control.MouseDown += Control_MouseDown;
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
                lastMousePosition = desktopPanel.PointToClient(Control.MousePosition);
                isDragging = false;
                dragTimer.Start();
            }
        }

        private void DragTimer_Tick(object sender, EventArgs e)
        {
            if (draggedControl != null)
            {
                Point currentMousePosition = desktopPanel.PointToClient(Control.MousePosition);
                int deltaX = currentMousePosition.X - lastMousePosition.X;
                int deltaY = currentMousePosition.Y - lastMousePosition.Y;

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

                    Control associatedControl = GetAssociatedControl(draggedControl);

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
                    else
                    {
                        Console.WriteLine($"Associated control not found for dragged control at position {newLocation}");
                        // Здесь вы можете создать новую метку, если она не существует
                        if (draggedControl is PictureBox)
                        {
                            string fileName = Path.GetFileName(GetAssociatedPath(draggedControl));
                            Label newLabel = new Label
                            {
                                Text = fileName,
                                AutoSize = true,
                                MaximumSize = new Size(64, 0),
                                BackColor = Color.Transparent,
                                ForeColor = Color.White,
                                Location = new Point(newLocation.X, newLocation.Y + 35)
                            };
                            desktopPanel.Controls.Add(newLabel);
                            CreateIconContextMenu(newLabel);
                            newLabel.MouseDown += Control_MouseDown;
                            newLabel.MouseUp += Control_MouseUp;
                        }
                    }

                    desktopPanel.Invalidate(new Rectangle(
                                    Math.Min(draggedControl.Left, newLocation.X) - 1,
                                    Math.Min(draggedControl.Top, newLocation.Y) - 1,
                                    Math.Abs(deltaX) + draggedControl.Width + 2,
                                    Math.Abs(deltaY) + draggedControl.Height + 2));
                }

                lastMousePosition = currentMousePosition;
            }
        }

        private void Control_MouseUp(object sender, MouseEventArgs e)
        {
            if (draggedControl != null)
            {
                dragTimer.Stop();
                if (!isDragging)
                {
                    // Handle click event
                    Control clickedControl = (Control)sender;
                    DateTime currentClickTime = DateTime.Now;

                    if (clickedControl == lastClickedControl &&
                        (currentClickTime - lastClickTime).TotalMilliseconds <= DoubleClickTime)
                    {
                        // Double click detected
                        OpenFileOrFolder(clickedControl);
                        HighlightIcon(clickedControl, false); // Unhighlight after opening
                    }
                    else
                    {
                        // Single click
                        HighlightIcon(lastClickedControl, false); // Unhighlight previous
                        HighlightIcon(clickedControl, true); // Highlight current
                    }

                    lastClickedControl = clickedControl;
                    lastClickTime = currentClickTime;
                }
                draggedControl = null;
                isDragging = false;
            }
        }

        private void HighlightIcon(Control control, bool highlight)
        {
            if (control != null)
            {
                if (control is PictureBox pictureBox)
                {
                    pictureBox.BackColor = highlight ? Color.LightBlue : Color.Transparent;
                }
                else if (control is Label label)
                {
                    label.BackColor = highlight ? Color.LightBlue : Color.Transparent;
                }
            }
        }

        private void OpenFileOrFolder(Control control)
        {
            string path = GetAssociatedPath(control);
            if (Directory.Exists(path))
            {
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
            else if (File.Exists(path))
            {
                try
                {
                    System.Diagnostics.Process.Start(path);
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    // If the file doesn't have an associated program, open it with Notepad
                    System.Diagnostics.Process.Start("notepad.exe", path);
                }
            }
        }

        private Control GetAssociatedControl(Control control)
        {
            if (control is PictureBox)
            {
                return desktopPanel.Controls.OfType<Label>()
                    .FirstOrDefault(l => l.Location.X == control.Location.X && l.Location.Y == control.Location.Y + 35);
            }
            else if (control is Label)
            {
                return desktopPanel.Controls.OfType<PictureBox>()
                    .FirstOrDefault(p => p.Location.X == control.Location.X && p.Location.Y == control.Location.Y - 35);
            }
            return null;
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
            if (clockLabel != null && dateLabel != null)
            {
                clockLabel.Text = DateTime.Now.ToString("HH:mm:ss");
                dateLabel.Text = DateTime.Now.ToString("dddd, MMMM d, yyyy");
                UpdateClockPosition();
            }
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
            Dictionary<string, Point> currentPositions = new Dictionary<string, Point>();
            foreach (Control control in desktopPanel.Controls)
            {
                if (control is PictureBox)
                {
                    Label associatedLabel = GetAssociatedLabel(control);
                    if (associatedLabel != null)
                    {
                        string fileName = associatedLabel.Text;
                        currentPositions[fileName] = control.Location;
                    }
                }
            }

            desktopPanel.Controls.Clear();
            CreateDesktopIcons(desktopPanel);

            // Restore positions for existing icons
            foreach (Control control in desktopPanel.Controls)
            {
                if (control is PictureBox)
                {
                    Label associatedLabel = GetAssociatedLabel(control);
                    if (associatedLabel != null)
                    {
                        string fileName = associatedLabel.Text;
                        if (currentPositions.ContainsKey(fileName))
                        {
                            control.Location = currentPositions[fileName];
                            associatedLabel.Location = new Point(control.Location.X, control.Location.Y + 35);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Associated label not found for control at position {control.Location}");
                        // Здесь вы можете создать новую метку, если она не существует
                        string fileName = Path.GetFileName(GetAssociatedPath(control));
                        Label newLabel = new Label
                        {
                            Text = fileName,
                            AutoSize = true,
                            MaximumSize = new Size(64, 0),
                            BackColor = Color.Transparent,
                            ForeColor = Color.White,
                            Location = new Point(control.Location.X, control.Location.Y + 35)
                        };
                        desktopPanel.Controls.Add(newLabel);
                        CreateIconContextMenu(newLabel);
                        newLabel.MouseDown += Control_MouseDown;
                        newLabel.MouseUp += Control_MouseUp;
                    }
                }
            }

            // Обновляем позиции иконок в конфигурационном файле
            SaveIconPositions();
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