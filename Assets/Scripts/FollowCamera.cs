using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Camera my_camera;

	void Start()
    {
		my_camera = GameObject.Find ("MainCameraTest").GetComponent<Camera>();
	}

    void Update ()
    {
        transform.LookAt(transform.position + my_camera.transform.rotation * Vector3.forward, my_camera.transform.rotation * Vector3.up);
    }
}
