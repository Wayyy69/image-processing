using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//tambahkan ini 
using System.Drawing.Imaging;
using System.IO; 

namespace UTS
{
    public partial class Form1 : Form
    {
        Bitmap sourceImage, processingImage, tempImage;
        int imageHeight, imageWidth;
        // number of processing image
        int imProcNo;
        int BIN = 256;           // jumlah histogram BIN 
        Bitmap grayImage; //gray image without noise
        Bitmap noiseImage; //gray image with noise
        int filterSmoothingType; //1 : meanFilter, 2: MedianFilter, 3:MinimumFilter, 4:MaximumFilter
        int filterSharpeningType;//1 : RoberFIlter, 2: PrewittFIlter, 3 : SobelFilter, 4:LaplacianFilter
        //variable of image flipping          
        int imageFlipping;
        //variable of image rotation         
        int imageRotation;
        //levef of resampling image         
        int resampleLevel;

        //level of intensity quantization        
        int quantizationLevel;
        public Form1()
        {
            InitializeComponent();
            //initialization            
            chart1.Series.Clear();
            textBox5.Text = "256";
            textBox5.TextAlign = HorizontalAlignment.Center; 
            textBoxInitialization(); 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                //loading source image
                sourceImage = (Bitmap)Bitmap.FromFile(openFileDialog1.FileName);
                processingImage = new Bitmap(sourceImage);
                tempImage = new Bitmap(sourceImage);
                //menampilkan sourceImage di pictureBox1
                pictureBox1.Image = sourceImage;
                //menampilkan label image sebagai "Original Image"

                //mencari tinggi dan lebar image
                imageHeight = sourceImage.Height;
                imageWidth = sourceImage.Width;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult d = saveFileDialog1.ShowDialog();
            if (d == DialogResult.OK)
            {
                string ext = Path.GetExtension(saveFileDialog1.FileName).ToLower();
                string fileName = saveFileDialog1.FileName;
                ImageFormat format = ImageFormat.Jpeg;

                if (ext == ".bmp")
                { format = ImageFormat.Bmp; }
                else if (ext == ".png")
                { format = ImageFormat.Png; }
                else if (ext == ".gif")
                { format = ImageFormat.Gif; }
                else if (ext == ".tiff")
                { format = ImageFormat.Tiff; }

                try
                {
                    lock (this)
                    {
                        Bitmap image = (Bitmap)pictureBox1.Image;

                        image.Save(fileName, format);
                    }
                }
                catch (Exception ex) { MessageBox.Show("Failed saving the image.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); };
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
            resetRadioButtonFlipping();
            resetRadioButtonHistogram();
            resetCheckBox();
            pictureBox1.Image = sourceImage;
            resetTextbox();
            chart1.Series.Clear();
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            grayImage = grayImaging(sourceImage);
            //menambahkan noise ke gray image
            noiseImage = noiseImaging(grayImage);
            //original image ditampilkan dalam bentuk gray image
            pictureBox1.Image = grayImage;
            //resetting radio button
            resetRadioButtonSmoothing();
            resetRadioButtonSharpening();

        }


        
      
        private void resetRadioButtonFlipping()
        {
            radioButton1.Checked = false; 
            radioButton2.Checked = false;
        }

        private void resetRadioButtonResampling()
        {
            radioButton3.Checked = false;
            radioButton4.Checked = false;
            radioButton5.Checked = false;
            radioButton6.Checked = false;
        }

        private void resetRadioButtonQuantization()
        {
            radioButton7.Checked = false;
            radioButton8.Checked = false;
            radioButton9.Checked = false;
            radioButton10.Checked = false;
        }

        private void resetRadioButtonRotation()
        {
            radioButton11.Checked = false;
            radioButton12.Checked = false;
            radioButton13.Checked = false;
        }

        private void resetRadioButtonSmoothing()
        {

            radioButton14.Checked = false;
            radioButton15.Checked = false;
            radioButton16.Checked = false;
            radioButton17.Checked = false;

        }

        private void resetRadioButtonSharpening()
        {
            radioButton18.Checked = false;
            radioButton19.Checked = false;
            radioButton20.Checked = false;
            radioButton21.Checked = false;

        }

        private void resetRadioButtonHistogram()
        {
            radioButton22.Checked = false;
            radioButton23.Checked = false;
            radioButton24.Checked = false;
            radioButton25.Checked = false;
            radioButton26.Checked = false;
        }

        private void resetCheckBox()
        {
            checkBox1.Checked = false;
           
        }

        private Bitmap grayImaging(Bitmap image)
        {
            Bitmap tempImage = new Bitmap(image);
            //grayscale convertion
            for (int x = 0; x < sourceImage.Width; x++)
                for (int y = 0; y < sourceImage.Height; y++)
                {
                    Color w = image.GetPixel(x, y);
                    int r = w.R; int g = w.G; int b = w.B;
                    int xg = (int)((r + g + b) / 3);
                    Color wb = Color.FromArgb(xg, xg, xg);
                    tempImage.SetPixel(x, y, wb);
                }
            return tempImage;
        }
        private Bitmap noiseImaging(Bitmap image)
        {
            noiseImage = new Bitmap(grayImage);
            int noiseProb = 10;
            Random r = new Random();
            for (int x = 0; x < grayImage.Width; x++)
                for (int y = 0; y < grayImage.Height; y++)
                {
                    Color w = image.GetPixel(x, y);
                    int xg = w.R;
                    int xb = xg;
                    //generate random number (0-100)
                    int nr = r.Next(0, 100);
                    //generationg 20% gaussian noise
                    if (nr < noiseProb) xb = 0;
                    Color wb = Color.FromArgb(xb, xb, xb);
                    noiseImage.SetPixel(x, y, wb);
                }
            return noiseImage;
        }


        //smoothing filter         
        private Bitmap smoothingfilter(int filterType)
        {
            Bitmap filteredImage = new Bitmap(noiseImage);

            int[] xt = new int[10];
            int xb = 0;

            for (int x = 1; x < noiseImage.Width - 1; x++)
                for (int y = 1; y < noiseImage.Height - 1; y++)
                {
                    Color w1 = noiseImage.GetPixel(x - 1, y - 1);
                    Color w2 = noiseImage.GetPixel(x - 1, y);
                    Color w3 = noiseImage.GetPixel(x - 1, y + 1);
                    Color w4 = noiseImage.GetPixel(x, y - 1);
                    Color w5 = noiseImage.GetPixel(x, y);
                    Color w6 = noiseImage.GetPixel(x, y + 1);
                    Color w7 = noiseImage.GetPixel(x + 1, y - 1);
                    Color w8 = noiseImage.GetPixel(x + 1, y);
                    Color w9 = noiseImage.GetPixel(x + 1, y + 1);

                    xt[1] = w1.R; xt[2] = w2.R; xt[3] = w3.R;
                    xt[4] = w4.R; xt[5] = w5.R; xt[6] = w6.R;
                    xt[7] = w7.R; xt[8] = w8.R; xt[9] = w9.R;

                    if (filterType == 1)
                    //mean filter                    
                    {

                        //calculation of mean  
                        xb = (xt[1] + xt[2] + xt[3] + xt[4] + xt[5] + xt[6] + xt[7] + xt[8] + xt[9]) / 9;

                        //the mean                       
                        xb = xt[5];

                    }
                    else if (filterType == 2)
                    //median filter                   
                    {
                        //looking for median                         
                        for (int i = 1; i < 9; i++)
                            for (int j = 1; j < 9; j++)
                            {
                                if (xt[j] > xt[j + 1])
                                {
                                    int a = xt[j];
                                    xt[j] = xt[j + 1];
                                    xt[j + 1] = a;
                                }
                            }
                        //the median                        
                        xb = xt[5];
                    }
                    else if (filterType == 3)
                    //minimum filter                   
                    {
                        int xMinimum = xt[1];
                        //initialization                         
                        //looking for minimum                        
                        for (int i = 2; i < 10; i++)
                        {
                            if (xt[i] < xMinimum)
                            {
                                xMinimum = xt[i];
                            }
                        }
                        xb = xMinimum;
                    }
                    else if (filterType == 4)
                    //maximum filter                    
                    {

                        int xMaximum = xt[1];
                        //initialization                         
                        //looking for Maximum                    
                        for (int i = 2; i < 10; i++)
                        {
                            if (xt[i] > xMaximum)
                            {
                                xMaximum = xt[i];
                            }
                        }
                        xb = xMaximum;

                    }
                    Color wb = Color.FromArgb(xb, xb, xb);
                    filteredImage.SetPixel(x, y, wb);
                }

            return filteredImage;
        }

        private void radioButton14_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton14.Checked == false) return;

            //resetting radio button            
            resetRadioButtonSharpening();
            resetRadioButtonFlipping();
            resetRadioButtonHistogram();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonRotation();

            if (noiseImage == null) return;
            Bitmap tempImage = new Bitmap(noiseImage);

            filterSmoothingType = 1;
            tempImage = smoothingfilter(filterSmoothingType);

            pictureBox1.Image = tempImage;
        }

        private void radioButton15_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton15.Checked == false) return;

