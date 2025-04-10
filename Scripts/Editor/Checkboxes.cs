using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Shadster.AvatarTools
{
    public class Checkboxes : EditorWindow
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
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ShadstersAvatarTools/Prefabs/TestPhysbones.prefab");
            if (flag)
                testPhysbonesPrefab = Instantiate(prefab);
            else
                DestroyImmediate(testPhysbonesPrefab, true);
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
    }
}
