using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenCvSharp;
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
        BarcodeReader reader;
        String prevData;

        public Form1()
        {
            InitializeComponent();

            for (int i = 0, max =10; i < max; i++) {
                //カメラ画像取得用のVideoCapture作成
                capture = new VideoCapture(i); // 0がインカメ, 1以降がウェブカメラ
                if (capture.IsOpened())
                {
                    break;
                }
            }
            //カメラ画像取得用のVideoCapture作成
            if (!capture.IsOpened())
            {
                MessageBox.Show("camera was not found!");
                Close();
                throw new Exception();
            }

            capture.FrameWidth = WIDTH;
            capture.FrameHeight = HEIGHT;

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

            // バーコードリーダーのインスタンスを作成
            reader = new BarcodeReader();
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
            String text = scanQRcode();
            Save(text);
        }

        /// <summary>
        /// QRコードの解析
        /// </summary>
        private String scanQRcode() {
            String text = null;
            try {
                //    Bitmap image = new Bitmap(userProfilePath + @"\Desktop\sample-qr-3.png");
                //    ZXing.Result result = reader.Decode(image);
                Result result = reader.Decode(frame.ToBitmap());
                if (result != null && prevData != result.Text) {
                    text = result.Text;
                    prevData = text;
                }
               
            } catch (Exception exception) {
                throw exception;
            }
            return text;
        }

        /// <summary>
        /// データの保存
        /// </summary>
        /// <param name="text"></param>
        private void Save(String text) {
            if (text == null) {
                return;
            }
            String userProfilePath = Environment.GetEnvironmentVariable("UserProfile");
            String logPath = @"\Desktop\log.txt";
            File.AppendAllText(userProfilePath + logPath, text + Environment.NewLine);
            // メモ帳を開く
            System.Diagnostics.Process.Start("notepad.exe", userProfilePath + logPath);
        }
    }
}
