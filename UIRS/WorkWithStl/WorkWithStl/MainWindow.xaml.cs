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
                //Преобразование в модель
                device = create3dmodel(list);
                device3D.Content = device;
                Cutt.IsEnabled = true;
                Count.IsEnabled = true;
                //--------------------------------------
                viewPort3d.Children.Add(device3D);
                viewPort3d.Children.Add(new SunLight());
                    viewPort3d.ZoomExtents();

            }
        }
        private Model3DGroup create3dmodel(List<Point3D> list)
        {
            var modelGroup = new Model3DGroup();
            MeshBuilder msh = new MeshBuilder();
            for (int k = 0; k < list.Count; k++)
                msh.AddBox(list[k], 0.1, 0.1, 0.1);
            var gm = new GeometryModel3D();
            var mesh = msh.ToMesh(true);
            var greenMaterial = MaterialHelper.CreateMaterial(Colors.Green);
            var insideMaterial = MaterialHelper.CreateMaterial(Colors.Yellow);
            modelGroup.Children.Add(new GeometryModel3D
            {
                Geometry = mesh,
                Material = greenMaterial,
                BackMaterial = insideMaterial
            });
            return modelGroup;
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
            slider.IsEnabled = true;
            
         }
        private void Updater(double z)
        {
            viewPort3d.Children.Clear();
            var modelGroup = new Model3DGroup();
            MeshBuilder msh = new MeshBuilder();
            for (int k = 0; k < list.Count; k++)
            {
                if(list[k].Z>z)
                msh.AddBox(list[k], 0.1, 0.1, 0.1);
            }
            var gm = new GeometryModel3D();
            var mesh = msh.ToMesh(true);
            var greenMaterial = MaterialHelper.CreateMaterial(Colors.Green);
            var insideMaterial = MaterialHelper.CreateMaterial(Colors.Yellow);
            modelGroup.Children.Add(new GeometryModel3D
            {
                Geometry = mesh,
                Material = greenMaterial,
                BackMaterial = insideMaterial
            });
            device = modelGroup;
            device3D.Content = device;
            //--------------------------------------

            viewPort3d.Children.Add(new SunLight());
            viewPort3d.Children.Add(device3D);
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Updater(slider.Value);
        }
    }
}
