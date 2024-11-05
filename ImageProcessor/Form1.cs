using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WebCamLib;
using ImageProcess2;
using System.Drawing.Imaging;

namespace ImageProcessor
{
    
    public partial class Form1 : Form
    {
        Bitmap loaded, processed, imageA, imageB, resultImage;
        Device[] devices;
        int screenColorThreshold;
        private Color screenColor = Color.FromArgb(0, 255, 0);
        private PictureBox activePictureBox=null;

        public Form1()
        {
            InitializeComponent();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            loaded = new Bitmap(openFileDialog1.FileName);
            pictureBox1.Image = loaded;
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processed = new Bitmap(loaded.Width, loaded.Height);
            Color pixel;
            for(int i = 0; i < loaded.Width; i++)
            {
                for (int j = 0; j < loaded.Height; j++)
                {
                    pixel = loaded.GetPixel(i, j);
                    processed.SetPixel(i, j,pixel);
                }
            }
            pictureBox2.Image = processed;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            processed.Save(saveFileDialog1.FileName);
        }

        private void greyscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processed = new Bitmap(loaded.Width, loaded.Height);
            Color pixel;
            int ave;
            for (int i = 0; i < loaded.Width; i++)
            {
                for (int j = 0; j < loaded.Height; j++)
                {
                    pixel = loaded.GetPixel(i, j);
                    ave = (int)(pixel.R + pixel.G + pixel.B) / 3;
                    Color gray = Color.FromArgb(ave, ave, ave);
                    processed.SetPixel(i, j, gray);
                }
            }
            pictureBox2.Image = processed;
        }

        private void colorInversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processed = new Bitmap(loaded.Width, loaded.Height);
            Color pixel;
            for (int i = 0; i < loaded.Width; i++)
            {
                for (int j = 0; j < loaded.Height; j++)
                {
                    pixel = loaded.GetPixel(i, j);
                    Color inv = Color.FromArgb(255 - pixel.R, 255 - pixel.G, 255 - pixel.B);
                    processed.SetPixel(i, j, inv);
                }
            }
            pictureBox2.Image = processed;
        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasicDIP.Histogram(ref loaded, ref processed);
            pictureBox2.Image = processed;
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processed = new Bitmap(loaded.Width, loaded.Height);
            Color pixel;
            int sepiaR, sepiaG, sepiaB;
            for (int i = 0; i < loaded.Width; i++)
            {
                for (int j = 0; j < loaded.Height; j++)
                {
                    pixel = loaded.GetPixel(i, j);
                    sepiaR = (int)(0.393 * pixel.R + 0.769 * pixel.G + 0.189 * pixel.B);
                    sepiaG = (int)(0.349 * pixel.R + 0.686 * pixel.G + 0.168 * pixel.B);
                    sepiaB = (int)(0.272 * pixel.R + 0.534 * pixel.G + 0.131 * pixel.B);
                    sepiaR = Math.Min(255, sepiaR);
                    sepiaG = Math.Min(255, sepiaG);
                    sepiaB = Math.Min(255, sepiaB);

                    Color sepia = Color.FromArgb(sepiaR, sepiaG, sepiaB);
                    processed.SetPixel(i, j, sepia);
                }
            }
            pictureBox2.Image = processed;
        }

        private void horizontallyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasicDIP.FlipHorizontal(ref loaded, ref processed);
            pictureBox2.Image = processed;
        }

        private void verticallyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasicDIP.FlipVertical(ref loaded, ref processed);
            pictureBox2.Image = processed;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            BasicDIP.Brightness(ref loaded, ref processed, trackBar1.Value);
            pictureBox2.Image = processed;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            BasicDIP.Rotate(ref loaded, ref processed, trackBar2.Value);
            pictureBox2.Image = processed;
        }

        private void scaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasicDIP.Scale(ref loaded, ref processed, 200,200);
            pictureBox2.Image = processed;
        }

        private void binaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasicDIP.Threshold(ref loaded, ref processed, 180);
            pictureBox2.Image = processed;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            devices = DeviceManager.GetAllDevices();
            button5.BackColor = screenColor;
            screenColorThreshold = trackBar3.Value;
        }

        private void cameraOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (activePictureBox != null)
            {
                devices[0].Stop();
                activePictureBox.Image = null;
            }

            activePictureBox = pictureBox1;
            devices[0].ShowWindow(pictureBox1);
        }

        private void cameraOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            devices[0].Stop();
        }

        private void openFileDialog3_FileOk(object sender, CancelEventArgs e)
        {
            imageA = new Bitmap(openFileDialog3.FileName);
            pictureBox4.Image = imageA;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int threshold = screenColorThreshold;


            int width = Math.Min(imageA.Width, imageB.Width);
            int height = Math.Min(imageA.Height, imageB.Height);

            resultImage = new Bitmap(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color pixel = imageB.GetPixel(x, y); 
                    Color backpixel = imageA.GetPixel(x, y);   

                    int redDiff = pixel.R - screenColor.R;
                    int greenDiff = pixel.G - screenColor.G;
                    int blueDiff = pixel.B - screenColor.B;
                    int colorDistance = (int)Math.Sqrt(redDiff * redDiff + greenDiff * greenDiff + blueDiff * blueDiff);

                    if (colorDistance < threshold)
                        resultImage.SetPixel(x, y, backpixel);
                    else
                        resultImage.SetPixel(x, y, pixel);
                }
            }

            pictureBox5.Image = resultImage;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            IDataObject data;
            Image bmap;
            devices[0].Sendmessage();
            data = Clipboard.GetDataObject();
            if (data != null)
            {
                bmap = (Image)data.GetData("System.Drawing.Bitmap");
                if (bmap != null)
                {
                    Bitmap b = new Bitmap(bmap);
                    BitmapFilter.GrayScale(b);
                    pictureBox2.Image = b;
                }
            }
        }

        private void greyscaleToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    screenColor = colorDialog.Color;
                    button5.BackColor = screenColor;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (activePictureBox != null)
            {
                devices[0].Stop();
                activePictureBox.Image = null;
            }

            activePictureBox = pictureBox3;
            devices[0].ShowWindow(pictureBox3);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            timer2.Enabled = true;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            IDataObject data;
            Image bmap;
            devices[0].Sendmessage();
            data = Clipboard.GetDataObject();
            if (data != null)
            {
                bmap = (Image)data.GetData("System.Drawing.Bitmap");
                if (bmap != null)
                {
                    Bitmap foregroundImage = new Bitmap(bmap);
                    Bitmap backgroundImage = new Bitmap(imageA, foregroundImage.Width, foregroundImage.Height);
                    Bitmap resultImage = BitmapFilter.ApplyGreenScreen(foregroundImage, backgroundImage, screenColor, screenColorThreshold);
                    pictureBox5.Image = resultImage;
                }
            }
        }


        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            screenColorThreshold = trackBar3.Value;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            saveFileDialog2.ShowDialog();
        }

        private void saveFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            pictureBox5.Image.Save(saveFileDialog2.FileName);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog3.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            imageB = new Bitmap(openFileDialog2.FileName);
            pictureBox3.Image = imageB;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }
    }
}
