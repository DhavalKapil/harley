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

        public MainWindow()
        {
            InitializeComponent();

            Angle1.Text = TestKinectMeasurementsLib().ToString();

            InitializeKinect();
        }

        /// <summary>
        /// Called at the start when the window is loaded
        /// </summary>
        private void InitializeKinect()
        {
            string circleKBPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"data\circleKB.save");
            circleDetector = new TemplatedGestureDetector("Circle", File.Create(circleKBPath));
            circleDetector.DisplayCanvas = videoCanvas;
            circleDetector.OnGestureDetected += OnGesture;

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

                this.kinectSensor.Start();
            }

            if (null == this.kinectSensor)
            {
                // Connection is failed
                return;
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

        private void OnGesture(string gesture)
        {
            Angle1.Text = "Circle gesture detected";
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
