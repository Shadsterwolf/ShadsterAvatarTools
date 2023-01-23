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
        Vector2 scrollPos;
        private bool startInSceneView;
        private bool useExperimentalPlayMode;
        private bool ignorePhysImmobile;

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
            ignorePhysImmobile = ShadstersAvatarTools.GetIgnorePhysImmobile();
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
            vrcAvatarDescriptor = ShadstersAvatarTools.SelectCurrentAvatarDescriptor();
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
                r.probeAnchor = vrcAvatar.transform.Find("Armature").Find("Hips");
            }
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

        public static List<VRC_PhysBone> GetAllAvatarPhysBones(GameObject vrcAvatar)
        {
            Transform armature = vrcAvatar.transform.Find("Armature");
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

        public void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));
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
            using (new EditorGUILayout.HorizontalScope())
            {
                var ignorePhysToggleState = GUILayout.Toggle(ignorePhysImmobile, new GUIContent("Ignore Physbone Immobile World", "When in Play Mode, updates all physbones with Immobile World Type to zero"), GUILayout.Height(24));
                if (ignorePhysToggleState != ignorePhysImmobile)
                {
                    ShadstersAvatarTools.SetIgnorePhysImmobile(ignorePhysToggleState);
                    ignorePhysImmobile = ShadstersAvatarTools.GetIgnorePhysImmobile();
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
                        ShadstersAvatarTools.SetupGogoLocoMenu(vrcMenu);
                    }
                    if (GUILayout.Button("Add Gogo Params", GUILayout.Height(24)))
                    {
                        ShadstersAvatarTools.SetupGogoLocoParams(vrcParameters);
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
                        ShadstersAvatarTools.MovePhysBonesFromArmature(vrcAvatar);
                    }
                    if (GUILayout.Button("Move Colliders from Armature", GUILayout.Height(24)))
                    {
                        ShadstersAvatarTools.MovePhysCollidersFromArmature(vrcAvatar);
                    }
                }
                if (GUILayout.Button("Set All Grab Movement to 1", GUILayout.Height(24)))
                {
                    SetAllGrabMovement(vrcAvatar);
                }

                GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3)); // NEW LINE ----------------------

                if (GUILayout.Button("Generate Animation Render Toggles", GUILayout.Height(24)))
                {
                    ShadstersAvatarTools.GenerateAnimationRenderToggles(vrcAvatar);
                }
                if (GUILayout.Button("Generate Animation Shapekeys", GUILayout.Height(24)))
                {
                    //GenerateAnimationShapekeys(vrcAvatar);
                    ShadstersAvatarTools.CombineAnimationShapekeys(vrcAvatar);
                    ShadstersAvatarTools.CombineEmoteShapekeys(vrcAvatar);
                }
                if (GUILayout.Button("Generate Emote Override Menu", GUILayout.Height(24)))
                {
                    ShadstersAvatarTools.GenerateEmoteOverrideMenu(vrcAvatarDescriptor);
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
                    ShadstersAvatarTools.CreateToggle(vrcAvatarDescriptor, layerName, paramName, clipA, clipB);
                }
                if (GUILayout.Button("Create/Overwrite BlendTree FX Layer (float)", GUILayout.Height(24)))
                {
                    ShadstersAvatarTools.CreateBlendTree(vrcAvatarDescriptor, layerName, paramName, clipA, clipB);
                }

                using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Create/Overwrite Parameter", GUILayout.Height(24)))
                    {
                        ShadstersAvatarTools.CreateFxParameter(vrcAvatarDescriptor, paramName, selectedParamType);
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
                    ShadstersAvatarTools.CreateMenuControl(vrcMenu, menuControlName, selectedControlType, paramName);
                }
                if (GUILayout.Button("Cleanup Unused Generated Animations", GUILayout.Height(24)))
                {
                    ShadstersAvatarTools.CleanupUnusedGeneratedAnimations();
                }
                if (GUILayout.Button("Cleanup Avatar & Export from current scene", GUILayout.Height(24)))
                {
                    ShadstersAvatarTools.Export();
                }
                

            } // Using Disable Scope
            //EditorGUILayout.LabelField("<i> Version " + version + " </i>", new GUIStyle(GUI.skin.label)
            //{
            //    richText = true,
            //    alignment = TextAnchor.MiddleRight
            //});
            EditorGUILayout.EndScrollView();
        } // GUI
    } // Class
} // Namespace