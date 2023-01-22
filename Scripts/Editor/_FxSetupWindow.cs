using Shadster.AvatarTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Shadster.AvatarTools.FxSetup
{
    [System.Serializable]
    public class _FxSetupWindow : EditorWindow
    {
        [SerializeField, HideInInspector] static _FxSetupWindow _tools;

        static EditorWindow toolWindow;
        Vector2 scrollPos;
        private bool startInSceneView;
        private bool useExperimentalPlayMode;

        [SerializeReference] private VRCAvatarDescriptor vrcAvatarDescriptor;
        [SerializeReference] private GameObject vrcAvatar;
        [SerializeReference] private VRCExpressionParameters vrcParameters;
        [SerializeReference] private VRCExpressionsMenu vrcMenu;
        [SerializeReference] private AnimatorController vrcFx;

        private List<SkinnedMeshRenderer> _multiToggles;
        private SkinnedMeshRenderer[] _meshRenderers;
        private bool[] _showShapekeys;
        private int[] _selectedKeys;

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

        public static _FxSetupWindow ToolsWindow
        {
            get
            {
                if (!_tools)
                    _tools = FindObjectOfType(typeof(_FxSetupWindow)) as _FxSetupWindow ?? CreateInstance<_FxSetupWindow>();
                return _tools;
            }

            private set
            {
                _tools = value;
            }
        }
        [MenuItem("ShadsterWolf/FX Setup", false, 0)]
        public static void ShowWindow()
        {
            if (!toolWindow)
            {
                toolWindow = EditorWindow.GetWindow(typeof(_FxSetupWindow));
                toolWindow.autoRepaintOnSceneChange = true;
                toolWindow.titleContent = new GUIContent("FX Setup");
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
            vrcFx = ShadstersAvatarTools.GetFxController(vrcAvatarDescriptor);

            _meshRenderers = vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (_showShapekeys == null || _showShapekeys.Length != _meshRenderers.Length)
            {
                _showShapekeys = new bool[_meshRenderers.Length];
                _selectedKeys = new int[_meshRenderers.Length];
            }
        }

        public void ResetAll()
        {
            vrcAvatarDescriptor = null;
            vrcAvatar = null;
            vrcMenu = null;
            vrcParameters = null;
            vrcFx = null;
            _multiToggles.Clear();
        }

        public void DrawFxLayers()
        {
            for (int i = 0; i < vrcFx.layers.Length; i++)
            {
                var layer = vrcFx.layers[i];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(layer.name);
                if (GUILayout.Button("Delete"))
                {
                    vrcFx.RemoveLayer(i);
                    //vrcFx.AddLayer("New Layer");
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        public void DrawFxObjects()
        {
            for (int i = 0; i < _meshRenderers.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _showShapekeys[i] = EditorGUILayout.Foldout(_showShapekeys[i], _meshRenderers[i].name);
                if (GUILayout.Button("Add Toggle", GUILayout.Width(110)))
                {
                    vrcFx.AddLayer(_meshRenderers[i].name + " Toggle");
                    break;
                }
                if (GUILayout.Button("Multi +", GUILayout.Width(110)))
                {
                    _multiToggles.Add(_meshRenderers[i]);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                if (_showShapekeys[i])
                {
                    EditorGUILayout.BeginHorizontal();
                    var shapeKeyDropdown = new string[0];
                    if (_meshRenderers[i].GetComponent<SkinnedMeshRenderer>() != null)
                    {
                        var mesh = _meshRenderers[i].GetComponent<SkinnedMeshRenderer>().sharedMesh;
                        if (mesh.blendShapeCount > 0)
                        {
                            shapeKeyDropdown = new string[mesh.blendShapeCount];
                            for (int j = 0; j < mesh.blendShapeCount; j++)
                            {
                                shapeKeyDropdown[j] = mesh.GetBlendShapeName(j);
                            }
                        }
                    }
                    if (shapeKeyDropdown.Length > 0)
                    {
                        _selectedKeys[i] = EditorGUILayout.Popup("Shape keys", _selectedKeys[i], shapeKeyDropdown);
                        if (GUILayout.Button("Add Toggle", GUILayout.Width(110)))
                        {
                            vrcFx.AddLayer(shapeKeyDropdown[_selectedKeys[i]] + " Toggle");
                            break;
                        }
                        if (GUILayout.Button("Add Slider", GUILayout.Width(110)))
                        {
                            vrcFx.AddLayer(shapeKeyDropdown[_selectedKeys[i]] + " Slider");
                            break;
                        }
                        //EditorGUILayout.LabelField("Value: " + _meshRenderers[i].GetComponent<SkinnedMeshRenderer>().GetBlendShapeWeight(selectedKey));
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(24);
                }
            }
        }

        public void DrawFxMulti()
        {
            for (int i = 0; i < _multiToggles.Count; i++)
            {
                var multiMesh = _multiToggles[i];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(multiMesh.name);
                EditorGUILayout.EndHorizontal();
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
                vrcFx = (AnimatorController)EditorGUILayout.ObjectField(vrcFx, typeof(AnimatorController), true, GUILayout.Height(24));
                vrcMenu = (VRCExpressionsMenu)EditorGUILayout.ObjectField(vrcMenu, typeof(VRCExpressionsMenu), true, GUILayout.Height(24));
                vrcParameters = (VRCExpressionParameters)EditorGUILayout.ObjectField(vrcParameters, typeof(VRCExpressionParameters), true, GUILayout.Height(24));
            }
            
            if (vrcFx != null)
            {
                if (GUILayout.Button("BackUp Current FX Controller", GUILayout.Height(24)))
                {
                    ShadstersAvatarTools.BackupController(vrcFx);
                }
                if (vrcFx.layers.Length > 0)
                {
                    DrawFxLayers();
                }
            }
            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3));
            if (vrcAvatar != null)
            {
                DrawFxObjects();
            }
            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3));
            if (vrcAvatar != null && _multiToggles.Count != 0)
            {
                DrawFxMulti();
            }
            //for (int i = 0; i < _meshRenderers.Length; i++)
            //{
            //    EditorGUILayout.BeginHorizontal();
            //    EditorGUILayout.LabelField(_meshRenderers[i].name);
            //    var shapeKeyDropdown = new string[0];
            //    if (_meshRenderers[i].GetComponent<SkinnedMeshRenderer>() != null)
            //    {
            //        var mesh = _meshRenderers[i].GetComponent<SkinnedMeshRenderer>().sharedMesh;
            //        if (mesh.blendShapeCount > 0)
            //        {
            //            shapeKeyDropdown = new string[mesh.blendShapeCount];
            //            for (int j = 0; j < mesh.blendShapeCount; j++)
            //            {
            //                shapeKeyDropdown[j] = mesh.GetBlendShapeName(j);
            //            }
            //        }
            //    }
            //    if (shapeKeyDropdown.Length > 0)
            //    {
            //        var selectedKey = EditorGUILayout.Popup("Shape keys", 0, shapeKeyDropdown);
            //        //EditorGUILayout.LabelField("Value: " + _meshRenderers[i].GetComponent<SkinnedMeshRenderer>().GetBlendShapeWeight(selectedKey));
            //    }
            //    EditorGUILayout.EndHorizontal();
            //}
            //foreach (var m in mRenders)
            //{
            //    EditorGUILayout.BeginHorizontal();
            //    EditorGUILayout.LabelField(m.name);
            //    if (GUILayout.Button("Add Toggle"))
            //    { 

            //    }
            //    EditorGUILayout.EndHorizontal();
            //}
            EditorGUILayout.EndScrollView();
        } //End GUI
    }
}

