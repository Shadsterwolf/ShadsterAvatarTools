using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using static Shadster.AvatarTools.AnimatorControl;
using static Shadster.AvatarTools.Params;
using static Shadster.AvatarTools.Menus;
using UnityEditor.Animations;
using Shadster.AvatarTools.VRLabs.AV3Manager;

namespace Shadster.AvatarTools
{
    public class GogoLoco : Editor
    {
        public static bool GogoLocoExist()
        {
            if (!(string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID("Assets/GoGo/GoLoco/Controllers/GoLocoBase.controller"))))
            {
                return true;
            }
            return false;
        }

        public static void SetupGogoLocoLayers(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            vrcAvatarDescriptor.customizeAnimationLayers = true; //ensure customizing playable layers is true
            vrcAvatarDescriptor.autoLocomotion = false; //disable force 6-point tracking

            vrcAvatarDescriptor.baseAnimationLayers[0].isDefault = false; //Base
            vrcAvatarDescriptor.baseAnimationLayers[3].isDefault = false; //Action
            vrcAvatarDescriptor.specialAnimationLayers[0].isDefault = false; //Sitting

            vrcAvatarDescriptor.baseAnimationLayers[0].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoBase.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.baseAnimationLayers[3].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoAction.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.specialAnimationLayers[0].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoSitting.controller", typeof(RuntimeAnimatorController));
            //Debug.Log(AssetDatabase.GetAssetPath(vrcAvatarDescriptor.specialAnimationLayers[0].animatorController));
        }

        public static void SetupGogoBeyondLayers(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            vrcAvatarDescriptor.customizeAnimationLayers = true; //ensure customizing playable layers is true
            vrcAvatarDescriptor.autoLocomotion = false; //disable force 6-point tracking

            vrcAvatarDescriptor.baseAnimationLayers[0].isDefault = false; //Base
            vrcAvatarDescriptor.baseAnimationLayers[1].isDefault = false; //Additive
            vrcAvatarDescriptor.baseAnimationLayers[2].isDefault = false; //Gesture
            vrcAvatarDescriptor.baseAnimationLayers[3].isDefault = false; //Action
            vrcAvatarDescriptor.specialAnimationLayers[0].isDefault = false; //Sitting
            vrcAvatarDescriptor.specialAnimationLayers[1].isDefault = false; //TPose

            vrcAvatarDescriptor.baseAnimationLayers[0].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoBase.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.baseAnimationLayers[1].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoAdditive.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.baseAnimationLayers[2].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoGesture.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.baseAnimationLayers[3].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoAction.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.specialAnimationLayers[0].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoSitting.controller", typeof(RuntimeAnimatorController));
            vrcAvatarDescriptor.specialAnimationLayers[1].animatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Controllers/GoLocoTPose.controller", typeof(RuntimeAnimatorController));
        }

