using UnityEngine;
using UnityEditor;
using System.Collections;

public class StartSceneView : MonoBehaviour
{
    // Start is called before the first frame update
#if UNITY_EDITOR
    void Start()
    {
        SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
        Invoke("LateStart", 2); //Delay after start

    }
    void LateStart()
    {
        var vrcCam = GameObject.Find("VRCCam");
        if (vrcCam != null) //If there is a VRCCam load game view instead (This is just easier than trying to callback the VRCSDK for on Avatar build...)
            EditorApplication.ExecuteMenuItem("Window/General/Game");
    }
#endif
}
