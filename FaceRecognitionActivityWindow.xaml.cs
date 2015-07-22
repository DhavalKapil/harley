// -----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Harley
{
    using System;
    using System.Windows;
    using System.Diagnostics;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    //using Kinect.Toolbox;
    //using Microsoft.Kinect.Toolkit.FaceTracking;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class FaceRecognitionActivityWindow : Window
    {
        /// <summary>
        /// The kinect sensor object
        /// </summary>

        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();
        private WriteableBitmap colorImageWritableBitmap;
        private byte[] colorImageData;
        private ColorImageFormat currentColorImageFormat = ColorImageFormat.Undefined;

        public FaceRecognitionActivityWindow()
        {
            Trace.WriteLine("init method start");
            InitializeComponent();

            //InitializeKinect(faceTrackingViewer);

            var faceTrackingViewerBinding = new Binding("Kinect") { Source = sensorChooser };
            faceTrackingViewer.SetBinding(FaceTrackingViewer.KinectProperty, faceTrackingViewerBinding);

            sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            Trace.WriteLine("init method");

            sensorChooser.Start();
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs kinectChangedEventArgs)
        {
            Trace.WriteLine("kinect changed!");
            KinectSensor oldSensor = kinectChangedEventArgs.OldSensor;
            KinectSensor newSensor = kinectChangedEventArgs.NewSensor;

            if (oldSensor != null)
            {
                Trace.WriteLine("old sensor");
                oldSensor.AllFramesReady -= KinectSensorOnAllFramesReady;
                oldSensor.ColorStream.Disable();
                oldSensor.DepthStream.Disable();
                oldSensor.DepthStream.Range = DepthRange.Default;
                oldSensor.SkeletonStream.Disable();
                oldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                oldSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            }

            if (newSensor != null)
            {
                try
                {
                    newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    newSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                    try
                    {
                        // This will throw on non Kinect For Windows devices.
                        newSensor.DepthStream.Range = DepthRange.Near;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        newSensor.DepthStream.Range = DepthRange.Default;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    newSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    Trace.WriteLine("before enable");
                    newSensor.SkeletonStream.Enable();
                    Trace.WriteLine("after enable");
                    
                    newSensor.AllFramesReady += KinectSensorOnAllFramesReady;
                }
                catch (InvalidOperationException)
                {
                    // This exception can be thrown when we are trying to
                    // enable streams on a device that has gone away.  This
                    // can occur, say, in app shutdown scenarios when the sensor
                    // goes away between the time it changed status and the
                    // time we get the sensor changed notification.
                    //
                    // Behavior here is to just eat the exception and assume
                    // another notification will come along if a sensor
                    // comes back.
                }
            }
        }


        private void WindowClosed(object sender, EventArgs e)
        {
            sensorChooser.Stop();
            faceTrackingViewer.Dispose();
        }

        private void KinectSensorOnAllFramesReady(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            Trace.WriteLine("All frames ready");
            using (var colorImageFrame = allFramesReadyEventArgs.OpenColorImageFrame())
            {
                Trace.WriteLine("open image frame");
                if (colorImageFrame == null)
                {
                    Trace.WriteLine("No color frame");
                    return;
                }

                // Make a copy of the color frame for displaying.
                var haveNewFormat = this.currentColorImageFormat != colorImageFrame.Format;
                if (haveNewFormat)
                {
                    this.currentColorImageFormat = colorImageFrame.Format;
                    this.colorImageData = new byte[colorImageFrame.PixelDataLength];
                    this.colorImageWritableBitmap = new WriteableBitmap(
                    colorImageFrame.Width, colorImageFrame.Height, 96, 96, PixelFormats.Bgr32, null);
                    Image.Source = this.colorImageWritableBitmap;
                    Trace.Write("have new fromat");
                }

                colorImageFrame.CopyPixelDataTo(this.colorImageData);
                this.colorImageWritableBitmap.WritePixels(
                    new Int32Rect(0, 0, colorImageFrame.Width, colorImageFrame.Height),
                    this.colorImageData,
                    colorImageFrame.Width * Bgr32BytesPerPixel,
                    0);
            }
        }

        private void ShapeDrawingLabel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow.SwitchToGestureActivityWindow();
            this.Close();
        }

        private void StarJumpLabel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow.SwitchToStarActivityWindow();
            this.Close();
        }

        private void HurdleJumpLabel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow.SwitchToHurdleJumpActivityWindow();
            this.Close();
        }

        private void KaraokeLabel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow.SwitchToKaraokeActivityWindow();
            this.Close();
        }

        private void DashboardLabel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow.SwitchToDashboardActivityWindow();
            this.Close();
        }
    }
}
