KinectDepthApp
==============

This simple application shows how to properly use [Kinect for Windows SDK](http://www.microsoft.com/en-us/kinectforwindows/ "Kinect for Windows SDK") in the WPF with data from Depth camera.

Depth camera is one of components included in Kinect device. Using data from this camera you can easliy manage bytes defined for every pixel from the Kinect surrounded area. For example using this data per pixel you could calculate distance or player index.

Attached code will show you also how to colorize each pixel depending on it distance from Kinect.

**How does it work?**

In `AllFramesReady` event method we have to use `DepthImageFrame` in this way:

`using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())`

Next, we have to store pixel depth data in array:

`byte[] pixels = GenerateColoredBytes(depthFrame);`

In the `GenerateColoredBytes(DepthImageFrame depthFrame)` method we just work with depth data, by checking their distance. First of all we have to store data in the array:

`short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);`

We need player index:

`int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;`

And calculate distance for each player:

`int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;`

After these operaions we only need to figure out how to colorize the pixels:

`if (depth <= 900)
                {
                    pixels[colorIndex + BlueIndex] = 255;
                    pixels[colorIndex + GreenIndex] = 0;
                    pixels[colorIndex + RedIndex] = 0;
                }`

The last step is to set the source for the Image control defined in `*.xaml`:

`imageDepth.Source =
                    BitmapSource.Create(depthFrame.Width,
                    depthFrame.Height,
                    96,
                    96,
                    PixelFormats.Bgr32,
                    null,
                    pixels,
                    stride);`

**More examples**

Feel free to visit my homepage [Tomasz Kowalczyk](http://tomek.kownet.info/ "Tomasz Kowalczyk") to see more complex examples.