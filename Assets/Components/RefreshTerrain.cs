using UnityEngine;

public class RefreshTerrain : MonoBehaviour {

	public enum ACTIONS {MOVE, MODIFY, ADD, DELETE, RELOAD};
	public ACTIONS action;

	public GameObject source;

}