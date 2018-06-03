KinectTiltCameraColorViewer
============================

This simple application shows how to properly use [Kinect for Windows SDK](http://www.microsoft.com/en-us/kinectforwindows/ "Kinect for Windows SDK") in the WPF with data from RGB camera using KinectColorViewer control (from MSFT).

Kinect Color Viewer is a special WPF control which helps using data from RGB camera in applicaion. It is ver easy to use, specially with [Kinect Sensor Chooser](https://github.com/tkowalczyk/KinectSimpleAppChooser "Kinect Sensor Chooser").

**How does it work?**

In `*.xaml` file we have to define a namespace where this control is stored.

`xmlns:my="clr-namespace:KinectTiltCameraColorViewer"`

And next call it:

`<my:KinectColorViewer x:Name="kcvColorViewer" Height="240" Width="320" Kinect="{Binding ElementName=kscSensorChooser, Path=Kinect}" />`

The last thing we need to do is couple lines of code in code-behind:

`kscSensorChooser.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kscSensorChooser_KinectSensorChanged);`

Enabled color data:

`sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);`

Good practice is also stop the device in `Window_Closing` event method:

`StopKinect(kcvColorViewer.Kinect);`

**More examples**

Feel free to visit my homepage [Tomasz Kowalczyk](http://tomek.kownet.info/ "Tomasz Kowalczyk") to see more complex examples.