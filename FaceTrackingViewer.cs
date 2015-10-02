// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FaceTrackingViewer.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Harley
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit.FaceTracking;

    using Point = System.Windows.Point;

    /// <summary>
    /// Class that uses the Face Tracking SDK to display a face mask for
    /// tracked skeletons
    /// </summary>
    public partial class FaceTrackingViewer : UserControl, IDisposable
    {
        public static readonly DependencyProperty KinectProperty = DependencyProperty.Register(
            "Kinect",
            typeof(KinectSensor),
            typeof(FaceTrackingViewer),
            new PropertyMetadata(
                null, (o, args) => ((FaceTrackingViewer)o).OnSensorChanged((KinectSensor)args.OldValue, (KinectSensor)args.NewValue)));

        private const uint MaxMissedFrames = 100;

        private readonly Dictionary<int, SkeletonFaceTracker> trackedSkeletons = new Dictionary<int, SkeletonFaceTracker>();

        private byte[] colorImage;

        private ColorImageFormat colorImageFormat = ColorImageFormat.Undefined;

        private short[] depthImage;

        private DepthImageFormat depthImageFormat = DepthImageFormat.Undefined;

        private bool disposed;

        private Skeleton[] skeletonData;

        private FaceRecognitionActivityWindow activityWindow;

        public FaceTrackingViewer(FaceRecognitionActivityWindow win)
        {
            this.activityWindow = win;
            //this.InitializeComponent();
        }

        ~FaceTrackingViewer()
        {
            this.Dispose(false);
        }

        public KinectSensor Kinect
        {
            get
            {
                return (KinectSensor)this.GetValue(KinectProperty);
            }

            set
            {
                this.SetValue(KinectProperty, value);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.ResetFaceTracking();

                this.disposed = true;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            foreach (SkeletonFaceTracker faceInformation in this.trackedSkeletons.Values)
            {
                faceInformation.DrawFaceModel(drawingContext);
            }
        }

        private void OnAllFramesReady(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            Trace.WriteLine("allframesready");
            ColorImageFrame colorImageFrame = null;
            DepthImageFrame depthImageFrame = null;
            SkeletonFrame skeletonFrame = null;

            try
            {
                Trace.WriteLine("try start");
                colorImageFrame = allFramesReadyEventArgs.OpenColorImageFrame();
                depthImageFrame = allFramesReadyEventArgs.OpenDepthImageFrame();
                skeletonFrame = allFramesReadyEventArgs.OpenSkeletonFrame();

                if (colorImageFrame == null || depthImageFrame == null || skeletonFrame == null)
                {
                    Trace.WriteLine("return from frameready");
                    return;
                }
                Trace.WriteLine("wayaround null");

                // Check for image format changes.  The FaceTracker doesn't
                // deal with that so we need to reset.
                if (this.depthImageFormat != depthImageFrame.Format)
                {
                    this.ResetFaceTracking();
                    this.depthImage = null;
                    this.depthImageFormat = depthImageFrame.Format;
                    Trace.WriteLine("depth image fromat no work");

                }

                if (this.colorImageFormat != colorImageFrame.Format)
                {
                    this.ResetFaceTracking();
                    this.colorImage = null;
                    this.colorImageFormat = colorImageFrame.Format;
                    Trace.WriteLine("color image fromat no work");

                }

                // Create any buffers to store copies of the data we work with
                if (this.depthImage == null)
                {
                    this.depthImage = new short[depthImageFrame.PixelDataLength];
                }

                if (this.colorImage == null)
                {
                    this.colorImage = new byte[colorImageFrame.PixelDataLength];
                }

                // Get the skeleton information
                if (this.skeletonData == null || this.skeletonData.Length != skeletonFrame.SkeletonArrayLength)
                {
                    this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                }
                Trace.WriteLine("starting copying of data");

                colorImageFrame.CopyPixelDataTo(this.colorImage);
                depthImageFrame.CopyPixelDataTo(this.depthImage);
                skeletonFrame.CopySkeletonDataTo(this.skeletonData);
                Trace.WriteLine("end copying of data");
                // Update the list of trackers and the trackers with the current frame information
                foreach (Skeleton skeleton in this.skeletonData)
                {
                    Trace.WriteLine("inside foreach loop");
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked
                        || skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                    {
                        Trace.WriteLine("iftracked");
                        // We want keep a record of any skeleton, tracked or untracked.
                        if (!this.trackedSkeletons.ContainsKey(skeleton.TrackingId))
                        {
                            this.trackedSkeletons.Add(skeleton.TrackingId, new SkeletonFaceTracker());
                            Trace.WriteLine("add skeleton tracking");
                        }

                        // Give each tracker the upated frame.
                        SkeletonFaceTracker skeletonFaceTracker;
                        if (this.trackedSkeletons.TryGetValue(skeleton.TrackingId, out skeletonFaceTracker))
                        {
                            Trace.WriteLine("before");
                            skeletonFaceTracker.OnFrameReady(this.Kinect, colorImageFormat, colorImage, depthImageFormat, depthImage, skeleton, this.activityWindow);
                            skeletonFaceTracker.LastTrackedFrame = skeletonFrame.FrameNumber;
                        }
                    }
                }

                this.RemoveOldTrackers(skeletonFrame.FrameNumber);

                this.InvalidateVisual();
            }
            finally
            {
                if (colorImageFrame != null)
                {
                    colorImageFrame.Dispose();
                }

                if (depthImageFrame != null)
                {
                    depthImageFrame.Dispose();
                }

                if (skeletonFrame != null)
                {
                    skeletonFrame.Dispose();
                }
            }
        }

        private void OnSensorChanged(KinectSensor oldSensor, KinectSensor newSensor)
        {
            if (oldSensor != null)
            {
                oldSensor.AllFramesReady -= this.OnAllFramesReady;
                this.ResetFaceTracking();
            }

            if (newSensor != null)
            {
                newSensor.AllFramesReady += this.OnAllFramesReady;
            }
        }

        public void OnSensorChanged2(KinectSensor oldSensor, KinectSensor newSensor)
        {
            if (oldSensor != null)
            {
                oldSensor.AllFramesReady -= this.OnAllFramesReady;
                this.ResetFaceTracking();
            }

            if (newSensor != null)
            {
                newSensor.AllFramesReady += this.OnAllFramesReady;
            }
        }

        /// <summary>
        /// Clear out any trackers for skeletons we haven't heard from for a while
        /// </summary>
        private void RemoveOldTrackers(int currentFrameNumber)
        {
            var trackersToRemove = new List<int>();

            foreach (var tracker in this.trackedSkeletons)
            {
                uint missedFrames = (uint)currentFrameNumber - (uint)tracker.Value.LastTrackedFrame;
                if (missedFrames > MaxMissedFrames)
                {
                    // There have been too many frames since we last saw this skeleton
                    trackersToRemove.Add(tracker.Key);
                }
            }

            foreach (int trackingId in trackersToRemove)
            {
                this.RemoveTracker(trackingId);
            }
        }

        private void RemoveTracker(int trackingId)
        {
            this.trackedSkeletons[trackingId].Dispose();
            this.trackedSkeletons.Remove(trackingId);
        }

        private void ResetFaceTracking()
        {
            foreach (int trackingId in new List<int>(this.trackedSkeletons.Keys))
            {
                this.RemoveTracker(trackingId);
            }
        }

        public class SkeletonFaceTracker : IDisposable
        {
            private static FaceTriangle[] faceTriangles;

            private EnumIndexableCollection<FeaturePoint, PointF> facePoints;

            private EnumIndexableCollection<AnimationUnit, float> AUs;

            private FaceTracker faceTracker;

            private bool lastFaceTrackSucceeded;

            private SkeletonTrackingState skeletonTrackingState;

            private int currentState;

            private int count;

            private Speech speech;

            private readonly string[] states = { "smiling", "sad", "angry" };

            public int LastTrackedFrame { get; set; }

            public SkeletonFaceTracker()
            {
                this.speech = new Speech(null, new string[0]{}, null);
                currentState = 0;
                count = 0;
            }

            public void Dispose()
            {
                if (this.faceTracker != null)
                {
                    this.faceTracker.Dispose();
                    this.faceTracker = null;
                }
            }

            public void DrawFaceModel(DrawingContext drawingContext)
            {
                if (!this.lastFaceTrackSucceeded || this.skeletonTrackingState != SkeletonTrackingState.Tracked)
                {
                    return;
                }

                var faceModelPts = new List<Point>();
                var faceModel = new List<FaceModelTriangle>();

                for (int i = 0; i < this.facePoints.Count; i++)
                {
                    faceModelPts.Add(new Point(this.facePoints[i].X + 0.5f, this.facePoints[i].Y + 0.5f));
                }

                foreach (var t in faceTriangles)
                {
                    var triangle = new FaceModelTriangle();
                    triangle.P1 = faceModelPts[t.First];
                    triangle.P2 = faceModelPts[t.Second];
                    triangle.P3 = faceModelPts[t.Third];
                    faceModel.Add(triangle);
                }

                var faceModelGroup = new GeometryGroup();
                for (int i = 0; i < faceModel.Count; i++)
                {
                    var faceTriangle = new GeometryGroup();
                    faceTriangle.Children.Add(new LineGeometry(faceModel[i].P1, faceModel[i].P2));
                    faceTriangle.Children.Add(new LineGeometry(faceModel[i].P2, faceModel[i].P3));
                    faceTriangle.Children.Add(new LineGeometry(faceModel[i].P3, faceModel[i].P1));
                    faceModelGroup.Children.Add(faceTriangle);
                }

                drawingContext.DrawGeometry(Brushes.LightYellow, new Pen(Brushes.LightYellow, 1.0), faceModelGroup);
            }

            /// <summary>
            /// Updates the face tracking information for this skeleton
            /// </summary>
            internal void OnFrameReady(KinectSensor kinectSensor, ColorImageFormat colorImageFormat, byte[] colorImage, DepthImageFormat depthImageFormat, short[] depthImage, Skeleton skeletonOfInterest, FaceRecognitionActivityWindow win)
            {
                if (CheckFace(kinectSensor, colorImageFormat, colorImage, depthImageFormat, depthImage, skeletonOfInterest))
                {
                    count++;
                }
                else count = 0;
                if (count == 1)
                {
                    count = 0;
                    currentState = (currentState + 1) % 3;
                    // highlight the next exercise


                    if (currentState == 0)
                    {
                        Color tileFill = Color.FromRgb(76, 76, 76);
                        SolidColorBrush brush1 = new SolidColorBrush(tileFill);
                        win.SadTile.Fill = brush1;
                        

                        Color focusTileFill = Color.FromRgb(96, 96, 96);
                        SolidColorBrush brush2 = new SolidColorBrush(focusTileFill);
                        win.HappyTile.Fill = brush2;

                        Color secTileFill = Color.FromRgb(76, 76, 76);
                        SolidColorBrush brush3 = new SolidColorBrush(tileFill);
                        win.AngryTile.Fill = brush3;

                        // display the large icon
                        win.ActivityImage.Source = new BitmapImage(new Uri(@"happy_big.png"));
                        win.ActivityLabel.Content = "Happy";
                    }
                    else if (currentState == 1)
                    {
                        Color tileFill = Color.FromRgb(96, 96, 96);
                        SolidColorBrush brush1 = new SolidColorBrush(tileFill);
                        win.SadTile.Fill = brush1;

                        Color focusTileFill = Color.FromRgb(76, 76, 76);
                        SolidColorBrush brush2 = new SolidColorBrush(focusTileFill);
                        win.HappyTile.Fill = brush2;

                        Color secTileFill = Color.FromRgb(76, 76, 76);
                        SolidColorBrush brush3 = new SolidColorBrush(tileFill);
                        win.AngryTile.Fill = brush3;

                        // display the large icon
                        win.ActivityImage.Source = new BitmapImage(new Uri(@"sad_big.png"));
                        win.ActivityLabel.Content = "Sad";
                    }
                    else if (currentState == 2)
                    {
                        Color tileFill = Color.FromRgb(76, 76, 76);
                        SolidColorBrush brush1 = new SolidColorBrush(tileFill);
                        win.SadTile.Fill = brush1;

                        Color focusTileFill = Color.FromRgb(76, 76, 76);
                        SolidColorBrush brush2 = new SolidColorBrush(focusTileFill);
                        win.HappyTile.Fill = brush2;

                        Color secTileFill = Color.FromRgb(96, 96, 96);
                        SolidColorBrush brush3 = new SolidColorBrush(tileFill);
                        win.AngryTile.Fill = brush3;

                        // display the large icon
                        win.ActivityImage.Source = new BitmapImage(new Uri(@"angry_big.png"));
                        win.ActivityLabel.Content = "Angry";
                    }


                    this.speech.SpeakAsync("Moving to next level");
                    // Notify to change face
                    Trace.WriteLine("Change state to: " + states[currentState]);
                }
            }

            private bool CheckFace(KinectSensor kinectSensor, ColorImageFormat colorImageFormat, byte[] colorImage, DepthImageFormat depthImageFormat, short[] depthImage, Skeleton skeletonOfInterest)
            {
                this.skeletonTrackingState = skeletonOfInterest.TrackingState;

                if (this.skeletonTrackingState != SkeletonTrackingState.Tracked)
                {
                    // nothing to do with an untracked skeleton.
                    return false;
                }

                if (this.faceTracker == null)
                {
                    try
                    {
                        this.faceTracker = new FaceTracker(kinectSensor);
                    }
                    catch (InvalidOperationException)
                    {
                        // During some shutdown scenarios the FaceTracker
                        // is unable to be instantiated.  Catch that exception
                        // and don't track a face.
                        Debug.WriteLine("AllFramesReady - creating a new FaceTracker threw an InvalidOperationException");
                        this.faceTracker = null;
                    }
                }

                if (this.faceTracker != null)
                {
                    FaceTrackFrame frame = this.faceTracker.Track(
                        colorImageFormat, colorImage, depthImageFormat, depthImage, skeletonOfInterest);

                    this.lastFaceTrackSucceeded = frame.TrackSuccessful;
                    if (this.lastFaceTrackSucceeded)
                    {
                        if (faceTriangles == null)
                        {
                            // only need to get this once.  It doesn't change.
                            faceTriangles = frame.GetTriangles();
                        }

                        //getting the Animation Unit Coefficients
                        this.AUs = frame.GetAnimationUnitCoefficients();
                        var jawLowerer = AUs[AnimationUnit.JawLower];
                        var browLower = AUs[AnimationUnit.BrowLower];
                        var browRaiser = AUs[AnimationUnit.BrowRaiser];
                        var lipDepressor = AUs[AnimationUnit.LipCornerDepressor];
                        var lipRaiser = AUs[AnimationUnit.LipRaiser];
                        var lipStretcher = AUs[AnimationUnit.LipStretcher];
                        //set up file for output
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter
                            (@"C:\Users\Public\data.txt"))
                        {
                            file.WriteLine("FaceTrack Data, started recording at " + DateTime.Now.ToString("HH:mm:ss tt"));
                        }

                        //here is the algorithm to test different facial features

                        //BrowLower is messed up if you wear glasses, works if you don't wear 'em

                        string state = "";

                        //surprised
                        if ((jawLowerer < 0.25 || jawLowerer > 0.25) && browLower < 0)
                        {
                            state = "surprised";
                        }
                        //smiling
                        if (lipStretcher > 0.4 || lipDepressor < 0)
                        {
                            state = "smiling";
                        }
                        //sad
                        if (browRaiser < 0 && lipDepressor > 0)
                        {
                            state = "sad";
                        }
                        //angry
                        if ((browLower > 0 && (jawLowerer > 0.25 || jawLowerer < -0.25)) ||
                            (browLower > 0 && lipDepressor > 0))
                        {
                            state = "angry";
                        }
                        //System.Diagnostics.Debug.WriteLine(browLower);

                        this.facePoints = frame.GetProjected3DShape();

                        if (states[currentState] == state)
                        {
                            Trace.WriteLine("Yo!");
                            return true;
                        }
                    }
                }

                return false;
            }

            private struct FaceModelTriangle
            {
                public Point P1;
                public Point P2;
                public Point P3;
            }
        }
    }
}