using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransition 
{
    /*private Matrix4x4 MatrixLerp(Matrix4x4 src, Matrix4x4 dest, float time)
    {
        Matrix4x4 ret = new Matrix4x4();
        for (int i = 0; i < 16; i++)
        {
            ret[i] = Mathf.Lerp(src[i], dest[i], time);
        }
        return ret;
    }

    private IEnumerator SmoothChangeCamera(bool fromMainCamera)
    {
        float duration = 1f;

        GameObject origin, goal;
        if (fromMainCamera)
        {
            origin = _MainCameras;
            goal = _TopViewCamera.gameObject;
        }
        else
        {
            origin = _TopViewCamera.gameObject;
            goal = _MainCameras;
        }

        _TransitionCamera.transform.position = origin.transform.position;
        _TransitionCamera.transform.rotation = origin.transform.rotation;


        origin.SetActive(false);
        origin.GetComponent<Camera>().enabled = false;
        _TransitionCamera.SetActive(true);

        Matrix4x4 ortho = _TopViewCamera.projectionMatrix;
        Matrix4x4 perspective = _MainCameras.GetComponent<Camera>().projectionMatrix;

        // if we came from Top View Camera, we need to start be switching between orthographic -> perspective before rotating; other way aroung later
        if (!fromMainCamera)
        {
            _TransitionCamera.GetComponent<Camera>().projectionMatrix = ortho;
            float startTime = Time.time;
            while (Time.time - startTime < duration)
            {
                _TransitionCamera.GetComponent<Camera>().projectionMatrix = MatrixLerp(_TransitionCamera.GetComponent<Camera>().projectionMatrix, perspective, 0.1f);
                yield return null;
            }
        }


        while (Vector3.Distance(_TransitionCamera.transform.position, goal.transform.position) > 0.1f || Quaternion.Angle(_TransitionCamera.transform.rotation, goal.transform.rotation) > 1)
        {
            _TransitionCamera.transform.position = Vector3.MoveTowards(_TransitionCamera.transform.position, goal.transform.position, Time.deltaTime * 10f);
            _TransitionCamera.transform.rotation = Quaternion.RotateTowards(_TransitionCamera.transform.rotation, goal.transform.rotation, Time.deltaTime * 75f);
            yield return null;
        }

        // if we came from Main Camera, we need to start be switching between perpective and orthographic after rotating
        if (fromMainCamera)
        {
            _TransitionCamera.GetComponent<Camera>().projectionMatrix = perspective;
            float startTime = Time.time;
            while (Time.time - startTime < duration)
            {
                _TransitionCamera.GetComponent<Camera>().projectionMatrix = MatrixLerp(_TransitionCamera.GetComponent<Camera>().projectionMatrix, ortho, 0.1f);
                yield return null;
            }
        }

        _TransitionCamera.SetActive(false);
        goal.SetActive(true);
        goal.GetComponent<Camera>().enabled = true;
        LockedInterface(false);
    }*/
}
