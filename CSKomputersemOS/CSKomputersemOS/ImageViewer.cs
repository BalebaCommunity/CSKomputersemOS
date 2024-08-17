using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CSKomputersemOS
{
    public class ImageViewer : Form
    {
        private PictureBox pictureBox;
        private MenuStrip menuStrip;
        private string currentImagePath;
        private string[] imageFiles;
        private int currentImageIndex;
        private float zoomFactor = 1.0f;

        public ImageViewer()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "CSKomputersemOS Image Viewer";
            this.Size = new Size(800, 600);

            // Create PictureBox
            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            // Create MenuStrip
            menuStrip = new MenuStrip();
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("Open", null, OpenImage);
            fileMenu.DropDownItems.Add("Exit", null, Exit);

            ToolStripMenuItem viewMenu = new ToolStripMenuItem("View");
            viewMenu.DropDownItems.Add("Zoom In", null, ZoomIn);
            viewMenu.DropDownItems.Add("Zoom Out", null, ZoomOut);
            viewMenu.DropDownItems.Add("Reset Zoom", null, ResetZoom);

            ToolStripMenuItem navigationMenu = new ToolStripMenuItem("Navigation");
            navigationMenu.DropDownItems.Add("Previous Image", null, PreviousImage);
            navigationMenu.DropDownItems.Add("Next Image", null, NextImage);

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(viewMenu);
            menuStrip.Items.Add(navigationMenu);

            // Add controls to form
            this.Controls.Add(pictureBox);
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;

            // Add key event handlers
            this.KeyPreview = true;
            this.KeyDown += ImageViewer_KeyDown;
        }

        private void OpenImage(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadImage(openFileDialog.FileName);
                LoadImagesInFolder(Path.GetDirectoryName(openFileDialog.FileName));
            }
        }

        private void LoadImage(string imagePath)
        {
            currentImagePath = imagePath;
            pictureBox.Image = Image.FromFile(imagePath);
            this.Text = $"CSKomputersemOS Image Viewer - {Path.GetFileName(imagePath)}";
            zoomFactor = 1.0f;
            ApplyZoom();
        }

        private void LoadImagesInFolder(string folderPath)
        {
            string[] supportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };
            imageFiles = Directory.GetFiles(folderPath)
                .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
                .ToArray();
            currentImageIndex = Array.IndexOf(imageFiles, currentImagePath);
        }

        private void ZoomIn(object sender, EventArgs e)
        {
            zoomFactor *= 1.2f;
            ApplyZoom();
        }

        private void ZoomOut(object sender, EventArgs e)
        {
            zoomFactor /= 1.2f;
            ApplyZoom();
        }

        private void ResetZoom(object sender, EventArgs e)
        {
            zoomFactor = 1.0f;
            ApplyZoom();
        }

        private void ApplyZoom()
        {
            if (pictureBox.Image != null)
            {
                pictureBox.SizeMode = PictureBoxSizeMode.Normal;
                pictureBox.Size = new Size(
                    (int)(pictureBox.Image.Width * zoomFactor),
                    (int)(pictureBox.Image.Height * zoomFactor)
                );
                pictureBox.Invalidate();
            }
        }

        private void PreviousImage(object sender, EventArgs e)
        {
            if (imageFiles != null && imageFiles.Length > 0)
            {
                currentImageIndex = (currentImageIndex - 1 + imageFiles.Length) % imageFiles.Length;
                LoadImage(imageFiles[currentImageIndex]);
            }
        }

        private void NextImage(object sender, EventArgs e)
        {
            if (imageFiles != null && imageFiles.Length > 0)
            {
                currentImageIndex = (currentImageIndex + 1) % imageFiles.Length;
                LoadImage(imageFiles[currentImageIndex]);
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ImageViewer_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    PreviousImage(sender, e);
                    break;
                case Keys.Right:
                    NextImage(sender, e);
                    break;
                case Keys.Add:
                case Keys.Oemplus:
                    ZoomIn(sender, e);
                    break;
                case Keys.Subtract:
                case Keys.OemMinus:
                    ZoomOut(sender, e);
                    break;
                case Keys.D0:
                case Keys.NumPad0:
                    ResetZoom(sender, e);
                    break;
            }
        }
    }
}
