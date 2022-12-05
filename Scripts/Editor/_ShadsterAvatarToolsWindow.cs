//Made by Shadsterwolf, some code inspired by the VRCSDK, Av3Creator, and PumpkinTools
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
#if VRC_SDK_VRCSDK3 && !UDON
using VRC.Core;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC_PhysBone = VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone;
using VRC_PhysCollider = VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBoneCollider;
#endif

namespace Shadster.AvatarTools
{
    [System.Serializable]
    public class _ShadstersAvatarToolsWindow : EditorWindow
    {
        [SerializeField, HideInInspector] static ShadstersAvatarTools _tools;

        static EditorWindow toolWindow;
        private bool startInSceneView;
        private bool useExperimentalPlayMode;

        [SerializeReference] private VRCAvatarDescriptor vrcAvatarDescriptor;
        [SerializeReference] private GameObject vrcAvatar;
        [SerializeReference] private VRCExpressionParameters vrcParameters;
        [SerializeReference] private VRCExpressionsMenu vrcMenu;


        [SerializeReference] private AnimationClip clipA;
        [SerializeReference] private AnimationClip clipB;
        [SerializeReference] private string layerName;
        [SerializeReference] private string paramName;
        [SerializeReference] private int selectedParamType = 0;
        [SerializeReference] private int selectedControlType = 0;
        [SerializeReference] private string menuControlName;
        [SerializeReference] private bool createControlChecked;

        [SerializeReference] private Transform breastBoneL;
        [SerializeReference] private Transform breastBoneR;
        [SerializeReference] private Transform buttBoneL;
        [SerializeReference] private Transform buttBoneR;
        [SerializeReference] private Transform earBoneR;
        [SerializeReference] private Transform earBoneL;
        [SerializeReference] private Transform tailBone;

        public static ShadstersAvatarTools ToolsWindow
        {
            get
            {
                if (!_tools)
                    _tools = FindObjectOfType(typeof(ShadstersAvatarTools)) as ShadstersAvatarTools ?? CreateInstance<ShadstersAvatarTools>();
                return _tools;
            }

            private set
            {
                _tools = value;
            }
        }

        private void OnEnable()
        {
            if (EditorApplication.isPlaying) return;
            useExperimentalPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
            startInSceneView = ShadstersAvatarTools.GetStartPlayModeInSceneView();
        }

        private void OnInspectorUpdate()
        {
            if (vrcAvatar != null && vrcAvatarDescriptor == null) //because play mode likes to **** with me and clear the descriptor
                vrcAvatarDescriptor = vrcAvatar.GetComponent<VRCAvatarDescriptor>();
        }

        [MenuItem("ShadsterWolf/Show Avatar Tools", false, 0)]
        public static void ShowWindow()
        {
            if (!toolWindow)
            {
                toolWindow = EditorWindow.GetWindow(typeof(_ShadstersAvatarToolsWindow));
                toolWindow.autoRepaintOnSceneChange = true;
                toolWindow.titleContent = new GUIContent("ShadsterTools");
                toolWindow.minSize = new Vector2(500, 800);
            }
            toolWindow.Show();
        }

        public void ResetAll()
        {
            vrcAvatarDescriptor = null;
            vrcAvatar = null;
            vrcMenu = null;
            vrcParameters = null;

            breastBoneL = null;
            breastBoneR = null;
            buttBoneL = null;
            buttBoneR = null;
            clipA = null;
            clipB = null;
            layerName = "";
            paramName = "";
            selectedParamType = 0;
        }

        public void AutoDetect()
        {
            vrcAvatarDescriptor = SelectCurrentAvatarDescriptor();
            vrcAvatar = vrcAvatarDescriptor.gameObject;
            vrcMenu = vrcAvatarDescriptor.expressionsMenu;
            vrcParameters = vrcAvatarDescriptor.expressionParameters;

            breastBoneL = GetAvatarBone(vrcAvatar, "Breast", "_L");
            breastBoneR = GetAvatarBone(vrcAvatar, "Breast", "_R");
            buttBoneL = GetAvatarBone(vrcAvatar, "Butt", "_L");
            buttBoneR = GetAvatarBone(vrcAvatar, "Butt", "_R");
        }

