using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsFaceTest
{
    public partial class form1 : Form
    {

        private Face face;

        public form1()
        {
            InitializeComponent();
            face = new Face(screen);
            
        }

        private void screen_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            
            //Image img = Image.FromFile(@"C:\Users\Eric\Documents\Face.png");
            //float centerX = screen.Width / 2, centerY = screen.Height / 2;
            //g.DrawImage(img, new PointF(centerX - img.Width/2, centerY - img.Height/2));
            
            face.draw(g);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (face.tick()) {
                Invalidate(true);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            face.speak("Hello there Human", delegate {
                Console.WriteLine("Finished");
               // Invalidate(true);
            });
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            float rot = ((TrackBar)sender).Value;
            float ten = face.getFacialExpression().leftEyeBrowTension;
            face.setFacialExpression(new FacialExpression(rot, rot, ten, ten));
            Invalidate(true);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            float rot = face.getFacialExpression().leftEyeBrowRotation;
            float ten = ((TrackBar)sender).Value * 0.1f;
            Console.WriteLine("Tension: " + ten);
            face.setFacialExpression(new FacialExpression(rot, rot, ten, ten));
            Invalidate(true);
        }
    }
}
