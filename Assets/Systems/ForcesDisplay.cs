using UnityEngine;
using FYFY;
using System.Collections;
using System.Collections.Generic;
using System;

/*
 * System "ForcesDisplay":
 * It handles the behaviour of the terrain and receive order to update (and how: see OnRefreshEntered()) from the rest of the systems.
 * 
 */

public class ForcesDisplay : UtilitySystem
{

	// ==== VARIABLES ====

	#region Families (= entities)
	private Family _pPlanFamily	 = FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
	private Family _sourcesFamily = FamilyManager.getFamily (new AllOfComponents (typeof(Field), typeof(Dimensions), typeof(Position)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
	private Family _shipFamily = FamilyManager.getFamily(new AllOfComponents(typeof(ShipInfo)), new NoneOfProperties(PropertyMatcher.PROPERTY.DISABLED));
	private Family _earthFamily = FamilyManager.getFamily (new AllOfComponents (typeof(FinishInformation)));
	private Family _obstacleFamily = FamilyManager.getFamily(new AnyOfComponents(typeof(ObstacleInformation), typeof(BreakableObstacle)));

	// Families to get Global components
	private Family _gameInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GameInformations), typeof(Constants)));
	private Family _stateFamily = FamilyManager.getFamily (new AllOfComponents (typeof(State)));
	private Family _animFamily = FamilyManager.getFamily (new AllOfComponents (typeof(AnimInformation)));
	private Family _levelInfoFamily	= FamilyManager.getFamily(new AllOfComponents(typeof(LevelInformations)));

	// Family to receive orders from other systems (a field has been moved, removed, added,... the terrain needs to be updated)
	private Family _refreshTerrainFamily = FamilyManager.getFamily (new AllOfComponents (typeof(RefreshTerrain)));
	#endregion

	#region Global components
	private Constants _cst; 
	private Terrain _terr;
	private State _currentState;
	private AnimInformation _animInfo;
	private GameObject _levelInfoGO;
	private LevelInformations _li;
	#endregion


	#region Other variables
	private float _epsilon0;
	private GameObject _planesAroundTerrain;
	#endregion



	// ==== Constructor ====

	/// <summary>
	/// Called once at the start of the game to initialize the variables, add callbacks, and initialize the terrain and the local/global heightmap arrays.
	/// </summary>
	public ForcesDisplay()
    {
		// Initialization of Global components & other variables
		_cst  = _gameInfoFamily.First ().GetComponent<Constants>();
		_terr = _pPlanFamily.First ().GetComponent<Terrain> ();
		_currentState = _stateFamily.First ().GetComponent<State>();
		_animInfo = _animFamily.First ().GetComponent<AnimInformation> ();
		_levelInfoGO = _levelInfoFamily.First ();
		_li = _levelInfoGO.GetComponent<LevelInformations> ();

		_epsilon0 = 8.85418782f * Mathf.Pow(10f, -12f); // A^2 * s^4 * kg^-1 * m^-3
		_planesAroundTerrain = GameObject.Find ("PlanesAroundTerrain");


		// Callbacks on family entries
		_refreshTerrainFamily.addEntryCallback (OnRefreshEntered);


		// Initialise the height of the terrain & the planes around it -- y position update
		Vector3 temp = _terr.transform.position;
		temp.y = _animInfo.startingHeightTerrain;
		_terr.transform.position = temp;

		temp = _planesAroundTerrain.transform.position;
		temp.y = _animInfo.startingHeightTerrain;
		_planesAroundTerrain.transform.position = temp;


        InitHeightMapValues();
		resetTerrain ();

		// Initialize the local heightmap arrays of the fields & apply it on terrain
		foreach (GameObject source in _sourcesFamily)
			SetHeightsTerrainDisplay (source, false);

	}

	/// <summary>
	/// Initializes the height map array values of LevelInformation component (unique).
	/// </summary>
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


	/// <summary>
	/// Resets terrain heights, but not the local height map arrays (TerrainDisplay component of the fields) nor the global one (from the unique LevelInformation component).
	/// </summary>
	protected void resetTerrain()
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





	// ==== LIFECYCLE ====

	#region Lifecycle
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
			// Terrain keeps moving down until it hits 0
			if (_terr.transform.position.y > 0 || _planesAroundTerrain.transform.position.y > 0)
            {
				TerrainMovement ();
			}
			else 
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
				_currentState.state = State.STATES.DIALOG;
				GameObjectManager.addComponent<RefreshTrajectory> (_levelInfoGO);
			}

			// To refresh the height based on the current coeff, so we have a smooth movement on the start
			foreach (GameObject source in _sourcesFamily)
			{
				SetHeightsTerrainDisplay (source, true);
			}
		}
		else
        {
			_animInfo.terrainPotentialsCoeff = 1f;
		}
			
	}
	#endregion




	// ==== METHODS ====

	#region Callback methods
	/// <summary>
	/// Other systems communicate with this one by adding the RefreshTerrain component to the GO holding the LevelInformation component.
	/// In the addComponent call, the action to perform (MOVE, ADD,...) and the field doing it have to be registered.
	/// </summary>
	/// <param name="s">GO holding LevelInformation component</param>
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
	#endregion



	#region Force field impacting the terrain methods
	// ===========================
	// === Functions to update the terrain :
	// === - when a force field is moved
	// === - when the height of a force field is modified / or added
	// === - when a force field is deleted
	// ============================

	/// <summary>
	/// When a force field has been moved, call this method to update the global height map array, and refresh the impact one the terrain.
	/// </summary>
	/// <param name="go">The force field that just moved.</param>
	protected void MoveTerrainDisplay(GameObject go, int test = 0)
    {
		TerrainDisplay td = go.GetComponent<TerrainDisplay> ();
		if (go == null || td == null)
        {
			Debug.Log ("Problem during MoveTerrainDisplay");
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


	/// <summary>
	/// When a force field has been created (wasAlreadyOnTerrain == false), or when its intensity has been modify (wasAlreadyOnTerrain == true), 
	/// call this method to update the local height map array of the force field, the global one, and refresh the impact on the terrain.
	/// </summary>
	/// <param name="go">The force field modified or added.</param>
	/// <param name="wasAlreadyOnTerrain">If set to <c>true</c> was already on terrain and do not need memory allocation.</param>
	protected void SetHeightsTerrainDisplay(GameObject go, bool wasAlreadyOnTerrain)
    {
		TerrainDisplay td = go.GetComponent<TerrainDisplay> ();
		if (go == null || td == null)
        {
			Debug.Log ("Problem during SetHeightsTerrainDisplay");
			return;
		}
		Vector3 pos = go.GetComponent<Position> ().pos;
		Field f = go.GetComponent<Field> ();

		int hmWidth = _terr.terrainData.heightmapWidth;
		int hmHeight = _terr.terrainData.heightmapHeight;

		// Compute the needed variables
		float factorToStayInsideMainTerrX = _terr.terrainData.size.x / _cst.TERRAIN_INTERACTABLE_X;
		float factorToStayInsideMainTerrY = _terr.terrainData.size.z / _cst.TERRAIN_INTERACTABLE_Y;
		float scaleX = hmWidth / factorToStayInsideMainTerrX; // = size of the middle terrain
		float scaleY = hmHeight / factorToStayInsideMainTerrY; // = size of the middle terrain

		float factorRadiusSources = 4f; // TODO: Find the right factor - 4 wasn't bad for gaussians
		td.realPosXInTerrain = pos.x * scaleX + hmWidth / 3; // Because we start at one third of the terrain
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

		// if the force field was added, allocate the local height map array
		if (!wasAlreadyOnTerrain)
        {
			td.localHeightmapValues = new float[diameter, diameter];
		}

		for (int x = 0; x < diameter; x++)
        {
			for (int y = 0; y < diameter; y++)
            {
				int currentPosXInTerrain = posXInTerrain - offset + x;
				int currentPosYInTerrain = posYInTerrain - offset + y;
				if (Mathf.Sqrt ((realPosXInTerrain - currentPosXInTerrain) * (realPosXInTerrain - currentPosXInTerrain) + (realPosYInterrain - currentPosYInTerrain) * (realPosYInterrain - currentPosYInTerrain)) < radius)
                {
					// if the force field was already impacting the terrain,  remove the impact of its position on the global height map
					if (wasAlreadyOnTerrain)
                    {
						_li.heightMapValues [currentPosYInTerrain, currentPosXInTerrain] -= td.localHeightmapValues [y, x];
					}

					// compute the potential values on the local height map
					td.localHeightmapValues [y, x] = GaussianEnergy (realPosXInTerrain, realPosYInterrain, f.sigx / 2f * scaleX, f.sigy / 2f * scaleY, f.A / 2f, currentPosXInTerrain, currentPosYInTerrain) * _animInfo.terrainPotentialsCoeff;
					//td.localHeightmapValues [y, x] = PotentialEnergy (realPosXInTerrain, realPosYInterrain, f.sigx / 2f * scaleX, f.sigy / 2f * scaleY, f.A / 2f, currentPosXInTerrain, currentPosYInTerrain, radius);

					// add them to the global height map
					_li.heightMapValues [currentPosYInTerrain, currentPosXInTerrain] += td.localHeightmapValues [y, x];
				}
			}
		}
			
		NormalizeAndSetHeights();
	}


	/// <summary>
	/// When a force field has been deleted, call this method to update the global height map array and remove its impact on the terrain.
	/// </summary>
	/// <param name="go">The force field deleted.</param>
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

		// Compute the needed variables
		float factorToStayInsideMainTerrX = _terr.terrainData.size.x / _cst.TERRAIN_INTERACTABLE_X;
		float factorToStayInsideMainTerrY = _terr.terrainData.size.z / _cst.TERRAIN_INTERACTABLE_Y;
		float scaleX = hmWidth / factorToStayInsideMainTerrX; // = size of the middle terrain
		float scaleY = hmHeight / factorToStayInsideMainTerrY; // = size of the middle terrain

		float factorRadiusSources = 4f; // TODO: Find the right factor - 4 wasn't bad for gaussians
		td.realPosXInTerrain = pos.x * scaleX + hmWidth / 3; // Because we start at one third of the terrain
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

		// Remove the impact of the current force field on the global height map array
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
			}
		}

		NormalizeAndSetHeights();
	}


	/// <summary>
	/// Normalizes the values of the fields' potential energy and apply it on the terrain.
	/// </summary>
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
		if (nbSources < 0) { // TODO: find the number to reach before updating the whole map and not each sources; currently 10

			// for each source, normalize its value and update set its height on the terrain
			foreach (GameObject source in _sourcesFamily)
            {
				TerrainDisplay td = source.GetComponent<TerrainDisplay> ();
				int diameter = td.impactSize;

				float realPosXInTerrain = td.realPosXInTerrain;
				float realPosYInterrain = td.realPosYInTerrain;
				int posXInTerrain = (int)(realPosXInTerrain);
				int posYInTerrain = (int)(realPosYInterrain);

				int offset = (int)(diameter / 2);

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
		}


		// Update the projection to the ground for : sources, ships, obstacles, earth(, bonuses?)
		foreach (GameObject source in _sourcesFamily) {
			MoveProjection (source);
		}

		foreach (GameObject ship in _shipFamily) {
			MoveProjection (ship);
		}

		foreach (GameObject obstacle in _obstacleFamily) {
			MoveProjection (obstacle);
		}

		MoveProjection (_earthFamily.First ());

	}
	#endregion



	#region Terrain animation methods
	/// <summary>
	/// Moves the terrain and the planes around it vertically from its position to 0.
	/// </summary>
	protected void TerrainMovement()
	{
		_terr.transform.position = Vector3.MoveTowards(_terr.transform.position, new Vector3 (_terr.transform.position.x, 0, _terr.transform.position.z), _animInfo.terrainLoweringSpeed * Time.deltaTime);
		_planesAroundTerrain.transform.position = Vector3.MoveTowards (_planesAroundTerrain.transform.position, new Vector3 (_planesAroundTerrain.transform.position.x, 0, _planesAroundTerrain.transform.position.z), _animInfo.terrainLoweringSpeed * Time.deltaTime);
	}
	#endregion




	#region Projection from Gameobject towards the terrain
	/// <summary>
	/// Moves the projection of a GameObject (anything with a LineRenderer and is on top of the playable zone).
	/// </summary>
	/// <param name="go">GameObject with a projection.</param>
	public void MoveProjection(GameObject go)
    {
		// moving projection to correct height
		//Transform projection = forceField.transform.GetChild(0);
		Vector3 projectionPoint = go.transform.position;
		LineRenderer line = go.GetComponent<LineRenderer> ();

		if (line == null) {
			Debug.Log ("The gameobject "+go.name+" has no projection or no line renderer; cannot move the projection.");
			return;
		}

		RaycastHit hit = new RaycastHit();
		int layerMask = 1 << 25;
		layerMask = ~layerMask;
		// touched something and was not UI nor HighPlan
		if (Physics.Raycast (go.transform.position, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
			projectionPoint = hit.point;
			//Transform cylinder = forceField.transform.GetChild (1);
			//cylinder.position = (forceField.transform.position + hit.point) * 0.5f;
			//cylinder.localScale = new Vector3(cylinder.localScale.x, Vector3.Distance (forceField.transform.position, hit.point)*0.5f/forceField.transform.lossyScale.y, cylinder.localScale.z);
		}
		line.SetPosition (1, go.transform.position);
		line.SetPosition (0, projectionPoint);
	}
	#endregion




	#region Functions to compute potential energy
	// Returns a value between -1 and 1
	/// <summary>
	/// Computes the potential energy at the (x0, y0) point, based on the width (sigx, sigy), intensity (A), origin (x,y) and the impact radius (R0) of a force field.
	/// </summary>
	/// <returns>The potential energy between -1 and 1.</returns>
	/// <param name="x0">The x coordinate of the impacted point.</param>
	/// <param name="y0">The y coordinate of the impacted point.</param>
	/// <param name="sigx">Sigx.</param>
	/// <param name="sigy">Sigy.</param>
	/// <param name="A">The intensity.</param>
	/// <param name="x">The x coordinate of the force field.</param>
	/// <param name="y">The y coordinate of the force field.</param>
	/// <param name="R0">The impact radius.</param>
	protected float PotentialEnergy(float x0, float y0, float sigx, float sigy, float A, float x, float y, float R0)
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


	/// <summary>
	/// Compute the value of a gaussian function at the (x0, y0) point, based on the width (sigx, sigy), the intensity (A), and the origin (x, y) of the force field.
	/// </summary>
	/// <returns>A value between -1 and 1.</returns>
	/// <param name="x0">The x coordinate of the impacted point.</param>
	/// <param name="y0">The y coordinate of the impacted point.</param>
	/// <param name="sigx">Sigx.</param>
	/// <param name="sigy">Sigy.</param>
	/// <param name="A">The intensity.</param>
	/// <param name="x">The x coordinate of the force field.</param>
	/// <param name="y">The y coordinate of the force field.</param>
	protected float GaussianEnergy(float x0, float y0, float sigx, float sigy, float A, float x, float y)
    {
		return A * Mathf.Exp (-((((x - x0)*(x - x0)) / (2 * sigx*sigx)) + (((y - y0)*(y - y0)) / (2 * sigy*sigy))));
	}
	#endregion

}