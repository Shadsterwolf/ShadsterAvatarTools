using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Shadster.AvatarTools
{
    public class Params
    {
        public static VRCExpressionParameters.ValueType ConvertAnimatorToVrcParamType(AnimatorControllerParameterType dataType)
        {
            VRCExpressionParameters.ValueType vrcParamType;
            switch (dataType)
            {
                case AnimatorControllerParameterType.Int:
                    vrcParamType = VRCExpressionParameters.ValueType.Int;
                    break;
                case AnimatorControllerParameterType.Float:
                    vrcParamType = VRCExpressionParameters.ValueType.Float;
                    break;
                default:
                    vrcParamType = VRCExpressionParameters.ValueType.Bool;
                    break;
            }
            return vrcParamType;
        }

        public static void CreateVrcParameter(VRCExpressionParameters vrcParameters, string paramName, VRCExpressionParameters.ValueType vrcExType)
        {
            CreateVrcParameter(vrcParameters, paramName, vrcExType, 0, true); //minimum defaults
        }

        public static void CreateVrcParameter(VRCExpressionParameters vrcParameters, string paramName, VRCExpressionParameters.ValueType vrcExType, float defaultValue, bool saved)
        {

            var vrcExParams = vrcParameters.parameters.ToList();
            for (int i = 0; i < vrcParameters.parameters.Length; i++)
            {
                if (paramName.Equals(vrcExParams[i].name))
                {
                    vrcExParams.Remove(vrcExParams[i]);
                    break;
                }
            }
            var newVrcExParam = new VRCExpressionParameters.Parameter()
            {
                name = paramName,
                valueType = vrcExType,
                defaultValue = defaultValue,
                saved = saved
            };
            //Debug.Log(newVrcExParam.name + ", default value: " + newVrcExParam.defaultValue);
            vrcExParams.Add(newVrcExParam);
            vrcParameters.parameters = vrcExParams.ToArray();

            EditorUtility.SetDirty(vrcParameters);
            AssetDatabase.Refresh();
        }

        public static void DeleteVrcParameter(VRCExpressionParameters vrcParameters, string paramName)
        {
            var parameter = vrcParameters.FindParameter(paramName);
            if (parameter != null)
            {
                var listVrcParameters = vrcParameters.parameters.ToList();
                listVrcParameters.Remove(parameter);
                vrcParameters.parameters = listVrcParameters.ToArray();
                EditorUtility.SetDirty(vrcParameters);
                AssetDatabase.Refresh();
            }

        }

        
    }
}
