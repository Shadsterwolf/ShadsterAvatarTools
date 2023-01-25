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

        [SerializeReference] private VRCAvatarDescriptor vrcAvatarDescriptor;
        [SerializeReference] private GameObject vrcAvatar;
        [SerializeReference] private VRCExpressionParameters vrcParameters;
        [SerializeReference] private VRCExpressionsMenu vrcMenu;
        [SerializeReference] private AnimatorController vrcFx;

        private List<SkinnedMeshRenderer> _multiList = new List<SkinnedMeshRenderer>();
        private SkinnedMeshRenderer[] _meshRenderers;
        private bool[] _showShapekeys;
        private int[] _selectedKeys;

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
                toolWindow = EditorWindow.GetWindow<_FxSetupWindow>();
                toolWindow.autoRepaintOnSceneChange = true;
                toolWindow.titleContent = new GUIContent("FX Setup");
                toolWindow.minSize = new Vector2(500, 800);
            }
            toolWindow.Show();
        }

        private void OnInspectorUpdate()
        {
            if (vrcAvatar != null && vrcAvatarDescriptor == null) //because play mode likes to **** with me and clear the descriptor
                vrcAvatarDescriptor = vrcAvatar.GetComponent<VRCAvatarDescriptor>();
            if (vrcAvatar == null && vrcAvatarDescriptor == null)
            {
                ResetAll();
            }
        }

        public void AutoDetect()
        {
            vrcAvatarDescriptor = ShadstersAvatarTools.SelectCurrentAvatarDescriptor();
            vrcAvatar = vrcAvatarDescriptor.gameObject;
            vrcMenu = vrcAvatarDescriptor.expressionsMenu;
            vrcParameters = vrcAvatarDescriptor.expressionParameters;
            vrcFx = ShadstersAvatarTools.GetFxController(vrcAvatarDescriptor);

            _meshRenderers = vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);
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
            _multiList.Clear();
        }

        public void DrawFxLayers()
        {
            vrcFx = ShadstersAvatarTools.GetFxController(vrcAvatarDescriptor);
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
                    var clips = ShadstersAvatarTools.GenerateAnimationToggle(_meshRenderers[i], vrcAvatar);
                    //Debug.Log(clips[0] + " " + clips[0].GetInstanceID());
                    //Debug.Log(clips[1]);
                    //Debug.Log(_meshRenderers[i].name + " = " + _meshRenderers[i].enabled);
                    if (_meshRenderers[i].enabled && _meshRenderers[i].gameObject.activeInHierarchy)
                    {
                        ShadstersAvatarTools.CreateToggle(vrcAvatarDescriptor, _meshRenderers[i].name + " Toggle", _meshRenderers[i].name, clips[1], clips[0]);
                    }
                    else
                    {
                        ShadstersAvatarTools.CreateToggle(vrcAvatarDescriptor, _meshRenderers[i].name + " Toggle", _meshRenderers[i].name, clips[0], clips[1]);
                    }
                    ShadstersAvatarTools.CreateMenuControl(vrcAvatarDescriptor, _meshRenderers[i].name + " Toggle", VRCExpressionsMenu.Control.ControlType.Toggle, _meshRenderers[i].name);

                    break;
                }
                if (GUILayout.Button("Multi +", GUILayout.Width(110)))
                {
                    _multiList.Add(_meshRenderers[i]);
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

        public static void DrawMenuItems(VRCExpressionsMenu vrcMenu, VRCExpressionParameters vrcParameters)
        {
            for (int i = 0; i < vrcMenu.controls.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(vrcMenu.controls[i].name);
                EditorGUILayout.LabelField(vrcMenu.controls[i].type.ToString(), GUILayout.Width(110));
                EditorGUILayout.LabelField(vrcMenu.controls[i].parameter.name, GUILayout.Width(110));
                if (GUILayout.Button("Delete"))
                {
                    ShadstersAvatarTools.DeleteVrcParameter(vrcParameters, vrcMenu.controls[i].parameter.name);
                    vrcMenu.controls.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        public void DrawFxMulti()
        {
            if (_multiList != null)
            {
                for (int i = 0; i < _multiList.Count; i++)
                {
                    var multiMesh = _multiList[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(multiMesh.name);
                    if (GUILayout.Button("Remove"))
                    {
                        _multiList.RemoveAt(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                using (new EditorGUI.DisabledScope(_multiList.Count < 2))
                {
                    GUILayout.Button("Create ");
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
                vrcFx = (AnimatorController)EditorGUILayout.ObjectField(vrcFx, typeof(AnimatorController), true, GUILayout.Height(24));
            }
            
            if (vrcAvatar != null && vrcFx != null)
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
            using (new EditorGUILayout.HorizontalScope())
            {
                vrcMenu = (VRCExpressionsMenu)EditorGUILayout.ObjectField(vrcMenu, typeof(VRCExpressionsMenu), true, GUILayout.Height(24));
                vrcParameters = (VRCExpressionParameters)EditorGUILayout.ObjectField(vrcParameters, typeof(VRCExpressionParameters), true, GUILayout.Height(24));
            }
            if (vrcMenu != null)
            {
                DrawMenuItems(vrcMenu, vrcParameters);
            }
            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3));
            if (vrcAvatar != null)
            {
                DrawFxObjects();
            }
            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3));
            if (vrcAvatar != null)
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

