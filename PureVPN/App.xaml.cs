using System.Windows;
using PureVPN.Helpers;
using PureVPN.ViewModels;

namespace PureVPN
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public NavigationStore NavigationStore;
        App()
        {
            NavigationStore = new NavigationStore()
            {
                CurrentViewModel = new ServersListPageViewModel()
            };
        }
    }
}
