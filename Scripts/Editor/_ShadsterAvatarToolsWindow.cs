//Made by Shadsterwolf, some code reverse engineered by the VRCSDK, Av3Creator, and PumpkinTools
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
#if VRC_SDK_VRCSDK3 && !UDON
using VRC.Core;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC_PhysBone = VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone;
#endif

namespace Shadster.AvatarTools
{
    [System.Serializable]
    public class _ShadstersAvatarToolsWindow : EditorWindow
    {
        [SerializeField, HideInInspector] static ShadstersAvatarTools _tools;
        //private string version = "0.4.4";

        static EditorWindow toolWindow;
        private bool startInSceneView = false;
        private GameObject startInSceneViewPrefab;
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
            startInSceneViewPrefab = GameObject.Find("StartInSceneView(Clone)"); //Find if existing prefab is already in Hierarchy
            if (startInSceneViewPrefab != null)
                startInSceneView = true;
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
            vrcParameters = vrcAvatarDescriptor.expressionParameters;
            vrcMenu = vrcAvatarDescriptor.expressionsMenu;
            breastBoneL = GetAvatarBone(vrcAvatar, "Breast", "_L");
            breastBoneR = GetAvatarBone(vrcAvatar, "Breast", "_R");
            buttBoneL = GetAvatarBone(vrcAvatar, "Butt", "_L");
            buttBoneR = GetAvatarBone(vrcAvatar, "Butt", "_R");
        }

