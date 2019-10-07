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
        private Plane3D ContourPlane;
        private ModelVisual3D device3D = new ModelVisual3D();
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
            Model3D device = null;
            try
            {
                //Adding a gesture here
                viewPort3d.RotateGesture = new MouseGesture(MouseAction.LeftClick);

                //Import 3D model file
                ModelImporter import = new ModelImporter();
                Material material = new DiffuseMaterial(new SolidColorBrush(Colors.Silver));
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
           
            string FilePath=null; 
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(openFileDialog.ShowDialog()==true)
            {
                FilePath = openFileDialog.FileName;
                STLReader stlReader = new STLReader(openFileDialog.FileName);
                TriangleMesh[] meshArray = stlReader.ReadFile();
                TriangleMesh[] normalArray = new TriangleMesh[meshArray.Length];  
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
            AddContours(device3D, 5,5, 5);
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
    }
}
