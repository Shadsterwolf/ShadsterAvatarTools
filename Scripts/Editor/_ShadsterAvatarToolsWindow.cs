//Made by Shadsterwolf, some code inspired by the VRCSDK, Av3Creator, and PumpkinTools
using Shadster.AvatarTools;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Shadster.AvatarTools.ShadsterAvatarToolsWindow
{
    [System.Serializable]
    public class _ShadsterAvatarToolsWindow : EditorWindow
    {
        [SerializeField, HideInInspector] static _ShadsterAvatarToolsWindow _tools;

        static EditorWindow toolWindow;
        Vector2 scrollPos;
        private bool startInSceneView;
        private bool useExperimentalPlayMode;
        private bool ignorePhysImmobile;
        private bool testPhysbones;

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

        [SerializeReference] private bool bonesFoldView;
        [SerializeReference] private bool animationFoldView;
        [SerializeReference] private bool texturesFoldView;
        [SerializeReference] private bool scenesFoldView;

        [SerializeReference] private Transform breastBoneL;
        [SerializeReference] private Transform breastBoneR;
        [SerializeReference] private Transform buttBoneL;
        [SerializeReference] private Transform buttBoneR;
        [SerializeReference] private Transform earBoneR;
        [SerializeReference] private Transform earBoneL;
        [SerializeReference] private Transform tailBone;

        public static _ShadsterAvatarToolsWindow ToolsWindow
        {
            get
            {
                if (!_tools)
                    _tools = FindObjectOfType(typeof(_ShadsterAvatarToolsWindow)) as _ShadsterAvatarToolsWindow ?? CreateInstance<_ShadsterAvatarToolsWindow>();
                return _tools;
            }

            private set
            {
                _tools = value;
            }
        }

        [MenuItem("ShadsterWolf/Shadster Tools", false, 0)]
        public static void ShowWindow()
        {
            if (!toolWindow)
            {
                toolWindow = EditorWindow.GetWindow<_ShadsterAvatarToolsWindow>();
                toolWindow.autoRepaintOnSceneChange = true;
                toolWindow.titleContent = new GUIContent("Shadster Tools");
                toolWindow.minSize = new Vector2(500, 800);
            }
            toolWindow.Show();
        }

        private void OnInspectorUpdate()
        {
            if (vrcAvatar != null && vrcAvatarDescriptor == null) //because play mode likes to **** with me and clear the descriptor
                vrcAvatarDescriptor = vrcAvatar.GetComponent<VRCAvatarDescriptor>();
            useExperimentalPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
            startInSceneView = ShadstersAvatarTools.GetStartPlayModeInSceneView();
            ignorePhysImmobile = ShadstersAvatarTools.GetIgnorePhysImmobile();
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
            
            breastBoneL = ShadstersAvatarTools.GetAvatarBone(vrcAvatar, "Breast", "_L");
            breastBoneR = ShadstersAvatarTools.GetAvatarBone(vrcAvatar, "Breast", "_R");
            buttBoneL = ShadstersAvatarTools.GetAvatarBone(vrcAvatar, "Butt", "_L");
            buttBoneR = ShadstersAvatarTools.GetAvatarBone(vrcAvatar, "Butt", "_R");
        }

        public bool Prompt(string banner)
        {
            bool result = EditorUtility.DisplayDialog(banner, "Are you sure?", "Yes", "No");
            if (result)
            {
                return true;
            }
            return false;
        }


        public void DrawBonesWindow()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                Rect subBoxRect = EditorGUILayout.BeginVertical();
                subBoxRect.x += 4;
                subBoxRect.width -= 4;
                subBoxRect.height += 2;
                Color currentColor = GUI.color;
                GUI.color = new Color(0.4f, 1f, 0.4f);
                GUI.Box(subBoxRect, "");

                GUI.color = currentColor;
                bonesFoldView = EditorGUILayout.Foldout(bonesFoldView, "Bones");
                if (bonesFoldView)
                {

                    Color currentBackgroundColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.6f, 1.5f, 0.6f);
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
                        ShadstersAvatarTools.AddPhysBones(breastBoneL);
                        ShadstersAvatarTools.AddPhysBones(breastBoneR);
                        ShadstersAvatarTools.AddButtPhysBones(buttBoneL);
                        ShadstersAvatarTools.AddButtPhysBones(buttBoneR);
                    }
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
                        ShadstersAvatarTools.SetAllGrabMovement(vrcAvatar);
                    }
                    if (GUILayout.Button("Delete End Bones", GUILayout.Height(24)))
                    {
                        ShadstersAvatarTools.DeleteEndBones(vrcAvatar);
                    }
                    if (GUILayout.Button("Repair Missing PhysBone Transforms", GUILayout.Height(24)))
                    {
                        ShadstersAvatarTools.RepairMissingPhysboneTransforms(vrcAvatar);
                    }
                    GUI.backgroundColor = currentBackgroundColor;
                }
                EditorGUILayout.EndVertical();
            }
        }

        public void DrawAnimationFoldout()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                Rect subBoxRect = EditorGUILayout.BeginVertical();
                subBoxRect.x += 4;
                subBoxRect.width -= 4;
                subBoxRect.height += 2;
                Color currentColor = GUI.color;
                GUI.color = new Color(0.4f, 0.4f, 1f);
                GUI.Box(subBoxRect, "");

                GUI.color = currentColor;
                animationFoldView = EditorGUILayout.Foldout(animationFoldView, "Animation");
                if (animationFoldView)
                {

                    Color currentBackgroundColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.6f, 0.6f, 1.6f);


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
                    if (GUILayout.Button("Generate Animation Poi Hues", GUILayout.Height(24)))
                    {
                        //GenerateAnimationShapekeys(vrcAvatar);
                        ShadstersAvatarTools.GenerateAnimationHueShaders(vrcAvatar);
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
                        if (Prompt("Cleanup Unused Generated Animations"))
                        {
                            ShadstersAvatarTools.CleanupUnusedGeneratedAnimations();
                        }
                    }
                    if (GUILayout.Button("Uncheck All Write Defaults states", GUILayout.Height(24)))
                    {
                        if (Prompt("Uncheck All Write Defaults states"))
                        {
                            ShadstersAvatarTools.UncheckAllWriteDefaults(vrcAvatarDescriptor);
                        }
                    }
                    GUI.backgroundColor = currentBackgroundColor;
                }
                EditorGUILayout.EndVertical();
            }
        } //End Animation Foldout

        public void DrawTexturesWindow()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                Rect subBoxRect = EditorGUILayout.BeginVertical();
                subBoxRect.x += 4;
                subBoxRect.width -= 4;
                subBoxRect.height += 2;
                Color currentColor = GUI.color;
                GUI.color = new Color(1f, 0.4f, 0.4f);
                GUI.Box(subBoxRect, "");

                GUI.color = currentColor;
                texturesFoldView = EditorGUILayout.Foldout(texturesFoldView, "Textures");
                if (texturesFoldView)
                {

                    Color currentBackgroundColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(1.6f, 0.6f, 0.6f);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Enable All Mip Maps", GUILayout.Height(24)))
                        {
                            ShadstersAvatarTools.UpdateAvatarTextureMipMaps(vrcAvatar, true);
                        }
                        if (GUILayout.Button("Disable All Mip Maps", GUILayout.Height(24)))
                        {
                            ShadstersAvatarTools.UpdateAvatarTextureMipMaps(vrcAvatar, false);
                        }
                    }
                    using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Set All Max Size 1k", GUILayout.Height(24)))
                        {
                            ShadstersAvatarTools.SetAvatarTexturesMaxSize(vrcAvatar, 1024);
                        }
                        if (GUILayout.Button("Set All Max Size 2k", GUILayout.Height(24)))
                        {
                            ShadstersAvatarTools.SetAvatarTexturesMaxSize(vrcAvatar, 2048);
                        }
                        if (GUILayout.Button("Set All Max Size 4k", GUILayout.Height(24)))
                        {
                            ShadstersAvatarTools.SetAvatarTexturesMaxSize(vrcAvatar, 4096);
                        }
                    }

                    using (var horizontalScope = new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Set Compression LQ", GUILayout.Height(24)))
                        {
                            ShadstersAvatarTools.SetAvatarTexturesCompression(vrcAvatar, TextureImporterCompression.CompressedLQ);
                        }
                        if (GUILayout.Button("Set Compression NQ", GUILayout.Height(24)))
                        {
                            ShadstersAvatarTools.SetAvatarTexturesCompression(vrcAvatar, TextureImporterCompression.Compressed);
                        }
                        if (GUILayout.Button("Set Compression HQ", GUILayout.Height(24)))
                        {
                            ShadstersAvatarTools.SetAvatarTexturesCompression(vrcAvatar, TextureImporterCompression.CompressedHQ);
                        }

                    }
                    GUI.backgroundColor = currentBackgroundColor;
                }
                EditorGUILayout.EndVertical();
            }
        }

        public void DrawScenesWindow()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                Rect subBoxRect = EditorGUILayout.BeginVertical();
                subBoxRect.x += 4;
                subBoxRect.width -= 4;
                subBoxRect.height += 2;
                Color currentColor = GUI.color;
                GUI.color = new Color(0.2f, 0.2f, 0.2f);
                GUI.Box(subBoxRect, "");

                GUI.color = currentColor;
                scenesFoldView = EditorGUILayout.Foldout(scenesFoldView, "Scenes");
                if (scenesFoldView)
                {
                    Color currentBackgroundColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f);

                    string originalScenePath = SceneManager.GetActiveScene().path;
                    string context = originalScenePath.Substring(0, originalScenePath.LastIndexOf("/"));
                    GUIStyle boldCenteredStyle = new GUIStyle(GUI.skin.label);
                    boldCenteredStyle.alignment = TextAnchor.MiddleCenter;
                    boldCenteredStyle.fontStyle = FontStyle.Bold;

                    GUILayout.Label(context, boldCenteredStyle);
                    if (GUILayout.Button("Cleanup All Related Scenes", GUILayout.Height(24)))
                    {
                        if (Prompt("Cleanup All Related Scenes"))
                        {
                            ShadstersAvatarTools.CleanUp();
                        }
                    }
                    if (GUILayout.Button("Cleanup + Fix All Related Scenes", GUILayout.Height(24)))
                    {
                        if (Prompt("Cleanup + Fix All Related Scenes"))
                        {
                            ShadstersAvatarTools.CleanUp(true);
                        }
                    }
                    if (GUILayout.Button("Export Current Context Folder", GUILayout.Height(24)))
                    {
                        ShadstersAvatarTools.Export();
                    }

                    GUI.backgroundColor = currentBackgroundColor;
                }
                EditorGUILayout.EndVertical();
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
                var sceneToggleState = GUILayout.Toggle(startInSceneView, new GUIContent("Start Play Mode in Scene View", "Loads prefab that will start play mode to Scene view instead of starting in Game View"), GUILayout.Height(24), GUILayout.Width(250));
                if (sceneToggleState != startInSceneView)
                {
                    ShadstersAvatarTools.SetStartPlayModeInSceneView(sceneToggleState);
                    startInSceneView = ShadstersAvatarTools.GetStartPlayModeInSceneView();
                }
                var playModeToggleState = GUILayout.Toggle(useExperimentalPlayMode, new GUIContent("Use Experimental Play Mode", "Instantly loads entering play mode, save often and disable if issues occur"), GUILayout.Height(24));
                if (playModeToggleState != useExperimentalPlayMode)
                {
                    ShadstersAvatarTools.UseExperimentalPlayMode(playModeToggleState);
                    useExperimentalPlayMode = playModeToggleState;

                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                var testPhysbonesState = GUILayout.Toggle(testPhysbones, new GUIContent("Test All Avatar Physbones", "When in Play Mode, automatically moves the avatar to check behaviour of physbones"), GUILayout.Height(24), GUILayout.Width(250));
                if (testPhysbonesState != testPhysbones)
                {
                    ShadstersAvatarTools.SetTestPhysbones(testPhysbonesState);
                    testPhysbones = ShadstersAvatarTools.GetTestPhysbones();
                }
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
                if (GUILayout.Button("Fix Avatar Descriptor (Missing Face/Body)", GUILayout.Height(24)))
                {
                    ShadstersAvatarTools.FixAvatarDescriptor(vrcAvatarDescriptor);
                }
                if (GUILayout.Button("Set All Mesh Bounds to 2.5sq", GUILayout.Height(24)))
                {
                    ShadstersAvatarTools.SetAvatarMeshBounds(vrcAvatar);
                }
                if (GUILayout.Button("Set All Anchor Probes to Hip", GUILayout.Height(24)))
                {
                    ShadstersAvatarTools.SetAvatarAnchorProbes(vrcAvatar);
                }                
                if (GUILayout.Button("Clear Avatar Blueprint ID", GUILayout.Height(24)))
                {
                    ShadstersAvatarTools.ClearAvatarBlueprintID(vrcAvatar);
                }
                if (GUILayout.Button("Save Avatar Prefab", GUILayout.Height(24)))
                {
                    ShadstersAvatarTools.SaveAvatarPrefab(vrcAvatar);
                }


                GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3)); // NEW LINE ----------------------
                DrawTexturesWindow();
                GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3)); // NEW LINE ----------------------
                DrawBonesWindow();
                GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3)); // NEW LINE ----------------------
                DrawAnimationFoldout();
                GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3)); // NEW LINE ----------------------

            } // Using Disable Scope
            DrawScenesWindow();

            EditorGUILayout.EndScrollView();
        } // GUI
    }
}

