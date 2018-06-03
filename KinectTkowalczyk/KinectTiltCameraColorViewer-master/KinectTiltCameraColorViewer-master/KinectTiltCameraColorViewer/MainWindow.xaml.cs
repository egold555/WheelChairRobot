using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace KinectTiltCameraColorViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kscSensorChooser.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kscSensorChooser_KinectSensorChanged);
        }

        void kscSensorChooser_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Choose sensor to use
            //KinectSensor sensor = (KinectSensor)e.OldValue;
            KinectSensor sensor = (KinectSensor)e.NewValue;
            if (sensor == null)
            {
                return;
            }

            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

            try
            {
                sensor.Start();
            }
            catch (System.IO.IOException)
            {
                kscSensorChooser.AppConflictOccurred();
            }
        }

        private void StopKinect(KinectSensor sensorToStop)
        {
            if (sensorToStop != null)
            {
                if (sensorToStop.IsRunning)
                {
                    sensorToStop.Stop();

                    if (sensorToStop.AudioSource != null)
                    {
                        sensorToStop.AudioSource.Stop();
                    }


                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopKinect(kcvColorViewer.Kinect);
        }
    }
}
