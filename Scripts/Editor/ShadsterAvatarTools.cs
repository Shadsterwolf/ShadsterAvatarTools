using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEditor.Animations;
#if VRC_SDK_VRCSDK3 && !UDON
using VRC.Core;
using VRC.SDKBase;
using VRC.SDK3.Avatars.Components;
//using VRC_AvatarDescriptor = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
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
        private static GameObject ignorePhysImmobilePrefab;

        public static bool GetStartPlayModeInSceneView()
        {
            if (startInSceneViewPrefab != null)
            {
                return true;
            }
            if (GameObject.Find("StartInSceneView(Clone)") != null)
            {
                startInSceneViewPrefab = GameObject.Find("StartInSceneView(Clone)");
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

        public static bool GetIgnorePhysImmobile()
        {
            if (ignorePhysImmobilePrefab != null)
            {
                return true;
            }
            if (GameObject.Find("IgnorePhysImmobile(Clone)") != null)
            {
                ignorePhysImmobilePrefab = GameObject.Find("IgnorePhysImmobile(Clone)");
                return true;
            }
            return false;
        }

        public static void SetIgnorePhysImmobile(bool flag)
        {
            //var prefabPath = AssetDatabase.GUIDToAssetPath("38bc44479eca0c9409fb9a16a3a9a873");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ShadstersAvatarTools/Prefabs/IgnorePhysImmobile.prefab");
            if (flag)
                ignorePhysImmobilePrefab = Instantiate(prefab);
            else
                DestroyImmediate(ignorePhysImmobilePrefab, true);
        }

        public static VRCAvatarDescriptor SelectCurrentAvatarDescriptor()
        {
            VRCAvatarDescriptor avatarDescriptor = null;
            //Get current selected avatar
            if (Selection.activeTransform && Selection.activeTransform.root.gameObject.GetComponent<VRCAvatarDescriptor>() != null)
            {
                avatarDescriptor = Selection.activeTransform.root.GetComponent<VRCAvatarDescriptor>();
                if (avatarDescriptor != null)
                    return avatarDescriptor;
            }
            //Find first potential avatar
            var potentialObjects = UnityEngine.Object.FindObjectsOfType<VRCAvatarDescriptor>().ToArray();
            if (potentialObjects.Length > 0)
            {
                avatarDescriptor = potentialObjects.First();
            }

            return avatarDescriptor;
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

        public static AnimatorController GetFxController(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            var runtime = vrcAvatarDescriptor.baseAnimationLayers[4].animatorController;
            //return AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(runtime));
            return (AnimatorController)runtime;
        }

        public static void BackupController(AnimatorController ac)
        {
            if (ac == null)
            {
                Debug.LogWarning("No Animator Controller selected.");
                return;
            }

            string path = AssetDatabase.GetAssetPath(ac);
            string newPath = AssetDatabase.GenerateUniqueAssetPath(Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(ac.name) + "_Backup" + Path.GetExtension(path));
            AnimatorController newController = UnityEditor.Animations.AnimatorController.Instantiate(ac);
            AssetDatabase.CreateAsset(newController, newPath);
            AssetDatabase.SaveAssets();

            Debug.Log("Animator controller cloned and saved at " + newPath);
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

        public static void Export()
        {
            //Setup
            var startPlayModePrefabExists = GetStartPlayModeInSceneView();
            var ignorePhysImmobilePrefabExists = GetIgnorePhysImmobile();
            //Cleanup
            var avatarDescriptors = Resources.FindObjectsOfTypeAll<VRC_AvatarDescriptor>();
            foreach (var avatarDescriptor in avatarDescriptors)
            {
                ClearAvatarBlueprintID(avatarDescriptor.gameObject);
            }
            if (startPlayModePrefabExists) { SetStartPlayModeInSceneView(false); }
            if (ignorePhysImmobilePrefabExists) { SetIgnorePhysImmobile(false); }
            //Export
            var scenePath = SceneManager.GetActiveScene().path;
            var path = scenePath.Substring(0, scenePath.LastIndexOf("/"));
            var name = path.Substring(path.IndexOf("/") + 1);
            AssetDatabase.ExportPackage(path, name + ".unitypackage", ExportPackageOptions.Recurse);
            //Reset
            if (startPlayModePrefabExists) { SetStartPlayModeInSceneView(true); }
            if (ignorePhysImmobilePrefabExists) { SetIgnorePhysImmobile(true); }
            //Open
            EditorUtility.RevealInFinder(Application.dataPath);
        }

        public static void MovePhysBonesFromArmature(GameObject vrcAvatar)
        {
            var armature = vrcAvatar.transform.Find("Armature");
            var physbones = armature.GetComponentsInChildren<VRC_PhysBone>();
            if (physbones.Length == 0)
            {
                EditorUtility.DisplayDialog("No PhysBones found!", "There are no PhysBones attached to armature in " + vrcAvatar.name, "Ok");
                return;
            }
            if (vrcAvatar.transform.Find("PhysBones") != null)
            {
                EditorUtility.DisplayDialog("PhysBones Object exists!", "PhysBones Object already exists for this avatar!", "Ok");
                return;
            }
            var physObjectRoot = new GameObject("PhysBones");
            physObjectRoot.transform.parent = vrcAvatar.transform;
            foreach (var pBone in physbones)
            {
                var physObject = new GameObject(pBone.name);
                physObject.transform.parent = physObjectRoot.transform;
                var copyPBone = CopyComponent(pBone, physObject);

                if (pBone.rootTransform == null)
                {
                    copyPBone.rootTransform = pBone.transform;
                }
                DestroyImmediate(pBone);
            }
        }

        public static void MovePhysCollidersFromArmature(GameObject vrcAvatar)
        {
            var armature = vrcAvatar.transform.Find("Armature");
            var physcolliders = armature.GetComponentsInChildren<VRC_PhysCollider>();
            var physbones = vrcAvatar.GetComponentsInChildren<VRC_PhysBone>();
            if (physcolliders.Length == 0)
            {
                EditorUtility.DisplayDialog("No PhysColliders found!", "There are no PhysColliders attached to armature in " + vrcAvatar.name, "Ok");
                return;
            }
            if (vrcAvatar.transform.Find("PhysColliders") != null)
            {
                EditorUtility.DisplayDialog("PhysCollider Object exists!", "PhysCollider Object already exists for this avatar!", "Ok");
                return;
            }
            var physObjectRoot = new GameObject("PhysColliders");
            physObjectRoot.transform.parent = vrcAvatar.transform;
            foreach (var pCollider in physcolliders)
            {
                var physObject = new GameObject(pCollider.name);
                physObject.transform.parent = physObjectRoot.transform;
                var copyPCollider = CopyComponent(pCollider, physObject);

                if (pCollider.rootTransform == null)
                {
                    copyPCollider.rootTransform = pCollider.transform;
                }

                foreach (var pBone in physbones) //Move all possible colliders from physbones
                {
                    for (int i = 0; i < pBone.colliders.Count; i++)
                    {
                        if (pBone.colliders[i] == pCollider)
                        {
                            pBone.colliders[i] = copyPCollider;
                        }
                    }
                }
                DestroyImmediate(pCollider);
            }
        }

        public static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            var type = original.GetType();
            var copy = destination.AddComponent(type);
            var fields = type.GetFields();
            foreach (var field in fields) field.SetValue(copy, field.GetValue(original));
            return copy as T;
        }

        public static string GetCurrentSceneRootPath()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            string scenePath = currentScene.path;
            string currentPath = Path.GetDirectoryName(scenePath);
            currentPath = currentPath.Replace("\\", "/"); //I am suffering
            return currentPath;

        }

        public static List<AnimationClip> GetAllGeneratedAnimationClips()
        {
            string folderPath = GetCurrentSceneRootPath() + "/Animations/Generated";
            string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new string[] { folderPath });
            List<AnimationClip> clips = new List<AnimationClip>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                clips.Add(clip);
            }
            if (clips.Count == 0)
            {
                return null;
            }
            else
            {
                return clips;
            }
        }

        public static void CleanupUnusedGeneratedAnimations()
        {
            AnimatorController[] aControllers = Resources.FindObjectsOfTypeAll<AnimatorController>();
            List<AnimationClip> generatedClips = GetAllGeneratedAnimationClips();
            foreach (AnimatorController aController in aControllers)
            {
                AnimationClip[] controllerClips = aController.animationClips;
                foreach (AnimationClip controllerClip in controllerClips)
                {
                    if (generatedClips.Contains(controllerClip))
                    {
                        //Debug.Log(controllerClip.name + " does exist in " + aController.name);
                        generatedClips.Remove(controllerClip); //clip is used, remove from list
                    }
                }
                //Debug.Log(generatedClips.Count);
            }
            foreach (AnimationClip unusedClip in generatedClips) 
            {
                var unusedClipPath = AssetDatabase.GetAssetPath(unusedClip);
                AssetDatabase.DeleteAsset(unusedClipPath);
            }

        }

    }
}
