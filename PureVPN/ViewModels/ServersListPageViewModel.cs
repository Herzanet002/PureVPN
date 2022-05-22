using PureVPN.Commands;
using PureVPN.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PureVPN.ViewModels.Base;

namespace PureVPN.ViewModels
{
    internal class ServersListPageViewModel : Base.ViewModel
    {
        private const string ADDRESS = @"https://www.vpngate.net/api/iphone/";
        private ObservableCollection<ServerInfo> _servers = null!;
        private static ServerInfo _selectedServer;
        private Visibility _isProgressBarEnabled = Visibility.Collapsed;
        public ICommand LoadListCommand { get; set; }
        public ICommand DataGridSelectedItemCommand { get; set; }
       // public ViewModel? CurrentViewModel => Current?.NavigationStore.CurrentViewModel;
        public ServersListPageViewModel()
        {
            Servers = new ObservableCollection<ServerInfo>();
            LoadListCommand = new LambdaCommand(_ => GetServersList());
            DataGridSelectedItemCommand = new Command(SelectedItemCommand_Executed, _ => true);
            GetServersList();
        }

        public void SelectedItemCommand_Executed(object obj)
        {
            
        }
        public Visibility IsProgressBarEnabled
        {
            get => _isProgressBarEnabled;
            set => Set(ref _isProgressBarEnabled, value);
        }
        public ServerInfo SelectedServer
        {
            get => _selectedServer;
            set
            {
                //CurrentViewModel = new ServerInfoPageViewModel();
                Set(ref _selectedServer, value);
            }
        }

        public ObservableCollection<ServerInfo> Servers
        {
            get => _servers;
            set => Set(ref _servers, value);
        }

        private async void GetServersList()
        {
            Servers = new ObservableCollection<ServerInfo>();
            IsProgressBarEnabled = Visibility.Visible;
            await Task.Run(async () =>
            {
                await foreach (var serverList in GetDataLines())
                    Application.Current.Dispatcher.Invoke(() => Servers.Add(serverList));
            });
            IsProgressBarEnabled = Visibility.Collapsed;
        }
        private static async Task<Stream> GetDataStream()
        {
            var httpClient = new HttpClient();
            var response = httpClient.GetAsync(ADDRESS).Result;
            return await response.Content.ReadAsStreamAsync();

        }
        private static IEnumerable<string> GetDataContentLines()
        {
            using var dataStream = GetDataStream().Result;
            using var streamReader = new StreamReader(dataStream);

            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                yield return line;

            }
        }

        private static ServerInfo InitializeServerList(string hostName, string ip, string ping, string speed, string country, string numOfSessions, string vpnOperator)
        {
            speed = speed.Length switch
            {
                7 => speed.Insert(1, ",")[..4],
                8 => speed.Insert(2, ",")[..5],
                9 => speed.Insert(3, ",")[..6],
                10 => speed.Insert(4, ",")[..7],
                _ => speed
            };
            return new ServerInfo
            {
                HostName = hostName.Trim() + ".opengw.net",
                Ip = ip.Trim(),
                Ping = ping.Trim() + " ms",
                Speed = speed.Trim() + " Mbps",
                CountryLong = country.Trim(),
                NumVpnSessions = numOfSessions.Trim(),
                Operator = vpnOperator.Trim(),
            };
        }

        private static async IAsyncEnumerable<ServerInfo> GetDataLines()
        {
            var lines = GetDataContentLines()
                .Skip(2)
                .Select(s => s.Split(','));

            foreach (var line in lines)
            {
                if (line.Length != 15) continue;
                // HostName,IP,Score,Ping,Speed,CountryLong,CountryShort,NumVpnSessions,Uptime,TotalUsers,TotalTraffic,LogType,Operator,Message,OpenVPN_ConfigData_Base64
                var list = InitializeServerList(line[0], line[1], line[3], line[4], line[5], line[7], line[12]);
                await Task.Delay(50);
                yield return list;

            }
        }

    }
}
