using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shadster.AvatarTools
{
    public class Materials
    {
        public static Material[] GetUniqueMaterials(GameObject obj)
        {
            if (obj != null)
            {
                SkinnedMeshRenderer[] skinnedMeshRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
                HashSet<Material> uniqueMaterials = new HashSet<Material>();
                foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
                {
                    Material[] materials = renderer.sharedMaterials;
                    foreach (Material material in materials)
                    {
                        uniqueMaterials.Add(material);
                    }
                }
                Material[] uniqueMaterialArray = new Material[uniqueMaterials.Count];
                uniqueMaterials.CopyTo(uniqueMaterialArray);

                //foreach (Material mat in uniqueMaterialArray) //DEBUG
                //{
                //    Debug.Log(mat.name);
                //}
                return uniqueMaterialArray;
            }
            return new Material[0];
        }

        public static MeshRenderer[] ReadChildMeshRenderers(GameObject gc)
        {
            MeshRenderer[] meshRenderers = gc.GetComponentsInChildren<MeshRenderer>();

            if (meshRenderers.Length == 0)
            {
                Debug.Log("The selected GameObject has no child MeshRenderers.");
            }
            else
            {
                for (int i = 0; i < meshRenderers.Length; i++)
                {
                    Debug.Log("Child MeshRenderer " + (i + 1) + ": " + meshRenderers[i].name);
                }
            }
            return meshRenderers;
        }

    }
}
