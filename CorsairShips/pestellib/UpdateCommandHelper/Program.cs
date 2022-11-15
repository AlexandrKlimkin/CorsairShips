using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;
using PestelLib.ServerShared;

namespace UpdateCommandHelper
{
    class Program
    {
        private static Options options = new Options();

        private static void UpdateCommandHelper()
        {
            string line;
            int lineCount = 0;

            bool lookingForCommand = false;

            var methods = new List<Method>();

            var s = Path.DirectorySeparatorChar;
            var pathToModules = string.Format("{1}Assets{0}Plugins{0}LibsSymlinks{0}ConcreteSharedLogic{0}Modules{0}", s, options.ProjectDir);
            ModuleIntegrator.Generate(options, pathToModules);
            var modules = Directory.GetFiles(pathToModules, "*.cs", SearchOption.AllDirectories);
            foreach (var f in modules)
            {
                System.IO.StreamReader file = new System.IO.StreamReader(f);

                string desc = "";

                while ((line = file.ReadLine()) != null)
                {
                    lineCount++;
                    
                    if (line.Contains("[SharedCommand"))
                    {
                        lookingForCommand = true;

                        Regex regex = new Regex(@"\[\s*SharedCommand\s*\(\s*""(.*)""\s*\)\s*\]");
                        Match match = regex.Match(line);
                        desc = match.Groups.Count == 2 ? match.Groups[1].Value : "";
                    }

                    if (lookingForCommand)
                    {
                        var declBegin = line.IndexOf("internal");
                        if (declBegin > -1)
                        {
                            var declLine = line.Substring(declBegin);
                            var parts = line.Split('(');
                            if (parts.Length != 2)
                            {
                                logError(f, lineCount, 1);
                                goto next;
                            }

                            parts[0] = parts[0].Trim();
                            parts[0] = parts[0].Replace("internal", "");
                            parts[0] = parts[0].Trim();

                            var methodParts = parts[0].Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                            if (methodParts.Length != 2)
                            {
                                logError(f, lineCount, 2);
                                goto next;
                            }

                            string returnType = methodParts[0];
                            string methodName = methodParts[1];
                            var methodArgs = new List<List<string>>();

                            parts[1] = parts[1].Trim();

                            parts[1] = parts[1].Split(')')[0];
                            var methodArgsParts = parts[1].Split(',');
                            foreach (var a in methodArgsParts)
                            {
                                var arg = a.Trim();
                                var argParts = arg.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                                if (argParts.Length == 2)
                                {
                                    methodArgs.Add(new List<string> { argParts[0], argParts[1] });
                                }
                                else if (argParts.Length != 0)
                                {
                                    logError(f, lineCount, 4);
                                    goto next;
                                }
                            }

                            methods.Add(new Method()
                            {
                                moduleName = Path.GetFileNameWithoutExtension(f),
                                returnType = returnType,
                                methodName = methodName,
                                arguments = methodArgs,
                                description = desc,
                                modulePath = Path.GetDirectoryName(f)
                            });

                            Console.WriteLine(line + " " + desc);

                            next:
                            lookingForCommand = false;
                        }
                    }
                }
                lineCount = 0;
                file.Close();
            }

            Generate(methods);
        }

