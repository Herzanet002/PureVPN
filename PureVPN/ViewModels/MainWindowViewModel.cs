using PureVPN.Commands;
using PureVPN.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;


namespace PureVPN.ViewModels
{
    internal class MainWindowViewModel : Base.ViewModel
    {
        private const string IP_SERVICE = "https://api.ipify.org";
        private static ObservableCollection<ServerInfo> _servers;
        private static Visibility _isServerInfoVisible = Visibility.Collapsed;
        private static Visibility _isProgressBarEnabled = Visibility.Collapsed;
        private static Visibility _isConnectionStatusVisible = Visibility.Collapsed;
        private static string? _ipString;
        private static ServerInfo _selectedServer;
        private static string _connectButtonContent;
        private static string? _searchText;
        private ICommand _connectCommand;
        private ICollectionView _collectionView;

        public ICommand LoadListCommand { get; set; }
        public ICommand CloseAppCommand => new LambdaCommand(CloseWindowCommand_Executed);
        public ICommand MinimizeAppCommand => new LambdaCommand(MinimizeWindowCommand_Executed);

        public ICommand ConnectCommand
        {
            get => _connectCommand;
            set => Set(ref _connectCommand, value);
        }
        public string ConnectButtonContent
        {
            get => _connectButtonContent;
            set => Set(ref _connectButtonContent, value);
        }
        public string? IpString
        {
            get => _ipString;
            set => Set(ref _ipString, value);
        }
        public ServerInfo SelectedServer
        {
            get => _selectedServer;
            set
            {
                IsServerInfoVisible = Visibility.Visible;
                Set(ref _selectedServer, value);
            }
        }
        public ICollectionView CollectionView
        {
            get => _collectionView;
            set => Set(ref _collectionView, value);
        }
        public ObservableCollection<ServerInfo> Servers
        {
            get => _servers;
            set => Set(ref _servers, value);
        }
        public Visibility IsProgressBarEnabled
        {
            get => _isProgressBarEnabled;
            set => Set(ref _isProgressBarEnabled, value);
        }
        public Visibility IsConnectionStatusVisible
        {
            get => _isConnectionStatusVisible;
            set => Set(ref _isConnectionStatusVisible, value);
        }
        public Visibility IsServerInfoVisible
        {
            get => _isServerInfoVisible;
            set => Set(ref _isServerInfoVisible, value);
        }
        public string? SearchText
        {
            get => _searchText;
            set
            {
                Set(ref _searchText, value);
                CollectionView.Filter = Filter;
            }
        }

        public MainWindowViewModel()
        {
            IpString = GetIpAddress();
            Servers = new ObservableCollection<ServerInfo>();
            ConnectButtonContent = Application.Current.Resources["ConnectTitleForButton"] as string ?? string.Empty;
            LoadListCommand = new LambdaCommand(GetServersListCommand_Executed, _ => true);
            ConnectCommand = new LambdaCommand(ConnectCommand_Executed, _ => true);
            GetServersList();
            CollectionView = CollectionViewSource.GetDefaultView(Servers);
        }

        private bool Filter(object itemForFiltering)
        {
            if (!(SearchText == null && string.IsNullOrEmpty(SearchText)))
            {
                return itemForFiltering is ServerInfo server &&
                       (server.CountryLong.ToLower().Contains(SearchText) ||
                        server.HostName.ToLower().Contains(SearchText) ||
                        server.Ip.ToLower().Contains(SearchText));
            }
            return true;
        }

        private async void GetServersList()
        {
            Servers.Clear();
            IsServerInfoVisible = Visibility.Collapsed;
            IsProgressBarEnabled = Visibility.Visible;
            var dataWorker = new DataWorker();

            await Task.Run(async () =>
            {
                await foreach (var serverList in dataWorker.GetDataLines())
                    Application.Current.Dispatcher.Invoke(() => Servers.Add(serverList));
            });

            IsProgressBarEnabled = Visibility.Collapsed;
        }

        private void GetServersListCommand_Executed(object obj) => GetServersList();
        private void CloseWindowCommand_Executed(object obj) => Application.Current.Shutdown();
        private void MinimizeWindowCommand_Executed(object obj)
        {
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }


        private async void ConnectCommand_Executed(object obj)
        {
            var result = await VPN.ConnectVpn(SelectedServer.HostName);
            if (result != 0) return;
            IsConnectionStatusVisible = Visibility.Visible;
            ConnectCommand = new LambdaCommand(DisconnectCommand_Executed, _ => true);
            IpString = GetIpAddress();
            ConnectButtonContent = Application.Current.Resources["DisconnectTitleForButton"] as string ?? string.Empty;
        }

        private void DisconnectCommand_Executed(object obj)
        {
            VPN.DisconnectVpn();
            ConnectCommand = new LambdaCommand(ConnectCommand_Executed, _ => true);
            IsConnectionStatusVisible = Visibility.Collapsed;
            IpString = GetIpAddress();
            ConnectButtonContent = Application.Current.Resources["ConnectTitleForButton"] as string ?? string.Empty;
        }

        private static string GetIpAddress() => new HttpClient().GetStringAsync(IP_SERVICE).Result;


    }


}
