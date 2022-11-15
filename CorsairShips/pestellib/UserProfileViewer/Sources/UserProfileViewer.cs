using MessagePack;
using UnityDI;
using PestelLib.SharedLogic;
using S;
using UnityEngine;
using PestelLib.SharedLogicBase;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

public class UserProfileViewer : MonoBehaviour {
    public UserProfile readonlyUserProfile;
    public object[] modulesStates;
    public Dictionary<object, List<MethodInfo>> ModulesPublicMethods = new Dictionary<object, List<MethodInfo>>();

    public ISharedLogic sharedLogic;

    IEnumerator Start() {
        while (sharedLogic == null)
        {
            sharedLogic = ContainerHolder.Container.Resolve<ISharedLogic>();
            yield return new WaitForSeconds(0.25f);
        }
    	Refresh();
    }

    [Conditional("UNITY_EDITOR")]
    public void Refresh() {
        if (sharedLogic == null)
            return;
        readonlyUserProfile = MessagePackSerializer.Deserialize<UserProfile>(sharedLogic.SerializedState);
        var state = MessagePackSerializer.Deserialize<UserProfile>(sharedLogic.SerializedState);
        var prop = sharedLogic.GetType().GetProperty("SerializableModules");
        if(prop == null)
            return;
        var mods = (List<Type>)prop.GetValue(sharedLogic, null);
        var getModule = sharedLogic.GetType().GetMethod("GetModule");
        var moduleStates = new List<object>();
        ModulesPublicMethods.Clear();
        foreach (var m in mods)
        {
            var moduleInst = (ISharedLogicModule)getModule.MakeGenericMethod(m).Invoke(sharedLogic, null);
            var methodsList = ModulesPublicMethods[moduleInst] = new List<MethodInfo>();
            var t = moduleInst.GetType();
            var methods = t.GetMethods()
                .Where(
                    method => method.IsPublic && 
                    method.DeclaringType == t && 
                    !method.IsVirtual &&
                    method.GetCustomAttributes(false)
                .All(a => a.GetType() != typeof(SharedCommand)))
                .ToArray();

            methodsList.AddRange(methods);
            if (moduleInst.RawState != null)
            {
                moduleStates.Add(moduleInst.RawState);
            }
        }
        modulesStates = moduleStates.ToArray();
    }
}