        public static void Generate(List<Method> methods)
        {
            var protocolFilePath = options.ProtocolFilePath;
            var wrapperFilePath = options.WrapperFilePath;
            var sharedLogicCorePath = options.SharedLogicCorePath;

            StringBuilder wrapperCode = new StringBuilder();
            StringBuilder slCode = new StringBuilder();
            Dictionary<string, StringBuilder> protoCode = new Dictionary<string, StringBuilder>();

            StringBuilder commandsContainer = new StringBuilder();
            var commandIndex = CommandIndexStart;

            foreach (var method in methods)
            {
                //commands Proto 
                StringBuilder commandParameters = new StringBuilder();
                var paramIndex = 1;
                foreach (var arg in method.arguments)
                {
                    commandParameters.AppendFormat("\r\n        [MessagePack.Key({2})]\r\n        public {0} {1} {{ get; set;}}\r\n",
                        arg[0], arg[1], paramIndex++);
                }
                if (commandParameters.Length != 0)
                    commandParameters.Append("\t");

                var cmdName = method.moduleName + "_" + method.methodName;
                var cmd = protobuffCommandTemplate.Replace("%COMMAND_NAME%", cmdName);
                cmd = cmd.Replace("%VARIABLES%", commandParameters.ToString());

                if (!protoCode.ContainsKey(method.modulePath))
                {
                    protoCode[method.modulePath] = new StringBuilder();
                }

                protoCode[method.modulePath].AppendLine(cmd);
                commandsContainer.AppendFormat("        [MessagePack.Key({1})]\r\n        public {0} {0} {{ get; set;}}\r\n", cmdName,
                    commandIndex++);


                //Commands shared logic code
                var slCodeInst = slCodeTemplate.Replace("%CMD%", cmdName).Replace("%CMD_CRC%", CommandCode.CodeByName(cmdName).ToString());
                var call = string.Format("   GetModule<{0}>().{1}({2});",
                    method.moduleName,
                    method.methodName,
                    String.Join(", ", method.arguments.Select(x => "cmd." + x[1]).ToArray())
                    );

                if (method.returnType != "void")
                {
                    call = "result = (object)" + call;
                }

                slCodeInst = slCodeInst.Replace("%CALL%", call);
                slCodeInst = slCodeInst.Replace("%ELSE%", (commandIndex > (CommandIndexStart + 1) ? "else " : ""));
                slCode.AppendLine(slCodeInst);

                //commands wrapper
                var wrapperTemplate = method.returnType == "void"
                    ? wrapperTemplateVoid
                    : wrapperTemplateValue;

                var wrapperCmd = wrapperTemplate.Replace("%METHOD_NAME%", method.methodName);
                wrapperCmd = wrapperCmd.Replace("%MODULE_NAME%", method.moduleName);
                wrapperCmd = wrapperCmd.Replace("%CMD_NAME%", cmdName);
                wrapperCmd = wrapperCmd.Replace("%CMD_CRC%", CommandCode.CodeByName(cmdName).ToString());

                if (method.returnType != "void")
                {
                    wrapperCmd = wrapperCmd.Replace("%RETURN_TYPE%", method.returnType);
                }

                wrapperCmd = wrapperCmd.Replace("%DESCRIPTION%", method.description);
                wrapperCmd = wrapperCmd.Replace("%METHOD_PARAMS%",
                    String.Join(",", method.arguments.Select(x => string.Format("{0} {1}", x[0], x[1])).ToArray())
                    );
                wrapperCmd = wrapperCmd.Replace("%PARAMS_ASSIGN%",
                    String.Join(",", method.arguments.Select(x => string.Format("{0} = {1}\r\n", x[1], x[1])).ToArray())
                    );
                // @TODO: починить [Obsolete]
                // if (attrs.Any(x => x is ObsoleteAttribute))
                // {
                //     wrapperCode.AppendLine("[Obsolete]");
                // }
                wrapperCode.AppendLine(wrapperCmd);
            }

            var commandsContainerCode = autoCommandTemplate.Replace("%COMMANDS%", commandsContainer.ToString());
            //protoCode.AppendLine(commandsContainerCode);
            foreach (var pair in protoCode)
            {
                string code = string.Format("using PestelLib.SharedLogic.Modules; \r\nnamespace {0}{{\r\n{1}\r\n}}", 
                    options.AutoCommandsNamespace,
                    pair.Value.ToString()
                );
                var fname = Path.GetFileName(pair.Key);
                File.WriteAllText(pair.Key + Path.DirectorySeparatorChar + fname + ".Generated.cs", code);
            }

            //update shared logic core
            FileHelper.ReplaceRegion(sharedLogicCorePath, "AUTO_GENERATED_COMMAND_WRAPPER", slCode.ToString());

            //update wrapper
            var resultingWrapperCode = wrapperContainerTemplate
                .Replace("%METHODS%", wrapperCode.ToString())
                .Replace("%AUTO_COMMANDS_NAMESPACE%", options.AutoCommandsNamespace);
            File.WriteAllText(wrapperFilePath, resultingWrapperCode);

            //update protocol:
            //FileHelper.ReplaceRegion(protocolFilePath, "AUTO_GENERATED", protoCode.ToString());

            Console.WriteLine("Command generation done.");
            
        }

