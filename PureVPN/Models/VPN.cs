using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PureVPN.Models
{
    public static class VPN
    {
        private static string FolderPath => string.Concat(Directory.GetCurrentDirectory(), "\\VPN");

        public static async Task<int> ConnectVpn(string hostName)
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            var infoStringBuilder = new StringBuilder();
            infoStringBuilder.AppendLine("[VPN]")
                .AppendLine("MEDIA=rastapi")
                .AppendLine("Port=VPN2-0")
                .AppendLine("Device=WAN Miniport (IKEv2)")
                .AppendLine("DEVICE=vpn")
                .AppendLine("PhoneNumber=" + hostName);

            await File.WriteAllTextAsync(FolderPath + "\\VpnConnection.pbk", infoStringBuilder.ToString());

            infoStringBuilder = new StringBuilder();
            infoStringBuilder.AppendLine("rasdial \"VPN\" " + "vpn" + " " + "vpn" + " /phonebook:\"" + FolderPath +
                                         "\\VpnConnection.pbk\"");

            await File.WriteAllTextAsync(FolderPath + "\\VpnConnection.bat", infoStringBuilder.ToString());

            var newProcess = new Process
            {
                StartInfo =
                {
                    FileName = FolderPath + "\\VpnConnection.bat",
                    //UseShellExecute = false,
                    //CreateNoWindow = true,
                }
            };

            newProcess.Start();

            await newProcess.WaitForExitAsync();

            var exitCode = newProcess.ExitCode;

            return exitCode;
        }

        public static void DisconnectVpn()
        {
            File.WriteAllText(FolderPath + "\\VpnDisconnect.bat", "rasdial /d");

            var newProcess = new Process
            {
                StartInfo =
                {
                    FileName = FolderPath + "\\VpnDisconnect.bat",
                    WindowStyle = ProcessWindowStyle.Normal
                }
            };

            newProcess.Start();
            newProcess.WaitForExit();
            
        }
    }
}
