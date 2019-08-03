using UnityEngine;


public class TerrainDisplay : MonoBehaviour {

	public int startingPointXOnTerrain;
	public int startingPointYOnTerrain;

	public int impactSize; // We will use a square
	public float[,] localHeightmapValues; // impactSize * impactSize dimensions

	public float realPosXInTerrain;
	public float realPosYInTerrain;

	public bool dropped;
}