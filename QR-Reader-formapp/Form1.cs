using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;

using Microsoft.Win32;
using static System.Console;
using OpenCvSharp.Extensions;
using ZXing;

namespace QR_Reader_formapp
{
    public partial class Form1 : Form
    {
        int WIDTH = 640;
        int HEIGHT = 480;
        Mat frame;
        VideoCapture capture;
        Bitmap bmp;
        Graphics graphic;
        ZXing.BarcodeReader reader;
        string prevData;

        public Form1()
        {
            InitializeComponent();

            //カメラ画像取得用のVideoCapture作成
            capture = new VideoCapture(0); // 0がインカメ, 1以降がウェブカメラ
            if (!capture.IsOpened())
            {
                capture = new VideoCapture(1); // 0がインカメ, 1以降がウェブカメラ
            }
            if (!capture.IsOpened())
            {
                MessageBox.Show("camera was not found!");
                this.Close();
                throw new Exception();
            }
            capture.FrameWidth = WIDTH;
            capture.FrameHeight = HEIGHT;

            reader = new ZXing.BarcodeReader();

            //取得先のMat作成
            frame = new Mat(HEIGHT, WIDTH, MatType.CV_8UC3);

            //表示用のBitmap作成
            bmp = new Bitmap(frame.Cols, frame.Rows, (int)frame.Step(), System.Drawing.Imaging.PixelFormat.Format24bppRgb, frame.Data);

            //PictureBoxを出力サイズに合わせる
            pictureBox1.Width = frame.Cols;
            pictureBox1.Height = frame.Rows;

            //描画用のGraphics作成
            graphic = pictureBox1.CreateGraphics();

            //画像取得スレッド開始
            backgroundWorker1.RunWorkerAsync();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //スレッドの終了を待機
            backgroundWorker1.CancelAsync();
            while (backgroundWorker1.IsBusy)
                Application.DoEvents();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = (BackgroundWorker)sender;

            while (!backgroundWorker1.CancellationPending)
            {
                //画像取得
                capture.Grab();
                NativeMethods.videoio_VideoCapture_operatorRightShift_Mat(capture.CvPtr, frame.CvPtr);

                bw.ReportProgress(0);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //描画
            graphic.DrawImage(bmp, 0, 0, frame.Cols, frame.Rows);
            scanQRcode();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string userProfilePath = Environment.GetEnvironmentVariable("UserProfile");
            string filePath = userProfilePath + @"\Desktop\cap.bmp";
            frame.SaveImage(filePath);
           
            using (Mat cap = new Mat(filePath))
            {
                //保存されたキャプチャ画像の出力
                Cv2.ImShow("test1", frame);
            }
        }

        private void scanQRcode() {
            string userProfilePath = Environment.GetEnvironmentVariable("UserProfile");

            if (reader == null) {
                return;
            }
            if (frame == null) {
                return;
            }
            if (frame.ToBitmap() == null)
            {
                return;
            }
            // QRコードの解析
            //ZXingに渡すのはBitmap
            try {
                ZXing.Result result = reader.Decode(frame.ToBitmap());
                var text = result == null ? DateTime.Now.ToString() : result.Text;
                label2.Text = text;
                if (result != null && prevData != result.Text)
                {
                    File.AppendAllText(userProfilePath + @"\Desktop\log.txt", text + Environment.NewLine);
                    // パラメータを指定して実行
                    System.Diagnostics.Process.Start("notepad.exe", userProfilePath + @"\Desktop\log.txt");
                    prevData = text;
                }
               
            } catch (Exception exception) {
                throw exception;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