        private  bool VerifyAvatarLoaded()
        {
            return vrcAvatarDescriptor != null;
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

        private void StartPlayModeInSceneView(bool value)
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath("38bc44479eca0c9409fb9a16a3a9a873");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (value)
                startInSceneViewPrefab = Instantiate(prefab);
            else
                DestroyImmediate(startInSceneViewPrefab, true);
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
            if (!(string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID("Assets/GoGo Loco/Go All/Go All Controller/Go All Base 1.5.controller"))))
            {
                return true;
            }
            return false;
        }

        private static void PrepGogoLoco(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            vrcAvatarDescriptor.customizeAnimationLayers = true; //ensure customizing playable layers is true
            vrcAvatarDescriptor.autoLocomotion = false; //disable force 6-point tracking

            vrcAvatarDescriptor.baseAnimationLayers[0].isDefault = false; //Base
            vrcAvatarDescriptor.baseAnimationLayers[3].isDefault = false; //Action
            vrcAvatarDescriptor.specialAnimationLayers[0].isDefault = false; //Sitting

            vrcAvatarDescriptor.baseAnimationLayers[0].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo Loco/Go All/Go All Controller/Go All Base 1.5.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.baseAnimationLayers[3].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo Loco/Go All/Go All Controller/Go All Action.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.specialAnimationLayers[0].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo Loco/Go All/Go All Controller/Go All Sitting.controller", typeof(RuntimeAnimatorController));
            //Debug.Log(AssetDatabase.GetAssetPath(vrcAvatarDescriptor.specialAnimationLayers[0].animatorController));
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

        public static bool BoneHasPhysBones(Transform bone)
        {
            //Debug.Log(bone);
            if (bone.GetComponent<VRC_PhysBone>() != null)
            {
                return true;
            }
            return false;
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
            if (!BoneHasPhysBones(bone))
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
            if (!BoneHasPhysBones(bone))
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

        private static void ClearAvatarBlueprintID(GameObject vrcAvatar)
        {
            PipelineManager blueprint = vrcAvatar.GetComponent<PipelineManager>();
            blueprint.blueprintId = null;
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

        public static AnimatorController GetFxController(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            var runtime = vrcAvatarDescriptor.baseAnimationLayers[4].animatorController;
            return AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(runtime));
        }

        private void CreateToggle(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            var fx = GetFxController(vrcAvatarDescriptor);
            CreateParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Bool);

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

            var startState = newLayer.stateMachine.AddState(clipA.name, new Vector3(250, 120));
            startState.writeDefaultValues = false;
            startState.motion = clipA;

            var endState = newLayer.stateMachine.AddState(clipB.name, new Vector3(250, 20));
            endState.writeDefaultValues = false;
            endState.motion = clipB;

            EditorUtility.SetDirty(startState);
            EditorUtility.SetDirty(endState);

            startState.AddTransition(new AnimatorStateTransition
            {
                destinationState = endState,
                hasFixedDuration = true,
                duration = 0f,
                exitTime = 0f,
                hasExitTime = false
            });
            startState.transitions[0].AddCondition(AnimatorConditionMode.If, 0f, paramName);

            endState.AddTransition(new AnimatorStateTransition
            {
                destinationState = startState,
                hasFixedDuration = true,
                duration = 0f,
                exitTime = 0f,
                hasExitTime = false
            });
            endState.transitions[0].AddCondition(AnimatorConditionMode.IfNot, 0f, paramName);

            fx.layers = fxLayers; //fixes save for default weight for some reason

            EditorUtility.SetDirty(fx);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        private void CreateBlendTree(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            var fx = GetFxController(vrcAvatarDescriptor);
            CreateParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Float);

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

            var newBlendTreeState = newLayer.stateMachine.AddState("Blend Tree", new Vector3(250, 120));
            var newBlendTree = new BlendTree();
            newBlendTree.AddChild(clipA, 0);
            newBlendTree.AddChild(clipB, 1);
            newBlendTree.name = "Blend Tree";
            newBlendTree.blendParameter = paramName;
            newBlendTree.blendType = BlendTreeType.Simple1D;
            newBlendTreeState.motion = newBlendTree;
            newBlendTreeState.writeDefaultValues = false;

            fx.layers = fxLayers; //fixes save for default weight for some reason

            EditorUtility.SetDirty(fx);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
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
        private void CreateMenuControl(VRCAvatarDescriptor vrcAvatarDescriptor, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName)
        {
            //var param = vrcAvatarDescriptor.expressionParameters;
            var menu = vrcAvatarDescriptor.expressionsMenu;
            CreateMenuControl(menu, controlName, controlType, paramName);
        }

        private void CreateMenuControl(VRCExpressionsMenu menu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName)
        {
            foreach (var control in menu.controls)
            {
                if (control.name.Equals(controlName))
                {
                    menu.controls.Remove(control); 
                    break;
                }
            }
            if (menu.controls.Count == 8)
            {
                EditorUtility.DisplayDialog("Menu control full!", "Free up controls or make a new one", "Ok");
                return;
            }
            var item = new VRCExpressionsMenu.Control {
                name = controlName,
                type = controlType,
            };
            if (controlType == VRCExpressionsMenu.Control.ControlType.RadialPuppet)
            {
                item.subParameters = new VRCExpressionsMenu.Control.Parameter[]
                { new  VRCExpressionsMenu.Control.Parameter {
                    name = paramName
                }};
            }
            else
            {
                item.parameter = new VRCExpressionsMenu.Control.Parameter
                {
                    name = paramName
                };
            }

            menu.controls.Add(item);
            EditorUtility.SetDirty(menu);

        }

        public static void CreateParameter(VRCAvatarDescriptor vrcAvatarDescriptor, string paramName, int dataType)
        {
            if (dataType == 1)
                CreateParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Int);
            else if (dataType == 2)
                CreateParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Float);
            else
                CreateParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Bool);
        }

        public static void CreateParameter(VRCAvatarDescriptor vrcAvatarDescriptor, string paramName, AnimatorControllerParameterType dataType)
        {
            var vrcEx = vrcAvatarDescriptor.expressionParameters;
            var fx = GetFxController(vrcAvatarDescriptor);
            VRCExpressionParameters.ValueType vrcExType;
            switch (dataType)
            {
                case AnimatorControllerParameterType.Int:
                    vrcExType = VRCExpressionParameters.ValueType.Int;
                    break;
                case AnimatorControllerParameterType.Float:
                    vrcExType = VRCExpressionParameters.ValueType.Float;
                    break;
                default:
                    vrcExType = VRCExpressionParameters.ValueType.Bool;
                    break;
            }


            for (int i = 0; i < fx.parameters.Length; i++)
            {
                if (paramName.Equals(fx.parameters[i].name))
                    fx.RemoveParameter(i); //Remove anyway just in case theres a new datatype
            }
            fx.AddParameter(paramName,dataType);

            var vrcExParams = vrcEx.parameters.ToList();
            for (int i = 0; i < vrcEx.parameters.Length; i++)
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
                defaultValue = 0
            };
            vrcExParams.Add(newVrcExParam);
            vrcEx.parameters = vrcExParams.ToArray();

            EditorUtility.SetDirty(fx);
            EditorUtility.SetDirty(vrcEx);
            AssetDatabase.Refresh();

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
                var sceneToggleState = GUILayout.Toggle(startInSceneView, new GUIContent("Start Play Mode in Scene View", "Loads prefab that will start play mode to Scene view instead of starting in Game View"), GUILayout.Height(24));
                if (sceneToggleState != startInSceneView)
                {
                    StartPlayModeInSceneView(sceneToggleState);
                    startInSceneView = sceneToggleState;
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
                if (GUILayout.Button("Install Gogo Loco", GUILayout.Height(24)))
                {
                    if (GogoLocoExist())
                    {
                        PrepGogoLoco(vrcAvatarDescriptor);
                    }
                }
                if (GUILayout.Button("Clear Avatar Blueprint ID", GUILayout.Height(24)))
                {
                    ClearAvatarBlueprintID(vrcAvatar);
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
                //if (GUILayout.Button("Update PhysBones", GUILayout.Height(24)))
                //{
                //    AddPhysBones(breastBoneL);
                //    AddPhysBones(breastBoneR);
                //    AddButtPhysBones(buttBoneL);
                //    AddButtPhysBones(buttBoneR);
                //}
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
                        CreateParameter(vrcAvatarDescriptor, paramName, selectedParamType);
                    }
                    selectedParamType = GUILayout.SelectionGrid(selectedParamType, new string[] { "bool", "int", "float" }, 3, GUILayout.Height(24));
                }
                vrcMenu = (VRCExpressionsMenu)EditorGUILayout.ObjectField(vrcMenu, typeof(VRCExpressionsMenu), true, GUILayout.Height(24));
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
                

            } // Using Disable Scope
            //EditorGUILayout.LabelField("<i> Version " + version + " </i>", new GUIStyle(GUI.skin.label)
            //{
            //    richText = true,
            //    alignment = TextAnchor.MiddleRight
            //});

        } // GUI
    } // Class
} // Namespace