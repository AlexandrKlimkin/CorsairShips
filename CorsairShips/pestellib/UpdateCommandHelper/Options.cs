using CommandLine;

namespace UpdateCommandHelper
{
    class Options
    {
        [Option("projDir", Default = "")]
        public string ProjectDir { get; set; }

        [Option("protocolFile", Default = "ProjectLib/ConcreteSharedLogic/Sources/UserProfile.cs")]
        public string ProtocolFilePath { get; set; }

        [Option("wrapperFile", Default = "Assets/CommandHelper.cs")]
        public string WrapperFilePath { get; set; }

        [Option("sharedLogicCoreFile", Default = "ProjectLib/ConcreteSharedLogic/Sources/SharedLogicCore.cs")]
        public string SharedLogicCorePath { get; set; }

        [Option("autoCommandsNamespace", Default = "S")]
        public string AutoCommandsNamespace { get; set; }

        [Option("definitionsFile", Default = "ProjectLib/ConcreteSharedLogic/Sources/Definitions.cs")]
        public string DefinitionsFilePath { get; set; }
    }
}