using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace EyeGazeTracking
{
    public partial class MainForm : Form
    {
        private VideoCapture videoCapture;

        private CascadeClassifier faceClassifier;
        private CascadeClassifier eyeClassifier;

        private SettingsForm settingsForm;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var cameraSettings = new Tuple<CapProp, int>[]
            {
                new Tuple<CapProp, int>(CapProp.HwAcceleration, (int)VideoAccelerationType.Any),
                new Tuple<CapProp, int>(CapProp.FrameWidth, 1920),
                new Tuple<CapProp, int>(CapProp.FrameHeight, 1080)
            };

            videoCapture = new VideoCapture(captureProperties: cameraSettings);
            videoCapture.FlipHorizontal = true;

            faceClassifier = new CascadeClassifier("face.xml");
            eyeClassifier = new CascadeClassifier("eye.xml");

            Application.Idle += Application_Idle;

            settingsForm = new SettingsForm();
            settingsForm.Show(this);
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            var frame = videoCapture.QueryFrame();

            if (frame != null)
            {
                var image = frame.ToImage<Bgr, byte>();

                var grayImage = image.Convert<Gray, byte>();
                Rectangle[] faces = faceClassifier.DetectMultiScale(
                    grayImage,
                    1.1,
                    10,
                    new Size((grayImage.Width / 5), (grayImage.Height / 4)));

                if (faces.Length > 0)
                {
                    var face = faces[0];

                    Rectangle faceROI = new Rectangle(face.Left, (face.Top + (face.Height / 5)),
                        face.Width, (face.Height / 3));

                    // Start Debug
                    image.Draw(face, new Bgr(Color.Red), 2);
                    image.Draw(faceROI, new Bgr(Color.Blue), 2);
                    // End Debug

                    var a1 = new Rectangle(faceROI.Left, faceROI.Top, (faceROI.Width / 2), faceROI.Height);
                    var a2 = new Rectangle((faceROI.Left + (faceROI.Width / 2)), faceROI.Top, (faceROI.Width / 2), faceROI.Height);

                    // Start Debug
                    image.Draw(a1, new Bgr(Color.Yellow), 1);
                    image.Draw(a2, new Bgr(Color.Green), 1);
                    // End Debug

                    Image<Gray, byte> grayLeftEyeFrame = grayImage.Copy(a1);
                    Image<Gray, byte> grayRightEyeFrame = grayImage.Copy(a2);

                    Rectangle[] lefEyes = eyeClassifier.DetectMultiScale(
                        grayLeftEyeFrame,
                        1.2,
                        5,
                        new Size((faceROI.Width / 7), (faceROI.Height / 2)));

                    Rectangle[] rightEyes = eyeClassifier.DetectMultiScale(
                        grayRightEyeFrame,
                        1.2,
                        5,
                        new Size((faceROI.Width / 7), (faceROI.Height / 2)));

                    if (lefEyes.Length > 0)
                    {
                        var lefEye = lefEyes[0];

                        var eyeroi = new Rectangle(faceROI.Left + lefEye.Left + 15,
                            faceROI.Top + lefEye.Top + 15,
                            lefEye.Width - 15,
                            lefEye.Height - 15);

                        var tmp = grayImage.Copy(eyeroi);
                        CvInvoke.Imshow("ciao1", tmp);

                        image.Draw(eyeroi, new Bgr(Color.Violet), 2);

                        var save = grayImage.Copy(eyeroi);
                        var abc = save.SmoothGaussian(settingsForm.SmoothGaussian);
                        var efg = abc.ThresholdBinary(new Gray(settingsForm.ThresholdBinary1), new Gray(settingsForm.ThresholdBinary2));

                        CvInvoke.Imshow("ciao2", efg);

                        var contours = new VectorOfVectorOfPoint();
                        CvInvoke.FindContourTree(efg, contours, ChainApproxMethod.ChainApproxSimple);

                        var area = double.MaxValue;
                        var index = 0;
                        for (int i = 0; i < contours.Size; i++)
                        {
                            double tmpArea = CvInvoke.ContourArea(contours[i], false);
                            Debug.WriteLine("Area: " + tmpArea);

                            if (tmpArea < area)
                            {
                                area = tmpArea;
                                index = i;
                            }
                        }
                        Debug.WriteLine("Smallest: " + area);

                        var rect = CvInvoke.BoundingRectangle(contours[index]);
                        Debug.WriteLine("Rect: " + rect);

                        var roi = new Rectangle(eyeroi.Left + rect.Left,
                            eyeroi.Top + rect.Top,
                            rect.Width, rect.Height);

                        image.Draw(roi, new Bgr(Color.DarkSalmon), 3);

                        var center = new Point(roi.Left - roi.Width / 2,
                            roi.Top - roi.Height / 2);
                        var circle = new CircleF(center, 3);
                        image.Draw(circle, new Bgr(Color.White), 2);
                    }
                    if (rightEyes.Length > 0)
                    {
                        var rightEye = rightEyes[0];

                        var eyeroi = new Rectangle(faceROI.Left + faceROI.Width / 2 + rightEye.Left - 5,
                            faceROI.Top + rightEye.Top,
                            rightEye.Width + 5,
                            rightEye.Height + 5);

                        image.Draw(eyeroi, new Bgr(Color.Violet), 2);
                    }
                }

                pictureBox1.Image = image.ToBitmap();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            faceClassifier.Dispose();

            videoCapture.Dispose();

            CvInvoke.DestroyAllWindows();

            settingsForm.Dispose();
        }
    }
}