        private static void UseExperimentalPlayMode(bool value)
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
            var potentialObjects = Object.FindObjectsOfType<VRCAvatarDescriptor>().ToArray();
            if (potentialObjects.Length > 0)
            {
                avatarDescriptor = potentialObjects.First();
            }

            return avatarDescriptor;
        }

        private static List<SkinnedMeshRenderer> GetAvatarSkinnedMeshRenderers(GameObject root, Bounds bounds)
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

        private static void EncapsulateAvatarBounds(GameObject vrcAvatar)
        {
            Undo.RecordObject(vrcAvatar, "Combine Mesh Bounds");
            Bounds bounds = CalculateLocalBounds(vrcAvatar);
            foreach (SkinnedMeshRenderer smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                smr.localBounds = bounds;
            }

        }

        private static void ResetAvatarBounds(GameObject vrcAvatar)
        {
            foreach (SkinnedMeshRenderer smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                smr.sharedMesh.RecalculateBounds();
            }

        }

        private static void OverrideAvatarBounds(GameObject vrcAvatar)
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
        private static void OverrideAvatarAnchorProbes(GameObject vrcAvatar)
        {

            foreach (Renderer r in vrcAvatar.GetComponentsInChildren<Renderer>(true))
            {
                Undo.RecordObject(r, "Set Avatar Anchor Probe");
                r.probeAnchor = GetAvatarArmature(vrcAvatar).Find("Hips");
            }
        }

        public static Transform GetAvatarArmature(GameObject vrcAvatar)
        {
            Transform vrcTransform = vrcAvatar.transform;
            Transform armature = vrcTransform.Find("Armature");

            return armature;
        }

