using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataGO {
	public Dictionary<System.Type, Dictionary<System.Reflection.FieldInfo, object>> allTheComponents;
	public Vector3 transformPos;
	public Quaternion transformRot;
	// id is only used for the fields to delete to keep track of the cancel actions
	public int id;
	// the GO is NOT for the delete cancel actions
	public GameObject theGO;

	public DataGO(){
		allTheComponents = new Dictionary<System.Type, Dictionary<System.Reflection.FieldInfo, object>> ();
	}
}
