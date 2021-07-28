using System;
using System.Windows.Forms;

namespace EyeGazeTracking
{
    public partial class SettingsForm : Form
    {
        public int SmoothGaussian { get; private set; }

        public double ThresholdBinary1 { get; private set; }
        public double ThresholdBinary2 { get; private set; }

        public double Canny1 { get; private set; }
        public double Canny2 { get; private set; }

        public SettingsForm()
        {
            InitializeComponent();

            textBox1.Text = "11";

            textBox2.Text = "30";
            textBox3.Text = "255";

            textBox4.Text = "30";
            textBox5.Text = "50";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
                SmoothGaussian = int.Parse(textBox1.Text);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox2.Text))
                ThresholdBinary1 = double.Parse(textBox2.Text);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox3.Text))
                ThresholdBinary2 = double.Parse(textBox3.Text);
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox4.Text))
                Canny1 = double.Parse(textBox4.Text);
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox5.Text))
                Canny2 = double.Parse(textBox5.Text);
        }
    }
}