using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateImage : MonoBehaviour
{
    public Camera my_camera;
    public float rotationX, rotationY;
    public float rotationZ = 0F;
    private const float TIME_ROTATE = 1f / 60;

    void Start(){
		my_camera = GameObject.Find ("MainCameraTest").GetComponent<Camera> ();
	}

    void Update()
    {
        transform.LookAt(transform.position + my_camera.transform.rotation * Vector3.forward, my_camera.transform.rotation * Vector3.up);
        rotationX = transform.rotation.x;
        rotationY = transform.rotation.y;
        rotationZ += Mathf.CeilToInt(Time.deltaTime / TIME_ROTATE);

        if (rotationZ >= 360)
            rotationZ = 0;

        transform.Rotate(new Vector3(rotationX, rotationY, rotationZ));
    }
}
