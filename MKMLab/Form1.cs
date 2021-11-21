using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MKMLab
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string[] files;

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog();
            if (FBD.ShowDialog() == DialogResult.OK)
            {
                string path = FBD.SelectedPath;
                files = Directory.GetFiles(path);
                groupBox1.Enabled = true;
                draw();
            }
        }

        private void draw()
        {
            if(radioButton1.Checked)
                pictureBox1.Image = DICOMHelper.MakeMIP(files,"MIP");
            else if(radioButton2.Checked)
                pictureBox1.Image = DICOMHelper.MakeMIP(files, "mIP");
            else
                pictureBox1.Image = DICOMHelper.MakeMIP(files, "Avg");
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                pictureBox1.Image = DICOMHelper.MakeMIP(files, "MIP");
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
                pictureBox1.Image = DICOMHelper.MakeMIP(files, "mIP");
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
                pictureBox1.Image = DICOMHelper.MakeMIP(files, "Avg");
        }
    }
}
