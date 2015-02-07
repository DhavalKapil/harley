using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Diagnostics;
using KinectMeasurementsLib;

namespace Harley
{
    class Star
    {
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
        /// Function to check a paricular body whether it is a star or not
        /// </summary>
        /// <param name="skeleton"></param>
        /// <returns>Return's true if it is star shaped</returns>
        public static bool CheckSkeleton(Skeleton skeleton)
        {
            // Checking if left elbow is straight
            float leftElbowAngle = KinectMeasurementsTools.AngleBetweenJoints(skeleton, JointType.ElbowLeft, JointType.WristLeft, JointType.ShoulderLeft);
            if ( leftElbowAngle > MAX_ELBOW_ANGLE || leftElbowAngle < MIN_ELBOW_ANGLE)
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
            if (leftShoulderAngle<MIN_SHOULDER_ANGLE || leftShoulderAngle>MAX_SHOULDER_ANGLE)
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
    }
}
