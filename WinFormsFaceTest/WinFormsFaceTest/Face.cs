using DSPUtil;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Media;
using System.Speech.Synthesis;
using System.Windows.Forms;
using Matrix = System.Drawing.Drawing2D.Matrix;

namespace WinFormsFaceTest
{

    class Face
    {

        private Color COLOR_BG_CENTER = ColorTranslator.FromHtml("#FFFDFF");
        private Color COLOR_BG_OUT = ColorTranslator.FromHtml("#C3C2C4");
        private Color COLOR_FACE = ColorTranslator.FromHtml("#3E3F3F");
        private Color COLOR_EYE_OUT = ColorTranslator.FromHtml("#3E3F3F");
        private Color COLOR_EYE_IN = ColorTranslator.FromHtml("#CD53C9");

        private const int SIZE_OUTTER_EYE = 37;
        private const int SIZE_INNER_EYE = SIZE_OUTTER_EYE / 2;

        private PictureBox screen;

        private FacialExpression facialExpression = FacialExpression.NEUTRAL;
        private Looking looking = Looking.STRAIGHT;

        public Face(PictureBox screen)
        {
            this.screen = screen;

            /*
            // Initialize a new instance of the SpeechSynthesizer.
            using (SpeechSynthesizer synth = new SpeechSynthesizer()) {

                // Output information about all of the installed voices. 
                Console.WriteLine("Installed voices -");
                foreach (InstalledVoice voice in synth.GetInstalledVoices()) {
                    VoiceInfo info = voice.VoiceInfo;
                    Console.WriteLine(" Voice Name: " + info.Name);
                }
            }
            */
        }

        public void draw(Graphics g)
        {
            //Gradient background
            Rectangle rc = new Rectangle(0, 0, screen.Width, screen.Width);
            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(rc);
            using (PathGradientBrush brush = new PathGradientBrush(path)) {
                brush.CenterColor = COLOR_BG_CENTER;
                brush.SurroundColors = new Color[] { COLOR_BG_OUT };
                g.FillRectangle(brush, rc);
            }

            
            drawCenteredArc(g, -137, -155, 100, -20, 23, COLOR_FACE, facialExpression.leftEyeBrowRotation, facialExpression.leftEyeBrowTension); //Left Eyebrow
            drawCenteredArc(g, 137, -155, 100, -20, 23, COLOR_FACE, 0, 0.9f); //Right Eyebrow

            drawEye(g, -120, -50, looking.eyeX, looking.eyeY); //Left Eye
            drawEye(g, 120, -50, looking.eyeX, looking.eyeY); //Right eye

            

            if (speaking) {
                //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

                //Pen pen = new Pen(Color.Red, 4);

                int sampleStart, sampleStop;
                //if (GetSampleRange(out sampleStart, out sampleStop)) {
                //    int width = screen.Width, height = screen.Height;
                //    int totalSamples = sampleStop - sampleStart;

                //    PointF lastPoint, currentPoint = new PointF();
                //    for (int x = 0; x < width; ++x) {
                //        lastPoint = currentPoint;

                //        int startSample = sampleStart + (int)((long)x * (long)totalSamples / width);
                //        int stopSample = sampleStart + (int)((long)(x + 1) * (long)totalSamples / width);

                //        double val = AverageSample(startSample, stopSample);
                //        int y = (int)Math.Round(height / 2 + (val * 400));

                //        currentPoint = new PointF(x, y);

                //        if (x > 0) {
                //            //g.DrawLine(pen, lastPoint, currentPoint);
                //        }
                //    }
                //}
                //else {
                //    //g.DrawLine(pen, 0, screen.Height / 2, screen.Width, screen.Height / 2);
                //}

                if (GetSampleRange(out sampleStart, out sampleStop)) {
                    double amplitude = AmplitudeSample(sampleStart, sampleStop);
                    drawCenteredArc(g, 0, 137, 118, (float) (amplitude * 60) + 10, 25, COLOR_FACE); //Mouth\
                } else {
                    //Stop flicker
                    drawCenteredArc(g, 0, 137, 118, 10, 25, COLOR_FACE); //Mouth
                }
            }
            else {
                drawCenteredArc(g, 0, 137, 118, 10, 25, COLOR_FACE); //Mouth
            }

        }

        private void drawEye(Graphics g, float offX, float offY)
        {
            drawEye(g, offX, offY, 0, 0);
        }

        private void drawEye(Graphics g, float offX, float offY, float inOffX, float inOffY)
        {
            drawFilledCircleCentered(g, offX, offY, SIZE_OUTTER_EYE, COLOR_EYE_OUT); //out
            drawFilledCircleCentered(g, offX + inOffX, offY + inOffY, SIZE_INNER_EYE, COLOR_EYE_IN); //in
        }

        private void drawCenteredArc(Graphics g, float offX, float offY, float size, float height, float penWidth, Color color)
        {
            drawCenteredArc(g, offX, offY, size, height, penWidth, color, 0, 0.5f);
        }

        private void drawCenteredArc(Graphics g, float offX, float offY, float size, float height, float penWidth, Color color, float rotation)
        {
            drawCenteredArc(g, offX, offY, size, height, penWidth, color, rotation, 0.5f);
        }

        private void drawCenteredArc(Graphics g, float offX, float offY, float size, float height, float penWidth, Color color, float rotation, float tension)
        {
            PointF ptStart = new PointF(screen.Width / 2 + offX - size / 2, screen.Height / 2 + offY);
            PointF ptEnd = new PointF(screen.Width / 2 + offX + size / 2, screen.Height / 2 + offY);
            PointF ptMiddle = new PointF(screen.Width / 2 + offX, screen.Height / 2 + offY + height / 2);

            Matrix m = new Matrix();
            m.RotateAt(rotation, ptMiddle);
            g.MultiplyTransform(m);

            Pen pen = new Pen(color, penWidth);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            g.DrawCurve(pen, new PointF[] { ptStart, ptMiddle, ptEnd }, tension);

            g.ResetTransform();

            pen.Dispose();
        }

