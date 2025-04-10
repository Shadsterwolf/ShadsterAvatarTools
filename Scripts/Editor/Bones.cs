using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC_PhysBone = VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone;
using VRC_PhysCollider = VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBoneCollider;

namespace Shadster.AvatarTools
{
    public class Bones : Editor
    {
        public static void UpdateHumanoidArmTwist(GameObject vrcAvatar, float upperArmTwist, float lowerArmTwist)
        {
            ModelImporter modelImporter = Helper.GetModelImporterForObject(vrcAvatar);
            if (modelImporter != null)
            {
                var humanDescription = modelImporter.humanDescription;
                // Update the "Lower Arm Twist" value
                humanDescription.upperArmTwist = upperArmTwist;
                humanDescription.lowerArmTwist = lowerArmTwist;
                modelImporter.humanDescription = humanDescription;

                // Save the changes
                modelImporter.SaveAndReimport();
            }
        }

        public static void MovePhysBonesFromArmature(GameObject vrcAvatar)
        {
            var armature = vrcAvatar.transform.Find("Armature");
            var physbones = armature.GetComponentsInChildren<VRC_PhysBone>();
            if (physbones.Length == 0)
            {
                EditorUtility.DisplayDialog("No PhysBones found!", "There are no PhysBones attached to armature in " + vrcAvatar.name, "Ok");
                return;
            }
            if (vrcAvatar.transform.Find("PhysBones") != null)
            {
                EditorUtility.DisplayDialog("PhysBones Object exists!", "PhysBones Object already exists for this avatar!", "Ok");
                return;
            }
            var physObjectRoot = new GameObject("PhysBones");
            physObjectRoot.transform.parent = vrcAvatar.transform;
            foreach (var pBone in physbones)
            {
                var physObject = new GameObject(pBone.name);
                physObject.transform.parent = physObjectRoot.transform;
                var copyPBone = Helper.CopyComponent(pBone, physObject);

                if (pBone.rootTransform == null)
                {
                    copyPBone.rootTransform = pBone.transform;
                }
                else
                {
                    copyPBone.name = pBone.rootTransform.name;
                }
                DestroyImmediate(pBone);
            }
        }

        public static void MovePhysCollidersFromArmature(GameObject vrcAvatar)
        {
            var armature = vrcAvatar.transform.Find("Armature");
            var physcolliders = armature.GetComponentsInChildren<VRC_PhysCollider>();
            var physbones = vrcAvatar.GetComponentsInChildren<VRC_PhysBone>();
            if (physcolliders.Length == 0)
            {
                EditorUtility.DisplayDialog("No PhysColliders found!", "There are no PhysColliders attached to armature in " + vrcAvatar.name, "Ok");
                return;
            }
            if (vrcAvatar.transform.Find("PhysColliders") != null)
            {
                EditorUtility.DisplayDialog("PhysCollider Object exists!", "PhysCollider Object already exists for this avatar!", "Ok");
                return;
            }
            var physObjectRoot = new GameObject("PhysColliders");
            physObjectRoot.transform.parent = vrcAvatar.transform;
            foreach (var pCollider in physcolliders)
            {
                var physObject = new GameObject(pCollider.name);
                physObject.transform.parent = physObjectRoot.transform;
                var copyPCollider = Helper.CopyComponent(pCollider, physObject);

                if (pCollider.rootTransform == null)
                {
                    copyPCollider.rootTransform = pCollider.transform;
                }
                else
                {
                    copyPCollider.name = pCollider.rootTransform.name;
                }

                foreach (var pBone in physbones) //Move all possible colliders from physbones
                {
                    for (int i = 0; i < pBone.colliders.Count; i++)
                    {
                        if (pBone.colliders[i] == pCollider)
                        {
                            pBone.colliders[i] = copyPCollider;
                        }
                    }
                }
                DestroyImmediate(pCollider);
            }
        }

        public static void SetAllGrabMovement(GameObject vrcAvatar)
        {
            List<VRC_PhysBone> pBones = GetAllAvatarPhysBones(vrcAvatar);
            if (pBones.Count > 0)
            {
                foreach (var pBone in pBones)
                {
                    Undo.RecordObject(pBone, "Set Avatar PhysBone Grab Movement");
                    pBone.grabMovement = 1;
                }
            }
        }

        public static List<VRC_PhysBone> GetAllAvatarPhysBones(GameObject vrcAvatar)
        {
            Transform armature = vrcAvatar.transform.Find("Armature");
            List<VRC_PhysBone> result = new List<VRC_PhysBone>();
            if (armature != null)
            {
                foreach (var pBone in vrcAvatar.GetComponentsInChildren<VRC_PhysBone>(true))
                {
                    result.Add(pBone);
                }
            }
            //Debug.Log(result);
            return result;
        }

        public static void DeleteEndBones(GameObject vrcAvatar)
        {
            Transform armature = vrcAvatar.transform.Find("Armature");
            if (armature != null)
            {
                foreach (Transform bone in armature.GetComponentsInChildren<Transform>(true))
                {
                    if (bone.name.EndsWith("_end"))
                    {
                        Undo.RecordObject(bone, "Delete End Bone");
                        DestroyImmediate(bone.gameObject);
                    }
                }
            }
        }

        public static void RepairMissingPhysboneTransforms(GameObject vrcAvatar)
        {
            Transform pTransform = vrcAvatar.transform.Find("PhysBones");
            if (pTransform != null)
            {
                foreach (VRCPhysBone pBone in pTransform.GetComponentsInChildren<VRCPhysBone>(true))
                {
                    RepairMissingPhysboneTransform(vrcAvatar, pBone);
                }
            }
        }

        public static void RepairMissingPhysboneTransform(GameObject vrcAvatar, VRCPhysBone pBone)
        {
            Transform armature = vrcAvatar.transform.Find("Armature");
            if (armature != null)
            {
                foreach (Transform bone in armature.GetComponentsInChildren<Transform>(true))
                {
                    if (pBone.rootTransform == null && pBone.name == bone.name)
                    {
                        pBone.rootTransform = bone;
                    }
                }
            }
        }

        public static void AddPhysBones(Transform bone)
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

        public static void AddButtPhysBones(Transform bone)
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

        public static bool BoneHasPhysBones(Transform bone)
        {
            if (bone.GetComponent<VRC_PhysBone>() != null)
            {
                return true;
            }
            return false;
        }

        public static Transform GetAvatarBone(GameObject vrcAvatar, string search, string direction)
        {
            Transform armature = vrcAvatar.transform.Find("Armature");
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

    }
}
