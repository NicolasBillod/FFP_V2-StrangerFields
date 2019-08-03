using UnityEngine;
using FYFY;
using System.Collections.Generic;

public class UtilitySystem : FSystem
{
	protected Family gameInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GameInformations), typeof(Constants)));


	private Constants _cst;
	private float _epsilon0;

	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
	}

	protected override void onProcess(int familiesUpdateCount) {
	}


	public UtilitySystem(){
		_epsilon0 = 8.85418782f * Mathf.Pow(10f, -12f); // A^2 * s^4 * kg^-1 * m^-3
		_cst = gameInfoFamily.First ().GetComponent<Constants>();
	}

	public void changePosition(Position position, Vector3 newPosition, Vector3 terrDims){

		// First, we update the component value
		position.pos = newPosition;

		// Then, we apply it to the Transform of the gameobject holding the component Position
		Transform tr = position.transform;
		Vector3 trPos = new Vector3 (newPosition.x * _cst.TERRAIN_INTERACTABLE_X, _cst.BASE_SOURCE_HEIGHT * terrDims.y, newPosition.y * _cst.TERRAIN_INTERACTABLE_Y);
		tr.position = trPos;
	}

    public void changeTransformPosToPosition(Position position, Vector3 thePosition, Vector3 terrDims)
    {
        Vector3 newPosition = new Vector3(thePosition.x / _cst.TERRAIN_INTERACTABLE_X, thePosition.z / _cst.TERRAIN_INTERACTABLE_Y, 0);

        position.pos = newPosition;
        position.initialPos = newPosition;
    }


    public DataGO StoreGameObjectComponents(GameObject go){
		DataGO fakeGO = new DataGO ();
		//fakeGO.allTheComponents.Clear();
		Component[] comps = go.GetComponents(typeof(Component));
		foreach (Component comp in comps) {
			System.Type type = comp.GetType ();
			System.Reflection.FieldInfo[] fields = type.GetFields ();
			fakeGO.allTheComponents [type] = new Dictionary<System.Reflection.FieldInfo, object> ();
			foreach (System.Reflection.FieldInfo field in fields) {
				fakeGO.allTheComponents [type] [field] = field.GetValue (comp);
			}
		}
		fakeGO.transformPos = go.transform.position;
		fakeGO.transformRot = go.transform.rotation;
		if (go.GetComponent<FieldID> ())
			fakeGO.id = go.GetComponent<FieldID> ().id;
		else
			fakeGO.id = -1;
		fakeGO.theGO = go;
		return fakeGO;
	}



	// ******* GENERIC POOL METHODS
	public GameObject GetGOFromPool(GameObject goWithPoolComponent){
		PoolMissileImpacts poolComponent = goWithPoolComponent.GetComponent<PoolMissileImpacts> ();
		poolComponent.usedImpacts++;
		GameObject go;
		if (poolComponent.usedImpacts >= poolComponent.poolImpacts.Count) {
			go = GameObject.Instantiate (poolComponent.impactPrefab, poolComponent.transform);
			poolComponent.poolImpacts.Add (go);
		}
		// there are still gameobjects in the pool
		else {
			go = poolComponent.poolImpacts [poolComponent.usedImpacts - 1];
		}
		return go;
	}

	public void ReturnGOToPool(GameObject goWithPoolComponent, GameObject goToReturn){
		PoolMissileImpacts poolComponent = goWithPoolComponent.GetComponent<PoolMissileImpacts> ();
		poolComponent.usedImpacts--;
		poolComponent.poolImpacts.Remove (goToReturn);
		poolComponent.poolImpacts.Add (goToReturn);
	}
	// ****** END GENERIC POOL METHODS


	public Vector3 MouseOrTouchPosition(){
		Vector3 res = Vector3.zero;
		#if (UNITY_STANDALONE || UNITY_EDITOR)
			res = Input.mousePosition;
		#else
			if(Input.touchCount >= 1)
				res = new Vector3 (Input.touches[0].position.x, Input.touches[0].position.y,0);
		#endif
		return res;
	}

	public bool GetMouseOrTouchDown(){
		bool res = false;
		#if (UNITY_STANDALONE || UNITY_EDITOR)
			res = Input.GetMouseButtonDown(0);
		#else
			if(Input.touches[0].phase == TouchPhase.Began)
				res = true;
		#endif
		return res;
	}

	public bool GetMouseOrTouchUp(){
		bool res = false;
		#if (UNITY_STANDALONE || UNITY_EDITOR)
			res = Input.GetMouseButtonUp(0);
		#else
			if(Input.touches[0].phase == TouchPhase.Ended)
				res = true;
		#endif
		return res;
	}

	public bool GetMouseOrTouchHold(){
		bool res = false;
		#if (UNITY_STANDALONE || UNITY_EDITOR)
			res = Input.GetMouseButton(0);
		#else
			if(Input.touches[0].phase == TouchPhase.Moved)
				res = true;
		#endif
		return res;
	}
}
