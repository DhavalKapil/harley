using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Kinect.Toolbox;
using KinectMeasurementsLib;

namespace Harley
{
    /// <summary>
    /// Interaction logic for StarActivityWindow.xaml
    /// </summary>
    public partial class StarActivityWindow : Window
    {
        /// <summary>
        /// The kinect sensor object
        /// </summary>
        private KinectSensor kinectSensor;

        /// <summary>
        /// The speech object
        /// </summary>
        private Speech speech;

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
        /// The minimum threshold angle at the elbow formed by the arm
        /// </summary>
        private const float MIN_ELBOW_ANGLE = 160.0f;

        /// <summary>
        /// The maximum threshold angle at the elbow formed by the arm
        /// </summary>
        private const float MAX_ELBOW_ANGLE = 190.0f;

        /// <summary>
        /// The minimum angle formed at the shoulder between the arm and the body
        /// </summary>
        private const float MIN_SHOULDER_ANGLE = 100.0f;

        /// <summary>
        /// The maximum angle formed at the shoulder between the arm and the body
        /// </summary>
        private const float MAX_SHOULDER_ANGLE = 135.0f;

        /// <summary>
        /// Static variable to maintain the number of continuous successes
        /// </summary>
        private static int count = 0;

        /// <summary>
        /// The total no of continuous successes that would give a final success
        /// </summary>
        private const int COUNT_LIMIT = 20;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;

        /// <summary>
        /// Whether the user has completed the level or not
        /// </summary>
        private bool success;

        /// <summary>
        /// Constructor
        /// </summary>
        public StarActivityWindow()
        {
            
            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(PromptUserForShape);
            timer.Interval = TIMER_INTERVAL; // in milliseconds
            timer.Start();

            success = false;

            InitializeComponent();

            InitializeKinect();
        }

        /// <summary>
        /// Called at the start when the window is loaded
        /// </summary>
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
            this.speech = new Speech(this.kinectSensor, grammar, this);
            this.speech.Start();
        }

        /// <summary>
        /// Function called after every timer interval
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void PromptUserForShape(object source, ElapsedEventArgs e)
        {   
            this.speech.Speak("You can do better. Trying making the shape with your arms as shown on the screen.");
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
            if (!success && CheckStar(skeleton))
            {
                timer.Stop();
                this.speech.RestartSpeak();
                this.speech.Speak("Congratulations for finishing this level!");
                success = true;
            }
        }

        /// <summary>
        /// Function to check a paricular body whether it is a star or not
        /// </summary>
        /// <param name="skeleton"></param>
        /// <returns>Return's true if it is star shaped</returns>
        private static bool CheckSkeleton(Skeleton skeleton)
        {
            // Checking if left elbow is straight
            float leftElbowAngle = KinectMeasurementsTools.AngleBetweenJoints(skeleton, JointType.ElbowLeft, JointType.WristLeft, JointType.ShoulderLeft);
            if (leftElbowAngle > MAX_ELBOW_ANGLE || leftElbowAngle < MIN_ELBOW_ANGLE)
            {
                return false;
            }

            // Checking if right elbow is straight
            float rightElbowAngle = KinectMeasurementsTools.AngleBetweenJoints(skeleton, JointType.ElbowRight, JointType.WristRight, JointType.ShoulderRight);
            if (rightElbowAngle > MAX_ELBOW_ANGLE || rightElbowAngle < MIN_ELBOW_ANGLE)
            {
                return false;
            }

            // Checking if left should angle is correct
            float leftShoulderAngle = KinectMeasurementsTools.AngleBetweenJoints(skeleton, JointType.ShoulderLeft, JointType.HipLeft, JointType.ElbowLeft);
            if (leftShoulderAngle < MIN_SHOULDER_ANGLE || leftShoulderAngle > MAX_SHOULDER_ANGLE)
            {
                return false;
            }

            // Checking if right should angle is correct
            float rightShoulderAngle = KinectMeasurementsTools.AngleBetweenJoints(skeleton, JointType.ShoulderRight, JointType.HipRight, JointType.ElbowRight);
            if (rightShoulderAngle < MIN_SHOULDER_ANGLE || rightShoulderAngle > MAX_SHOULDER_ANGLE)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Function that returns true if COUNT_LIMIT consecutive frames are in perfect shape
        /// </summary>
        /// <param name="skeleton"></param>
        /// <returns></returns>
        private static bool CheckStar(Skeleton skeleton)
        {
            if (CheckSkeleton(skeleton))
                count++;
            else
                count = 0;

            if (count == COUNT_LIMIT)
                return true;
            return false;
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

        private void HurdleJumpLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.SwitchToHurdleJumpActivityWindow();
            this.Close();
        }

        private void KaraokeLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.SwitchToKaraokeActivityWindow();
            this.Close();
        }
    }
}
