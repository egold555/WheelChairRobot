KinectTiltCamera
=================

This simple application shows how to properly use [Kinect for Windows SDK](http://www.microsoft.com/en-us/kinectforwindows/ "Kinect for Windows SDK") in the WPF with data from RGB camera. Attached example shows also how to use tilt to change elevation angle of the device.

RGB camera is one of components included in Kinect device. Data from this camera are commonly used for displaying frames in the WPF [Image](http://msdn.microsoft.com/en-us/library/system.windows.controls.image.aspx "Image") control.
Elevation angle property is used for setting vertical position of the device. Value can be set between -27 to +27 degree.

**How does it work? - RGB camera**

In `AllFramesReady` event method we have to use `ColorImageFrame` in this way:

`using (ColorImageFrame colorFrame = e.OpenColorImageFrame())`

Next, we have to store pixel data in array:

`byte[] pixels = new byte[colorFrame.PixelDataLength];`

and copy data for these pixels:

`colorFrame.CopyPixelDataTo(pixels);`

we also have to define stride:

`int stride = colorFrame.Width * 4;`

The last step is set source for the WPF Image control:

`imageRGB.Source =
                    BitmapSource.Create(colorFrame.Width,
                    colorFrame.Height,
                    96,
                    96,
                    PixelFormats.Bgr32,
                    null,
                    pixels,
                    stride);`

**How does it work? - Tilt ElevationAngle**

First step is to check if our sensor is running, so the best way to do it is just call:

`if (_sensor.IsRunning && _sensor != null)`

And use `ElevationAngle` property of the `KinectSensor` object this way:

`_sensor.ElevationAngle = 10;`

**More examples**

Feel free to visit my homepage [Tomasz Kowalczyk](http://tomek.kownet.info/ "Tomasz Kowalczyk") to see more complex examples.