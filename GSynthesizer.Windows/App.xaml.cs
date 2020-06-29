using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GSynthesizer.Views.Windows;
using Prism.Ioc;
using Prism.Unity;

namespace GSynthesizer.Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton(typeof(DeviceSettings));
            containerRegistry.RegisterSingleton(typeof(MainSynthesizer));
        }
    }
}
