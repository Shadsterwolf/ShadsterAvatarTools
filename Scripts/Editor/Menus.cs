using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using System.IO;
using static Shadster.AvatarTools.Helper;

namespace Shadster.AvatarTools
{
    public class Menus
    {
        public static VRCExpressionsMenu CreateNewMenu(string menuName)
        {
            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            string saveFolder = GetCurrentSceneRootPath() + "/" + "Menus";
            if (!(AssetDatabase.IsValidFolder(saveFolder)))
                Directory.CreateDirectory(saveFolder);
            string savePath = GetCurrentSceneRootPath() + "/" + "Menus" + "/" + menuName + ".asset";
            menu.name = menuName;
            CreateOrReplaceAsset<VRCExpressionsMenu>(menu, savePath);
            menu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(savePath);
            return menu;
        }

        public static void CreateMenuControl(VRCExpressionsMenu vrcMenu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName)
        {
            CreateMenuControl(vrcMenu, controlName, controlType, paramName, null, null, 1);
        }
        public static void CreateMenuControl(VRCExpressionsMenu vrcMenu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName, int value)
        {
            CreateMenuControl(vrcMenu, controlName, controlType, paramName, null, null, value);
        }
        public static void CreateMenuControl(VRCExpressionsMenu vrcMenu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName, Texture2D icon)
        {
            CreateMenuControl(vrcMenu, controlName, controlType, paramName, null, icon, 1);
        }
        public static void CreateMenuControl(VRCExpressionsMenu vrcMenu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName, VRCExpressionsMenu subMenu, Texture2D icon)
        {
            CreateMenuControl(vrcMenu, controlName, controlType, paramName, subMenu, icon, 1);
        }

        public static void CreateMenuControl(VRCExpressionsMenu menu, string controlName, int controlType, string paramName)
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
        public static void CreateMenuControl(VRCAvatarDescriptor vrcAvatarDescriptor, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName)
        {
            //var param = vrcAvatarDescriptor.expressionParameters;
            var vrcMenu = vrcAvatarDescriptor.expressionsMenu;
            CreateMenuControl(vrcMenu, controlName, controlType, paramName);
        }

        public static void CreateMenuControl(VRCExpressionsMenu vrcMenu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, VRCExpressionsMenu subMenu)
        {
            CreateMenuControl(vrcMenu, controlName, controlType, "", subMenu, (Texture2D)AssetDatabase.LoadAssetAtPath(("Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Expressions Menu/Icons/item_folder.png"), typeof(Texture2D)), 1);
        }

        public static void CreateMenuControl(VRCExpressionsMenu vrcMenu, string controlName, VRCExpressionsMenu.Control.ControlType controlType, string paramName, VRCExpressionsMenu subMenu, Texture2D icon, int value)
        {
            foreach (var control in vrcMenu.controls)
            {
                if (control.name.Equals(controlName))
                {
                    vrcMenu.controls.Remove(control);
                    break;
                }
            }
            if (vrcMenu.controls.Count == 8)
            {
                EditorUtility.DisplayDialog("Menu control full! (8 Max)", "Free up menu and/or make a new one as a submenu", "Ok");
                return;
            }
            var item = new VRCExpressionsMenu.Control
            {
                name = controlName,
                type = controlType,
                value = value
            };
            if (controlType == VRCExpressionsMenu.Control.ControlType.RadialPuppet)
            {
                item.subParameters = new VRCExpressionsMenu.Control.Parameter[]
                { new  VRCExpressionsMenu.Control.Parameter {
                    name = paramName
                }};
            }
            else if (controlType == VRCExpressionsMenu.Control.ControlType.SubMenu)
            {
                item.subMenu = subMenu;
            }
            else
            {
                item.parameter = new VRCExpressionsMenu.Control.Parameter
                {
                    name = paramName
                };
            }
            if (icon != null)
            {
                item.icon = icon;
            }

            vrcMenu.controls.Add(item);
            EditorUtility.SetDirty(vrcMenu);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
}
