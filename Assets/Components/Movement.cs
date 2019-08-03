using UnityEngine;

/*
 * Project ISG : "Force Field Potentials"
 * UPMC 2017/2018
 * 
 * Nicolas BILLOD
 * Guillaume LORTHIOIR
 * Tanguy SOTO
 */

public class Movement : MonoBehaviour {

	public Vector3 initialSpeed = new Vector3(0,0,0); // only used for the ship in case of retry
	public Vector3 speed 		= new Vector3(0,0,0);
	public Vector3 acceleration = new Vector3(0,0,0);

}