using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEditor.Animations;
using static Shadster.AvatarTools.Helper;
using static Shadster.AvatarTools.AnimatorControl;

namespace Shadster.AvatarTools
{
    public class Setup
    {
        public static void SetupVRCMenus()
        {
            string samplesPath = GetPackageSamplesPath();
            string scenePath = GetCurrentScenePath();

            string paramAsset = samplesPath + "/AV3 Demo Assets/Expressions Menu/DefaultExpressionParameters.asset";
            string menuAsset = samplesPath + "/AV3 Demo Assets/Expressions Menu/DefaultExpressionsMenu.asset";

            var paramPath = DuplicateFile(paramAsset, scenePath + "/Menus", "Params.asset");
            var menuPath = DuplicateFile(menuAsset, scenePath + "/Menus", "Menu_Main.asset");

            var vrcAvatarDescriptor = SelectCurrentAvatarDescriptor();
            if (vrcAvatarDescriptor != null) //If we have an avatar that exists with a descriptor, lets plug the controller in
            {
                vrcAvatarDescriptor.customExpressions = true;
                vrcAvatarDescriptor.expressionsMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(menuPath);
                vrcAvatarDescriptor.expressionParameters = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(paramPath);
                AssetDatabase.Refresh();
            }


        }

        public static void SetupVRCController()
        {
            string samplesPath = GetPackageSamplesPath();
            string scenePath = GetCurrentScenePath();

            string menuAsset = samplesPath + "/AV3 Demo Assets/Animation/Controllers/vrc_AvatarV3HandsLayer2.controller";

            var fxPath = DuplicateFile(menuAsset, scenePath, "FX.controller");
            var vrcAvatarDescriptor = SelectCurrentAvatarDescriptor();
            if (GetFxController(vrcAvatarDescriptor) == null) //If we have an avatar descriptor that exists without the controller, lets plug the controller in
            {
                vrcAvatarDescriptor.customizeAnimationLayers = true; //ensure customizing playable layers is true
                //vrcAvatarDescriptor.baseAnimationLayers[4].isEnabled = true;
                vrcAvatarDescriptor.baseAnimationLayers[4].isDefault = false;
                vrcAvatarDescriptor.baseAnimationLayers[4].animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(fxPath);

            }
        }
    }
}