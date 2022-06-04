using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PureVPN.Models
{
    internal class DataWorker
    {
        private const string ADDRESS = @"https://www.vpngate.net/api/iphone/";
        private async Task<Stream> GetDataStream()
        {
            var httpClient = new HttpClient();
            var response = httpClient.GetAsync(ADDRESS).Result;
            return await response.Content.ReadAsStreamAsync();
        }
        private IEnumerable<string> GetDataContentLines()
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

        private ServerInfo InitializeServerList(string hostName, string ip, string score, string ping, string speed,
            string country, string numOfSessions, string uptime, string totalTraffic, string vpnOperator)
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
                Score = score.Trim(),
                Ping = ping.Trim() + " ms",
                Speed = speed.Trim() + " Mbps",
                CountryLong = country.Trim(),
                NumVpnSessions = numOfSessions.Trim(),
                Uptime = uptime.Trim(),
                Operator = vpnOperator.Trim(),
                TotalTraffic = totalTraffic.Trim()
            };
        }

        private readonly Dictionary<string, int> _indexes = new Dictionary<string, int>()
        {
            { "HostName", 0 },
            { "Ip", 1 },
            { "Score", 2 },
            { "Ping", 3 },
            { "Speed", 4 },
            { "Country", 5 },
            { "NumSessions", 7 },
            { "Uptime", 8 },
            { "TotalTraffic", 10 },
            { "Operator", 12 }
        };

        public async IAsyncEnumerable<ServerInfo> GetDataLines()
        {
            const int LINELENGTH = 15;
            var lines = GetDataContentLines()
                .Skip(2)
                .Select(s => s.Split(','));

            foreach (var line in lines)
            {
                if (line.Length != LINELENGTH) continue;
                // HostName,IP,Score,Ping,Speed,CountryLong,CountryShort,NumVpnSessions,Uptime,TotalUsers,TotalTraffic,LogType,Operator,Message,OpenVPN_ConfigData_Base64
                var list = InitializeServerList(line[_indexes["HostName"]], line[_indexes["Ip"]],
                    line[_indexes["Score"]], line[_indexes["Ping"]], line[_indexes["Speed"]],
                    line[_indexes["Country"]], line[_indexes["NumSessions"]], line[_indexes["Uptime"]],
                    line[_indexes["TotalTraffic"]], line[_indexes["Operator"]]);
                await Task.Delay(20);
                yield return list;

            }
        }
    }
}