            //resetting radio button            
            resetRadioButtonSharpening();
            resetRadioButtonFlipping();
            resetRadioButtonHistogram();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
           
           
            if (noiseImage == null) return;
            Bitmap tempImage = new Bitmap(noiseImage);

            filterSmoothingType = 2;
            tempImage = smoothingfilter(filterSmoothingType);

            pictureBox1.Image = tempImage; 
        }

        private void radioButton16_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton16.Checked == false) return;

            //resetting radio button            
            resetRadioButtonSharpening();
            resetRadioButtonFlipping();
            resetRadioButtonHistogram();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
           
            if (noiseImage == null) return;
            Bitmap tempImage = new Bitmap(noiseImage);

            filterSmoothingType = 3;
            tempImage = smoothingfilter(filterSmoothingType);

            pictureBox1.Image = tempImage; 
        }

        private void radioButton17_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton17.Checked == false) return;

            //resetting radio button            
            resetRadioButtonSharpening();
            resetRadioButtonFlipping();
            resetRadioButtonHistogram();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
           
            if (noiseImage == null) return;
            Bitmap tempImage = new Bitmap(noiseImage);

            filterSmoothingType = 4;
            tempImage = smoothingfilter(filterSmoothingType);

            pictureBox1.Image = tempImage; 
        }

        //sharpening filter         
        private Bitmap sharpeningFilter(int filterType)
        {
            noiseImage = grayImage;
            Bitmap filteredImage = new Bitmap(noiseImage);

            int[] xt = new int[10];
            int xb = 0;

            for (int x = 1; x < noiseImage.Width - 1; x++)
                for (int y = 1; y < noiseImage.Height - 1; y++)
                {
                    Color w1 = noiseImage.GetPixel(x - 1, y - 1);
                    Color w2 = noiseImage.GetPixel(x - 1, y);
                    Color w3 = noiseImage.GetPixel(x - 1, y + 1);
                    Color w4 = noiseImage.GetPixel(x, y - 1);
                    Color w5 = noiseImage.GetPixel(x, y);
                    Color w6 = noiseImage.GetPixel(x, y + 1);
                    Color w7 = noiseImage.GetPixel(x + 1, y - 1);
                    Color w8 = noiseImage.GetPixel(x + 1, y);
                    Color w9 = noiseImage.GetPixel(x + 1, y + 1);

                    xt[1] = w1.R; xt[2] = w2.R; xt[3] = w3.R;
                    xt[4] = w4.R; xt[5] = w5.R; xt[6] = w6.R;
                    xt[7] = w7.R; xt[8] = w8.R; xt[9] = w9.R;

                    // Robert filter                     
                    //           -1   1                   
                    //           1   -1                 
                    //                   
                    // Prewit vertical filter           
                    //      -1   0   1              
                    //      -1   0   1            
                    //      -1   0   1          
                    //                  
                    // Prewit horizontal filter     
                    //      -1   -1  -1            
                    //       0   0   0            
                    //       1   1   1            
                    //                 
                    // Sobel horizontal filter 
                    //      -1  -2  -1                
                    //       0   0   0             
                    //       1   2   1             
                    //                    
                    // Sobel vertical filter   
                    //      -1   0   1       
                    //      -2   0   2           
                    //      -1   0   1           
                    //                 
                    // Laplacian filter        
                    //       1   -2   1           
                    //       -2   4   -2             
                    //       1   -2   1            
                    // 

                    if (filterType == 1)
                    //Robert filter  
                    {

                        //calculation of mean  
                        xb = xt[5] - xt[2] + xt[5] - xt[4];
                        if (xb < 0) xb = 0;
                        if (xb > 255) xb = 255;
                    }
                    else if (filterType == 2)
                    //Prewitt filter        
                    {
                        int xh = -1 * xt[1] - 1 * xt[2] - 1 * xt[3] +
                            0 * xt[4] + 0 * xt[5] + 0 * xt[6] +
                            1 * xt[7] + 1 * xt[8] + 1 * xt[9];
                        int xv = -1 * xt[1] + 0 * xt[2] + 1 * xt[3] +
                            -1 * xt[4] + 0 * xt[5] + 1 * xt[6] +
                            -1 * xt[7] + 0 * xt[8] + 1 * xt[9];

                        xb = xh + xv;
                        if (xb < 0) xb = 0;
                        if (xb > 255) xb = 255;

                    }
                    else if (filterType == 3)
                    //Sobel filter  
                    {
                        //tambahkan koding  
                        int xh = -1 * xt[1] - 2 * xt[2] - 1 * xt[3] +
                           0 * xt[4] + 0 * xt[5] + 0 * xt[6] +
                           1 * xt[7] + 2 * xt[8] + 1 * xt[9];
                        int xv = -1 * xt[1] + 0 * xt[2] + 1 * xt[3]
                            - 2 * xt[4] + 0 * xt[5] + 2 * xt[6] +
                            -1 * xt[7] + 0 * xt[8] + 1 * xt[9];

                        xb = xh + xv;
                        if (xb < 0) xb = 0;
                        if (xb > 255) xb = 255;
                    }
                    else if (filterType == 4)
                    //Laplacian filter   
                    {
                        // tambahkan koding 
                        xb = (int)(1 * xt[1] + -2 * xt[2] + 1 * xt[3]
                         - 2 * xt[4] + 4 * xt[5] + -2 * xt[6] +
                         1 * xt[7] + -2 * xt[8] + 1 * xt[9]);
                        if (xb < 0) xb = 0;
                        if (xb > 255) xb = 255;
                    }

                    Color wb = Color.FromArgb(xb, xb, xb);
                    filteredImage.SetPixel(x, y, wb);
                }

            return filteredImage;
        }

        private void radioButton18_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton18.Checked == false) return; if (noiseImage == null) return;

            
            resetRadioButtonSmoothing();
            resetRadioButtonFlipping();
            resetRadioButtonHistogram();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
           

            Bitmap tempImage = new Bitmap(noiseImage);

            filterSharpeningType = 1;
            tempImage = sharpeningFilter(filterSharpeningType);
            pictureBox1.Image = tempImage; 
        }

        private void radioButton19_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton19.Checked == false) return; if (noiseImage == null) return;

            
            resetRadioButtonSmoothing();
            resetRadioButtonFlipping();
            resetRadioButtonHistogram();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
           

            Bitmap tempImage = new Bitmap(noiseImage);

            filterSharpeningType = 2;
            tempImage = sharpeningFilter(filterSharpeningType);
            pictureBox1.Image = tempImage; 
        }

        private void radioButton20_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton20.Checked == false) return; if (noiseImage == null) return;

            resetRadioButtonSmoothing();
            resetRadioButtonFlipping();
            resetRadioButtonHistogram();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
           

            Bitmap tempImage = new Bitmap(noiseImage);

            filterSharpeningType = 3;
            tempImage = sharpeningFilter(filterSharpeningType);
            pictureBox1.Image = tempImage; 
        }

        private void radioButton21_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton21.Checked == false) return; if (noiseImage == null) return;

            
            resetRadioButtonSmoothing();
            resetRadioButtonFlipping();
            resetRadioButtonHistogram();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
           

            Bitmap tempImage = new Bitmap(noiseImage);

            filterSharpeningType = 4;
            tempImage = sharpeningFilter(filterSharpeningType);
            pictureBox1.Image = tempImage; 
        }

      
        private void textBoxInitialization()
        {
            textBox1.Text = "0";
            textBox2.Text = "0";
            textBox3.Text = "0";
            textBox4.Text = "0";
        }

        private void setImageFlipping(int flipping)
        {
            if (tempImage == null) return;

            //image flipping             
            // 1 = horizontal             
            // 2 = vertical 

            Bitmap flipImage = new Bitmap(tempImage);
            for (int x = 0; x < imageWidth; x++)
                for (int y = 0; y < imageHeight; y++)
                {
                    Color w = flipImage.GetPixel(x, y);

                    if (flipping == 1) //flip horizontal                    
                    { tempImage.SetPixel(imageWidth - 1 - x, y, w); }
                    else if (flipping == 2) //flip vertical                    
                    { tempImage.SetPixel(x, imageHeight - 1 - y, w); }
                }
            pictureBox1.Image = tempImage;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            imageFlipping = 1;
            setImageFlipping(imageFlipping);
            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonHistogram();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
           
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            imageFlipping = 2;
            setImageFlipping(imageFlipping);
            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonHistogram();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
           
        }

        private void setImageRotation(int rotation)
        {
            if (tempImage == null) return;

            if (rotation == 90)
            { tempImage.RotateFlip(RotateFlipType.Rotate90FlipNone); }
            else if (rotation == 180)
            { tempImage.RotateFlip(RotateFlipType.Rotate180FlipNone); }
            else if (rotation == 270)
            { tempImage.RotateFlip(RotateFlipType.Rotate270FlipNone); }
            pictureBox1.Image = tempImage;
        }

        private void radioButton11_CheckedChanged(object sender, EventArgs e)
        {
            imageRotation = 90;
            setImageRotation(imageRotation);
            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonHistogram();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonFlipping();
           
        }

        private void radioButton12_CheckedChanged(object sender, EventArgs e)
        {
            imageRotation = 180;
            setImageRotation(imageRotation);
            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonHistogram();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonFlipping();
        }

        private void radioButton13_CheckedChanged(object sender, EventArgs e)
        {
            imageRotation = 270;
            setImageRotation(imageRotation);
            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonHistogram();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonFlipping();
        }

        private void setTranslation(int xTrans, int yTrans)
        {
            Bitmap transImage = new Bitmap(imageWidth, imageHeight);
            for (int x = 0; x < imageWidth; x++)
                for (int y = 0; y < imageHeight; y++)
                {
                    Color w = tempImage.GetPixel(x, y);
                    byte warnaMerah = w.R; byte warnaHijau = w.G;
                    int xT = x + xTrans;
                    int yT = y + yTrans;
                    if (yT < imageHeight && yT > 0 && xT < imageWidth && xT > 0)
                        transImage.SetPixel(xT, yT, w);
                }
            pictureBox1.Image = transImage;

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (tempImage == null) return;

            int xTrans = int.Parse(textBox3.Text);
            int yTrans = int.Parse(textBox4.Text);

            //setting X translation             
            setTranslation(xTrans, yTrans);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (tempImage == null) return;

            int xTrans = int.Parse(textBox3.Text); 
            int yTrans = int.Parse(textBox4.Text);

            //setting Y translation             
            setTranslation(xTrans, yTrans);

        }

      

        private void imageResample()
        {
            if (sourceImage == null) return;

            //resampling to new Width and new Height
            int ht = (int)(imageHeight / resampleLevel);
            int wd = (int)(imageWidth / resampleLevel);
            int i, j, k, l, new_valueR, new_valueG, new_valueB;
            for (i = 0; i < ht; i++)///
            {
                for (j = 0; j < wd; j++)///
                {
                    new_valueR = 0; new_valueG = 0; new_valueB = 0;
                    for (k = 0; k < resampleLevel; k++)
                    {
                        for (l = 0; l < resampleLevel; l++)
                        {
                            Color w = sourceImage.GetPixel(j * resampleLevel + l, i *
                           resampleLevel + k);
                            int r = w.R; //red value
                            int g = w.G; //green value
                            int b = w.B; //blue value
                            new_valueR = new_valueR + r;
                            new_valueG = new_valueG + g;
                            new_valueB = new_valueB + b;
                        }
                    }
                    new_valueR = (int)(new_valueR / (resampleLevel * resampleLevel));
                    new_valueG = (int)(new_valueG / (resampleLevel * resampleLevel));
                    new_valueB = (int)(new_valueB / (resampleLevel * resampleLevel));
                    if (new_valueR > 255) new_valueR = 255;
                    if (new_valueG > 255) new_valueG = 255;
                    if (new_valueB > 255) new_valueB = 255;
                    Color colorRed = Color.FromArgb(new_valueR, new_valueG,
                   new_valueB);
                    for (k = 0; k < resampleLevel; k++)
                    {
                        for (l = 0; l < resampleLevel; l++)
                        {
                            processingImage.SetPixel(j * resampleLevel + l, i *
                           resampleLevel + k, colorRed);
                        }
                    }
                }
            }
            pictureBox1.Image = processingImage;
        }

        private void setResampleLevel(int iLevel)
        {
            resampleLevel = iLevel;
        }

        
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked == false) return;
            //change the label to Resample image
            label14.Text = "Resample Image 8";
            setResampleLevel(8);
            imageResample();

            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonHistogram();
            resetRadioButtonQuantization();
            resetRadioButtonRotation();
            resetRadioButtonFlipping();
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked == false) return;
            //change the label to Resample image
            label14.Text = "Resample Image 16";
            setResampleLevel(16);
            imageResample();

            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonHistogram();
            resetRadioButtonQuantization();
            resetRadioButtonRotation();
            resetRadioButtonFlipping();
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked == false) return;
            //change the label to Resample image
            label14.Text = "Resample Image 32";
            setResampleLevel(32);
            imageResample();

            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonHistogram();
            resetRadioButtonQuantization();
            resetRadioButtonRotation();
            resetRadioButtonFlipping();
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton6.Checked == false) return;
            //change the label to Resample image
            label14.Text = "Resample Image 64";
            setResampleLevel(64);
            imageResample();

            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonHistogram();
            resetRadioButtonQuantization();
            resetRadioButtonRotation();
            resetRadioButtonFlipping();
        }

        private void imageQuantization()
        {
            if (sourceImage == null) return;
            for (int x = 0; x < imageWidth; x++)
                for (int y = 0; y < imageHeight; y++)
                {
                    Color w = sourceImage.GetPixel(x, y);
                    int r = w.R;
                    int g = w.G;
                    int b = w.B;
                    int rk = quantizationLevel * (int)(r / quantizationLevel);
                    int gk = quantizationLevel * (int)(g / quantizationLevel);
                    int bk = quantizationLevel * (int)(b / quantizationLevel);
                    Color wb = Color.FromArgb(rk, gk, bk);
                    processingImage.SetPixel(x, y, wb);
                }
            pictureBox1.Image = processingImage;
        }

        private void setQuantizationLevel(int iLevel)
        {
            quantizationLevel = iLevel;
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton7.Checked == false) return;
            label14.Text = "Quantizing Image 8";
            setQuantizationLevel(8);
            imageQuantization();

            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonHistogram();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
            resetRadioButtonFlipping();
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton7.Checked == false) return;
            label14.Text = "Quantizing Image 16";
            setQuantizationLevel(16);
            imageQuantization();

            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonHistogram();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
            resetRadioButtonFlipping();
        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton7.Checked == false) return;
            label14.Text = "Quantizing Image 32";
            setQuantizationLevel(32);
            imageQuantization();

            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonHistogram();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
            resetRadioButtonFlipping();
        }

        private void radioButton10_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton7.Checked == false) return;
            label14.Text = "Quantizing Image 64";
            setQuantizationLevel(64);
            imageQuantization();

            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonHistogram();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
            resetRadioButtonFlipping();
        }

        private void setBrightness(int brightness)
        {
            //inisialisasi bright image
            Bitmap bImage = new Bitmap(processingImage);
            for (int x = 0; x < imageWidth; x++)
                for (int y = 0; y < imageHeight; y++)
                {
                    Color w = processingImage.GetPixel(x, y);
                    int R = (int)(brightness + w.R);
                    //because intensity is between 0-255
                    if (R > 255) R = 255; if (R < 0) R = 0;

                    int G = (int)(brightness + w.G);
                    //because intensity is between 0-255
                    if (G > 255) G = 255; if (G < 0) G = 0;

                    int B = (int)(brightness + w.B);
                    //because intensity is between 0-255
                    if (B > 255) B = 255; if (B < 0) B = 0;

                    //setting the new color
                    Color wBaru = Color.FromArgb(R, G, B);
                    bImage.SetPixel(x, y, wBaru);
                }
            pictureBox1.Image = bImage;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (processingImage == null) return;
            int brightness = int.Parse(textBox1.Text);
            if (brightness < 0 || brightness > 255) return;
            //setting brightness
            setBrightness(brightness);
        }

        private void setContrast(double contrast)
        {
            //inisialisasi processing image
            Bitmap cImage = new Bitmap(processingImage);
            contrast = (100.0 + contrast) / 100.0;
            contrast *= contrast;
            for (int x = 0; x < imageWidth; x++)
                for (int y = 0; y < imageHeight; y++)
                {
                    Color w = processingImage.GetPixel(x, y);
                    double R = w.R / 255.0;
                    R -= 0.5;
                    R *= contrast;
                    R += 0.5;
                    R *= 255;
                    if (R > 255) R = 255; if (R < 0) R = 0;
                    double G = w.G / 255.0;
                    G -= 0.5;
                    G *= contrast;
                    G += 0.5;
                    G *= 255;
                    if (G > 255) G = 255; if (G < 0) G = 0;
                    double B = w.B / 255.0;
                    B -= 0.5;
                    B *= contrast;
                    B += 0.5;
                    B *= 255;
                    if (B > 255) B = 255; if (B < 0) B = 0;
                    Color wBaru = Color.FromArgb((byte)R, (byte)G, (byte)B);
                    cImage.SetPixel(x, y, wBaru);
                }
            pictureBox1.Image = cImage;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (processingImage == null) return;
            double contrast = double.Parse(textBox2.Text);

            //setting Contrast
            setContrast(contrast);
        }

        private void checkTextBox()
        {
            if (int.Parse(textBox5.Text) < 0)
                textBox5.Text = "0";
            else if (int.Parse(textBox5.Text) > 256)
                textBox5.Text = "256";
        }

        private Bitmap imageConvert(int imageChannel)
        {
            if (sourceImage == null) return null;

            Bitmap convImage = new Bitmap(sourceImage);

            for (int x = 0; x < sourceImage.Width; x++)
                for (int y = 0; y < sourceImage.Height; y++)
                {
                    //get the RGB value of the pixel at (x,y) 
                    Color w = sourceImage.GetPixel(x, y);


                    byte r = w.R; //red value                
                    byte g = w.G; // green value                  
                    byte b = w.B; // blue value 

                    //calculate gray channel   
                    byte gray = (byte)(0.5 * r + 0.419 * g + 0.081 * b);
                    //set the color of each channel      
                    //red channel image                    
                    Color redColor = Color.FromArgb(r, 0, 0);
                    //for green, blue and gray channel image,  
                    Color greenColor = Color.FromArgb(0, g, 0);
                    Color blueColor = Color.FromArgb(0, 0, b);
                    Color grayColor = Color.FromArgb(gray, gray, gray);
                    //please add yourself the coding for them    
                    // tambah coding sendiri  
                    //set the image pixel          
                    if (imageChannel == 1)
                    //red 
                    {
                        convImage.SetPixel(x, y, redColor);
                    }
                    else if (imageChannel == 2)
                    //green                   
                    {        // tambah coding sendiri   
                        convImage.SetPixel(x, y, greenColor);
                    }
                    else if (imageChannel == 3)
                    //blue                    
                    { // tambah coding sendiri
                        convImage.SetPixel(x, y, blueColor);
                    }
                    else if (imageChannel == 4)
                    //gray                  
                    { // tambah coding sendiri 
                        convImage.SetPixel(x, y, grayColor);
                    }
                }
            return convImage;
        }


        private float[] hitungHistogram(int imageChannel)
        {
            //init of bins            
            checkTextBox();
            BIN = int.Parse(textBox5.Text);

            //initializaation of histogram el         
            float[] h = new float[BIN];

            //histogram init           
            for (int i = 0; i < BIN; i++)
            {
                h[i] = 0;
            }

            //histogram calculation            
            for (int x = 0; x < sourceImage.Width; x++)
                for (int y = 0; y < sourceImage.Height; y++)
                {
                    Color w = sourceImage.GetPixel(x, y);

                    int r = (int)(w.R * BIN / 256);
                    int g = (int)(w.G * BIN / 256);
                    int b = (int)(w.B * BIN / 256);

                    //calculate gray channel         
                    int gray = (int)((0.5 * w.R + 0.419 * w.G + 0.081 * w.B) * BIN / 256);
                    //calculate histogram           
                    if (imageChannel == 1)
                        h[r] = h[r] + 1;
                    else if (imageChannel == 2)
                        h[g] = h[g] + 1;
                    else if (imageChannel == 3)
                        h[b] = h[b] + 1;
                    else if (imageChannel == 4)
                        h[gray] = h[gray] + 1;
                }
            return h;
        }

        private float[] hitungHistogramRGB()
        {
            //initializaation of histogram          
            checkTextBox();
            BIN = int.Parse(textBox5.Text);

            float[] h = new float[BIN * 3];

            //histogram init       
            for (int i = 0; i < BIN * 3; i++)
            { h[i] = 0; }

            //histogram calculation           
            for (int x = 0; x < sourceImage.Width; x++)
                for (int y = 0; y < sourceImage.Height; y++)
                {
                    Color w = sourceImage.GetPixel(x, y);

                    int r = (int)(w.R * BIN / 256);
                    int g = (int)(w.G * BIN / 256);
                    int b = (int)(w.B * BIN / 256);

                    //calculate histogram      
                    h[r] = h[r] + 1; //0 - BIN  
                    h[g + BIN] = h[g + BIN] + 1; //BIN - 2*BIN  
                    h[b + 2 * BIN] = h[b + 2 * BIN] + 1; //2*BIN - 3*BIN 
                }
            return h;
        }

        private void histogramRGBDisplay()
        {             //delete the histogram         
            if (chart1.Series.Count > 0)
            {
                chart1.Series.RemoveAt(0);
            }

            //chart init       
            chart1.Series.Add("RGB Image");
            chart1.Series["RGB Image"].Color = Color.Maroon;


            foreach (var series in chart1.Series)
            {
                series.Points.Clear();
            }

            float[] his = new float[BIN * 3];

            his = hitungHistogramRGB();

            for (int i = 0; i < BIN * 3; i++)
            {
                chart1.Series["RGB Image"].Points.AddXY(i, his[i]);
            }

            label3.ForeColor = Color.Maroon;
            label3.Text = "RGB Image";
            label15.ForeColor = Color.Maroon;
            label15.Text = string.Format("RGB Image Histogram {0} Bins", BIN * 3);

        }

        private void radioButton23_CheckedChanged(object sender, EventArgs e)
        {
            if (sourceImage == null) return;

            if (radioButton23.Checked == false) return;
             int pilChannel = 1;

            //delete the histogram           
            if (chart1.Series.Count > 0)
            {
                chart1.Series.RemoveAt(0);
            }

            //chart init         
            chart1.Series.Add("Red Channel Image");
            chart1.Series["Red Channel Image"].Color = Color.Red;


            foreach (var series in chart1.Series)
            { series.Points.Clear(); }

            float[] his = new float[BIN];

            his = hitungHistogram(pilChannel);
            for (int i = 0; i < BIN; i++)
            {
                chart1.Series["Red Channel Image"].Points.AddXY(i, his[i]);
            }

            //displaying Red Channel            
            Bitmap redImage = imageConvert(pilChannel);
            pictureBox1.Image = redImage;

            label3.Text = "Red Channel Image";
            label3.ForeColor = Color.Red;
            label15.ForeColor = Color.Red;
            label15.Text = string.Format("Red Channel Image Histogram {0} Bins", BIN);


            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
            resetRadioButtonFlipping();
        }

        private void radioButton24_CheckedChanged(object sender, EventArgs e)
        {
            if (sourceImage == null) return;

            if (radioButton24.Checked == false) return;
             int pilChannel = 2;

            //delete the histogram           
            if (chart1.Series.Count > 0)
            {
                chart1.Series.RemoveAt(0);
            }

            //chart init         
            chart1.Series.Add("Green Channel Image");
            chart1.Series["Green Channel Image"].Color = Color.Green;


            foreach (var series in chart1.Series)
            { series.Points.Clear(); }

            float[] his = new float[BIN];

            his = hitungHistogram(pilChannel);
            for (int i = 0; i < BIN; i++)
            {
                chart1.Series["Green Channel Image"].Points.AddXY(i, his[i]);
            }

            //displaying Green Channel            
            Bitmap greenImage = imageConvert(pilChannel);
            pictureBox1.Image = greenImage;

            label3.Text = "Green Channel Image";
            label3.ForeColor = Color.Green;
            label15.ForeColor = Color.Green;
            label15.Text = string.Format("Green Channel Image Histogram {0} Bins", BIN);


            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
            resetRadioButtonFlipping();
        }

        private void radioButton25_CheckedChanged(object sender, EventArgs e)
        {
            if (sourceImage == null) return;

            if (radioButton25.Checked == false) return;
             int pilChannel = 3;

            //delete the histogram           
            if (chart1.Series.Count > 0)
            {
                chart1.Series.RemoveAt(0);
            }

            //chart init         
            chart1.Series.Add("Blue Channel Image");
            chart1.Series["Blue Channel Image"].Color = Color.Blue;


            foreach (var series in chart1.Series)
            { series.Points.Clear(); }

            float[] his = new float[BIN];

            his = hitungHistogram(pilChannel);
            for (int i = 0; i < BIN; i++)
            {
                chart1.Series["Blue Channel Image"].Points.AddXY(i, his[i]);
            }

            //displaying Red Channel            
            Bitmap blueImage = imageConvert(pilChannel);
            pictureBox1.Image = blueImage;

            label3.Text = "Blue Channel Image";
            label3.ForeColor = Color.Blue;
            label15.ForeColor = Color.Blue;
            label15.Text = string.Format("Blue Channel Image Histogram {0} Bins", BIN);


            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
            resetRadioButtonFlipping();
        }

        private void radioButton26_CheckedChanged(object sender, EventArgs e)
        {
            if (sourceImage == null) return;

            if (radioButton26.Checked == false) return;
            int pilChannel = 4;

            //delete the histogram           
            if (chart1.Series.Count > 0)
            {
                chart1.Series.RemoveAt(0);
            }

            //chart init         
            chart1.Series.Add("Gray Channel Image");
            chart1.Series["Gray Channel Image"].Color = Color.Gray;


            foreach (var series in chart1.Series)
            { series.Points.Clear(); }

            float[] his = new float[BIN];

            his = hitungHistogram(pilChannel);
            for (int i = 0; i < BIN; i++)
            {
                chart1.Series["Gray Channel Image"].Points.AddXY(i, his[i]);
            }

            //displaying Red Channel            
            Bitmap grayImage = imageConvert(pilChannel);
            pictureBox1.Image = grayImage;

            label3.Text = "Gray Channel Image";
            label3.ForeColor = Color.Gray;
            label15.ForeColor = Color.Gray;
            label15.Text = string.Format("Gray Channel Image Histogram {0} Bins", BIN);


            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
            resetRadioButtonFlipping();
        }

        private void radioButton22_CheckedChanged(object sender, EventArgs e)
        {
            if (sourceImage == null) return; 
            if (radioButton22.Checked == false) return;

            histogramRGBDisplay();

            pictureBox1.Image = sourceImage;


            resetRadioButtonSharpening();
            resetRadioButtonSmoothing();
            resetRadioButtonQuantization();
            resetRadioButtonResampling();
            resetRadioButtonRotation();
            resetRadioButtonFlipping();
        }

       
        private void resetTextbox()

        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            

        }

        


       
    }
}
