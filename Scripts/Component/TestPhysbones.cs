using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

public class TestPhysbones : MonoBehaviour
{
#if UNITY_EDITOR
    public VRCAvatarDescriptor[] avatars;
    public float speed = 1.0f;
    private bool moveRight = true;
    public float limitX = 1.0f;
    public float radius = 0.1f;
    private float angle;
    public float amplitude = 0.1f;
    private float startY;
    private float time;

    private void Start()
    {
        avatars = Resources.FindObjectsOfTypeAll<VRCAvatarDescriptor>();
    }

    void Update()
    {
        foreach (var avatar in avatars)
        {
            //LeftRight(avatar.gameObject);
            //CircleClockwise(avatar.gameObject);
            Bounce(avatar.gameObject);

        }
    }

    public bool LeftRight(GameObject go)
    {
        if (moveRight)
        {
            go.transform.position += new Vector3(speed * Time.deltaTime, 0, 0);
            if (go.transform.position.x >= limitX)
            {
                moveRight = false;
            }
        }
        else
        {
            go.transform.position -= new Vector3(speed * Time.deltaTime, 0, 0);
            if (go.transform.position.x <= -limitX)
            {
                moveRight = true;
            }
        }
        return true;
    }

    public void CircleClockwise(GameObject go) 
    {
        angle += speed * Time.deltaTime;
        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;
        go.transform.position = new Vector3(x, y, go.transform.position.z);

    }

    public void Bounce(GameObject go)
    {
        time += Time.deltaTime;
        float y = startY + amplitude * Mathf.Sin(speed * time);
        go.transform.position = new Vector3(go.transform.position.x, y, go.transform.position.z);
    }
#endif
}
