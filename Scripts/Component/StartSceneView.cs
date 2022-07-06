using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class StartSceneView : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
#endif
    }
}
