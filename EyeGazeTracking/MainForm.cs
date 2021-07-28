using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
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

        private void Form1_Load(object sender, System.EventArgs e)
        {
            videoCapture = new VideoCapture();
            videoCapture.Set(CapProp.FrameWidth, 1920);     // 1080
            videoCapture.Set(CapProp.FrameHeight, 1080);    // 720
            videoCapture.FlipHorizontal = true;

            faceClassifier = new CascadeClassifier("face.xml");
            eyeClassifier = new CascadeClassifier("eye.xml");

            Application.Idle += Application_Idle;

            settingsForm = new SettingsForm();
            settingsForm.Show(this);
        }

        private void Application_Idle(object sender, System.EventArgs e)
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

                        image.Draw(eyeroi, new Bgr(Color.Violet), 2);

                        var save = grayImage.Copy(eyeroi);
                        var abc = save.SmoothGaussian(settingsForm.SmoothGaussian);
                        var efg = abc.ThresholdBinary(new Gray(settingsForm.ThresholdBinary1), new Gray(settingsForm.ThresholdBinary2));

                        var contours = new VectorOfVectorOfPoint();
                        CvInvoke.FindContourTree(efg, contours, ChainApproxMethod.ChainApproxSimple);

                        var largest_area = double.MaxValue;
                        var largest = 0;
                        for (int i = 0; i < contours.Size; i++)
                        {
                            double a = CvInvoke.ContourArea(contours[i], false);
                            Debug.WriteLine("Area: " + a);

                            if (a < largest_area)
                            {
                                largest_area = a;
                                largest = i;
                            }
                        }
                        Debug.WriteLine("Smallest: " + largest_area);

                        var rect = CvInvoke.BoundingRectangle(contours[largest]);
                        Debug.WriteLine("Rect: " + rect);

                        var roi = new Rectangle(eyeroi.Left + rect.Left,
                            eyeroi.Top + rect.Top,
                            rect.Width, rect.Height);

                        image.Draw(roi, new Bgr(Color.DarkSalmon), 5);
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