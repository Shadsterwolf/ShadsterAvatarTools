using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
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

        static EditorWindow _toolsWindow;
        private bool startInSceneView = false;

        [SerializeReference] private VRCAvatarDescriptor vrcAvatarDescriptor;
        [SerializeReference] private GameObject vrcAvatar;
        [SerializeReference] private string currentPath;

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

        [MenuItem("ShadsterWolf/Show Avatar Tools", false, 0)]
        public static void ShowWindow()
        {
            if (!_toolsWindow)
            {
                _toolsWindow = EditorWindow.GetWindow(typeof(_ShadstersAvatarToolsWindow));
                _toolsWindow.autoRepaintOnSceneChange = true;
                _toolsWindow.titleContent = new GUIContent("ShadsterTools");
            }
            _toolsWindow.Show();
        }

        public static VRCAvatarDescriptor SelectCurrentAvatarDescriptor()
        {
            VRCAvatarDescriptor vrcAvatarDescriptor = null;
            //Get current selected avatar
            if (Selection.activeTransform && Selection.activeTransform.root.gameObject.GetComponent<VRCAvatarDescriptor>() != null)
            {
                vrcAvatarDescriptor = Selection.activeTransform.root.GetComponent<VRCAvatarDescriptor>();
                if (vrcAvatarDescriptor != null)
                    return vrcAvatarDescriptor;
            }
            //Find first potential avatar
            var potentialObjects = Object.FindObjectsOfType<VRCAvatarDescriptor>().ToArray();
            if (potentialObjects.Length > 0)
            {
                vrcAvatarDescriptor = potentialObjects.First();
            }

            return vrcAvatarDescriptor;
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
                smr.localBounds = bounds;
            }
        }
        private static void OverrideAvatarAnchorProbes(GameObject vrcAvatar)
        {

            foreach (SkinnedMeshRenderer smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                smr.probeAnchor = GetAvatarArmature(vrcAvatar).Find("Hips");
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

        private static void DeleteEndBones(GameObject vrcAvatar)
        {
            Transform armature = GetAvatarArmature(vrcAvatar);
            if (armature != null)
            {
                foreach (Transform bone in armature.GetComponentsInChildren<Transform>(true))
                {
                    if (bone.name.EndsWith("_end"))
                    {
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

        public static List<Material> GetObjectMaterials(GameObject go)
        {
            List<Material> goMaterials = go.GetComponent<Renderer>().materials.ToList();
            return goMaterials;
        }

        public static List<Texture> GetObjectTextures(GameObject go)
        {
            List<Texture> goTextures = new List<Texture>();
            Renderer goRender = go.GetComponent<Renderer>();
            Shader shader = goRender.sharedMaterial.shader;
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    Texture texture = goRender.sharedMaterial.GetTexture(ShaderUtil.GetPropertyName(shader, i));
                    //Debug.Log(texture.ToString());
                    goTextures.Add(texture);
                }
            }
            Debug.Log(goTextures);
            return goTextures;

        }

        private static void UncheckAvatarTextureMipMaps(GameObject vrcAvatar)
        {
            List<Texture> aTextures = new List<Texture>();
            foreach (Renderer render in vrcAvatar.GetComponentsInChildren<Renderer>(true))
            {
                Debug.Log(render.gameObject);
                aTextures.AddRange(GetObjectTextures(render.gameObject));
            }
            aTextures = aTextures.Distinct().ToList();
            TextureImporter importer = new TextureImporter();
            foreach (Texture tex in aTextures)
            {
                Debug.Log(tex.name);
                importer = AssetDatabase.LoadAssetAtPath<TextureImporter>(tex.name);
                importer.mipmapEnabled = false;
            }
        }

        public void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                vrcAvatarDescriptor = (VRCAvatarDescriptor)EditorGUILayout.ObjectField(vrcAvatarDescriptor, typeof(VRCAvatarDescriptor), true, GUILayout.Height(24));

                if (GUILayout.Button("Auto-Detect", GUILayout.Height(24)))
                {
                    vrcAvatarDescriptor = SelectCurrentAvatarDescriptor();
                    vrcAvatar = vrcAvatarDescriptor.gameObject;
                    breastBoneL = GetAvatarBone(vrcAvatar, "Breast", "_L");
                    breastBoneR = GetAvatarBone(vrcAvatar, "Breast", "_R");
                    buttBoneL = GetAvatarBone(vrcAvatar, "Butt", "_L");
                    buttBoneR = GetAvatarBone(vrcAvatar, "Butt", "_R");
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                vrcAvatar = (GameObject)EditorGUILayout.ObjectField(vrcAvatar, typeof(GameObject), true, GUILayout.Height(24));


                if (GUILayout.Button("Reset-All", GUILayout.Height(24)))
                {
                    vrcAvatarDescriptor = null;
                    vrcAvatar = null;
                    breastBoneL = null;
                    breastBoneR = null;
                    buttBoneL = null;
                    buttBoneR = null;
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
            if (GUILayout.Button("Override Mesh Bounds", GUILayout.Height(24)))
            {
                OverrideAvatarBounds(vrcAvatar);
            }
            if (GUILayout.Button("Override Anchor Probes", GUILayout.Height(24)))
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
            if (GUILayout.Button("Uncheck Texture Mip Maps", GUILayout.Height(24)))
            {
                UncheckAvatarTextureMipMaps(vrcAvatar);
            }
            startInSceneView = GUILayout.Toggle(startInSceneView, "Start play mode in Scene view", GUILayout.Height(24));
            if (startInSceneView)
            {
                SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
            }

            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(3));

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
        }
    }
}