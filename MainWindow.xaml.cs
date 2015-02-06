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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using KinectMeasurementsLib;

namespace Harley
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Angle1.Text = TestKinectMeasurementsLib().ToString();
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
