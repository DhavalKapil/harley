using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;
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
    /// Interaction logic for GestureActivityWindow.xaml
    /// </summary>
    public partial class GestureActivityWindow : Window
    {
        /// <summary>
        /// The kinect sensor object
        /// </summary>
        private KinectSensor kinectSensor;

        /// <summary>
        /// The circle gesture recognizer
        /// </summary>
        private TemplatedGestureDetector circleDetector;

        private SwipeGestureDetector gestureDetector;

        /// <summary>
        /// The speech object
        /// </summary>
        private Speech speech;

        private int levelNumber;

        private string currentLevel;

        private string levelForTimer;

        private List<string> levels;

        private const string CIRCLE = "Circle";
        private const string TRIANGLE = "Triangle";
        private const string SQUARE = "Square";
        
        /// <summary>
        /// Prompt interval for user inactivity
        /// </summary>
        private const int PROMPT_INTERVAL = 7000;

        private Timer timer;

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
        private readonly string[] grammar = { "pro", "bitch" };

        public GestureActivityWindow()
        {
            // set current level number
            this.levelNumber = 0;

            // set current level
            this.currentLevel = CIRCLE;
            
            // update all available levels
            levels = new List<string>();
            levels.Add(CIRCLE);
            levels.Add(TRIANGLE);
            levels.Add(SQUARE);

            this.levelForTimer = this.currentLevel;

            InitializeComponent();

            InitializeKinect();

            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(PromptUserForGesture);
            timer.Interval = PROMPT_INTERVAL; // in milliseconds
            timer.Start();

            this.playNextLevel(CIRCLE);
        }

        private void PromptUserForGesture(object source, ElapsedEventArgs e)
        {
            if (this.levelForTimer == this.currentLevel)
            {
                this.speech.Speak("You can do better. Try to immitate the symbol using your hand as shown.");
            }
            else
            { 
                this.levelForTimer = this.currentLevel;
            }
        }

        /// <summary>
        /// Called at the start when the window is loaded
        /// </summary>
        private void InitializeKinect()
        {
           using (Stream recordStream = File.Open(@"C:\Users\Abhi\Projects\harley\data\circleKB.save", FileMode.OpenOrCreate))
            {
                this.circleDetector = new TemplatedGestureDetector("Circle", recordStream);
                this.circleDetector.DisplayCanvas = videoCanvas;
                this.circleDetector.OnGestureDetected += OnHandGesture;
            }

           this.gestureDetector = new SwipeGestureDetector();
           //this.gestureDetector.DisplayCanvas = videoCanvas;
           this.gestureDetector.OnGestureDetected += OnHandGesture;
            
           ParallelCombinedGestureDetector parallelCombinedGestureDetector = new ParallelCombinedGestureDetector();
           parallelCombinedGestureDetector.OnGestureDetected += OnHandGesture;
           parallelCombinedGestureDetector.DisplayCanvas = videoCanvas;
           parallelCombinedGestureDetector.Add(circleDetector);
           parallelCombinedGestureDetector.Add(gestureDetector);

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
            //this.speech.Start();
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
            if (true || gesture == this.currentLevel)
            {
                this.timer.Stop();

                this.levelNumber++;

                if (this.levelNumber >= this.levels.Count())
                {
                    // stop the timer
                    this.timer.Stop();

                    // stop looking for hand gestures
                    this.circleDetector.OnGestureDetected -= OnHandGesture;
                    
                    // stop drawing red ellipses
                    this.circleDetector.DisplayCanvas = null;

                    this.speech.SpeakAsync("Very well! You have completed all the levels.");

                    return;
                }

                if (this.currentLevel == CIRCLE)
                {
                    // highlight the next exercise

                    Color tileFill = Color.FromRgb(76, 76, 76);
                    SolidColorBrush brush1 = new SolidColorBrush(tileFill);
                    CircleTile.Fill = brush1;

                    Color focusTileFill = Color.FromRgb(96, 96, 96);
                    SolidColorBrush brush2 = new SolidColorBrush(focusTileFill);
                    SwipeRightTile.Fill = brush2;

                    // display the large icon
                    ActivityImage.Source = new BitmapImage(new Uri(@"C:\Users\Abhi\Projects\harley\resources\images\gestures\triangle_big.png"));
                    ActivityLabel.Content = "Triangle";
                }
                else if(this.currentLevel == TRIANGLE)
                {
                    // highlight the next exercise

                    Color tileFill = Color.FromRgb(76, 76, 76);
                    SolidColorBrush brush1 = new SolidColorBrush(tileFill);
                    SwipeRightTile.Fill = brush1;

                    Color focusTileFill = Color.FromRgb(96, 96, 96);
                    SolidColorBrush brush2 = new SolidColorBrush(focusTileFill);
                    SwipeLeftTile.Fill = brush2;

                    ActivityImage.Source = new BitmapImage(new Uri(@"C:\Users\Abhi\Projects\harley\resources\images\gestures\square_big.png"));
                    ActivityLabel.Content = "Square";
                }

                this.currentLevel = this.levels.ElementAt(this.levelNumber);

                this.speech.SpeakAsync("Well done!");

                this.levelForTimer = this.currentLevel;
                this.timer.Start();

                this.playNextLevel(this.currentLevel);
            }
        }

        private void playNextLevel(string level)
        {
              this.speech.SpeakAsync("A " + level + " is shown, try drawing it by moving your right hand.");
        }

        /// <summary>
        /// Cleanup tasks related to kinect skeleton tracking and speech
        /// </summary>
        private void Window_Closing()
        {
            this.kinectSensor.Stop();
        }
    }
}