        public static bool GogoLocoExist()
        {
            if (!(string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID("Assets/GoGo/Loco/GoControllers/GoLocoBase.controller"))))
            {
                return true;
            }
            return false;
        }

        private static void SetupGogoLocoLayers(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            vrcAvatarDescriptor.customizeAnimationLayers = true; //ensure customizing playable layers is true
            vrcAvatarDescriptor.autoLocomotion = false; //disable force 6-point tracking

            vrcAvatarDescriptor.baseAnimationLayers[0].isDefault = false; //Base
            vrcAvatarDescriptor.baseAnimationLayers[3].isDefault = false; //Action
            vrcAvatarDescriptor.specialAnimationLayers[0].isDefault = false; //Sitting

            vrcAvatarDescriptor.baseAnimationLayers[0].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/Loco/GoControllers/GoLocoBase.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.baseAnimationLayers[3].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/Loco/GoControllers/GoLocoAction.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.specialAnimationLayers[0].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/Loco/GoControllers/GoLocoSitting.controller", typeof(RuntimeAnimatorController));
            //Debug.Log(AssetDatabase.GetAssetPath(vrcAvatarDescriptor.specialAnimationLayers[0].animatorController));
        }

        private static void SetupGogoLocoMenu(VRCExpressionsMenu vrcMenu)
        {
            var subMenu = (VRCExpressionsMenu)AssetDatabase.LoadAssetAtPath("Assets/GoGo/Loco/GoMenus/GoAllMainMenu.asset", typeof(VRCExpressionsMenu));
            var icon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/GoGo/Loco/Icons/icon_Go_Loco.png", typeof(Texture2D));
            CreateMenuControl(vrcMenu, "GoGo Loco Menu", VRCExpressionsMenu.Control.ControlType.SubMenu, "", subMenu, icon);
        }

        private static void SetupGogoLocoParams(VRCExpressionParameters vrcParameters)
        {
            CreateVrcParameter(vrcParameters, "VRCEmote", VRCExpressionParameters.ValueType.Int, 0, false);
            CreateVrcParameter(vrcParameters, "Go/Float", VRCExpressionParameters.ValueType.Float, 0.25f, false);
            CreateVrcParameter(vrcParameters, "Go/Stationary", VRCExpressionParameters.ValueType.Bool, 0, false);
            CreateVrcParameter(vrcParameters, "Go/Locomotion", VRCExpressionParameters.ValueType.Bool, 0, true);
            CreateVrcParameter(vrcParameters, "Go/JumpAndFall", VRCExpressionParameters.ValueType.Bool, 0, true);
        }

        public float GetAvatarHeight(GameObject vrcAvatar)
        {
            Animator anim = vrcAvatar.GetComponent<Animator>();
            Transform shoulderL = anim.GetBoneTransform(HumanBodyBones.LeftShoulder);
            float height = shoulderL.position.y - vrcAvatar.transform.position.y;
            Debug.Log(height);
            return height;
        }



        private Transform GetAvatarBone(GameObject vrcAvatar, string search, string direction)
        {
            Transform armature = GetAvatarArmature(vrcAvatar);
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

        public static List<VRC_PhysBone> GetAllAvatarPhysBones(GameObject vrcAvatar)
        {
            Transform armature = GetAvatarArmature(vrcAvatar);
            List<VRC_PhysBone> result = new List<VRC_PhysBone>();
            if (armature != null)
            {
                foreach (Transform bone in armature.GetComponentsInChildren<Transform>(true))
                {
                    if (ShadstersAvatarTools.BoneHasPhysBones(bone))
                    {
                        VRC_PhysBone pBone = bone.GetComponent<VRC_PhysBone>();
                        result.Add(pBone);
                    }


                }
            }
            //Debug.Log(result);
            return result;
        }

        private static void DeleteEndBones(GameObject vrcAvatar)
        {
            Transform armature = GetAvatarArmature(vrcAvatar);
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



        public static AnimationCurve LinearAnimationCurve()
        {
            AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.5f));
            curve.preWrapMode = WrapMode.Default;
            curve.postWrapMode = WrapMode.Default;

            return curve;
        }

        private static void AddPhysBones(Transform bone)
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

        private static void AddButtPhysBones(Transform bone)
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

        public static string GetCurrentSceneRootPath()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            string scenePath = currentScene.path;
            string currentPath = Path.GetDirectoryName(scenePath);
            currentPath = currentPath.Replace("\\", "/"); //I am suffering
            return currentPath;

        }



        private static void SaveAvatarPrefab(GameObject vrcAvatar)
        {
            string prefabPath = GetCurrentSceneRootPath() + "/Prefabs";
            if (!(AssetDatabase.IsValidFolder(prefabPath))) //If folder doesn't exist "Assets\AvatarName\Prefabs"
            {
                Directory.CreateDirectory(prefabPath);
            }
            string savePath = prefabPath + "/" + vrcAvatar.name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(vrcAvatar, savePath);
        }

        public static List<Object> GetAvatarTextures(GameObject vrcAvatar)
        {
            List<Object> aTextures = new List<Object>();
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

        private static void UncheckAvatarTextureMipMaps(GameObject vrcAvatar)
        {
            List<string> paths = new List<string>();
            List<Object> aTextures = GetAvatarTextures(vrcAvatar);
            if (aTextures.Count > 0)
            {
                foreach (Object o in aTextures)
                {
                    TextureImporter t = (TextureImporter)o;
                    if (t.mipmapEnabled)
                    {
                        Undo.RecordObject(t, "Un-Generate Mip Maps");
                        t.mipmapEnabled = false;
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

        private static void SetAvatarTexturesMaxSize(GameObject vrcAvatar, int maxSize)
        {
            List<string> paths = new List<string>();
            List<Object> aTextures = GetAvatarTextures(vrcAvatar);
            if (aTextures.Count > 0)
            {
                Debug.Log(aTextures.Count);
                foreach (Object o in aTextures)
                {
                    Debug.Log(o);
                    TextureImporter t = (TextureImporter)o;
                    if (t.maxTextureSize != maxSize)
                    {
                        Undo.RecordObject(t, "Set Textures size to 4k");
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

        private static void SetAllGrabMovement(GameObject vrcAvatar)
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

        private static void UncheckAllWriteDefaults(VRCAvatarDescriptor vrcAvatarDescriptor)
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
        private static void SaveAnimation(AnimationClip anim, string savePath)
        {
            if (!(AssetDatabase.IsValidFolder(savePath)))
                Directory.CreateDirectory(savePath);
            savePath = savePath + "/" + anim.name + ".anim";
            AssetDatabase.CreateAsset(anim, savePath);
        }

        private static void GenerateAnimationRenderToggles(GameObject vrcAvatar)
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
                aClipOn.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 1)));
                allOff.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 0)));
                allOn.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(new Keyframe(0, 1)));

                SaveAnimation(aClipOn, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");
                SaveAnimation(aClipOff, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");
            }
            SaveAnimation(allOff, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");
            SaveAnimation(allOn, GetCurrentSceneRootPath() + "/Animations/Generated/Toggles");

        }

        private static void GenerateAnimationShapekeys(GameObject vrcAvatar)
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

        private static void CombineAnimationShapekeys(GameObject vrcAvatar)
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

        private static void CombineEmoteShapekeys(GameObject vrcAvatar)
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

        private void GenerateEmoteOverrideMenu(VRCAvatarDescriptor vrcAvatarDescriptor)
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
                for (int i = 0; i < emoteFiles.Length; i++)
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

        public static AnimatorController GetFxController(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            var runtime = vrcAvatarDescriptor.baseAnimationLayers[4].animatorController;
            //return AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(runtime));
            return (AnimatorController)runtime;
        }

        private void DeleteExistingFxLayer(AnimatorController fx, string layerName)
        {
            for (int i = 0; i < fx.layers.Length; i++) //delete existing layer
            {
                if (fx.layers[i].name.Equals(layerName))
                {
                    fx.RemoveLayer(i);
                    break;
                }
            }
        }

        private void CreateToggle(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            var fx = GetFxController(vrcAvatarDescriptor);
            CreateFxParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Bool);
            DeleteExistingFxLayer(fx, layerName);

            fx.AddLayer(layerName);
            var fxLayers = fx.layers;
            var newLayer = fxLayers[fx.layers.Length - 1];
            newLayer.defaultWeight = 1f;

            var startState = newLayer.stateMachine.AddState(clipA.name, new Vector3(250, 120));
            startState.writeDefaultValues = false;
            startState.motion = clipA;

            var endState = newLayer.stateMachine.AddState(clipB.name, new Vector3(250, 20));
            endState.writeDefaultValues = false;
            endState.motion = clipB;

            startState.AddTransition(endState);
            startState.transitions[0].hasFixedDuration = true;
            startState.transitions[0].duration = 0f;
            startState.transitions[0].exitTime = 0f;
            startState.transitions[0].hasExitTime = false;
            startState.transitions[0].AddCondition(AnimatorConditionMode.If, 0f, paramName);

            endState.AddTransition(startState);
            endState.transitions[0].hasFixedDuration = true;
            endState.transitions[0].duration = 0f;
            endState.transitions[0].exitTime = 0f;
            endState.transitions[0].hasExitTime = false;
            endState.transitions[0].AddCondition(AnimatorConditionMode.IfNot, 0f, paramName);

            fx.layers = fxLayers; //fixes save for default weight for some reason
            EditorUtility.SetDirty(fx);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        private void CreateBlendTree(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            var fx = GetFxController(vrcAvatarDescriptor);
            CreateFxParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Float);

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

        private static void CreateMenuControl(VRCExpressionsMenu vrcMenu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName)
        {
            CreateMenuControl(vrcMenu, controlName, controlType, paramName, null, null, 1);
        }
        private static void CreateMenuControl(VRCExpressionsMenu vrcMenu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName, int value)
        {
            CreateMenuControl(vrcMenu, controlName, controlType, paramName, null, null, value);
        }
        private static void CreateMenuControl(VRCExpressionsMenu vrcMenu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName, Texture2D icon)
        {
            CreateMenuControl(vrcMenu, controlName, controlType, paramName, null, icon, 1);
        }
        private static void CreateMenuControl(VRCExpressionsMenu vrcMenu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName, VRCExpressionsMenu subMenu, Texture2D icon)
        {
            CreateMenuControl(vrcMenu, controlName, controlType, paramName, subMenu, icon, 1);
        }

        public void CreateMenuControl(VRCExpressionsMenu menu, string controlName, int controlType, string paramName)
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
        private static void CreateMenuControl(VRCAvatarDescriptor vrcAvatarDescriptor, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName)
        {
            //var param = vrcAvatarDescriptor.expressionParameters;
            var vrcMenu = vrcAvatarDescriptor.expressionsMenu;
            CreateMenuControl(vrcMenu, controlName, controlType, paramName);
        }



        private static void CreateMenuControl(VRCExpressionsMenu vrcMenu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName, VRCExpressionsMenu subMenu, Texture2D icon, int value)
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
                EditorUtility.DisplayDialog("Menu control full!", "Free up controls or make a new one", "Ok");
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
            var menu = new VRCExpressionsMenu();
            string saveFolder = GetCurrentSceneRootPath() + "/" + "Menus";
            if (!(AssetDatabase.IsValidFolder(saveFolder)))
                Directory.CreateDirectory(saveFolder);
            string savePath = GetCurrentSceneRootPath() + "/" + "Menus" + "/" + menuName + ".asset";
            menu.name = menuName;
            AssetDatabase.CreateAsset(menu, savePath);
            return menu;
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
            vrcExParams.Add(newVrcExParam);
            vrcParameters.parameters = vrcExParams.ToArray();

            EditorUtility.SetDirty(vrcParameters);
            AssetDatabase.Refresh();

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
            var fx = GetFxController(vrcAvatarDescriptor);
            VRCExpressionParameters.ValueType vrcParamType = ConvertAnimatorToVrcParamType(dataType);

            for (int i = 0; i < fx.parameters.Length; i++)
            {
                if (paramName.Equals(fx.parameters[i].name))
                    fx.RemoveParameter(i); //Remove anyway just in case theres a new datatype
            }
            fx.AddParameter(paramName, dataType);

            CreateVrcParameter(vrcAvatarDescriptor.expressionParameters, paramName, vrcParamType);

            EditorUtility.SetDirty(fx);
            AssetDatabase.Refresh();
        }

        public void MovePhysBonesFromArmature(GameObject vrcAvatar)
        {
            var armature = GetAvatarArmature(vrcAvatar);
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
            var physObjectRoot = new GameObject(name = "PhysBones");
            physObjectRoot.transform.parent = vrcAvatar.transform;
            foreach (var pBone in physbones)
            {
                var physObject = new GameObject(name = pBone.name);
                physObject.transform.parent = physObjectRoot.transform;
                var copyPBone = CopyComponent(pBone, physObject);

                if (pBone.rootTransform == null)
                {
                    copyPBone.rootTransform = pBone.transform;
                }
                DestroyImmediate(pBone);
            }
        }

        public void MovePhysCollidersFromArmature(GameObject vrcAvatar)
        {
            var armature = GetAvatarArmature(vrcAvatar);
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
            var physObjectRoot = new GameObject(name = "PhysColliders");
            physObjectRoot.transform.parent = vrcAvatar.transform;
            foreach (var pCollider in physcolliders)
            {
                var physObject = new GameObject(name = pCollider.name);
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

        public void UpdatePhysBoneCollider(VRC_PhysBone pbone, VRC_PhysCollider pCollider)
        {

        }

        public static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            var type = original.GetType();
            var copy = destination.AddComponent(type);
            var fields = type.GetFields();
            foreach (var field in fields) field.SetValue(copy, field.GetValue(original));
            return copy as T;
        }

        public void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                vrcAvatarDescriptor = (VRCAvatarDescriptor)EditorGUILayout.ObjectField(vrcAvatarDescriptor, typeof(VRCAvatarDescriptor), true, GUILayout.Height(24));

                if (GUILayout.Button("Auto-Detect", GUILayout.Height(24)))
                {
                    AutoDetect();
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                vrcAvatar = (GameObject)EditorGUILayout.ObjectField(vrcAvatar, typeof(GameObject), true, GUILayout.Height(24));


                if (GUILayout.Button("Reset-All", GUILayout.Height(24)))
                {
                    ResetAll();
                }

            }
            using (new EditorGUILayout.HorizontalScope())
            {
                vrcMenu = (VRCExpressionsMenu)EditorGUILayout.ObjectField(vrcMenu, typeof(VRCExpressionsMenu), true, GUILayout.Height(24));
                vrcParameters = (VRCExpressionParameters)EditorGUILayout.ObjectField(vrcParameters, typeof(VRCExpressionParameters), true, GUILayout.Height(24));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                var sceneToggleState = GUILayout.Toggle(startInSceneView, new GUIContent("Start Play Mode in Scene View", "Loads prefab that will start play mode to Scene view instead of starting in Game View"), GUILayout.Height(24));
                if (sceneToggleState != startInSceneView)
                {
                    ShadstersAvatarTools.SetStartPlayModeInSceneView(sceneToggleState);
                    startInSceneView = ShadstersAvatarTools.GetStartPlayModeInSceneView();
                }
                var playModeToggleState = GUILayout.Toggle(useExperimentalPlayMode, new GUIContent("Use Experimental Play Mode", "Instantly loads entering play mode, save often and disable if issues occur"), GUILayout.Height(24));
                if (playModeToggleState != useExperimentalPlayMode)
                {
                    UseExperimentalPlayMode(playModeToggleState);
                    useExperimentalPlayMode = playModeToggleState;

                }
            }

            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3));
            using (new EditorGUI.DisabledScope(vrcAvatarDescriptor == null))
            {
                using (new EditorGUI.DisabledScope(!GogoLocoExist()))
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Setup Gogo Layers", GUILayout.Height(24)))
                    {
                        SetupGogoLocoLayers(vrcAvatarDescriptor);
                    }
                    if (GUILayout.Button("Add Gogo Menu", GUILayout.Height(24)))
                    {
                        SetupGogoLocoMenu(vrcMenu);
                    }
                    if (GUILayout.Button("Add Gogo Params", GUILayout.Height(24)))
                    {
                        SetupGogoLocoParams(vrcParameters);
                    }

                }

                GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3));
                if (GUILayout.Button("Delete End Bones", GUILayout.Height(24)))
                {
                    DeleteEndBones(vrcAvatar);
                }
                //if (GUILayout.Button("Combine Mesh Bounds", GUILayout.Height(24)))
                //{
                //    EncapsulateAvatarBounds(vrcAvatar);
                //}

                if (GUILayout.Button("Set All Mesh Bounds to 2.5sq", GUILayout.Height(24)))
                {
                    OverrideAvatarBounds(vrcAvatar);
                }
                if (GUILayout.Button("Set All Anchor Probes to Hip", GUILayout.Height(24)))
                {
                    OverrideAvatarAnchorProbes(vrcAvatar);
                }

                //if (GUILayout.Button("Reset Mesh Bounds", GUILayout.Height(24)))
                //{
                //    ResetAvatarBounds(vrcAvatar);
                //}

                if (GUILayout.Button("Clear Avatar Blueprint ID", GUILayout.Height(24)))
                {
                    ShadstersAvatarTools.ClearAvatarBlueprintID(vrcAvatar);
                }
                if (GUILayout.Button("Save Avatar Prefab", GUILayout.Height(24)))
                {
                    SaveAvatarPrefab(vrcAvatar);
                }
                if (GUILayout.Button("Uncheck All Texture Mip Maps", GUILayout.Height(24)))
                {
                    UncheckAvatarTextureMipMaps(vrcAvatar);
                }
                using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Set Textures Max Size 2k", GUILayout.Height(24)))
                    {
                        SetAvatarTexturesMaxSize(vrcAvatar, 2048);
                    }
                    if (GUILayout.Button("Set Textures Max Size 4k", GUILayout.Height(24)))
                    {
                        SetAvatarTexturesMaxSize(vrcAvatar, 4096);
                    }
                }
                if (GUILayout.Button("Uncheck All Write Defaults states", GUILayout.Height(24)))
                {
                    UncheckAllWriteDefaults(vrcAvatarDescriptor);
                }


                GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3)); // NEW LINE ----------------------
                var fold = EditorGUILayout.Foldout(false, "Bones", true);
                if (fold)
                {
                    EditorGUILayout.LabelField("Breast Bones");
                    using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                    {
                        breastBoneL = (Transform)EditorGUILayout.ObjectField(breastBoneL, typeof(Transform), true, GUILayout.Height(24));
                        breastBoneR = (Transform)EditorGUILayout.ObjectField(breastBoneR, typeof(Transform), true, GUILayout.Height(24));
                    }
                    EditorGUILayout.LabelField("Butt Bones");
                    using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                    {
                        buttBoneL = (Transform)EditorGUILayout.ObjectField(buttBoneL, typeof(Transform), true, GUILayout.Height(24));
                        buttBoneR = (Transform)EditorGUILayout.ObjectField(buttBoneR, typeof(Transform), true, GUILayout.Height(24));
                    }
                    if (GUILayout.Button("Auto Add PhysBones", GUILayout.Height(24)))
                    {
                        AddPhysBones(breastBoneL);
                        AddPhysBones(breastBoneR);
                        AddButtPhysBones(buttBoneL);
                        AddButtPhysBones(buttBoneR);
                    }
                }
                //if (GUILayout.Button("Update PhysBones", GUILayout.Height(24)))
                //{
                //    AddPhysBones(breastBoneL);
                //    AddPhysBones(breastBoneR);
                //    AddButtPhysBones(buttBoneL);
                //    AddButtPhysBones(buttBoneR);
                //}
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Move PhysBones from Armature", GUILayout.Height(24)))
                    {
                        MovePhysBonesFromArmature(vrcAvatar);
                    }
                    if (GUILayout.Button("Move Colliders from Armature", GUILayout.Height(24)))
                    {
                        MovePhysCollidersFromArmature(vrcAvatar);
                    }
                }
                if (GUILayout.Button("Set All Grab Movement to 1", GUILayout.Height(24)))
                {
                    SetAllGrabMovement(vrcAvatar);
                }

                GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3)); // NEW LINE ----------------------

                if (GUILayout.Button("Generate Animation Render Toggles", GUILayout.Height(24)))
                {
                    GenerateAnimationRenderToggles(vrcAvatar);
                }
                if (GUILayout.Button("Generate Animation Shapekeys", GUILayout.Height(24)))
                {
                    //GenerateAnimationShapekeys(vrcAvatar);
                    CombineAnimationShapekeys(vrcAvatar);
                    CombineEmoteShapekeys(vrcAvatar);
                }
                if (GUILayout.Button("Generate Emote Override Menu", GUILayout.Height(24)))
                {
                    GenerateEmoteOverrideMenu(vrcAvatarDescriptor);
                }
                using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Main/Start Clip", GUILayout.Height(24));
                    GUILayout.Label("Last/End Clip", GUILayout.Height(24));
                }
                using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                {
                    clipA = (AnimationClip)EditorGUILayout.ObjectField(clipA, typeof(AnimationClip), true, GUILayout.Height(24));
                    clipB = (AnimationClip)EditorGUILayout.ObjectField(clipB, typeof(AnimationClip), true, GUILayout.Height(24));
                }
                using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Layer Name:", GUILayout.Height(24));
                    layerName = EditorGUILayout.TextField(layerName, GUILayout.Height(24));
                }
                using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Param Name:", GUILayout.Height(24));
                    paramName = EditorGUILayout.TextField(paramName, GUILayout.Height(24));
                }
                //vrcParameters = (VRCExpressionParameters)EditorGUILayout.ObjectField(vrcParameters, typeof(VRCExpressionParameters), true, GUILayout.Height(24));
                if (GUILayout.Button("Create/Overwrite Toggle FX Layer (bool)", GUILayout.Height(24)))
                {
                    CreateToggle(vrcAvatarDescriptor);
                }
                if (GUILayout.Button("Create/Overwrite BlendTree FX Layer (float)", GUILayout.Height(24)))
                {
                    CreateBlendTree(vrcAvatarDescriptor);
                }

                using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Create/Overwrite Parameter", GUILayout.Height(24)))
                    {
                        CreateFxParameter(vrcAvatarDescriptor, paramName, selectedParamType);
                    }
                    selectedParamType = GUILayout.SelectionGrid(selectedParamType, new string[] { "bool", "int", "float" }, 3, GUILayout.Height(24));
                }
                using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Menu Control Name:", GUILayout.Height(24));
                    menuControlName = EditorGUILayout.TextField(menuControlName, GUILayout.Height(24));
                    selectedControlType = GUILayout.SelectionGrid(selectedControlType, new string[] { "Toggle", "Button", "Two AP", "Four AP", "Radial" }, 5, GUILayout.Height(24));
                }
                if (GUILayout.Button("Create/Overwrite Menu Control", GUILayout.Height(24)))
                {
                    CreateMenuControl(vrcMenu, menuControlName, selectedControlType, paramName);
                }
                if (GUILayout.Button("Cleanup & Export from current scene", GUILayout.Height(24)))
                {
                    ShadstersAvatarTools.Export();
                }


            } // Using Disable Scope
            //EditorGUILayout.LabelField("<i> Version " + version + " </i>", new GUIStyle(GUI.skin.label)
            //{
            //    richText = true,
            //    alignment = TextAnchor.MiddleRight
            //});

        } // GUI
    } // Class
} // Namespace