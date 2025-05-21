using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

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

        public static string GetMaterialPath(Material material)
        {
            if (material == null) return null;
            var path = AssetDatabase.GetAssetPath(material);
            //Debug.Log("GetMaterialPath " + path);
            return path;
            
        }

        public static Material FindOrCreateQuestMaterial(Material oriMat)
        {
            if (oriMat == null) return null;

            string originalPath = GetMaterialPath(oriMat);
            if (string.IsNullOrEmpty(originalPath)) return null;

            string directory = Path.GetDirectoryName(originalPath);
            string newMatName = oriMat.name + " 1";
            string newMatPath = Path.Combine(directory, newMatName + ".mat").Replace("\\", "/");

            Material existing = AssetDatabase.LoadAssetAtPath<Material>(newMatPath);

            if (existing != null)
            {
                //Debug.Log("Found existing material" + existing.name);
                return existing;
            }

            Material newMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            newMat.name = newMatName;
            TransferAlbedoTexture(oriMat, newMat);
            //Debug.Log("Created New Material " + newMat.name);
            AssetDatabase.CreateAsset(newMat, newMatPath);
            AssetDatabase.SaveAssets();
            return newMat;
        }

        public static void TransferAlbedoTexture(Material matA, Material matB)
        {
            if (matA == null || matB == null)
            {
                Debug.LogError("No valid given material");
                return;
            }
            
            Texture texA = matA.GetTexture("_MainTex");
            if (texA == null)
            {
                Debug.LogWarning("No Albedo texture found in " + matA.name);
            }
            else
            {
                matB.SetTexture("_MainTex", texA);
            }
        }

        public static void ConvertMaterialsToQuestToon(GameObject obj)
        {
            var materials = new List<Material>();
            var renderers = obj.GetComponentsInChildren<Renderer>(true);

            foreach (var renderer in renderers)
            {
                materials.AddRange(renderer.sharedMaterials);
                Material[] newMaterials = new Material[materials.Count];
                for (int i = 0; i < newMaterials.Length; i++)
                {
                    newMaterials[i] = FindOrCreateQuestMaterial(materials[i]);
                }
                renderer.sharedMaterials = newMaterials;
            }
        }

        public static void ConvertAvatarMaterialsToQuestToon(GameObject vrcAvatar)
        {
            List<GameObject> objs = Helper.GetRenderersInChildren(vrcAvatar);
            foreach (GameObject obj in objs)
            {
                Undo.RecordObject(obj, "Convert Avatar Materials To Quest Toon");
                ConvertMaterialsToQuestToon(obj);
                EditorUtility.SetDirty(obj);
            }
        }

    }
}
