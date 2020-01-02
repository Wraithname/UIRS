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
        private Model3D Counter_model = null;
        private Plane3D ContourPlane;
        private ModelVisual3D device3D = new ModelVisual3D();
        private ModelVisual3D Counter_model3D = new ModelVisual3D();
        private TriangleMesh[] meshArray;
        private List<Point3D> point = new List<Point3D>();
        private List<Point3D> list = new List<Point3D>();
        private List<Point3D> list_counter = new List<Point3D>();
        private List<Point3D> list_contr = new List<Point3D>();
        private List<Point3D> counter = new List<Point3D>();
        private double x_n = 0;
        private double y_n = 0;
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
                for (int i = 0; i < meshArray.Length; i++)
                {
                    point.Add(new Point3D(meshArray[i].vert1.x, meshArray[i].vert1.y, meshArray[i].vert1.z));
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
                //if (list[k].Z < 7 && list[k].Z > 6.9)
                //{
                    //  msh.AddBox(list[k], 0.1, 0.1, 0.1);
                    list_counter.Add(list[k]);
                //}
            for (int i = 0; i < list_counter.Count; i++)
            {
                list_contr.Add(new Point3D(list_counter[i].X, list_counter[i].Y, -1));
            }
            for (int k = 0; k < list_contr.Count; k++)
                if (list_contr[k].Z == -1)
                    msh.AddBox(list_contr[k], 0.1, 0.1, 0.1);

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

            for (int i = 0; i < list_contr.Count; i++)
            {
                List<Point3D> minn = new List<Point3D>();
                double min = 60;//(изначально большое число)
                double distance = 0;//--дистанция до точки
                double x = list_contr[i].X;
                double y = list_contr[i].Y;
                double z = list_contr[i].Z;
                double x1 = 0;
                double y1 = 0;
                double z1 = 0;
                LinesVisual3D line = new LinesVisual3D();
                line.Color = Colors.Red;
                line.Thickness = 2;
                line.Points.Add(new Point3D(x, y, z));
                for (int e = 0; e < list_contr.Count; e=e+2)
                {
                    if (list_contr[e].X != x && list_contr[e].Y != y&& list_contr[e].X != x_n && list_contr[e].Y != y_n)
                    {
                        distance = Math.Pow((x - list_contr[e].X), 2) + Math.Pow((y - list_contr[e].Y), 2) + Math.Pow((z - list_contr[e].Z), 2);
                        if (distance < min)
                        {
                            min = distance;
                            x1 = list_contr[e].X;
                            y1 = list_contr[e].Y;
                            z1 = list_contr[e].Z;
                        }
                    }
                }
                line.Points.Add(new Point3D(x1, y1, z1));
                viewPort3d.Children.Add(line);
                x_n = x;
                y_n = y;
            }
        }
        private void AddContours(Visual3D model1, int o, int m, int n)
        {
            var bounds = Visual3DHelper.FindBounds(model1, Transform3D.Identity);
            for (int i = 1; i < n; i++)
            {
                ContourPlane = new Plane3D(new Point3D(0, 0, list_counter[i].Z + bounds.Size.Z * i / n), new Vector3D(0, 0, 1));
                counter.Add(new Point3D(0, 0, list_counter[i].Z + bounds.Size.Z * i / n));
                Visual3DHelper.Traverse<GeometryModel3D>(model1, this.AddContours);
            }
            //for (int i = 1; i < m; i++)
            //{
            //    this.ContourPlane = new Plane3D(new Point3D(0, bounds.Location.Y + bounds.Size.Y * i / m, 0), new Vector3D(0, 1, 0));
            //    Visual3DHelper.Traverse<GeometryModel3D>(model1, this.AddContours);
            //}
            //for (int i = 1; i < o; i++)
            //{
            //    this.ContourPlane = new Plane3D(new Point3D(bounds.Location.X + bounds.Size.X * i / o, 0, 0), new Vector3D(1, 0, 0));
            //    Visual3DHelper.Traverse<GeometryModel3D>(model1, this.AddContours);
            //}
        }
        private void showCounter(object sender, RoutedEventArgs e)
        {
            MeshBuilder msh = new MeshBuilder();
            viewPort3d.Children.Clear();
            AddContours(device3D, 5, 5, 4);
            Counter_model = device3D.Content;
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
                if (list[k].Z > z)
                    msh.AddBox(list[k], 0.1, 0.1, 0.1);
            }
            list_counter = list;
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
