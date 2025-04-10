using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using static Shadster.AvatarTools.Helper;

namespace Shadster.AvatarTools
{
    public class Common
    {
        public static void FixAvatarDescriptor(VRCAvatarDescriptor vrcAvatarDescriptor)
        {
            Transform armature = vrcAvatarDescriptor.transform.Find("Armature");
            Transform face = vrcAvatarDescriptor.transform.Find("Face");
            if (face == null) { face = vrcAvatarDescriptor.transform.Find("Body"); }
            if (face != null)
            {
                vrcAvatarDescriptor.lipSync = VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape;
                vrcAvatarDescriptor.VisemeSkinnedMesh = face.GetComponent<SkinnedMeshRenderer>();

                vrcAvatarDescriptor.customEyeLookSettings.eyelidType = VRCAvatarDescriptor.EyelidType.Blendshapes;
                vrcAvatarDescriptor.customEyeLookSettings.eyelidsSkinnedMesh = face.GetComponent<SkinnedMeshRenderer>();
                vrcAvatarDescriptor.customEyeLookSettings.eyelidsLookingUp = null;
                vrcAvatarDescriptor.customEyeLookSettings.eyelidsLookingDown = null;
            }
            if (armature != null)
            {
                vrcAvatarDescriptor.customEyeLookSettings.leftEye = FindChildGameObjectByName(armature, "Eye_L");
                vrcAvatarDescriptor.customEyeLookSettings.rightEye = FindChildGameObjectByName(armature, "Eye_R");
            }
        }


        public static void ClearAvatarBlueprintID(GameObject vrcAvatar)
        {
            PipelineManager blueprint = vrcAvatar.GetComponent<PipelineManager>();
            blueprint.blueprintId = null;
        }

        public static void SetAvatarMeshBounds(GameObject vrcAvatar)
        {
            Vector3 vectorSize;
            vectorSize.x = 2.5f;
            vectorSize.y = 2.5f;
            vectorSize.z = 2.5f;
            Bounds bounds = new Bounds(Vector3.zero, vectorSize);
            foreach (SkinnedMeshRenderer smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                Undo.RecordObject(smr, "Set Avatar Bounds");
                smr.localBounds = bounds;
            }
        }
        public static void SetAvatarAnchorProbes(GameObject vrcAvatar)
        {

            foreach (Renderer r in vrcAvatar.GetComponentsInChildren<Renderer>(true))
            {
                Undo.RecordObject(r, "Set Avatar Anchor Probe");
                r.probeAnchor = vrcAvatar.transform.Find("Armature").Find("Hips");
            }
        }

        public static void ResetAvatarBounds(GameObject vrcAvatar)
        {
            foreach (SkinnedMeshRenderer smr in vrcAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                smr.sharedMesh.RecalculateBounds();
            }

        }

    }
}
