using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

#if VRC_SDK_VRCSDK3
using VRC.Core;
using VRC.SDKBase;
#endif

#if VRC_SDK_VRCSDK3 && !UDON
using VRC_AvatarDescriptor = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
using VRC_SpatialAudioSource = VRC.SDK3.Avatars.Components.VRCSpatialAudioSource;
using VRC_PhysBone = VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone;
using VRC_PhysCollider = VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBoneCollider;
using VRC.SDK3.Dynamics.PhysBone.Components;
using UnityEngine.SceneManagement;
#endif

namespace Shadster.AvatarTools
{

    public class ShadstersAvatarTools : EditorWindow
    {
        private static GameObject startInSceneViewPrefab;

        public static bool GetStartPlayModeInSceneView()
        {
            if (startInSceneViewPrefab != null)
            {
                return true;
            }
            return false;
        }

        public static void SetStartPlayModeInSceneView(bool flag)
        {
            //var prefabPath = AssetDatabase.GUIDToAssetPath("38bc44479eca0c9409fb9a16a3a9a873");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ShadstersAvatarTools/Prefabs/StartInSceneView.prefab");
            if (flag)
                startInSceneViewPrefab = Instantiate(prefab);
            else
                DestroyImmediate(startInSceneViewPrefab, true);
        }

        public static void ClearAvatarBlueprintID(GameObject vrcAvatar)
        {
            PipelineManager blueprint = vrcAvatar.GetComponent<PipelineManager>();
            blueprint.blueprintId = null;
        }


        public static bool BoneHasPhysBones(Transform bone)
        {
            if (bone.GetComponent<VRC_PhysBone>() != null)
            {
                return true;
            }
            return false;
        }

        public static List<VRC_PhysBone> GetAllAvatarPhysBones(GameObject vrcAvatar)
        {
            Transform armature = vrcAvatar.transform.Find("Armature");
            List<VRC_PhysBone> result = new List<VRC_PhysBone>();
            if (armature != null)
            {
                foreach (Transform bone in vrcAvatar.GetComponentsInChildren<Transform>(true))
                {
                    if (BoneHasPhysBones(bone))
                    {
                        VRC_PhysBone pBone = bone.GetComponent<VRC_PhysBone>();
                        result.Add(pBone);
                    }


                }
            }
            //Debug.Log(result);
            return result;
        }

        public static void Export()
        {
            //Setup
            var prefabExists = GetStartPlayModeInSceneView();
            //Cleanup
            var avatarDescriptors = Resources.FindObjectsOfTypeAll<VRC_AvatarDescriptor>();
            foreach (var avatarDescriptor in avatarDescriptors)
            {
                ClearAvatarBlueprintID(avatarDescriptor.gameObject);
            }
            if (prefabExists) { SetStartPlayModeInSceneView(false); }
            //Export
            var scenePath = SceneManager.GetActiveScene().path;
            var path = scenePath.Substring(0, scenePath.LastIndexOf("/"));
            var name = path.Substring(path.IndexOf("/") + 1);
            AssetDatabase.ExportPackage(path, name + ".unitypackage", ExportPackageOptions.Recurse);
            //Reset
            if (prefabExists) { SetStartPlayModeInSceneView(true); }
            //Open
            EditorUtility.RevealInFinder(Application.dataPath);
        }

    }
}
