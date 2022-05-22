using PureVPN.Views;
using System.Net.Http;
using System.Windows;
using PureVPN.Helpers;
using PureVPN.ViewModels.Base;


namespace PureVPN.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        private static string _ipString;

        private static ServersPageView _serversListPage;


        public string IpString
        {
            get => _ipString;
            set => Set(ref _ipString, value);
        }

        public static App? Current => Application.Current as App;
        
        public ViewModel? CurrentViewModel => Current?.NavigationStore.CurrentViewModel;

        public MainWindowViewModel()
        {
            IpString = GetIpAddress();
            //_serversListPage = new ServersListPage();
            //CurrentPage = _serversListPage;
            

        }


        public static string GetIpAddress()
        {
            const string IP_SERVICE = "https://api.ipify.org";
            return new HttpClient().GetStringAsync(IP_SERVICE).Result;
        }

    }
}
