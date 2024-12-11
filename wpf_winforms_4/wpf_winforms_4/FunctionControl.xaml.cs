using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace wpf_winforms_4
{
    public partial class FunctionControl : UserControl
    {
        private readonly IFunction function;

        public FunctionControl(IFunction function, System.Windows.UIElement tabSelectedFunction)
        {
            this.function = function;
            InitializeComponent();

            PerspectiveCamera camera = new PerspectiveCamera()
            {
                FieldOfView = 80
            };
            new CameraClontrols(camera, viewport, tabSelectedFunction, light);

            //axies
            DisplayModel(Cube(new Point3D(1000, 0.02, 0.02), new Point3D(0, 0, 0), Colors.Black)); //  Y |
            DisplayModel(Cube(new Point3D(0.02, 0.02, 1000), new Point3D(0, 0, 0), Colors.Black)); //    |___ X
            DisplayModel(Cube(new Point3D(0.02, 1000, 0.02), new Point3D(0, 0, 0), Colors.Black)); // Z /
            //skybox
            DisplayModel(Cube(new Point3D(1000, 1000, 1000), new Point3D(0, 0, 0), Colors.White));

            DisplayModel(Surface(10));
        }


        /// <summary>
        /// Gradient
        /// </summary>
        private double MinFuncValue(int boundsRadius)
        {
            double minFuncVale = int.MaxValue;

            Point maxPoint = new Point(boundsRadius, boundsRadius);
            Point minPoint = new Point(-boundsRadius, -boundsRadius);

            //<= because we need to calculate the values in all boundaries
            for (double x = minPoint.X; x <= maxPoint.X; x++)
            {
                for (double z = minPoint.Y; z <= maxPoint.Y; z++)
                {
                    double funcValue = function.Calculate(x, z);

                    if (funcValue < minFuncVale)
                        minFuncVale = funcValue;
                }
            }

            return minFuncVale;
        }

        private double MaxFuncValue(int boundsRadius)
        {
            double maxFuncValue = int.MinValue;

            Point maxPoint = new Point(boundsRadius, boundsRadius);
            Point minPoint = new Point(-boundsRadius, -boundsRadius);

            //<= because we need to calculate the values in all boundaries
            for (double x = minPoint.X; x <= maxPoint.X; x++)
            {
                for (double z = minPoint.Y; z <= maxPoint.Y; z++)
                {
                    if (function.Calculate(x, z) > maxFuncValue)
                        maxFuncValue = function.Calculate(x, z);
                }
            }

            return maxFuncValue;
        }

        private PointCollection MeshTextureCoordinates(int boundsRadius, double detail)
        {
            Point maxPoint = new Point(boundsRadius, boundsRadius);
            Point minPoint = new Point(-boundsRadius, -boundsRadius);

            PointCollection meshTextureCoordinates = new PointCollection();

            //<= because we need to calculate the values in all boundaries
            for (double x = minPoint.X; x <= maxPoint.X; x += 1 / detail)
            {
                for (double z = minPoint.Y; z <= maxPoint.Y; z += 1 / detail)
                {
                    double funcValue = function.Calculate(x, z);

                    Point point = new Point()
                    {
                        X = (x - minPoint.X) / (maxPoint.X - minPoint.X),
                        Y = (funcValue - MinFuncValue(boundsRadius)) / (MaxFuncValue(boundsRadius) - MinFuncValue(boundsRadius))
                    };

                    meshTextureCoordinates.Add(point);
                }
            }

            return meshTextureCoordinates;
        }

        private DiffuseMaterial DiffuseMaterialGradient()
        {
            System.Drawing.Bitmap gradient = new System.Drawing.Bitmap(Properties.Resources.gradient);

            ImageBrush imageBrush = new ImageBrush()
            {
                ImageSource = Imaging.CreateBitmapSourceFromHBitmap(gradient.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                Stretch = Stretch.Fill,
                TileMode = TileMode.Tile
            };

            return new DiffuseMaterial(imageBrush);
        }


        /// <summary>
        /// Display
        /// </summary>
        private void DisplayModel(GeometryModel3D recievedModel)
        {
            ModelVisual3D model = new ModelVisual3D
            {
                Content = recievedModel
            };

            visual.Children.Add(model);
        }


        /// <summary>
        /// Shapes (bigger "detail" value - better detail)
        /// </summary>
        private GeometryModel3D Surface(int boundsRadius, double detail = 1)
        {
            int meshMatrixRang = Convert.ToInt32(2 * boundsRadius * detail);
            Point maxPoint = new Point(boundsRadius, boundsRadius);
            Point minPoint = new Point(-boundsRadius, -boundsRadius);

            MeshGeometry3D mesh = new MeshGeometry3D();

            //<= because we need to calculate the values in all boundaries
            for (double x = minPoint.X; x <= maxPoint.X; x += 1 / detail)
            {
                for (double z = minPoint.Y; z <= maxPoint.Y; z += 1 / detail)
                {
                    Point3D meshPoint = new Point3D(x, function.Calculate(x, z), z);
                    mesh.Positions.Add(meshPoint);
                }
            }

            for (int x = 0; x < meshMatrixRang; x++)
            {
                for (int z = 0; z < meshMatrixRang; z++)
                {
                    int p1 = x * (meshMatrixRang + 1) + z;
                    int p2 = x * (meshMatrixRang + 1) + z + 1;
                    int p3 = (x + 1) * (meshMatrixRang + 1) + z;
                    int p4 = (x + 1) * (meshMatrixRang + 1) + z + 1;

                    mesh.TriangleIndices.Add(p1); //    |  /
                    mesh.TriangleIndices.Add(p2); //    | /
                    mesh.TriangleIndices.Add(p3); //    |/

                    mesh.TriangleIndices.Add(p3); //      /|
                    mesh.TriangleIndices.Add(p2); //     / |
                    mesh.TriangleIndices.Add(p4); //    /  |
                }
            }

            mesh.TextureCoordinates = MeshTextureCoordinates(boundsRadius, detail);

            GeometryModel3D surfaceModel = new GeometryModel3D()
            {
                Geometry = mesh,
                Material = DiffuseMaterialGradient(),
                BackMaterial = DiffuseMaterialGradient()
            };

            return surfaceModel;
        }

        public GeometryModel3D Cube(Point3D sidesLength, Point3D center, Color color)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            
            mesh.Positions.Add(new Point3D(center.X - sidesLength.X / 2, center.Y - sidesLength.Y / 2, center.Z - sidesLength.Z / 2));
            mesh.Positions.Add(new Point3D(center.X + sidesLength.X / 2, center.Y - sidesLength.Y / 2, center.Z - sidesLength.Z / 2));
            mesh.Positions.Add(new Point3D(center.X - sidesLength.X / 2, center.Y + sidesLength.Y / 2, center.Z - sidesLength.Z / 2));
            mesh.Positions.Add(new Point3D(center.X + sidesLength.X / 2, center.Y + sidesLength.Y / 2, center.Z - sidesLength.Z / 2));
            mesh.Positions.Add(new Point3D(center.X - sidesLength.X / 2, center.Y - sidesLength.Y / 2, center.Z + sidesLength.Z / 2));
            mesh.Positions.Add(new Point3D(center.X + sidesLength.X / 2, center.Y - sidesLength.Y / 2, center.Z + sidesLength.Z / 2));
            mesh.Positions.Add(new Point3D(center.X - sidesLength.X / 2, center.Y + sidesLength.Y / 2, center.Z + sidesLength.Z / 2));
            mesh.Positions.Add(new Point3D(center.X + sidesLength.X / 2, center.Y + sidesLength.Y / 2, center.Z + sidesLength.Z / 2));

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1);

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(2);

            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(6);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(4);

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(4);

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(5);

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(7);

            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(6);

            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(5);

            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(3);

            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(7);

            GeometryModel3D model = new GeometryModel3D()
            {
                Geometry = mesh,
                Material = new DiffuseMaterial(new SolidColorBrush(color)),
                BackMaterial = new DiffuseMaterial(new SolidColorBrush(color))
            };

            return model;
        }
    }
}