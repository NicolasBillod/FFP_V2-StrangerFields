using UnityEngine;
using System.Collections;

/*
 * Project ISG : "Force Field Potentials"
 * UPMC 2017/2018
 * 
 * Nicolas BILLOD
 * Guillaume LORTHIOIR
 * Tanguy SOTO
 */

public class CameraParams : MonoBehaviour {
	
	public Transform target;
	public float distance = 5.0f;
	public float xSpeed = 200.0f;
	public float zoomDampening = 7.0f;

	public float pointStartDrag_x;
	public float distanceDrag_x;

}