        private const string moduleClassTemplate =
            @"        
public class MyTestModuleLogic
{
    %COMMANDS%
}
";

        private const string wrapperTemplateVoid =
            @"
public partial class %MODULE_NAME%
{
    [SharedCommand(""%DESCRIPTION%"")]
    public static void %METHOD_NAME%(%METHOD_PARAMS%)
    {
        var cmd = new S.CommandContainer
        {
            CommandId = %CMD_CRC%,
            CommandData = Serializer.Serialize(new %CMD_NAME%
            {
                %PARAMS_ASSIGN%
            })
        };

        var cmdBytes = Serializer.Serialize(cmd);
        CommandProcessor.ExecuteCommand<object>(cmdBytes);
    }
}
";

        private const string wrapperTemplateValue =
            @"
public partial class %MODULE_NAME%
{

    [SharedCommand(""%DESCRIPTION%"")]
    public static %RETURN_TYPE% %METHOD_NAME%(%METHOD_PARAMS%)
    {
        var cmd = new S.CommandContainer
        {
            CommandId = %CMD_CRC%,
            CommandData = Serializer.Serialize(new %CMD_NAME%
            {
                %PARAMS_ASSIGN%
            })
        };

        var cmdBytes = Serializer.Serialize(cmd);
        return CommandProcessor.ExecuteCommand<%RETURN_TYPE%>(cmdBytes);
    }
}
";

        private const string wrapperContainerTemplate =
            @"
using System.Collections.Generic;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicBase;
using PestelLib.ServerShared;
using PestelLib.UniversalSerializer;
using UnityDI;
using %AUTO_COMMANDS_NAMESPACE%;

namespace PestelLib.SharedLogicClient {
    public class SharedLogicCommand
    {
        private static CommandProcessor _commandProcessor;

        private static CommandProcessor CommandProcessor
        {
            get
            {
                if (_commandProcessor == null)
                {
                    _commandProcessor = ContainerHolder.Container.Resolve<CommandProcessor>();
                }
                return _commandProcessor;
            }
        }

        %METHODS%
    }
}
";

        private const string slCodeTemplate =
            @"                %ELSE%if (autoCommand.CommandId == %CMD_CRC%) {
                    var cmd = Serializer.Deserialize<%CMD%>(autoCommand.CommandData);
                    %CALL%
                }
";

        private const string autoCommandTemplate =
            @"    [MessagePack.MessagePackObject]
    public class AutoCommands {
%COMMANDS%
    }
";

        private const string protobuffCommandTemplate =
            @"    [MessagePack.MessagePackObject]
    public class %COMMAND_NAME% {%VARIABLES%}
";

        private const int CommandIndexStart = 100;

        private static void logError(string module, int line, int code)
        {
            Console.WriteLine("(" + code + ")" + "Something wrong in module: " + Path.GetFileNameWithoutExtension(module) +
                              " around line " + (line) + ". Please declare shared command method in one line");
        }

        static int Main(string[] args)
        {
            DualOut.Init();

            IEnumerable<Error> errors = null;
            Parser.Default.ParseArguments<Options>(args).WithParsed((o) => options = o).WithNotParsed((e) => errors = e);

            if (errors != null)
            {
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
                return 1;
            }

            var pwd = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            options.ProjectDir = string.IsNullOrEmpty(options.ProjectDir) ? Path.GetFullPath(Path.Combine(pwd, "..", "..", "..") + Path.DirectorySeparatorChar) : Path.GetFullPath(options.ProjectDir);
            options.SharedLogicCorePath = options.ProjectDir + options.SharedLogicCorePath;
            options.ProtocolFilePath = options.ProjectDir + options.ProtocolFilePath;
            options.WrapperFilePath = options.ProjectDir + options.WrapperFilePath;
            options.DefinitionsFilePath = options.ProjectDir + options.DefinitionsFilePath;

            Console.WriteLine($"ProjectDir: {options.ProjectDir}");

            UpdateCommandHelper();
            
            return 0;
        }
    }
}
