using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceCommon;
using System.Reflection;

namespace PestelCrew.Service
{
    class PestelCrewServiceActivator
    {
        string _assemblyPath;
        string _typeName;
        public PestelCrewServiceActivator(string assemblyPath, string typeName)
        {
            _assemblyPath = assemblyPath;
            _typeName = typeName;
        }

        public PestelCrewService Instantiate()
        {
            var asm = Assembly.LoadFrom(_assemblyPath);
            var type = asm.GetType(_typeName);
            var service = Activator.CreateInstance(type) as PestelCrewService;
            if (service == null) throw new Exception($"'{_typeName}' from assembly '{_assemblyPath}' doesn't implement PestelCrewService");
            return service;
        }
    }
}
