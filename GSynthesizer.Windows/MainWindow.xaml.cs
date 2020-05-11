using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Unity;

namespace GSynthesizer.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [Dependency]
        public IContainerExtension ContainerExtension { get; set; }

        [Dependency]
        public IRegionManager RegionManager { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        public void OnLoaded(object sender, RoutedEventArgs e)
        {
        }
    }
}