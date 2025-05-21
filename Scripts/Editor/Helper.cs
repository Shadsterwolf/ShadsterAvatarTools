using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using VRC.SDK3.Avatars.Components;
using UnityEngine.SceneManagement;

namespace Shadster.AvatarTools
{
    public class Helper
    {
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

        public static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            var type = original.GetType();
            var copy = destination.AddComponent(type);
            var fields = type.GetFields();
            foreach (var field in fields) field.SetValue(copy, field.GetValue(original));
            return copy as T;
        }

        public static string GetPackageSamplesPath()
        {
            string packageSamplesPath = "Packages/com.vrchat.avatars/Samples";
            return packageSamplesPath;
        }

        public static string GetAssetsFolderPath()
        {
            string assetsFolderPath = Application.dataPath;
            assetsFolderPath = assetsFolderPath.Substring(0, assetsFolderPath.Length - "Assets".Length);
            return assetsFolderPath;
        }

        public static string GetCurrentScenePath()
        {
            string scenePath = EditorSceneManager.GetActiveScene().path;

            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError("No active scene or scene not saved. Unable to get scene folder.");
                return null;
            }

            // Get the directory of the scene path
            string sceneFolder = Path.GetDirectoryName(scenePath);

            return sceneFolder;
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

        public static ModelImporter GetModelImporterForObject(GameObject obj)
        {
            // Get the Animator component attached to the humanoid GameObject
            Animator animator = obj.GetComponent<Animator>();

            if (animator != null && animator.avatar != null && animator.avatar.isHuman)
            {
                // Get the path of the asset associated with the GameObject
                string assetPath = AssetDatabase.GetAssetPath(animator.avatar);

                // Check if the GameObject is associated with a model asset
                if (!string.IsNullOrEmpty(assetPath))
                {
                    // Get the ModelImporter for the asset
                    return AssetImporter.GetAtPath(assetPath) as ModelImporter;
                }
                else
                {
                    Debug.LogWarning("The humanoid GameObject is not associated with a model asset.");
                }
            }
            else
            {
                Debug.LogWarning("The GameObject is not a humanoid rig or Animator component is missing.");
            }

            return null;
        }

        public static bool IsHumanoidRig(GameObject obj)
        {
            Animator animator = obj.GetComponent<Animator>();

            if (animator != null && animator.avatar != null)
            {
                if (animator.avatar.isHuman)
                {
                    // The model has a humanoid rig
                    return true;
                }
                else
                {
                    // The model does not have a humanoid rig
                    return false;
                }
            }
            else
            {
                Debug.LogWarning("Animator or Object missing");
                return false;
            }
        }

        public static string DuplicateFile(string sourceFile, string destinationFolderPath, string newFileName)
        {
            if (!File.Exists(sourceFile))
            {
                Debug.LogError("Source file does not exist!");
                return null;
            }

            // Check if destination folder exists, if not create it
            if (!Directory.Exists(destinationFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(destinationFolderPath);
                }
                catch (IOException e)
                {
                    Debug.LogError("Error creating destination folder: " + e.Message);
                    return null;
                }
            }

            string copiedFile = Path.Combine(destinationFolderPath, newFileName);
            // Check if the file already exists in the destination folder
            if (File.Exists(copiedFile))
            {
                Debug.LogWarning("File already exists in the destination folder. Cancelling...");
                return null;
            }
            try
            {
                // Copy the file to the destination folder with the new name
                string currentAssetName = Path.GetFileNameWithoutExtension(sourceFile);
                string destinationAssetPath = Path.Combine(destinationFolderPath, newFileName);
                AssetDatabase.CopyAsset(sourceFile, destinationAssetPath);
                AssetDatabase.RenameAsset(destinationAssetPath, newFileName);
                AssetDatabase.Refresh();
                return destinationAssetPath;
            }
            catch (IOException e)
            {
                Debug.LogError("Error duplicating file: " + e.Message);
                return null;
            }

        }

        public static void DeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    if (File.Exists(path + ".meta"))
                    {
                        File.Delete(path + ".meta");
                    }
                    Debug.Log("File deleted successfully: " + path);
                }
                else
                {
                    Debug.LogWarning("File not found: " + path);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error deleting file: " + e.Message);
            }
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

        public static string GetCurrentSceneRootPath()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            string scenePath = currentScene.path;
            string currentPath = Path.GetDirectoryName(scenePath);
            currentPath = currentPath.Replace("\\", "/"); //I am suffering
            return currentPath;
        }

        public static List<GameObject> GetRenderersInChildren(GameObject parent)
        {
            if (parent == null) return null;

            var result = new List<GameObject>();
            var renderers = parent.GetComponentsInChildren<Renderer>(true);

            foreach (var renderer in renderers)
            {
                result.Add(renderer.gameObject);
                Debug.Log("GetRenderersInChildren: " + renderer.gameObject.name);
            }
            return result;
        }

        public static string RemoveQuotes(string input)
        {
            return input.Replace("\"", "").Replace("'", "");
        }

    }

}

