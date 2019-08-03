using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FYFY;

/*
 *  Deprecated: Camera from http://wiki.unity3d.com/index.php?title=MouseOrbitZoom and adapted to ECS
 * 
 *	System "PlayerActions":
 *	It handles the drag'n'drop from the UI to the world, and the drag'n'drop of a force field inside the playable area.
 */
using System.Collections.Generic;


public class PlayerActions : UtilitySystem
{
	// ==== VARIABLES ====
	private Family _PPlanFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Terrain)));
	private Family _sourcesFamily 	= FamilyManager.getFamily (new AllOfComponents (typeof(Field), typeof(Dimensions), typeof(Position)));
	private Family _invisibleWallsFamily = FamilyManager.getFamily(new AnyOfLayers(16));

	private Family _dragFamily = FamilyManager.getFamily(new AllOfComponents(typeof(IsDragging)));
	private Family _selectionFamily = FamilyManager.getFamily (new AllOfComponents (typeof(SelectInformation)));
	private Family _stateFamily = FamilyManager.getFamily(new AllOfComponents(typeof(State)));
	private Family _previousActionFamily = FamilyManager.getFamily (new AllOfComponents (typeof (PreviousActions)));
	private Family _gameInfoFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GameInformations), typeof(Constants)));
	private Family _levelInfoFamily	= FamilyManager.getFamily(new AllOfComponents(typeof(LevelInformations)));
	private Family _fieldsCounterFamily = FamilyManager.getFamily(new AllOfComponents(typeof(FieldsCounter)));
    private Family _interfaceFamily = FamilyManager.getFamily(new AllOfComponents(typeof(GlobalUI), typeof(OtherPanel), typeof(LoadingUI)));

    private Family _cameraFamily = FamilyManager.getFamily(new AllOfComponents(typeof(CameraParams)));

	private Family _idCounterFamily = FamilyManager.getFamily (new AllOfComponents (typeof(IdCounter)));

	private Family _soundFamily = FamilyManager.getFamily(new AllOfComponents(typeof(Sound)));

    // Components
	private SelectInformation _selectInfo;
	private Constants _cst;
	private GameObject _levelInfoGO;
	private PreviousActions _previousActions;
	private IdCounter _idCounter;
	private State _currentState;
    private GlobalUI _globalUI;
	private Sound _sound;

	// === Object drag and drop
	private bool isMouseDrag = false;
	private Vector3 screenPosition;
	private Vector3 offset;
	private IsDragging _sourceDrag;

	// == Raycast
	private int _layerMask;
	private Ray _ray;
	private RaycastHit _hit;
	// Raycaster UI things
	List<RaycastResult> _results;

	private bool _clickedChecked;
	private bool _fromDragnDrop;
	private DataGO _startData;
	private Vector3 _startPos;

	// ==== Constructor ====
	public PlayerActions()
	{
		_clickedChecked = false;
		_fromDragnDrop = true;

		// Gets the global components
		_sourceDrag = _dragFamily.First().GetComponent<IsDragging>();
		_selectInfo = _selectionFamily.First().GetComponent<SelectInformation> ();
		_cst = _gameInfoFamily.First ().GetComponent<Constants> ();
		_levelInfoGO = _levelInfoFamily.First ();
		_previousActions = _previousActionFamily.First ().GetComponent<PreviousActions>();
		_idCounter = _idCounterFamily.First ().GetComponent<IdCounter> ();
		_currentState = _stateFamily.First ().GetComponent<State> ();

        _globalUI = _interfaceFamily.First().GetComponent<GlobalUI>();
		_sound = _soundFamily.First ().GetComponent<Sound> ();

		// Raycast
		//_layerMask += 1 << 11; // Force fields
		_layerMask = 1 << 24; // selectable

		_results = new List<RaycastResult> ();
	}

	// ==== LIFECYCLE ====
	protected override void onProcess(int familiesUpdateCount) {
		if (_currentState.state == State.STATES.SETUP) {
			OnClickedAndDraggedButton ();
			DetectMouseSelection ();
		}
	}

	// ==== METHODS ====
	/// <summary>
	/// Checks if the player just clicked and dragged the Add a force button (attractive or repulsive), and creates the force field.
	/// Informs the UI system to update the display if needed (adds an UpdateUIAddFF component).
	/// </summary>
	public void OnClickedAndDraggedButton()
    {
		// we just clicked the add ff button and are holding it down
		if(((_globalUI.attractiveButton.GetComponent<AddFFButton>().clicked && _globalUI.attractiveButton.interactable) || (_globalUI.repulsiveButton.GetComponent<AddFFButton>().clicked && _globalUI.repulsiveButton.interactable)) && _clickedChecked == false){
			_clickedChecked = true;
			_fromDragnDrop = true;

			FieldsCounter fieldsRemaining = _fieldsCounterFamily.First().GetComponent<FieldsCounter>();
			GameObject field = null;

			// Update the number of fields remaining
			if (_globalUI.attractiveButton.GetComponent<AddFFButton> ().clicked) {
				if (fieldsRemaining.fieldsAttPlaced < fieldsRemaining.fieldsAttToPlace) {
					field = fieldsRemaining.poolAttractive [fieldsRemaining.fieldsAttPlaced].gameObject;
					fieldsRemaining.fieldsAttPlaced++;
					// reset intensity
					field.GetComponent<Field>().A = -0.5f;
				}
			}
			else if (_globalUI.repulsiveButton.GetComponent<AddFFButton>().clicked){
				if (fieldsRemaining.fieldsRepPlaced < fieldsRemaining.fieldsRepToPlace) {
					field = fieldsRemaining.poolRepulsive[fieldsRemaining.fieldsRepPlaced].gameObject;
					fieldsRemaining.fieldsRepPlaced++;
					// reset intensity
					field.GetComponent<Field>().A = 0.5f;
				}
			}
			GameObjectManager.setGameObjectState(field, true);

			// Reset lineRenderer if already used before
			LineRenderer line = field.GetComponent<LineRenderer> ();
			line.SetPosition (1, field.transform.position);
			line.SetPosition (0, field.transform.position);


			// disable collisions bewteen Force fields & Invisible Walls
			foreach(GameObject go in _invisibleWallsFamily)
				Physics.IgnoreCollision (field.GetComponent<SphereCollider> (), go.GetComponent<Collider>());


			Vector3 currentScreenSpace = new Vector3(MouseOrTouchPosition().x, MouseOrTouchPosition().y, _cameraFamily.First().GetComponent<CameraParams>().distance/2);
			Vector3 currentPosition = Camera.main.ScreenToWorldPoint (currentScreenSpace);
			Vector3 cursorPosition = new Vector3 (currentPosition.x / _cst.TERRAIN_INTERACTABLE_X, currentPosition.z / _cst.TERRAIN_INTERACTABLE_Y, 0);
			changePosition (field.GetComponent<Position> (), cursorPosition, _PPlanFamily.First().GetComponent<Terrain>().terrainData.size);

			offset = Vector3.zero;

			_selectInfo.selectedGameObject = field;

			_sourceDrag.isDragging = isMouseDrag = true;
			screenPosition = Camera.main.WorldToScreenPoint(field.transform.position);
			//offset = field.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z));

			//field.GetComponent<MeshRenderer> ().material = _AttractiveButton.GetComponent<AddFFButton> ().notDropableMat;
			field.GetComponent<TerrainDisplay> ().dropped = false;

			// inform UI to update because a force field has been added
			GameObjectManager.addComponent<UpdateUIAddFF>(_levelInfoGO, new {field = field, addff = true});
		}

		// we let go of the click
		else if (_globalUI.attractiveButton.GetComponent<AddFFButton>().clicked == false && _globalUI.repulsiveButton.GetComponent<AddFFButton>().clicked == false && _clickedChecked) {
			_clickedChecked = false;
			if (_selectInfo.selectedGameObject.GetComponent<TerrainDisplay> ().dropped) {
				// We have to add an id so we can pass it to the CANCEL
				_selectInfo.selectedGameObject.GetComponent<FieldID>().id = _idCounter.numberOfID;
				_idCounter.numberOfID++;

				_previousActions.listOfPreviousActions.Add (PreviousActions.CANCELACTIONS.ADD);
				_previousActions.cancelData.Add (StoreGameObjectComponents (_selectInfo.selectedGameObject));
				_previousActions.listOfRedoActions.Clear ();

				if (_selectInfo.selectedGameObject.GetComponent<Field> ()) {
					if (_selectInfo.selectedGameObject.GetComponent<Field> ().isRepulsive) {
						if(_gameInfoFamily.First().GetComponent<GameInformations>().playSounds)
							FMODUnity.RuntimeManager.PlayOneShot (_sound.RepFieldPlacedEvent);
					}
					else {
						if(_gameInfoFamily.First().GetComponent<GameInformations>().playSounds)
							FMODUnity.RuntimeManager.PlayOneShot (_sound.AttFieldPlacedEvent);
					}
				}
			}
		}
	}

	// === Object Selection and Drag & drop
	/// <summary>
	/// Detects the mouse selection and handles the drag and drop of force fields. 
	/// If let go over undropable zone, informs UI system to update the display, else informs ForcesDisplay and ForcesComputation systems to update what needs updating.
	/// </summary>
	protected void DetectMouseSelection()
	{

		if (_sourceDrag.cameraChanged)
		{
			_sourceDrag.isDragging = isMouseDrag = false;
			_sourceDrag.cameraChanged = false;
		}
			

		if (GetMouseOrTouchDown())
		{
			
			_ray = Camera.main.ScreenPointToRay (MouseOrTouchPosition());

			// touched something
			if (Physics.Raycast(_ray, out _hit, Mathf.Infinity, _layerMask))
			{
				// Different raycast for UI
				PointerEventData pointerData = new PointerEventData (EventSystem.current);
				pointerData.position = MouseOrTouchPosition();
				EventSystem.current.RaycastAll (pointerData, _results);

				// if we are not over UI
				if (_results.Count == 0) {
					// select now object if it match our criteria
					GameObject go = _hit.collider.transform.parent.gameObject;

					if (go.CompareTag ("Field"))
						isSourceSelected (go);
				}
				_results.Clear ();
			}
		}


		if (GetMouseOrTouchUp() && isMouseDrag)
		{
			if (_selectInfo.selectedGameObject != null)
			{
				Rigidbody r = _selectInfo.selectedGameObject.GetComponent<Rigidbody> ();
				r.velocity = Vector3.zero;
				_selectInfo.selectedGameObject.GetComponent<Position> ().pos = new Vector3 (_selectInfo.selectedGameObject.transform.position.x / _cst.TERRAIN_INTERACTABLE_X, _selectInfo.selectedGameObject.transform.position.z / _cst.TERRAIN_INTERACTABLE_Y, 0);

				if (_selectInfo.selectedGameObject.GetComponent<TerrainDisplay> ().dropped == false) {
					// Delete the force field
					GameObjectManager.setGameObjectState(_selectInfo.selectedGameObject, false);

					FieldsCounter fieldsRemaining = _fieldsCounterFamily.First().GetComponent<FieldsCounter>();
					if (_selectInfo.selectedGameObject.GetComponent<Field> ().isRepulsive) {
						fieldsRemaining.poolRepulsive.Remove (_selectInfo.selectedGameObject.transform);
						fieldsRemaining.poolRepulsive.Add (_selectInfo.selectedGameObject.transform);
						fieldsRemaining.fieldsRepPlaced--;
					} else {
						fieldsRemaining.poolAttractive.Remove (_selectInfo.selectedGameObject.transform);
						fieldsRemaining.poolAttractive.Add (_selectInfo.selectedGameObject.transform);
						fieldsRemaining.fieldsAttPlaced--;
					}

					_selectInfo.selectedGameObject = null;
					GameObjectManager.addComponent<UpdateUIAddFF>(_levelInfoGO, new {addff = false});

				}
				else {
					GameObjectManager.addComponent<RefreshTerrain> (_levelInfoGO, new {action = RefreshTerrain.ACTIONS.MOVE, source = _selectInfo.selectedGameObject });
					GameObjectManager.addComponent<RefreshTrajectory> (_levelInfoGO);
				}
			}
			_sourceDrag.isDragging = isMouseDrag = false;
		}


		if (isMouseDrag)
		{
			/*
			//track mouse position.
			Vector3 currentScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z);
			//convert screen position to world position with offset changes.
			Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenSpace) + offset;
			*/
			// Test raycast
			_ray = Camera.main.ScreenPointToRay (MouseOrTouchPosition());
			int layerMask = 1 << 25;
			if (Physics.Raycast (_ray, out _hit, Mathf.Infinity, layerMask)) {
				Vector3 currentPosition = _hit.point + offset;

				Rigidbody r = _selectInfo.selectedGameObject.GetComponent<Rigidbody> ();
				Vector3 pos = new Vector3 (currentPosition.x, _selectInfo.selectedGameObject.transform.position.y, currentPosition.z);
				Vector3 direction = (pos - _selectInfo.selectedGameObject.transform.position);
				r.velocity = direction * 10;

				if (r.velocity.magnitude > 40) {
					direction.Normalize ();
					r.velocity = direction * 50;
				}

				Position selectPosition = _selectInfo.selectedGameObject.GetComponent<Position> ();
				selectPosition.pos = new Vector3 (_selectInfo.selectedGameObject.transform.position.x / _cst.TERRAIN_INTERACTABLE_X, _selectInfo.selectedGameObject.transform.position.z / _cst.TERRAIN_INTERACTABLE_Y, 0);

				// we are in the dropable area for the first time
				if ((selectPosition.pos.x > 0.0375 && selectPosition.pos.x < 0.9625 && selectPosition.pos.y > 0.0375 && selectPosition.pos.y < 0.9625) && _selectInfo.selectedGameObject.GetComponent<TerrainDisplay> ().dropped == false) {
					_selectInfo.selectedGameObject.GetComponent<TerrainDisplay> ().dropped = true;

					/*if (_selectInfo.selectedGameObject.GetComponent<Field> ().isRepulsive)
						selectPosition.GetComponent<MeshRenderer> ().material = _RepulsiveButton.GetComponent<AddFFButton> ().normalMat;
					else
						selectPosition.GetComponent<MeshRenderer> ().material = _AttractiveButton.GetComponent<AddFFButton> ().normalMat;*/

					foreach (GameObject go in _invisibleWallsFamily)
						Physics.IgnoreCollision (selectPosition.GetComponent<SphereCollider> (), go.GetComponent<Collider> (), false);
					GameObjectManager.addComponent<RefreshTrajectory> (_levelInfoGO);
					GameObjectManager.addComponent<RefreshTerrain> (_levelInfoGO, new { action = RefreshTerrain.ACTIONS.ADD, source = selectPosition.gameObject });
				}
				// we are in the dropable area but not for the first time
				else if (_selectInfo.selectedGameObject.GetComponent<TerrainDisplay> ().dropped) {
					GameObjectManager.addComponent<RefreshTerrain> (_levelInfoGO, new {action = RefreshTerrain.ACTIONS.MOVE, source = _selectInfo.selectedGameObject });
					GameObjectManager.addComponent<RefreshTrajectory> (_levelInfoGO);
				}
			}
		}
	}

	/// <summary>
	/// Checks if what is selected is a field. If it is an editable field, sets the dragging state.
	/// </summary>
	/// <returns><c>true</c>, if source selected was ised, <c>false</c> otherwise.</returns>
	/// <param name="go">Go.</param>
	protected bool isSourceSelected(GameObject go)
	{
		// Source found
		if (_sourcesFamily.contains (go.GetInstanceID ()))
		{
			_selectInfo.selectedGameObject = go;

			// Still editable ?
			if (go.GetComponent<Editable> () != null)
			{
				_sourceDrag.isDragging = isMouseDrag = true;
				screenPosition = Camera.main.WorldToScreenPoint(go.transform.position);
				//offset = go.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z));
				_ray = Camera.main.ScreenPointToRay (MouseOrTouchPosition());
				int layerMask = 1 << 25;
				if (Physics.Raycast (_ray, out _hit, Mathf.Infinity, layerMask)) {
					Vector3 testPos = _hit.point;

					Vector3 pos = new Vector3 (testPos.x, go.transform.position.y, testPos.z);
					offset = (go.transform.position - pos);
				}

				//_previousActions.listOfPreviousActions.Add (PreviousActions.CANCELACTIONS.MOVE);
				//_previousActions.cancelData.Add (StoreGameObjectComponents (go));
				//_previousActions.listOfRedoActions.Clear ();
				_startData = StoreGameObjectComponents(go);
				_startPos = go.GetComponent<Position> ().pos;

				_fromDragnDrop = false;
			}


			return true;
		}

		return false;
	}
}