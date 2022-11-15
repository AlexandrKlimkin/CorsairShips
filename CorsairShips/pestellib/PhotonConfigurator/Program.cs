using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Xml.Linq;

namespace PhotonConfigurator
{
    class Program
    {
        private const string PhotonconfiguratorLog = "PhotonConfigurator.log";

        public static string GetPublicIP()
        {
            string url = "http://checkip.dyndns.org";
            var req = WebRequest.Create(url);
            var resp = req.GetResponse();
            var sr = new StreamReader(resp.GetResponseStream());
            string response = sr.ReadToEnd().Trim();
            string[] a = response.Split(':');
            string a2 = a[1].Substring(1);
            string[] a3 = a2.Split('<');
            string a4 = a3[0];
            return a4;
        }

        static string GetHostName()
        {
            ProcessStartInfo process = new ProcessStartInfo("nslookup.exe", GetPublicIP());
            //ProcessStartInfo process = new ProcessStartInfo("nslookup.exe", "54.38.176.212");

            process.RedirectStandardOutput = true;
            process.UseShellExecute = false;
            var p = Process.Start(process);
            p.WaitForExit();
            var nslookup = p.StandardOutput.ReadToEnd().Split('\n');
            var nameRow = nslookup.FirstOrDefault(x => x.StartsWith("Name:"));
            if (nameRow != null)
            {
                var row = nameRow.Replace("Name:", "").Trim();
                return row;
            }
            else
            {
                return "localhost";
            }
        }

        static void Main(string[] args)
        {
            if (File.Exists(PhotonconfiguratorLog))
            {
                File.Delete(PhotonconfiguratorLog);
            }

            UpdateHostNameInConfig();
            CheckLicenseFiles();
        }

        private static void CheckLicenseFiles()
        {
            var licenseFiles32 = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\bin_Win32\", "*.license");
            var licenseFiles64 = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\bin_Win64\", "*.license");

            if (licenseFiles64.Length != 1 || licenseFiles32.Length != 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Log("PLEASE CHECK PHOTON LICENSE FILES");
                Console.ResetColor();
            }
            else
            {
                Log("Found license files");
            }
        }

        private static void UpdateHostNameInConfig()
        {
            var configPath = Directory.GetCurrentDirectory() +
                             @"\Loadbalancing\GameServer\bin\Photon.LoadBalancing.dll.config";
            configPath = Path.GetFullPath(configPath);

            Log("Using file: " + configPath);

            //@"D:\Projects\DesertWars\photon\deploy\Loadbalancing\GameServer\bin\Photon.LoadBalancing.dll.config";

            var doc = XDocument.Load(configPath);

            var appSettings = doc.Element("configuration").Element("applicationSettings");
            var gameServerSettings = appSettings.Element("Photon.LoadBalancing.GameServer.GameServerSettings");

            var host = gameServerSettings.Elements().FirstOrDefault(x =>
                x.Attributes().Any(attr => attr.Name == "name" && attr.Value == "PublicHostName"));

            var hostName = GetHostName();
            host.Element("value").SetValue(hostName);
            Log("Set host to: " + hostName);

            var publicIp = gameServerSettings.Elements().FirstOrDefault(x =>
                x.Attributes().Any(attr => attr.Name == "name" && attr.Value == "PublicIPAddress"));
            var publicIpValue = GetPublicIP();
            publicIp.Element("value").SetValue(publicIpValue);

            var masterServerSettings = appSettings.Element("Photon.LoadBalancing.MasterServer.MasterServerSettings");
            publicIp = masterServerSettings.Elements().FirstOrDefault(x =>
                x.Attributes().Any(attr => attr.Name == "name" && attr.Value == "PublicIPAddress"));
            publicIp.Element("value").SetValue(publicIpValue);

            Log("Set public IP to: " + publicIpValue);

            File.WriteAllText(configPath, doc.ToString());

            Log("Done! Config {0} have updated", configPath);
        }

        public static void Log(string format, params object[] arg)
        {
            File.AppendAllLines(PhotonconfiguratorLog, new [] { string.Format(format, arg) });
            Console.WriteLine(format, arg);
        }
    }
}
