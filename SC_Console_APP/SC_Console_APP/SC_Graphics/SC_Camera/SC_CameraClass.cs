﻿using SharpDX;

namespace SC_WPF_RENDER.SC_Graphics.Camera
{
    public class DCamera                    // 53 lines
    {
        // Properties.
        private float PositionX { get; set; }
        private float PositionY { get; set; }
        private float PositionZ { get; set; }
        private float RotationX { get; set; }
        private float RotationY { get; set; }
        private float RotationZ { get; set; }
        public Matrix ViewMatrix { get; set; }
        public Matrix rotationMatrix { get;  set; }

        // Constructor
        public DCamera() { }

        public Vector3 GetPosition()
        {
            return new Vector3(PositionX, PositionY, PositionZ);
        }
        // Methods.
        public void SetPosition(float x, float y, float z)
        {
            PositionX = x;
            PositionY = y;
            PositionZ = z;
        }


        public Vector3 GetRotation()
        {
            return new Vector3(RotationX, RotationY, RotationZ);
        }

        public void SetRotation(float x, float y, float z)
        {
            RotationX = x;
            RotationY = y;
            RotationZ = z;
        }


        public void Render()
        {
            // Setup the position of the camera in the world.
            //Vector3 position = new Vector3(PositionX, PositionY, PositionZ);

            // Setup where the camera is looking by default.
            //Vector3 lookAt = new Vector3(0, 0, -1);

            // Set the yaw (Y axis), pitch (X axis), and roll (Z axis) rotations in radians.
            //float pitch = RotationX * 0.0174532925f;
            //float yaw = RotationY * 0.0174532925f;
            //float roll = RotationZ * 0.0174532925f;

            // Create the rotation matrix from the yaw, pitch, and roll values.
            //rotationMatrix = Matrix.RotationYawPitchRoll(yaw, pitch, roll);

            //rotationMatrix.Invert();


            // Transform the lookAt and up vector by the rotation matrix so the view is correctly rotated at the origin.
            /*lookAt = Vector3.TransformCoordinate(lookAt, rotationMatrix);
            Vector3 up = Vector3.TransformCoordinate(Vector3.UnitY, rotationMatrix);

            // Translate the rotated camera position to the location of the viewer.
            lookAt = position + lookAt;

            // Finally create the view matrix from the three updated vectors.
            ViewMatrix = Matrix.LookAtRH(position, lookAt, up);*/
        }
    }
}