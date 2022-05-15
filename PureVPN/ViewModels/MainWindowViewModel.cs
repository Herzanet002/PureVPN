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

namespace PureVPN.ViewModels
{
    internal class MainWindowViewModel : Base.ViewModel
    {
        private const string ADDRESS = @"https://www.vpngate.net/api/iphone/";

        private ObservableCollection<ServerInfo> _servers;
        private Visibility _isProgressBarEnabled = Visibility.Collapsed;
        private static string _statusText = "Ready!";
        public ICommand LoadListCommand { get; set; }

        public string StatusText
        {
            get => _statusText;
            set => Set(ref _statusText, value);
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
        public MainWindowViewModel()
        {
            _servers = new ObservableCollection<ServerInfo>();
            LoadListCommand = new LambdaCommand(GetServersList, _ => true);

        }

        private async void GetServersList(object obj)
        {
            IsProgressBarEnabled = Visibility.Visible;
            StatusText = "Loading...";
            var ip = GetIpAddress();
            await Task.Run(async () =>
            {
                await foreach (var serverList in GetDataLines())
                    Application.Current.Dispatcher.Invoke(() => Servers.Add(serverList));
            });

            IsProgressBarEnabled = Visibility.Collapsed;
            StatusText = "Ready!";


        }
        private static async Task<Stream> GetDataStream()
        {
            var httpClient = new HttpClient();
            var response = httpClient.GetAsync(ADDRESS).Result;
            return await response.Content.ReadAsStreamAsync();
        }

        public static string GetIpAddress()
        {
            const string IP_SERVICE = "https://api.ipify.org";
            return new HttpClient().GetStringAsync(IP_SERVICE).Result;
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
            switch (speed.Length)
            {
                case 7:
                    speed = speed.Insert(1, ",").Substring(0, 4);
                    break;
                case 8:
                    speed = speed.Insert(2, ",").Substring(0, 5);
                    break;
                case 9:
                    speed = speed.Insert(3, ",").Substring(0, 6);
                    break;
                case 10:
                    speed = speed.Insert(4, ",").Substring(0, 7);
                    break;


            }
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
