using UnityEngine;
using FYFY;

/*
 * Project ISG : "Force Field Potentials"
 * UPMC 2017/2018
 * 
 * Nicolas BILLOD
 * Guillaume LORTHIOIR
 * Tanguy SOTO
 */

public class Constants : MonoBehaviour {

	// ==== VARIABLES ====

	public float BASE_PPLAN_HEIGHT 	= 0.5f;
	public float BASE_SOURCE_HEIGHT 	= 1.08f;

	public int 	FORCES_ROUNDING 		= 1000;	// give 3 numbers after floating point		
	public float FORCES_SCALING 			= 20;
	public float SOURCES_SIZE_SCALING 	= 7;

	public int TERRAIN_INTERACTABLE_X = 10;
	public int TERRAIN_INTERACTABLE_Y = 10;
}