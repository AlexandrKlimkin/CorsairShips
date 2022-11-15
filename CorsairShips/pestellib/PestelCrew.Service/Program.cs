using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PestelLib.ServerCommon;

namespace PestelCrew.Service
{
    class AssemblyType
    {
        public string AssemblyPath;
        public string Type;

        public AssemblyType(string assemblyPath, string type)
        {
            AssemblyPath = assemblyPath;
            Type = type;
        }
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            Log.Init();
            var config = args[0];
            var servicesParams = new List<AssemblyType>();
            using (var r = new StreamReader(File.OpenRead(config)))
            {
                servicesParams.Add(new AssemblyType(r.ReadLine(), r.ReadLine()));
            }
            // only for testing
            if (args[0] == "/nonservice")
            {
                var services = servicesParams.Select(sp => Create(sp));
                foreach (var s in services)
                    s.Start();

                while (true)
                {
                    var line = Console.ReadLine();
                    if (line == "exit")
                        break;
                }
                foreach (var s in services)
                    s.Stop();
            }
            else
            {
                var ServicesToRun = new List<ServiceBase>();
                foreach (var s in servicesParams)
                {
                    ServicesToRun.Add(new Service(s.AssemblyPath, s.Type));
                }
                ServiceBase.Run(ServicesToRun.ToArray());
            }
        }

        static ServiceCommon.PestelCrewService Create(AssemblyType assemblyType)
        {
            var serviceActivator = new PestelCrewServiceActivator(assemblyType.AssemblyPath, assemblyType.Type);
            return serviceActivator.Instantiate();
        }
    }
}
