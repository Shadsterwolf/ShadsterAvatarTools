using Shadster.AvatarTools;
using Shadster.AvatarTools.ShadsterAvatarToolsWindow;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

public class _GogoSetupWindow : EditorWindow
{

    [SerializeField, HideInInspector] static _GogoSetupWindow _tools;

    [SerializeReference] private VRCAvatarDescriptor vrcAvatarDescriptor;
    [SerializeReference] private GameObject vrcAvatar;
    [SerializeReference] private VRCExpressionParameters vrcParameters;
    [SerializeReference] private VRCExpressionsMenu vrcMenu;

    static EditorWindow toolWindow;
    Vector2 scrollPos;


    public static _GogoSetupWindow ToolsWindow
    {
        get
        {
            if (!_tools)
                _tools = FindObjectOfType(typeof(_GogoSetupWindow)) as _GogoSetupWindow ?? CreateInstance<_GogoSetupWindow>();
            return _tools;
        }

        private set
        {
            _tools = value;
        }
    }

    [MenuItem("ShadsterWolf/Gogo Setup", false, 0)]
    public static void ShowWindow()
    {
        if (!toolWindow)
        {
            toolWindow = EditorWindow.GetWindow<_GogoSetupWindow>();
            toolWindow.autoRepaintOnSceneChange = true;
            toolWindow.titleContent = new GUIContent("Gogo Setup");
            toolWindow.minSize = new Vector2(500, 800);
        }
        toolWindow.Show();
    }

    public void AutoDetect()
    {
        vrcAvatarDescriptor = ShadstersAvatarTools.SelectCurrentAvatarDescriptor();
        vrcAvatar = vrcAvatarDescriptor.gameObject;
        vrcMenu = vrcAvatarDescriptor.expressionsMenu;
        vrcParameters = vrcAvatarDescriptor.expressionParameters;

    }
    public void ResetAll()
    {
        vrcAvatarDescriptor = null;
        vrcAvatar = null;
        vrcMenu = null;
        vrcParameters = null;
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
        GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3));
            
        using (new EditorGUI.DisabledScope(vrcAvatarDescriptor == null))
        {
            using (new EditorGUI.DisabledScope(!ShadstersAvatarTools.GogoLocoExist()))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Setup Gogo Layers", GUILayout.Height(24)))
                    {
                        ShadstersAvatarTools.SetupGogoLocoLayers(vrcAvatarDescriptor);
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
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Setup Broke Layers", GUILayout.Height(24)))
                    {
                        ShadstersAvatarTools.SetupGogoBrokeLayers(vrcAvatarDescriptor);
                    }
                    if (GUILayout.Button("Add Broke Menu", GUILayout.Height(24)))
                    {
                        ShadstersAvatarTools.SetupGogoBrokeMenu(vrcMenu);
                    }
                    if (GUILayout.Button("Add Broke Params", GUILayout.Height(24)))
                    {
                        ShadstersAvatarTools.SetupGogoBrokeParams(vrcParameters);
                    }

                }
            }
        }
        EditorGUILayout.EndScrollView();

    }
}
