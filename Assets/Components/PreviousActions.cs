using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviousActions : MonoBehaviour {

	public enum CANCELACTIONS {DEL, ADD, MOVE, INTENSITY, SPEED, ANGLE};
	public List<CANCELACTIONS> listOfPreviousActions;
	public List<DataGO> cancelData;

	public List<CANCELACTIONS> listOfRedoActions;
	public List<DataGO> redoData;

	public PreviousActions(){
		listOfPreviousActions = new List<CANCELACTIONS> ();
		cancelData = new List<DataGO> ();

		listOfRedoActions = new List<CANCELACTIONS> ();
		redoData = new List<DataGO> ();
	}
}
