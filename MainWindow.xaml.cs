using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Microsoft.Kinect;
using Kinect.Toolbox;
using KinectMeasurementsLib;

namespace Harley
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The kinect sensor object
        /// </summary>
        private KinectSensor kinectSensor;

        /// <summary>
        /// The circle gesture recognizer
        /// </summary>
        private TemplatedGestureDetector circleDetector;

        /// <summary>
        /// The speech object
        /// </summary>
        private Speech speech;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;

        public MainWindow()
        {
            InitializeComponent();
            
            InitializeKinect();
        }

        /// <summary>
        /// Called at the start when the window is loaded
        /// </summary>
        private void InitializeKinect()
        {
           using (Stream recordStream = File.Open(@"C:\Users\Abhi\Projects\harley\data\squareKB.save", FileMode.OpenOrCreate))
            {
                this.circleDetector = new TemplatedGestureDetector("Square", recordStream);
                this.circleDetector.DisplayCanvas = videoCanvas;
                this.circleDetector.OnGestureDetected += OnHandGesture;
            }

            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.kinectSensor = potentialSensor;
                    break;
                }
            }

            if (null != this.kinectSensor)
            {
                // Turning on skeleton stream
                this.kinectSensor.SkeletonStream.Enable();
                this.kinectSensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Turn on the color stream to receive color frames
                this.kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.kinectSensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.kinectSensor.ColorStream.FrameWidth, this.kinectSensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.Image.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new color frame data
                this.kinectSensor.ColorFrameReady += this.SensorColorFrameReady;

                this.kinectSensor.Start();
            }

            if (null == this.kinectSensor)
            {
                // Connection is failed
                return;
            }

            this.speech = new Speech(this.kinectSensor);
            this.speech.Start();
        }

        /// <summary>
        /// Event handler for Kinect sensor's ColorFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        /// <summary>
        /// Function called whenever a new skeleton frame arrives
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.handleSkeleton(skel);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Function called whenever a skeleton is tracked
        /// </summary>
        /// <param name="skeleton"></param>
        private void handleSkeleton(Skeleton skeleton)
        {
            this.circleDetector.Add(skeleton.Joints[JointType.HandRight].Position, this.kinectSensor);
        }

        private void OnHandGesture(string gesture)
        {
            GestureLabel.Content = gesture + " gesture detected!";
            Trace.WriteLine("Circle gesture detected!");
        }

        /// <summary>
        /// Spaghetti code to test working of KinectMeasurementsLib
        /// </summary>
        public float TestKinectMeasurementsLib()
        {
            KinectMeasurementsLib.Vector3 vec1 = new KinectMeasurementsLib.Vector3(1, 1, 0);
            KinectMeasurementsLib.Vector3 vec2 = new KinectMeasurementsLib.Vector3(1, 0, 0);
            float angle = KinectMeasurementsTools.AngleBetweenTwoVectors(vec1, vec2);

            return angle;
        }
    }

}