        private void drawFilledCircleCentered(Graphics g, double offX, double offY, float radius, Color color)
        {
            float x = (float)(screen.Width / 2 + offX - radius);
            float y = (float)(screen.Height / 2 + offY - radius);
            SolidBrush brush = new SolidBrush(color);
            g.FillEllipse(brush, x, y, radius * 2, radius * 2);
            brush.Dispose();
        }

        public void setFacialExpression(FacialExpression expression)
        {
            this.facialExpression = expression;
        }

        public void setLooking(Looking where)
        {
            this.looking = where;
        }

        //Text to speech stuff
        public void speak(string text, Action whenDone = null)
        {
            string fileName = "tempfile.wav";
            File.Delete(fileName);

            stopSpeaking();

            var synth = new SpeechSynthesizer(); //Create one and not dispose of it? Might be better memory management
            //http://www.kobaspeech.com/en/download-voices
            try
            {
                synth.SelectVoice("Vocalizer Karen - English (Australia) For KobaSpeech 3");
            }
            catch (System.ArgumentException e)
            {
                //Incase they dont have the voice installed, we will just use the default microsoft david voice witch every computer should have
                synth.SelectVoice("Microsoft David Desktop");
            }
            synth.SetOutputToWaveFile(fileName);
            synth.Speak(text);
            synth.SetOutputToNull();

            SetWaveFile(fileName);

            SoundPlayer soundPlayer = new SoundPlayer(fileName);
            soundPlayer.Play();

            BeginDisplay(whenDone);

            soundPlayer.Dispose();
            synth.Dispose();
        }

        double[] samples;
        int sampleRate;
        Stopwatch stopwatch;
        bool speaking = false;
        Action whenDone;
        const long millisToDisplay = 30;

        private void SetWaveFile(string fileName)
        {
            WaveReader waveReader = new WaveReader(fileName);
            samples = (from samp in waveReader select samp[0]).ToArray();
            sampleRate = (int)waveReader.SampleRate;
            waveReader.Close();
        }

        public void BeginDisplay(Action whenDoneWithWaveFile)
        {
            whenDone = whenDoneWithWaveFile;

            stopwatch = new Stopwatch();
            stopwatch.Start();
            speaking = true;
        }

        private double AverageSample(int startSample, int stopSample)
        {
            double v = 0;

            if (startSample == stopSample)
                return samples[startSample];

            for (int i = startSample; i < stopSample; ++i) {
                v += samples[i];
            }

            return v / (stopSample - startSample);
        }

        private double AmplitudeSample(int startSample, int stopSample)
        {
            double v = 0;
            double average;

            if (startSample == stopSample)
                return 0;

            for (int i = startSample; i < stopSample; ++i) {
                v += samples[i];
            }

            average = v / (stopSample - startSample);

            double maxAmplitude = 0;
            for (int i = startSample; i < stopSample; ++i) {
                double amp = Math.Abs(samples[i] - average);
                maxAmplitude = Math.Max(amp, maxAmplitude);
            }

            return maxAmplitude;
        }


        // Get the range of samples we should display.
        private bool GetSampleRange(out int sampleStart, out int sampleStop)
        {
            long lengthOfSamples = ((long)sampleRate * millisToDisplay) / 1000;
            long startSample = ((long)sampleRate * stopwatch.ElapsedMilliseconds) / 1000;

            if (startSample + lengthOfSamples < (long)samples.Length) {
                sampleStart = (int)startSample;
                sampleStop = (int)(startSample + lengthOfSamples);
                return true;
            }
            else {
                sampleStart = sampleStop = 0;
                return false;
            }
        }

        public void stopSpeaking()
        {
            speaking = false;
            if (whenDone != null) {
                whenDone();
            }
            whenDone = null;
        }

        public bool tick()
        {
            if (samples != null) {
                long lengthOfSound = ((long)samples.Length * 1000 / sampleRate);

                if (stopwatch != null && stopwatch.IsRunning && stopwatch.ElapsedMilliseconds > lengthOfSound && whenDone != null) {
                    stopSpeaking();
                }

                return true;
            }
            return false;
        }
    }

    class Looking
    {
        public readonly float eyeX;
        public readonly float eyeY;

        public static readonly Looking STRAIGHT = new Looking(0, 0);
        public static readonly Looking LEFT = new Looking(-10, 0);
        public static readonly Looking RIGHT = new Looking(10, 0);
        public static readonly Looking UP = new Looking(0, -10);
        public static readonly Looking DOWN = new Looking(0, 10);

        public Looking(float eyeX, float eyeY)
        {
            this.eyeX = eyeX;
            this.eyeY = eyeY;
        }

    }

    class FacialExpression {

        public readonly float leftEyeBrowRotation;
        public readonly float leftEyeBrowTension;
        public readonly float rightEyeBrowRotation;
        public readonly float rightEyeBrowTension;

        public static readonly FacialExpression NEUTRAL = new FacialExpression(0, 0, 0.9f, 0.9f);

        public FacialExpression(float leftEyeBrowRotation, float rightEyeBrowRotation, float leftEyeBrowTension, float rightEyeBrowTension)
        {
            this.leftEyeBrowRotation = leftEyeBrowRotation;
            this.leftEyeBrowTension = leftEyeBrowTension;
            this.rightEyeBrowRotation = rightEyeBrowRotation;
            this.rightEyeBrowTension = rightEyeBrowTension;
        }

    }

}
