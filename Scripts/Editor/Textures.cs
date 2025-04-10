using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Shadster.AvatarTools
{
    public class Textures
    {
        public static List<UnityEngine.Object> GetAvatarTextures(GameObject vrcAvatar)
        {
            List<UnityEngine.Object> aTextures = new List<UnityEngine.Object>();
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

        public static void UpdateAvatarTextureMipMaps(GameObject vrcAvatar, bool mipMapStatus)
        {
            List<string> paths = new List<string>();
            List<UnityEngine.Object> aTextures = GetAvatarTextures(vrcAvatar);
            if (aTextures.Count > 0)
            {
                foreach (UnityEngine.Object o in aTextures)
                {
                    TextureImporter t = (TextureImporter)o;
                    if (t.mipmapEnabled != mipMapStatus)
                    {
                        Undo.RecordObject(t, mipMapStatus ? "Generate Mip Maps" : "Un-Generate Mip Maps");
                        t.mipmapEnabled = mipMapStatus;
                        t.streamingMipmaps = mipMapStatus;
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

        public static void SetAvatarTexturesMaxSize(GameObject vrcAvatar, int maxSize)
        {
            List<string> paths = new List<string>();
            List<UnityEngine.Object> aTextures = GetAvatarTextures(vrcAvatar);
            if (aTextures.Count > 0)
            {
                //Debug.Log(aTextures.Count);
                foreach (UnityEngine.Object o in aTextures)
                {
                    //Debug.Log(o);
                    TextureImporter t = (TextureImporter)o;
                    if (t.maxTextureSize != maxSize)
                    {
                        Undo.RecordObject(t, "Set Textures size to " + maxSize.ToString());
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

        public static void SetAvatarTexturesCompression(GameObject vrcAvatar, TextureImporterCompression compressionType)
        {
            List<string> paths = new List<string>();
            List<UnityEngine.Object> aTextures = GetAvatarTextures(vrcAvatar);
            if (aTextures.Count > 0)
            {
                //Debug.Log(aTextures.Count);
                foreach (UnityEngine.Object o in aTextures)
                {
                    //Debug.Log(o);
                    TextureImporter t = (TextureImporter)o;
                    if (t.textureCompression != compressionType)
                    {
                        Undo.RecordObject(t, "Set Avatar compression");
                        t.textureCompression = compressionType;
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
    }
}