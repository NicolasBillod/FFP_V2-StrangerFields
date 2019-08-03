using UnityEngine;
using FYFY;
using System.Collections;
using System.Collections.Generic;
using System;

using System.Linq; // used for Sum of array

public class ForcesDisplayLD : UtilitySystem
{

	// ==== VARIABLES ====

	private Family _pPlanFamily	 = FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
	private Family _sourcesFamily = FamilyManager.getFamily (new AllOfComponents (typeof(Field), typeof(Dimensions), typeof(Position)), new NoneOfProperties(PropertyMatcher.PROPERTY.DISABLED));
	private Family _gameInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GameInformations), typeof(Constants)));

	private Family _stateFamily = FamilyManager.getFamily (new AllOfComponents (typeof(State)));
	private Family _animFamily = FamilyManager.getFamily (new AllOfComponents (typeof(AnimInformation)));

	private Family _levelInfoFamily	= FamilyManager.getFamily(new AllOfComponents(typeof(LevelInformations)));

	private Family _refreshTerrainFamily = FamilyManager.getFamily (new AllOfComponents (typeof(RefreshTerrain)));

	// ==== Components
	private Constants _cst;
	private Terrain _terr;
	private State _currentState;
	private AnimInformation _animInfo;

	private GameObject _levelInfoGO;
	private LevelInformations _li;

	private float _epsilon0;

	private GameObject _planesAroundTerrain;

	// ==== LIFECYCLE ====

	public ForcesDisplayLD()
    {
		_planesAroundTerrain = GameObject.Find ("PlanesAroundTerrain");

		_refreshTerrainFamily.addEntryCallback (OnRefreshEntered);
		_cst  = _gameInfoFamily.First ().GetComponent<Constants>();
		_terr = _pPlanFamily.First ().GetComponent<Terrain> ();
		_epsilon0 = 8.85418782f * Mathf.Pow(10f, -12f); // A^2 * s^4 * kg^-1 * m^-3
		_currentState = _stateFamily.First ().GetComponent<State>();
		_levelInfoGO = _levelInfoFamily.First ();
		_li = _levelInfoGO.GetComponent<LevelInformations> ();
		_animInfo = _animFamily.First ().GetComponent<AnimInformation> ();

		// Initialise the height of the terrain & the planes around it
		Vector3 temp = _terr.transform.position;
		temp.y = _animInfo.startingHeightTerrain;
		_terr.transform.position = temp;

		temp = _planesAroundTerrain.transform.position;
		temp.y = _animInfo.startingHeightTerrain;
		_planesAroundTerrain.transform.position = temp;

        InitHeightMapValues();
		refreshTerrain ();

		foreach (GameObject source in _sourcesFamily)
        {
			SetHeightsTerrainDisplay (source, false);
		}
	}

	protected override void onPause(int currentFrame)
    {
	}

	protected override void onResume(int currentFrame)
    {
	}

	protected override void onProcess(int familiesUpdateCount)
    {
		if (_currentState.state == State.STATES.ANIM1)
        {
			if (_terr.transform.position.y > 0 || _planesAroundTerrain.transform.position.y > 0)
            {
				terrainMovement ();
			} else
            {
				_currentState.state = State.STATES.ANIM2;
			}
		}
		else if (_currentState.state == State.STATES.ANIM2)
        {
			if (_animInfo.terrainPotentialsCoeff < 1f)
            {
				_animInfo.terrainPotentialsCoeff += Time.deltaTime * _animInfo.terrainPotentialsSpeed;
			}
            else
            {
				_animInfo.terrainPotentialsCoeff = 1f;
				_currentState.state = State.STATES.SETUP;
				GameObjectManager.addComponent<RefreshTrajectory> (_levelInfoGO);
			}
			NormalizeAndSetHeights ();
		}
		else
        {
			_animInfo.terrainPotentialsCoeff = 1f;
			_terr.transform.position = new Vector3(_terr.transform.position.x, 0, _terr.transform.position.z);
		}
			
	}

	// ==== METHODS ====

    protected void InitHeightMapValues()
    {
		int hmWidth = _terr.terrainData.heightmapWidth;
		int hmHeight = _terr.terrainData.heightmapHeight;

        _li.heightMapValues = new float[hmHeight, hmWidth];
        for (int x = 0; x < hmWidth; x++)
        {
            for (int y = 0; y < hmHeight; y++)
            {
                _li.heightMapValues[y, x] = _cst.BASE_PPLAN_HEIGHT;
            }
        }

    }

    protected void OnRefreshEntered(GameObject s)
    {
		RefreshTerrain[] listRefreshT = s.GetComponents<RefreshTerrain> ();
		foreach(RefreshTerrain refreshT in listRefreshT)
        {
			GameObject source = refreshT.source;
			switch (refreshT.action)
            {
			case RefreshTerrain.ACTIONS.MOVE:
				MoveTerrainDisplay (source);
				break;
			case RefreshTerrain.ACTIONS.MODIFY:
				SetHeightsTerrainDisplay (source, true);
				break;
			case RefreshTerrain.ACTIONS.ADD:
				SetHeightsTerrainDisplay (source, false);
				break;
			case RefreshTerrain.ACTIONS.DELETE:
				ResetTerrainDisplay (source);
				break;
			case RefreshTerrain.ACTIONS.RELOAD:
				NormalizeAndSetHeights ();
				break;
			default:
				break;
			}
			// After actualisation, remove the component
			GameObjectManager.removeComponent<RefreshTerrain> (s);
		}
	}

	// === Animation
	protected void terrainMovement()
    {
		_terr.transform.position = Vector3.MoveTowards(_terr.transform.position, new Vector3 (_terr.transform.position.x, 0, _terr.transform.position.z), _animInfo.terrainLoweringSpeed * Time.deltaTime);
		_planesAroundTerrain.transform.position = Vector3.MoveTowards (_planesAroundTerrain.transform.position, new Vector3 (_planesAroundTerrain.transform.position.x, 0, _planesAroundTerrain.transform.position.z), _animInfo.terrainLoweringSpeed * Time.deltaTime);
	}

	protected void refreshTerrain()
    {
		// Get Terrain heightMap and place it
		int hmWidth = _terr.terrainData.heightmapWidth;
		int hmHeight = _terr.terrainData.heightmapHeight;

		float[,] level = _terr.terrainData.GetHeights (0, 0, hmWidth, hmHeight);
		//float [,] level = terr.terrainData.GetHeights(0, 0, terr.terrainData.heightmapResolution, terr.terrainData.heightmapResolution); // terr.terrainData.heightmapResolution = 129 before tests
		for (int x = 0; x < hmWidth; x++)
        {
			for (int y = 0; y < hmHeight; y++)
            {
				level [y, x] = _cst.BASE_PPLAN_HEIGHT;
			}
		}
		_terr.terrainData.SetHeights (0, 0, level);
	}

	// ===========================
	// === Functions to update the terrain :
	// === - when a field force is moved
	// === - when the height of a field force is modified / or added
	// === - when a field force is deleted
	// ============================

	protected void MoveTerrainDisplay(GameObject go)
    {
		// Get all the needed info
		TerrainDisplay td = go.GetComponent<TerrainDisplay> ();
		if (go == null || td == null)
        {
			Debug.Log ("Problem during UpdateTerrainDisplay");
			return;
		}
		Vector3 pos = go.GetComponent<Position> ().pos;
		Field f = go.GetComponent<Field> ();

		int hmWidth = _terr.terrainData.heightmapWidth;
		int hmHeight = _terr.terrainData.heightmapHeight;

		int diameter = td.impactSize;

		// First - Remove the impact of the current force field from its previous position on the global heightMap 
		float[,] level = new float[diameter,diameter];
		for (int x = 0; x < diameter; x++)
        {
			for (int y = 0; y < diameter; y++)
            {
				int currentPosXInTerrain = td.startingPointXOnTerrain + x;
				int currentPosYInTerrain = td.startingPointYOnTerrain + y;
				if (Mathf.Sqrt ((td.realPosXInTerrain - currentPosXInTerrain) * (td.realPosXInTerrain - currentPosXInTerrain) + (td.realPosYInTerrain - currentPosYInTerrain) * (td.realPosYInTerrain - currentPosYInTerrain)) < td.impactSize / 2)
                {
					_li.heightMapValues [currentPosYInTerrain, currentPosXInTerrain] -= td.localHeightmapValues [y, x];
				}
				level [y, x] = _cst.BASE_PPLAN_HEIGHT;
			}
		}
		// Not necessary if number of fields > 10
		_terr.terrainData.SetHeights (td.startingPointXOnTerrain, td.startingPointYOnTerrain, level);


		// Compute the needed variables
		float factorToStayInsideMainTerrX = _terr.terrainData.size.x / _cst.TERRAIN_INTERACTABLE_X;
		float factorToStayInsideMainTerrY = _terr.terrainData.size.z / _cst.TERRAIN_INTERACTABLE_Y;
		float scaleX = hmWidth / factorToStayInsideMainTerrX; // = size of the middle terrain
		float scaleY = hmHeight / factorToStayInsideMainTerrY; // = size of the middle terrain

		float factorRadiusSources = 4f; // TODO: Find the right factor - 4 wasn't bad for gaussians
		td.realPosXInTerrain = pos.x * scaleX + hmWidth / 3; // Because we start at one third of the terrain
		float realPosXInTerrain = td.realPosXInTerrain;
		td.realPosYInTerrain = pos.y * scaleY + hmHeight / 3;
		float realPosYInTerrain = td.realPosYInTerrain;
		int posXInTerrain = (int)(realPosXInTerrain);
		int posYInTerrain = (int)(realPosYInTerrain);

		diameter = (int)(f.sigx * scaleX * factorRadiusSources) ;
		td.impactSize = diameter;
		float radius = diameter / 2;
		int offset = (int)(radius);
		td.startingPointXOnTerrain = posXInTerrain - offset;
		td.startingPointYOnTerrain = posYInTerrain - offset;


		// Second - Add the impact of the force field at the new position on the global heightMap
		for (int x = 0; x < diameter; x++)
        {
			for (int y = 0; y < diameter; y++)
            {
				int currentPosXInTerrain = posXInTerrain - offset + x;
				int currentPosYInTerrain = posYInTerrain - offset + y;
				if (Mathf.Sqrt ((realPosXInTerrain - currentPosXInTerrain) * (realPosXInTerrain - currentPosXInTerrain) + (realPosYInTerrain - currentPosYInTerrain) * (realPosYInTerrain - currentPosYInTerrain)) < radius)
                {
					_li.heightMapValues [currentPosYInTerrain, currentPosXInTerrain] += td.localHeightmapValues [y, x];
				}
			}
		}


		// Third - Normalize and set heights
		NormalizeAndSetHeights();
	}

	protected void SetHeightsTerrainDisplay(GameObject go, bool wasAlreadyOnTerrain)
    {
		TerrainDisplay td = go.GetComponent<TerrainDisplay> ();
		if (go == null || td == null)
        {
			Debug.Log ("Problem during InitializeTerrainDisplay");
			return;
		}
		Vector3 pos = go.GetComponent<Position> ().pos;
		Field f = go.GetComponent<Field> ();

		int hmWidth = _terr.terrainData.heightmapWidth;
		int hmHeight = _terr.terrainData.heightmapHeight;

		float factorToStayInsideMainTerrX = _terr.terrainData.size.x / _cst.TERRAIN_INTERACTABLE_X;
		float factorToStayInsideMainTerrY = _terr.terrainData.size.z / _cst.TERRAIN_INTERACTABLE_Y;
		float scaleX = hmWidth / factorToStayInsideMainTerrX;
		float scaleY = hmHeight / factorToStayInsideMainTerrY;

		float factorRadiusSources = 4f; // TODO: Find the right factor - 4 wasn't bad for gaussians
		td.realPosXInTerrain = pos.x * scaleX + hmWidth/3;
		float realPosXInTerrain = td.realPosXInTerrain;
		td.realPosYInTerrain = pos.y * scaleY + hmHeight / 3;
		float realPosYInterrain = td.realPosYInTerrain;
		int posXInTerrain = (int)(realPosXInTerrain);
		int posYInTerrain = (int)(realPosYInterrain);

		int diameter = (int)(f.sigx * scaleX * factorRadiusSources) ;
		td.impactSize = diameter;

		float radius = diameter / 2;
		int offset = (int)(radius);
		td.startingPointXOnTerrain = posXInTerrain - offset;
		td.startingPointYOnTerrain = posYInTerrain - offset;

		if (!wasAlreadyOnTerrain)
        {
			td.localHeightmapValues = new float[diameter, diameter];
		}
		//float[,] level = new float[diameter,diameter];

		for (int x = 0; x < diameter; x++)
        {
			for (int y = 0; y < diameter; y++)
            {
				int currentPosXInTerrain = posXInTerrain - offset + x;
				int currentPosYInTerrain = posYInTerrain - offset + y;
				if (Mathf.Sqrt ((realPosXInTerrain - currentPosXInTerrain) * (realPosXInTerrain - currentPosXInTerrain) + (realPosYInterrain - currentPosYInTerrain) * (realPosYInterrain - currentPosYInTerrain)) < radius)
                {
					if (wasAlreadyOnTerrain)
                    {
						_li.heightMapValues [currentPosYInTerrain, currentPosXInTerrain] -= td.localHeightmapValues [y, x];
					}
					td.localHeightmapValues [y, x] = gaussian (realPosXInTerrain, realPosYInterrain, f.sigx / 2f * scaleX, f.sigy / 2f * scaleY, f.A / 2f, currentPosXInTerrain, currentPosYInTerrain);
					//td.localHeightmapValues [y, x] = testPotentialEnergy (realPosXInTerrain, realPosYInterrain, f.sigx / 2f * scaleX, f.sigy / 2f * scaleY, f.A / 2f, currentPosXInTerrain, currentPosYInTerrain, radius);
					_li.heightMapValues [currentPosYInTerrain, currentPosXInTerrain] += td.localHeightmapValues [y, x];
				}
				//level [y, x] = li.heightMapValues [currentPosYInTerrain, currentPosXInTerrain];
			}
		}

		//terr.terrainData.SetHeights (td.startingPointXOnTerrain, td.startingPointYOnTerrain, level);
		NormalizeAndSetHeights();
	}

	protected void ResetTerrainDisplay(GameObject go)
    {
		TerrainDisplay td = go.GetComponent<TerrainDisplay> ();
		if (go == null || td == null)
        {
			Debug.Log ("Problem during ResetTerrainDisplay");
			return;
		}

		Vector3 pos = go.GetComponent<Position> ().pos;
		Field f = go.GetComponent<Field> ();

		int hmWidth = _terr.terrainData.heightmapWidth;
		int hmHeight = _terr.terrainData.heightmapHeight;

		float factorToStayInsideMainTerrX = _terr.terrainData.size.x / _cst.TERRAIN_INTERACTABLE_X;
		float factorToStayInsideMainTerrY = _terr.terrainData.size.z / _cst.TERRAIN_INTERACTABLE_Y;
		float scaleX = hmWidth / factorToStayInsideMainTerrX;
		float scaleY = hmHeight / factorToStayInsideMainTerrY;

		float factorRadiusSources = 4f; // TODO: Find the right factor - 4 wasn't bad for gaussians
		td.realPosXInTerrain = pos.x * scaleX + hmWidth/3;
		float realPosXInTerrain = td.realPosXInTerrain;
		td.realPosYInTerrain = pos.y * scaleY + hmHeight / 3;
		float realPosYInterrain = td.realPosYInTerrain;
		int posXInTerrain = (int)(realPosXInTerrain);
		int posYInTerrain = (int)(realPosYInterrain);

		int diameter = (int)(f.sigx * scaleX * factorRadiusSources) ;
		td.impactSize = diameter;

		float radius = diameter / 2;
		int offset = (int)(radius);
		td.startingPointXOnTerrain = posXInTerrain - offset;
		td.startingPointYOnTerrain = posYInTerrain - offset;

		//float[,] level = new float[diameter,diameter];

		for (int x = 0; x < diameter; x++)
        {
			for (int y = 0; y < diameter; y++)
            {
				int currentPosXInTerrain = posXInTerrain - offset + x;
				int currentPosYInTerrain = posYInTerrain - offset + y;
				if (Mathf.Sqrt ((realPosXInTerrain - currentPosXInTerrain) * (realPosXInTerrain - currentPosXInTerrain) + (realPosYInterrain - currentPosYInTerrain) * (realPosYInterrain - currentPosYInTerrain)) < radius)
                {
					_li.heightMapValues [currentPosYInTerrain, currentPosXInTerrain] -= td.localHeightmapValues [y, x];
				}
				//level [y, x] = li.heightMapValues [currentPosYInTerrain, currentPosXInTerrain];
			}
		}

		NormalizeAndSetHeights();
	}

	protected void NormalizeAndSetHeights()
    {
		float max = _cst.BASE_PPLAN_HEIGHT;
		float min = _cst.BASE_PPLAN_HEIGHT;

		int hmWidth = _terr.terrainData.heightmapWidth;
		int hmHeight = _terr.terrainData.heightmapHeight;

		// get the min and max heights
		for (int x = 0; x < hmWidth; x++)
        {
			for (int y = 0; y < hmHeight; y++)
            {
				float currentValue = _li.heightMapValues [y, x] - 0.5f;
				if (currentValue > max)
                {
					max = currentValue;
				}
				if (currentValue < min)
                {
					min = currentValue;
				}
			}
		}
		float maxDiff = Mathf.Max (max, -min);


		int nbSources = _sourcesFamily.Count;
		if (nbSources < 10) { // TODO: find the number to reach before updating the whole map and not each sources; currently 10

			// for each source normalize its value and update set its height on the terrain
			foreach (GameObject source in _sourcesFamily)
            {
				TerrainDisplay td = source.GetComponent<TerrainDisplay> ();
				int diameter = td.impactSize;

				float realPosXInTerrain = td.realPosXInTerrain;
				float realPosYInterrain = td.realPosYInTerrain;
				int posXInTerrain = (int)(realPosXInTerrain);
				int posYInTerrain = (int)(realPosYInterrain);

				int offset = (int)(diameter / 2);

				//float[,] level = terr.terrainData.GetHeights (posXInTerrain - offset, posYInTerrain - offset, diameter, diameter);
				float [,] level = new float[diameter,diameter];

				// Normalize and keep plan at 0.5f
				for (int x = 0; x < diameter; x++) {
					for (int y = 0; y < diameter; y++) {
						int currentPosXInTerrain = posXInTerrain - offset + x;
						int currentPosYInTerrain = posYInTerrain - offset + y;
						level [y, x] = _li.heightMapValues [currentPosYInTerrain, currentPosXInTerrain];

						//if (Mathf.Sqrt ((realPosXInTerrain - currentPosXInTerrain) * (realPosXInTerrain - currentPosXInTerrain) + (realPosYInterrain - currentPosYInTerrain) * (realPosYInterrain - currentPosYInTerrain)) < diameter / 2){ 
							if (maxDiff > 0.5f) { // if normalization is necessary
								if (level [y, x] > 0.5f) {
									level [y, x] = 0.5f * (level [y, x] - 0.5f) / (0.5f + maxDiff - 0.5f) + 0.5f; //(b-a)*(myValue-min)/(max-min) + a
								} else if (level [y, x] < 0.5f) {
									level [y, x] = 0.5f - 0.5f * (-(level [y, x] - 0.5f)) / maxDiff;
								}
							}
						//}

					}
				}
				_terr.terrainData.SetHeights (posXInTerrain-offset, posYInTerrain-offset, level);
				moveProjection (source);
			}

		}
		else
        { // Update the whole map
			float[,] level = new float[hmHeight, hmWidth];
			for (int x = 0; x < hmWidth; x++)
            {
				for (int y = 0; y < hmHeight; y++)
                {
                    level [y, x] = _li.heightMapValues [y, x];
					if (maxDiff > 0.5f) {
						if (level [y, x] > 0.5f)
                        {
							level [y, x] = 0.5f * (level [y, x] - 0.5f) / (0.5f + maxDiff - 0.5f) + 0.5f; //(b-a)*(myValue-min)/(max-min) + a
						} else if (level [y, x] < 0.5f)
                        {
							level [y, x] = 0.5f - 0.5f * ( -(level [y, x] - 0.5f)) / maxDiff;
						}
					}
				}
			}
			_terr.terrainData.SetHeights (0, 0, level);
			foreach (GameObject source in _sourcesFamily) {
				moveProjection (source);
			}
		}

	}

	/*
	public void moveProjection(GameObject forceField)
    {
		// moving projection to correct height
		//Transform projection = forceField.transform.GetChild(0);
		//LineRenderer line = projection.gameObject.GetComponent<LineRenderer> ();

		//if (projection == null || line == null) {
		//	Debug.Log ("The gameobject "+forceField.name+" has no projection or no line renderer; cannot move the projection.");
		//	return;
		//}

		RaycastHit hit = new RaycastHit();
		// touched something and was not UI
		if (Physics.Raycast (forceField.transform.position, Vector3.down, out hit))
        {
			//projection.position = hit.point;
			Transform cylinder = forceField.transform.GetChild (0);
			cylinder.position = (forceField.transform.position + hit.point) * 0.5f;
			cylinder.localScale = new Vector3(cylinder.localScale.x, Vector3.Distance (forceField.transform.position, hit.point)*0.5f/forceField.transform.lossyScale.y, cylinder.localScale.z);
		}
		//line.SetPosition (1, forceField.transform.position);
		//line.SetPosition (0, projection.position);
	}*/

	public void moveProjection(GameObject forceField)
	{
		// moving projection to correct height
		//Transform projection = forceField.transform.GetChild(0);
		Vector3 projectionPoint = forceField.transform.position;
		LineRenderer line = forceField.GetComponent<LineRenderer> ();

		if (line == null) {
			Debug.Log ("The gameobject "+forceField.name+" has no projection or no line renderer; cannot move the projection.");
			return;
		}

		RaycastHit hit = new RaycastHit();
		// touched something and was not UI
		if (Physics.Raycast (forceField.transform.position, Vector3.down, out hit))
		{
			projectionPoint = hit.point;
			//Transform cylinder = forceField.transform.GetChild (1);
			//cylinder.position = (forceField.transform.position + hit.point) * 0.5f;
			//cylinder.localScale = new Vector3(cylinder.localScale.x, Vector3.Distance (forceField.transform.position, hit.point)*0.5f/forceField.transform.lossyScale.y, cylinder.localScale.z);
		}
		line.SetPosition (1, forceField.transform.position);
		line.SetPosition (0, projectionPoint);
	}


	// =======================
	// ==== Functions to compute potential energy
	//========================


	// Returns a value between -1 and 1
	protected float testPotentialEnergy(float x0, float y0, float sigx, float sigy, float A, float x, float y, float R0)
    {
		//float epsilon0 = 1 / (36 * Mathf.PI) * Mathf.Pow (10f, -9f);
		float r = Mathf.Sqrt ((x0 - x) * (x0 - x) + (y0 - y) * (y0 - y)); // radius between 0 and 257 if resolution is 257
		float R = sigy;

		float cst1 = A/(4*Mathf.PI*_epsilon0) * (3/(2*R) - 1/R0) ;
		float cst2 = -A / (4*Mathf.PI*_epsilon0*R0);

		float highestValue = 1 / (8f * Mathf.PI * _epsilon0 * R*R*R) + 1/(4*Mathf.PI*_epsilon0) * (3/(2*R) - 1/R0); // TO CHANGE IF sliders are no longer between 0 and 1

		float height = _cst.BASE_PPLAN_HEIGHT;
		if (r <= R) {
			height = (-A * r * r) / (8f * Mathf.PI * _epsilon0 * R*R*R) + cst1;
		} else {
			height = A / (4f * Mathf.PI * _epsilon0 * r) + cst2;
		}

		height /= highestValue;
		return height;
	}

	protected float gaussian(float x0, float y0, float sigx, float sigy, float A, float x, float y)
    {
		return A * Mathf.Exp (-((((x - x0)*(x - x0)) / (2 * sigx*sigx)) + (((y - y0)*(y - y0)) / (2 * sigy*sigy))));
	}


}