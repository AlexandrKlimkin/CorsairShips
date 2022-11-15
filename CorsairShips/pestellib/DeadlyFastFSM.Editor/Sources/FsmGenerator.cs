using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeadlyFast;
using UnityEditor;
using UnityEngine;
using FsmUtils = DeadlyFast.Utils;

namespace PestelLib.DeadlyFastFSM.Editor
{
    public class FsmGenerator
    {
        #region TEMPLATES
        private const string FsmClassTemplate = @"using System;

public class %FSM_NAME%
{
    public Action<FsmState> OnEnterState = (state) => {};
    public Action<FsmState> OnExitState = (state) => {};
    public FsmState state { get; private set; }
    %FSM_STATE_ENUM%
    %FSM_EVENT_ENUM%
    public %FSM_NAME%() {
        state = FsmState.%INITIAL_STATE%;
    }
    public void ProcessEvent(FsmEvent evt)
    {
%FSM_EVENT_PROCESSING%
    }

    private void SwitchToState(FsmState newState)
    {
        OnExitState(state);
        state = newState;
        OnEnterState(state);
    }
}
";

        private const string EnumTemplate =
            @"
    public enum %ENUM_NAME%
    {{
{0}
    }};
";
        private const string StateEnumRowTemplate = @"        {0} = {1}";
        private const string StateEnumRowComma = ",\r\n";
        #endregion
        
        public static string GenerateFSMCode(FsmData fsm)
        {
            var controllerFullPath = Application.dataPath.Replace("/Assets", "") + "/" + AssetDatabase.GetAssetPath(fsm);
            var outputPath = Path.GetDirectoryName(controllerFullPath) + "/" + FsmUtils.MakeCodeIdentifier(fsm.name) + ".cs";

            var stateEnumCode = GenerateStatesEnum(fsm);
            var eventsEnumCode = GenerateEventsEnumCode(fsm);
            var processingCode = GenerateProcessingCode(fsm);

            var initialNode = FsmUtils.GetInitialNode(fsm.Nodes);
            if (initialNode == null)
            {
                Debug.LogError("Can't find initial node. Check 'Entry' state and its transitions");
                return null;
            }

            var result = FsmClassTemplate
                .Replace("%FSM_NAME%", FsmUtils.MakeCodeIdentifier(fsm.name))
                .Replace("%FSM_STATE_ENUM%", stateEnumCode)
                .Replace("%FSM_EVENT_ENUM%", eventsEnumCode)
                .Replace("%FSM_EVENT_PROCESSING%", processingCode)
                .Replace("%INITIAL_STATE%", FsmUtils.MakeCodeIdentifier(initialNode.Name));

            File.WriteAllText(outputPath, result);
            AssetDatabase.Refresh();
            return outputPath;
        }

        private static string GenerateProcessingCode(FsmData stateMachine)
        {
            var result = new StringBuilder();

            //transitions from 'any' state
            var anyState = stateMachine.Nodes.FirstOrDefault(x => x.IsAnyState);
            if (anyState != null)
            {
                result.AppendLine("        switch (evt)\r\n        {");
                foreach (var transition in anyState.Transitions)
                {
                    var targetState = FsmUtils.MakeCodeIdentifier(transition.TargetState);
                    if (stateMachine.Nodes.Any(x => FsmUtils.MakeCodeIdentifier(x.Name) == targetState))
                    {
                        result.AppendFormat("            case FsmEvent.{0}: SwitchToState(FsmState.{1}); break;\r\n",
                            transition.Event,
                            transition.TargetState
                            );
                    }
                }
                result.AppendLine("        }");
            }
            
            //regular transitions
            result.AppendLine("        switch (state)\r\n        {");
            foreach (var node in stateMachine.Nodes)
            {
                if (node.IsAnyState || node.IsEntry) continue;

                result.AppendFormat("            case FsmState.{0}:\r\n", FsmUtils.MakeCodeIdentifier(node.Name));
                result.AppendLine("                switch (evt)\r\n                {");
                foreach (var transition in node.Transitions)
                {
                    var targetState = FsmUtils.MakeCodeIdentifier(transition.TargetState);
                    if (stateMachine.Nodes.Any(x => FsmUtils.MakeCodeIdentifier(x.Name) == targetState))
                    {
                        result.AppendFormat("                    case FsmEvent.{0}: SwitchToState(FsmState.{1}); break;\r\n",
                            FsmUtils.MakeCodeIdentifier(transition.Event),
                            targetState
                            );
                    }
                }
                result.AppendLine("                }\r\n                break;");
            }
            result.AppendLine("        }");
            return result.ToString();
        }

        private static string GenerateEventsEnumCode(FsmData fsmData)
        {
            var eventList = new HashSet<string>();
            for (var nodeIndex = 0; nodeIndex < fsmData.Nodes.Count; nodeIndex++)
            {
                var node = fsmData.Nodes[nodeIndex];
                for (var transitionIndex = 0; transitionIndex < node.Transitions.Count; transitionIndex++)
                {
                    if (!eventList.Contains(FsmUtils.MakeCodeIdentifier(node.Transitions[transitionIndex].Event)))
                    {
                        eventList.Add(FsmUtils.MakeCodeIdentifier(node.Transitions[transitionIndex].Event));
                    }
                }
            }
            return StringArrayToEnum(eventList.ToArray()).Replace("%ENUM_NAME%", "FsmEvent");
        }

        private static string GenerateStatesEnum(FsmData stateMachine)
        {
            var stateNames = stateMachine.Nodes.Select(x => FsmUtils.MakeCodeIdentifier(x.Name)).ToArray();
            return StringArrayToEnum(stateNames).Replace("%ENUM_NAME%", "FsmState");
        }

        private static string StringArrayToEnum(string[] names)
        {
            StringBuilder namesList = new StringBuilder();
            for (int i = 0; i < names.Length; i++)
            {
                namesList.Append(string.Format(StateEnumRowTemplate, names[i], i));
                if (i < names.Length - 1)
                {
                    namesList.Append(StateEnumRowComma);
                }
            }
            var stateEnumCode = string.Format(EnumTemplate, namesList);
            return stateEnumCode;
        }
    }
}
