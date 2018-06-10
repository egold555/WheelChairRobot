using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KinectSkeletonWinForms
{
    public partial class Form1 : Form
    {
        //Width of output drawing
        private const float RenderWidth = 640.0f;

        //Height of our output drawing
        private const float RenderHeight = 480.0f;

        //Thickness of drawn joint lines
        private const float JointThickness = 3;

        //Thickness of body center ellipse
        private const float BodyCenterThickness = 10;

        //Thickness of clip edge rectangles
        private const float ClipBoundsThickness = 10;

        //Brush used to draw skeleton center point
        private readonly Brush centerPointBrush = Brushes.Blue;

        //Brush used for drawing joints that are currently tracked
        private readonly Brush trackedJointBrush = new SolidBrush(Color.FromArgb(255, 68, 192, 68));

        //Brush used for drawing joints that are currently inferred      
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        //Pen used for drawing bones that are currently tracked
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        //Pen used for drawing bones that are currently inferred   
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        //Active Kinect sensor
        private KinectSensor sensor;

        // Serial port to control the robot.
        SerialPort serialPort = new SerialPort();





        public Form1()
        {
            InitializeComponent();

            try {
                if (SerialPort.GetPortNames().Length == 0) {
                    Console.WriteLine("No Serial ports detected!");
                    this.statusBarText.Text = "No Serial ports detected!";

                }
                else {
                    serialPort.PortName = SerialPort.GetPortNames()[0];
                    serialPort.BaudRate = 57600;
                    //serialPort.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
                    serialPort.Open();
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                this.statusBarText.Text = "Serial port not opened!";
            }

        }


        //Draws indicators to show which edges are clipping skeleton data
        private static void RenderClippedEdges(Skeleton skeleton, Graphics graphics)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom)) {
                graphics.FillRectangle(
                    Brushes.Red,
                    new RectangleF(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top)) {
                graphics.FillRectangle(
                    Brushes.Red,
                    new RectangleF(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left)) {
                graphics.FillRectangle(
                    Brushes.Red,
                    new RectangleF(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right)) {
                graphics.FillRectangle(
                    Brushes.Red,
                    new RectangleF(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            foreach (var potentialSensor in KinectSensor.KinectSensors) {
                if (potentialSensor.Status == KinectStatus.Connected) {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor) {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try {
                    this.sensor.Start();
                    this.statusBarText.Text = "Status: Found Kinect!";
                }
                catch (IOException) {
                    this.sensor = null;
                }
            }

            if (null == this.sensor) {
                this.statusBarText.Text = "Status: No ready Kinect found!";
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (null != this.sensor) {
                this.sensor.Stop();
            }
        }

        RectangleF RectFromCenterSize(PointF center, float width, float height)
        {
            return new RectangleF(center.X - width / 2, center.Y - height / 2, width, height);
        }

        //Event handler for Kinect sensor's SkeletonFrameReady event
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) {
                if (skeletonFrame != null) {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (Graphics graphics = skelImage.CreateGraphics()) {
                graphics.Clear(Color.Black);

                if (skeletons.Length != 0) {
                    foreach (Skeleton skel in skeletons) {
                        RenderClippedEdges(skel, graphics);
                        TurnRobot(skel);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked) {
                            this.DrawBonesAndJoints(skel, graphics);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly) {
                            graphics.FillEllipse(
                            this.centerPointBrush,
                            RectFromCenterSize(
                                this.SkeletonPointToScreen(skel.Position),
                                BodyCenterThickness,
                                BodyCenterThickness));
                        }
                    }
                }
            }
        }

        int lastSent = 0;

        private void TurnRobot(Skeleton skel)
        {
            int sendValue = 100;

            if (skel.ClippedEdges.HasFlag(FrameEdges.Left)) {
                sendValue = 50;
            }
            else if (skel.ClippedEdges.HasFlag(FrameEdges.Right)) {
                sendValue = 150;
            }
            else {
                sendValue = 100;
            }

            if (lastSent != sendValue) {
                if (serialPort.IsOpen) {
                    serialPort.WriteLine("l" + sendValue);
                    this.statusBarText.Text = "Sending value " + sendValue;  
                }
                else {
                    this.statusBarText.Text = "Serial port is closed! Error!";
                }
                lastSent = sendValue;

            }
        }

        //Draws a skeleton's bones and joints
        private void DrawBonesAndJoints(Skeleton skeleton, Graphics graphics)
        {
            // Render Torso
            this.DrawBone(skeleton, graphics, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, graphics, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, graphics, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, graphics, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, graphics, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, graphics, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, graphics, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, graphics, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, graphics, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, graphics, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, graphics, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, graphics, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, graphics, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, graphics, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, graphics, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, graphics, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, graphics, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, graphics, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, graphics, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            foreach (Joint joint in skeleton.Joints) {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked) {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred) {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null) {
                    graphics.FillEllipse(drawBrush, 
                        RectFromCenterSize(this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness));
                }
            }
        }

        //Draws a bone line between two joints
        private PointF SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new PointF(depthPoint.X, depthPoint.Y);
        }

        //Draws a bone line between two joints
        private void DrawBone(Skeleton skeleton, Graphics graphics, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked || joint1.TrackingState == JointTrackingState.NotTracked) {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred && joint1.TrackingState == JointTrackingState.Inferred) {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked) {
                drawPen = this.trackedBonePen;
            }

            graphics.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

    }
}
