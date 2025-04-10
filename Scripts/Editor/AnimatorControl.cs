using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEditor.Animations;
using System.Linq;
using System.IO;
using static Shadster.AvatarTools.Params;

namespace Shadster.AvatarTools
{
    public class AnimatorControl : Editor
    {
        public static AnimatorController GetFxController(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            AnimatorController runtime = vrcAvatarDescriptor.baseAnimationLayers[4].animatorController as AnimatorController;
            return runtime;

        }

        public static void CreateFxParameter(VRCAvatarDescriptor vrcAvatarDescriptor, string paramName, int dataType)
        {
            if (dataType == 1)
                CreateFxParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Int);
            else if (dataType == 2)
                CreateFxParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Float);
            else
                CreateFxParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Bool);
        }

        public static void CreateFxParameter(VRCAvatarDescriptor vrcAvatarDescriptor, string paramName, AnimatorControllerParameterType dataType)
        {
            CreateFxParameter(vrcAvatarDescriptor, paramName, dataType, 0);
        }
        public static void CreateFxParameter(VRCAvatarDescriptor vrcAvatarDescriptor, string paramName, AnimatorControllerParameterType dataType, float defaultValue)
        {
            var fx = GetFxController(vrcAvatarDescriptor);
            VRCExpressionParameters.ValueType vrcParamType = ConvertAnimatorToVrcParamType(dataType);

            for (int i = 0; i < fx.parameters.Length; i++)
            {
                if (paramName.Equals(fx.parameters[i].name))
                    fx.RemoveParameter(i); //Remove anyway just in case theres a new datatype
            }
            fx.AddParameter(paramName, dataType);

            CreateVrcParameter(vrcAvatarDescriptor.expressionParameters, paramName, vrcParamType, defaultValue, true);

            EditorUtility.SetDirty(fx);
            AssetDatabase.Refresh();
        }

        public static void BackupController(AnimatorController ac)
        {
            if (ac == null)
            {
                Debug.LogWarning("No Animator Controller selected.");
                return;
            }

            string path = AssetDatabase.GetAssetPath(ac);
            string newPath = AssetDatabase.GenerateUniqueAssetPath(Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(ac.name) + "_Backup" + Path.GetExtension(path));
            AnimatorController newController = UnityEditor.Animations.AnimatorController.Instantiate(ac);
            AssetDatabase.CreateAsset(newController, newPath);
            AssetDatabase.SaveAssets();

            Debug.Log("Animator controller cloned and saved at " + newPath);
        }

        public static void UncheckAllWriteDefaults(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            var baseAnimations = vrcAvatarDescriptor.baseAnimationLayers;
            var specialAnimations = vrcAvatarDescriptor.specialAnimationLayers;
            var aControllers = baseAnimations.Concat(specialAnimations);
            foreach (var aController in aControllers)
            {
                if (aController.isDefault == false)
                {
                    //Debug.Log(aController.animatorController);
                    var controller = aController.animatorController as UnityEditor.Animations.AnimatorController;
                    foreach (var cLayer in controller.layers)
                    {
                        //Debug.Log(cLayer.stateMachine);
                        var cStates = cLayer.stateMachine.states;
                        //Debug.Log(cLayer.stateMachine.stateMachines);
                        foreach (var cState in cStates)
                        {
                            //Debug.Log(cState.state);
                            if (cState.state.writeDefaultValues)
                            {
                                cState.state.writeDefaultValues = false;
                                Debug.Log("Unchecked Write Defaults for: " + cState.state);
                            }
                        }
                    }
                }
            }
        }

        public static void CopyControllerParams(AnimatorController source, AnimatorController destination)
        {
            foreach (var sParam in source.parameters)
            {
                if (!destination.parameters.Contains(sParam))
                {
                    destination.AddParameter(sParam);
                }
            }
        }

        public static void CreateToggleLayer(VRCAvatarDescriptor vrcAvatarDescriptor, string layerName, string paramName, AnimationClip clipA, AnimationClip clipB)
        {
            CreateToggleLayer(vrcAvatarDescriptor, layerName, paramName, clipA, clipB, 0f);
        }

        public static void CreateToggleLayer(VRCAvatarDescriptor vrcAvatarDescriptor, string layerName, string paramName, AnimationClip clipA, AnimationClip clipB, float paramValue)
        {
            var fx = GetFxController(vrcAvatarDescriptor);
            CreateFxParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Bool, paramValue);
            DeleteExistingFxLayer(fx, layerName);

            fx.AddLayer(layerName);

            var fxLayers = fx.layers;
            var newLayer = fxLayers[fx.layers.Length - 1];
            newLayer.defaultWeight = 1f;

            var startState = newLayer.stateMachine.AddState(clipA.name, new Vector3(250, 120));
            var endState = newLayer.stateMachine.AddState(clipB.name, new Vector3(250, 20));
            //Debug.Log("Statemachine: " + clipA.name + " " + clipA.GetInstanceID());
            //Debug.Log("Statemachine: " + clipB.name + " " + clipB.GetInstanceID());

            startState.AddTransition(endState);
            startState.transitions[0].hasFixedDuration = true;
            startState.transitions[0].duration = 0f;
            startState.transitions[0].exitTime = 0f;
            startState.transitions[0].hasExitTime = false;
            startState.transitions[0].AddCondition(AnimatorConditionMode.If, 0f, paramName);
            startState.writeDefaultValues = false;
            startState.motion = clipA;

            endState.AddTransition(startState);
            endState.transitions[0].hasFixedDuration = true;
            endState.transitions[0].duration = 0f;
            endState.transitions[0].exitTime = 0f;
            endState.transitions[0].hasExitTime = false;
            endState.transitions[0].AddCondition(AnimatorConditionMode.IfNot, 0f, paramName);
            endState.writeDefaultValues = false;
            endState.motion = clipB;

            fx.layers = fxLayers; //fixes save for default weight for some reason
            EditorUtility.SetDirty(fx);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void CreateBlendTreeLayer(VRCAvatarDescriptor vrcAvatarDescriptor, string layerName, string paramName, AnimationClip clipA, AnimationClip clipB)
        {
            List<AnimationClip> clipList = new List<AnimationClip>();
            clipList.Add(clipA);
            clipList.Add(clipB);
            CreateBlendTreeLayer(vrcAvatarDescriptor, layerName, paramName, clipList, 0, BlendTreeType.Simple1D);
        }

        public static void CreateBlendTreeLayer(VRCAvatarDescriptor vrcAvatarDescriptor, string layerName, string paramName, AnimationClip clipA, AnimationClip clipB, float defaultValue)
        {
            List<AnimationClip> clipList = new List<AnimationClip>();
            clipList.Add(clipA);
            clipList.Add(clipB);
            CreateBlendTreeLayer(vrcAvatarDescriptor, layerName, paramName, clipList, defaultValue, BlendTreeType.Simple1D);
        }

        public static void CreateBlendTreeLayer(VRCAvatarDescriptor vrcAvatarDescriptor, string layerName, string paramName, List<AnimationClip> clipList, float defaultValue, BlendTreeType blendTreeType)
        {
            var fx = GetFxController(vrcAvatarDescriptor);
            string treeName = layerName + "BlendTree";
            CreateFxParameter(vrcAvatarDescriptor, paramName, AnimatorControllerParameterType.Float, defaultValue);
            DeleteExistingFxLayer(fx, layerName);
            DeleteExistingBlendTree(fx, treeName);

            fx.AddLayer(layerName);

            var fxLayers = fx.layers;
            var newLayer = fxLayers[fx.layers.Length - 1];
            newLayer.defaultWeight = 1f;

            var blendTree = new BlendTree
            {
                name = treeName,
                blendParameter = paramName,
                useAutomaticThresholds = true,
                blendType = blendTreeType,
            };
            if (blendTreeType == BlendTreeType.Simple1D)
            {
                blendTree.AddChild(clipList[0], 0f);
                blendTree.AddChild(clipList[1], 1f);
            }
            if (blendTreeType == BlendTreeType.FreeformCartesian2D)
            {
                for (int i = 0; i < clipList.Count; i++)
                {
                    blendTree.AddChild(clipList[i], 0f);
                }
            }
            AssetDatabase.AddObjectToAsset(blendTree, fx); //Because Unity is stupid, we have to serialize it this way to save the blendtree before we assign it
            var blendTreeState = newLayer.stateMachine.AddState(treeName, new Vector3(250, 120));
            blendTreeState.motion = blendTree;
            blendTreeState.writeDefaultValues = false;

            fx.layers = fxLayers; //fixes save for default weight for some reason
            EditorUtility.SetDirty(fx);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void DeleteExistingFxLayer(AnimatorController fx, string layerName)
        {
            for (int i = 0; i < fx.layers.Length; i++)
            {
                if (fx.layers[i].name.Equals(layerName))
                {
                    fx.RemoveLayer(i);
                    break;
                }
            }
        }

        public static void DeleteExistingBlendTree(AnimatorController fx, string treeName)
        {
            {
                if (fx == null)
                {
                    Debug.LogError("AnimatorController is null.");
                    return;
                }

                var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(fx));
                foreach (var asset in subAssets)
                {
                    if (asset != null && asset.name == treeName)
                    {
                        // Remove the object
                        DestroyImmediate(asset, true); // Use true to allow asset deletion
                        Debug.Log($"Deleted object '{treeName}' from AnimatorController.");
                        EditorUtility.SetDirty(fx); // Mark controller as modified
                        AssetDatabase.SaveAssets(); // Save changes
                        return;
                    }
                }
            }
        }

        public static void DeleteControllerParam(AnimatorController ac, string paramName)
        {
            for (int i = 0; i < ac.parameters.Length; i++)
            {
                if (paramName.Equals(ac.parameters[i].name))
                {
                    ac.RemoveParameter(i); //Delete Param
                    EditorUtility.SetDirty(ac);
                    AssetDatabase.Refresh();
                }
            }

        }

    }
}
