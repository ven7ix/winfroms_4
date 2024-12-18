using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace wpf_winforms_4
{
    public partial class FunctionControl : UserControl
    {
        private readonly IFunction function;
        readonly private DiffuseMaterial materialGradient;
        readonly private int functionSize = 10;
        readonly double detail = 1;

        private readonly DispatcherTimer ballFallingTimer;
        private readonly DispatcherTimer graphFallingTimer;
        double graphYPos = 0;
        private Ball ball;

        private readonly List<Crack> cracks = new List<Crack>();
        private readonly List<Crack> crackPresets = new List<Crack>();

        public FunctionControl(IFunction function, UIElement tabSelectedFunction)
        {
            this.function = function;
            InitializeComponent();

            PerspectiveCamera camera = new PerspectiveCamera()
            {
                FieldOfView = 80
            };
            new CameraClontrols(camera, viewport, tabSelectedFunction, light);

            materialGradient = DiffuseMaterialGradient();

            //axies
            RenderModel(visualBasicStuff, Cube(new Point3D(1000, 0.02, 0.02), new Point3D()), new DiffuseMaterial(new SolidColorBrush(Colors.Black)), false); //  Y |
            RenderModel(visualBasicStuff, Cube(new Point3D(0.02, 0.02, 1000), new Point3D()), new DiffuseMaterial(new SolidColorBrush(Colors.Black)), false); //    |___ X
            RenderModel(visualBasicStuff, Cube(new Point3D(0.02, 1000, 0.02), new Point3D()), new DiffuseMaterial(new SolidColorBrush(Colors.Black)), false); // Z /
            //skybox
            RenderModel(visualBasicStuff, Cube(new Point3D(1000, 1000, 1000), new Point3D()), new DiffuseMaterial(new SolidColorBrush(Colors.White)), false);

            CreateCrackPresets();

            MouseLeftButtonDown += Window_MouseLeftButtonDown;
            RenderFunctionWithCubeAndCracks(functionSize, new DiffuseMaterial(new SolidColorBrush(Colors.Red)));

            ballFallingTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(10000)
            };

            graphFallingTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(10000)
            };

            ballFallingTimer.Tick += BallFallingTimer_Tick;
            graphFallingTimer.Tick += GraphFallingTimer_Tick;
        }


        /// <summary>
        /// Cracks & Balls
        /// </summary>
        struct Ball
        {
            public Point3D destination;
            public Point3D origin;
            public Point3D currentPosition;
            public double speed;

            public Ball(Point3D destination, Point3D origin, double speed)
            {
                this.destination = destination;
                this.origin = origin;
                currentPosition = origin;
                this.speed = speed;
            }

            public Ball(Ball ball)
            {
                destination = ball.destination;
                origin = ball.origin;
                currentPosition = ball.origin;
                speed = ball.speed;
            }
        }

        struct Crack
        {
            public List<Abs> functions;
            public readonly int presetIndex;

            public Crack(int presetIndex)
            {
                functions = new List<Abs>();
                this.presetIndex = presetIndex;
            }

            public Crack(Crack crack)
            {
                functions = new List<Abs>();
                foreach (Abs function in crack.functions)
                    functions.Add(new Abs(function));

                presetIndex = crack.presetIndex;
            }
        }

        private void BallFallingTimer_Tick(object sender, EventArgs e)
        {
            if (ball.currentPosition.Y < ball.destination.Y && ball.speed < 0 || ball.currentPosition.Y > ball.destination.Y && ball.speed > 0)
            {
                visualBalls.Children.Clear();
                ballFallingTimer.Stop();
                CracksHandler(ball.destination);
                RenderFunctionWithCubeAndCracks(functionSize, new DiffuseMaterial(new SolidColorBrush(Colors.Red)));
                return;
            }

            RenderModel(visualBalls, Sphere(1, 10, 10, ball.currentPosition), new DiffuseMaterial(new SolidColorBrush(Colors.Blue)), true);

            ball.currentPosition.Y += ball.speed;
        }

        private void GraphFallingTimer_Tick(object sender, EventArgs e)
        {
            graphYPos -= 1;
            RenderFunctionWithCubeAndCracks(functionSize, new DiffuseMaterial(new SolidColorBrush(Colors.Red)), new Point3D(0, graphYPos, 0));
        }

        private void CracksHandler(Point3D center)
        {
            for (int i = 0; i < cracks.Count; i++)
            {
                Crack c = cracks[i];
                Abs func = c.functions[0];

                if (Math.Sign(func.b) == Math.Sign(center.X) && Math.Sign(func.d) == Math.Sign(center.Z))
                {
                    Point3D oldCrackCenter = new Point3D(func.b, 0, func.d);

                    c = new Crack(crackPresets[c.presetIndex + 1]);
                    for (int j = 0; j < c.functions.Count; j++)
                    {
                        c.functions[j].b = oldCrackCenter.X;
                        c.functions[j].d = oldCrackCenter.Z;
                    }
                    cracks[i] = c;

                    if (c.presetIndex >= crackPresets.Count - 1)
                    {
                        MouseLeftButtonDown -= Window_MouseLeftButtonDown;
                        ballFallingTimer.Stop();
                        graphFallingTimer.Start();
                    }

                    return;
                }
            }

            Crack crack = new Crack(crackPresets[0]);

            for (int i = 0; i < crack.functions.Count; i++)
            {
                crack.functions[i].b = center.X;
                crack.functions[i].d = center.Z;
            }
            cracks.Add(crack);
        }
        
        private void Window_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (ballFallingTimer.IsEnabled)
                return;

            Point3D? posInGraph = GetRayIntersection(e.GetPosition(viewport));

            if (posInGraph != null)
            {
                Point3D notNullPos = (Point3D)posInGraph;

                if (Math.Abs(notNullPos.X) > 100 || Math.Abs(notNullPos.Y) > 100 || Math.Abs(notNullPos.Z) > 100)
                    return;

                ball = new Ball(notNullPos, new Point3D(notNullPos.X, notNullPos.Y + 10, notNullPos.Z), -0.5);
                ballFallingTimer.Start();
            }
        }

        private Point3D? GetRayIntersection(Point mousePosition)
        {
            PointHitTestParameters hitParams;

            hitParams = new PointHitTestParameters(new Point(mousePosition.X, mousePosition.Y));
            Point3D? hitPoint = null;

            VisualTreeHelper.HitTest(
                viewport,
                null,
                result =>
                {
                    if (result is RayMeshGeometry3DHitTestResult meshHitResult)
                    {
                        hitPoint = meshHitResult.PointHit;
                        if (hitPoint != null)
                        {
                            Point3D p = (Point3D)hitPoint;
                        }
                        return HitTestResultBehavior.Stop;

                    }
                    return HitTestResultBehavior.Continue;
                },
                hitParams
            );

            return hitPoint;
        }

        private void CreateCrackPresets()
        {
            Crack crack;

            crack = new Crack(0);
            crack.functions.Add(new Abs(10, 0, 1, 0, -6));
            crackPresets.Add(crack);

            crack = new Crack(1);
            crack.functions.Add(new Abs(10, 0, 1, 0, -4));
            crack.functions.Add(new Abs(1, -1, 2, -1, -8));
            crackPresets.Add(crack);

            crack = new Crack(2);
            crack.functions.Add(new Abs(10, 0, 1, 0, -6));
            crack.functions.Add(new Abs(1, -1, 2, -1, -9));
            crack.functions.Add(new Abs(1, 1, 5, 1, -10));
            crackPresets.Add(crack);

            crack = new Crack(3);
            crack.functions.Add(new Abs(15, 0, 1, 0, -6));
            crack.functions.Add(new Abs(1, -1, 2, -1, -11));
            crack.functions.Add(new Abs(1, 1, 5, 1, -13));
            crackPresets.Add(crack);

            crack = new Crack(4);
            crack.functions.Add(new Abs(15, 0, 1, 0, -11));
            crack.functions.Add(new Abs(1, -1, 2, -1, 13));
            crack.functions.Add(new Abs(0, 1, 5, 1, -15));
            crackPresets.Add(crack);
        }

        /// <summary>
        /// Render
        /// </summary>
        private void RenderModel(ModelVisual3D whereToRender, MeshGeometry3D mesh, DiffuseMaterial material, bool clearScene = true)
        {
            GeometryModel3D surfaceModel = new GeometryModel3D()
            {
                Geometry = mesh,
                Material = material,
                BackMaterial = material

                //Material = materialGradient,
                //BackMaterial = materialGradient
            };

            if (clearScene)
                whereToRender.Children.Clear();

            ModelVisual3D model = new ModelVisual3D
            {
                Content = surfaceModel
            };

            whereToRender.Children.Add(model);
        }

        public void RenderFunctionWithCubeAndCracks(int size, DiffuseMaterial material, Point3D offset = new Point3D())
        {
            int meshMatrixRang = Convert.ToInt32(2 * size * detail);

            MeshGeometry3D surfaceMesh = SurfaceWithCracks(function, size);
            MeshGeometry3D bottomMesh = SurfaceWithCracks(new LinearExpression(0, 0, -10), size);

            MeshGeometry3D combinedMesh = new MeshGeometry3D();

            for (int i = 0; i < (meshMatrixRang + 1) * (meshMatrixRang + 1); i++)
            {
                Point3D pointSurface = new Point3D()
                {
                    X = surfaceMesh.Positions[i].X + offset.X,
                    Y = surfaceMesh.Positions[i].Y + offset.Y,
                    Z = surfaceMesh.Positions[i].Z + offset.Z
                };
                Point3D pointBottom = new Point3D()
                {
                    X = bottomMesh.Positions[i].X + offset.X,
                    Y = bottomMesh.Positions[i].Y + offset.Y,
                    Z = bottomMesh.Positions[i].Z + offset.Z
                };

                combinedMesh.Positions.Add(pointSurface);
                combinedMesh.Positions.Add(pointBottom);
            }

            for (int x = 0; x < 2 * meshMatrixRang; x += 2)
            {
                for (int z = 0; z < 2 * meshMatrixRang; z++)
                {
                    int p1 = x * (meshMatrixRang + 1) + z;
                    int p2 = x * (meshMatrixRang + 1) + z + 2;
                    int p3 = (x + 2) * (meshMatrixRang + 1) + z;
                    int p4 = (x + 2) * (meshMatrixRang + 1) + z + 2;

                    if (combinedMesh.Positions[p2 - 1].Y == combinedMesh.Positions[p2].Y)
                        continue;

                    combinedMesh.TriangleIndices.Add(p1); //    |  /
                    combinedMesh.TriangleIndices.Add(p2); //    | /
                    combinedMesh.TriangleIndices.Add(p3); //    |/

                    combinedMesh.TriangleIndices.Add(p3); //      /|
                    combinedMesh.TriangleIndices.Add(p2); //     / |
                    combinedMesh.TriangleIndices.Add(p4); //    /  |
                }
            }

            int verticesCount = combinedMesh.TriangleIndices.Count;

            for (int i = 0; i < verticesCount; i += 12)
            {
                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i]);
                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 1]);
                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 7]);

                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 6]);
                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i]);
                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 7]);

                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 3]);
                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 5]);
                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 8]);

                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 5]);
                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 11]);
                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 8]);

                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i]);
                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 3]);
                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 6]);

                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 8]);
                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 6]);
                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 3]);

                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 1]);
                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 5]);
                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 7]);

                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 5]);
                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 11]);
                combinedMesh.TriangleIndices.Add(combinedMesh.TriangleIndices[i + 7]);
            }

            RenderModel(visual, combinedMesh, material);
        }

        /// <summary>
        /// Shapes (bigger "detail" value - better detail)
        /// </summary>
        public double CalcuclateFunctionValueWithCracks(IFunction selectedFunction, double x, double y)
        {
            double funcitonValue = selectedFunction.Calculate(x, y);

            for (int i = 0; i < cracks.Count; i++)
            {
                foreach (Abs crack in cracks[i].functions)
                    funcitonValue = Math.Min(funcitonValue, crack.Calculate(x, y));
            }

            if (funcitonValue < -10)
                funcitonValue = -10;

            return funcitonValue;
        }

        private MeshGeometry3D SurfaceWithCracks(IFunction selectedFunction, int boundsRadius)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            int meshMatrixRang = Convert.ToInt32(2 * boundsRadius * detail);
            Point maxPoint = new Point(boundsRadius, boundsRadius);
            Point minPoint = new Point(-boundsRadius, -boundsRadius);

            //<= because we need to calculate the values in all boundaries
            for (double x = minPoint.X; x <= maxPoint.X; x += 1 / detail)
            {
                for (double z = minPoint.Y; z <= maxPoint.Y; z += 1 / detail)
                {
                    double functionValue = CalcuclateFunctionValueWithCracks(selectedFunction, x, z); //selectedFunction.Calculate(x, z);

                    Point3D meshPoint = new Point3D(x, functionValue, z);

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

            return mesh;
        }

        private MeshGeometry3D Cube(Point3D sidesLength, Point3D center)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            {
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
            }

            return mesh;
        }

        private MeshGeometry3D Sphere(double radius, int pointsAmountTheta, int pointsAmountPhi, Point3D center)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            for (int i = 0; i <= pointsAmountTheta; ++i)
            {
                double theta = i * Math.PI / pointsAmountTheta;
                for (int j = 0; j <= pointsAmountPhi; ++j)
                {
                    double phi = j * 2 * Math.PI / pointsAmountPhi;
                    Point3D point = new Point3D(
                        center.X + radius * Math.Sin(theta) * Math.Cos(phi),
                        center.Y + radius * Math.Cos(theta),
                        center.Z + radius * Math.Sin(theta) * Math.Sin(phi));
                    mesh.Positions.Add(point);
                }
            }

            for (int i = 0; i < pointsAmountTheta; ++i)
            {
                for (int j = 0; j < pointsAmountPhi; ++j)
                {
                    int i1 = i * (pointsAmountPhi + 1) + j;
                    int i2 = i * (pointsAmountPhi + 1) + j + 1;
                    int i3 = (i + 1) * (pointsAmountPhi + 1) + j;
                    int i4 = (i + 1) * (pointsAmountPhi + 1) + j + 1;

                    mesh.TriangleIndices.Add(i1);
                    mesh.TriangleIndices.Add(i2);
                    mesh.TriangleIndices.Add(i3);

                    mesh.TriangleIndices.Add(i2);
                    mesh.TriangleIndices.Add(i4);
                    mesh.TriangleIndices.Add(i3);
                }
            }
            return mesh;
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
    }
}