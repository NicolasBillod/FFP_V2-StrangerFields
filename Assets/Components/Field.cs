using UnityEngine;

/*
 * Project ISG : "Force Field Potentials"
 * UPMC 2017/2018
 * 
 * Nicolas BILLOD
 * Guillaume LORTHIOIR
 * Tanguy SOTO
 */

public class Field : MonoBehaviour {
	
	public float sigx = 0.1f;
	public float sigy = 0.1f;

	// === Gaussian parameters
	public float A = 0.2f;
	public bool isRepulsive = false;

	// === Plan parameters : z = bx + cy
	/*public bool isUniform = false;

	public float b = 0.5f;
	public float c = 0f;
	*/
}