        public static void SetupGogoBeyondPrefab(GameObject vrcAvatar)
        {
            var existingPrefab = vrcAvatar.transform.Find("Beyond Prefab");
            if (existingPrefab != null)
            {
                DestroyImmediate(existingPrefab.gameObject);
            }

            string prefabPath = "Assets/GoGo/GoLoco/Prefabs/Beyond Prefab.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                GameObject ob = GameObject.Instantiate(prefab, vrcAvatar.transform);
                ob.name = "Beyond Prefab";
                Transform flyRotation = ob.transform.Find("Fly/Rotation");
                if (flyRotation != null)
                {
                    RotationConstraint constraint = flyRotation.GetComponent<RotationConstraint>();
                    if (constraint != null)
                    {
                        Transform head = vrcAvatar.transform.Find("Armature/Hips/Spine/Chest/Neck/Head");
                        if (head != null)
                        {
                            ConstraintSource source = new ConstraintSource();
                            source.sourceTransform = head;
                            source.weight = 1.0f;

                            constraint.SetSource(0, source);
                        }
                        else { Debug.LogError("Head not found in the hierarchy."); }
                    }
                    else { Debug.LogError("RotationConstraint component not found on 'Fly/Rotation' GameObject."); }
                }
                else { Debug.LogError("'Fly/Rotation' GameObject not found."); }
                EditorUtility.SetDirty(ob);
                AssetDatabase.Refresh();
            }
            else { Debug.LogError("Prefab not found at path: " + prefabPath); }

        }

        public static void SetupGogoLocoParams(VRCExpressionParameters vrcParameters)
        {
            SetupGogoBeyondParams(vrcParameters);
        }

        public static void SetupGogoBeyondParams(VRCExpressionParameters vrcParameters)
        {
            DeleteVrcParameter(vrcParameters, "Go/JumpAndFall");
            DeleteVrcParameter(vrcParameters, "Go/ScaleFloat");
            DeleteVrcParameter(vrcParameters, "Go/Horizon");
            DeleteVrcParameter(vrcParameters, "Go/ThirdPerson");
            DeleteVrcParameter(vrcParameters, "Go/ThirdPersonMirror");
            CreateVrcParameter(vrcParameters, "VRCEmote", VRCExpressionParameters.ValueType.Int, 0, true);
            CreateVrcParameter(vrcParameters, "Go/Float", VRCExpressionParameters.ValueType.Float, 0f, true);
            CreateVrcParameter(vrcParameters, "Go/PoseRadial", VRCExpressionParameters.ValueType.Float, 0f, false);
            CreateVrcParameter(vrcParameters, "Go/Stationary", VRCExpressionParameters.ValueType.Bool, 0, false);
            CreateVrcParameter(vrcParameters, "Go/Locomotion", VRCExpressionParameters.ValueType.Bool, 0, true);
            CreateVrcParameter(vrcParameters, "Go/Jump&Fall", VRCExpressionParameters.ValueType.Bool, 0, true);
            CreateVrcParameter(vrcParameters, "Go/StandIdle", VRCExpressionParameters.ValueType.Int, 0, true);
            CreateVrcParameter(vrcParameters, "Go/CrouchIdle", VRCExpressionParameters.ValueType.Int, 0, true);
            CreateVrcParameter(vrcParameters, "Go/ProneIdle", VRCExpressionParameters.ValueType.Int, 0, true);
            CreateVrcParameter(vrcParameters, "Go/Swimming", VRCExpressionParameters.ValueType.Bool, 0, false);
            CreateVrcParameter(vrcParameters, "Go/PuppetX", VRCExpressionParameters.ValueType.Float, 0f, false);
            CreateVrcParameter(vrcParameters, "Go/PuppetY", VRCExpressionParameters.ValueType.Float, 0f, false);
            CreateVrcParameter(vrcParameters, "Go/Height", VRCExpressionParameters.ValueType.Float, 0f, true);
        }

        public static void SetupGogoBeyondFX(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            var fx = GetFxController(vrcAvatarDescriptor);
            AnimatorController gogoController = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/GoGo/GoLoco/Controllers/GoLocoFXBeyond.controller");

            CopyControllerParams(gogoController, fx);
            VRLabs.AV3Manager.AnimatorCloner.CopyControllerLayer(gogoController, 1, fx);
            VRLabs.AV3Manager.AnimatorCloner.CopyControllerLayer(gogoController, 2, fx);

        }

        public static void SetupGogoLocoMenu(VRCExpressionsMenu vrcMenu)
        {
            var subMenu = (VRCExpressionsMenu)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/MainMenu/Menu/GoAllMenu.asset", typeof(VRCExpressionsMenu));
            var icon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Icons/icon_Go_Loco.png", typeof(Texture2D));
            CreateMenuControl(vrcMenu, "GoGo Loco Menu", VRCExpressionsMenu.Control.ControlType.SubMenu, "", subMenu, icon);
        }

        public static void SetupGogoBeyondMenu(VRCExpressionsMenu vrcMenu)
        {
            var subMenu = (VRCExpressionsMenu)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/MainMenu/Menu/GoBeyondMenu.asset", typeof(VRCExpressionsMenu));
            var icon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/GoGo/GoLoco/Icons/icon_Go_Loco.png", typeof(Texture2D));
            CreateMenuControl(vrcMenu, "GoGo Loco Menu", VRCExpressionsMenu.Control.ControlType.SubMenu, "", subMenu, icon);
        }
    }
}

