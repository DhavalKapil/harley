using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using KinectMeasurementsLib;

namespace Harley
{
    class Star
    {
        /// <summary>
        /// The threshold angle at the elbow formed by the arm
        /// </summary>
        private const float ELBOW_ANGLE_THRESHOLD = 3.0f;

        /// <summary>
        /// The minimum angle formed at the shoulder between the arm and the body
        /// </summary>
        private const float MIN_SHOULDER_ANGLE = 117.0f;

        /// <summary>
        /// The maximum angle formed at the shoulder between the arm and the body
        /// </summary>
        private const float MAX_SHOULDER_ANGLE = 127.0f;

        /// <summary>
        /// Function to check a paricular body whether it is a star or not
        /// </summary>
        /// <param name="skeleton"></param>
        /// <returns>Return's true if it is star shaped</returns>
        public static bool CheckSkeleton(Skeleton skeleton)
        {
            // Checking if left elbow is straight
            if (KinectMeasurementsTools.AngleBetweenJoints(skeleton, JointType.ElbowLeft, JointType.WristLeft, JointType.ShoulderLeft) > ELBOW_ANGLE_THRESHOLD)
            {
                return false;
            }

            // Checking if right elbow is straight
            if (KinectMeasurementsTools.AngleBetweenJoints(skeleton, JointType.ElbowRight, JointType.WristRight, JointType.ShoulderRight) > ELBOW_ANGLE_THRESHOLD)
            {
                return false;
            }

            // Checking if left should angle is correct
            float leftShoulderAngle = KinectMeasurementsTools.AngleBetweenJoints(skeleton, JointType.ShoulderLeft, JointType.HipLeft, JointType.ElbowLeft);
            if (leftShoulderAngle<MIN_SHOULDER_ANGLE && leftShoulderAngle>MAX_SHOULDER_ANGLE)
            {
                return false;
            }

            // Checking if right should angle is correct
            float rightShoulderAngle = KinectMeasurementsTools.AngleBetweenJoints(skeleton, JointType.ShoulderRight, JointType.HipRight, JointType.ElbowRight);
            if (rightShoulderAngle < MIN_SHOULDER_ANGLE && rightShoulderAngle > MAX_SHOULDER_ANGLE)
            {
                return false;
            }

            return true;
        }
    }
}
