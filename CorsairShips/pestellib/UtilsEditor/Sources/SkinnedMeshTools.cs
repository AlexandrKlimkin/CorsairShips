using System.Collections.Generic;
using System.Text;
using PestelLib.Utils;
using UnityEditor;
using UnityEngine;

namespace PestelLib.UtilsEditor
{
    public class SkinnedMeshTools 
    {
        [MenuItem("PestelCrew/SkinTools/Print bones")]
        public static void PrintBones()
        {
            var go = Selection.activeGameObject;
            var skin = go.GetComponent<SkinnedMeshRenderer>();
            foreach (var skinBone in skin.bones)
            {
                Debug.Log(skinBone.FullPath());
            }
            Debug.Log("total bones: " + skin.bones.Length);
        }

        [MenuItem("PestelCrew/SkinTools/Rebind bones by name (find bones in sibilings) (LOD1)")]
        public static void RebindBones()
        {
            var go = Selection.activeGameObject;
            var skin = go.GetComponent<SkinnedMeshRenderer>();
            DoRebind(skin.transform.parent);
        }

        [MenuItem("PestelCrew/SkinTools/Rebind bones by name (find bones in children) (LOD0)")]
        public static void RebindBones2()
        {
            var go = Selection.activeGameObject;
            var skin = go.GetComponent<SkinnedMeshRenderer>();
            DoRebind(skin.transform);
        }

        public static void DoRebind(Transform root)
        {
            var go = Selection.activeGameObject;
            var skin = go.GetComponent<SkinnedMeshRenderer>();

            var newBones = new List<Transform>(skin.bones.Length);
            var bonesListDebug = new StringBuilder();

            foreach (var skinBone in skin.bones)
            {
                var newBone = root.Find(skinBone.name);
                newBones.Add(newBone);
                bonesListDebug.AppendLine(newBone.name);
            }

            skin.bones = newBones.ToArray();
            Debug.Log("done: " + bonesListDebug);
        }
    }
}