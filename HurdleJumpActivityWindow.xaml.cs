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

namespace Harley
{
    /// <summary>
    /// Interaction logic for HurdleJumpActivityWindow.xaml
    /// </summary>
    public partial class HurdleJumpActivityWindow : Window
    {
        public HurdleJumpActivityWindow()
        {
            InitializeComponent();
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
    }
}
