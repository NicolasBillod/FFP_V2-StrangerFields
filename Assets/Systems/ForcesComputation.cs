using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.CollisionManager;
using System.Collections;

/*
 * System "Forces Computation":
 * It handles everything related to the computation of the forces: position of the missile at every step after launch, trajectories preview, AI computation path
 * 
 */


public class ForcesComputation : UtilitySystem {

	// ==== VARIABLES ====

	#region Families (= entities)
	private Family _pPlanFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
	private Family _allSourcesFamily = FamilyManager.getFamily (new AllOfComponents (typeof(Field), typeof(Dimensions), typeof(Position)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
	private Family _missilesFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Dimensions),typeof(Movement),typeof(Position),typeof(Mass)));
	private Family _shipFamily = FamilyManager.getFamily (new AllOfComponents (typeof(ShipInfo), typeof(Position), typeof(Mass), typeof(Editable)));
	private Family _allShipsFamily = FamilyManager.getFamily(new AllOfComponents(typeof(ShipInfo)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
	private Family _enemyShipFamily = FamilyManager.getFamily (new AllOfComponents (typeof(ShipInfo), typeof(Position), typeof(Mass), typeof(Enemy)), new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED));
	private Family _unbreakableObstacleFamily = FamilyManager.getFamily (new AllOfComponents (typeof(ObstacleInformation)), new NoneOfComponents(typeof(BreakableObstacle)));
	private Family _finishFamily = FamilyManager.getFamily (new AllOfComponents (typeof(FinishInformation)));

	// Families to get Global components
	private Family _gameInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GameInformations), typeof(Constants)));
	private Family _stateFamily = FamilyManager.getFamily (new AllOfComponents (typeof(State)));
	private Family _levelInfoFamily	= FamilyManager.getFamily(new AllOfComponents(typeof(LevelInformations)));

	// To be able to start coroutine
	private Family _mainLoopFamily = FamilyManager.getFamily(new AllOfComponents(typeof(MainLoop)));

	// Families to receive orders from other systems:
	// for example, a field has been added so the trajectories preview need to be updated
	private Family _refreshTrajectoryFamily = FamilyManager.getFamily (new AllOfComponents (typeof(RefreshTrajectory)));
	// for exmple, the player pressed the "Fire" button, so we need to start computing a path for the AI
	private Family _aiShouldComputeFamily = FamilyManager.getFamily (new AllOfComponents (typeof(AIShouldCompute)));
	#endregion


	#region Global components
	private GameObject _starship;
	private GameObject _levelInfoGO;
	private Terrain _terr;
	private Constants _cst;
	private MainLoop _mainLoop;
	private State  _currentState;
	#endregion


	// ==== Constructor ====

	/// <summary>
	/// Called once at the start of the game to get the global components.
	/// </summary>
	public ForcesComputation()
    {
		_mainLoop = _mainLoopFamily.First ().GetComponent<MainLoop> ();
		_cst = _gameInfoFamily.First ().GetComponent<Constants> ();
		_terr = _pPlanFamily.First ().GetComponent<Terrain> ();
		_starship = _shipFamily.First ();
		_levelInfoGO = _levelInfoFamily.First ();
		_currentState = _stateFamily.First ().GetComponent<State>();
	}



	// ==== LIFECYCLE ====

	#region Lifecycle
	protected override void onPause(int currentFrame) {
	}

	protected override void onResume(int currentFrame){
	}

	protected override void onProcess(int familiesUpdateCount)
    {
		if (_aiShouldComputeFamily.Count == 1) {
			foreach (GameObject enemyShip in _enemyShipFamily)
            {
				// Start a coroutine for each enemy ship to compute a path
				_mainLoop.StartCoroutine (SimulatedAnnealing (enemyShip, _starship.transform.position, enemyShip.GetComponent<ShipInfo> ().angle, enemyShip.GetComponent<ShipInfo> ().fireIntensity.magnitude));
			}
			GameObjectManager.removeComponent<AIShouldCompute>(_aiShouldComputeFamily.First());
		}
		

		if (_currentState.state == State.STATES.PLAYING)
        {
			ApplyForceToMissiles (); // missiles are moving
		}

		if (_currentState.state == State.STATES.SETUP) {
			// Refresh the trajectory preview
			if (_refreshTrajectoryFamily.Count == 1)
            {
				foreach (GameObject ship in _allShipsFamily)
                {
					ShowTrajectory (ship); // compute and show the trajectory
				}

				GameObject refreshTerrainGO = _refreshTrajectoryFamily.First ();
				RefreshTrajectory[] allComponents = refreshTerrainGO.GetComponents<RefreshTrajectory> ();
				foreach (RefreshTrajectory rt in allComponents) {
					GameObjectManager.removeComponent<RefreshTrajectory> (refreshTerrainGO);
				}
			}
		}
		else { // TODO: modify so it does this only one and not do it every frame
			foreach (GameObject ship in _allShipsFamily)
            {
				HideTrajectory (ship);
			}
		}
	}
	#endregion




	// ==== METHODS ====

	#region Apply force to missile(s)
	/// <summary>
	/// Applies the force to all missiles in play. See ApplyForceToMissle for more information.
	/// </summary>
	protected void ApplyForceToMissiles()
    {
		foreach (GameObject missile in _missilesFamily)
        {
			ApplyForceToMissile (missile);
		}
	}


	/// <summary>
	/// Applies the force to one missile using a computation of forces (see ComputeForceAt), and thus moving it to its position for the next frame. Also changes its rotation and projection.
	/// </summary>
	/// <param name="missile">Missile.</param>
	protected void ApplyForceToMissile(GameObject missile)
    {
		Vector3 terrDims = _terr.terrainData.size;

		// Save the components of each force field in a table
		int count = _allSourcesFamily.Count;
		Field[] tabField = new Field[count];
		Position[] tabPosition = new Position[count];
		for (int i = 0; i < count; i++) {
			GameObject s = _allSourcesFamily.getAt (i);
			tabField[i] = s.GetComponent<Field> ();
			tabPosition[i] = s.GetComponent<Position> ();
		}


		// Get the ship and its position, speed, acc and mass
		Position missilePosition = missile.GetComponent<Position> ();
		Movement movement = missile.GetComponent<Movement> ();
		Mass mass = missile.GetComponent<Mass> ();

		Target tar = missile.GetComponent<Target> ();

		if (tar.target == Vector3.zero)
        {
			// Compute forces to be applied
			Vector3 forces = ComputeForceAt (missilePosition.pos.x, missilePosition.pos.y, tabField, tabPosition);


			// Apply force to the ship using Euler and rotate it in the direciton of the speed
			movement.acceleration = (forces / mass.mass);
			movement.speed += movement.acceleration * Time.deltaTime;
			missilePosition.pos += movement.speed * Time.deltaTime; 

			changePosition (missilePosition, missilePosition.pos, terrDims);
			ChangeRotationToFaceVector (missile, movement.speed);
			MoveProjection (missile);
			//ArrowDisplay (ship, forces);

		}
		else
        {
			// Addforce following the direction (target - position) until collision
			Vector3 direction = tar.target - missilePosition.pos;
			if (Vector3.Distance (tar.target, missilePosition.pos) < 0.01f) {
				GameObjectManager.addComponent<Kamikaze> (missile);
			}
			direction.Normalize ();

			Vector3 newSpeed = direction * movement.speed.magnitude;
			missilePosition.pos += newSpeed * Time.deltaTime;
			changePosition (missilePosition, missilePosition.pos, terrDims);
			ChangeRotationToFaceVector (missile, newSpeed);
			MoveProjection (missile);
		}

		/* Apply force using unity and thus allowing smooth collisons
		Rigidbody r = ship.GetComponent<Rigidbody> ();
		r.AddForce (new Vector3(forces.x,forces.z,forces.y));
		shipPosition.pos = new Vector3 (r.transform.position.x/terrDims.x, r.transform.position.z/terrDims.z, r.transform.position.y);
		m.speed =  new Vector3 (Mathf.Round (1000f * r.velocity.x) / 1000f, Mathf.Round (1000f * r.velocity.z) / 1000f, Mathf.Round (1000f * r.velocity.y) / 1000f);
		m.acceleration = forces;
		*/
	}

	/// <summary>
	/// Changes the rotation of a GameObject to face a vector3.
	/// </summary>
	/// <param name="go">GameObject to rotate.</param>
	/// <param name="vectorToFace">Vector3 to face.</param>
	public static void ChangeRotationToFaceVector(GameObject go, Vector3 vectorToFace)
    {
		Transform tr = go.GetComponent<Transform> ();
		tr.rotation = Quaternion.Euler(0, (360-Mathf.Atan2(vectorToFace.y,vectorToFace.x)*Mathf.Rad2Deg+90)%360,0);
	}

	/// <summary>
	/// Moves the projection of the missile.
	/// </summary>
	/// <param name="missile">Missile.</param>
	public void MoveProjection(GameObject missile)
    {
		// moving projection to correct height
		Transform projection = missile.transform.GetChild(1);
		LineRenderer line = projection.gameObject.GetComponent<LineRenderer> ();

		if (projection == null || line == null) {
			Debug.Log ("The gameobject "+missile.name+" has no projection or no line renderer ; cannot move the projection.");
			return;
		}

		RaycastHit hit = new RaycastHit();
		int layerMask = 1 << 25;
		layerMask = ~layerMask;
		// touched something and was not UI nor HighPlan
		if (Physics.Raycast (missile.transform.position, Vector3.down, out hit, Mathf.Infinity, layerMask)) {
			projection.position = hit.point;
		}
		line.SetPosition (1, missile.transform.position);
		line.SetPosition (0, projection.position);
	}



	/// <summary>
	/// Arrow to display when the missile is moving to show where the force is pushing it.
	/// </summary>
	/// <param name="missile">Missile.</param>
	/// <param name="force">Force.</param>
	public void ArrowDisplay(GameObject missile, Vector3 force)
    {
		Transform arrowProjection = missile.transform.GetChild (2);
		LineRenderer line = arrowProjection.gameObject.GetComponent<LineRenderer> ();
		if (arrowProjection == null || line == null) {
			Debug.Log ("The gameobject "+missile.name+" has no projection or no line renderer ; cannot move the projection.");
			return;
		}

		// Just to visually accentuate the effect of the force
		int factorVisibility = 100;
		arrowProjection.position = missile.transform.position + new Vector3(force.x * factorVisibility, force.z, force.y * factorVisibility);
		line.SetPosition (1, missile.transform.position);
		line.SetPosition (0, arrowProjection.position);
	}
	#endregion


	#region Computation of the force at an (x,y) point
	/// <summary>
	/// Computes the value of the force at (x, y), based on a table of Fields and their positions.
	/// </summary>
	/// <returns>The value of the forces <see cref="UnityEngine.Vector3"/>.</returns>
	/// <param name="x">The x coordinate (between 0 and 1: component Position).</param>
	/// <param name="y">The y coordinate (between 0 and 1: component Position).</param>
	/// <param name="tabField">Table with all the force fields.</param>
	/// <param name="tabPosition">Table with all the force fields positions.</param>
	public Vector3 ComputeForceAt(float x, float y, Field[] tabField, Position[] tabPosition){
		int hmWidth = _terr.terrainData.heightmapWidth;
		int hmHeight = _terr.terrainData.heightmapHeight;

		Vector3 forces = Vector3.zero;
			/*
			// Old way of doing it, using derivates of gaussian instead of a computation close to reality.
			foreach (GameObject s in _allSourcesFamily) {
				// Get associated field and position for each source
				Field f = s.GetComponent<Field> ();
				Position position = s.GetComponent<Position> ();
				Vector3 p = position.pos;
				// Compute force
				//if (f.isUniform) {
				//forces.x += Mathf.Round (Constants.FORCES_ROUNDING *planDerivativeX(p.x * hmWidth, p.y * hmHeight, f.sigx * hmWidth, f.sigy * hmHeight, f.b, f.c, x * hmWidth, y*hmHeight)) / Constants.FORCES_ROUNDING;
				//forces.y += Mathf.Round (Constants.FORCES_ROUNDING * planDerivativeY (p.x * hmWidth, p.y * hmHeight, f.sigx * hmWidth, f.sigy * hmHeight, f.b, f.c,x * hmWidth, y*hmHeight)) / Constants.FORCES_ROUNDING;
			//} else {
				//forces.x += Mathf.Round (_cst.FORCES_ROUNDING * GaussianDerivativeX (p.x * hmWidth, p.y * hmHeight, f.sigx / 2f * hmWidth, f.sigy / 2f * hmHeight, f.A / 2f, x * hmWidth, y * hmHeight)) / _cst.FORCES_ROUNDING;
				//forces.y += Mathf.Round (_cst.FORCES_ROUNDING * GaussianDerivativeY (p.x * hmWidth, p.y * hmHeight, f.sigx / 2f * hmWidth, f.sigy / 2f * hmHeight, f.A / 2f, x * hmWidth, y * hmHeight)) / _cst.FORCES_ROUNDING;
				Vector3 direction = new Vector3 (x, y, p.z) - p;
				forces += direction * testRealForce (p.x * hmWidth, p.y * hmHeight, f.sigx / 2f * hmWidth, f.sigy / 2f * hmHeight, f.A / 2f, x * hmWidth, y * hmHeight);

				//}
			}*/
		int count = tabField.Length;
		for (int i = 0; i < count; i++) {
			Vector3 p = tabPosition[i].pos;
			Vector3 direction = new Vector3 (x, y, p.z) - p;
			forces += direction * RealForce (p.x * hmWidth, p.y * hmHeight, tabField[i].sigx / 2f * hmWidth, tabField[i].sigy / 2f * hmHeight, tabField[i].A / 2f, x * hmWidth, y * hmHeight);
		}
			

		return forces;
	}

	/// <summary>
	/// Derivate of a gaussian at (x0, y0), world unit, for the X part. It is only used for the force computation.
	/// </summary>
	/// <returns>The force value.</returns>
	/// <param name="x0">The x coordinate of the force field.</param>
	/// <param name="y0">The y coordinate of the force field.</param>
	/// <param name="sigx">Sigx.</param>
	/// <param name="sigy">Sigy.</param>
	/// <param name="A">The intensity.</param>
	/// <param name="x">The x coordinate to compute for.</param>
	/// <param name="y">The y coordinate to compute for.</param>
	public float GaussianDerivativeX(float x0, float y0, float sigx, float sigy, float A, float x, float y){
		return _cst.FORCES_SCALING * A * ((x-x0)/(sigx*sigx)) * Mathf.Exp (-((((x - x0)*(x - x0)) / (2 * sigx*sigx)) + (((y - y0)*(y - y0)) / (2 * sigy*sigy))));
	}



	/// <summary>
	/// Derivate of a gaussian at (x0, y0), world unit, for the Y part. It is only used for the force computation.
	/// </summary>
	/// <returns>The force value.</returns>
	/// <param name="x0">The x coordinate of the force field.</param>
	/// <param name="y0">The y coordinate of the force field.</param>
	/// <param name="sigx">Sigx.</param>
	/// <param name="sigy">Sigy.</param>
	/// <param name="A">The intensity.</param>
	/// <param name="x">The x coordinate to compute for.</param>
	/// <param name="y">The y coordinate to compute for.</param>
	public float GaussianDerivativeY(float x0, float y0, float sigx, float sigy, float A, float x, float y){
		return _cst.FORCES_SCALING *A * ((y-y0)/(sigy*sigy)) * Mathf.Exp (-((((x - x0)*(x - x0)) / (2 * sigx*sigx)) + (((y - y0)*(y - y0)) / (2 * sigy*sigy))));
	}


	/// <summary>
	/// Value of the force at the (x0,y0) world point.
	/// </summary>
	/// <returns>The force value.</returns>
	/// <param name="x0">The x coordinate of the force field.</param>
	/// <param name="y0">The y coordinate of the force field.</param>
	/// <param name="sigx">Sigx.</param>
	/// <param name="sigy">Sigy.</param>
	/// <param name="A">The intensity.</param>
	/// <param name="x">The x coordinate to compute for.</param>
	/// <param name="y">The y coordinate to compute for.</param>
	public float RealForce(float x0, float y0, float sigx, float sigy, float A, float x, float y){

		float r = Mathf.Sqrt ((x0 - x) * (x0 - x) + (y0 - y) * (y0 - y));
		float R = sigx;
		float epsilon0 = 8.85418782f * Mathf.Pow (10f, -12f); // A^2 * s^4 * kg^-1 * m^-3
		float constant = 0.000001f;

		if (r < R) {
			return constant * (A / (4 * Mathf.PI * epsilon0 * R * R * R) * r);
		}
		else {
			return constant * (A / (4 * Mathf.PI * epsilon0 * r * r));
		}
	}


	// Used for uniform force field; currently, they have been removed.
	protected float planDerivativeX(float x0, float y0, float sizeX, float sizeY, float b, float c, float x, float y){
		float planX = x0 - x;
		float planY = y0 - y;

		if (Mathf.Abs(planX) < sizeX && Mathf.Abs(planY) < sizeY) {
			return b * _cst.FORCES_SCALING*2;
		}

		return 0;
	}

	// Used for uniform force field; currently, they have been removed.
	protected float planDerivativeY(float x0, float y0, float sizeX, float sizeY, float b, float c, float x, float y){
		float planX = x0 - x;
		float planY = y0 - y;

		if (Mathf.Abs(planX) <= sizeX && Mathf.Abs(planY) <= sizeY) {
			return c * _cst.FORCES_SCALING *2;
		}

		return 0;
	}
	#endregion



	#region Trajectory preview
	/// <summary>
	/// Shows the trajectory preview of a ship, by computing forces for 100 points (starting at 3 different angles for the main ship) based on the fire intensity.
	/// </summary>
	/// <param name="ship">Ship.</param>
	protected void ShowTrajectory(GameObject ship)
    {
		Vector3 terrDims = _terr.terrainData.size;
		Vector3 pos = ship.GetComponent<Position> ().initialPos;
		Vector3 acc = Vector3.zero;
		Vector3 spe = ship.GetComponent<ShipInfo> ().fireIntensity;
		//float mass = shipLauncherFamily.First ().GetComponent<Mass> ().mass;

		// 100 points for the LineRenderer
		int nbPoint = 100;
		// dt based on the launch speed
		float deltaTime = 1 /(100 * spe.magnitude);
		Vector3[] positions = new Vector3 [nbPoint];
		positions [0] = new Vector3 (pos.x * _cst.TERRAIN_INTERACTABLE_X, _cst.BASE_SOURCE_HEIGHT * terrDims.y, pos.y * _cst.TERRAIN_INTERACTABLE_Y);

		ShipInfo sInfo = ship.GetComponent<ShipInfo> ();

        int oldChildCount;

        if (ship.CompareTag("ShipTag"))
            oldChildCount = 8;
        else
            oldChildCount = 7;

        // In case we have less trajectories than missiles; typically the first time it is called
        while (ship.transform.childCount < sInfo.nbMissiles + oldChildCount)
        { 
			GameObject.Instantiate (sInfo.trajectoryPrefab, ship.transform);
		}

		// Save the components of each force field in a table
		int count = _allSourcesFamily.Count;
		Field[] tabField = new Field[count];
		Position[] tabPosition = new Position[count];
		for (int i = 0; i < count; i++)
        {
			GameObject s = _allSourcesFamily.getAt (i);
			tabField[i] = s.GetComponent<Field> ();
			tabPosition[i] = s.GetComponent<Position> ();
		}

		int angle = 0;
		int signe = 1;
		bool alternate = true;
		float oldMagnitude = sInfo.fireIntensity.magnitude;

		// for each trajectory, usually 3 for the main ship
		for (int k = oldChildCount; k < sInfo.nbMissiles + oldChildCount; k++)
        {
			pos = ship.GetComponent<Position> ().initialPos;
			spe = new Vector3(Mathf.Cos(Mathf.Deg2Rad*(sInfo.angle+angle*signe)),Mathf.Sin(Mathf.Deg2Rad*(sInfo.angle+angle*signe)),0);
			spe.Normalize();
			spe *= oldMagnitude;
			if (alternate)
            {
				angle += 30;
			}
			else
            {
				signe *= -1;
			}

			alternate = !alternate;

			LineRenderer line = ship.transform.GetChild(k).GetComponent<LineRenderer> ();

			if (!line.enabled)
            {
				line.enabled = true;
			}

			line.positionCount = nbPoint;

			int i=1;
			float dist = 0;


			while(dist < _cst.TERRAIN_INTERACTABLE_X * _cst.TERRAIN_INTERACTABLE_Y && i < nbPoint)
            {
				// Compute forces to be applied
				Vector3 forces = ComputeForceAt(pos.x,pos.y, tabField, tabPosition);

				// Apply force to the point using Euler
				acc = forces;
				spe += acc * deltaTime;
				pos += spe * deltaTime;
				positions [i] = new Vector3 (pos.x * _cst.TERRAIN_INTERACTABLE_X, _cst.BASE_SOURCE_HEIGHT * terrDims.y, pos.y * _cst.TERRAIN_INTERACTABLE_Y);
				dist += Vector3.Distance (positions [i], positions [i - 1]);
				i++;
			}
			// fill array if not full
			for (int j = i; j < nbPoint; j++)
            {
				positions [j] = positions [i-1];
			}

			line.SetPositions (positions);
		}

	}


	/// <summary>
	/// Hides the trajectory preview for a given ship.
	/// </summary>
	/// <param name="ship">Ship.</param>
	protected void HideTrajectory(GameObject ship)
    {
        int kValue;

        if (ship.CompareTag("ShipTag"))
            kValue = 8;
        else
            kValue = 7;

        for (int k = kValue; k < ship.transform.childCount; k++)
        {
			LineRenderer line = ship.transform.GetChild(k).GetComponent<LineRenderer> ();
			if (line.enabled)
            {
				line.enabled = false;
			}
		}
	}
	#endregion




	/******** ENEMY *********/

	#region Enemy
	// startPos & targetPos need to be vector3 between 0 and 1 (from the component Position), angle int between -180 and 180, intensity float between 0 and 1 with a precision of 0.01
	/// <summary>
	/// How close the missile got to the target. Works a lot like ShowTrajectory().
	/// </summary>
	/// <returns>The minimum distance the missile will got to Earth.</returns>
	/// <param name="shipInitialPos">Launch initial position (between 0 and 1, Position component).</param>
	/// <param name="target">Target position (between 0 and 1, Position component).</param>
	/// <param name="angle">Angle (int between -180 and 180).</param>
	/// <param name="intensity">Intensity (float betwee 0 and 1 with precision of 0.01).</param>
	/// <param name="tabField">Table of all fields.</param>
	/// <param name="tabPosition">Table of all positions.</param>
	public float TargetReached(Vector3 shipInitialPos, Vector3 target, float angle, float intensity, Field[] tabField, Position[] tabPosition){
		Vector3 terrDims = _terr.terrainData.size;

		// Try out the trajectory
		Vector3 pos = shipInitialPos;
		Vector3 realPos = new Vector3 (pos.x * _cst.TERRAIN_INTERACTABLE_X, _cst.BASE_SOURCE_HEIGHT * terrDims.y, pos.y * _cst.TERRAIN_INTERACTABLE_Y);
		Vector3 oldRealPos = realPos;

		Vector3 earthPos = _finishFamily.First ().transform.position;

		Vector3 acceleration = Vector3.zero;
		Vector3 speed = new Vector3 (Mathf.Cos (Mathf.Deg2Rad * angle), Mathf.Sin (Mathf.Deg2Rad * angle), 0);
		speed.Normalize ();
		speed *= intensity;


		// 100 points is an arbitrary value; but it looks good enough
		int nbPointMax = 100;
		float deltaTime = 1 /(nbPointMax * speed.magnitude);
	

		int cpt = 0;
		float minDist = Vector3.Distance (realPos, target);

		float distanceChecked = 0; // the maximum distance the missile will test before stopping: the perimeter of the map?

		while (cpt < nbPointMax*2 && distanceChecked < _cst.TERRAIN_INTERACTABLE_X*_cst.TERRAIN_INTERACTABLE_Y) {
			//float mass = shipLauncherFamily.First ().GetComponent<Mass> ().mass;
			Vector3 forces = ComputeForceAt (pos.x, pos.y, tabField, tabPosition);

			// Apply force to the ship using Euler and rotate it in the direciton of the speed
			acceleration = forces;
			speed += acceleration * deltaTime;//Time.deltaTime;
			pos += speed * deltaTime;//Time.deltaTime; 
			oldRealPos = realPos;
			realPos = new Vector3 (pos.x * _cst.TERRAIN_INTERACTABLE_X, _cst.BASE_SOURCE_HEIGHT * terrDims.y, pos.y * _cst.TERRAIN_INTERACTABLE_Y);
			//realPos [0] = pos.x * _cst.TERRAIN_INTERACTABLE_X;
			//realPos [2] = pos.y * _cst.TERRAIN_INTERACTABLE_Y;

			float distanceToTarget = Vector3.Distance (realPos, target);

			// Checks to see if this position is close to target
			if (distanceToTarget < minDist) {
				minDist = distanceToTarget;
			}

			// Is the missile out of the playable terrain?
			if (!(pos.x >= 0 && pos.x <= 1 && pos.y >= 0 && pos.y <= 1)) {
				return minDist;
			}

			// Is the missile colliding with a planet?
			foreach (GameObject source in _allSourcesFamily) {
				if (Vector3.Distance (source.transform.position, realPos) < 1.6f) { // TODO: maybe change this value, based on the radius of a force field
					return minDist;
				}
			}

			// Is the missile colliding with an unbreakable obstacle?
			foreach (GameObject obstacle in _unbreakableObstacleFamily) {
				if (Vector3.Distance (obstacle.transform.position, realPos) < 2.1f) { // TODO: maybe change this value, based on the radius of an obstacle
					return minDist;
				}
			}

			// Is the missile colling with the Earth?
			if (Vector3.Distance (earthPos, realPos) < 1.125f) { // TODO: maybe change this value, based on the radius of the Earth
				return minDist;
			}

			// Has the missile reached its target?
			if (distanceToTarget < 0.5f) {
				return minDist;
			}
			cpt++;
			distanceChecked += Vector3.Distance (oldRealPos, realPos);
		}

		return minDist;
	}


	/// <summary>
	/// Performs a simulated annealing to find the minimum distance the missile can get to Earth by modify the launch angle and intensity.
	/// </summary>
	/// <param name="ship">Ship.</param>
	/// <param name="target">Target.</param>
	/// <param name="startingAngle">Starting angle.</param>
	/// <param name="startingIntensity">Starting intensity.</param>
	public IEnumerator SimulatedAnnealing(GameObject ship, Vector3 target, float startingAngle, float startingIntensity){
		// Save the components of each force field in a table -- only useful to optimize ComputeForceAt
		int count = _allSourcesFamily.Count;
		Field[] tabField = new Field[count];
		Position[] tabPosition = new Position[count];
		for (int i = 0; i < count; i++) {
			GameObject s = _allSourcesFamily.getAt (i);
			tabField[i] = s.GetComponent<Field> ();
			tabPosition[i] = s.GetComponent<Position> ();
		}

		// Add a component to the LevelInfomation gameobject to tell that it is computing; removing one after completion
		GameObjectManager.addComponent<AIComputing> (_levelInfoGO);

		Vector3 shipInitialPos = ship.GetComponent<Position> ().initialPos;

		float T0 = 1.0f;
		//float Tmin = 0.01f;
		//float tau = 10000;
		float currentTemp = T0;

		float currentAngle = startingAngle;
		float currentIntensity = startingIntensity;

		Vector2 neighbor = new Vector2 ();
		float neighborAngle = currentAngle;
		float neighborIntensity = currentIntensity;

		float bestAngle = currentAngle;
		float bestIntensity = currentIntensity;

		// in our case, energy = distance to target
		float energy = TargetReached (shipInitialPos, target, currentAngle, currentIntensity, tabField, tabPosition);
		float energyNeighbor = energy;
		float bestEnergy = energy;

		int fourAnglesChecked = 0;
		int signeAngle = -1;


		while (energy > 0.5f && fourAnglesChecked < 3) {
			int t = 1;
			int tmax = 1000;
			currentTemp = T0;
			//currentAngle = ((startingAngle + 60*signeAngle) + 180) % 360 - 180;
			currentIntensity = startingIntensity;
			energy = TargetReached (shipInitialPos, target, currentAngle, currentIntensity, tabField, tabPosition);

			// we need to check convergence OR we can do it with the loop on the temperature
			int convergence = 0;
			float previousAngle = currentAngle;
			float previousIntensity = currentIntensity;

			while (t < tmax/* && currentTemp > Tmin*/ && energy > 0.5f && convergence < 200) {
				//currentTemp = T0 * Mathf.Exp (-t / tau);
				currentTemp *= 0.999f;
				//Debug.Log ("Current temp : " + currentTemp);
				neighbor = RandomNeighbor (currentAngle, currentIntensity);
				neighborAngle = neighbor [0];
				neighborIntensity = neighbor [1];
				energyNeighbor = TargetReached (shipInitialPos, target, neighborAngle, neighborIntensity, tabField, tabPosition);
				//Debug.Log ("Energy difference "+(energyNeighbor - energy)+" probability law : "+Mathf.Exp (-(energyNeighbor - energy) / currentTemp)+" temperature "+currentTemp);
				if (energyNeighbor < energy /* || Random.Range (0.0f, 1.0f) < Mathf.Exp (-(energyNeighbor - energy) / currentTemp)*/) {
					currentAngle = neighborAngle;
					currentIntensity = neighborIntensity;
					energy = energyNeighbor;
				}
				else if (Random.Range (0.0f, 1.0f) < Mathf.Exp (-(energyNeighbor - energy) / currentTemp)) {
					//Debug.Log ("Accepted");
					currentAngle = neighborAngle;
					currentIntensity = neighborIntensity;
					energy = energyNeighbor;
				}

				if (energy < bestEnergy) {
					bestAngle = currentAngle;
					bestIntensity = currentIntensity;
					bestEnergy = energy;
				}
				t++;
				if (previousAngle == currentAngle && previousIntensity == currentIntensity) {
					convergence++;
				} else {
					convergence = 0;
				}

				previousAngle = currentAngle;
				previousIntensity = currentIntensity;

				if (t % 25 == 0) { // Each frame does 25 tests, but if a lot of ennemies are computing it multiplies...
					yield return null;
				}
			}
			//Debug.Log ("Best angle "+bestAngle+"°, "+bestIntensity+" m/s, with energy "+bestEnergy+" number of tests "+ t);
			currentAngle = ((startingAngle + 60*signeAngle) + 180) % 360 - 180;
			fourAnglesChecked++;
			signeAngle *= -1;

		}


		ship.GetComponent<ShipInfo> ().fireIntensity = new Vector3(Mathf.Cos(Mathf.Deg2Rad*bestAngle),Mathf.Sin(Mathf.Deg2Rad*bestAngle),0);
		ship.GetComponent<ShipInfo> ().fireIntensity.Normalize ();
		ship.GetComponent<ShipInfo> ().fireIntensity *= bestIntensity;
		ship.GetComponent<ShipInfo> ().angle = bestAngle;

		//And its angle
		bestAngle *= -1;
		float angleTemp = bestAngle;
		if (bestAngle >= 0 && bestAngle <= 180)
			angleTemp = bestAngle;
		else //(bestAngle > 180)
			angleTemp = (360 - bestAngle) * (-1);
		

		Quaternion quat = Quaternion.Euler (ship.transform.rotation.eulerAngles[0], angleTemp, ship.transform.rotation.eulerAngles[2]);

		// Making the ship turn smoothly
		while (Vector3.Distance(ship.transform.eulerAngles, quat.eulerAngles) > 1f) {
			ship.transform.rotation = Quaternion.RotateTowards (ship.transform.rotation, quat, 1);
			yield return null;
		}

		//Computation over, remove an AIComputing component
		GameObjectManager.removeComponent<AIComputing> (_levelInfoGO);
	}



	/// <summary>
	/// Selects a random neighbor, meaning (angle, intensity) to the curent values (+/-1 for angle OR +/-1 for intensity).
	/// </summary>
	/// <returns>The neighbor.</returns>
	/// <param name="angle">Angle.</param>
	/// <param name="intensity">Intensity.</param>
	private Vector2 RandomNeighbor(float angle, float intensity){
		Vector2 neighbor = new Vector2 (angle, intensity);

		while (neighbor [0] == angle && neighbor [1] == intensity) {
			bool angleTurn = true; // change either angle or intensity
			if (Random.Range (0.0f, 1.0f) < 0.5f) {
				angleTurn = false;
			}
			int signeNeighbor = 1;
			if (angleTurn) {
				// we change the angle
				if (angle == 180.0f)
					signeNeighbor = -1;
				else if (angle == -180.0f)
					signeNeighbor = 1;
				else {
					if (Random.Range (0.0f, 1.0f) < 0.5f) {
						signeNeighbor = 1;
					} else {
						signeNeighbor = -1;
					}
				}
				neighbor [0] += signeNeighbor;
			} else {
				// we change the intensity
				if (intensity >= 0.94f)
					signeNeighbor = -1;
				else if (intensity <= 0.06f)
					signeNeighbor = 1;
				else {
					if (Random.Range (0.0f, 1.0f) < 0.5f) {
						signeNeighbor = 1;
					} else {
						signeNeighbor = -1;
					}
				}
				neighbor [1] += 0.05f * signeNeighbor;
			}
		}
		return neighbor;
	}
	#endregion
}