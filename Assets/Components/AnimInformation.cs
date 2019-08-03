using UnityEngine;
using System.Collections;

public class AnimInformation : MonoBehaviour {

	public float startingHeightTerrain = 7f;//cst.BASE_SOURCE_HEIGHT * terrDims.y / 2;


	public float terrainLoweringSpeed = 2f; // TODO: find the right value
	public float terrainPotentialsCoeff = 0f;
	public float terrainPotentialsSpeed = 0.6f; // TODO: find the right value
}
