using UnityEngine;
using UnityEditor;

public class StartSceneView : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
    }
}
