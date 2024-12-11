using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace wpf_winforms_4
{
    internal class CameraClontrols
    {
        private const double deltaPhi = Math.PI / 60;
        private const double deltaTheta = Math.PI / 60;

        public PerspectiveCamera camera;
        private readonly UIElement window;
        private readonly DirectionalLight light;

        public Point3D position;
        public double radius = 40;
        public double phi = 1;
        public double theta = 180;

        private Point mouseLastPosition;
        private Point mouseCurrentPosition;

        public CameraClontrols(PerspectiveCamera camera, Viewport3D viewport, UIElement window, DirectionalLight light)
        {
            this.camera = camera;
            viewport.Camera = camera;
            this.window = window;
            this.light = light;

            this.window.MouseRightButtonDown += Window_MouseRightButtonDown;
            this.window.MouseWheel += Window_MouseWheel;

            CalculateCameraPosition();
        }

        /// <summary>
        /// Position
        /// </summary>
        private void CalculateCameraPosition()
        {
            Point3D newPosition = new Point3D()
            {
                X = radius * Math.Sin(phi) * Math.Sin(theta),
                Y = radius * Math.Cos(phi),
                Z = radius * Math.Sin(phi) * Math.Cos(theta)
            };

            camera.Position = newPosition;
            camera.LookDirection = new Vector3D(-newPosition.X, -newPosition.Y, -newPosition.Z);

            light.Direction = new Vector3D(-newPosition.X, -newPosition.Y, -newPosition.Z);
        }


        /// <summary>
        /// Movement and scaling
        /// </summary>
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0 && radius < 100)
                radius *= 1.1f;
            else if (e.Delta > 0 && radius > 10)
                radius /= 1.1f;

            CalculateCameraPosition();
        }

        private void Window_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            if (!window.CaptureMouse())
                return;

            
            window.MouseRightButtonUp += Window_MouseRightButtonUp;
            window.MouseMove += Window_MouseMove;

            mouseCurrentPosition = e.GetPosition(window);
        }

        private void Window_MouseRightButtonUp(object sender, MouseEventArgs e)
        {
            window.ReleaseMouseCapture();
            window.MouseRightButtonUp -= Window_MouseRightButtonUp;
            window.MouseMove -= Window_MouseMove;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            mouseLastPosition = mouseCurrentPosition;
            mouseCurrentPosition = e.GetPosition(window);

            Point mouseDelta = new Point()
            {
                X = mouseCurrentPosition.X - mouseLastPosition.X, 
                Y = mouseCurrentPosition.Y - mouseLastPosition.Y
            };

            double rotationSpeed = 0.1f;
            theta -= mouseDelta.X * deltaTheta * rotationSpeed;
            phi -= mouseDelta.Y * deltaPhi * rotationSpeed;

            CalculateCameraPosition();
        }
    }
}