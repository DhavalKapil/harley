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
        /// All tracked bodies
        /// </summary>
        private Body[] bodies;

        /// <summary>
        /// The body frame reader
        /// </summary>
        private BodyFrameReader bodyReader;

        /// <summary>
        /// The circle gesture recognizer
        /// </summary>
        private TemplatedGestureDetector circleDetector;

        public MainWindow()
        {
            InitializeComponent();

            Angle1.Text = TestKinectMeasurementsLib().ToString();

            InitializeKinect();

            // Close Kinect when closing app
            Closing += OnClosing;
        }

        /// <summary>
        /// Called at the start when the window is loaded
        /// </summary>
        private void InitializeKinect()
        {
            kinectSensor = KinectSensor.GetDefault();

            circleDetector = new TemplatedGestureDetector("Circle", File.Create(@"C:\Users\Abhi\Projects\KinectToolbox\GesturesViewer\Data\circleKB.save"));
            circleDetector.OnGestureDetected += OnGesture;

            // Initialize Body
            InitializeBody();
        }

        /// <summary>
        /// Function to initialize the body
        /// </summary>
        private void InitializeBody()
        {
            if (kinectSensor == null)
                return;

            bodies = new Body[kinectSensor.BodyFrameSource.BodyCount];

            bodyReader = kinectSensor.BodyFrameSource.OpenReader();

            bodyReader.FrameArrived += OnBodyFrameArrived;
        }

        /// <summary>
        /// Function called whenever a new body frame arrives
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            BodyFrameReference refer = e.FrameReference;

            if (refer == null)
                return;

            BodyFrame frame = refer.AcquireFrame();

            if (frame == null)
                return;

            using (frame)
            {
                // Acquiring body data
                frame.GetAndRefreshBodyData(bodies);

                foreach (var body in bodies)
                {
                    if (body != null)
                    {
                        if (body.IsTracked)
                        {
                            HandleBody(body);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Function called whenever a body is tracked
        /// </summary>
        /// <param name="body"></param>
        private void HandleBody(Body body)
        {

        }

        private void OnGesture(string gesture)
        {
            Angle1.Text = "Circle gesture detected";
        }

        /// <summary>
        /// Called when Application is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Close Kinect
            if (kinectSensor != null)
                kinectSensor.Close();
        }

        /// <summary>
        /// Spaghetti code to test working of KinectMeasurementsLib
        /// </summary>
        public float TestKinectMeasurementsLib()
        {
            Vector3 vec1 = new Vector3(1,1,0);
            Vector3 vec2 = new Vector3(1,0,0);
            float angle = KinectMeasurementsTools.AngleBetweenTwoVectors(vec1, vec2);

            return angle;
        }
    }

}
