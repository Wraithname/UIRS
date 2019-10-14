using HelixToolkit.Wpf;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using System.Windows.Media;
using STL_Tools;
using System.Linq;
using System.Collections.Generic;

namespace WorkWithStl
{ /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
    public partial class MainWindow : Window
    {
        private Model3D device = null;
        private Plane3D ContourPlane;
        private ModelVisual3D device3D = new ModelVisual3D();
        private TriangleMesh[] meshArray;
        private List<Point3D> point = new List<Point3D>();
        private List<Point3D> list = new List<Point3D>();
        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Display 3D Model
        /// </summary>
        /// <param name="model">Path to the Model file</param>
        /// <returns>3D Model Content</returns>
        private Model3D Display3d(string model)
        {
            
            try
            {
                //Adding a gesture here
                viewPort3d.RotateGesture = new MouseGesture(MouseAction.RightClick);
                Material material = new DiffuseMaterial(new SolidColorBrush(Colors.Silver));
                ModelImporter import = new ModelImporter();
                //Load the 3D model file
                import.DefaultMaterial = material;
                device = import.Load(model);
            }
            catch (Exception e)
            {
                // Handle exception in case can not file 3D model
                MessageBox.Show("Exception Error : " + e.StackTrace);
            }
            return device;
        }
        private void OpenFile(object sender, RoutedEventArgs e)
        {
            string FilePath = null;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                    FilePath = openFileDialog.FileName;
                    STLReader stlReader = new STLReader(openFileDialog.FileName);
                    meshArray = stlReader.ReadFile();
                    TriangleMesh[] normalArray = new TriangleMesh[meshArray.Length];
                //Перобразование в лист Point3D
                for(int i = 0; i < meshArray.Length; i++)
                {
                    point.Add(new Point3D(meshArray[i].vert1.x,meshArray[i].vert1.y,meshArray[i].vert1.z));
                    point.Add(new Point3D(meshArray[i].vert2.x, meshArray[i].vert2.y, meshArray[i].vert2.z));
                    point.Add(new Point3D(meshArray[i].vert3.x, meshArray[i].vert3.y, meshArray[i].vert3.z));
                }
                //удаление дубликатов точек
                list = point.Distinct().ToList<Point3D>();
                device3D.Content = Display3d(FilePath);
                    viewPort3d.Children.Clear();
                    // Добавление в порт
                    viewPort3d.Children.Add(device3D);
                    viewPort3d.Children.Add(new SunLight());
                    viewPort3d.ZoomExtents();

            }
        }

        private void Quit(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }
        private void AddContours(GeometryModel3D model, Transform3D transform)
        {
            var p = ContourPlane.Position;
            var n = ContourPlane.Normal;
            var segments = MeshGeometryHelper.GetContourSegments(model.Geometry as MeshGeometry3D, p, n).ToList();
            foreach (var contour in MeshGeometryHelper.CombineSegments(segments, 1e-6).ToList())
            {
                if (contour.Count == 0)
                    continue;
                viewPort3d.Children.Add(new TubeVisual3D { Diameter = 0.03, Path = new Point3DCollection(contour), Fill = Brushes.Red });
            }
        }
        private void AddContours(Visual3D model1, int o, int m, int n)
        {
            var bounds = Visual3DHelper.FindBounds(model1, Transform3D.Identity);
            for (int i = 1; i < n; i++)
            {
                this.ContourPlane = new Plane3D(new Point3D(0, 0, bounds.Location.Z + bounds.Size.Z * i / n), new Vector3D(0, 0, 1));
                Visual3DHelper.Traverse<GeometryModel3D>(model1, this.AddContours);
            }
            for (int i = 1; i < m; i++)
            {
                this.ContourPlane = new Plane3D(new Point3D(0, bounds.Location.Y + bounds.Size.Y * i / m, 0), new Vector3D(0, 1, 0));
                Visual3DHelper.Traverse<GeometryModel3D>(model1, this.AddContours);
            }
            for (int i = 1; i < o; i++)
            {
                this.ContourPlane = new Plane3D(new Point3D(bounds.Location.X + bounds.Size.X * i / o, 0, 0), new Vector3D(1, 0, 0));
                Visual3DHelper.Traverse<GeometryModel3D>(model1, this.AddContours);
            }
        }
        private void showCounter(object sender, RoutedEventArgs e)
        {
            viewPort3d.Children.Clear();
            AddContours(device3D, 5, 5, 5);
        }
        private void showModel(object sender, RoutedEventArgs e)
        {
            viewPort3d.Children.Clear();
            viewPort3d.Children.Add(device3D);
            viewPort3d.Children.Add(new SunLight());
            viewPort3d.ZoomExtents();
        }
        private void showModelCounter(object sender, RoutedEventArgs e)
        {
            viewPort3d.Children.Clear();
            viewPort3d.Children.Add(device3D);
            viewPort3d.Children.Add(new SunLight());
            viewPort3d.ZoomExtents();
            AddContours(device3D, 5, 5, 5);
        }
        private void cutModel(object sender, RoutedEventArgs e)
        {
            RectangleVisual3D plane = new RectangleVisual3D();
            plane.Origin = new Point3D(-4, -1, 1);
            plane.Width = device.Bounds.SizeY;
            plane.Length = device.Bounds.SizeX;
            TranslateManipulator manipulator1 = new TranslateManipulator();
            manipulator1.Bind(plane);
            manipulator1.Color = Colors.Red;
            manipulator1.Direction = new Vector3D(1, 0, 0);
            manipulator1.Diameter = 0.5;
            manipulator1.Length = plane.Length / 2;
            TranslateManipulator manipulator2 = new TranslateManipulator();
            manipulator2.Bind(plane);
            manipulator2.Color = Colors.Green;
            manipulator2.Direction = new Vector3D(0, 1, 0);
            manipulator2.Diameter = 0.5;
            manipulator2.Length = plane.Length / 2;
            TranslateManipulator manipulator3 = new TranslateManipulator();
            manipulator3.Bind(plane);
            manipulator3.Color = Colors.Blue;
            manipulator3.Direction = new Vector3D(0, 0, 1);
            manipulator3.Diameter = 0.5;
            manipulator3.Length = plane.Length/2;
            //Разрез

            //Добавление в порт
            viewPort3d.Children.Add(plane);
            viewPort3d.Children.Add(manipulator1);
            viewPort3d.Children.Add(manipulator2);
            viewPort3d.Children.Add(manipulator3);
        }
        private Transform3DGroup GetTransforms(Model3D model)
        {
            var transforms = new Transform3DGroup();
            // Rotation around X
            transforms.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0)));
            // Rotation around Y 
            transforms.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0)));
            // Rotation around Z
            transforms.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0)));
            // Translate transform (if required)
            transforms.Children.Add(new TranslateTransform3D());
            model.Transform = transforms;
            return transforms;
        }
    }
}
