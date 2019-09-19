using HelixToolkit.Wpf;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using System.Windows.Media;

namespace WorkWithStl
{ /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
    public partial class MainWindow : Window
    {
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
                ModelVisual3D device3D = new ModelVisual3D();
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
    }
}
