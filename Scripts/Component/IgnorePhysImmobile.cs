//Made by ShadsterWolf
using UnityEngine;
using VRC_PhysBone = VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone;
using UnityEditor;
using System.Collections;
using System;

public class IgnorePhysImmobile : MonoBehaviour
{
#if UNITY_EDITOR
    void Start()
    {
        if (Application.isPlaying) //While in playmode, temporarily update 
        {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            foreach (GameObject go in allObjects)
            {
                VRC_PhysBone physBone = go.GetComponent<VRC_PhysBone>();
                if (physBone != null)
                {
                    if ((int)physBone.immobileType == 1) //Check type is world
                    {
                            physBone.immobile = 0; //Set value to 0
                    }
                }
            }
        }
    }
#endif
}
