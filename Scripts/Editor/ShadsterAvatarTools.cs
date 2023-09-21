using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEditor.Animations;
using System.Collections;
using UnityEngine.Animations;
#if VRC_SDK_VRCSDK3
using VRC.Core;
using VRC.SDKBase;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
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
        private static GameObject testPhysbonesPrefab;

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

        public static bool GetTestPhysbones()
        {
            if (testPhysbonesPrefab != null)
            {
                return true;
            }
            if (GameObject.Find("TestPhysbones(Clone)") != null)
            {
                testPhysbonesPrefab = GameObject.Find("TestPhysbones(Clone)");
                return true;
            }
            return false;
        }

        public static void SetTestPhysbones(bool flag)
        {
            //var prefabPath = AssetDatabase.GUIDToAssetPath("38bc44479eca0c9409fb9a16a3a9a873");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ShadstersAvatarTools/Prefabs/TestPhysbones.prefab");
            if (flag)
                testPhysbonesPrefab = Instantiate(prefab);
            else
                DestroyImmediate(testPhysbonesPrefab, true);
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
                foreach (var pBone in vrcAvatar.GetComponentsInChildren<VRC_PhysBone>(true))
                {
                    result.Add(pBone);
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

        public static void CleanUp()
        {
            CleanUp(false);
        }

        public static void CleanUp(bool includefixes)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            string originalScenePath = SceneManager.GetActiveScene().path;
            string path = originalScenePath.Substring(0, originalScenePath.LastIndexOf("/"));

            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { path });
            foreach (string guid in guids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                Scene scene = EditorSceneManager.OpenScene(scenePath);
                Debug.Log("Opening scene... " + scene.name + ": " + scenePath);

                //Update Scene
                var startPlayModePrefabExists = GetStartPlayModeInSceneView();
                var ignorePhysImmobilePrefabExists = GetIgnorePhysImmobile();
                var testPhysbonesPrefabExists = GetTestPhysbones();

                //Cleanup
                Debug.Log("Cleaning scene... ");
                var avatarDescriptors = Resources.FindObjectsOfTypeAll<VRCAvatarDescriptor>();
                foreach (var avatarDescriptor in avatarDescriptors)
                {
                    ClearAvatarBlueprintID(avatarDescriptor.gameObject);
                    //Include Fixes?
                    if (includefixes)
                    {
                        Debug.Log("Implementing fixes...");
                        FixAvatarDescriptor(avatarDescriptor);
                        SetAvatarMeshBounds(avatarDescriptor.gameObject);
                        SetAvatarAnchorProbes(avatarDescriptor.gameObject);
                    }
                }
                if (startPlayModePrefabExists) { SetStartPlayModeInSceneView(false); }
                if (ignorePhysImmobilePrefabExists) { SetIgnorePhysImmobile(false); }
                if (testPhysbonesPrefabExists) { SetTestPhysbones(false); }
                if (GameObject.Find("GestureManager") != null)
                {
                    var temp = GameObject.Find("GestureManager");
                    DestroyImmediate(temp, true);
                }

                // Save the updated scene
                Debug.Log("Saving scene... ");
                EditorSceneManager.SaveScene(scene);
            }
            EditorSceneManager.OpenScene(originalScenePath);
        }

        public static void Export()
        {
            //Setup
            List<string> paths = new List<string>();
            //Export
            var scenePath = SceneManager.GetActiveScene().path;
            paths.Add(scenePath.Substring(0, scenePath.LastIndexOf("/")));
            var name = paths[0].Substring(paths[0].IndexOf("/") + 1);
            if (GogoLocoExist())
            {
                paths.Add("Assets/GoGo");
            }
            AssetDatabase.ExportPackage(paths.ToArray(), name + ".unitypackage", ExportPackageOptions.Recurse);
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
                else
                {
                    copyPBone.name = pBone.rootTransform.name;
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
                else
                {
                    copyPCollider.name = pCollider.rootTransform.name;
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

        public static void CreateToggle(VRCAvatarDescriptor vrcAvatarDescriptor, string layerName, string paramName, AnimationClip clipA, AnimationClip clipB)
        {
            CreateToggle(vrcAvatarDescriptor, layerName, paramName, clipA, clipB, 0f);
        }

        public static void CreateToggle(VRCAvatarDescriptor vrcAvatarDescriptor, string layerName, string paramName, AnimationClip clipA, AnimationClip clipB, float paramValue)
        {
            var fx = GetFxController(vrcAvatarDescriptor);
            CreateFxParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Bool, paramValue);
            DeleteExistingFxLayer(fx, layerName);

            fx.AddLayer(layerName);

            var fxLayers = fx.layers;
            var newLayer = fxLayers[fx.layers.Length - 1];
            newLayer.defaultWeight = 1f;

            var startState = newLayer.stateMachine.AddState(clipA.name, new Vector3(250, 120));
            var endState = newLayer.stateMachine.AddState(clipB.name, new Vector3(250, 20));
            //Debug.Log("Statemachine: " + clipA.name + " " + clipA.GetInstanceID());
            //Debug.Log("Statemachine: " + clipB.name + " " + clipB.GetInstanceID());

            startState.AddTransition(endState);
            startState.transitions[0].hasFixedDuration = true;
            startState.transitions[0].duration = 0f;
            startState.transitions[0].exitTime = 0f;
            startState.transitions[0].hasExitTime = false;
            startState.transitions[0].AddCondition(AnimatorConditionMode.If, 0f, paramName);
            startState.writeDefaultValues = false;
            startState.motion = clipA;

            endState.AddTransition(startState);
            endState.transitions[0].hasFixedDuration = true;
            endState.transitions[0].duration = 0f;
            endState.transitions[0].exitTime = 0f;
            endState.transitions[0].hasExitTime = false;
            endState.transitions[0].AddCondition(AnimatorConditionMode.IfNot, 0f, paramName);
            endState.writeDefaultValues = false;
            endState.motion = clipB;

            fx.layers = fxLayers; //fixes save for default weight for some reason

            AssetDatabase.SaveAssets();
        }
        public static void CreateBlendTree(VRCAvatarDescriptor vrcAvatarDescriptor, string layerName, string paramName, AnimationClip clipA, AnimationClip clipB)
        {
            CreateBlendTree(vrcAvatarDescriptor, layerName, paramName, 0, clipA, clipB);
        }

        public static void CreateBlendTree(VRCAvatarDescriptor vrcAvatarDescriptor, string layerName, string paramName, float defaultValue, AnimationClip clipA, AnimationClip clipB)
        {
            var fx = GetFxController(vrcAvatarDescriptor);
            CreateFxParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Float, defaultValue);

            for (int i = 0; i < fx.layers.Length; i++) //delete existing layer
            {
                if (fx.layers[i].name.Equals(layerName))
                {
                    fx.RemoveLayer(i);
                    break;
                }
            }
            fx.AddLayer(layerName);
            var fxLayers = fx.layers;
            var newLayer = fxLayers[fx.layers.Length - 1];
            newLayer.defaultWeight = 1f;

            var newBlendTree = new BlendTree();
            fx.CreateBlendTreeInController(layerName, out newBlendTree, fx.layers.Length - 1); //Just WTF unity, just whyyyyyyyyyy
            newBlendTree.AddChild(clipA, 0);
            newBlendTree.AddChild(clipB, 1);
            newBlendTree.name = "Blend Tree";
            newBlendTree.blendParameter = paramName;
            newBlendTree.blendType = BlendTreeType.Simple1D;
            newBlendTree.hideFlags = HideFlags.HideAndDontSave; //Because Unity, for some reason, doesn't want the Interface and Code to have the same defaults
            newLayer.stateMachine.states[0].state.writeDefaultValues = false;
            newLayer.stateMachine.states[0].position = new Vector3(250, 120); //Doesn't work for some reason?

            fx.layers = fxLayers; //fixes save for default weight for some reason

            EditorUtility.SetDirty(fx);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        public static VRCExpressionParameters.ValueType ConvertAnimatorToVrcParamType(AnimatorControllerParameterType dataType)
        {
            VRCExpressionParameters.ValueType vrcParamType;
            switch (dataType)
            {
                case AnimatorControllerParameterType.Int:
                    vrcParamType = VRCExpressionParameters.ValueType.Int;
                    break;
                case AnimatorControllerParameterType.Float:
                    vrcParamType = VRCExpressionParameters.ValueType.Float;
                    break;
                default:
                    vrcParamType = VRCExpressionParameters.ValueType.Bool;
                    break;
            }
            return vrcParamType;
        }

        public static void CreateFxParameter(VRCAvatarDescriptor vrcAvatarDescriptor, string paramName, int dataType)
        {
            if (dataType == 1)
                CreateFxParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Int);
            else if (dataType == 2)
                CreateFxParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Float);
            else
                CreateFxParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Bool);
        }

        public static void CreateFxParameter(VRCAvatarDescriptor vrcAvatarDescriptor, string paramName, AnimatorControllerParameterType dataType)
        {
            CreateFxParameter(vrcAvatarDescriptor, paramName, dataType, 0);
        }
        public static void CreateFxParameter(VRCAvatarDescriptor vrcAvatarDescriptor, string paramName, AnimatorControllerParameterType dataType, float defaultValue)
        {
            var fx = GetFxController(vrcAvatarDescriptor);
            VRCExpressionParameters.ValueType vrcParamType = ConvertAnimatorToVrcParamType(dataType);

            for (int i = 0; i < fx.parameters.Length; i++)
            {
                if (paramName.Equals(fx.parameters[i].name))
                    fx.RemoveParameter(i); //Remove anyway just in case theres a new datatype
            }
            fx.AddParameter(paramName, dataType);

            CreateVrcParameter(vrcAvatarDescriptor.expressionParameters, paramName, vrcParamType, defaultValue, true);

            EditorUtility.SetDirty(fx);
            AssetDatabase.Refresh();
        }

        public static void CreateVrcParameter(VRCExpressionParameters vrcParameters, string paramName, VRCExpressionParameters.ValueType vrcExType)
        {
            CreateVrcParameter(vrcParameters, paramName, vrcExType, 0, true); //minimum defaults
        }

        public static void CreateVrcParameter(VRCExpressionParameters vrcParameters, string paramName, VRCExpressionParameters.ValueType vrcExType, float defaultValue, bool saved)
        {

            var vrcExParams = vrcParameters.parameters.ToList();
            for (int i = 0; i < vrcParameters.parameters.Length; i++)
            {
                if (paramName.Equals(vrcExParams[i].name))
                {
                    vrcExParams.Remove(vrcExParams[i]);
                    break;
                }
            }
            var newVrcExParam = new VRCExpressionParameters.Parameter()
            {
                name = paramName,
                valueType = vrcExType,
                defaultValue = defaultValue,
                saved = saved
            };
            //Debug.Log(newVrcExParam.name + ", default value: " + newVrcExParam.defaultValue);
            vrcExParams.Add(newVrcExParam);
            vrcParameters.parameters = vrcExParams.ToArray();

            EditorUtility.SetDirty(vrcParameters);
            AssetDatabase.Refresh();
        }

        public static void DeleteVrcParameter(VRCExpressionParameters vrcParameters, string paramName)
        {
            var parameter = vrcParameters.FindParameter(paramName);
            if (parameter != null)
            {
                var listVrcParameters = vrcParameters.parameters.ToList();
                listVrcParameters.Remove(parameter);
                vrcParameters.parameters = listVrcParameters.ToArray();
                EditorUtility.SetDirty(vrcParameters);
                AssetDatabase.Refresh();
            }

        }

        public static void SetupGogoLocoParams(VRCExpressionParameters vrcParameters)
        {
            SetupGogoBeyondParams(vrcParameters);
        }

        public static void SetupGogoBeyondParams(VRCExpressionParameters vrcParameters)
        {
            DeleteVrcParameter(vrcParameters, "Go/JumpAndFall");
            DeleteVrcParameter(vrcParameters, "Go/ScaleFloat");
            DeleteVrcParameter(vrcParameters, "Go/Horizon");
            DeleteVrcParameter(vrcParameters, "Go/ThirdPerson");
            DeleteVrcParameter(vrcParameters, "Go/ThirdPersonMirror");
            CreateVrcParameter(vrcParameters, "VRCEmote", VRCExpressionParameters.ValueType.Int, 0, true);
            CreateVrcParameter(vrcParameters, "Go/Float", VRCExpressionParameters.ValueType.Float, 0f, true);
            CreateVrcParameter(vrcParameters, "Go/PoseRadial", VRCExpressionParameters.ValueType.Float, 0f, false);
            CreateVrcParameter(vrcParameters, "Go/Stationary", VRCExpressionParameters.ValueType.Bool, 0, false);
            CreateVrcParameter(vrcParameters, "Go/Locomotion", VRCExpressionParameters.ValueType.Bool, 0, true);
            CreateVrcParameter(vrcParameters, "Go/Jump&Fall", VRCExpressionParameters.ValueType.Bool, 0, true);
            CreateVrcParameter(vrcParameters, "Go/StandIdle", VRCExpressionParameters.ValueType.Int, 0, true);
            CreateVrcParameter(vrcParameters, "Go/CrouchIdle", VRCExpressionParameters.ValueType.Int, 0, true);
            CreateVrcParameter(vrcParameters, "Go/ProneIdle", VRCExpressionParameters.ValueType.Int, 0, true);
            CreateVrcParameter(vrcParameters, "Go/Swimming", VRCExpressionParameters.ValueType.Bool, 0, false);
            CreateVrcParameter(vrcParameters, "Go/PuppetX", VRCExpressionParameters.ValueType.Float, 0f, false);
            CreateVrcParameter(vrcParameters, "Go/PuppetY", VRCExpressionParameters.ValueType.Float, 0f, false);
            CreateVrcParameter(vrcParameters, "Go/Height", VRCExpressionParameters.ValueType.Float, 0f, true);
        }

        public static void SetupGogoBeyondFX(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            var fx = GetFxController(vrcAvatarDescriptor);
            AnimatorController gogoController = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/GoGo/GoLoco/Controllers/GoLocoFXBeyond.controller");
            
            CopyControllerParams(gogoController, fx);
            VRLabs.AV3Manager.AnimatorCloner.CopyControllerLayer(gogoController, 1, fx);
            VRLabs.AV3Manager.AnimatorCloner.CopyControllerLayer(gogoController, 2, fx);

        }

        public static void CopyControllerParams(AnimatorController source, AnimatorController destination)
        {
            foreach (var sParam in source.parameters)
            {
                if (!destination.parameters.Contains(sParam))
                {
                    destination.AddParameter(sParam);
                }
            }
        }

        public static void DeleteExistingFxLayer(AnimatorController fx, string layerName)
        {
            for (int i = 0; i < fx.layers.Length; i++)
            {
                if (fx.layers[i].name.Equals(layerName))
                {
                    fx.RemoveLayer(i);
                    break;
                }
            }
        }

        public static void DeleteControllerParam(AnimatorController ac, string paramName)
        {
            for (int i = 0; i < ac.parameters.Length; i++)
            {
                if (paramName.Equals(ac.parameters[i].name))
                {
                    ac.RemoveParameter(i); //Delete Param
                    EditorUtility.SetDirty(ac);
                    AssetDatabase.Refresh();
                }
            }
            
        }

        public static AnimationClip SaveAnimation(AnimationClip anim, string savePath)
        {
            if (anim != null && anim.name != "")
            {
                if (!(AssetDatabase.IsValidFolder(savePath)))
                    Directory.CreateDirectory(savePath);
                savePath = savePath + "/" + anim.name + ".anim";
                CreateOrReplaceAsset<AnimationClip>(anim, savePath);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath);
                return clip;
            }
            return null;
        }

        public static void CreateOrReplaceAsset<T>(T asset, string path) where T : UnityEngine.Object
        {
            T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

            //Debug.Log("Existing asset... " + existingAsset.name + " = " + existingAsset.GetInstanceID());
            if (existingAsset == null)
            {
                AssetDatabase.CreateAsset(asset, path);
            }
            else
            {
                EditorUtility.CopySerialized(asset, existingAsset);
                existingAsset = asset;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static AnimationClip[] GenerateAnimationToggle(SkinnedMeshRenderer mRender, GameObject vrcAvatar)
        {
            AnimationClip[] clips = new AnimationClip[2]; //0 off, 1 on
            AnimationClip aClipOff = new AnimationClip();
            AnimationClip aClipOn = new AnimationClip();
            aClipOff.name = mRender.name + " OFF";
            aClipOn.name = mRender.name + " ON";
            var path = AnimationUtility.CalculateTransformPath(mRender.transform, vrcAvatar.transform);
            aClipOff.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 0)));
            aClipOn.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 1)));
            clips[0] = SaveAnimation(aClipOff, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");
            clips[1] = SaveAnimation(aClipOn, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");

            return clips;
        }

        public static AnimationClip[] GenerateAnimationToggle(GameObject go, GameObject vrcAvatar)
        {
            AnimationClip[] clips = new AnimationClip[2]; //0 off, 1 on
            AnimationClip aClipOff = new AnimationClip();
            AnimationClip aClipOn = new AnimationClip();
            aClipOff.name = go.name + " OFF";
            aClipOn.name = go.name + " ON";
            var path = AnimationUtility.CalculateTransformPath(go.transform, vrcAvatar.transform);
            aClipOff.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 0)));
            aClipOn.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 1)));
            clips[0] = SaveAnimation(aClipOff, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");
            clips[1] = SaveAnimation(aClipOn, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");

            return clips;
        }

        public static void GenerateAnimationRenderToggles(GameObject vrcAvatar)
        {
            AnimationClip allOff = new AnimationClip();
            AnimationClip allOn = new AnimationClip();
            allOff.name = "all OFF";
            allOn.name = "all ON";
            foreach (var r in vrcAvatar.GetComponentsInChildren<Renderer>(true))
            {
                //Debug.Log(r.name);
                AnimationClip aClipOff = new AnimationClip();
                AnimationClip aClipOn = new AnimationClip();
                aClipOff.name = r.name + " OFF";
                aClipOn.name = r.name + " ON";
                var path = AnimationUtility.CalculateTransformPath(r.transform, vrcAvatar.transform);
                aClipOff.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 0)));
                AddToggleShapekeys(vrcAvatar, r, aClipOff, false);
                aClipOn.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 1)));
                AddToggleShapekeys(vrcAvatar, r, aClipOn, true);
                allOff.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 0)));
                allOn.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 1)));

                SaveAnimation(aClipOn, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");
                SaveAnimation(aClipOff, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");
            }
            SaveAnimation(allOff, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");
            SaveAnimation(allOn, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");

        }

        public static void AddToggleShapekeys(GameObject vrcAvatar, Renderer r, AnimationClip clip, bool toggle)
        {

            foreach (var smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                //Debug.Log(smr.name);
                for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                {
                    var path = AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform);
                    var splitString = smr.sharedMesh.GetBlendShapeName(i).Split('_');
                    string prefix = string.Join("_", splitString.Take(splitString.Length - 1));
                    string suffix = splitString[splitString.Length - 1].ToLower();
                    //Debug.Log("Prefix: " + prefix);
                    //Debug.Log("Suffix: " + suffix);
                    if (prefix == r.name && suffix == "on")
                    {
                        if (toggle)
                            clip.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(i), new AnimationCurve(new Keyframe(0, 100)));
                        else
                            clip.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(i), new AnimationCurve(new Keyframe(0, 0)));
                    }
                }
            }
        }

        public static AnimationClip[] GenerateShapekeyToggle(SkinnedMeshRenderer smr, string shapeKeyName, GameObject vrcAvatar)
        {
            AnimationClip[] clips = new AnimationClip[2]; //0 off, 1 on
            Debug.Log(smr.name);
            AnimationClip aClipMin = new AnimationClip();
            AnimationClip aClipMax = new AnimationClip();
            var path = AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform);
            aClipMin.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + shapeKeyName, new AnimationCurve(new Keyframe(0, 0)));
            aClipMin.name = smr.name + "_" + shapeKeyName + " MIN";
            aClipMax.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + shapeKeyName, new AnimationCurve(new Keyframe(0, 100)));
            aClipMax.name = smr.name + "_" + shapeKeyName + " MAX";

            clips[0] = SaveAnimation(aClipMin, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");
            clips[1] = SaveAnimation(aClipMax, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");

            return clips;

        }

        public static void GenerateAnimationShapekeys(GameObject vrcAvatar)
        {
            foreach (var smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                Debug.Log(smr.name);
                for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                {
                    //Debug.Log(smr.sharedMesh.GetBlendShapeName(i));
                    AnimationClip aClipMin = new AnimationClip();
                    AnimationClip aClipMax = new AnimationClip();
                    var path = AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform);
                    aClipMin.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(i), new AnimationCurve(new Keyframe(0, 0)));
                    aClipMin.name = smr.name + "_" + smr.sharedMesh.GetBlendShapeName(i) + " MIN";
                    aClipMax.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(i), new AnimationCurve(new Keyframe(0, 100)));
                    aClipMax.name = smr.name + "_" + smr.sharedMesh.GetBlendShapeName(i) + " MAX";

                    SaveAnimation(aClipMin, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");
                    SaveAnimation(aClipMax, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");
                }
            }
        }

        public static void GenerateAnimationPhysbones(GameObject vrcAvatar, VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            //AnimationClip aClipOn = new AnimationClip();
            //AnimationClip aClipOff = new AnimationClip();
            AnimationClip aClipCollisonOn = new AnimationClip();
            AnimationClip aClipCollisonOff = new AnimationClip();
            AnimationClip aClipCollisionOthers = new AnimationClip();
            
            foreach (var pbone in vrcAvatar.GetComponentsInChildren<VRC_PhysBone>(true))
            {

                var path = AnimationUtility.CalculateTransformPath(pbone.transform, vrcAvatar.transform);
                aClipCollisonOff.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.1f, 1)));
                aClipCollisonOn.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.1f, 1)));
                aClipCollisionOthers.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.1f, 1)));

                aClipCollisonOff.SetCurve(path, typeof(VRC_PhysBone), "allowCollision", new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.1f, 0)));
                aClipCollisonOn.SetCurve(path, typeof(VRC_PhysBone), "allowCollision", new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.1f, 1)));
                aClipCollisionOthers.SetCurve(path, typeof(VRC_PhysBone), "allowCollision", new AnimationCurve(new Keyframe(0, 2), new Keyframe(0.1f, 2)));

                aClipCollisonOn.name = "Physbone Collision ON";
                aClipCollisonOff.name = "Physbone Collision OFF";
                aClipCollisionOthers.name = "Physbone Collision Self OFF";

            }
            //SaveAnimation(aClipOn, GetCurrentSceneRootPath() + "/Animations/Generated/Physbones");
            //SaveAnimation(aClipOff, GetCurrentSceneRootPath() + "/Animations/Generated/Physbones");
            var clip1 = SaveAnimation(aClipCollisonOn, GetCurrentSceneRootPath() + "/Animations/Generated/Physbones");
            SaveAnimation(aClipCollisonOff, GetCurrentSceneRootPath() + "/Animations/Generated/Physbones");
            var clip2 = SaveAnimation(aClipCollisionOthers, GetCurrentSceneRootPath() + "/Animations/Generated/Physbones");
            string paramName = "PhysCollision";
            CreateToggle(vrcAvatarDescriptor, "PhysCollision Settings", paramName, clip2, clip1);
            //CreateFxParameter(vrcAvatarDescriptor, "PhysCollision", AnimatorControllerParameterType.Bool);
            var menu = CreateNewMenu("Menu_Physbones");
            CreateMenuControl(menu, "Enable Self Colliders" , VRCExpressionsMenu.Control.ControlType.Toggle, paramName);

        }

        public static void CombineAnimationShapekeys(GameObject vrcAvatar)
        {
            List<string> blendPaths = new List<string>();
            AnimationClip allMin = new AnimationClip();
            AnimationClip allMax = new AnimationClip();
            allMin.name = "all MIN";
            allMax.name = "all MAX";
            foreach (var smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                //Debug.Log(smr.name);
                for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                {
                    blendPaths.Add(AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform)); //First blend path
                    foreach (var smr2 in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true)) //check other skinned mesh renders
                    {
                        if (smr.name != smr2.name)
                        {
                            for (int j = 0; j < smr2.sharedMesh.blendShapeCount; j++) //check those render's shapekeys
                            {
                                if (smr.sharedMesh.GetBlendShapeName(i) == smr2.sharedMesh.GetBlendShapeName(j)) //Matching shapes found
                                {
                                    blendPaths.Add(AnimationUtility.CalculateTransformPath(smr2.transform, vrcAvatar.transform));
                                }
                            }
                        }
                    }
                    AnimationClip aClipMin = new AnimationClip();
                    AnimationClip aClipMax = new AnimationClip();
                    aClipMin.name = smr.sharedMesh.GetBlendShapeName(i) + " MIN";
                    aClipMax.name = smr.sharedMesh.GetBlendShapeName(i) + " MAX";
                    foreach (var path in blendPaths)
                    {
                        //var path = AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform);
                        aClipMin.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(i), new AnimationCurve(new Keyframe(0, 0)));
                        allMin.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(i), new AnimationCurve(new Keyframe(0, 0)));
                        aClipMax.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(i), new AnimationCurve(new Keyframe(0, 100)));
                        allMax.SetCurve(path, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(i), new AnimationCurve(new Keyframe(0, 100)));
                    }
                    SaveAnimation(aClipMin, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");
                    SaveAnimation(aClipMax, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");
                    blendPaths.Clear();
                }
            }
            SaveAnimation(allMin, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");
            SaveAnimation(allMax, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys");
        }

        public static void CombineEmoteShapekeys(GameObject vrcAvatar)
        {

            List<int> blendIndex = new List<int>();
            foreach (var smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                string blendPath = AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform);
                for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                {
                    if (smr.sharedMesh.GetBlendShapeName(i).Contains("Emote"))
                    {
                        blendIndex.Add(i);
                    }
                }
                if (blendIndex.Count > 0)
                {
                    //Debug.Log("SMR NAME: " + smr.name);
                    //Debug.Log("INDEX COUNT: " + blendIndex.Count);
                    AnimationClip emoteIdle = new AnimationClip();
                    emoteIdle.name = "Emote_Idle";
                    for (int i = 0; i < blendIndex.Count; i++)
                    {
                        AnimationClip emoteClip = new AnimationClip();
                        emoteClip.name = smr.sharedMesh.GetBlendShapeName(blendIndex[i]);
                        emoteClip.SetCurve(blendPath, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(blendIndex[i]), new AnimationCurve(new Keyframe(0, 100)));
                        emoteIdle.SetCurve(blendPath, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(blendIndex[i]), new AnimationCurve(new Keyframe(0, 0)));
                        for (int j = 0; j < blendIndex.Count; j++)
                        {
                            if (blendIndex[i] != blendIndex[j])
                            {
                                emoteClip.SetCurve(blendPath, typeof(SkinnedMeshRenderer), "blendShape." + smr.sharedMesh.GetBlendShapeName(blendIndex[j]), new AnimationCurve(new Keyframe(0, 0)));
                            }

                        }
                        SaveAnimation(emoteClip, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys/Emotes");
                    }
                    SaveAnimation(emoteIdle, GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys/Emotes");
                    blendIndex.Clear();
                }
            }
        }

        public static void GenerateEmoteOverrideMenu(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            if (Directory.Exists(GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys/Emotes"))
            {
                var dir = new DirectoryInfo(GetCurrentSceneRootPath() + "/Animations/Generated/ShapeKeys/Emotes");
                var emoteFiles = dir.GetFiles("*.anim");
                string paramName = "EmoteOverride";
                string layerName = "Emote Override Control";
                CreateFxParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Int);
                var menu = CreateNewMenu("Menu_EmoteOverride");
                var fx = GetFxController(vrcAvatarDescriptor);
                DeleteExistingFxLayer(fx, layerName);
                fx.AddLayer(layerName);
                var fxLayers = fx.layers;
                var newLayer = fxLayers[fx.layers.Length - 1];
                newLayer.defaultWeight = 1f;
                var emptyState = newLayer.stateMachine.AddState("Empty", new Vector3(250, 220));
                emptyState.writeDefaultValues = true; //Reset defaults as we don't want to override anymore
                EditorUtility.SetDirty(emptyState);
                int emoteCount = emoteFiles.Length;
                if (emoteCount > 8) { emoteCount = 8; }
                for (int i = 0; i < emoteCount; i++)
                {
                    var emoteAsset = "Assets" + emoteFiles[i].FullName.Substring(Application.dataPath.Length);
                    var emote = AssetDatabase.LoadAssetAtPath(emoteAsset, typeof(AnimationClip)) as AnimationClip;
                    //var emote = Resources.Load<AnimationClip>(emoteFiles[i].FullName);

                    var emoteState = newLayer.stateMachine.AddState(emote.name, new Vector3(650, 20 + (i * 50)));
                    emoteState.writeDefaultValues = false;
                    emoteState.motion = emote;
                    EditorUtility.SetDirty(emoteState);

                    emptyState.AddTransition(emoteState);
                    emptyState.transitions[i].hasFixedDuration = true;
                    emptyState.transitions[i].duration = 0f;
                    emptyState.transitions[i].exitTime = 0f;
                    emptyState.transitions[i].hasExitTime = false;
                    emptyState.transitions[i].AddCondition(AnimatorConditionMode.Equals, i + 1, paramName);

                    emoteState.AddTransition(emptyState);
                    emoteState.transitions[0].hasFixedDuration = true;
                    emoteState.transitions[0].duration = 0f;
                    emoteState.transitions[0].exitTime = 0f;
                    emoteState.transitions[0].hasExitTime = false;
                    emoteState.transitions[0].AddCondition(AnimatorConditionMode.NotEqual, i + 1, paramName);

                    CreateMenuControl(menu, emote.name, VRCExpressionsMenu.Control.ControlType.Toggle, paramName, i + 1);
                }
                fx.layers = fxLayers; //fixes save for default weight for some reason
                //EditorUtility.SetDirty(fx);
                //AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }

        public static void CreateMenuControl(VRCExpressionsMenu vrcMenu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName)
        {
            CreateMenuControl(vrcMenu, controlName, controlType, paramName, null, null, 1);
        }
        public static void CreateMenuControl(VRCExpressionsMenu vrcMenu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName, int value)
        {
            CreateMenuControl(vrcMenu, controlName, controlType, paramName, null, null, value);
        }
        public static void CreateMenuControl(VRCExpressionsMenu vrcMenu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName, Texture2D icon)
        {
            CreateMenuControl(vrcMenu, controlName, controlType, paramName, null, icon, 1);
        }
        public static void CreateMenuControl(VRCExpressionsMenu vrcMenu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName, VRCExpressionsMenu subMenu, Texture2D icon)
        {
            CreateMenuControl(vrcMenu, controlName, controlType, paramName, subMenu, icon, 1);
        }

        public static void CreateMenuControl(VRCExpressionsMenu menu, string controlName, int controlType, string paramName)
        {
            switch (controlType)
            {
                case 1:
                    CreateMenuControl(menu, controlName, VRCExpressionsMenu.Control.ControlType.Button, paramName);
                    break;
                case 2:
                    CreateMenuControl(menu, controlName, VRCExpressionsMenu.Control.ControlType.TwoAxisPuppet, paramName);
                    break;
                case 3:
                    CreateMenuControl(menu, controlName, VRCExpressionsMenu.Control.ControlType.FourAxisPuppet, paramName);
                    break;
                case 4:
                    CreateMenuControl(menu, controlName, VRCExpressionsMenu.Control.ControlType.RadialPuppet, paramName);
                    break;
                default:
                    CreateMenuControl(menu, controlName, VRCExpressionsMenu.Control.ControlType.Toggle, paramName);
                    break;
            }
        }
        public static void CreateMenuControl(VRCAvatarDescriptor vrcAvatarDescriptor, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName)
        {
            //var param = vrcAvatarDescriptor.expressionParameters;
            var vrcMenu = vrcAvatarDescriptor.expressionsMenu;
            CreateMenuControl(vrcMenu, controlName, controlType, paramName);
        }

        public static void CreateMenuControl(VRCExpressionsMenu vrcMenu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, VRCExpressionsMenu subMenu)
        {
            CreateMenuControl(vrcMenu, controlName, controlType, "", subMenu, (Texture2D)AssetDatabase.LoadAssetAtPath(("Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Expressions Menu/Icons/item_folder.png"), typeof(Texture2D)), 1);
        }

        public static void CreateMenuControl(VRCExpressionsMenu vrcMenu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName, VRCExpressionsMenu subMenu, Texture2D icon, int value)
        {
            foreach (var control in vrcMenu.controls)
            {
                if (control.name.Equals(controlName))
                {
                    vrcMenu.controls.Remove(control);
                    break;
                }
            }
            if (vrcMenu.controls.Count == 8)
            {
                EditorUtility.DisplayDialog("Menu control full! (8 Max)", "Free up menu and/or make a new one as a submenu", "Ok");
                return;
            }
            var item = new VRCExpressionsMenu.Control
            {
                name = controlName,
                type = controlType,
                value = value
            };
            if (controlType == VRCExpressionsMenu.Control.ControlType.RadialPuppet)
            {
                item.subParameters = new VRCExpressionsMenu.Control.Parameter[]
                { new  VRCExpressionsMenu.Control.Parameter {
                    name = paramName
                }};
            }
            else if (controlType == VRCExpressionsMenu.Control.ControlType.SubMenu)
            {
                item.subMenu = subMenu;
            }
            else
            {
                item.parameter = new VRCExpressionsMenu.Control.Parameter
                {
                    name = paramName
                };
            }
            if (icon != null)
            {
                item.icon = icon;
            }

            vrcMenu.controls.Add(item);
            EditorUtility.SetDirty(vrcMenu);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        public static VRCExpressionsMenu CreateNewMenu(string menuName)
        {
            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            string saveFolder = GetCurrentSceneRootPath() + "/" + "Menus";
            if (!(AssetDatabase.IsValidFolder(saveFolder)))
                Directory.CreateDirectory(saveFolder);
            string savePath = GetCurrentSceneRootPath() + "/" + "Menus" + "/" + menuName + ".asset";
            menu.name = menuName;
            CreateOrReplaceAsset<VRCExpressionsMenu>(menu, savePath);
            menu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(savePath);
            return menu;
        }

        public static void SetupGogoLocoMenu(VRCExpressionsMenu vrcMenu)
        {
            var subMenu = (VRCExpressionsMenu)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/MainMenu/Menu/GoAllMenu.asset", typeof(VRCExpressionsMenu));
            var icon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Icons/icon_Go_Loco.png", typeof(Texture2D));
            CreateMenuControl(vrcMenu, "GoGo Loco Menu", VRCExpressionsMenu.Control.ControlType.SubMenu, "", subMenu, icon);
        }

        public static void SetupGogoBeyondMenu(VRCExpressionsMenu vrcMenu)
        {
            var subMenu = (VRCExpressionsMenu)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/MainMenu/Menu/GoBeyondMenu.asset", typeof(VRCExpressionsMenu));
            var icon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Icons/icon_Go_Loco.png", typeof(Texture2D));
            CreateMenuControl(vrcMenu, "GoGo Loco Menu", VRCExpressionsMenu.Control.ControlType.SubMenu, "", subMenu, icon);
        }

        public static AnimationCurve LinearAnimationCurve()
        {
            AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.5f));
            curve.preWrapMode = WrapMode.Default;
            curve.postWrapMode = WrapMode.Default;

            return curve;
        }

        public static void AddPhysBones(Transform bone)
        {
            if (!ShadstersAvatarTools.BoneHasPhysBones(bone))
            {
                VRC_PhysBone pBone = bone.gameObject.AddComponent<VRC_PhysBone>();
                pBone.rootTransform = bone;
                pBone.integrationType = VRC_PhysBone.IntegrationType.Advanced;
                pBone.pull = 0.2f;
                //pBone.pullCurve = LinearAnimationCurve();
                pBone.spring = 0.8f;
                pBone.stiffness = 0.2f;
                pBone.immobile = 0.3f;

                pBone.limitType = VRC_PhysBone.LimitType.Angle;
                pBone.maxAngleX = 45;
            }
        }

        public static void AddButtPhysBones(Transform bone)
        {
            if (!ShadstersAvatarTools.BoneHasPhysBones(bone))
            {
                VRC_PhysBone pBone = bone.gameObject.AddComponent<VRC_PhysBone>();
                pBone.rootTransform = bone;
                pBone.integrationType = VRC_PhysBone.IntegrationType.Advanced;
                pBone.pull = 0.2f;
                //pBone.pullCurve = LinearAnimationCurve();
                pBone.spring = 0.8f;
                pBone.stiffness = 0.2f;
                pBone.immobile = 0.3f;

                pBone.limitType = VRC_PhysBone.LimitType.Angle;
                pBone.maxAngleX = 45;
            }
        }

        public static void UseExperimentalPlayMode(bool value)
        {
            const string EditorSettingsAssetPath = "ProjectSettings/EditorSettings.asset";
            SerializedObject editorSettings = new SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath(EditorSettingsAssetPath)[0]);
            SerializedProperty m_playMode = editorSettings.FindProperty("m_EnterPlayModeOptionsEnabled");
            SerializedProperty m_playModeOptions = editorSettings.FindProperty("m_EnterPlayModeOptions");
            if (value)
            {
                m_playMode.boolValue = true;
                m_playModeOptions.intValue = 1; //0 = all checked, 1 = scene only, 2 = domain only, 3 = none?
            }
            else
            {
                m_playMode.boolValue = false;
                m_playModeOptions.intValue = 3;
            }
            editorSettings.ApplyModifiedProperties();
        }
        public static List<SkinnedMeshRenderer> GetAvatarSkinnedMeshRenderers(GameObject root, Bounds bounds)
        {
            List<SkinnedMeshRenderer> smrList = null;
            Debug.Log(root.GetComponentsInChildren<Renderer>()[1]);
            foreach (SkinnedMeshRenderer smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                //Debug.Log(smr);
                //Debug.Log(smr.bounds);
                //smrList.Add(smr);
            }

            return smrList;
        }

        public static Bounds CalculateLocalBounds(GameObject root)
        {
            //Vector3 extentTotal;
            //extentTotal.x = 0f;
            //extentTotal.y = 0f;
            //extentTotal.z = 0f;

            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }
            //bounds.extents = extentTotal;
            return bounds;
        }

        public static void EncapsulateAvatarBounds(GameObject vrcAvatar)
        {
            Undo.RecordObject(vrcAvatar, "Combine Mesh Bounds");
            Bounds bounds = CalculateLocalBounds(vrcAvatar);
            foreach (SkinnedMeshRenderer smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                smr.localBounds = bounds;
            }

        }

        public static void ResetAvatarBounds(GameObject vrcAvatar)
        {
            foreach (SkinnedMeshRenderer smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                smr.sharedMesh.RecalculateBounds();
            }

        }

        public static void SetAvatarMeshBounds(GameObject vrcAvatar)
        {
            Vector3 vectorSize;
            vectorSize.x = 2.5f;
            vectorSize.y = 2.5f;
            vectorSize.z = 2.5f;
            Bounds bounds = new Bounds(Vector3.zero, vectorSize);
            foreach (SkinnedMeshRenderer smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                Undo.RecordObject(smr, "Set Avatar Bounds");
                smr.localBounds = bounds;
            }
        }
        public static void SetAvatarAnchorProbes(GameObject vrcAvatar)
        {

            foreach (Renderer r in vrcAvatar.GetComponentsInChildren<Renderer>(true))
            {
                Undo.RecordObject(r, "Set Avatar Anchor Probe");
                r.probeAnchor = vrcAvatar.transform.Find("Armature").Find("Hips");
            }
        }

        public static bool GogoLocoExist()
        {
            if (!(string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID("Assets/GoGo/GoLoco/Controllers/GoLocoBase.controller"))))
            {
                return true;
            }
            return false;
        }

        public static void SetupGogoLocoLayers(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            vrcAvatarDescriptor.customizeAnimationLayers = true; //ensure customizing playable layers is true
            vrcAvatarDescriptor.autoLocomotion = false; //disable force 6-point tracking

            vrcAvatarDescriptor.baseAnimationLayers[0].isDefault = false; //Base
            vrcAvatarDescriptor.baseAnimationLayers[3].isDefault = false; //Action
            vrcAvatarDescriptor.specialAnimationLayers[0].isDefault = false; //Sitting

            vrcAvatarDescriptor.baseAnimationLayers[0].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoBase.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.baseAnimationLayers[3].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoAction.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.specialAnimationLayers[0].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoSitting.controller", typeof(RuntimeAnimatorController));
            //Debug.Log(AssetDatabase.GetAssetPath(vrcAvatarDescriptor.specialAnimationLayers[0].animatorController));
        }

        public static void SetupGogoBeyondLayers(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            vrcAvatarDescriptor.customizeAnimationLayers = true; //ensure customizing playable layers is true
            vrcAvatarDescriptor.autoLocomotion = false; //disable force 6-point tracking

            vrcAvatarDescriptor.baseAnimationLayers[0].isDefault = false; //Base
            vrcAvatarDescriptor.baseAnimationLayers[1].isDefault = false; //Additive
            vrcAvatarDescriptor.baseAnimationLayers[2].isDefault = false; //Gesture
            vrcAvatarDescriptor.baseAnimationLayers[3].isDefault = false; //Action
            vrcAvatarDescriptor.specialAnimationLayers[0].isDefault = false; //Sitting
            vrcAvatarDescriptor.specialAnimationLayers[1].isDefault = false; //TPose

            vrcAvatarDescriptor.baseAnimationLayers[0].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoBase.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.baseAnimationLayers[1].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoAdditive.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.baseAnimationLayers[2].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoGesture.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.baseAnimationLayers[3].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoAction.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.specialAnimationLayers[0].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoSitting.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.specialAnimationLayers[1].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoTPose.controller", typeof(RuntimeAnimatorController));
        }

        public static void SetupGogoBeyondPrefab(GameObject vrcAvatar)
        {
            var existingPrefab = vrcAvatar.transform.Find("Beyond Prefab");
            if (existingPrefab != null)
            {
                DestroyImmediate(existingPrefab.gameObject);
            }

            string prefabPath = "Assets/GoGo/GoLoco/Prefabs/Beyond Prefab.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                GameObject ob = GameObject.Instantiate(prefab, vrcAvatar.transform);
                ob.name = "Beyond Prefab";
                Transform flyRotation = ob.transform.Find("Fly/Rotation");
                if (flyRotation != null)
                {
                    RotationConstraint constraint = flyRotation.GetComponent<RotationConstraint>();
                    if (constraint != null)
                    {
                        Transform head = vrcAvatar.transform.Find("Armature/Hips/Spine/Chest/Neck/Head");
                        if (head != null)
                        {
                            ConstraintSource source = new ConstraintSource();
                            source.sourceTransform = head;
                            source.weight = 1.0f;

                            constraint.SetSource(0, source);
                        } else { Debug.LogError("Head not found in the hierarchy."); }
                    } else { Debug.LogError("RotationConstraint component not found on 'Fly/Rotation' GameObject."); }
                } else { Debug.LogError("'Fly/Rotation' GameObject not found."); }
            } else { Debug.LogError("Prefab not found at path: " + prefabPath); }
        }

        public static float GetAvatarHeight(GameObject vrcAvatar)
        {
            Animator anim = vrcAvatar.GetComponent<Animator>();
            Transform shoulderL = anim.GetBoneTransform(HumanBodyBones.LeftShoulder);
            float height = shoulderL.position.y - vrcAvatar.transform.position.y;
            Debug.Log(height);
            return height;
        }

        public static Transform GetAvatarBone(GameObject vrcAvatar, string search, string direction)
        {
            Transform armature = vrcAvatar.transform.Find("Armature");
            Transform result = null;
            if (armature != null)
            {
                foreach (Transform bone in armature.GetComponentsInChildren<Transform>(true))
                {
                    if (bone.name.Contains(search))
                    {
                        if (result == null && bone.name.Contains(direction))
                        {
                            result = bone;
                        }
                    }


                }
            }
            //Debug.Log(result);
            return result;
        }

        public static void DeleteEndBones(GameObject vrcAvatar)
        {
            Transform armature = vrcAvatar.transform.Find("Armature");
            if (armature != null)
            {
                foreach (Transform bone in armature.GetComponentsInChildren<Transform>(true))
                {
                    if (bone.name.EndsWith("_end"))
                    {
                        Undo.RecordObject(bone, "Delete End Bone");
                        DestroyImmediate(bone.gameObject);
                    }
                }
            }
        }


        public static void SaveAvatarPrefab(GameObject vrcAvatar)
        {
            string prefabPath = GetCurrentSceneRootPath() + "/Prefabs";
            if (!(AssetDatabase.IsValidFolder(prefabPath))) //If folder doesn't exist "Assets\AvatarName\Prefabs"
            {
                Directory.CreateDirectory(prefabPath);
            }
            string savePath = prefabPath + "/" + vrcAvatar.name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(vrcAvatar, savePath);
        }

        public static List<UnityEngine.Object> GetAvatarTextures(GameObject vrcAvatar)
        {
            List<UnityEngine.Object> aTextures = new List<UnityEngine.Object>();
            List<string> extensions = new List<string>(new string[] { ".bmp", ".exr", ".gif", ".hdr", ".iff", ".jpg", ".pict", ".png", ".psd", ".tga", ".tiff" });
            foreach (Renderer r in vrcAvatar.GetComponentsInChildren<Renderer>(true))
            {
                foreach (Material m in r.sharedMaterials)
                {
                    if (!m)
                        continue;
                    int[] texIDs = m.GetTexturePropertyNameIDs();
                    if (texIDs == null)
                        continue;
                    foreach (int i in texIDs)
                    {
                        Texture t = m.GetTexture(i);
                        if (!t)
                            continue;
                        string path = AssetDatabase.GetAssetPath(t);
                        if (!string.IsNullOrEmpty(path))
                        {
                            if (extensions.Any(s => path.Contains(s))) //check if actual texture file
                            {
                                //Debug.Log(path);
                                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                                aTextures.Add(importer);
                            }
                        }

                    }
                }
            }
            aTextures = aTextures.Distinct().ToList(); //Clear duplicates
            return aTextures;
        }

        public static void UpdateAvatarTextureMipMaps(GameObject vrcAvatar, bool mipMapStatus)
        {
            List<string> paths = new List<string>();
            List<UnityEngine.Object> aTextures = GetAvatarTextures(vrcAvatar);
            if (aTextures.Count > 0)
            {
                foreach (UnityEngine.Object o in aTextures)
                {
                    TextureImporter t = (TextureImporter)o;
                    if (t.mipmapEnabled != mipMapStatus)
                    {
                        Undo.RecordObject(t, mipMapStatus ? "Generate Mip Maps" : "Un-Generate Mip Maps");
                        t.mipmapEnabled = mipMapStatus;
                        t.streamingMipmaps = mipMapStatus;
                        EditorUtility.SetDirty(t);
                        paths.Add(t.assetPath);
                    }
                }
            }
            if (paths.Count > 0)
            {
                AssetDatabase.ForceReserializeAssets(paths);
                AssetDatabase.Refresh();
            }
        }

        public static void SetAvatarTexturesMaxSize(GameObject vrcAvatar, int maxSize)
        {
            List<string> paths = new List<string>();
            List<UnityEngine.Object> aTextures = GetAvatarTextures(vrcAvatar);
            if (aTextures.Count > 0)
            {
                //Debug.Log(aTextures.Count);
                foreach (UnityEngine.Object o in aTextures)
                {
                    //Debug.Log(o);
                    TextureImporter t = (TextureImporter)o;
                    if (t.maxTextureSize != maxSize)
                    {
                        Undo.RecordObject(t, "Set Textures size to " + maxSize.ToString());
                        t.maxTextureSize = maxSize;
                        EditorUtility.SetDirty(t);
                        paths.Add(t.assetPath);
                    }
                }
            }
            if (paths.Count > 0)
            {
                AssetDatabase.ForceReserializeAssets(paths);
                AssetDatabase.Refresh();
            }
        }

        public static void SetAvatarTexturesCompression(GameObject vrcAvatar, TextureImporterCompression compressionType)
        {
            List<string> paths = new List<string>();
            List<UnityEngine.Object> aTextures = GetAvatarTextures(vrcAvatar);
            if (aTextures.Count > 0)
            {
                //Debug.Log(aTextures.Count);
                foreach (UnityEngine.Object o in aTextures)
                {
                    //Debug.Log(o);
                    TextureImporter t = (TextureImporter)o;
                    if (t.textureCompression != compressionType)
                    {
                        Undo.RecordObject(t, "Set Avatar compression");
                        t.textureCompression = compressionType;
                        EditorUtility.SetDirty(t);
                        paths.Add(t.assetPath);
                    }
                }
            }
            if (paths.Count > 0)
            {
                AssetDatabase.ForceReserializeAssets(paths);
                AssetDatabase.Refresh();
            }
        }

        public static void SetAllGrabMovement(GameObject vrcAvatar)
        {
            List<VRC_PhysBone> pBones = GetAllAvatarPhysBones(vrcAvatar);
            if (pBones.Count > 0)
            {
                foreach (var pBone in pBones)
                {
                    Undo.RecordObject(pBone, "Set Avatar PhysBone Grab Movement");
                    pBone.grabMovement = 1;
                }
            }
        }

        public static void UncheckAllWriteDefaults(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            var baseAnimations = vrcAvatarDescriptor.baseAnimationLayers;
            var specialAnimations = vrcAvatarDescriptor.specialAnimationLayers;
            var aControllers = baseAnimations.Concat(specialAnimations);
            foreach (var aController in aControllers)
            {
                if (aController.isDefault == false)
                {
                    //Debug.Log(aController.animatorController);
                    var controller = aController.animatorController as UnityEditor.Animations.AnimatorController;
                    foreach (var cLayer in controller.layers)
                    {
                        //Debug.Log(cLayer.stateMachine);
                        var cStates = cLayer.stateMachine.states;
                        //Debug.Log(cLayer.stateMachine.stateMachines);
                        foreach (var cState in cStates)
                        {
                            //Debug.Log(cState.state);
                            if (cState.state.writeDefaultValues)
                            {
                                cState.state.writeDefaultValues = false;
                                Debug.Log("Unchecked Write Defaults for: " + cState.state);
                            }
                        }
                    }
                }
            }
        }

        public static void RepairMissingPhysboneTransforms(GameObject vrcAvatar)
        {
            Transform pTransform = vrcAvatar.transform.Find("PhysBones");
            if (pTransform != null)
            {
                foreach (VRCPhysBone pBone in pTransform.GetComponentsInChildren<VRCPhysBone>(true))
                {
                    RepairMissingPhysboneTransform(vrcAvatar, pBone);
                }
            }
        }

        public static void RepairMissingPhysboneTransform(GameObject vrcAvatar, VRCPhysBone pBone)
        {
            Transform armature = vrcAvatar.transform.Find("Armature");
            if (armature != null)
            {
                foreach (Transform bone in armature.GetComponentsInChildren<Transform>(true))
                {
                    if (pBone.rootTransform == null && pBone.name == bone.name)
                    {
                        pBone.rootTransform = bone;
                    }
                }
            }
        }

        public static void FixAvatarDescriptor(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            Transform armature = vrcAvatarDescriptor.transform.Find("Armature");
            Transform face = vrcAvatarDescriptor.transform.Find("Face");
            if (face == null) { face = vrcAvatarDescriptor.transform.Find("Body"); }
            if (face != null)
            {
                vrcAvatarDescriptor.lipSync = VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape;
                vrcAvatarDescriptor.VisemeSkinnedMesh = face.GetComponent<SkinnedMeshRenderer>();

                vrcAvatarDescriptor.customEyeLookSettings.eyelidType = VRCAvatarDescriptor.EyelidType.Blendshapes;
                vrcAvatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh = face.GetComponent<SkinnedMeshRenderer>();
                vrcAvatarDescriptor.customEyeLookSettings.eyelidsLookingUp = null;
                vrcAvatarDescriptor.customEyeLookSettings.eyelidsLookingDown = null;
            }
            if (armature != null)
            {
                vrcAvatarDescriptor.customEyeLookSettings.leftEye = FindChildGameObjectByName(armature, "Eye_L");
                vrcAvatarDescriptor.customEyeLookSettings.rightEye = FindChildGameObjectByName(armature, "Eye_R");
            }
        }

        public static Transform FindChildGameObjectByName(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).name == name)
                {
                    return parent.GetChild(i);
                }
                Transform child = FindChildGameObjectByName(parent.GetChild(i), name);
                if (child != null)
                {
                    return child;
                }
            }
            return null;
        }

        public static Material[] GetUniqueMaterials(GameObject obj)
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
            return uniqueMaterialArray;
        }

        public static void GenerateAnimationHueShaders(GameObject vrcAvatar)
        {
            var vrcAvatarDescriptor = vrcAvatar.GetComponent<VRCAvatarDescriptor>();
            var mats = GetUniqueMaterials(vrcAvatar);
            AnimationClip aClipLightLimitMin = new AnimationClip();
            AnimationClip aClipLightLimitMax = new AnimationClip();
            AnimationClip aClipGlitterLimitMin = new AnimationClip();
            AnimationClip aClipGlitterLimitMax = new AnimationClip();
            AnimationClip aClipOutlineOn = new AnimationClip();
            AnimationClip aClipOutlineOff = new AnimationClip();
            AnimationClip[] aClipMainHueMin = new AnimationClip[mats.Length];
            AnimationClip[] aClipMainHueMax = new AnimationClip[mats.Length];
            AnimationClip[] aClipMainSatMin = new AnimationClip[mats.Length];
            AnimationClip[] aClipMainSatMax = new AnimationClip[mats.Length];
            AnimationClip[] aClipMainBrightMin = new AnimationClip[mats.Length];
            AnimationClip[] aClipMainBrightMax = new AnimationClip[mats.Length];
            AnimationClip[] aClipDecal0HueMin = new AnimationClip[mats.Length];
            AnimationClip[] aClipDecal0HueMax = new AnimationClip[mats.Length];
            AnimationClip[] aClipDecal1HueMin = new AnimationClip[mats.Length];
            AnimationClip[] aClipDecal1HueMax = new AnimationClip[mats.Length];
            AnimationClip[] aClipDecal2HueMin = new AnimationClip[mats.Length];
            AnimationClip[] aClipDecal2HueMax = new AnimationClip[mats.Length];
            AnimationClip[] aClipDecal3HueMin = new AnimationClip[mats.Length];
            AnimationClip[] aClipDecal3HueMax = new AnimationClip[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                aClipMainHueMin[i] = new AnimationClip();
                aClipMainHueMax[i] = new AnimationClip();
                aClipMainSatMin[i] = new AnimationClip();
                aClipMainSatMax[i] = new AnimationClip();
                aClipMainBrightMin[i] = new AnimationClip();
                aClipMainBrightMax[i] = new AnimationClip();
                aClipDecal0HueMin[i] = new AnimationClip();
                aClipDecal0HueMax[i] = new AnimationClip();
                aClipDecal1HueMin[i] = new AnimationClip();
                aClipDecal1HueMax[i] = new AnimationClip();
                aClipDecal2HueMin[i] = new AnimationClip();
                aClipDecal2HueMax[i] = new AnimationClip();
                aClipDecal3HueMin[i] = new AnimationClip();
                aClipDecal3HueMax[i] = new AnimationClip();
            }


            var menuPoi = CreateNewMenu("Menu_Poi");
            var menuMain = CreateNewMenu("Menu_Poi_Main");
            var menuDecal0 = CreateNewMenu("Menu_Poi_Decal0");
            var menuDecal1 = CreateNewMenu("Menu_Poi_Decal1");
            var menuDecal2 = CreateNewMenu("Menu_Poi_Decal2");
            var menuDecal3 = CreateNewMenu("Menu_Poi_Decal3");

            string savePath = GetCurrentSceneRootPath() + "/Animations/Generated/Poi";
            foreach (var smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                //Debug.Log(smr.name);
                foreach (var mat in smr.sharedMaterials)
                {
                    //Debug.Log(mat.name);
                    int i = Array.IndexOf(mats, mats.FirstOrDefault(material => material.name == mat.name));
                    int propertyCount = ShaderUtil.GetPropertyCount(mat.shader);

                    var properties = Enumerable.Range(0, propertyCount)
                        .Where(j => ShaderUtil.GetPropertyType(mat.shader, j) == ShaderUtil.ShaderPropertyType.Range || ShaderUtil.GetPropertyType(mat.shader, j) == ShaderUtil.ShaderPropertyType.Float)
                        .Select(j => new { PropertyName = ShaderUtil.GetPropertyName(mat.shader, j), FloatValue = mat.GetFloat(ShaderUtil.GetPropertyName(mat.shader, j)) })
                        .Where(property => property.FloatValue == 1);

                    foreach (var property in properties)
                    {
                        string propertyName = property.PropertyName;
                        float floatValue = property.FloatValue;
                        var path = AnimationUtility.CalculateTransformPath(smr.transform, vrcAvatar.transform);
                        if (propertyName == "_EnableOutlines" && floatValue == 1)
                        {
                            aClipOutlineOff.SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_EnableOutlines", new AnimationCurve(new Keyframe(0, 0)));
                            aClipOutlineOn.SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_EnableOutlines", new AnimationCurve(new Keyframe(0, 1)));
                            aClipOutlineOff.name = "Outline Off";
                            aClipOutlineOn.name = "Outline On";
                        }
                        if (propertyName == "_LightingCapEnabled" && floatValue == 1)
                        {
                            aClipLightLimitMin.SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_LightingMinLightBrightness", new AnimationCurve(new Keyframe(0, 0f)));
                            aClipLightLimitMax.SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_LightingMinLightBrightness", new AnimationCurve(new Keyframe(0, 1f)));
                            aClipLightLimitMin.name = "LightLimit MIN";
                            aClipLightLimitMax.name = "LightLimit MAX";
                        }
                        if (propertyName == "_GlitterEnable" && floatValue == 1)
                        {
                            aClipGlitterLimitMin.SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_GlitterBrightness", new AnimationCurve(new Keyframe(0, 0f)));
                            aClipGlitterLimitMax.SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_GlitterBrightness", new AnimationCurve(new Keyframe(0, 2f)));
                            aClipGlitterLimitMin.name = "GlitterLimit MIN";
                            aClipGlitterLimitMax.name = "GlitterLimit MAX";
                        }
                        if (propertyName == "_MainHueShiftToggle" && floatValue == 1)
                        {
                            aClipMainHueMin[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_MainHueShift", new AnimationCurve(new Keyframe(0, 0)));
                            aClipMainHueMax[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_MainHueShift", new AnimationCurve(new Keyframe(0, 1)));
                            aClipMainSatMin[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_Saturation", new AnimationCurve(new Keyframe(0, -1)));
                            aClipMainSatMax[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_Saturation", new AnimationCurve(new Keyframe(0, 1)));
                            aClipMainBrightMin[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_MainBrightness", new AnimationCurve(new Keyframe(0, -1)));
                            aClipMainBrightMax[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_MainBrightness", new AnimationCurve(new Keyframe(0, 1)));

                            aClipMainHueMin[i].name = mat.name + "Main Hue" + " MIN";
                            aClipMainHueMax[i].name = mat.name + "Main Hue" + " MAX";
                            aClipMainSatMin[i].name = mat.name + "Main Saturation" + " MIN";
                            aClipMainSatMax[i].name = mat.name + "Main Saturation" + " MAX";
                            aClipMainBrightMin[i].name = mat.name + "Main Brightness" + " MIN";
                            aClipMainBrightMax[i].name = mat.name + "Main Brightness" + " MAX";
                        }
                        if (propertyName == "_DecalHueShiftEnabled" && floatValue == 1)
                        {
                            aClipDecal0HueMin[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_DecalHueShift", new AnimationCurve(new Keyframe(0, 0)));
                            aClipDecal0HueMax[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_DecalHueShift", new AnimationCurve(new Keyframe(0, 1)));
                            aClipDecal0HueMin[i].name = mat.name + "Decal0 Hue" + " MIN";
                            aClipDecal0HueMax[i].name = mat.name + "Decal0 Hue" + " MAX";

                        }
                        if (propertyName == "_DecalHueShiftEnabled1" && floatValue == 1)
                        {
                            aClipDecal1HueMin[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_DecalHueShift1", new AnimationCurve(new Keyframe(0, 0)));
                            aClipDecal1HueMax[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_DecalHueShift1", new AnimationCurve(new Keyframe(0, 1)));
                            aClipDecal1HueMin[i].name = mat.name + "Decal1 Hue" + " MIN";
                            aClipDecal1HueMax[i].name = mat.name + "Decal1 Hue" + " MAX";
                        }
                        if (propertyName == "_DecalHueShiftEnabled2" && floatValue == 1)
                        {
                            aClipDecal2HueMin[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_DecalHueShift2", new AnimationCurve(new Keyframe(0, 0)));
                            aClipDecal2HueMax[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_DecalHueShift2", new AnimationCurve(new Keyframe(0, 1)));
                            aClipDecal2HueMin[i].name = mat.name + "Decal2 Hue" + " MIN";
                            aClipDecal2HueMax[i].name = mat.name + "Decal2 Hue" + " MAX";
                        }
                        if (propertyName == "_DecalHueShiftEnabled3" && floatValue == 1)
                        {
                            aClipDecal3HueMin[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_DecalHueShift3", new AnimationCurve(new Keyframe(0, 0)));
                            aClipDecal3HueMax[i].SetCurve(path, typeof(SkinnedMeshRenderer), "material." + "_DecalHueShift3", new AnimationCurve(new Keyframe(0, 1)));
                            aClipDecal3HueMin[i].name = mat.name + "Decal3 Hue" + " MIN";
                            aClipDecal3HueMax[i].name = mat.name + "Decal3 Hue" + " MAX";
                        }
                    }
                }//end foreach mat
            }// end foreach smr
            if (aClipOutlineOn != null)
            {
                var clip1 = SaveAnimation(aClipOutlineOff, savePath);
                var clip2 = SaveAnimation(aClipOutlineOn, savePath);
                CreateBlendTree(vrcAvatarDescriptor, "Poi Outline", "Outline", 0, clip1, clip2);
                CreateMenuControl(menuPoi, "Outline", VRCExpressionsMenu.Control.ControlType.Toggle, "Outline");

            }
            if (aClipLightLimitMax != null)
            {
                var clip1 = SaveAnimation(aClipLightLimitMin, savePath);
                var clip2 = SaveAnimation(aClipLightLimitMax, savePath);
                CreateBlendTree(vrcAvatarDescriptor, "Poi LightLimit", "LightLimit", 0.1f, clip1, clip2);
                CreateMenuControl(menuPoi, "Min Light Limit", VRCExpressionsMenu.Control.ControlType.RadialPuppet, "LightLimit");
            }
            if (aClipGlitterLimitMax != null)
            {
                var clip1 = SaveAnimation(aClipGlitterLimitMin, savePath);
                var clip2 = SaveAnimation(aClipGlitterLimitMax, savePath);
                CreateBlendTree(vrcAvatarDescriptor, "Poi Glitter Limit", "GlitterLimit", clip1, clip2);
                CreateMenuControl(menuPoi, "Glitter Brightness", VRCExpressionsMenu.Control.ControlType.RadialPuppet, "GlitterLimit");
            }

            for (int i = 0; i < mats.Length; i++)
            {
                var mat = mats[i];
                Debug.Log(mat.name);
                var menuMat = CreateNewMenu("Menu_Poi_" + mat.name);
                CreateMenuControl(menuPoi, mat.name + " Settings", VRCExpressionsMenu.Control.ControlType.SubMenu, menuMat);
                if (aClipMainHueMax != null)
                {
                    var clip1 = SaveAnimation(aClipMainHueMin[i], savePath);
                    var clip2 = SaveAnimation(aClipMainHueMax[i], savePath);
                    var clip5 = SaveAnimation(aClipMainSatMin[i], savePath);
                    var clip6 = SaveAnimation(aClipMainSatMax[i], savePath);
                    var clip7 = SaveAnimation(aClipMainBrightMin[i], savePath);
                    var clip8 = SaveAnimation(aClipMainBrightMax[i], savePath);

                    CreateBlendTree(vrcAvatarDescriptor, mat.name + "Main Hue", mat.name + "MainHue", clip1, clip2);
                    CreateBlendTree(vrcAvatarDescriptor, mat.name + "Main Saturation", mat.name + "MainSat", 0.5f, clip5, clip6);
                    CreateBlendTree(vrcAvatarDescriptor, mat.name + "Main Brightness", mat.name + "MainBright", 0.5f, clip7, clip8);

                    CreateMenuControl(menuMat, "Main Hue Color", VRCExpressionsMenu.Control.ControlType.RadialPuppet, mat.name + "MainHue");
                    CreateMenuControl(menuMat, "Main Saturation", VRCExpressionsMenu.Control.ControlType.RadialPuppet, mat.name + "MainSat");
                    CreateMenuControl(menuMat, "Main Brightness", VRCExpressionsMenu.Control.ControlType.RadialPuppet, mat.name + "MainBright");

                }
                if (aClipDecal0HueMax != null)
                {
                    var clip1 = SaveAnimation(aClipDecal0HueMin[i], savePath);
                    var clip2 = SaveAnimation(aClipDecal0HueMax[i], savePath);
                    CreateBlendTree(vrcAvatarDescriptor, mat.name + "Decal0 Hue", mat.name + "Decal0Hue", clip1, clip2);
                    //CreateMenuControl(menuPoi, "Decal0 Settings", VRCExpressionsMenu.Control.ControlType.SubMenu, menuMat);
                    CreateMenuControl(menuMat, "Decal0 Hue Color", VRCExpressionsMenu.Control.ControlType.RadialPuppet, mat.name + "Decal0Hue");
                }
                if (aClipDecal1HueMax != null)
                {
                    var clip1 = SaveAnimation(aClipDecal1HueMin[i], savePath);
                    var clip2 = SaveAnimation(aClipDecal1HueMax[i], savePath);
                    CreateBlendTree(vrcAvatarDescriptor, mat.name + "Decal1 Hue", mat.name + "Decal1Hue", clip1, clip2);
                    CreateMenuControl(menuMat, "Decal1 Hue Color", VRCExpressionsMenu.Control.ControlType.RadialPuppet, mat.name + "Decal1Hue");
                }
                if (aClipDecal2HueMax != null)
                {
                    var clip1 = SaveAnimation(aClipDecal2HueMin[i], savePath);
                    var clip2 = SaveAnimation(aClipDecal2HueMax[i], savePath);
                    CreateBlendTree(vrcAvatarDescriptor, mat.name + "Decal2 Hue", mat.name + "Decal2Hue", clip1, clip2);
                    CreateMenuControl(menuMat, "Decal2 Hue Color", VRCExpressionsMenu.Control.ControlType.RadialPuppet, mat.name + "Decal2Hue");
                }
                if (aClipDecal3HueMax != null)
                {
                    var clip1 = SaveAnimation(aClipDecal3HueMin[i], savePath);
                    var clip2 = SaveAnimation(aClipDecal3HueMax[i], savePath);
                    CreateBlendTree(vrcAvatarDescriptor, mat.name + "Decal3 Hue", mat.name + "Decal3Hue", clip1, clip2);
                    CreateMenuControl(menuMat, "Decal3 Hue Color", VRCExpressionsMenu.Control.ControlType.RadialPuppet, mat.name + "Decal3Hue");
                }
            }
        }

        public static void CloneAndRegenerateGUIDs(string contextPath)
        {
            Dictionary<string, string> guidDictionary = new Dictionary<string, string>();
            string clonedPath = contextPath + " 1";
            string contextName = contextPath.Split('/')[1];
            if (AssetDatabase.IsValidFolder(clonedPath))
            {
                Debug.LogError("Cloned Folder already exists!" + clonedPath);
                return;
            }
            string[] assetPaths = Directory.GetFiles(contextPath, "*", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(".meta")) //&& !path.EndsWith(".fbx") && !path.EndsWith(".wav"))
            .ToArray();

            foreach (string originalAssetPath in assetPaths)
            {
                string assetPath = originalAssetPath.Replace("\\", "/");
                string relativePath = assetPath.Substring(contextPath.Length + 1);
                string clonedAssetPath = Path.Combine(clonedPath, relativePath).Replace("\\", "/");
                string directoryPath = Path.GetDirectoryName(clonedAssetPath);
                Directory.CreateDirectory(directoryPath);
                if (AssetDatabase.CopyAsset(assetPath, clonedAssetPath))
                {
                    guidDictionary[AssetDatabase.AssetPathToGUID(assetPath)] = AssetDatabase.AssetPathToGUID(clonedAssetPath);
                }
            }
            AssetDatabase.Refresh();
            string[] clonedAssetPaths = Directory.GetFiles(clonedPath, "*", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(".meta")) //&& !path.EndsWith(".fbx") && !path.EndsWith(".wav"))
            .ToArray();
            foreach (string clonedAsset in clonedAssetPaths)
            {
                UpdateGuidsInFile(clonedAsset, guidDictionary);
            }
            AssetDatabase.Refresh();
        }

        public static void UpdateGuidsInFile(string path, Dictionary<string,string> guidDictionary)
        {
            if (!File.Exists(path) || String.IsNullOrEmpty(path))
            {
                Debug.LogError($"File at {path} does not exist or is NULL/EMPTY");
                return;
            }

            string[] lines = File.ReadAllLines(path);

            if (lines.Length > 0)
            {
                if (lines[0].Contains("%YAML"))
                {
                    for (int i = 1; i < lines.Length; i++)
                    {
                        foreach (var pair in guidDictionary)
                        {
                            if (lines[i].Contains(pair.Key))
                            {
                                // Replace the old GUID with the new one
                                lines[i] = lines[i].Replace(pair.Key, pair.Value);
                            }
                        }
                    }
                    File.WriteAllLines(path, lines);
                }
            }            
        }
    }
}
