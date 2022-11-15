using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using PestelLib.ClientConfig;
using PestelLib.ServerShared;

namespace UpdateSharedLogicVersion
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Updating shared logic CRC in sharedConfig.json begin");

            Console.WriteLine(Directory.GetCurrentDirectory());

            if (args.Length != 3)
            {
                Console.WriteLine("Usage: UpdateSharedLogicVersion.exe <sharedLogicDllPath> <sharedConfig.json client path> <sharedConfig.json server path>");
                return -1;
            }

            var sharedLogicDllPath = args[0];
            if (!File.Exists(sharedLogicDllPath))
            {
                Console.WriteLine("ERROR: Can't find shared logic dll: invalid path: " + sharedLogicDllPath);
                Console.WriteLine("Check parameter #1");
                return -1;
            }

            var sharedConfigPath = args[1];
            var sharedConfigDirectory = Path.GetDirectoryName(sharedConfigPath);
            if (!Directory.Exists(sharedConfigDirectory))
            {
                Console.WriteLine("ERROR: Output directory doesn't exist: " + sharedConfigDirectory);
                Console.WriteLine("Check parameter #2");
                return -1;
            }

            var sharedConfigServerPath = args[2];

            var slBytes = File.ReadAllBytes(sharedLogicDllPath);
            var crc = Crc32.Compute(slBytes);

            string configText = "";
            SharedConfig configDeserialized = new SharedConfig();

            if (File.Exists(sharedConfigPath))
            {
                configText = File.ReadAllText(sharedConfigPath);
                configDeserialized = JsonConvert.DeserializeObject<SharedConfig>(configText);
            }
            
            configDeserialized.SharedLogicCrc = crc;
            configDeserialized.SharedLogicVersion++;

            configText = JsonConvert.SerializeObject(configDeserialized, Formatting.Indented);
            File.WriteAllText(sharedConfigPath, configText);

            var sharedConfigServerPathDir = Path.GetDirectoryName(sharedConfigServerPath);
            if (Directory.Exists(sharedConfigServerPathDir))
            {
                File.WriteAllText(sharedConfigServerPath, configText);
            }
            else
            {
                Console.WriteLine("WARNING: Output directory doesn't exist: " + sharedConfigServerPathDir);
                Console.WriteLine("Check parameter #3");
            }

            Console.WriteLine("Updating shared logic CRC in sharedConfig.json finished. Resulting CRC = " + crc);
            return 0;
        }
    }
}
