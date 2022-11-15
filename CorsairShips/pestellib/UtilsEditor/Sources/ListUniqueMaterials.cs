using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace PestelLib.UtilsEditor.Sources
{
    public class ListUniqueMaterials
    {
        [MenuItem("PestelCrew/List unique materials")]
        public static void ListMat()
        {
            var results = new Dictionary<Texture, HashSet<GameObject>>();

            CollectMaterials(results);

            var report = new StringBuilder();
            int materialNumber = 1;
            foreach (var mat in results.Keys)
            {
                report.AppendLine(string.Format("{0}: {1}", materialNumber++, mat.name));

                foreach (var gameObjects in results[mat])
                {
                    report.AppendLine(gameObjects.name);
                }

                report.AppendLine("");
            }
            Debug.Log(report.ToString());
        }

        private static void CollectMaterials(Dictionary<Texture, HashSet<GameObject>> results)
        {
            foreach (var gameObject in Selection.gameObjects)
            {
                var renderers = gameObject.GetComponent<Renderer>();
                var materials = renderers.sharedMaterials;

                foreach (var material in materials)
                {
                    if (!results.ContainsKey(material.mainTexture))
                    {
                        results.Add(material.mainTexture, new HashSet<GameObject>(new[] { gameObject }));
                    }
                    else
                    {
                        if (!results[material.mainTexture].Contains(gameObject))
                        {
                            results[material.mainTexture].Add(gameObject);
                        }
                    }
                }
            }
        }
    }
}
