using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using ServiceCommon;
using System.IO;

namespace PestelCrew.Service
{
    public partial class Service : ServiceBase
    {
        PestelCrewService _service;
        string _assemblyName;
        string _typeName;
        public Service(string assemblyName, string typeName)
        {
            _assemblyName = assemblyName;
            _typeName = typeName;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var serviceActivator = new PestelCrewServiceActivator(_assemblyName, _typeName);
            _service = serviceActivator.Instantiate();
            _service.Start();
        }
        protected override void OnStop()
        {
            if (_service == null)
                return;

            _service.Stop();
            _service = null;
        }
    }
}
