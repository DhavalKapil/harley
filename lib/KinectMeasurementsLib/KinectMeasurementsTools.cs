using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Drawing;

namespace KinectMeasurementsLib
{
    /// <summary>
    /// Set of tools and extension methods to measure skeletons
    /// @author Tomasz Kowalczyk http://tomek.kownet.info
    /// </summary>
    public static class KinectMeasurementsTools
    {
        #region Distance
        /// <summary>
        /// Returns the distance of tracked body from Kinect device
        /// </summary>
        /// <param name="body">Body</param>
        /// <returns>Distance in meters</returns>
        public static double GetSkeletonDistance(this Body body)
        {
            Joint spine = body.Joints[JointType.SpineMid];

            return (spine.Position.X * spine.Position.X) +
                   (spine.Position.Y * spine.Position.Y) +
                   (spine.Position.Z * spine.Position.Z);
        }

        /// <summary>
        /// Returns the distance of specific joint from Kinect device
        /// </summary>
        /// <param name="body">Body</param>
        /// <param name="jointType">JointType</param>
        /// <returns>Distance in meters</returns>
        public static double GetJointDistance(this Body body, JointType jointType)
        {
            return (body.Joints[jointType].Position.X * body.Joints[jointType].Position.X) +
                   (body.Joints[jointType].Position.Y * body.Joints[jointType].Position.Y) +
                   (body.Joints[jointType].Position.Z * body.Joints[jointType].Position.Z);
        }

        /// <summary>
        /// Returns the distance between two specific joints
        /// </summary>
        /// <param name="body">Body</param>
        /// <param name="firstJointType">JointType</param>
        /// <param name="secondJointType">JointType</param>
        /// <returns>Distance in meters</returns>
        public static double GetDistanceBetweenJoints(this Body body, JointType firstJointType, JointType secondJointType)
        {
            return Math.Sqrt(
                Math.Pow((body.Joints[firstJointType].Position.X - body.Joints[secondJointType].Position.X), 2) +
                Math.Pow((body.Joints[firstJointType].Position.Y - body.Joints[secondJointType].Position.Y), 2) +
                Math.Pow((body.Joints[firstJointType].Position.Z - body.Joints[secondJointType].Position.Z), 2)
                );
        }
        #endregion

        #region Angles
        /// <summary>
        /// Measure angle between joints
        /// </summary>
        /// <param name="body">Body</param>
        /// <param name="centerJoint">joint which is in the middle</param>
        /// <param name="topJoint">joint which is on top</param>
        /// <param name="bottomJoint">joint which is on bottom</param>
        /// <returns>Angle in degrees</returns>
        public static float AngleBetweenJoints(this Body body, JointType centerJoint, JointType topJoint, JointType bottomJoint)
        {
            Vector3 centerJointCoord = new Vector3(body.Joints[centerJoint].Position.X, body.Joints[centerJoint].Position.Y, body.Joints[centerJoint].Position.Z);
            Vector3 topJointCoord = new Vector3(body.Joints[topJoint].Position.X, body.Joints[topJoint].Position.Y, body.Joints[topJoint].Position.Z);
            Vector3 bottomJointCoord = new Vector3(body.Joints[bottomJoint].Position.X, body.Joints[bottomJoint].Position.Y, body.Joints[bottomJoint].Position.Z);

            Vector3 firstVector = bottomJointCoord - centerJointCoord;
            Vector3 secondVector = topJointCoord - centerJointCoord;

            return AngleBetweenTwoVectors(firstVector, secondVector);
        }

        /// <summary>
        /// Measure angle between two vectors in 3D space.
        /// </summary>
        /// <param name="vectorA">first Vector3</param>
        /// <param name="vectorB">second Vector3</param>
        /// <returns>Angle in degrees</returns>
        public static float AngleBetweenTwoVectors(Vector3 vectorA, Vector3 vectorB)
        {
            vectorA.Normalize();
            vectorB.Normalize();

            float dotProduct = 0.0f;

            dotProduct = Vector3.Dot(vectorA, vectorB);

            return (float)Math.Round((Math.Acos(dotProduct) * 180 / Math.PI), 2);
        }
        #endregion

        /// <summary>
        /// Returns true if tracked Body has jumped
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static bool HasJumped(this Body body)
        {
            CameraSpacePoint position = body.Joints[JointType.FootLeft].Position;
            if (position.X > 0)
                return true;
            else return false;
        }

        #region Body height
        // from http://www.codeproject.com/Articles/380152/Kinect-for-Windows-Find-user-height-accurately

        public static double Height(this Body body)
        {
            const double HEAD_DIVERGENCE = 0.1;

            var head = body.Joints[JointType.Head];
            var neck = body.Joints[JointType.Neck];
            var spine = body.Joints[JointType.SpineMid];
            var waist = body.Joints[JointType.SpineBase];
            var hipLeft = body.Joints[JointType.HipLeft];
            var hipRight = body.Joints[JointType.HipRight];
            var kneeLeft = body.Joints[JointType.KneeLeft];
            var kneeRight = body.Joints[JointType.KneeRight];
            var ankleLeft = body.Joints[JointType.AnkleLeft];
            var ankleRight = body.Joints[JointType.AnkleRight];
            var footLeft = body.Joints[JointType.FootLeft];
            var footRight = body.Joints[JointType.FootRight];

            // Find which leg is tracked more accurately.
            int legLeftTrackedJoints = NumberOfTrackedJoints(hipLeft, kneeLeft, ankleLeft, footLeft);
            int legRightTrackedJoints = NumberOfTrackedJoints(hipRight, kneeRight, ankleRight, footRight);

            double legLength = legLeftTrackedJoints > legRightTrackedJoints ? Length(hipLeft, kneeLeft, ankleLeft, footLeft) : Length(hipRight, kneeRight, ankleRight, footRight);

            return Length(head, neck, spine, waist) + legLength + HEAD_DIVERGENCE;
        }

        private static double Length(Joint p1, Joint p2)
        {
            return Math.Sqrt(
                Math.Pow(p1.Position.X - p2.Position.X, 2) +
                Math.Pow(p1.Position.Y - p2.Position.Y, 2) +
                Math.Pow(p1.Position.Z - p2.Position.Z, 2)
                );
        }

        private static double Length(params Joint[] joints)
        {
            double length = 0;

            for (int index = 0; index < joints.Length - 1; index++)
            {
                length += Length(joints[index], joints[index + 1]);
            }

            return length;
        }

        private static int NumberOfTrackedJoints(params Joint[] joints)
        {
            int trackedJoints = 0;

            foreach (var joint in joints)
            {
                if (joint.TrackingState == Microsoft.Kinect.TrackingState.Tracked)
                {
                    trackedJoints++;
                }
            }

            return trackedJoints;
        }
        #endregion
    }
}