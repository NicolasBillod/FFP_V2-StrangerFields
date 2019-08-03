using UnityEngine;
using System.Collections;

public class ShipInfo : MonoBehaviour 
{
	public int health;

	public GameObject missilePool;
	public GameObject trajectoryPrefab;

	public Vector3 fireIntensity;
	public float valueSpeedSlider;
	public float angle = 0;

	public int nbMissiles = 5;
}
