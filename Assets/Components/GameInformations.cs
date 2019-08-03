using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 * Project ISG : "Force Field Potentials"
 * UPMC 2017/2018
 * 
 * Nicolas BILLOD
 * Guillaume LORTHIOIR
 * Tanguy SOTO
 */

public class GameInformations : MonoBehaviour
{

	public int noLevel 			= 1; // numero level - level in which the player is in
	public int unlockedLevels 	= 1; // number of unlocked levels
	public int totalLevels 		= 15; // number of levels

	public bool creationMode = false;

	public bool playSounds = true;
	public bool playMusics = true;
}