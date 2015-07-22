using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.IO;
using System.Diagnostics;
using System.Timers;
using Microsoft.Kinect;
using Kinect.Toolbox;
using KinectMeasurementsLib;

namespace Harley
{
    /// <summary>
    /// Interaction logic for HurdleJumpActivityWindow.xaml
    /// </summary>
    /// 
  
    public partial class HurdleJumpActivityWindow : Window
    {

        private KinectSensor kinectSensor;

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



        /// <summary>
        /// Readonly array of word list to recognize
        /// </summary>
        private readonly string[] grammar = { "some", "options" };

        /// <summary>
        /// The timer object
        /// </summary>
        private Timer timer;

        /// <summary>
        /// The timer interval
        /// </summary>
        private const int TIMER_INTERVAL = 8000;
        /// <summary>
        /// Whether the user has completed the level or not
        /// </summary>
        private bool success;
        public HurdleJumpActivityWindow()
        {
            timer = new Timer();
            timer.Interval = TIMER_INTERVAL; // in milliseconds
            timer.Start();

            success = false;
            InitializeComponent();
            InitializeKinect();
        }

        private void StarJumpLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.SwitchToStarActivityWindow();
            //this.speech.StopSpeak();
            this.Close();
        }

        private void ShapeDrawingLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.SwitchToGestureActivityWindow();
            //this.speech.StopSpeak();
            this.Close();
        }

        private void FacialExpressionLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.SwitchToFaceRecognitionActivityWindow();
            this.Close();
        }

        private void KaraokeLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.SwitchToKaraokeActivityWindow();
            this.Close();
        }

        private void DashboardLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.SwitchToDashboardActivityWindow();
            this.Close();
        }
        private void InitializeKinect()
        {
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
                this.kinectSensor.SkeletonStream.Enable();

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
            this.speech = new Speech(this.kinectSensor, grammar, this);
            this.speech.Start();
        }
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
    }
}
