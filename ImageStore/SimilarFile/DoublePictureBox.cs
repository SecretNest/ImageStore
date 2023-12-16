using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace SecretNest.ImageStore.SimilarFile
{
    internal partial class DoublePictureBox : UserControl
    {
        Image originalImage1, originalImage2;
        Guid loadedFileId1, loadedFileId2;

        bool _autoResizePictures;
        public bool AutoResizePictures
        {
            get => _autoResizePictures;
            set
            {
                _autoResizePictures = value;
                pictureBox1.Top = 0;
                pictureBox1.Left = 0;
                pictureBox2.Top = 0;
                pictureBox2.Left = 0;
                if (_autoResizePictures)
                {
                    pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
                    pictureBox2.SizeMode = PictureBoxSizeMode.Normal;
                    pictureBox1.Size = panel1.Size;
                    pictureBox2.Size = panel2.Size;
                }
                else
                {
                    pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
                    pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;
                }
                ReloadPictures();
            }
        }

        internal DoublePictureBox()
        {
            InitializeComponent();
        }

        internal void ClearPictures()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            loadedFileId1 = Guid.Empty;
            loadedFileId2 = Guid.Empty;
            DisposeOldImage1();
            DisposeOldImage2();
        }

        void DisposeOldImage1()
        {
            if (_autoResizePictures)
            {
                if (pictureBox1.Image != null)
                {
                    var img = pictureBox1.Image;
                    pictureBox1.Image = null;
                    img.Dispose();
                    img = null;
                }
            }
            else
            {
                pictureBox1.Image = null;
            }
            if (originalImage1 != null)
            {
                originalImage1.Dispose();
                originalImage1 = null;
            }
        }

        void DisposeOldImage2()
        {
            if (_autoResizePictures)
            {
                if (pictureBox2.Image != null)
                {
                    var img = pictureBox2.Image;
                    pictureBox2.Image = null;
                    img.Dispose();
                    img = null;
                }
            }
            else
            {
                pictureBox2.Image = null;
            }
            if (originalImage2 != null)
            {
                originalImage2.Dispose();
                originalImage2 = null;
            }
        }

        internal void LoadPictures(FileInfo file1, FileInfo file2)
        {
            var needReload = false;
            if (file1.FileId != loadedFileId1)
            {
                DisposeOldImage1();

                textBox1.Text = file1.FileNameWithExtension + " (" + file1.PathToDirectory + ")";
                originalImage1 = LoadImageHelper.LoadFromFile(file1.FilePath, out bool succeeded);
                if (!succeeded)
                    textBox1.Text = "(Error) " + textBox1.Text;
                needReload = true;
            }
            if (file2.FileId != loadedFileId2)
            {
                DisposeOldImage2();

                textBox2.Text = file2.FileNameWithExtension + " (" + file2.PathToDirectory + ")";
                originalImage2 = LoadImageHelper.LoadFromFile(file2.FilePath, out bool succeeded);
                if (!succeeded)
                    textBox2.Text = "(Error) " + textBox2.Text;
                needReload = true;
            }

            if (needReload)
                ReloadPictures();
        }

        void ReloadPictures()
        {
            if (_autoResizePictures)
            {
                RefreshResizedImage();
            }
            else
            {
                pictureBox1.Image = originalImage1;
                pictureBox2.Image = originalImage2;
            }
        }

        void RefreshResizedImage()
        {
            if (originalImage1 != null && originalImage2 != null)
            {
                float resizePercent = LoadImageHelper.GetResizePercent(originalImage1, originalImage2, pictureBox1.Size, pictureBox2.Size);
                pictureBox1.Image = LoadImageHelper.ResizeImage(originalImage1, pictureBox1.Size, resizePercent);
                pictureBox2.Image = LoadImageHelper.ResizeImage(originalImage2, pictureBox2.Size, resizePercent);
            }
        }

        private void splitContainer2_Resize(object sender, EventArgs e)
        {
            if (_autoResizePictures)
            {
                pictureBox1.Size = panel1.Size;
                pictureBox2.Size = panel2.Size;

                RefreshResizedImage();
            }
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            var path = pictureBox1.Image?.Tag;
            OpenFile((string)path);
        }

        private void pictureBox2_DoubleClick(object sender, EventArgs e)
        {
            var path = pictureBox2.Image?.Tag;
            OpenFile((string)path);
        }

        private void OpenFile(string path)
        {
            if (path != null)
            {
                Process.Start("explorer", "\"" + path + "\"");
            }
        }
    }
}
